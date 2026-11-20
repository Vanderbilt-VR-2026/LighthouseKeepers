using System;
using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.Core.Progression
{
    [Serializable] public sealed class StoryVoiceLine
    {
        public int cue, priority;
        public string id, speaker, text, delivery, trigger, prerequisite, repeat, suppress, mode, file, resource;
        public bool Antagonist => id != null && id.StartsWith("ANT_", StringComparison.Ordinal);
        public bool Help => id != null && id.StartsWith("HM_HELP_", StringComparison.Ordinal);
        public bool OncePerRun => repeat == "1/run";
        public bool PerShip => repeat == "1/ship";
        public int NightLimit => repeat == "2/night" ? 2 : repeat == "1/night" ? 1 : int.MaxValue;
        public double Cooldown => repeat == "60 s cd" ? 60 : repeat == "30 s cd" ? 30 : 0;
    }
    public static class StoryVoiceCatalog
    {
        [Serializable] sealed class Table { public int schema; public StoryVoiceLine[] entries; }
        static Dictionary<string,StoryVoiceLine> byId;
        static Dictionary<int,StoryVoiceLine> byCue;
        static StoryVoiceLine[] entries;
        public const int FirstCue = 200, LastCue = 311;
        public static IReadOnlyList<StoryVoiceLine> Lines { get { Load(); return entries; } }
        static void Load()
        {
            if (byId != null) return;
            byId = new(StringComparer.Ordinal); byCue = new(); entries = Array.Empty<StoryVoiceLine>();
            var asset = Resources.Load<TextAsset>("StoryVoiceCatalog"); if (!asset) return;
            var table = JsonUtility.FromJson<Table>(asset.text);
            if (table?.schema != 1 || table.entries == null) throw new InvalidOperationException("Unknown story voice schema");
            foreach (var e in table.entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.id) || string.IsNullOrWhiteSpace(e.text) ||
                    e.cue < FirstCue || e.cue > LastCue || e.priority < 1 || e.priority > 4 ||
                    (e.mode != "S" && e.mode != "C" && e.mode != "B") || byId.ContainsKey(e.id) || byCue.ContainsKey(e.cue))
                    throw new InvalidOperationException("Invalid or duplicate story cue");
                byId.Add(e.id,e); byCue.Add(e.cue,e);
            }
            entries = table.entries;
        }
        public static StoryVoiceLine Get(string id) { Load(); return id != null && byId.TryGetValue(id,out var e) ? e : null; }
        public static StoryVoiceLine Get(int cue) { Load(); return byCue.TryGetValue(cue,out var e) ? e : null; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] public static void Invalidate() { byId=null; byCue=null; entries=null; }
    }
}
