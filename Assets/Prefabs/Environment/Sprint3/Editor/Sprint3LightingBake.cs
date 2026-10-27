using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using LighthouseKeepers.Editor.Environment;
using LighthouseKeepers.Presentation.Environment;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace LighthouseKeepers.Environment.Sprint3.Editor
{
    public static class Sprint3LightingBake
    {
        public const string LightingRoot = "Assets/Settings/Lighting/Sprint3/";
        public const string Stamp = LightingRoot + "BakeFingerprint.txt";
        static readonly string[] MainsNames = { "M_Environment_Lamp", "M_Environment_Lamp_Half", "M_C_Warm", "M_C_Green" };
        static bool IsMains(Material m) => m && (MainsNames.Contains(m.name) || m.name.StartsWith("M_C_Mains_", StringComparison.Ordinal));
        public static string Fingerprint()
        {
            var paths = AssetDatabase.GetDependencies(ShipSceneCatalog.Paths, true)
                .Concat(Directory.GetFiles("Assets/Prefabs/Environment/Sprint3", "*.cs", SearchOption.AllDirectories))
                .Concat(new[] { "Docs/StationContract.json", "Assets/Settings/Lighting/LuminanceCalibration.json" })
                .Where(File.Exists).SelectMany(p => File.Exists(p + ".meta") ? new[] { p, p + ".meta" } : new[] { p })
                .Distinct().OrderBy(p => p, StringComparer.Ordinal);
            using (var hash = SHA256.Create())
            using (var buffer = new MemoryStream())
            {
                foreach (var path in paths)
                {
                    var name = System.Text.Encoding.UTF8.GetBytes(path); buffer.Write(name, 0, name.Length);
                    var bytes = File.ReadAllBytes(path); var digest = hash.ComputeHash(bytes); buffer.Write(digest, 0, digest.Length);
                }
                return BitConverter.ToString(hash.ComputeHash(buffer.ToArray())).Replace("-", "");
            }
        }
        public static void RequireCurrentBake()
        {
            if (!File.Exists(Stamp) || File.ReadAllText(Stamp).Trim() != Fingerprint())
                throw new BuildFailedException("Sprint 3 requires two fresh bakes for these scene/material sources. Run Sprint3LightingBake.Bake; stale blue/Half presets are not valid red-fault evidence.");
            RequireSharedAtlas(AssetDatabase.LoadAssetAtPath<EnvironmentLightingPreset>(LightingRoot + "LightingPreset_Powered.asset"),
                AssetDatabase.LoadAssetAtPath<EnvironmentLightingPreset>(LightingRoot + "LightingPreset_Fault.asset"));
            foreach (string state in new[] { "Powered", "Fault" })
            {
                var preset = AssetDatabase.LoadAssetAtPath<EnvironmentLightingPreset>(LightingRoot + "LightingPreset_" + state + ".asset");
                if (!preset || preset.colorMaps == null || preset.colorMaps.Length == 0 || preset.colorMaps.Any(m => !m))
                    throw new BuildFailedException("Missing complete baked lighting preset: " + state);
            }
        }
        static void RequireSharedAtlas(EnvironmentLightingPreset powered, EnvironmentLightingPreset fault)
        {
            if (!powered || !fault || powered.rendererIndices == null || fault.rendererIndices == null || !powered.rendererIndices.SequenceEqual(fault.rendererIndices) || !powered.rendererScaleOffsets.SequenceEqual(fault.rendererScaleOffsets)
                || powered.mode != fault.mode || powered.colorMaps.Length != fault.colorMaps.Length)
                throw new BuildFailedException("Powered/fault bakes do not share atlas coordinates; static-batched renderers cannot swap these sets safely.");
        }
        [MenuItem("Lighthouse/Sprint 3/Bake powered and red fault")]
        public static void Bake()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) throw new BuildFailedException("Baking requires a graphics-enabled Editor. Omit -nographics.");
            Sprint3EnvironmentAuthoring.Prepare();
            var scene = SceneManager.GetActiveScene();
            var group = ShipSceneCatalog.In<BeaconAndMainsLightingGroup>(scene).Single(); group.enabled = false;
            var renderers = ShipSceneCatalog.In<MeshRenderer>(scene).Where(r => r.gameObject.isStatic).OrderBy(r => EnvironmentLightingBake.PathOf(r.transform), StringComparer.Ordinal).ToArray();
            var lights = ShipSceneCatalog.In<Light>(scene).ToArray();
            // Oil is a separate material instance so blacking mains emission cannot extinguish it.
            foreach (var oil in ShipSceneCatalog.In<MeshRenderer>(scene).Where(r => r.name == "SM_Lamp_House_N24"))
            {
                var materials = oil.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (!materials[i] || !MainsNames.Contains(materials[i].name)) continue;
                    const string oilPath = LightingRoot + "OilLampIndependent.mat";
                    var independent = AssetDatabase.LoadAssetAtPath<Material>(oilPath);
                    if (!independent) { independent = new Material(materials[i]) { name = "OilLampIndependent" }; AssetDatabase.CreateAsset(independent, oilPath); }
                    materials[i] = independent;
                }
                oil.sharedMaterials = materials;
            }
            // Replace the old blue lens material on this scene instance only.
            var red = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Sprint3/Materials/M_C_Emergency.mat");
            foreach (var r in ShipSceneCatalog.In<MeshRenderer>(scene))
            {
                var materials = r.sharedMaterials; bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                    if (materials[i] && materials[i].name.StartsWith("M_Environment_Emergency", StringComparison.Ordinal)) { materials[i] = red; changed = true; }
                if (changed) r.sharedMaterials = materials;
            }
            // Clone mains materials into the owned lighting folder before toggling emission.
            // The earlier preview's materials under Assets/WatchDemo belong to another lane.
            foreach (var r in ShipSceneCatalog.In<MeshRenderer>(scene))
            {
                var mats = r.sharedMaterials; bool changed = false;
                for (int i=0;i<mats.Length;i++)
                {
                    if (!IsMains(mats[i]) || AssetDatabase.GetAssetPath(mats[i]).StartsWith(LightingRoot, StringComparison.Ordinal)) continue;
                    string ownedPath = LightingRoot + "M_C_Mains_" + mats[i].name + ".mat";
                    var owned = AssetDatabase.LoadAssetAtPath<Material>(ownedPath);
                    if (!owned) { owned = new Material(mats[i]) { name = "M_C_Mains_" + mats[i].name }; AssetDatabase.CreateAsset(owned, ownedPath); }
                    mats[i] = owned; changed = true;
                }
                if (changed) r.sharedMaterials = mats;
            }
            var mains = ShipSceneCatalog.In<MeshRenderer>(scene).Where(r => r.sharedMaterials.Any(IsMains)).ToArray();
            var materialsToSwitch = mains.SelectMany(r => r.sharedMaterials).Where(IsMains).Distinct().ToArray();
            var emission = materialsToSwitch.Select(m => m.GetColor("_EmissionColor")).ToArray();
            var settings = new LightingSettings { name = "Sprint 3 dual-state lighting", bakedGI = true, realtimeGI = false,
                lightmapper = LightingSettings.Lightmapper.ProgressiveCPU, lightmapResolution = 20, lightmapMaxSize = 1024,
                directSampleCount = 256, indirectSampleCount = 512, environmentSampleCount = 256 };
            string settingsPath = LightingRoot + "Sprint3.lighting";
            var existing = AssetDatabase.LoadAssetAtPath<LightingSettings>(settingsPath);
            if (existing) { EditorUtility.CopySerialized(settings, existing); UnityEngine.Object.DestroyImmediate(settings); settings = existing; }
            else AssetDatabase.CreateAsset(settings, settingsPath);
            Lightmapping.lightingSettings = settings;
            EnvironmentLightingPreset powered = null, fault = null;
            try
            {
                foreach (bool on in new[] { true, false })
                {
                    for (int i = 0; i < materialsToSwitch.Length; i++) materialsToSwitch[i].SetColor("_EmissionColor", on ? emission[i] : Color.black);
                    foreach (var l in lights)
                    {
                        bool emergency = l.name.StartsWith("Emergency", StringComparison.Ordinal) || l.name.StartsWith("LIGHT_", StringComparison.Ordinal);
                        bool independent = l.name.StartsWith("Oil", StringComparison.Ordinal) || l.name == "LIGHT_BoardEmergencySpot";
                        l.enabled = independent || (emergency ? !on : on);
                    }
                    RenderSettings.ambientMode = AmbientMode.Flat;
                    // Nighttime remains nighttime; visibility comes from authored fixtures.
                    RenderSettings.ambientLight = on ? new Color(.08f,.07f,.06f) : new Color(.006f,.002f,.002f);
                    EditorSceneManager.SaveScene(scene, LightingRoot + "LK_Sprint3_Bake.unity");
                    if (!Lightmapping.Bake()) throw new BuildFailedException("Independent " + (on ? "powered" : "fault") + " bake failed.");
                    var preset = Capture(on ? "Powered" : "Fault", renderers);
                    if (on) powered = preset; else fault = preset;
                }
                RequireSharedAtlas(powered, fault);
                var serialized = new SerializedObject(group);
                serialized.FindProperty("poweredPreset").objectReferenceValue = powered;
                serialized.FindProperty("emergencyPreset").objectReferenceValue = fault;
                SetArray(serialized.FindProperty("bakedRenderers"), renderers);
                SetArray(serialized.FindProperty("mainsEmitters"), mains);
                SetArray(serialized.FindProperty("poweredRealtimeLights"), lights.Where(l => l.lightmapBakeType != LightmapBakeType.Baked && !l.name.StartsWith("Oil", StringComparison.Ordinal) && !l.name.StartsWith("Emergency", StringComparison.Ordinal) && !l.name.StartsWith("LIGHT_", StringComparison.Ordinal)).ToArray());
                serialized.FindProperty("powered").boolValue = true;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EnvironmentLightingBake.Apply(powered, renderers);
                group.enabled = true;
                EditorSceneManager.SaveScene(scene, ShipSceneCatalog.ScenePath); AssetDatabase.SaveAssets();
            }
            finally
            {
                for (int i = 0; i < materialsToSwitch.Length; i++) { materialsToSwitch[i].SetColor("_EmissionColor", emission[i]); EditorUtility.SetDirty(materialsToSwitch[i]); }
                AssetDatabase.SaveAssets();
            }
            File.WriteAllText(Stamp, Fingerprint() + "\n"); AssetDatabase.ImportAsset(Stamp);
            Debug.Log("SPRINT3_DUAL_BAKE_READY — luminance and headset checks still required");
        }
        static void SetArray(SerializedProperty property, UnityEngine.Object[] values)
        { property.arraySize = values.Length; for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; }
        static EnvironmentLightingPreset Capture(string state, MeshRenderer[] renderers)
        {
            string dir = LightingRoot + state; Directory.CreateDirectory(dir); AssetDatabase.Refresh();
            string path = LightingRoot + "LightingPreset_" + state + ".asset";
            var p = AssetDatabase.LoadAssetAtPath<EnvironmentLightingPreset>(path);
            if (!p) { p = ScriptableObject.CreateInstance<EnvironmentLightingPreset>(); AssetDatabase.CreateAsset(p, path); }
            var maps = LightmapSettings.lightmaps;
            if (maps.Length == 0) throw new BuildFailedException("Bake produced no lightmaps: " + state);
            p.colorMaps = maps.Select((m,i) => Copy(m.lightmapColor, dir, i, "Color")).ToArray();
            p.directionMaps = maps.Select((m,i) => Copy(m.lightmapDir, dir, i, "Direction")).ToArray();
            p.shadowMasks = maps.Select((m,i) => Copy(m.shadowMask, dir, i, "Shadow")).ToArray();
            p.rendererIndices = renderers.Select(r => r.lightmapIndex).ToArray();
            p.rendererScaleOffsets = renderers.Select(r => r.lightmapScaleOffset).ToArray();
            p.mode = LightmapSettings.lightmapsMode; p.ambient = RenderSettings.ambientLight;
            var probes = LightmapSettings.lightProbes ? LightmapSettings.lightProbes.bakedProbes : Array.Empty<SphericalHarmonicsL2>();
            p.probeCoefficients = new float[probes.Length * 27];
            for (int i=0;i<probes.Length;i++) for (int c=0;c<3;c++) for (int k=0;k<9;k++) p.probeCoefficients[i*27+c*9+k] = probes[i][c,k];
            EditorUtility.SetDirty(p); AssetDatabase.SaveAssets(); return p;
        }
        static Texture2D Copy(Texture2D source, string dir, int index, string kind)
        {
            if (!source) return null;
            string from = AssetDatabase.GetAssetPath(source), to = dir + "/" + index + "-" + kind + Path.GetExtension(from);
            if (File.Exists(to)) { File.Copy(from, to, true); AssetDatabase.ImportAsset(to, ImportAssetOptions.ForceUpdate); }
            else if (!AssetDatabase.CopyAsset(from, to)) throw new BuildFailedException("Cannot preserve baked map " + from);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(to);
        }
    }
}
