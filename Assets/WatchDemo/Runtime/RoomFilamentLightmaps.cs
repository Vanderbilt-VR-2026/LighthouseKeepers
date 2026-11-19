using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Dim only the existing baked pools. Portable direct lighting is unaffected; no new lights.
    public sealed class RoomFilamentLightmaps:MonoBehaviour
    {
        readonly Dictionary<Material,Material> copies=new();
        readonly List<(Renderer renderer,Material[] original)> changed=new();
        void Start()
        {
            var shader=Resources.Load<Shader>("RoomFilamentLit");
            if(!shader||!shader.isSupported){Debug.LogError("Missing room-filament shader; keeping original lighting");return;}
            foreach(var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){
                if(r.lightmapIndex<0||r.lightmapIndex>=LightmapSettings.lightmaps.Length)continue;
                var originals=r.sharedMaterials;var materials=(Material[])originals.Clone();bool used=false;
                for(int i=0;i<materials.Length;i++){
                    var m=materials[i];if(!m||m.shader.name!="Universal Render Pipeline/Lit")continue;
                    if(!copies.TryGetValue(m,out var copy)){copy=new Material(m){name=m.name+" (filament GI)",shader=shader};copies.Add(m,copy);}
                    materials[i]=copy;used=true;
                }
                if(used){changed.Add((r,originals));r.sharedMaterials=materials;}
            }
        }
        void OnDestroy(){foreach(var p in changed)if(p.renderer)p.renderer.sharedMaterials=p.original;foreach(var m in copies.Values)Destroy(m);Shader.SetGlobalVector("_KeeperRoomPulse",Vector4.one);Shader.SetGlobalFloat("_KeeperHousePulse",1);}
    }
}
