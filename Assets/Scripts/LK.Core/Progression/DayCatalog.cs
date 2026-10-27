using System;
using UnityEngine;

namespace LighthouseKeepers.Core.Progression
{
    [CreateAssetMenu(menuName = "Lighthouse Keepers/Sprint 3/Day catalog")]
    public sealed class DayCatalog : ScriptableObject
    {
        public DayDefinition[] days = Array.Empty<DayDefinition>();
        public AudioClip objectiveTick;
        public DayDefinition Find(int day) { foreach (var entry in days) if (entry && entry.day == day) return entry; return null; }
        public DayDefinition Next(int day)
        {
            DayDefinition next = null;
            foreach (var entry in days) if (entry && entry.day > day && (!next || entry.day < next.day)) next = entry;
            return next;
        }
    }
}
