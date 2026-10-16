using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;

namespace LighthouseKeepers.Editor
{
    [InitializeOnLoad]
    public static class LighthousePlayLaunch
    {
        static double started;
        static int errors;
        static LighthousePlayLaunch()
        {
            EditorApplication.playModeStateChanged += StateChanged;
            EditorApplication.delayCall += ConfigureDefaultStartup;
        }
        static void ConfigureDefaultStartup()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool("LKDevelopmentEntry", false)) return;
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(LighthouseNetworkLobby.ScenePath);
        }
        [MenuItem("Lighthouse Keepers/Play game")]
        public static void PlayGame()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(LighthouseNetworkLobby.ScenePath);
            SessionState.SetBool("LKDevelopmentEntry", false);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(LighthouseNetworkLobby.ScenePath);
            SessionState.SetBool("LKUserLaunch", true);
            EditorApplication.delayCall += EditorApplication.EnterPlaymode;
        }

        [MenuItem("Lighthouse Keepers/Development/Play environment")]
        public static void Launch()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var path = LighthouseScenes.ScenePath("LK_Bootstrap");
            EditorSceneManager.OpenScene(path);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            SessionState.SetBool("LKDevelopmentEntry", true);
            SessionState.SetBool("LKUserLaunch", true);
            EditorApplication.delayCall += EditorApplication.EnterPlaymode;
        }

        static void StateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool("LKUserLaunch", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                started = EditorApplication.timeSinceStartup;
                errors = 0;
                Application.logMessageReceived += RecordError;
                EditorApplication.update += CheckLaunch;
                EditorApplication.ExecuteMenuItem("Window/General/Game");
            }
            else if (state == PlayModeStateChange.ExitingPlayMode) StopMonitoring();
        }

        static void RecordError(string message, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++;
        }

        static void CheckLaunch()
        {
            if (EditorApplication.timeSinceStartup - started < 15) return;
            var bootstrap = Object.FindAnyObjectByType<EnvironmentBootstrap>();
            var lobby = Object.FindAnyObjectByType<LobbyManager>();
            bool ready = errors == 0 && (SessionState.GetBool("LKDevelopmentEntry", false)
                ? bootstrap && bootstrap.Ready && SceneManager.sceneCount == 7
                : (lobby && !lobby.IsLocalPreview && lobby.Transport != null) ||
                  (Object.FindAnyObjectByType<LighthouseKeepers.StormWatch.StormWatchDirector>()?.Ready == true));
            Directory.CreateDirectory("Docs/Verification");
            var result = (ready ? "PASS" : "FAIL") + ": interactive Play Mode launch; scenes=" + SceneManager.sceneCount + "; ready=" + ready + "; runtime errors=" + errors + ". Game left running for the user.";
            File.WriteAllText("Docs/Verification/InteractiveLaunch.txt", result + "\n");
            Debug.Log("LK INTERACTIVE LAUNCH: " + result);
            StopMonitoring();
        }

        static void StopMonitoring()
        {
            SessionState.SetBool("LKUserLaunch", false);
            Application.logMessageReceived -= RecordError;
            EditorApplication.update -= CheckLaunch;
        }
    }
}
