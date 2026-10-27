using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.Lobby;
using LighthouseKeepers.Player;
using Object = UnityEngine.Object;

namespace LighthouseKeepers.Editor
{
    public static class OneLighthouseM1
    {
        const string Output = "artifacts/one/M1";
        [MenuItem("Lighthouse Keepers/Apply Ship Build Scenes")]
        public static void ApplyShipBuildScenes() => EditorBuildSettings.scenes = LighthouseNetworkLobby.GameScenes
            .Select(path => new EditorBuildSettingsScene(path, true)).ToArray();

        public static void Survey()
        {
            Directory.CreateDirectory(Output);
            var report = new StringBuilder();
            foreach (string path in LighthouseNetworkLobby.GameScenes)
            {
                var scene = EditorSceneManager.OpenScene(path);
                report.AppendLine("SCENE " + path);
                foreach (var root in scene.GetRootGameObjects())
                {
                    report.AppendLine("ROOT " + root.name + " : " + string.Join(",", root.GetComponents<Component>().Where(c=>c).Select(c=>c.GetType().Name)));
                    foreach (var text in root.GetComponentsInChildren<TMPro.TMP_Text>(true)) report.AppendLine("TEXT " + Hierarchy(text.transform) + " : " + text.text);
                    foreach (var text in root.GetComponentsInChildren<TextMesh>(true)) report.AppendLine("TEXTMESH " + Hierarchy(text.transform) + " : " + text.text);
                    foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        var mesh = r.GetComponent<MeshFilter>();
                        var mat = r.sharedMaterial;
                        if (mesh && mesh.sharedMesh && (mesh.sharedMesh.name == "Cube" || (mat && mat.shader.name.Contains("Unlit"))))
                            report.AppendLine("PRIMITIVE " + Hierarchy(r.transform) + " at " + r.transform.position + " scale " + r.transform.lossyScale + " material " + (mat ? mat.name + " / " + mat.shader.name + " / emission " + (mat.HasProperty("_EmissionColor")?mat.GetColor("_EmissionColor").ToString():"none") : "MISSING"));
                    }
                }
            }
            File.WriteAllText(Output + "/survey.txt", report.ToString());
        }
        static string Hierarchy(Transform t) => t.parent ? Hierarchy(t.parent) + "/" + t.name : t.name;

        public static void InstallHands(GameObject rig)
        {
            foreach (var side in new[] { "Left", "Right" })
            {
                var controller = rig.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name == side + " Controller");
                if (!controller) throw new InvalidOperationException("Missing " + side + " Controller in " + rig.name);
                foreach (var legacy in controller.GetComponentsInChildren<Transform>(true).Where(t=>t.name == "LK Stylized Glove").ToArray())
                    if (legacy) Object.DestroyImmediate(legacy.gameObject);
                foreach (var r in controller.GetComponentsInChildren<Renderer>(true))
                    if (!(r is LineRenderer) && !r.GetComponentInParent<HandRig>(true)) r.enabled = false;
                foreach (var direct in controller.GetComponentsInChildren<XRDirectInteractor>(true))
                    if (!direct.GetComponentInParent<HandRig>(true)) direct.enabled = false;
                var hands = controller.GetComponentsInChildren<HandRig>(true);
                for (int i = 1; i < hands.Length; i++) Object.DestroyImmediate(hands[i].gameObject);
                if (hands.Length == 0)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Hands/Resources/Hands/PF_Hand_" + (side == "Left" ? "L" : "R") + ".prefab");
                    PrefabUtility.InstantiatePrefab(prefab, controller);
                }
            }
        }

        static bool RemoveVignette(GameObject root)
        {
            bool changed = false;
            foreach (var provider in root.GetComponentsInChildren<HeadBoundaryComfort>(true)) { Object.DestroyImmediate(provider); changed = true; }
            foreach (var vignette in root.GetComponentsInChildren<TunnelingVignetteController>(true))
            {
                // Keep a prefab root valid while stripping its effect; child instances are removed entirely.
                if (vignette.gameObject == root)
                {
                    foreach (var r in root.GetComponentsInChildren<Renderer>(true)) Object.DestroyImmediate(r);
                    Object.DestroyImmediate(vignette);
                }
                else Object.DestroyImmediate(vignette.gameObject);
                changed = true;
            }
            return changed;
        }
        public static void Apply()
        {
            Directory.CreateDirectory(Output);
            var log = new StringBuilder();
            string vignetteGuid = AssetDatabase.FindAssets("TunnelingVignetteController t:MonoScript").Single(g=>AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(g)).GetClass() == typeof(TunnelingVignetteController));
            string boundaryGuid = AssetDatabase.AssetPathToGUID("Assets/_LighthouseKeepers/Scripts/Runtime/Player/HeadBoundaryComfort.cs");
            // All authored scenes and prefabs, including inactive and nested instances.
            foreach (string path in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath))
            {
                bool rig = path == "Assets/_LighthouseKeepers/Prefabs/Player/LK_QuestPlayer.prefab";
                string serialized = File.ReadAllText(path);
                if (!rig && !serialized.Contains(vignetteGuid) && !serialized.Contains(boundaryGuid)) continue;
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    bool changed = RemoveVignette(root);
                    if (rig) { InstallHands(root); changed = true; }
                    if (changed) { PrefabUtility.SaveAsPrefabAsset(root, path); log.AppendLine("CLEAN PREFAB " + path); }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            foreach (string path in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath))
            {
                bool ship = LighthouseNetworkLobby.GameScenes.Contains(path);
                string serialized = File.ReadAllText(path);
                if (!ship && !serialized.Contains(vignetteGuid) && !serialized.Contains(boundaryGuid)) continue;
                var scene = EditorSceneManager.OpenScene(path);
                bool changed = false;
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (PrefabUtility.IsPartOfPrefabInstance(root) && root.GetComponentsInChildren<TunnelingVignetteController>(true).Length > 0)
                        PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    changed |= RemoveVignette(root);
                    if (!ship) continue;
                    foreach (var origin in root.GetComponentsInChildren<XROrigin>(true)) { InstallHands(origin.gameObject); changed = true; }
                    if (path == LighthouseNetworkLobby.ScenePath) continue;
                    foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        var mesh = renderer.GetComponent<MeshFilter>();
                        var material = renderer.sharedMaterial;
                        bool ceilingCube = renderer.name == "Caged tungsten service lamp";
                        bool placeholder = mesh && mesh.sharedMesh && mesh.sharedMesh.name == "Cube" && material && material.shader.name.Contains("Unlit");
                        if (!ceilingCube && !placeholder) continue;
                        log.AppendLine("REMOVE PRIMITIVE VISUAL " + Hierarchy(renderer.transform));
                        Object.DestroyImmediate(renderer); if (mesh) Object.DestroyImmediate(mesh); changed = true;
                    }
                    foreach (var diagnostic in root.GetComponentsInChildren<LighthouseKeepers.Core.DevelopmentDiagnostics>(true)) { Object.DestroyImmediate(diagnostic); changed = true; }
                    // Prototype Fusion launchers must not create a second runner in offline or hosted play.
                    if (root.GetComponent<Fusion.FusionBootstrap>() || root.GetComponent<Fusion.NetworkRunner>())
                    { log.AppendLine("REMOVE " + path + " / " + root.name); Object.DestroyImmediate(root); changed = true; continue; }
                    foreach (var board in root.GetComponentsInChildren<LighthouseKeepers.StormWatch.StormWatchBoard>(true))
                    { Object.DestroyImmediate(board.gameObject); changed = true; }
                    foreach (var text in root.GetComponentsInChildren<TMPro.TMP_Text>(true))
                    { log.AppendLine("REMOVE TEXT " + Hierarchy(text.transform)); Object.DestroyImmediate(text.gameObject); changed = true; }
                    foreach (var text in root.GetComponentsInChildren<TextMesh>(true))
                    { log.AppendLine("REMOVE TEXT " + Hierarchy(text.transform)); Object.DestroyImmediate(text.gameObject); changed = true; }
                }
                if (changed) { EditorSceneManager.SaveScene(scene); log.AppendLine("CLEAN SCENE " + path); }
            }
            foreach (string path in AssetDatabase.FindAssets("t:VolumeProfile", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath))
            {
                var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
                foreach (var v in profile.components.OfType<Vignette>().ToArray())
                { profile.components.Remove(v); Object.DestroyImmediate(v, true); EditorUtility.SetDirty(profile); log.AppendLine("REMOVE VOLUME VIGNETTE " + path); }
            }
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Hands/Resources/Hands/M_Glove.mat");
            mat.SetColor("_BaseColor", new Color(.62f, .52f, .43f)); EditorUtility.SetDirty(mat);
            var config = AssetDatabase.LoadAssetAtPath<LobbyConfig>("Assets/_LighthouseKeepers/Settings/NetworkLobbyConfig.asset");
            LighthouseScenes.Set(config, "startSceneName", ShipWorld.GameplaySceneName);
            LighthouseScenes.Set(config, "minPlayersToStart", 1);
            foreach (var guid in AssetDatabase.FindAssets("t:ComfortSettings")) EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<ComfortSettings>(AssetDatabase.GUIDToAssetPath(guid)));
            ApplyShipBuildScenes(); AssetDatabase.SaveAssets();
            File.WriteAllText(Output + "/authoring.txt", log.ToString());
            EditorSceneManager.OpenScene(LighthouseNetworkLobby.ScenePath);
        }
        public static void Build()
        {
            ApplyShipBuildScenes();
            Directory.CreateDirectory("Builds");
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = LighthouseNetworkLobby.GameScenes,
                target = BuildTarget.Android, locationPathName = "Builds/LighthouseKeepers-M1.apk", options = BuildOptions.Development });
            File.WriteAllText(Output + "/build.txt", $"{report.summary.result}\nErrors={report.summary.totalErrors}\nWarnings={report.summary.totalWarnings}\nBytes={report.summary.totalSize}\nDuration={report.summary.totalTime}\n");
            if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("M1 Android build failed.");
        }
    }
    public sealed class OneLighthouseShipGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!EditorBuildSettings.scenes.Select(s=>s.path).SequenceEqual(LighthouseNetworkLobby.GameScenes) || EditorBuildSettings.scenes.Any(s=>!s.enabled))
                throw new BuildFailedException("Use Lighthouse Keepers/Apply Ship Build Scenes before building.");
        }
    }
}
