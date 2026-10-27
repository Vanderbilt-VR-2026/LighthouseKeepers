using Fusion;
using UnityEditor;
using UnityEngine;
namespace LighthouseKeepers.Net.Editor
{
    public static class NetworkConditionsMenu
    {
        [MenuItem("Lighthouse Keepers/Sprint 3/Networking/Simulate 80 ms added RTT and 1 percent loss (Play Mode)")]
        public static void Impair()
        {
            if(!EditorApplication.isPlaying){Debug.LogError("Enter Play Mode at the lobby before hosting, then choose this command.");return;}
            if(FusionMessageChannel.Active){Debug.LogError("Leave the session before changing network conditions.");return;}
            var conditions=NetworkProjectConfig.Global.NetworkConditions;
            conditions.Enabled=true;conditions.DelayMin=conditions.DelayMax=.04f;conditions.AdditionalJitter=0;
            conditions.LossChanceMin=conditions.LossChanceMax=.01f;conditions.AdditionalLoss=0;
            Debug.Log("Network simulation requested: 40 ms one-way delay, 1% loss. Measure total RTT; do not enable on both peers without accounting for compounded delay/loss.");
        }
        [MenuItem("Lighthouse Keepers/Sprint 3/Networking/Use strict two-actor gate (Play Mode)")]
        public static void StrictGate()
        {
            if (!EditorApplication.isPlaying) return;
            DebugSoloOverride.Enabled=false;
            Debug.Log("Day 3 solo override disabled for this Editor host. Requires D to bind the new gate.");
        }
        [MenuItem("Lighthouse Keepers/Sprint 3/Networking/Clear simulated network conditions (Play Mode)")]
        public static void Clear()
        {
            if(!EditorApplication.isPlaying || FusionMessageChannel.Active)return;
            NetworkProjectConfig.Global.NetworkConditions.Enabled=false;
        }
    }
}
