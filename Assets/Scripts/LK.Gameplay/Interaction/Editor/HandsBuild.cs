using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LighthouseKeepers.Gameplay.Interaction.Editor
{
    public sealed class HandsBuild : IPreprocessBuildWithReport
    {
        const string SettingsPath = "Assets/Prefabs/Hands/Resources/Hands/InteractionSettings.asset";
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report) { BindTuning(); ValidateAssets(); }
        [MenuItem("Lighthouse Keepers/Hands/Bind simulation tuning")]
        public static void BindTuning()
        {
            var settings = AssetDatabase.LoadAssetAtPath<InteractionSettings>(SettingsPath);
            if (!settings) throw new BuildFailedException("Missing Session A interaction settings.");
            var tuning = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Data/Tuning/FloodTuning.asset");
            var serialized = new SerializedObject(settings);
            var property = serialized.FindProperty("floodTuning");
            if (property.objectReferenceValue != tuning)
            { property.objectReferenceValue = tuning; serialized.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(settings); }
            if (tuning)
            {
                var type = tuning.GetType();
                const string linkerPath = "Assets/Scripts/LK.Gameplay/Interaction/link.xml";
                var linker = "<linker>\n  <assembly fullname=\"" + type.Assembly.GetName().Name + "\">\n    <type fullname=\"" + type.FullName + "\" preserve=\"all\" />\n  </assembly>\n</linker>\n";
                if (!File.Exists(linkerPath) || File.ReadAllText(linkerPath) != linker)
                { File.WriteAllText(linkerPath, linker); AssetDatabase.ImportAsset(linkerPath); }
            }
            if (!settings.TryGetMinimumStrokeSeconds(out _))
                Debug.LogWarning("Session B FloodTuning is absent or invalid. Manual-pump strokes remain disabled. This APK cannot satisfy integrated pump acceptance.");
        }
        [MenuItem("Lighthouse Keepers/Hands/Validate authored assets")]
        public static void ValidateAssets()
        {
            foreach (var side in new[] { "L", "R" })
            {
                var path = "Assets/Prefabs/Hands/Resources/Hands/PF_Hand_" + side + ".prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!prefab || !prefab.GetComponent<HandRig>()) throw new BuildFailedException("Invalid hand prefab: " + path);
                var serialized = new SerializedObject(prefab.GetComponent<HandRig>());
                if (!serialized.FindProperty("model").objectReferenceValue || !serialized.FindProperty("gloveMaterial").objectReferenceValue)
                    throw new BuildFailedException("Hand payload/material reference missing: " + path);
                if (prefab.GetComponentsInChildren<Transform>(true).Length != 28) throw new BuildFailedException("Hand must contain wrist/palm + 24 finger joints: " + path);
            }
            foreach (var name in new[] { "grab", "release", "impact", "clunk", "gulp", "detent", "failed_crank", "engine_catch" })
                if (!Resources.Load<AudioClip>("Hands/" + name)) throw new BuildFailedException("Missing positioned feedback clip: " + name);
            if (!Resources.Load<Shader>("Hands/GripRim") || !Resources.Load<Material>("Hands/M_WaterSurge"))
                throw new BuildFailedException("Missing grip rim shader/water material.");
        }
        // Self-contained entry: reads the existing build list; never authors another session's scene/rig.
        public static void BuildQuest()
        {
            var output = Environment.GetEnvironmentVariable("LK_HANDS_APK");
            if (string.IsNullOrEmpty(output)) output = "artifacts/LighthouseKeepers-Hands.apk";
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? "artifacts");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                target = BuildTarget.Android, locationPathName = output, options = BuildOptions.Development
            });
            File.WriteAllText("artifacts/hands-build-result.txt", $"Result={report.summary.result}\nErrors={report.summary.totalErrors}\nAPK={output}\nHeadset=not executed\n");
            if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("Hands Android build failed; inspect Unity log.");
        }
    }
}
