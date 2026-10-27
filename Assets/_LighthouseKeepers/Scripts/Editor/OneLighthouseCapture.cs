using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Core;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.Player;
using Object = UnityEngine.Object;

namespace LighthouseKeepers.Editor
{
    [InitializeOnLoad]
    public static class OneLighthouseCapture
    {
        const string Key = "OneLighthouseCapture", Output = "artifacts/one/M1";
        static int phase, frame;
        static double began;
        static Camera preview;
        static readonly List<string> errors = new();
        static readonly Vector3[] Positions = { new(1.8f,1.65f,-2.25f), new(1.6f,4.85f,-2.2f), new(2.4f,8.05f,1), new(3.5f,11.25f,1.5f) };
        static readonly Vector3[] Targets = { new(-.7f,1.35f,-3.7f), new(-.4f,4.5f,-3.6f), new(-1,7.9f,-3), new(0,11.25f,0) };
        static OneLighthouseCapture() => EditorApplication.playModeStateChanged += State;
        public static void Run()
        {
            Directory.CreateDirectory(Output);
            SessionState.SetString(Key + "OldStart", AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(LighthouseNetworkLobby.ScenePath);
            EditorSceneManager.OpenScene(LighthouseNetworkLobby.ScenePath);
            SessionState.SetBool(Key, true);
            EditorApplication.delayCall += EditorApplication.EnterPlaymode;
        }
        static void State(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key, false) || state != PlayModeStateChange.EnteredPlayMode) return;
            phase = frame = 0; began = EditorApplication.timeSinceStartup; errors.Clear();
            Application.targetFrameRate = 72;
            Application.logMessageReceived += Log;
            EditorApplication.update += Tick;
        }
        static void Log(string message, string trace, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + trace); }
        static void Tick()
        {
            if (EditorApplication.timeSinceStartup - began > 180) { Finish("Timeout"); return; }
            if (Time.frameCount - frame < 90) return;
            frame = Time.frameCount;
            try
            {
                if (phase == 0)
                {
                    if (!GameObject.Find("PlaySolo")) return;
                    Capture(Camera.main, "01-lobby");
                    GameObject.Find("PlaySolo").GetComponent<Button>().onClick.Invoke(); phase++; return;
                }
                var bootstrap = Object.FindAnyObjectByType<EnvironmentBootstrap>();
                if (!bootstrap || !bootstrap.Ready) return;
                if (phase == 1)
                {
                    if (SceneManager.sceneCount != 7) throw new Exception("Expected seven world scenes.");
                    if (Object.FindObjectsByType<XROrigin>(FindObjectsSortMode.None).Length != 1) throw new Exception("Duplicate local rig.");
                    if (Object.FindObjectsByType<HandRig>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length != 2) throw new Exception("Expected two Sprint 3 hands.");
                    if (Object.FindObjectsByType<TunnelingVignetteController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length != 0) throw new Exception("Vignette remains.");
                    if (Object.FindAnyObjectByType<MultiplayerPanel>()) throw new Exception("Floating crew panel remains.");
                    Capture(Camera.main, "02-spawn");
                    var rig = Object.FindAnyObjectByType<XROrigin>();
                    foreach (var modality in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager>(true)) modality.enabled = false;
                    foreach (var hand in rig.GetComponentsInChildren<HandRig>(true))
                    {
                        // Desktop evidence only: position the actual tracked-controller parents in front of the player camera.
                        var controller = hand.transform.parent;
                        controller.gameObject.SetActive(true);
                        foreach (var tracker in controller.GetComponents<UnityEngine.InputSystem.XR.TrackedPoseDriver>()) tracker.enabled = false;
                        bool left = hand.Node == UnityEngine.XR.XRNode.LeftHand;
                        controller.SetPositionAndRotation(Camera.main.transform.TransformPoint(new Vector3(left ? -.15f : .15f, -.13f, .38f)), Camera.main.transform.rotation);
                    }
                    phase++; return;
                }
                if (phase == 2)
                {
                    var handEvidence = Object.FindAnyObjectByType<XROrigin>().GetComponentsInChildren<HandRig>(true);
                    File.WriteAllLines(Output + "/hands.txt", handEvidence.Select(h=>h.name + " active=" + h.gameObject.activeInHierarchy + " position=" + h.transform.position + " renderers=" + string.Join(",", h.GetComponentsInChildren<Renderer>(true).Select(r=>r.name + ":" + r.enabled + ":" + r.bounds.ToString()))));
                    foreach (var h in handEvidence)
                        if (!h.gameObject.activeInHierarchy || !h.GetComponentsInChildren<SkinnedMeshRenderer>().Any(r=>r.enabled)) throw new Exception("Hand skin not visible after desktop pose activation.");
                    Capture(Camera.main, "03-hands-desktop-poses");
                    preview = new GameObject("M1 eye-height evidence camera").AddComponent<Camera>();
                    preview.CopyFrom(Camera.main); preview.tag = "Untagged"; preview.fieldOfView = 75;
                    foreach (var renderer in Object.FindAnyObjectByType<XROrigin>().GetComponentsInChildren<Renderer>(true)) renderer.forceRenderingOff = true;
                    preview.transform.position = Positions[0]; preview.transform.LookAt(Targets[0]);
                    phase++; return;
                }
                int floor = phase - 3;
                Capture(preview, "0" + (4 + floor) + "-floor-" + (floor + 1));
                if (floor == 3) { Finish(null); return; }
                preview.transform.position = Positions[floor + 1]; preview.transform.LookAt(Targets[floor + 1]); phase++;
            }
            catch (Exception error) { Finish(error.ToString()); }
        }
        static void Capture(Camera camera, string name)
        {
            var old = camera.targetTexture; var active = RenderTexture.active;
            var target = new RenderTexture(1600, 1000, 24);
            camera.targetTexture = target; Canvas.ForceUpdateCanvases(); camera.Render();
            RenderTexture.active = target;
            var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); texture.Apply();
            File.WriteAllBytes(Output + "/" + name + ".png", texture.EncodeToPNG());
            camera.targetTexture = old; RenderTexture.active = active;
            Object.Destroy(texture); target.Release(); Object.Destroy(target);
        }
        static void Finish(string failure)
        {
            if (failure != null) errors.Add(failure);
            SessionState.SetBool(Key, false); EditorApplication.update -= Tick; Application.logMessageReceived -= Log;
            string previous = SessionState.GetString(Key + "OldStart", "");
            EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previous) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);
            File.WriteAllText(Output + "/graphics.txt", errors.Count == 0 ? "PASS: graphics Play Mode from actual lobby PLAY SOLO to seven-scene Bootstrap, one local rig, two Sprint 3 hands, no vignette or floating crew panel. Seven rendered images. Hands use injected desktop controller poses; Quest wrist orientation/comfort are not verified.\n" : string.Join("\n", errors));
            EditorApplication.Exit(errors.Count == 0 ? 0 : 1);
        }
    }
}
