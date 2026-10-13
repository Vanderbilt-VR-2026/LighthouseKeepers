using UnityEditor;
using UnityEditor.SceneManagement;

namespace LighthouseKeepers.Editor
{
    public static class LighthouseNetworkLobby
    {
        public const string ScenePath = "Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity";

        [MenuItem("Lighthouse Keepers/Networking/Play Photon crew lobby")]
        public static void Play()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            EditorApplication.delayCall += EditorApplication.EnterPlaymode;
        }
    }
}
