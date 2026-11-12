using System;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchState
    {
        public Sprint5Tuning Sprint5 { get; private set; }
        public TensionDirector Tension { get; private set; }
        public bool TensionQuiet => Tension?.Quiet(Elapsed) == true;
        public bool SuppressBackgroundNarration => TensionQuiet || Tension?.Phase == TensionPhase.Hunt;
        TensionDirector NewTension(DayDefinition day) => Master && Sprint5 && Sprint5.tensionDirector && day.scenario?.configured == true
            ? new TensionDirector(day.scenario, Sprint5.minimumProblemGapSeconds, Sprint5.preScareQuietSeconds, Sprint5.earliestScareSeconds) { ReadOnly = ReadOnly } : null;
        void TickTension()
        {
            if (Tension == null || ReadOnly) return;
            bool active = false;
            foreach (var job in Definition.tasks) if (!IsDone(job.stationId) && !IsPending(job.stationId)) { active = true; break; }
            float water = Math.Max(0, LatestWater) / (Tuning ? Tuning.failWaterHeightM : .55f);
            Tension.Tick(Elapsed, Duration - Elapsed, Duration, Danger, water, !BeaconShining && BeaconDarkLimit > 0, BeaconDarkRemaining, Complete, active);
        }
        // A recovery window can extend the arrival tank budget. It never cuts a running engine
        // or fakes power: the physical fuel gauge still reaches E, with its last three seconds of sputter.
        void PaceArrivalFuel()
        {
            if (Tension == null || !NightTwoFuelFailure || NightTwoOutage || Engine == null || !Engine.Running) return;
            double protectedSeconds = Math.Max(Tension.ReliefUntil, Tension.NextProblemAt) - Elapsed;
            float normal = TutorialFuelDrain;
            float rate = protectedSeconds > 0 ? Math.Min(normal, Engine.Fuel / (float)(protectedSeconds + Master.fuelSputterSeconds)) : normal;
            Engine.SetConsumption(Math.Min(Engine.Consumption,rate));
        }
        public bool PrepareScare(bool dark, bool escapeReachable, bool hunt) => !ReadOnly && !Failed && !Complete &&
            Tension != null && Tension.PrepareScare(Elapsed, Tutorial || WatchStartHeld || StationGraceRemaining > 0, dark, escapeReachable, hunt);
        public bool LaunchScare(bool dark, bool escapeReachable) => !ReadOnly && !Failed && !Complete && Tension != null &&
            Tension.LaunchScare(Elapsed, Tutorial || WatchStartHeld || StationGraceRemaining > 0, dark, escapeReachable);
    }
}
