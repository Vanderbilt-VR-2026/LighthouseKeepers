using System;
using System.Linq;
using LighthouseKeepers.Puzzles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using LighthouseKeepers.Environment;
using LighthouseKeepers.Flood;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LighthouseKeepers.Editor
{
    /// <summary>Opt-in legacy environment authoring. Does not alter Storm Watch or socket identities.</summary>
    public static class LegacyInteractionAuthoring
    {
        static readonly string[] LevelNames = { "LK_Level01_Plumbing", "LK_Level02_Generator", "LK_Level03_Communications", "LK_Level04_Lantern" };
        static string PathFor(string name) => "Assets/_LighthouseKeepers/Scenes/Levels/" + name + ".unity";

        /// <summary>Batch executeMethod: wire and save only existing legacy levels, restoring the initial setup.</summary>
        public static void BindAndSaveLegacyScenes()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Bind legacy controls outside Play Mode.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save or discard open scene edits before running legacy interaction batch authoring.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string name in LevelNames)
                {
                    string path = PathFor(name);
                    if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(path)) throw new InvalidOperationException("Missing existing legacy scene: " + path);
                    var scene = SceneManager.GetSceneByPath(path);
                    if (!scene.IsValid() || !scene.isLoaded) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    string socketsBefore = SocketSignature(scene);
                    BindScene(scene);
                    ValidateBindings(scene);
                    if (socketsBefore != SocketSignature(scene)) throw new InvalidOperationException("Legacy socket identity/anchor changed in " + name);
                    // Save only this level's combined meshes. SaveMesh updates existing assets in place,
                    // preserving their GUIDs; no unrelated dirty assets are flushed.
                    foreach (var root in scene.GetRootGameObjects().Where(g => g.name == "Static render batches"))
                        foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
                            if (filter.sharedMesh && AssetDatabase.Contains(filter.sharedMesh)) AssetDatabase.SaveAssetIfDirty(filter.sharedMesh);
                    if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save authored legacy scene: " + path);
                }
                Debug.Log("LEGACY BASIC INTERACTIONS: saved four existing levels; exact socket bindings, physical interaction layers and socket identity preservation verified. Headset acceptance remains separate.");
            }
            finally
            {
                // A fresh -batchmode process can report no loaded/active scenes. Unity refuses
                // that setup; restore saved Editor scenes only when the setup is valid.
                bool canRestore = setup.Length > 0 && setup.Any(s => s.isLoaded && s.isActive) &&
                    setup.Count(s => s.isActive) == 1 && setup.All(s => !string.IsNullOrEmpty(s.path) && AssetDatabase.LoadAssetAtPath<SceneAsset>(s.path));
                if (canRestore) EditorSceneManager.RestoreSceneManagerSetup(setup);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        [MenuItem("Lighthouse Keepers/Development/Bind legacy basic interactions in loaded scenes")]
        public static void BindLoadedScenes()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Bind legacy controls outside Play Mode.");
            int count = 0;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded || !LevelNames.Any(name => scene.path == PathFor(name))) continue;
                BindScene(scene);
                ValidateBindings(scene);
                count++;
            }
            Debug.Log($"Bound basic interactions in {count} loaded legacy scenes. Review and save scene/mesh changes. These local hooks do not implement Storm Watch rules or voice networking.");
        }

        static void BindScene(Scene scene)
        {
            var all = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
            bool rebuildBatches = all.Any(t => t.gameObject.isStatic &&
                (t.name == "Shutoff valve handwheel" || t.name == "Red valve wheel spoke" || t.name == "Separated starter lever" || t.name == "Handheld radio"));
            var sockets = all.Select(t => t.GetComponent<PuzzleSocket>()).Where(s => s).ToArray();
            PuzzleSocket Socket(string station, string role)
            {
                var matches = sockets.Where(s => s.StationId == station && s.Role == role).ToArray();
                if (matches.Length != 1 || !matches[0].Anchor) throw new InvalidOperationException($"Expected one anchored {station} / {role} in {scene.name}.");
                return matches[0];
            }
            if (scene.name == "LK_Level01_Plumbing")
            {
                var socket = Socket("Plumbing Repair", "Wrench");
                foreach (var wheel in all.Where(t => t.name == "Shutoff valve handwheel"))
                {
                    Dynamic(wheel.gameObject);
                    var control = Add<ValveHandwheel>(wheel.gameObject);
                    Set(control, "localTwistAxis", Vector3.up);
                    Set(control, "repairSocket", socket);
                    var spokes = all.Where(t => t.name == "Red valve wheel spoke" && Vector3.Distance(t.position, wheel.position) < .2f).ToArray();
                    foreach (var spoke in spokes) Dynamic(spoke.gameObject);
                    var settings = new SerializedObject(control);
                    var array = settings.FindProperty("syncedVisuals"); array.arraySize = spokes.Length;
                    for (int j = 0; j < spokes.Length; j++) array.GetArrayElementAtIndex(j).objectReferenceValue = spokes[j];
                    settings.ApplyModifiedProperties();
                }
            }
            if (scene.name == "LK_Level02_Generator")
            {
                var lever = all.Single(t => t.name == "Separated starter lever");
                Dynamic(lever.gameObject);
                var control = Add<LeverSwitch>(lever.gameObject);
                Set(control, "localTwistAxis", Vector3.right);
                Set(control, "boundSocket", Socket("Generator Service", "Starter"));
            }
            if (scene.name == "LK_Level03_Communications")
            {
                var radio = all.Single(t => t.name == "Handheld radio");
                Dynamic(radio.gameObject);
                Set(Add<PushToTalkButton>(radio.gameObject), "boundSocket", Socket("Radio Relay", "Push to talk"));
                Dial(Socket("Radio Relay", "Frequency dial"), "Legacy frequency dial");
            }
            if (scene.name == "LK_Level04_Lantern")
            {
                Dial(Socket("Lantern Alignment", "Bearing controls"), "Legacy bearing wheel", true);
                var socket = Socket("Breaker Coordination", "Beacon status");
                var lever = Primitive(socket, "Legacy beacon status lever", PrimitiveType.Cube, new Vector3(.12f,.5f,.12f));
                var control = Add<LeverSwitch>(lever);
                Set(control, "localTwistAxis", Vector3.right);
                Set(control, "boundSocket", socket);
            }
            // Existing source renderers were hidden inside combined static meshes. Remove their
            // old baked copies, otherwise a moving control leaves an immobile visual behind.
            if (rebuildBatches || BatchVertexCountsDiffer(scene)) RebuildSceneBatches(scene);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        static string SocketSignature(Scene scene) => string.Join("\n", scene.GetRootGameObjects()
            .SelectMany(g => g.GetComponentsInChildren<PuzzleSocket>(true)).OrderBy(s => s.name)
            .Select(s => s.name + ":" + EditorJsonUtility.ToJson(s) + ":" + s.transform.position.ToString("R") + ":" + s.transform.rotation.ToString("R")));

        static void ValidateBindings(Scene scene)
        {
            var all = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
            int expectedSockets = scene.name == LevelNames[0] || scene.name == LevelNames[2] ? 5 : scene.name == LevelNames[1] ? 4 : 2;
            var sockets = all.Select(t => t.GetComponent<PuzzleSocket>()).Where(s => s).ToArray();
            if (sockets.Length != expectedSockets) throw new InvalidOperationException($"Expected {expectedSockets} existing sockets in {scene.name}; found {sockets.Length}.");
            void Bound<T>(string field, string station, string role, int expected) where T : Component
            {
                var controls = all.Select(t => t.GetComponent<T>()).Where(c => c).ToArray();
                if (controls.Length != expected) throw new InvalidOperationException($"Expected {expected} {typeof(T).Name} in {scene.name}.");
                foreach (var control in controls)
                {
                    var socket = new SerializedObject(control).FindProperty(field).objectReferenceValue as PuzzleSocket;
                    if (!socket || socket.StationId != station || socket.Role != role || socket.gameObject.scene != scene || !socket.Anchor)
                        throw new InvalidOperationException("Wrong existing socket binding on " + control.name);
                    var interactable = control.GetComponent<XRBaseInteractable>();
                    if (!interactable || !interactable.enabled || !control.gameObject.activeInHierarchy || control.gameObject.layer != 0 || control.gameObject.isStatic || !control.GetComponent<Collider>())
                        throw new InvalidOperationException("Unreachable or static legacy interactable: " + control.name);
                }
            }
            if (scene.name == LevelNames[0]) Bound<ValveHandwheel>("repairSocket", "Plumbing Repair", "Wrench", 3);
            if (scene.name == LevelNames[1]) Bound<LeverSwitch>("boundSocket", "Generator Service", "Starter", 1);
            if (scene.name == LevelNames[2])
            {
                Bound<PushToTalkButton>("boundSocket", "Radio Relay", "Push to talk", 1);
                Bound<RotaryDial>("boundSocket", "Radio Relay", "Frequency dial", 1);
            }
            if (scene.name == LevelNames[3])
            {
                Bound<LeverSwitch>("boundSocket", "Breaker Coordination", "Beacon status", 1);
                Bound<RotaryDial>("boundSocket", "Lantern Alignment", "Bearing controls", 1);
            }
        }

        static MeshRenderer[] StaticBatchSources(Scene scene, Transform exclude = null) => scene.GetRootGameObjects()
            .SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true))
            // Match the old FindObjectsByType search: disabled source renderers are included,
            // but inactive GameObjects (e.g. flood warning lamps) must never become visible batches.
            .Where(r => (!exclude || !r.transform.IsChildOf(exclude)) && r.gameObject.activeInHierarchy && r.gameObject.isStatic &&
                !r.GetComponentInParent<Rigidbody>() && !r.GetComponentInParent<BeaconRotation>() && !r.GetComponent<FloodSurface>() &&
                !r.GetComponentInParent<LODGroup>() && r.GetComponent<MeshFilter>() && r.sharedMaterials.Length == 1).ToArray();

        static bool BatchVertexCountsDiffer(Scene scene)
        {
            var root = scene.GetRootGameObjects().SingleOrDefault(g => g.name == "Static render batches");
            if (!root) return false;
            var expected = StaticBatchSources(scene, root.transform).GroupBy(r => r.sharedMaterial)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.GetComponent<MeshFilter>().sharedMesh.vertexCount));
            var actual = root.GetComponentsInChildren<MeshRenderer>(true).GroupBy(r => r.sharedMaterial)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.GetComponent<MeshFilter>().sharedMesh.vertexCount));
            return expected.Count != actual.Count || expected.Any(pair => !actual.TryGetValue(pair.Key, out int count) || pair.Value != count);
        }

        static void RebuildSceneBatches(Scene scene)
        {
            // The older global rebuild helper searches every loaded scene. Keep this operation
            // scoped to one level so additive batch authoring cannot combine another room's mesh.
            var old = scene.GetRootGameObjects().SingleOrDefault(g => g.name == "Static render batches");
            if (!old) return;
            UnityEngine.Object.DestroyImmediate(old);
            var candidates = StaticBatchSources(scene);
            var root = new GameObject("Static render batches"); SceneManager.MoveGameObjectToScene(root, scene);
            foreach (var group in candidates.GroupBy(r => r.sharedMaterial))
            {
                string name = scene.name + "_" + group.Key.name;
                var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(group.Select(r => new CombineInstance { mesh = r.GetComponent<MeshFilter>().sharedMesh, transform = r.transform.localToWorldMatrix }).ToArray(), true, true);
                Unwrapping.GenerateSecondaryUVSet(mesh);
                mesh = LighthouseAssets.SaveMesh(mesh, name);
                EditorUtility.SetDirty(mesh);
                var go = new GameObject(group.Key.name, typeof(MeshFilter), typeof(MeshRenderer));
                go.transform.SetParent(root.transform, false); go.GetComponent<MeshFilter>().sharedMesh = mesh;
                go.GetComponent<MeshRenderer>().sharedMaterial = group.Key; go.isStatic = true;
                foreach (var renderer in group) renderer.enabled = false;
            }
        }

        static void Dial(PuzzleSocket socket, string name, bool hideDuringStormWatch = false)
        {
            var go = Primitive(socket, name, PrimitiveType.Cylinder, new Vector3(.22f,.04f,.22f));
            go.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var dial = Add<RotaryDial>(go);
            Set(dial, "localTwistAxis", Vector3.up);
            Set(dial, "boundSocket", socket);
            Set(dial, "hideDuringStormWatch", hideDuringStormWatch);
        }
        static GameObject Primitive(PuzzleSocket socket, string name, PrimitiveType type, Vector3 scale)
        {
            var existing = socket.Anchor.Find(name);
            if (existing) return existing.gameObject;
            var go = GameObject.CreatePrimitive(type);
            Undo.RegisterCreatedObjectUndo(go, "Create legacy interaction control");
            go.name = name;
            go.transform.SetParent(socket.Anchor, false);
            go.transform.localScale = scale;
            // Use an existing room material; these are functional primitives, not new art assets.
            var material = LighthouseAssets.M("Brass");
            if (material) go.GetComponent<MeshRenderer>().sharedMaterial = material;
            Dynamic(go);
            return go;
        }
        static void Dynamic(GameObject go)
        {
            Undo.RecordObject(go, "Make control dynamic");
            go.isStatic = false;
            // The integrated environment retires the old starter prop by deactivating it.
            // Only explicit reused control targets/spokes call this helper; leave other sources alone.
            go.SetActive(true);
            // The existing Quest near interactor queries Default. Static environment props use
            // layer 8, which its near/far masks intentionally exclude.
            go.layer = 0;
            if (go.TryGetComponent<Renderer>(out var renderer))
            { Undo.RecordObject(renderer, "Show control source renderer"); renderer.enabled = true; }
        }
        static T Add<T>(GameObject go) where T : Component => go.TryGetComponent<T>(out var existing) ? existing : Undo.AddComponent<T>(go);
        static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
        { var settings = new SerializedObject(target); settings.FindProperty(field).objectReferenceValue = value; settings.ApplyModifiedProperties(); }
        static void Set(UnityEngine.Object target, string field, Vector3 value)
        { var settings = new SerializedObject(target); settings.FindProperty(field).vector3Value = value; settings.ApplyModifiedProperties(); }
        static void Set(UnityEngine.Object target, string field, bool value)
        { var settings = new SerializedObject(target); settings.FindProperty(field).boolValue = value; settings.ApplyModifiedProperties(); }
    }
}
