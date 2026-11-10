using UnityEngine;
namespace LighthouseKeepers.Core.Progression
{
    [CreateAssetMenu(menuName="Lighthouse Keepers/Five-night campaign")]
    public sealed class FiveNightCatalog:ScriptableObject
    {
        [Tooltip("Keep off until schedules, ships, progression and timing have passed their delivery gates.")]
        public bool enabled;
        public DayDefinition[] nights;
        public DayDefinition demo;
    }
}
