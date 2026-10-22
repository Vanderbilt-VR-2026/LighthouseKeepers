using System;
using System.IO;
using System.Linq;
using UnityEngine;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Simulation.Unity;
namespace LighthouseKeepers.Net
{
    [Serializable] public sealed class ShipManifest
    {
        public int version;
        public string lobbyScene, networkLobbyScene, entryScene, entryDressScene;
        public string[] scenes, additiveScenes;
        public string EntryName => Path.GetFileNameWithoutExtension(entryScene);
        public string LobbyName => Path.GetFileNameWithoutExtension(lobbyScene);
        public static ShipManifest Parse(string json)
        {
            var p=JsonUtility.FromJson<ShipManifest>(json);
            if(p==null || p.version!=2 || p.scenes==null || p.additiveScenes==null || p.scenes.Length!=14 ||
               p.additiveScenes.Length!=11 || p.scenes.Distinct().Count()!=14 ||
               p.entryScene!="Assets/Scenes/Shell/LK_Shell_House.unity" ||
               !p.scenes.SequenceEqual(new[]{p.lobbyScene,p.entryScene}.Concat(p.additiveScenes).Concat(new[]{p.networkLobbyScene})) ||
               p.additiveScenes[0]!=p.entryDressScene || p.scenes.Any(Excluded))
                throw new InvalidOperationException("Invalid canonical ShipScenes.json: require lobby, House, eleven additive Shell/Dress scenes and NetworkLobby; no rehearsal scenes.");
            var world=new[]{p.entryScene}.Concat(p.additiveScenes).ToArray();
            var expected=new[]{"House","F1","F2","F3","F4","Exterior"}.SelectMany(n=>new[]{"Assets/Scenes/Shell/LK_Shell_"+n+".unity","Assets/Scenes/Dress/LK_Dress_"+n+".unity"});
            if(!world.SequenceEqual(expected))throw new InvalidOperationException("Ship manifest must contain every Shell/Dress pair in encounter order.");
            return p;
        }
        static bool Excluded(string path) => string.IsNullOrEmpty(path) || path.Contains("LK_StormWatch") || path.Contains("LK_Lab_Environment") || path.Contains("LK_DevGym") || path.Contains("LK_Bootstrap") || path.Contains("LK_Level");
    }
    public sealed class ShipRouteSettings : ScriptableObject
    {
        public const string RoomPrefix="LK_S3R2_";
        public TextAsset manifest;
        public GameObject localRig;
        public Vector3 houseSpawn = new Vector3(0,0,-8.5f);
        public float houseSpawnYaw;
        public DayCatalog days;
        public FloodTuning tuning;
        ShipManifest parsed;
        public ShipManifest Plan => parsed ?? (parsed=manifest ? ShipManifest.Parse(manifest.text) : throw new InvalidOperationException("Assign the authoritative ShipScenes.json TextAsset."));
        void OnValidate()=>parsed=null;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterRoutes()
        {
            LighthouseKeepers.Networking.FusionLobbyTransport.ResolveGameplayScene=()=>Load().Plan.EntryName;
            LighthouseKeepers.Networking.FusionLobbyTransport.ResolveSessionRoom=code=>RoomPrefix+code;
        }
        public static ShipRouteSettings Load() => Resources.Load<ShipRouteSettings>("ShipRouteSettings") ?? throw new InvalidOperationException("Run LK/Ship/Bind lobby and player routes before Play Solo or building.");
    }
}
