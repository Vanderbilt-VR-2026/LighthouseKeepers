using System;

namespace LighthouseKeepers.Progression
{
    public enum WatchPhase { Briefing, Repair, Escape, Recovery, Complete, Failed }
    public enum ValveResult { Ignored, WrongValve, Progress, ValveSealed, RepairComplete }

    /// <summary>Local first-floor rehearsal rules. Contains no networking or scene authority.</summary>
    public sealed class FirstWatchRules
    {
        public const float DefaultDegreesPerValve = 270;
        public const float DefaultAirSeconds = 6;
        public const float DefaultRecoverySeconds = 12;
        public float DegreesPerValve { get; }
        public float AirSeconds { get; }
        public float RecoverySeconds { get; }

        public FirstWatchRules(float degreesPerValve = DefaultDegreesPerValve,
            float airSeconds = DefaultAirSeconds, float recoverySeconds = DefaultRecoverySeconds)
        {
            DegreesPerValve = Positive(degreesPerValve, nameof(degreesPerValve));
            AirSeconds = Positive(airSeconds, nameof(airSeconds));
            RecoverySeconds = Positive(recoverySeconds, nameof(recoverySeconds));
        }

        static float Positive(float value, string parameter)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0)
                throw new ArgumentOutOfRangeException(parameter, "Watch thresholds must be finite and positive.");
            return value;
        }
        readonly int[] order = { 0, 1, 2 };
        public WatchPhase Phase { get; private set; } = WatchPhase.Briefing;
        public int SealedCount { get; private set; }
        public float TurnDegrees { get; private set; }
        public float SubmergedSeconds { get; private set; }
        public float RecoveryRemaining { get; private set; }
        public float Elapsed { get; private set; }
        public int Mistakes { get; private set; }
        public int NextValve => SealedCount < 3 ? order[SealedCount] : -1;
        public int ValveAt(int step) => order[step];
        public bool IsSealed(int valve) => Array.IndexOf(order, valve) < SealedCount && valve >= 0 && valve < 3;

        public void Reset(int seed)
        {
            for (int i = 0; i < 3; i++) order[i] = i;
            var random = new Random(seed);
            for (int i = 2; i > 0; i--) { int j = random.Next(i + 1); (order[i], order[j]) = (order[j], order[i]); }
            Phase = WatchPhase.Briefing;
            SealedCount = Mistakes = 0;
            TurnDegrees = SubmergedSeconds = RecoveryRemaining = Elapsed = 0;
        }

        public void Begin() { if (Phase == WatchPhase.Briefing) Phase = WatchPhase.Repair; }

        public ValveResult Turn(int valve, float degrees, bool braced)
        {
            if (Phase != WatchPhase.Repair || !braced || valve < 0 || valve > 2 ||
                float.IsNaN(degrees) || float.IsInfinity(degrees) || degrees <= 0 || IsSealed(valve)) return ValveResult.Ignored;
            if (valve != NextValve) { Mistakes++; return ValveResult.WrongValve; }
            TurnDegrees += degrees;
            if (TurnDegrees < DegreesPerValve) return ValveResult.Progress;
            TurnDegrees = 0;
            SealedCount++;
            if (SealedCount < 3) return ValveResult.ValveSealed;
            Phase = WatchPhase.Escape;
            return ValveResult.RepairComplete;
        }

        public void Tick(float seconds, float headClearance, bool onLanding)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds <= 0) return;
            if (Phase == WatchPhase.Repair || Phase == WatchPhase.Escape)
            {
                Elapsed += seconds;
                SubmergedSeconds = headClearance <= 0 ? SubmergedSeconds + seconds : Math.Max(0, SubmergedSeconds - seconds * 2);
                if (SubmergedSeconds >= AirSeconds) { Phase = WatchPhase.Failed; return; }
                if (Phase == WatchPhase.Escape && onLanding && headClearance > 0)
                { Phase = WatchPhase.Recovery; RecoveryRemaining = RecoverySeconds; }
            }
            else if (Phase == WatchPhase.Recovery)
            {
                // Breathing room must actually be spent on the safe landing.
                if (!onLanding || headClearance <= 0) { Phase = WatchPhase.Escape; RecoveryRemaining = 0; return; }
                RecoveryRemaining = Math.Max(0, RecoveryRemaining - seconds);
                if (RecoveryRemaining <= 0) Phase = WatchPhase.Complete;
            }
        }
    }
}
