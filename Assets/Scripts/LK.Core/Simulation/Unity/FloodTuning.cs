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

        public FloodParameters ToParameters() => new FloodParameters(leakInflowM3PerSec,
            manualPumpStrokeM3, manualPumpMinStrokeSeconds, electricPumpM3PerSec,
            floorAreaM2, failHeightM, safeHeightM, failSustainSeconds,
            electricPumpStartPower, initialFuelSeconds);
    }
}
