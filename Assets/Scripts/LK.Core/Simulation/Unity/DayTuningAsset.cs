using LighthouseKeepers.Core.Simulation;
using UnityEngine;

namespace LighthouseKeepers.Simulation.Unity
{
    [CreateAssetMenu(menuName = "Lighthouse Keepers/Simulation/Day Tuning")]
    public sealed class DayTuningAsset : ScriptableObject
    {
        [Range(1, 7)] public int dayIndex;
        [Range(0, 3)] public int leakCount;
        [Min(0)] public float inflowMultiplier;
        public bool generatorFault;
        [Min(0), Tooltip("Watch duration in seconds; D owns the clock and incident schedule.")]
        public float dayLengthSeconds;

        public DayTuning ToParameters() => new DayTuning(dayIndex, leakCount,
            inflowMultiplier, generatorFault, dayLengthSeconds);
    }
}
