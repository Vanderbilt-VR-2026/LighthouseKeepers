using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using LighthouseKeepers.Core.Progression;

namespace LighthouseKeepers.Presentation.Board
{
    // C owns board rendering and all scene geometry. D supplies lines/actions and positioned audio.
    [DisallowMultipleComponent]
    public sealed class DayBoardPresenter : MonoBehaviour
    {
        [SerializeField] DayDirector director;
        [SerializeField] MonoBehaviour boardRenderer;
        [SerializeField] AudioSource checkboxAudio, radioAudio;
        [SerializeField] AudioClip checkboxClip;
        IDayDirector readOnlyDirector;
        DayStage remoteStage;
        DayResult remoteResult;
        float remoteElapsed;
        string remoteHeading = "", remoteBriefing = "";
        MethodInfo setLines, setActions;
        int shownSecond = -1;
        DayStage? actionsStage;
        bool warnedBoard, warnedAudio;

        void OnEnable()
        {
            EnsureAudioSources();
            SceneManager.sceneLoaded += SceneLoaded;
            if (director)
            {
                director.Changed += Refresh; director.ObjectiveCompleted += ObjectiveCompleted;
                director.OnDayStarted += DayStarted;
            }
            ResolveBoard(); Refresh();
        }
        void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            if (director)
            {
                director.Changed -= Refresh; director.ObjectiveCompleted -= ObjectiveCompleted;
                director.OnDayStarted -= DayStarted;
            }
            actionsStage = null;
            if (boardRenderer && setActions != null) setActions.Invoke(boardRenderer, new object[] { Array.Empty<string>(), new Action<int>(index => { }) });
        }

        // E calls this with its replicated IDayDirector projection; no client DayFlow is created.
        public void BindReadOnly(IDayDirector projection) { readOnlyDirector = projection; Refresh(); }
        public void UpdateReadOnlyPresentation(DayStage stage, DayResult result, float elapsed, string heading, string briefing)
        { remoteStage = stage; remoteResult = result; remoteElapsed = elapsed; remoteHeading = heading; remoteBriefing = briefing; Refresh(); }

        void SceneLoaded(Scene scene, LoadSceneMode mode) { ResolveBoard(); Refresh(); }
        void ResolveBoard()
        {
            if (!boardRenderer)
            {
                var anchor = GameObject.Find("STN_BRIEFING_BOARD");
                if (anchor)
                    foreach (var component in anchor.GetComponentsInChildren<MonoBehaviour>(true))
                        if (component && component.GetType().GetMethod("SetLines", new[] { typeof(string[]) }) != null)
                        { boardRenderer = component; break; }
            }
            if (boardRenderer)
            {
                setLines = boardRenderer.GetType().GetMethod("SetLines", new[] { typeof(string[]) });
                setActions = boardRenderer.GetType().GetMethod("SetActions", new[] { typeof(string[]), typeof(Action<int>) });
                actionsStage = null;
            }
            if (setLines == null && !warnedBoard)
            {
                warnedBoard = true;
                Debug.LogWarning("Session D needs C's STN_BRIEFING_BOARD.SetLines(string[]). See D-to-C.md.", this);
            }
        }

        void Update()
        {
            float elapsed = readOnlyDirector != null ? remoteElapsed : director ? director.Flow.ElapsedSeconds : 0;
            if ((int)elapsed != shownSecond) { shownSecond = (int)elapsed; Refresh(); }
        }

        void Refresh()
        {
            IDayDirector source = readOnlyDirector ?? director;
            if (source == null || !boardRenderer || setLines == null) return;
            bool remote = readOnlyDirector != null;
            var definition = !remote && director ? director.CurrentDefinition : null;
            var stage = remote ? remoteStage : director.Stage;
            var lines = DayBoardContent.Build(source, remote ? remoteHeading : definition ? definition.briefingHeading : "Keeper watch",
                remote ? remoteBriefing : definition ? definition.briefingText : "", stage,
                remote ? remoteElapsed : director.Flow.ElapsedSeconds, remote ? remoteResult : director.Flow.Result,
                remote || director.HasNextDay);
            try
            {
                setLines.Invoke(boardRenderer, new object[] { lines });
                if (setActions != null && !remote && director.IsAuthority) BindActions(stage);
            }
            catch (Exception error) { Debug.LogError("Board binding failed: " + (error.InnerException ?? error).Message, this); }
        }

        void BindActions(DayStage stage)
        {
            if (actionsStage == stage) return;
            actionsStage = stage;
            var labels = new System.Collections.Generic.List<string>();
            var callbacks = new System.Collections.Generic.List<Action>();
            if (stage == DayStage.Failed) { labels.Add("Retry this day"); callbacks.Add(() => director.RetryCurrentDay()); }
            else if (stage == DayStage.Results && director.HasNextDay) { labels.Add("Continue"); callbacks.Add(() => director.ContinueToNextDay()); }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            foreach (var definition in director.AvailableDays)
            {
                if (!definition) continue;
                int target = definition.day; labels.Add("DEV: Day " + target); callbacks.Add(() => director.JumpToDay(target));
            }
#endif
            setActions.Invoke(boardRenderer, new object[] { labels.ToArray(), new Action<int>(index =>
            { if (index >= 0 && index < callbacks.Count) callbacks[index](); }) });
        }

        void ObjectiveCompleted(int index)
        {
            var clip = checkboxClip ? checkboxClip : director ? director.ObjectiveTickClip : null;
            if (checkboxAudio && clip) checkboxAudio.PlayOneShot(clip, .35f);
            else WarnAudio();
        }
        void EnsureAudioSources()
        {
            if (!checkboxAudio) checkboxAudio = CreateSource();
            if (!radioAudio || radioAudio == checkboxAudio) radioAudio = CreateSource();
        }
        AudioSource CreateSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.spatialBlend = 1; source.minDistance = 1; source.maxDistance = 14;
            return source;
        }
        void DayStarted(int day)
        {
            if (!director || director.SkipIntro) return;
            var definition = director.CurrentDefinition;
            if (radioAudio && definition && definition.briefingVoice) radioAudio.PlayOneShot(definition.briefingVoice);
            else WarnAudio();
        }
        void WarnAudio()
        {
            if (warnedAudio) return;
            warnedAudio = true;
            Debug.LogWarning("Assign separate positioned checkbox/radio sources and briefing voice clips. Audio is unverified; see D-to-C.md.", this);
        }
    }
}
