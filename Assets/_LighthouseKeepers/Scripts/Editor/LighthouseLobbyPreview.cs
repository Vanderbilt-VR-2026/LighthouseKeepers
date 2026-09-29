using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using LighthouseKeepers.Lobby;

namespace LighthouseKeepers.Editor
{
    /// <summary>Opt-in lobby preview. Keeps the shared environment entry point unchanged.</summary>
    public static class LighthouseLobbyPreview
    {
        public const string ScenePath = "Assets/_LighthouseKeepers/Scenes/Development/LK_LobbyPreview.unity";

        [MenuItem("Lighthouse Keepers/Play lobby preview")]
        public static void Play()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) CreateScene();
            EditorSceneManager.OpenScene(ScenePath);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            EditorApplication.EnterPlaymode();
        }

        // Also callable in batch mode to reproduce the preview scene without touching authored environments.
        public static void CreateScene()
        {
            if (File.Exists(ScenePath)) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_LighthouseKeepers/Prefabs/Player/LK_QuestPlayer.prefab");
            if (!prefab) throw new System.InvalidOperationException("Missing LK_QuestPlayer prefab.");
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            rig.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Lobby standing platform";
            floor.transform.position = new Vector3(0, -0.1f, 0);
            floor.transform.localScale = new Vector3(20, 0.2f, 20);
            var lobby = new GameObject("Keeper briefing");
            lobby.AddComponent<LobbyManager>();
            lobby.AddComponent<LobbyScreen>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }
    }
}
