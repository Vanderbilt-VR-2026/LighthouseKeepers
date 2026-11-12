using System;
namespace LighthouseKeepers.Core.Progression
{
    public enum TensionPhase { Calm, Dread, Sabotage, Hunt, Relief }
    [Serializable] public sealed class TensionFrame
    {
        public bool enabled, prepared, huntPlanned, hunting;
        public int phase, storm = 1, music;
        public float value;
        public double quietUntil, nextProblemAt, reliefUntil, lastProblemAt = -1;
        public TensionFrame Copy() => (TensionFrame)MemberwiseClone();
        public bool Valid(double now) => double.IsFinite(now) && now >= 0 &&
            phase >= 0 && phase <= 4 && storm >= 1 && storm <= 4 && music >= 0 && music <= 2 &&
            float.IsFinite(value) && value >= 0 && value <= 1 &&
            Time(quietUntil, now + 3.01) && Time(nextProblemAt, now + 60.01) && Time(reliefUntil, now + 20.01) &&
            double.IsFinite(lastProblemAt) && lastProblemAt >= -1 && lastProblemAt <= now + .01 &&
            (!hunting || !prepared && phase == (int)TensionPhase.Hunt) && (!huntPlanned || prepared) &&
            (enabled || !prepared && !hunting && !huntPlanned && phase == 0 && value == 0 && quietUntil == 0 && nextProblemAt == 0 && reliefUntil == 0 && lastProblemAt == -1);
        static bool Time(double t, double max) => double.IsFinite(t) && t >= 0 && t <= max;
    }
    // One simulation-clock authority. The authored schedule resolves dependencies; views render outputs.
    public sealed class TensionDirector
    {
        readonly NightScenario night;
        readonly double gap, quietSeconds, earliestScare;
        TensionFrame frame = new TensionFrame { enabled = true };
        public bool ReadOnly { get; set; }
        public TensionDirector(NightScenario night, double minimumGap = 15, double quiet = 2.5, double firstScare = 60)
        {
            this.night = night ?? throw new ArgumentNullException(nameof(night));
            gap = Math.Clamp(Math.Max(minimumGap, night.faultSpacingSeconds), 15, 60);
            quietSeconds = Math.Clamp(quiet, 2, 3); earliestScare = Math.Max(60, firstScare);
        }
        public TensionPhase Phase => (TensionPhase)frame.phase;
        public float Value => frame.value;
        public int StormStage => frame.storm;
        public int MusicLayer => frame.music;
        public double ReliefUntil => frame.reliefUntil;
        public double NextProblemAt => frame.nextProblemAt;
        public bool Quiet(double now) => frame.prepared && now < frame.quietUntil;
        public bool CanProblem(double now, bool finale = false) => !ReadOnly &&
            (finale && night.finaleFaultPileup || !frame.prepared && !frame.hunting && now >= frame.nextProblemAt && now >= frame.reliefUntil);
        public void Problem(double now)
        {
            if (ReadOnly) return;
            frame.lastProblemAt = now; frame.nextProblemAt = now + gap;
            CancelPreparation();
        }
        public void Fixed(double now)
        {
            if (ReadOnly) return;
            frame.reliefUntil = Math.Max(frame.reliefUntil, now + Math.Clamp(night.reliefSeconds, 10, 20));
            frame.hunting = false; frame.phase = (int)TensionPhase.Relief; CancelPreparation();
        }
        void CancelPreparation() { frame.prepared = frame.huntPlanned = false; frame.quietUntil = 0; }
        // Future sabotage/entity must obtain this ticket, then recheck safety when it launches.
        // No hunt or scare is spawned by the foundation itself.
        public bool PrepareScare(double now, bool tutorial, bool dark, bool escapeReachable, bool hunt)
        {
            if (ReadOnly || now < earliestScare || tutorial || night.noFail || !dark || !escapeReachable || !CanProblem(now)) return false;
            frame.prepared = true; frame.huntPlanned = hunt; frame.quietUntil = now + quietSeconds;
            return true;
        }
        public bool LaunchScare(double now, bool tutorial, bool dark, bool escapeReachable)
        {
            if (ReadOnly || !frame.prepared || now < frame.quietUntil) return false;
            bool hunt = frame.huntPlanned; CancelPreparation();
            if (now < earliestScare || tutorial || !dark || !escapeReachable || !CanProblem(now)) return false;
            Problem(now); frame.hunting = hunt;
            if (hunt) frame.phase = (int)TensionPhase.Hunt;
            return true;
        }
        public void Retreat(double now) { if (!ReadOnly) Fixed(now); }
        public void Tick(double now, double remaining, double duration, float danger, float water,
            bool dark, double darkRemaining, bool complete, bool activeFault)
        {
            if (ReadOnly) return;
            if (complete) { frame.hunting = false; CancelPreparation(); }
            bool relief = now < frame.reliefUntil;
            int stage = remaining <= 120 || danger >= .9f ? 4 : danger >= .75f || remaining <= duration * .4 ? 3 : danger >= .5f || remaining <= duration * .7 ? 2 : 1;
            stage = Math.Max(stage, night.stormBaseStage);
            if (now >= duration * night.peakStormAfterFraction) stage = 4;
            frame.storm = complete ? 1 : Math.Max(1, stage - (relief ? 1 : 0));
            frame.phase = (int)(complete ? TensionPhase.Calm : frame.hunting ? TensionPhase.Hunt : relief ? TensionPhase.Relief : frame.lastProblemAt >= 0 && now - frame.lastProblemAt < 3 ? TensionPhase.Sabotage : activeFault || danger > .25f ? TensionPhase.Dread : TensionPhase.Calm);
            frame.value = complete ? 0 : Math.Clamp(Math.Max(frame.hunting ? 1 : danger, (frame.storm - 1) / 3f) * (relief ? .65f : 1), 0, 1);
            frame.music = complete ? 0 : frame.hunting ? 2 : relief ? 0 : water > .7f || dark && darkRemaining < 20 || remaining < 60 || frame.storm == 4 ? 2 : activeFault ? 1 : 0;
        }
        public TensionFrame Capture() => frame.Copy();
        public bool Apply(TensionFrame source, double now)
        {
            if (!ReadOnly || source == null || !source.enabled || !source.Valid(now)) return false;
            frame = source.Copy(); return true;
        }
    }
}
