using System;
using System.Collections.Generic;

namespace LighthouseKeepers.Core.Progression
{
    public interface IDayDirector
    {
        int CurrentDay { get; }
        string CurrentObjectiveText { get; }
        IReadOnlyList<ObjectiveStep> Steps { get; }
        event Action<int> OnDayStarted;
        event Action<int> OnDayCompleted;
        event Action<int> OnDayFailed;
    }

    public readonly struct ObjectiveStep
    {
        public readonly string Text;
        public readonly bool Complete;
        public ObjectiveStep(string text, bool complete) { Text = text; Complete = complete; }
    }

    public enum DayStage { Inactive, WaitingForCrew, Playing, Results, Failed }
    public enum DayPredicate
    {
        WrenchGrabbed, NearLeak, LeakRepaired, WaterSafe, BedGrip,
        IncidentFired, FuelChecked, Primed, ChokeClosed, GeneratorRestored,
        TwoKeepers, PartnerIsolationObserved
    }
    public enum DayIncidentKind { ActivateLeak, GeneratorFault }
    public enum DayFailureCondition { SimulationFailed }
    public enum DaySuccessCondition { AllObjectives }
    public enum DayRequest { ResetRun, BeginDay, StartGenerator }
    public enum RestartResult { NotPlaying, NoFault, Cooldown, NoFuel, NeedsPrimer, NeedsChoke, AwaitingEngine, Requested }

    [Serializable]
    public sealed class ObjectiveDefinition
    {
        public string text = "New objective";
        public DayPredicate[] predicates = Array.Empty<DayPredicate>();
        public string incidentId = "";
    }

    [Serializable]
    public sealed class IncidentDefinition
    {
        public string id = "leak";
        public float atSeconds;
        public DayIncidentKind kind;
    }

    // Plain data copied from a day asset. Adding another day does not require a switch on day number.
    public sealed class DayPlan
    {
        public int Day;
        public int RequiredKeepers = 1;
        public string Heading = "";
        public string Briefing = "";
        public string FaultObjectivePrefix = "";
        public ObjectiveDefinition[] Objectives = Array.Empty<ObjectiveDefinition>();
        public IncidentDefinition[] Incidents = Array.Empty<IncidentDefinition>();
        public DaySuccessCondition Success;
        public DayFailureCondition Failure;
        public bool RequiresPartnerIsolation;
        public int PrimerStrokes = 3;
        public float CrankCooldownSeconds = 2f;
        public float ChokeReleaseSeconds = 10f;
        public float IsolationFreshnessSeconds = .3f;
    }

    // A read projection at the integration boundary, never a second simulation or writable snapshot.
    public readonly struct DayObservation
    {
        public readonly int Tick;
        public readonly float WaterHeightM;
        public readonly bool GeneratorRunning;
        public readonly bool Failed;
        public DayObservation(int tick, float height, bool generatorRunning, bool failed)
        { Tick = tick; WaterHeightM = height; GeneratorRunning = generatorRunning; Failed = failed; }
    }

    public readonly struct DayResult
    {
        public readonly int Day, PumpStrokes, GeneratorRestarts;
        public readonly float Seconds, PeakWaterHeightM;
        public readonly bool Succeeded;
        public DayResult(int day, float seconds, float peak, int strokes, int restarts, bool succeeded)
        { Day = day; Seconds = seconds; PeakWaterHeightM = peak; PumpStrokes = strokes; GeneratorRestarts = restarts; Succeeded = succeeded; }
    }
}
