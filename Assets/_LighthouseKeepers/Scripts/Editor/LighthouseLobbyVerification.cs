using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;

namespace LighthouseKeepers.Editor
{
    /// <summary>Repeatable rendered lobby evidence plus the actual lobby-to-environment scene transition.</summary>
    [InitializeOnLoad]
    public static class LighthouseLobbyVerification
    {
        const string ActiveKey = "LKLobbyVerification";
        const string Output = "Docs/Verification/Lobby";
        static double began;
        static int phase, frame, errors;
        static RenderTexture target;
        static Camera camera;
        static LobbyScreen screen;
        static LobbyManager manager;
        static string oldStartScene;

        static LighthouseLobbyVerification() => EditorApplication.playModeStateChanged += StateChanged;

        public static void RunBatch()
        {
            LighthouseLobbyPreview.CreateScene();
            EditorSceneManager.OpenScene(LighthouseLobbyPreview.ScenePath);
            oldStartScene = AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene);
            SessionState.SetString("LKLobbyOldStartScene", oldStartScene);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(LighthouseLobbyPreview.ScenePath);
            SessionState.SetBool(ActiveKey, true);
            // Let Unity's startup search-index callback finish before the Play Mode reload.
            EditorApplication.delayCall += EditorApplication.EnterPlaymode;
        }
        static void StateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                began = EditorApplication.timeSinceStartup;
                phase = frame = errors = 0;
                Application.logMessageReceived += Log;
                EditorApplication.update += Tick;
            }
        }
        static void Log(string message, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++;
        }
        static void Click(string name)
        {
            var button = screen.GetComponentsInChildren<Button>().Single(b => b.name == name);
            if (!button.interactable) throw new Exception(name + " is unexpectedly disabled.");
            button.onClick.Invoke();
        }
        static void Tick()
        {
            if (EditorApplication.timeSinceStartup - began > 90) { Finish(false, "Timed out."); return; }
            if (Time.frameCount - frame < 30) return;
            frame = Time.frameCount;
            try
            {
                if (phase == 0)
                {
                    screen = UnityEngine.Object.FindAnyObjectByType<LobbyScreen>();
                    manager = UnityEngine.Object.FindAnyObjectByType<LobbyManager>();
                    camera = Camera.main;
                    if (!screen || !manager || !camera) throw new Exception("Missing lobby objects.");
                    if (!screen.GetComponentsInChildren<Text>().Single(t => t.name == "Mode").text.StartsWith("LOCAL PRACTICE"))
                        throw new Exception("Initial UI does not reflect the local transport.");
                    foreach (var button in screen.GetComponentsInChildren<Button>())
                        if (string.IsNullOrWhiteSpace(button.GetComponentInChildren<Text>().text))
                            throw new Exception("Missing label on " + button.name);
                    target = new RenderTexture(1280, 860, 24);
                    camera.targetTexture = target;
                    var canvas = screen.GetComponentInChildren<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                    canvas.planeDistance = 1;
                    Canvas.ForceUpdateCanvases();
                    phase++;
                }
                else if (phase == 1)
                {
                    Capture("01-offline");
                    Click("OpenPractice");
                    if (manager.CanStart) throw new Exception("Unready crew can start.");
                    phase++;
                }
                else if (phase == 2)
                {
                    Capture("02-not-ready");
                    Click("Ready");
                    if (!manager.CanStart) throw new Exception("Ready solo practice cannot start.");
                    phase++;
                }
                else if (phase == 3)
                {
                    Capture("03-ready");
                    Click("Ready");
                    if (manager.CanStart) throw new Exception("Unready toggle did not revoke start.");
                    Click("Leave");
                    Click("JoinCrew");
                    if (!manager.IsOnline || manager.IsHost || manager.CanStart) throw new Exception("Guest preview rules failed.");
                    phase++;
                }
                else if (phase == 4)
                {
                    Capture("04-guest-preview");
                    Click("Leave");
                    Click("OpenPractice");
                    Click("Ready");
                    camera.targetTexture = null;
                    target.Release();
                    UnityEngine.Object.Destroy(target);
                    Click("Start");
                    phase++;
                }
                else
                {
                    var bootstrap = UnityEngine.Object.FindAnyObjectByType<EnvironmentBootstrap>();
                    if (!bootstrap || !bootstrap.Ready) return;
                    if (SceneManager.sceneCount != 7) throw new Exception("Expected seven environment scenes.");
                    if (UnityEngine.Object.FindAnyObjectByType<LobbyScreen>()) throw new Exception("Lobby leaked into environment.");
                    Finish(errors == 0, "Rendered four lobby states; exercised host, ready/unready, leave, guest preview and start; environment ready with seven scenes. Runtime errors: " + errors);
                }
            }
            catch (Exception e) { Finish(false, e.ToString()); }
        }
        static void Capture(string name)
        {
            Directory.CreateDirectory(Output);
            // Rebuild text at the capture canvas scale, rather than reusing geometry
            // cached for the initial editor Game view size before the camera was assigned.
            foreach (var text in screen.GetComponentsInChildren<Text>()) text.SetAllDirty();
            Canvas.ForceUpdateCanvases();
            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
            File.WriteAllBytes(Output + "/" + name + ".png", image.EncodeToPNG());
            UnityEngine.Object.Destroy(image);
            RenderTexture.active = previous;
        }
        static void Finish(bool success, string message)
        {
            SessionState.SetBool(ActiveKey, false);
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= Log;
            var previous = SessionState.GetString("LKLobbyOldStartScene", "");
            EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previous) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + "/PlayMode.txt", (success ? "PASS: " : "FAIL: ") + message + "\n");
            EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
