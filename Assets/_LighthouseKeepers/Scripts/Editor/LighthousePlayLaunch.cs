using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using LighthouseKeepers.Core;

namespace LighthouseKeepers.Editor
{
    [InitializeOnLoad]
    public static class LighthousePlayLaunch
    {
        static double started;
        static int errors;
        static LighthousePlayLaunch() { EditorApplication.playModeStateChanged += StateChanged; }

        [MenuItem("Lighthouse Keepers/Play environment")]
        public static void Launch()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var path = LighthouseScenes.ScenePath("LK_Bootstrap");
            EditorSceneManager.OpenScene(path);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
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
            bool ready = bootstrap && bootstrap.Ready && SceneManager.sceneCount == 7 && errors == 0;
            Directory.CreateDirectory("Docs/Verification");
            var result = (ready ? "PASS" : "FAIL") + ": interactive Play Mode launch; scenes=" + SceneManager.sceneCount + "; ready=" + (bootstrap && bootstrap.Ready) + "; runtime errors=" + errors + ". Game left running for the user.";
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
