using System.Reflection;
using UnityEngine;

namespace LighthouseKeepers.Gameplay.Interaction
{
    [CreateAssetMenu(menuName = "Lighthouse Keepers/Hands/Interaction settings")]
    public sealed class InteractionSettings : ScriptableObject
    {
        [Tooltip("Session B's Assets/Data/Tuning/FloodTuning.asset. Read only; never clone simulation tuning here.")]
        [SerializeField] UnityEngine.Object floodTuning;
        public bool TryGetMinimumStrokeSeconds(out float seconds)
        {
            seconds = 0;
            if (!floodTuning) return false;
            // B owns the concrete asset type; bind its agreed field once, without adding a cyclic assembly dependency.
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var type = floodTuning.GetType();
            var field = type.GetField("manualPumpMinStrokeSeconds", flags);
            object value = field != null ? field.GetValue(floodTuning) : type.GetProperty("manualPumpMinStrokeSeconds", flags)?.GetValue(floodTuning);
            if (!(value is float duration) || float.IsNaN(duration) || float.IsInfinity(duration) || duration <= 0) return false;
            seconds = duration; return true;
        }
    }
}
