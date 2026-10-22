using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LighthouseKeepers.Simulation.Editor;
using LighthouseKeepers.Simulation.Unity;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LighthouseKeepers.Tests.Simulation
{
    /// <summary>R8 acceptance entry. No direct world load, debug UI, auto-repair or fake second keeper.</summary>
    public sealed class Round2ShipReachabilityTests
    {
        const string SetupKey = "LK.Simulation.Round2.PreviousSceneSetup";
        [Serializable] sealed class SavedSetup { public string[] paths; public bool[] loaded, active; }

        [UnityTest] public IEnumerator LobbyPlaySoloReachesShipAndCapturesPipe()
        {
            Assert.That(Application.isPlaying, Is.False, "Run as an EditMode integration test.");
            Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(GraphicsDeviceType.Null), "Capture needs graphics: omit -nographics.");
            string[] manifest = ShipSceneBuildPolicy.ReadManifest();
            foreach (var scene in Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt))
                Assert.That(scene.isDirty, Is.False, "Save your edited scenes before running ship acceptance.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            SessionState.SetString(SetupKey, JsonUtility.ToJson(new SavedSetup
            {
                paths = setup.Select(s => s.path).ToArray(), loaded = setup.Select(s => s.isLoaded).ToArray(),
                active = setup.Select(s => s.isActive).ToArray()
            }));
            Directory.CreateDirectory("artifacts/round2/B");
            if (File.Exists(ShipSceneBuildPolicy.EvidencePath)) File.Delete(ShipSceneBuildPolicy.EvidencePath);
            if (File.Exists(ShipSceneBuildPolicy.ScreenshotPath)) File.Delete(ShipSceneBuildPolicy.ScreenshotPath);
            EditorSceneManager.OpenScene(manifest[0], OpenSceneMode.Single);
            yield return new EnterPlayMode();

            Button solo = null; double deadline = Time.realtimeSinceStartupAsDouble + 45;
            while (!solo && Time.realtimeSinceStartupAsDouble < deadline)
            {
                solo = Object.FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b =>
                    b.isActiveAndEnabled && b.interactable && Caption(b).Replace(" ", "").IndexOf("PlaySolo", StringComparison.OrdinalIgnoreCase) >= 0);
                if (!solo) yield return null;
            }
            Assert.That(solo, Is.Not.Null, "The actual lobby must expose Play Solo; do not substitute a debug scene load.");
            solo.onClick.Invoke(); // The same registered UI action a player selects.
            var world = manifest.Where(p => p.StartsWith("Assets/Scenes/Shell/", StringComparison.Ordinal) ||
                p.StartsWith("Assets/Scenes/Dress/", StringComparison.Ordinal)).ToArray();
            deadline = Time.realtimeSinceStartupAsDouble + 120;
            while (!world.All(p => SceneManager.GetSceneByPath(p).isLoaded) && Time.realtimeSinceStartupAsDouble < deadline)
                yield return null;
            Assert.That(world.All(p => SceneManager.GetSceneByPath(p).isLoaded), Is.True, "Play Solo did not load every Shell/Dress scene.");
            string[] loaded = Enumerable.Range(0, SceneManager.sceneCount).Select(i => SceneManager.GetSceneAt(i).path).ToArray();
            Assert.That(loaded.Except(manifest, StringComparer.Ordinal), Is.Empty, "Legacy or unlisted scenes are still active.");

            SimulationHostBehaviour host = null; deadline = Time.realtimeSinceStartupAsDouble + 15;
            while (!host && Time.realtimeSinceStartupAsDouble < deadline)
            {
                var hosts = Object.FindObjectsByType<SimulationHostBehaviour>(FindObjectsSortMode.None)
                    .Where(h => h.isActiveAndEnabled && h.IsInitialized && h.Current.DayIndex == 1).ToArray();
                Assert.That(hosts.Length, Is.LessThanOrEqualTo(1), "One host authority must own the shared water.");
                host = hosts.SingleOrDefault(); if (!host) yield return null;
            }
            Assert.That(host, Is.Not.Null, "Ship scenes loaded, but B's initialized Day 1 host is unreachable.");
            Assert.That(host.Current.Phase, Is.EqualTo(LighthouseKeepers.Core.Simulation.RunPhase.Running), "Play Solo must reach a live watch, not a terminal run.");
            var directors = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(m => m.isActiveAndEnabled &&
                m.GetType().GetInterfaces().Any(t => t.FullName == "LighthouseKeepers.Core.Progression.IDayDirector")).ToArray();
            Assert.That(directors.Length, Is.EqualTo(1), "Progression code must be installed in the actual ship route.");
            var director = directors[0]; var contract = director.GetType().GetInterfaces().Single(t =>
                t.FullName == "LighthouseKeepers.Core.Progression.IDayDirector");
            Assert.That(contract.GetProperty("CurrentDay").GetValue(director), Is.EqualTo(1));
            Assert.That(contract.GetProperty("CurrentObjectiveText").GetValue(director), Is.Not.Null.And.Not.EqualTo(""));
            // Use A's own read-only validator; do not repair missing bindings in the test.
            Type binding = AppDomain.CurrentDomain.GetAssemblies().Select(a =>
                a.GetType("LighthouseKeepers.Gameplay.Interaction.StationInteractionBinding", false)).FirstOrDefault(t => t != null);
            Assert.That(binding, Is.Not.Null, "A's physical interactions are absent from the shipped scene route.");
            var validate = binding.GetMethod("CollectMissingShipBindings", BindingFlags.Public | BindingFlags.Static);
            Assert.That(validate, Is.Not.Null, "Use A's Round 2 physical binding validator.");
            var missing = new List<string>(); deadline = Time.realtimeSinceStartupAsDouble + 20;
            do
            {
                validate.Invoke(null, new object[] { missing });
                if (missing.Count == 0) break;
                yield return null;
            } while (Time.realtimeSinceStartupAsDouble < deadline);
            Assert.That(missing, Is.Empty, "Code merged without physical hands/wrench/pump/generator bindings:\n" + string.Join("\n", missing));
            var anchors = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            var pipe = anchors.SingleOrDefault(t => t.name == "STN_PIPE_LEAK");
            Assert.That(pipe, Is.Not.Null, "C must publish the real pipe anchor in the ship scene set.");
            Assert.That(anchors.Count(t => t.name == "STN_BRIEFING_BOARD"), Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<SimulationDebugPanel>(FindObjectsSortMode.None)
                .Any(p => p.GetComponentsInChildren<Canvas>().Length > 0), Is.False, "No B debug panel may appear in the ship experience.");

            CaptureObserverView(pipe, host);
            File.WriteAllText(ShipSceneBuildPolicy.EvidencePath, JsonUtility.ToJson(new ShipSceneBuildPolicy.ReachabilityEvidence
            {
                editorPlayMode = Application.isPlaying, playSoloInvoked = true, anchor = pipe.name,
                screenshot = ShipSceneBuildPolicy.ScreenshotPath, fingerprint = ShipSceneBuildPolicy.Fingerprint(manifest),
                loadedScenes = loaded, dayIndex = host.Current.DayIndex, tick = host.Current.Tick,
                phase = host.Current.Phase.ToString(),
                unityVersion = Application.unityVersion, capturedAtUtc = DateTime.UtcNow.ToString("O")
            }, true));
        }

        static string Caption(Button button)
        {
            var tmp = button.GetComponentInChildren<TMP_Text>();
            if (tmp) return tmp.text;
            var label = button.GetComponentInChildren<Text>();
            return label ? label.text : button.name;
        }

        static void CaptureObserverView(Transform anchor, SimulationHostBehaviour host)
        {
            var observer = new GameObject("B / evidence observer camera");
            var camera = observer.AddComponent<Camera>(); camera.enabled = false;
            camera.stereoTargetEye = StereoTargetEyeMask.None; camera.fieldOfView = 70; camera.nearClipPlane = .05f;
            var inward = -new Vector3(anchor.position.x, 0, anchor.position.z).normalized;
            if (inward.sqrMagnitude == 0) inward = Vector3.forward;
            var eye = anchor.position + inward * 2; eye.y = (float)host.FloorWorldY + 1.65f;
            camera.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(anchor.position - eye));
            var render = new RenderTexture(1280, 900, 24); var pixels = new Texture2D(1280, 900, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                render.Create();
                RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = render });
                RenderTexture.active = render; pixels.ReadPixels(new Rect(0, 0, 1280, 900), 0, 0); pixels.Apply();
                Assert.That(pixels.GetPixels32().Select(c => (c.r << 16) | (c.g << 8) | c.b).Distinct().Take(16).Count(),
                    Is.GreaterThan(1), "The evidence render is blank; do not report it as a ship screenshot.");
                File.WriteAllBytes(ShipSceneBuildPolicy.ScreenshotPath, pixels.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous; render.Release();
                Object.Destroy(render); Object.Destroy(pixels); Object.Destroy(observer);
            }
        }

        [UnityTearDown] public IEnumerator RestoreEditorSceneSetup()
        {
            if (Application.isPlaying) yield return new ExitPlayMode();
            string saved = SessionState.GetString(SetupKey, "");
            if (string.IsNullOrEmpty(saved)) yield break;
            SessionState.EraseString(SetupKey);
            var setup = JsonUtility.FromJson<SavedSetup>(saved);
            if (setup.paths.All(string.IsNullOrEmpty)) EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            else EditorSceneManager.RestoreSceneManagerSetup(setup.paths.Select((path, i) => new SceneSetup
            { path = path, isLoaded = setup.loaded[i], isActive = setup.active[i] }).ToArray());
        }
    }
}
