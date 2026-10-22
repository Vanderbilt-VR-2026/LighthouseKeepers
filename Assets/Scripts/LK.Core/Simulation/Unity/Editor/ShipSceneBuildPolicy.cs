using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LighthouseKeepers.Simulation.Editor
{
    /// <summary>Consumes C's sole ship manifest. Never creates a substitute or edits scenes/build settings.</summary>
    public static class ShipSceneBuildPolicy
    {
        public const string ManifestPath = "Assets/Scenes/Sprint3/ShipScenes.json";
        public const string EvidencePath = "artifacts/round2/B/reachability.json";
        public const string ScreenshotPath = "artifacts/round2/B/STN_PIPE_LEAK.png";
        [Serializable] sealed class Manifest { public string[] scenes; }

        [Serializable] public sealed class ReachabilityEvidence
        {
            public bool editorPlayMode, playSoloInvoked;
            public string anchor, screenshot, fingerprint, unityVersion, capturedAtUtc, phase;
            public string[] loadedScenes;
            public int dayIndex, tick;
        }

        public static string[] Validate(string[] scenes, Func<string, bool> exists)
        {
            var errors = new List<string>();
            if (scenes == null || scenes.Length == 0)
                throw new ArgumentException("ShipScenes.json must contain the canonical scenes array.");
            if (exists == null) throw new ArgumentNullException(nameof(exists));
            var required = new HashSet<string>(StringComparer.Ordinal) { "LK_Lobby_Environment", "LK_NetworkLobby" };
            foreach (string area in new[] { "House", "F1", "F2", "F3", "F4", "Exterior" })
            {
                required.Add("LK_Shell_" + area); required.Add("LK_Dress_" + area);
            }
            if (Path.GetFileNameWithoutExtension(scenes[0]) != "LK_Lobby_Environment")
                errors.Add("Entry scene must be LK_Lobby_Environment, followed by the shared ship route.");
            if (scenes.Length < 2 || Path.GetFileNameWithoutExtension(scenes[1]) != "LK_Shell_House")
                errors.Add("Scene 1 must be LK_Shell_House; NetworkLobby is a retained dependency, not the Play Solo destination.");
            var paths = new HashSet<string>(StringComparer.Ordinal);
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (string path in scenes)
            {
                if (string.IsNullOrWhiteSpace(path)) { errors.Add("Empty scene path."); continue; }
                string name = Path.GetFileNameWithoutExtension(path);
                if (!paths.Add(path) || !names.Add(name)) errors.Add("Duplicate scene: " + path);
                if (!path.StartsWith("Assets/", StringComparison.Ordinal) || path.Contains("..") ||
                    !path.EndsWith(".unity", StringComparison.Ordinal) || !exists(path)) errors.Add("Missing/invalid scene path: " + path);
                if (!required.Contains(name)) errors.Add("Non-ship scene is forbidden: " + path);
                if (name.StartsWith("LK_Shell_", StringComparison.Ordinal) && !path.StartsWith("Assets/Scenes/Shell/", StringComparison.Ordinal))
                    errors.Add("Shell scene must use the lead's Shell folder: " + path);
                if (name.StartsWith("LK_Dress_", StringComparison.Ordinal) && !path.StartsWith("Assets/Scenes/Dress/", StringComparison.Ordinal))
                    errors.Add("Dress scene must use the lead's Dress folder: " + path);
            }
            foreach (string name in required)
                if (!names.Contains(name)) errors.Add("Missing required ship scene: " + name);
            if (errors.Count > 0) throw new ArgumentException(string.Join("\n", errors));
            return (string[])scenes.Clone();
        }

        public static string[] ReadManifest()
        {
            if (!File.Exists(ManifestPath))
                throw new BuildFailedException("Ship build blocked: C's " + ManifestPath + " is missing. See Docs/Sprint3/Requests/B-to-C.md.");
            try
            {
                var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
                return Validate(manifest?.scenes, File.Exists);
            }
            catch (Exception error) { throw new BuildFailedException("Ship manifest rejected: " + error.Message); }
        }

        public static void ValidateBuildSettings(string[] manifest)
        {
            var enabled = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (!enabled.SequenceEqual(manifest, StringComparer.Ordinal))
                throw new BuildFailedException("Build Settings must exactly match ShipScenes.json. Refusing to package Storm Watch/Lab/DevGym/Level01-04 as the ship game.");
        }

        public static string Fingerprint(string[] manifest)
        {
            var input = new StringBuilder(File.ReadAllText(ManifestPath));
            input.Append(EditorUserBuildSettings.activeBuildTarget);
            foreach (string path in manifest) input.Append(path).Append(AssetDatabase.GetAssetDependencyHash(path));
            foreach (string path in Directory.EnumerateFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
                input.Append(path).Append(File.ReadAllText(path));
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(input.ToString()))).Replace("-", "");
        }

        public static void ValidateReachability(string[] manifest)
        {
            if (!File.Exists(EvidencePath))
                throw new BuildFailedException("R8: run LobbyPlaySoloReachesShipAndCapturesPipe before building. Missing " + EvidencePath);
            var proof = JsonUtility.FromJson<ReachabilityEvidence>(File.ReadAllText(EvidencePath));
            var world = manifest.Where(p => p.StartsWith("Assets/Scenes/Shell/", StringComparison.Ordinal) ||
                p.StartsWith("Assets/Scenes/Dress/", StringComparison.Ordinal));
            if (proof == null || !proof.editorPlayMode || !proof.playSoloInvoked || proof.anchor != "STN_PIPE_LEAK" ||
                proof.dayIndex != 1 || proof.phase != "Running" || proof.tick <= 0 || proof.screenshot != ScreenshotPath ||
                !File.Exists(ScreenshotPath) || new FileInfo(ScreenshotPath).Length == 0 ||
                proof.fingerprint != Fingerprint(manifest) || proof.loadedScenes == null ||
                world.Except(proof.loadedScenes, StringComparer.Ordinal).Any() ||
                proof.loadedScenes.Except(manifest, StringComparer.Ordinal).Any())
                throw new BuildFailedException("R8: Lobby -> Play Solo evidence is missing, stale or from a different scene set/build target. Rerun the ship reachability test with Android selected.");
        }
    }

    /// <summary>Applies to every player build, including legacy menu builders.</summary>
    public sealed class ShipSimulationBuildGate : IPreprocessBuildWithReport, IProcessSceneWithReport
    {
        public int callbackOrder => -10000;
        public void OnPreprocessBuild(BuildReport report)
        {
            var manifest = ShipSceneBuildPolicy.ReadManifest();
            ShipSceneBuildPolicy.ValidateBuildSettings(manifest);
            ShipSceneBuildPolicy.ValidateReachability(manifest);
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null) return; // Editor Play Mode is not a player build.
            var paths = ShipSceneBuildPolicy.ReadManifest();
            if (!paths.Contains(scene.path, StringComparer.Ordinal))
                throw new BuildFailedException("Build attempted an unlisted scene: " + scene.path);
            foreach (var root in scene.GetRootGameObjects())
                if (root.GetComponentsInChildren<LighthouseKeepers.Simulation.Unity.SimulationDebugPanel>(true).Length > 0)
                    throw new BuildFailedException("Remove B's diagnostic panel component from ship scene " + scene.path + "; the house KEEPER WATCH board is the only task UI.");
        }
    }
}
