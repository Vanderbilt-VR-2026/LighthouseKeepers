using System;
using System.IO;
using System.Linq;
using LighthouseKeepers.Editor.Environment;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LighthouseKeepers.Environment.Sprint3.Editor
{
    public static class Sprint3EnvironmentAuthoring
    {
        public const string PrefabPath = "Assets/Prefabs/Environment/Sprint3/PF_Sprint3Fixtures.prefab";
        [MenuItem("Lighthouse/Sprint 3/Prepare fixtures and lighting")]
        public static void Prepare()
        {
            EditorSceneManager.OpenScene(ShipSceneCatalog.ScenePath, OpenSceneMode.Single);
            if (!UnityEngine.Object.FindFirstObjectByType<ShipScene>())
                throw new InvalidOperationException("Run generate_assets.py then apply_to_scene.py before the Unity preparation pass.");
            foreach (var board in ShipSceneCatalog.In<BriefingBoardView>(SceneManager.GetActiveScene()))
            {
                board.Prepare();
                board.SetLines("KEEPER WATCH", new (string, bool)[] { ("Take the wrench",false), ("Find the leak",false), ("Seal the pipe",false), ("Restore power",false), ("Regroup upstairs",false) });
            }
            foreach (var label in ShipSceneCatalog.In<FixtureLabel>(SceneManager.GetActiveScene())) label.Prepare();
            foreach (var emitter in ShipSceneCatalog.In<PumpDischargeEmitter>(SceneManager.GetActiveScene())) emitter.Prepare();
            ConfigureLights(); ConfigureGlass(); ConfigureExhaust();
            // New static fixture meshes participate in both independent bakes; moving controls,
            // labels and ticks use probes. Never rebatch a pump handle or a held tool.
            foreach (var renderer in ShipSceneCatalog.In<MeshRenderer>(SceneManager.GetActiveScene()))
            {
                if (!AssetDatabase.GetAssetPath(renderer.GetComponent<MeshFilter>()?.sharedMesh).StartsWith("Assets/Art/Sprint3/", StringComparison.Ordinal)) continue;
                bool moving = renderer.GetComponentInParent<BriefingBoardView>() || Ancestor(renderer.transform, "PIVOT_PumpHandle") || Ancestor(renderer.transform, "Wrench visual") || Ancestor(renderer.transform, "Tick ");
                if (!moving) GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
            }
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
            Debug.Log("SPRINT3_FIXTURES_PREPARED — pending bake, validation and headset acceptance");
        }
        static bool Ancestor(Transform t, string name)
        {
            for (; t; t = t.parent) if (t.name.StartsWith(name, StringComparison.Ordinal)) return true;
            return false;
        }
        static Color Html(string value) { ColorUtility.TryParseHtmlString(value, out var c); return c; }
        static void ConfigureLights()
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var light in ShipSceneCatalog.In<Light>(scene))
            {
                if (light.name.StartsWith("Mains", StringComparison.Ordinal))
                { light.color = Html("#FFB85C"); light.useColorTemperature = true; light.colorTemperature = 2700; }
                if (light.name.StartsWith("Emergency", StringComparison.Ordinal)) light.color = Html("#8B2B22");
            }
            foreach (var t in ShipSceneCatalog.In<Transform>(scene).Where(t => t.name.StartsWith("LIGHT_", StringComparison.Ordinal)).ToArray())
            {
                var l = t.GetComponent<Light>() ?? t.gameObject.AddComponent<Light>();
                l.type = t.name == "LIGHT_BoardEmergencySpot" ? LightType.Spot : LightType.Point;
                l.range = l.type == LightType.Spot ? 3 : 5;
                l.intensity = l.type == LightType.Spot ? 1.8f : 1.5f;
                l.color = Html(l.type == LightType.Spot ? "#FFB85C" : "#8B2B22");
                l.spotAngle = 100; l.innerSpotAngle = 65;
                l.shadows = LightShadows.Hard;
                l.lightmapBakeType = LightmapBakeType.Baked;
                if (l.type == LightType.Spot)
                    t.rotation = Quaternion.LookRotation(t.parent.TransformPoint(new Vector3(0, 1.4f, -.16f)) - t.position);
            }
        }
        static void ConfigureGlass()
        {
            const string path = "Assets/Art/Sprint3/Materials/M_C_PanelGlass.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetFloat("_Surface", 1); material.SetFloat("_Blend", 0);
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0); material.SetFloat("_Smoothness", .7f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.renderQueue = 3000;
                material.color = new Color(.70f,.78f,.76f,.08f); AssetDatabase.CreateAsset(material, path);
            }
            foreach (var pane in ShipSceneCatalog.In<MeshRenderer>(SceneManager.GetActiveScene()).Where(r => r.name == "Glazed inspection pane")) pane.sharedMaterial = material;
        }
        static void ConfigureExhaust()
        {
            var gen = ShipSceneCatalog.In<Transform>(SceneManager.GetActiveScene()).Single(t => t.name == "STN_GENERATOR");
            var elbow = gen.Find("Lagged exhaust wall elbow");
            Vector3 start = gen.TransformPoint(new Vector3(-.72f, 2.03f, .12f));
            Physics.SyncTransforms();
            var hits = Physics.RaycastAll(start, -gen.forward, 1.5f, ~0, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance);
            var wall = hits.FirstOrDefault(h => h.collider.name.StartsWith("UCX_SM_Wall_", StringComparison.Ordinal));
            if (!wall.collider) throw new InvalidOperationException("No wall behind generator exhaust.");
            float length = wall.distance - .003f;
            elbow.position = start - gen.forward * (length / 2);
            elbow.rotation = Quaternion.FromToRotation(Vector3.up, -gen.forward);
            elbow.localScale = new Vector3(1, length / .13f, 1);
            // Interior pipe terminates at the wall face. Exterior continuation starts at the
            // outer wall surface; neither requires a false furniture-penetration exemption.
            var soot = ShipSceneCatalog.In<Transform>(SceneManager.GetActiveScene()).Single(t => t.name == "EXHAUST_ExteriorSoot");
            var outside = start - gen.forward * 1.5f;
            var exterior = Physics.RaycastAll(outside, gen.forward, 1.5f).Where(h => h.collider == wall.collider).OrderBy(h => h.distance).FirstOrDefault();
            if (!exterior.collider) throw new InvalidOperationException("Cannot find exterior exhaust exit.");
            soot.position = exterior.point + exterior.normal * .003f;
            soot.rotation = Quaternion.LookRotation(exterior.normal);
            // An authored service pipe has a dark, flat soot halo; no point/spot fill light.
            var halo = soot.GetComponent<MeshFilter>() ?? soot.gameObject.AddComponent<MeshFilter>();
            halo.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/Sprint3/Meshes/SM_C_Decal.asset");
            var renderer = soot.GetComponent<MeshRenderer>() ?? soot.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Sprint3/Materials/M_C_Soot.mat");
            soot.localScale = new Vector3(.42f,.55f,1);
        }
        public static void BuildAndroid()
        {
            Directory.CreateDirectory("artifacts/sprint3-environment");
            ShipSceneCleanlinessValidator.Run(); GeometryIntersectionValidator.Run();
            // Preserve the frozen route-width/headroom checks as furniture is added.
            EditorSceneManager.OpenScene(ShipSceneCatalog.ScenePath, OpenSceneMode.Single);
            foreach (var controller in ShipSceneCatalog.In<CharacterController>(SceneManager.GetActiveScene())) controller.enabled = false;
            Physics.SyncTransforms();
            var navigation = NavigationClearanceValidator.Check();
            EnvironmentValidation.Save(navigation);
            if (!navigation.passed) throw new UnityEditor.Build.BuildFailedException("Frozen navigation routes are obstructed; see NavigationClearanceValidator.json.");
            EditorSceneManager.OpenScene(ShipSceneCatalog.ScenePath, OpenSceneMode.Single);
            Sprint3LightingBake.RequireCurrentBake(); LuminanceValidator.Run();
            string apk = "artifacts/builds/LighthouseKeepers-Sprint3-Environment.apk";
            Directory.CreateDirectory("artifacts/builds");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { EnvironmentLobbyBuild.LobbyPath, ShipSceneCatalog.ScenePath },
                locationPathName = apk, target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.CompressWithLz4 });
            File.WriteAllText("artifacts/sprint3-environment/android-build.txt", report.summary.result + "\nErrors=" + report.summary.totalErrors + "\nAPK=" + apk);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new UnityEditor.Build.BuildFailedException("Sprint 3 environment APK failed.");
        }
    }
}
