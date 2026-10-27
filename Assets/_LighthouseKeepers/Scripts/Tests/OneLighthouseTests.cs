using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using LighthouseKeepers.Gameplay.Interaction;
using Unity.XR.CoreUtils;

namespace LighthouseKeepers.Tests
{
    public sealed class OneLighthouseTests
    {
        static readonly string[] Names = { "LK_NetworkLobby", "LK_Bootstrap", "LK_Exterior", "LK_Core", "LK_Level01_Plumbing", "LK_Level02_Generator", "LK_Level03_Communications", "LK_Level04_Lantern" };
        [Test] public void ExactShipManifestHasNoOtherScenesEvenDisabled()
        {
            CollectionAssert.AreEqual(Names, EditorBuildSettings.scenes.Select(s=>System.IO.Path.GetFileNameWithoutExtension(s.path)).ToArray());
            Assert.IsTrue(EditorBuildSettings.scenes.All(s=>s.enabled));
        }
        [Test] public void BuildScenesAndPlayerPrefabsHaveNoVignetteAndExactlyTwoSprint3Hands()
        {
            var previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (var entry in EditorBuildSettings.scenes)
                {
                    var scene = EditorSceneManager.OpenScene(entry.path);
                    foreach (var root in scene.GetRootGameObjects()) CheckRoot(root, entry.path);
                }
                foreach (string path in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_LighthouseKeepers/Prefabs/Player", "Assets/Prefabs/Hands", "Assets/Prefabs/Network" })
                    .Select(AssetDatabase.GUIDToAssetPath))
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    CheckRoot(prefab, path);
                }
            }
            finally { if (previous.Any(s=>s.isLoaded) && previous.Count(s=>s.isActive)==1) EditorSceneManager.RestoreSceneManagerSetup(previous);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); }
        }
        static void CheckRoot(GameObject root, string path)
        {
            Assert.IsEmpty(root.GetComponentsInChildren<TunnelingVignetteController>(true), path + "/" + root.name);
            foreach (var rig in root.GetComponentsInChildren<XROrigin>(true))
            {
                var hands = rig.GetComponentsInChildren<HandRig>(true);
                Assert.AreEqual(2, hands.Length, path);
                Assert.AreEqual(2, hands.Select(h=>h.Node).Distinct().Count(), path);
                foreach (var h in hands)
                {
                    var data = new SerializedObject(h);
                    var material = (Material)data.FindProperty("gloveMaterial").objectReferenceValue;
                    var color = material.GetColor("_BaseColor");
                    Assert.Greater(color.r, color.g); Assert.Greater(color.g, color.b);
                    foreach (var renderer in h.transform.parent.GetComponentsInChildren<Renderer>(true))
                        if (!(renderer is LineRenderer) && !renderer.GetComponentInParent<HandRig>(true)) Assert.IsFalse(renderer.enabled, path + "/" + renderer.name);
                }
            }
        }
        [Test] public void NoVolumeProfileContainsVignette()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:VolumeProfile", new[] { "Assets" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                Assert.IsFalse(AssetDatabase.LoadAssetAtPath<VolumeProfile>(path).components.Any(c=>c is Vignette), path);
            }
        }
    }
}
