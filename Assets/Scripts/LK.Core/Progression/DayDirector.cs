using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LighthouseKeepers.Core.Progression
{
    [DisallowMultipleComponent]
    public sealed class DayDirector : MonoBehaviour, IDayDirector
    {
        const string SaveKey = "LighthouseKeepers.Sprint3.CurrentDay.v1";
        [SerializeField] DayCatalog catalog;
        [SerializeField] MonoBehaviour simulationHost;
        [SerializeField] UnityEngine.Object floodTuning;
        [SerializeField] Transform localHead;
        [SerializeField, Min(.1f)] float bedGripReachM = 1.5f;
        [SerializeField, Min(.1f)] float leakDiscoveryRadiusM = 2.5f;
        readonly DayFlow flow = new DayFlow();
        SharedSimulationPort simulation;
        SharedInteractionSubscription interactions;
        Transform pipeAnchor, bedAnchor;
        DayObservation observation;
        bool authority, waitingForBaseline, simulationFaulted, previewRun, wired;
        int keeperCount, baselineTick, localActorId;
        float nextPoll;
        string status = "Awaiting session authority.";
        public DayFlow Flow => flow;
        public DayStage Stage => simulationFaulted ? DayStage.Inactive : flow.Stage;
        public bool IsAuthority => authority;
        public bool SkipIntro { get; private set; }
        public DayDefinition CurrentDefinition { get; private set; }
        public bool HasNextDay => catalog && catalog.Next(CurrentDay);
        public IReadOnlyList<DayDefinition> AvailableDays => catalog ? catalog.days : Array.Empty<DayDefinition>();
        public AudioClip ObjectiveTickClip => catalog ? catalog.objectiveTick : null;
        public string IntegrationStatus => status;
        public int CurrentDay => flow.CurrentDay;
        public IReadOnlyList<ObjectiveStep> Steps => flow.Steps;
        public string CurrentObjectiveText => simulationFaulted ? "Watch unavailable. Return to the lobby." :
            Stage == DayStage.Inactive ? "Preparing the watch..." : flow.CurrentObjectiveText;
        public event Action<int> OnDayStarted;
        public event Action<int> OnDayCompleted;
        public event Action<int> OnDayFailed;
        public event Action Changed;
        public event Action<int> ObjectiveCompleted;
        public event Action<IncidentDefinition> IncidentFired;
        public event Action<RestartResult, int> RestartFeedback;
        public event Action<float> FailureFadeRequested;

        void Awake() => Wire();
        void Wire()
        {
            if (wired) return;
            wired = true;
            flow.OnDayStarted += day => { OnDayStarted?.Invoke(day); Changed?.Invoke(); };
            flow.OnDayCompleted += day =>
            {
                if (!previewRun) SaveDay(catalog.Next(day) ? catalog.Next(day).day : day);
                OnDayCompleted?.Invoke(day);
            };
            flow.OnDayFailed += day => { if (!previewRun) SaveDay(day); FailureFadeRequested?.Invoke(.35f); OnDayFailed?.Invoke(day); };
            flow.OnChanged += () => Changed?.Invoke();
            flow.OnObjectiveCompleted += index => ObjectiveCompleted?.Invoke(index);
            flow.OnIncidentFired += incident => IncidentFired?.Invoke(incident);
            flow.OnRestartFeedback += (result, actor) => RestartFeedback?.Invoke(result, actor);
            flow.OnRequest += Send;
            flow.TryFireIncident = incident =>
            {
                try { return simulation.Fire(incident, CurrentDay); }
                catch (Exception error) { IntegrationFailure(error); return false; }
            };
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += SceneLoaded; ResolveAnchors();
            if (authority && Stage != DayStage.Inactive && interactions == null)
                try { BindSimulation(); } catch (Exception error) { IntegrationFailure(error); }
        }
        void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            interactions?.Dispose(); interactions = null;
        }
        void SceneLoaded(Scene scene, LoadSceneMode mode) => ResolveAnchors();
        void ResolveAnchors()
        {
            var pipe = GameObject.Find("STN_PIPE_LEAK"); if (pipe) pipeAnchor = pipe.transform;
            var bed = GameObject.Find("STN_BED"); if (bed) bedAnchor = bed.transform;
            if (!localHead && Camera.main) localHead = Camera.main.transform;
        }

        public void ConfigureAuthority(bool isAuthority, int keepers, int actorId = 0)
        {
            Wire(); authority = isAuthority; keeperCount = Mathf.Clamp(keepers, 0, 2); localActorId = actorId;
            if (!authority) { interactions?.Dispose(); interactions = null; }
            else if (Stage != DayStage.Inactive && interactions == null)
            {
                try { BindSimulation(); } catch (Exception error) { IntegrationFailure(error); }
            }
            flow.SetKeeperCount(keeperCount); Changed?.Invoke();
        }

        public void AssignDependencies(DayCatalog days, MonoBehaviour host, UnityEngine.Object tuning)
        { catalog = days; simulationHost = host; floodTuning = tuning; }

        public void SetKeeperCount(int count)
        { if (!authority) return; keeperCount = Mathf.Clamp(count, 0, 2); flow.SetKeeperCount(keeperCount); }

        public bool BeginSavedDay()
        {
            if (!catalog || catalog.days == null || catalog.days.Length == 0) return false;
            int saved = PlayerPrefs.GetInt(SaveKey, catalog.days[0].day);
            return BeginDay(catalog.Find(saved) ? saved : catalog.days[0].day);
        }
        public bool BeginDay(int day) => BeginDayInternal(day, false, false);

        bool BeginDayInternal(int day, bool skipIntro, bool preview)
        {
            if (!authority || !isActiveAndEnabled) return false;
            Wire();
            try
            {
                if (!catalog) throw new ArgumentException("Assign the Sprint 3 DayCatalog asset.");
                var definition = catalog.Find(day);
                if (!definition) throw new ArgumentException("No day asset for day " + day);
                BindSimulation();
                var plan = definition.CreatePlan(); simulation.Validate(plan);
                observation = simulation.Read(out _); baselineTick = observation.Tick;
                CurrentDefinition = definition; SkipIntro = skipIntro; previewRun = preview;
                simulationFaulted = false; status = ""; waitingForBaseline = true; nextPoll = 0;
                flow.Begin(plan, simulation.SafeHeightM, keeperCount);
                if (!previewRun) SaveDay(day);
                ResolveAnchors(); Changed?.Invoke(); return true;
            }
            catch (Exception error) { IntegrationFailure(error); return false; }
        }

        void BindSimulation()
        {
            simulation = new SharedSimulationPort(simulationHost, floodTuning);
            if (interactions == null)
                interactions = new SharedInteractionSubscription(ReportPumpStroke,
                    actor => ReportLeakRepairCompleted(actor), actor => ReportStarterPull(actor));
        }

        void Send(DayRequest command, int actor)
        {
            if (!authority || simulationFaulted) return;
            try
            {
                if (command == DayRequest.ResetRun)
                {
                    baselineTick = simulation.Read(out _).Tick;
                    waitingForBaseline = true;
                }
                simulation.Send(command, actor, CurrentDay);
            }
            catch (Exception error) { flow.GeneratorStartRejected(); IntegrationFailure(error); }
        }

        void Update()
        {
            if (!authority || simulation == null || simulationFaulted || Stage != DayStage.Playing) return;
            if (Time.unscaledTime >= nextPoll)
            {
                nextPoll = Time.unscaledTime + .1f;
                try
                {
                    observation = simulation.Read(out bool running);
                    if (waitingForBaseline && running && observation.Tick != baselineTick) waitingForBaseline = false;
                }
                catch (Exception error) { IntegrationFailure(error); return; }
            }
            if (waitingForBaseline) return;
            flow.Advance(Time.deltaTime, observation);
            if (localHead) ReportPlayerPosition(localActorId, localHead.position);
        }

        static void SaveDay(int day) { PlayerPrefs.SetInt(SaveKey, day); PlayerPrefs.Save(); }
        void IntegrationFailure(Exception error)
        {
            simulationFaulted = true;
            status = "Progression integration unavailable: " + (error.InnerException ?? error).Message;
            Debug.LogError(status, this); Changed?.Invoke();
        }

        bool AcceptReports => authority && !simulationFaulted && !waitingForBaseline && isActiveAndEnabled;
        public void ReportPumpStroke(int actorId) { if (AcceptReports) flow.PumpStroke(actorId); }
        public bool ReportLeakRepairCompleted(int actorId) => AcceptReports && flow.LeakRepairCompleted(actorId);
        public void ReportWrenchGrabbed(int actorId) { if (AcceptReports) flow.WrenchGrabbed(actorId); }
        public void ReportPlayerPosition(int actorId, Vector3 headPosition)
        {
            if (AcceptReports && pipeAnchor && Vector3.SqrMagnitude(headPosition - pipeAnchor.position) <= leakDiscoveryRadiusM * leakDiscoveryRadiusM)
                flow.PlayerNearLeak(actorId);
        }
        public bool ReportBedGrip(int actorId, Vector3 handWorldPosition)
        {
            if (!AcceptReports || !bedAnchor || Vector3.SqrMagnitude(handWorldPosition - bedAnchor.position) > bedGripReachM * bedGripReachM) return false;
            // Recheck the newest authoritative state before allowing the terminal action.
            try { observation = simulation.Read(out _); flow.Advance(0, observation); }
            catch (Exception error) { IntegrationFailure(error); return false; }
            return !simulationFaulted && flow.BedGrip(actorId);
        }
        public void ReportFuelRead(int actorId, float fraction) { if (AcceptReports) flow.ReadFuel(actorId, fraction); }
        public void ReportPrimerStroke(int actorId) { if (AcceptReports) flow.PrimerStroke(actorId); }
        public void ReportChoke(int actorId, bool closed) { if (AcceptReports) flow.SetChoke(actorId, closed); }
        public void ReportIsolation(int actorId, bool held, bool pressureSafe) { if (AcceptReports) flow.ObserveIsolation(actorId, held, pressureSafe); }
        public bool CanRepairPipe(int actorId) => AcceptReports && flow.CanRepairPipe(actorId);
        public RestartResult ReportStarterPull(int actorId) => AcceptReports ? flow.StarterPulled(actorId) : RestartResult.NotPlaying;

        public bool ContinueToNextDay()
        {
            if (!authority || Stage != DayStage.Results) return false;
            var next = catalog.Next(CurrentDay);
            return next && BeginDayInternal(next.day, false, previewRun);
        }
        public bool RetryCurrentDay() => authority && Stage == DayStage.Failed && BeginDayInternal(CurrentDay, false, previewRun);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public bool JumpToDay(int day) => BeginDayInternal(day, true, true);
        [ContextMenu("Development/Start local Day 1")]
        void StartLocalPreview() { ConfigureAuthority(true, 1); JumpToDay(1); }
        [ContextMenu("Development/Jump to Day 2")]
        void PreviewDay2() { if (authority) JumpToDay(2); }
#endif
    }
}
