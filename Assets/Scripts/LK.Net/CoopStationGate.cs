using System;
using System.Collections.Generic;

namespace LighthouseKeepers.Net
{
    /// <summary>Host-only, expiring station holds. The caller supplies authenticated actor IDs.</summary>
    public sealed class CoopStationGate
    {
        public const double HoldTimeoutSeconds = .6;
        readonly Dictionary<int, Hold> holds = new();
        readonly struct Hold
        {
            public readonly int Actor;
            public readonly double Seen;
            public Hold(int actor, double seen) { Actor = actor; Seen = seen; }
        }
        public bool Observe(int actor, int station, bool held, double now)
        {
            if (actor <= 0 || station <= 0 || double.IsNaN(now) || double.IsInfinity(now)) return false;
            if (!held)
            {
                if (holds.TryGetValue(station, out var old) && old.Actor == actor) holds.Remove(station);
                return true;
            }
            if (holds.TryGetValue(station, out var current) && Fresh(current, now) && current.Actor != actor) return false;
            holds[station] = new Hold(actor, now);
            return true;
        }
        public bool Allows(int firstStation, int secondStation, double now)
        {
            return firstStation != secondStation && holds.TryGetValue(firstStation, out var first) &&
                holds.TryGetValue(secondStation, out var second) && Fresh(first, now) && Fresh(second, now) && first.Actor != second.Actor;
        }
        public bool AllowsForDevelopment(int firstStation, int secondStation, double now)
        {
            if (Allows(firstStation, secondStation, now)) return true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return DebugSoloOverride.Enabled && firstStation != secondStation &&
                holds.TryGetValue(firstStation, out var first) && holds.TryGetValue(secondStation, out var second) &&
                Fresh(first, now) && Fresh(second, now);
#else
            return false;
#endif
        }
        public void RemoveActor(int actor)
        {
            var remove = new List<int>();
            foreach (var entry in holds) if (entry.Value.Actor == actor) remove.Add(entry.Key);
            foreach (var station in remove) holds.Remove(station);
        }
        public void Clear() => holds.Clear();
        static bool Fresh(Hold hold, double now) => now >= hold.Seen && now - hold.Seen <= HoldTimeoutSeconds;
    }
}
