using System;

namespace LighthouseKeepers.Core.Simulation
{
    /// <summary>Immutable copy of the tuning asset; the core never reads Unity objects.</summary>
    public sealed class FloodParameters
    {
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

        public FloodParameters(double leakInflowM3PerSec, double manualPumpStrokeM3,
            double manualPumpMinStrokeSeconds, double electricPumpM3PerSec,
            double floorAreaM2, double failHeightM, double safeHeightM,
            double failSustainSeconds, double electricPumpStartPower, double initialFuelSeconds)
        {
            double[] positive = { leakInflowM3PerSec, manualPumpStrokeM3, manualPumpMinStrokeSeconds,
                electricPumpM3PerSec, floorAreaM2, failHeightM, failSustainSeconds };
            foreach (double value in positive)
                if (!FixedSimulationGeometry.Finite(value) || value <= 0)
                    throw new ArgumentException("Flood rates, area and durations must be finite and positive.");
            if (!FixedSimulationGeometry.Finite(safeHeightM) || safeHeightM < 0 || safeHeightM >= failHeightM ||
                !FixedSimulationGeometry.Finite(electricPumpStartPower) || electricPumpStartPower < 0 || electricPumpStartPower > 1 ||
                !FixedSimulationGeometry.Finite(initialFuelSeconds) || initialFuelSeconds < 0)
                throw new ArgumentException("Invalid safe height, power threshold or fuel duration.");
            LeakInflowM3PerSec = leakInflowM3PerSec; ManualPumpStrokeM3 = manualPumpStrokeM3;
            ManualPumpMinStrokeSeconds = manualPumpMinStrokeSeconds; ElectricPumpM3PerSec = electricPumpM3PerSec;
            FloorAreaM2 = floorAreaM2; FailHeightM = failHeightM; SafeHeightM = safeHeightM;
            FailSustainSeconds = failSustainSeconds; ElectricPumpStartPower = electricPumpStartPower;
            InitialFuelSeconds = initialFuelSeconds;
        }
    }
}
