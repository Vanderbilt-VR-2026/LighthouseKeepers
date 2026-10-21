using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LighthouseKeepers.Editor.Environment
{
    public static class EnvironmentLobbyBuild
    {
        public const string LobbyPath="Assets/Scenes/Lab/LK_Lobby_Environment.unity";
        public static void Prepare()
        {
            // Reuse the team's authored lobby and behavior, with a separate destination config.
            EditorSceneManager.OpenScene("Assets/_LighthouseKeepers/Scenes/Development/LK_LobbyPreview.unity",OpenSceneMode.Single);
            var manager=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(c=>c.GetType().FullName=="LighthouseKeepers.Lobby.LobbyManager");
            var serialized=new SerializedObject(manager);var field=serialized.FindProperty("config");
            var type=manager.GetType().Assembly.GetType("LighthouseKeepers.Lobby.LobbyConfig");
            const string configPath="Assets/Data/Layout/EnvironmentLobbyConfig.asset";
            var config=AssetDatabase.LoadAssetAtPath<ScriptableObject>(configPath);
            if(!config){config=field.objectReferenceValue?UnityEngine.Object.Instantiate((ScriptableObject)field.objectReferenceValue):ScriptableObject.CreateInstance(type);AssetDatabase.CreateAsset(config,configPath);}
            var settings=new SerializedObject(config);
            settings.FindProperty("startSceneName").stringValue="LK_Lab_Environment";
            settings.FindProperty("maxPlayers").intValue=2;
            settings.FindProperty("minPlayersToStart").intValue=1;
            settings.FindProperty("requireAllReady").boolValue=false;
            settings.FindProperty("lobbyTitle").stringValue="Lighthouse Keepers — Solo Watch";
            settings.ApplyModifiedPropertiesWithoutUndo();
            // This is a single-player assessment build. The shared Photon lobby is preserved
            // in its own scene; this copied lobby always uses the deterministic local transport.
            foreach(var network in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                .Where(c=>c.GetType().Name=="FusionLauncher")) UnityEngine.Object.DestroyImmediate(network);
            var localType=manager.GetType().Assembly.GetType("LighthouseKeepers.Lobby.LocalLobbyTransport");
            var local=manager.GetComponents<MonoBehaviour>().FirstOrDefault(c=>c&&c.GetType()==localType);
            if(!local)local=manager.gameObject.AddComponent(localType) as MonoBehaviour;
            field.objectReferenceValue=config;
            serialized.FindProperty("transportBehaviour").objectReferenceValue=local;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())EnvironmentDeviceRepair.HideTorso(root);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),LobbyPath);AssetDatabase.SaveAssets();
            Debug.Log("LOBBY_ENVIRONMENT_FLOW_CONFIGURED");
        }
    }
}
