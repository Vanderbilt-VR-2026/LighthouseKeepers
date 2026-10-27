using System.Collections.Generic;
using UnityEngine;
using LighthouseKeepers.StormWatch;
namespace LighthouseKeepers.Net
{
    /// <summary>Suppresses legacy placeholder meshes only when the new network avatar is available.</summary>
    public sealed class LegacyKeeperVisibility : MonoBehaviour
    {
        readonly List<Renderer> hidden = new();
        float nextScan;
        void Update()
        {
            if (!FusionMessageChannel.Active || !NetworkPrefabRegistry.Player) { Restore();return; }
            if (Time.unscaledTime<nextScan) return;
            nextScan=Time.unscaledTime+1;
            hidden.RemoveAll(item=>!item);
            if(hidden.Count>0)return;
            var legacy=FindAnyObjectByType<StormWatchDirector>();
            if(!legacy || !legacy.Ready)return;
            foreach (var item in legacy.GetComponentsInChildren<Renderer>(true))
            {
                string n=item.gameObject.name;
                if (n!="Other keeper head" && n!="Other keeper coat" && n!="Other keeper left glove" && n!="Other keeper right glove") continue;
                if (!item.enabled) continue;
                hidden.Add(item);item.enabled=false;
            }
        }
        void Restore() { foreach (var item in hidden) if (item) item.enabled=true;hidden.Clear(); }
        void OnDestroy() => Restore();
    }
}
