using UnityEngine;
namespace LighthouseKeepers.Core.Progression
{
    [CreateAssetMenu(menuName="Lighthouse Keepers/Five-night campaign")]
    public sealed class FiveNightCatalog:ScriptableObject
    {
        [Tooltip("Select the five-night task schedule. Ships and saved progression are still in development.")]
        public bool enabled;
        public DayDefinition[] nights;
        public DayDefinition demo;
    }
}
