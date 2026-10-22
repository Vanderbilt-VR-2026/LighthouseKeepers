using System;
using System.IO;
using System.Linq;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Networking;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace LighthouseKeepers.Net.Editor
{
    public static class ShipRouteAuthoring
    {
        public const string ManifestPath="Assets/Scenes/Sprint3/ShipScenes.json";
        public const string SettingsPath="Assets/Prefabs/Network/Resources/ShipRouteSettings.asset";
        public const string PlayerPath="Assets/Prefabs/Network/Resources/PF_ShipLocalPlayer.prefab";
        [MenuItem("LK/Ship/Bind lobby and player routes")]
        public static void Apply()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new BuildFailedException("Stop Play Mode before binding ship routes.");
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new BuildFailedException("Save the open scenes before binding ship routes.");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var manifest=AssetDatabase.LoadAssetAtPath<TextAsset>(ManifestPath);
                if(!manifest)throw new BuildFailedException("Missing canonical ShipScenes.json.");
                var plan=ShipManifest.Parse(manifest.text);
                var settings=AssetDatabase.LoadAssetAtPath<ShipRouteSettings>(SettingsPath);
                if(!settings){settings=ScriptableObject.CreateInstance<ShipRouteSettings>();AssetDatabase.CreateAsset(settings,SettingsPath);}
                settings.manifest=manifest;
                settings.days=AssetDatabase.LoadAssetAtPath<LighthouseKeepers.Core.Progression.DayCatalog>("Assets/Data/Days/DayCatalog.asset");
                settings.tuning=AssetDatabase.LoadAssetAtPath<LighthouseKeepers.Simulation.Unity.FloodTuning>("Assets/Data/Tuning/FloodTuning.asset");
                var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_LighthouseKeepers/Prefabs/Player/LK_QuestPlayer.prefab");
                if(!source)throw new BuildFailedException("Missing the existing local XR rig.");
                var rig=(GameObject)PrefabUtility.InstantiatePrefab(source);
                try
                {
                    // Clone the established rig and input bindings; never edit A's hand prefabs.
                    foreach(var c in rig.GetComponentsInChildren<Behaviour>(true))
                        if(c && c.GetType().Name=="TunnelingVignetteController")
                        {c.enabled=false;c.gameObject.SetActive(false);Debug.Log("Round2 vignette removed from cloned rig: "+c.transform.name);}
                    foreach(var filter in rig.GetComponentsInChildren<MeshFilter>(true))
                        if(filter.sharedMesh && !AssetDatabase.GetAssetPath(filter.sharedMesh).StartsWith("Assets/",StringComparison.Ordinal) && !AssetDatabase.GetAssetPath(filter.sharedMesh).StartsWith("Packages/",StringComparison.Ordinal))
                        {var renderer=filter.GetComponent<MeshRenderer>();if(renderer)UnityEngine.Object.DestroyImmediate(renderer);UnityEngine.Object.DestroyImmediate(filter);}
                    settings.localRig=PrefabUtility.SaveAsPrefabAsset(rig,PlayerPath);
                }
                finally{UnityEngine.Object.DestroyImmediate(rig);}
                EditorUtility.SetDirty(settings);AssetDatabase.SaveAssetIfDirty(settings);
                const string configPath="Assets/Prefabs/Network/CrewShipLobbyConfig.asset";
                foreach(var path in new[]{plan.lobbyScene,plan.networkLobbyScene})
                {
                    var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                    var manager=UnityEngine.Object.FindAnyObjectByType<LobbyManager>();
                    var screen=UnityEngine.Object.FindAnyObjectByType<LobbyScreen>();
                    if(!manager || !screen)throw new BuildFailedException("Missing existing lobby manager/board: "+path);
                    var managerData=new SerializedObject(manager);
                    var original=managerData.FindProperty("config").objectReferenceValue as LobbyConfig;
                    var config=AssetDatabase.LoadAssetAtPath<LobbyConfig>(configPath);
                    if(!config){config=UnityEngine.Object.Instantiate(original);AssetDatabase.CreateAsset(config,configPath);}
                    var configData=new SerializedObject(config);configData.FindProperty("startSceneName").stringValue=plan.EntryName;
                    configData.FindProperty("maxPlayers").intValue=2;configData.FindProperty("minPlayersToStart").intValue=2;
                    configData.FindProperty("requireAllReady").boolValue=true;configData.ApplyModifiedPropertiesWithoutUndo();
                    foreach(var old in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                        if(old && (old.GetType().Name=="KeeperReplicationSession" || old.GetType().Name=="MultiplayerPanel"))UnityEngine.Object.DestroyImmediate(old);
                    var raw=UnityEngine.Object.FindAnyObjectByType<FusionLobbyTransport>();
                    if(!raw)raw=new GameObject("Fusion crew session").AddComponent<FusionLobbyTransport>();
                    var rawData=new SerializedObject(raw);rawData.FindProperty("lobbySceneName").stringValue=plan.LobbyName;rawData.ApplyModifiedPropertiesWithoutUndo();
                    var crew=raw.GetComponent<CrewLobbyTransport>()??raw.gameObject.AddComponent<CrewLobbyTransport>();
                    var crewData=new SerializedObject(crew);crewData.FindProperty("fusion").objectReferenceValue=raw;crewData.ApplyModifiedPropertiesWithoutUndo();
                    Add<FusionMessageChannel>(raw.gameObject);Add<CrewPoseStream>(raw.gameObject);Add<FusionGrabAuthority>(raw.gameObject);
                    managerData.FindProperty("config").objectReferenceValue=config;managerData.FindProperty("transportBehaviour").objectReferenceValue=crew;managerData.ApplyModifiedPropertiesWithoutUndo();
                    var board=screen.GetComponent<CrewLobbyBoard>()??screen.gameObject.AddComponent<CrewLobbyBoard>();
                    var boardData=new SerializedObject(board);boardData.FindProperty("transport").objectReferenceValue=crew;boardData.ApplyModifiedPropertiesWithoutUndo();
                    // Only component/config references change; all lobby art, text layout and transforms remain authored.
                    EditorSceneManager.SaveScene(scene);
                }
                AssetDatabase.SaveAssets();
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
        static void Add<T>(GameObject root) where T:Component {if(!root.GetComponent<T>())root.AddComponent<T>();}
        public static void ApplyBuildScenes()
        {
            var text=AssetDatabase.LoadAssetAtPath<TextAsset>(ManifestPath);
            if(!text)throw new BuildFailedException("Missing ship manifest.");
            var plan=ShipManifest.Parse(text.text);
            foreach(var path in plan.scenes)if(!File.Exists(path))throw new BuildFailedException("Missing ship scene: "+path);
            EditorBuildSettings.scenes=plan.scenes.Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();
        }
        public static void Validate()
        {
            var settings=AssetDatabase.LoadAssetAtPath<ShipRouteSettings>(SettingsPath);
            if(!settings || !settings.localRig || !settings.days || !settings.tuning || AssetDatabase.GetAssetPath(settings.manifest)!=ManifestPath)
                throw new BuildFailedException("Run LK/Ship/Bind lobby and player routes.");
            if(!EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).SequenceEqual(settings.Plan.scenes))
                throw new BuildFailedException("Build Settings differ from the canonical ship manifest.");
        }
    }
}
