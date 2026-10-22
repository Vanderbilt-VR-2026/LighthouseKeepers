using System;
using LighthouseKeepers.Core.Simulation;
using UnityEngine;

namespace LighthouseKeepers.Simulation.Unity
{
    [CreateAssetMenu(menuName = "Lighthouse Keepers/Simulation/Flood Tuning")]
    public sealed class FloodTuning : ScriptableObject
    {
        // Defaults are deliberately stored only in the asset; tests read that same asset.
        [Min(0)] public float leakInflowM3PerSec;
        [Min(0)] public float manualPumpStrokeM3;
        [Min(0)] public float manualPumpMinStrokeSeconds;
        [Min(0)] public float electricPumpM3PerSec;
        [Min(0)] public float floorAreaM2;
        [Min(0)] public float failHeightM;
        [Min(0)] public float safeHeightM;
        [Min(0)] public float failSustainSeconds;
        [Range(0, 1)] public float electricPumpStartPower;
        [Min(0), Tooltip("Seconds of generator runtime in a full tank; preserved between days.")]
        public float initialFuelSeconds;
        [Min(0), Tooltip("Slower flow during a watch generator fault, m³/s. Must still exceed sustained manual throughput.")]
        public float generatorFaultSeepM3PerSec;

        [Tooltip("Exactly seven profiles ordered Day 1 through Day 7. D reads the same assets.")]
        public DayTuningAsset[] days;

        public DayTuning GetDay(int dayIndex)
        {
            if (days == null || days.Length != 7 || dayIndex < 1 || dayIndex > 7 || !days[dayIndex - 1])
                throw new InvalidOperationException("Assign seven ordered day tuning assets.");
            DayTuning result = days[dayIndex - 1].ToParameters();
            if (result.DayIndex != dayIndex) throw new InvalidOperationException("Day tuning is out of order.");
            return result;
        }
        DayTuning[] CopyDays()
        {
            var result = new DayTuning[7];
            for (int i = 0; i < result.Length; i++) result[i] = GetDay(i + 1);
            return result;
        }

        public FloodParameters ToParameters() => new FloodParameters(leakInflowM3PerSec,
            manualPumpStrokeM3, manualPumpMinStrokeSeconds, electricPumpM3PerSec,
            floorAreaM2, failHeightM, safeHeightM, failSustainSeconds,
            electricPumpStartPower, initialFuelSeconds, generatorFaultSeepM3PerSec, CopyDays());
    }
}
