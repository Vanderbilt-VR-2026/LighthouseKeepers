using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LighthouseKeepers.Environment.Sprint3.Editor
{
    public static class ShipSceneCatalog
    {
        public const string ManifestPath = "Assets/Scenes/Sprint3/ShipScenes.json";
        public const string ScenePath = "Assets/Scenes/Shell/LK_Shell_House.unity";
        [Serializable] sealed class Manifest { public string[] scenes; }
        public static string[] Paths
        {
            get
            {
                var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
                if (manifest?.scenes == null || manifest.scenes.Length == 0) throw new BuildFailedException("Ship scene manifest is empty.");
                foreach (var path in manifest.scenes)
                    if (!File.Exists(path) || !path.StartsWith("Assets/Scenes/", StringComparison.Ordinal))
                        throw new BuildFailedException("Missing or out-of-lane ship scene: " + path);
                return manifest.scenes;
            }
        }
        public static bool IsShip(Scene scene) => Paths.Contains(scene.path) || scene.GetRootGameObjects().Any(g => g.GetComponentInChildren<ShipScene>(true));
        public static IEnumerable<T> In<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true));
        public static string PathOf(Transform t) => t.parent ? PathOf(t.parent) + "/" + t.name : t.name;
        public static void RunAll(Action<Scene> validate)
        {
            if (Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
                throw new InvalidOperationException("Save open scenes before batch validation.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try { foreach (var path in Paths) validate(EditorSceneManager.OpenScene(path, OpenSceneMode.Single)); }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }
        public static void Report(string name, string scene, int checks, List<string> failures)
        {
            Directory.CreateDirectory("artifacts/sprint3-environment");
            var lines = new[] { "Scene: " + scene, "Checks: " + checks, "Failures: " + failures.Count }.Concat(failures);
            File.WriteAllLines("artifacts/sprint3-environment/" + name + ".txt", lines);
            if (failures.Count > 0) throw new BuildFailedException(name + ":\n" + string.Join("\n", failures));
            Debug.Log(name + " PASS (editor only), checks=" + checks);
        }
    }

    public static class ShipSceneCleanlinessValidator
    {
        static readonly Regex Forbidden = new Regex(@"^(DEBUG_.*|Placeholder.*|GameObject.*|New .*|Temp.*)$", RegexOptions.CultureInvariant);
        static readonly HashSet<long> BuiltInMeshIds = new HashSet<long> { 10202, 10206, 10207, 10208, 10209, 10210 };
        [MenuItem("Lighthouse/Sprint 3/Validate ship cleanliness")]
        public static void Run() => ShipSceneCatalog.RunAll(Check);
        public static void Check(Scene scene)
        {
            var failures = new List<string>(); int checks = 0;
            foreach (var t in ShipSceneCatalog.In<Transform>(scene))
            {
                checks++;
                if (Forbidden.IsMatch(t.name)) failures.Add("Forbidden name " + ShipSceneCatalog.PathOf(t));
            }
            foreach (var renderer in ShipSceneCatalog.In<Renderer>(scene))
            {
                checks++;
                Mesh mesh = renderer is SkinnedMeshRenderer skinned ? skinned.sharedMesh : renderer.GetComponent<MeshFilter>()?.sharedMesh;
                if (!mesh) continue;
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mesh, out string assetGuid, out long id);
                string path = AssetDatabase.GetAssetPath(mesh);
                if ((path == "Library/unity default resources" || assetGuid == "0000000000000000e000000000000000") && BuiltInMeshIds.Contains(id))
                    failures.Add("Built-in mesh " + mesh.name + " at " + ShipSceneCatalog.PathOf(renderer.transform));
                foreach (var material in renderer.sharedMaterials)
                    if (!material || !material.shader || material.shader.name == "Hidden/InternalErrorShader")
                        failures.Add("Missing material/shader at " + ShipSceneCatalog.PathOf(renderer.transform));
            }
            ShipSceneCatalog.Report(nameof(ShipSceneCleanlinessValidator), scene.path, checks, failures);
        }
    }

    public static class GeometryIntersectionValidator
    {
        const float MaximumDepthM = .01f;
        // These meshes ARE the building. Wall/floor/roof joins are construction joints,
        // not furniture penetrating that building. Never exempt an entire ancestor/root.
        static readonly string[] ConstructionPrefixes = { "SM_Wall_", "SM_Floor_", "SM_Roof_", "SM_Ceiling_", "SM_Stair_", "SM_Gallery_Deck", "SM_Cornice_", "SM_Corbel_", "SM_Trim_" };
        static bool Boundary(Collider c) => c.enabled && !c.isTrigger &&
            (c.name.StartsWith("UCX_SM_Wall_", StringComparison.Ordinal) || c.name.StartsWith("UCX_SM_Floor_", StringComparison.Ordinal)
            || c.name.StartsWith("UCX_SM_Gallery_Deck", StringComparison.Ordinal)
            || c.name.StartsWith("WALL_", StringComparison.Ordinal) || c.name.StartsWith("FLOOR_", StringComparison.Ordinal));
        static bool Construction(Renderer r) => ConstructionPrefixes.Any(p => r.name.StartsWith(p, StringComparison.Ordinal));
        [MenuItem("Lighthouse/Sprint 3/Validate geometry intersections")]
        public static void Run() => ShipSceneCatalog.RunAll(Check);
        public static void Check(Scene scene)
        {
            var failures = new List<string>(); int checks = 0;
            Physics.SyncTransforms();
            var boundaries = ShipSceneCatalog.In<Collider>(scene).Where(Boundary).ToArray();
            if (boundaries.Length == 0) throw new BuildFailedException("No wall/floor colliders in " + scene.path + "; refusing an empty pass.");
            var probeObject = new GameObject("Intersection measurement volume") { hideFlags = HideFlags.HideAndDontSave };
            var probe = probeObject.AddComponent<BoxCollider>(); probe.isTrigger = true;
            try
            {
                foreach (var r in ShipSceneCatalog.In<Renderer>(scene))
                {
                    if (Construction(r)) continue;
                    Bounds bounds = r.bounds;
                    if (bounds.size.sqrMagnitude < .00000001f) continue;
                    probe.size = new Vector3(Mathf.Max(bounds.size.x, .0001f), Mathf.Max(bounds.size.y, .0001f), Mathf.Max(bounds.size.z, .0001f));
                    foreach (var boundary in boundaries)
                    {
                        checks++;
                        if (!bounds.Intersects(boundary.bounds)) continue;
                        // Broad AABBs alone falsely fill the interior of curved wall mesh colliders.
                        // A convex bounds-box against the actual boundary mesh supplies the depth.
                        if (Physics.ComputePenetration(probe, bounds.center, Quaternion.identity, boundary, boundary.transform.position,
                                boundary.transform.rotation, out var direction, out float depth) && depth > MaximumDepthM + .00001f)
                            failures.Add($"{ShipSceneCatalog.PathOf(r.transform)} boundsCentre={bounds.center:F3} min={bounds.min:F3} max={bounds.max:F3} into {ShipSceneCatalog.PathOf(boundary.transform)} depth={depth:F4}m escape={direction:F3}");
                    }
                }
                ValidatePump(scene, failures, ref checks);
                ValidateDatums(scene, failures);
            }
            finally { UnityEngine.Object.DestroyImmediate(probeObject); }
            ShipSceneCatalog.Report(nameof(GeometryIntersectionValidator), scene.path, checks, failures);
        }
        static void ValidatePump(Scene scene, List<string> failures, ref int checks)
        {
            var pumps = ShipSceneCatalog.In<Transform>(scene).Where(t => t.name == "STN_MANUAL_PUMP").ToArray();
            if (pumps.Length != 1) { failures.Add("Expected one F1 STN_MANUAL_PUMP; found " + pumps.Length); return; }
            var pump = pumps[0]; var pivot = pump.Find("PIVOT_PumpHandle"); var grip = pivot ? pivot.Find("GRIP_PumpHandle") : null;
            if (!pivot || !grip) { failures.Add("Pump pivot/grip missing"); return; }
            if (Mathf.Abs(pivot.position.y - 1.05f) > .001f || Mathf.Abs(grip.localPosition.magnitude - .25f) > .001f)
                failures.Add("Pump pivot/grip dimensions differ from the published contract.");
            for (int i = 0; i <= 18; i++)
            {
                var p = pivot.position + pivot.rotation * (Quaternion.AngleAxis(-22.5f + i * 2.5f, Vector3.right) * grip.localPosition);
                checks++;
                foreach (var collider in Physics.OverlapSphere(p, .08f, ~0, QueryTriggerInteraction.Ignore))
                    if (!collider.transform.IsChildOf(pump)) failures.Add("Pump hand sweep obstructed by " + collider.name + " at " + p.ToString("F3"));
            }
            var clearCentre = pivot.position + pump.forward * .35f;
            foreach (var collider in Physics.OverlapSphere(clearCentre, .5f, ~0, QueryTriggerInteraction.Ignore))
                if (!collider.transform.IsChildOf(pump)) failures.Add("1m pump swing clearance obstructed by " + collider.name + " at " + clearCentre.ToString("F3"));
        }
        static void ValidateDatums(Scene scene, List<string> failures)
        {
            var gen = ShipSceneCatalog.In<Transform>(scene).SingleOrDefault(t => t.name == "STN_GENERATOR");
            if (!gen || !gen.Find("PLINTH_TOP") || Mathf.Abs(gen.Find("PLINTH_TOP").position.y - 3.75f) > .001f)
                failures.Add("Generator PLINTH_TOP must be world Y=3.750m.");
            var plinth = ShipSceneCatalog.In<Renderer>(scene).SingleOrDefault(t => t.name == "Concrete plinth");
            if (!plinth || Mathf.Abs(plinth.bounds.max.y - 3.75f) > .001f) failures.Add("Rendered concrete plinth top differs from world Y=3.750m.");
            var board = ShipSceneCatalog.In<Renderer>(scene).SingleOrDefault(t => t.name == "White enamel writing surface");
            if (!board || Mathf.Abs(board.bounds.min.y - 1f) > .001f || Mathf.Abs(board.bounds.max.y - 1.8f) > .001f)
                failures.Add("Board surface must span world Y=1.000..1.800m.");
        }
    }

    // IProcessSceneWithReport validates the actual scenes included in BuildPlayer, including
    // temporary build copies. Exceptions are BuildFailedException, never swallowed warnings.
    public sealed class ShipSceneBuildGate : IProcessSceneWithReport
    {
        public int callbackOrder => -90;
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null || !ShipSceneCatalog.IsShip(scene)) return;
            ShipSceneCleanlinessValidator.Check(scene);
            GeometryIntersectionValidator.Check(scene);
            Sprint3LightingBake.RequireCurrentBake();
            LuminanceValidator.RequireCurrentMeasurements();
        }
    }
}
