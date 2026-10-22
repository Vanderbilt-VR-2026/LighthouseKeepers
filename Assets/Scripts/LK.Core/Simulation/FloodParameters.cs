using System;
using System.Collections.Generic;

namespace LighthouseKeepers.Core.Simulation
{
    /// <summary>Immutable copy of the tuning asset; the core never reads Unity objects.</summary>
    public sealed class FloodParameters
    {
        readonly DayTuning[] days;
        public int DayCount => days.Length;
        public DayTuning GetDay(int dayIndex)
        {
            if (dayIndex < 1 || dayIndex > days.Length) throw new ArgumentOutOfRangeException(nameof(dayIndex));
            return days[dayIndex - 1];
        }
        public double LeakInflowM3PerSec { get; }
        public double ManualPumpStrokeM3 { get; }
        public double ManualPumpMinStrokeSeconds { get; }
        public double ElectricPumpM3PerSec { get; }
        public double FloorAreaM2 { get; }
        public double FailHeightM { get; }
        public double SafeHeightM { get; }
        public double FailSustainSeconds { get; }
        public double ElectricPumpStartPower { get; }
        public double InitialFuelSeconds { get; }
        public double GeneratorFaultSeepM3PerSec { get; }

        public FloodParameters(double leakInflowM3PerSec, double manualPumpStrokeM3,
            double manualPumpMinStrokeSeconds, double electricPumpM3PerSec,
            double floorAreaM2, double failHeightM, double safeHeightM,
            double failSustainSeconds, double electricPumpStartPower, double initialFuelSeconds,
            double generatorFaultSeepM3PerSec, IReadOnlyList<DayTuning> days = null)
        {
            double[] positive = { leakInflowM3PerSec, manualPumpStrokeM3, manualPumpMinStrokeSeconds,
                electricPumpM3PerSec, floorAreaM2, failHeightM, failSustainSeconds };
            foreach (double value in positive)
                if (!FixedSimulationGeometry.Finite(value) || value <= 0)
                    throw new ArgumentException("Flood rates, area and durations must be finite and positive.");
            if (!FixedSimulationGeometry.Finite(safeHeightM) || safeHeightM < 0 || safeHeightM >= failHeightM ||
                !FixedSimulationGeometry.Finite(electricPumpStartPower) || electricPumpStartPower < 0 || electricPumpStartPower > 1 ||
                !FixedSimulationGeometry.Finite(initialFuelSeconds) || initialFuelSeconds < 0 ||
                !FixedSimulationGeometry.Finite(generatorFaultSeepM3PerSec) || generatorFaultSeepM3PerSec <= 0)
                throw new ArgumentException("Invalid safe height, power threshold, fuel duration or generator-fault seep rate.");
            LeakInflowM3PerSec = leakInflowM3PerSec; ManualPumpStrokeM3 = manualPumpStrokeM3;
            ManualPumpMinStrokeSeconds = manualPumpMinStrokeSeconds; ElectricPumpM3PerSec = electricPumpM3PerSec;
            FloorAreaM2 = floorAreaM2; FailHeightM = failHeightM; SafeHeightM = safeHeightM;
            FailSustainSeconds = failSustainSeconds; ElectricPumpStartPower = electricPumpStartPower;
            InitialFuelSeconds = initialFuelSeconds;
            GeneratorFaultSeepM3PerSec = generatorFaultSeepM3PerSec;
            if (days != null && days.Count != 7) throw new ArgumentException("Supply exactly seven day profiles.", nameof(days));
            this.days = new DayTuning[days == null ? 0 : days.Count];
            for (int i = 0; i < this.days.Length; i++)
            {
                DayTuning day = days[i];
                // Revalidate default(struct) as well as order; do not retain the caller's mutable array.
                this.days[i] = new DayTuning(day.DayIndex, day.LeakCount, day.InflowMultiplier,
                    day.GeneratorFault, day.DayLengthSeconds);
                if (day.DayIndex != i + 1) throw new ArgumentException("Day profiles must be ordered 1..7.", nameof(days));
            }
        }
    }
}
