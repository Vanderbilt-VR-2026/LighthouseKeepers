using System;
using System.Collections.Generic;

namespace LighthouseKeepers.Core.Progression
{
    // Authority-only sequencing. The simulation owns flood failure (including its three-second sustain).
    public sealed class DayFlow : IDayDirector
    {
        readonly List<ObjectiveStep> steps = new List<ObjectiveStep>();
        readonly IReadOnlyList<ObjectiveStep> stepView;
        readonly HashSet<string> firedIncidents = new HashSet<string>();
        DayPlan plan;
        DayObservation observation;
        bool wrench, nearLeak, repair, bed, fuelChecked, chokeClosed, isolationObserved, fault, faultRecovered, startPending;
        int primeStrokes, keepers, isolationActor = -1;
        float safeHeight, lastIsolationAt, lastCrankAt, runningSince;
        bool previousGeneratorRunning;
        public DayStage Stage { get; private set; }
        public int CurrentDay => plan == null ? 0 : plan.Day;
        public float ElapsedSeconds { get; private set; }
        public float PeakWaterHeightM { get; private set; }
        public int PumpStrokes { get; private set; }
        public int GeneratorRestarts { get; private set; }
        public int PrimerStrokes => primeStrokes;
        public float FuelFraction { get; private set; }
        public bool ChokeClosed => chokeClosed;
        public bool CanOpenChoke => faultRecovered && observation.GeneratorRunning && ElapsedSeconds - runningSince >= plan.ChokeReleaseSeconds;
        public string IntegrationIssue { get; private set; } = "";
        public IReadOnlyList<ObjectiveStep> Steps => stepView;
        public DayResult Result { get; private set; }
        public event Action<int> OnDayStarted;
        public event Action<int> OnDayCompleted;
        public event Action<int> OnDayFailed;
        public event Action OnChanged;
        public event Action<int> OnObjectiveCompleted;
        public event Action<IncidentDefinition> OnIncidentFired;
        public event Action<RestartResult, int> OnRestartFeedback;
        public event Action<int> OnRepairRejected;
        public event Action<DayRequest, int> OnRequest;
        public Func<IncidentDefinition, bool> TryFireIncident;

        public DayFlow() { stepView = steps.AsReadOnly(); }

        public string CurrentObjectiveText
        {
            get
            {
                if (plan != null && keepers < plan.RequiredKeepers)
                    return "Day " + CurrentDay + (plan.RequiredKeepers == 2 ? " requires two keepers." : " requires a keeper.");
                if (Stage == DayStage.Failed) return "The lighthouse flooded";
                if (Stage == DayStage.Results) return "Watch survived. Review the results.";
                if (IntegrationIssue.Length > 0) return "Watch paused. Please return to the lobby.";
                if (fault && fuelChecked && FuelFraction <= 0) return "Fuel is empty. Refill from a jerry can.";
                if (fault && fuelChecked && FuelFraction > 0 && primeStrokes >= plan.PrimerStrokes && !chokeClosed && !observation.GeneratorRunning)
                    return "Set the choke to Closed for a cold start.";
                for (int i = 0; i < steps.Count; i++)
                    if (!steps[i].Complete)
                        return (fault && plan.FaultObjectivePrefix.Length > 0 ? plan.FaultObjectivePrefix + "\n" : "") + steps[i].Text;
                return plan == null ? "" : plan.Briefing;
            }
        }

        public void Begin(DayPlan next, float safeHeightM, int keeperCount)
        {
            Validate(next, safeHeightM);
            plan = next; safeHeight = safeHeightM; keepers = Math.Max(0, keeperCount);
            Stage = keepers >= plan.RequiredKeepers ? DayStage.Playing : DayStage.WaitingForCrew;
            ElapsedSeconds = PeakWaterHeightM = 0;
            PumpStrokes = GeneratorRestarts = primeStrokes = 0;
            wrench = nearLeak = repair = bed = fuelChecked = chokeClosed = isolationObserved = fault = faultRecovered = startPending = false;
            FuelFraction = 0; isolationActor = -1; lastIsolationAt = lastCrankAt = float.NegativeInfinity;
            previousGeneratorRunning = false; runningSince = 0; observation = default;
            IntegrationIssue = ""; Result = default; firedIncidents.Clear(); steps.Clear();
            foreach (var objective in plan.Objectives) steps.Add(new ObjectiveStep(objective.text, false));
            OnChanged?.Invoke();
            if (Stage == DayStage.Playing) StartDay();
        }

        static void Validate(DayPlan next, float safeHeightM)
        {
            if (next == null || next.Day < 1 || next.RequiredKeepers < 1 || next.RequiredKeepers > 2)
                throw new ArgumentException("A day needs a positive index and one or two keepers.");
            if (!Finite(safeHeightM) || safeHeightM < 0 || next.Objectives == null || next.Objectives.Length == 0 || next.Incidents == null)
                throw new ArgumentException("A day needs objectives, incidents and the simulation's safe height.");
            if (next.PrimerStrokes < 1 || !Finite(next.CrankCooldownSeconds) || next.CrankCooldownSeconds < 0 ||
                !Finite(next.ChokeReleaseSeconds) || next.ChokeReleaseSeconds < 0 ||
                !Finite(next.IsolationFreshnessSeconds) || next.IsolationFreshnessSeconds <= 0)
                throw new ArgumentException("Invalid generator or isolation timing.");
            var ids = new HashSet<string>();
            foreach (var incident in next.Incidents)
                if (incident == null || string.IsNullOrWhiteSpace(incident.id) || !ids.Add(incident.id) ||
                    !Finite(incident.atSeconds) || incident.atSeconds < 0 || !Enum.IsDefined(typeof(DayIncidentKind), incident.kind))
                    throw new ArgumentException("Incidents need unique IDs, supported kinds and nonnegative times.");
            foreach (var objective in next.Objectives)
            {
                if (objective == null || string.IsNullOrWhiteSpace(objective.text) || objective.predicates == null || objective.predicates.Length == 0)
                    throw new ArgumentException("Each objective needs text and at least one predicate.");
                foreach (var predicate in objective.predicates)
                    if (!Enum.IsDefined(typeof(DayPredicate), predicate)) throw new ArgumentException("Unsupported objective predicate.");
                if (!string.IsNullOrEmpty(objective.incidentId) && !ids.Contains(objective.incidentId))
                    throw new ArgumentException("Objective references an unknown incident.");
            }
            if (next.RequiresPartnerIsolation && next.RequiredKeepers != 2)
                throw new ArgumentException("Partner isolation requires two keepers.");
            if (!Enum.IsDefined(typeof(DaySuccessCondition), next.Success) || !Enum.IsDefined(typeof(DayFailureCondition), next.Failure))
                throw new ArgumentException("Unsupported day success/failure condition.");
        }

        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        void StartDay()
        {
            Stage = DayStage.Playing;
            OnRequest?.Invoke(DayRequest.ResetRun, -1);
            OnRequest?.Invoke(DayRequest.BeginDay, -1);
            OnDayStarted?.Invoke(CurrentDay);
            OnChanged?.Invoke();
        }

        public void SetKeeperCount(int count)
        {
            keepers = Math.Max(0, count);
            if (plan != null && keepers < plan.RequiredKeepers) isolationActor = -1;
            // A disconnect never opens a solo fallback. E owns ending/rejoining the crew session.
            if (Stage == DayStage.WaitingForCrew && keepers >= plan.RequiredKeepers) StartDay();
            else OnChanged?.Invoke();
        }

        public void Advance(float seconds, DayObservation current)
        {
            if (Stage != DayStage.Playing || keepers < plan.RequiredKeepers) return;
            if (!Finite(seconds) || seconds < 0 || !Finite(current.WaterHeightM)) throw new ArgumentOutOfRangeException(nameof(seconds));
            ElapsedSeconds += seconds;
            observation = current;
            PeakWaterHeightM = Math.Max(PeakWaterHeightM, current.WaterHeightM);
            // Failure is checked before incident/objective evaluation, even if the bed is pressed this tick.
            if (plan.Failure == DayFailureCondition.SimulationFailed && current.Failed) { End(false); return; }
            if (fault && current.GeneratorRunning && !previousGeneratorRunning)
            {
                GeneratorRestarts++; runningSince = ElapsedSeconds; startPending = false; faultRecovered = true;
            }
            previousGeneratorRunning = current.GeneratorRunning;
            foreach (var incident in plan.Incidents)
            {
                if (firedIncidents.Contains(incident.id) || ElapsedSeconds < incident.atSeconds) continue;
                if (TryFireIncident == null || !TryFireIncident(incident))
                {
                    IntegrationIssue = "Waiting for simulation incident: " + incident.id;
                    OnChanged?.Invoke(); return;
                }
                firedIncidents.Add(incident.id); IntegrationIssue = "";
                if (incident.kind == DayIncidentKind.GeneratorFault)
                {
                    fault = true; faultRecovered = false; primeStrokes = 0; chokeClosed = false; startPending = false;
                    lastCrankAt = float.NegativeInfinity;
                }
                else { repair = false; isolationObserved = false; isolationActor = -1; }
                OnIncidentFired?.Invoke(incident); OnChanged?.Invoke();
            }
            EvaluateObjectives();
        }

        void EvaluateObjectives()
        {
            if (Stage != DayStage.Playing) return;
            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i].Complete) continue;
                var objective = plan.Objectives[i];
                if (!string.IsNullOrEmpty(objective.incidentId) && !firedIncidents.Contains(objective.incidentId)) return;
                foreach (var predicate in objective.predicates) if (!Matches(predicate)) return;
                steps[i] = new ObjectiveStep(steps[i].Text, true);
                OnObjectiveCompleted?.Invoke(i); OnChanged?.Invoke();
            }
            if (plan.Success == DaySuccessCondition.AllObjectives) End(true);
        }

        bool Matches(DayPredicate predicate)
        {
            switch (predicate)
            {
                case DayPredicate.WrenchGrabbed: return wrench;
                case DayPredicate.NearLeak: return nearLeak;
                case DayPredicate.LeakRepaired: return repair;
                case DayPredicate.WaterSafe: return observation.WaterHeightM <= safeHeight;
                case DayPredicate.BedGrip: return bed;
                case DayPredicate.IncidentFired: return firedIncidents.Count > 0;
                case DayPredicate.FuelChecked: return fuelChecked && FuelFraction > 0;
                case DayPredicate.Primed: return primeStrokes >= plan.PrimerStrokes;
                case DayPredicate.ChokeClosed: return chokeClosed;
                case DayPredicate.GeneratorRestored: return faultRecovered && observation.GeneratorRunning;
                case DayPredicate.TwoKeepers: return keepers >= 2;
                case DayPredicate.PartnerIsolationObserved: return isolationObserved;
                default: return false;
            }
        }

        bool Accepts(int actorId) => Stage == DayStage.Playing && keepers >= plan.RequiredKeepers && actorId >= 0;
        public void WrenchGrabbed(int actorId) { if (Accepts(actorId)) { wrench = true; EvaluateObjectives(); } }
        public void PlayerNearLeak(int actorId) { if (Accepts(actorId)) { nearLeak = true; EvaluateObjectives(); } }
        public void PumpStroke(int actorId) { if (Accepts(actorId)) PumpStrokes++; }

        public void ObserveIsolation(int actorId, bool held, bool pressureSafe)
        {
            if (!Accepts(actorId)) return;
            if (!held || !pressureSafe) { if (isolationActor == actorId) isolationActor = -1; return; }
            isolationActor = actorId; lastIsolationAt = ElapsedSeconds;
        }

        // B and A must call this BEFORE accepting repair effort, not after changing the simulation.
        public bool CanRepairPipe(int actorId)
        {
            if (!Accepts(actorId)) return false;
            bool activeLeakIncident = false;
            foreach (var incident in plan.Incidents)
                if (incident.kind == DayIncidentKind.ActivateLeak && firedIncidents.Contains(incident.id)) activeLeakIncident = true;
            return activeLeakIncident && (!plan.RequiresPartnerIsolation ||
                (keepers >= 2 && isolationActor >= 0 && isolationActor != actorId &&
                 ElapsedSeconds - lastIsolationAt <= plan.IsolationFreshnessSeconds));
        }

        public bool LeakRepairCompleted(int actorId)
        {
            if (!CanRepairPipe(actorId)) { OnRepairRejected?.Invoke(actorId); return false; }
            repair = true; isolationObserved = plan.RequiresPartnerIsolation; EvaluateObjectives(); return true;
        }

        public bool BedGrip(int actorId)
        {
            if (!Accepts(actorId)) return false;
            // Early grips are not banked: return to the bed after recovery, without a forced teleport.
            for (int i = 0; i < steps.Count; i++)
                if (!steps[i].Complete && Array.IndexOf(plan.Objectives[i].predicates, DayPredicate.BedGrip) < 0) return false;
            bed = true; EvaluateObjectives(); return true;
        }

        public void ReadFuel(int actorId, float fraction)
        {
            if (!Accepts(actorId) || !Finite(fraction)) return;
            FuelFraction = Math.Max(0, Math.Min(1, fraction)); fuelChecked = true; EvaluateObjectives();
        }
        public void PrimerStroke(int actorId)
        {
            if (Accepts(actorId) && fault && !observation.GeneratorRunning)
            { if (primeStrokes < plan.PrimerStrokes) primeStrokes++; EvaluateObjectives(); }
        }
        public void SetChoke(int actorId, bool closed)
        {
            if (!Accepts(actorId) || (observation.GeneratorRunning && !closed && !CanOpenChoke)) return;
            chokeClosed = closed; EvaluateObjectives();
        }

        public RestartResult StarterPulled(int actorId)
        {
            RestartResult result;
            if (!Accepts(actorId)) result = RestartResult.NotPlaying;
            else if (!fault || observation.GeneratorRunning) result = RestartResult.NoFault;
            else if (startPending && ElapsedSeconds - lastCrankAt < plan.CrankCooldownSeconds) result = RestartResult.AwaitingEngine;
            else if (ElapsedSeconds - lastCrankAt < plan.CrankCooldownSeconds) result = RestartResult.Cooldown;
            else
            {
                startPending = false;
                lastCrankAt = ElapsedSeconds;
                if (!fuelChecked || FuelFraction <= 0) result = RestartResult.NoFuel;
                else if (primeStrokes < plan.PrimerStrokes) result = RestartResult.NeedsPrimer;
                else if (!chokeClosed) result = RestartResult.NeedsChoke;
                else { result = RestartResult.Requested; startPending = true; OnRequest?.Invoke(DayRequest.StartGenerator, actorId); }
            }
            OnRestartFeedback?.Invoke(result, actorId); OnChanged?.Invoke(); return result;
        }

        public void GeneratorStartRejected() { startPending = false; }

        void End(bool succeeded)
        {
            Stage = succeeded ? DayStage.Results : DayStage.Failed;
            Result = new DayResult(CurrentDay, ElapsedSeconds, PeakWaterHeightM, PumpStrokes, GeneratorRestarts, succeeded);
            if (succeeded) OnDayCompleted?.Invoke(CurrentDay); else OnDayFailed?.Invoke(CurrentDay);
            OnChanged?.Invoke();
        }
    }
}
