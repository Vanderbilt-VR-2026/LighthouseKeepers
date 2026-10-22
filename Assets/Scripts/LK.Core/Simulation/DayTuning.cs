using System;

namespace LighthouseKeepers.Core.Simulation
{
    /// <summary>Immutable watch plan copied from B's tuning assets. D owns incident timing,
    /// objective completion and the day clock; this plan never fires an incident by itself.</summary>
    public readonly struct DayTuning
    {
        public int DayIndex { get; }
        public int LeakCount { get; }
        public double InflowMultiplier { get; }
        public bool GeneratorFault { get; }
        public double DayLengthSeconds { get; }

        public DayTuning(int dayIndex, int leakCount, double inflowMultiplier,
            bool generatorFault, double dayLengthSeconds)
        {
            if (dayIndex < 1 || dayIndex > 7 || leakCount < 0 || leakCount > 3 ||
                !FixedSimulationGeometry.Finite(inflowMultiplier) || inflowMultiplier <= 0 ||
                !FixedSimulationGeometry.Finite(dayLengthSeconds) || dayLengthSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(dayIndex), "Day 1..7, 0..3 couplings and positive finite multiplier/duration are required.");
            DayIndex = dayIndex; LeakCount = leakCount; InflowMultiplier = inflowMultiplier;
            GeneratorFault = generatorFault; DayLengthSeconds = dayLengthSeconds;
        }
    }
}
