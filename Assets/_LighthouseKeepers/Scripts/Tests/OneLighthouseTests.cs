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
        static readonly string[] Names = { "LK_NetworkLobby", "LK_Lab_Environment" };
        [Test] public void RestoredLabDoesNotRequireSessionCFixturesUnlessExplicitlyMarked()
        {
            var previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/Lab/LK_Lab_Environment.unity");
                var catalog = System.Type.GetType("LighthouseKeepers.Environment.Sprint3.Editor.ShipSceneCatalog, LK.Environment.Sprint3.Editor", true);
                var isShip = catalog.GetMethod("IsShip");
                Assert.IsFalse((bool)isShip.Invoke(null, new object[] { scene }), "Restored WatchDemo is not Session C.");
                var marker = new GameObject("Session C validation marker");
                try
                {
                    marker.AddComponent(System.Type.GetType("LighthouseKeepers.Environment.Sprint3.ShipScene, LK.Environment.Sprint3", true));
                    Assert.IsTrue((bool)isShip.Invoke(null, new object[] { scene }), "Marked scenes must retain the Session C gates.");
                }
                finally { Object.DestroyImmediate(marker); }
            }
            finally
            {
                if (previous.Any(s=>s.isLoaded) && previous.Count(s=>s.isActive)==1) EditorSceneManager.RestoreSceneManagerSetup(previous);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }
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
        [Test] public void RuntimeHandsUseTanLitMaterialWithPreloadedShaderVariants()
        {
            var material=Resources.Load<Material>("Hands/M_Glove");
            Assert.AreEqual("Universal Render Pipeline/Lit",material.shader.name);
            var color=material.GetColor("_BaseColor");
            Assert.That(color.r,Is.EqualTo(198f/255).Within(.001f));
            Assert.That(color.g,Is.EqualTo(142f/255).Within(.001f));
            Assert.That(color.b,Is.EqualTo(106f/255).Within(.001f));
            Assert.AreEqual(0,material.GetFloat("_SpecularHighlights"));
            Assert.AreEqual(0,material.GetFloat("_EnvironmentReflections"));
            var variants=Resources.Load<ShaderVariantCollection>("Hands/HandShaders");
            Assert.IsNotNull(variants);
            Assert.IsTrue(variants.Contains(new ShaderVariantCollection.ShaderVariant(material.shader,PassType.ScriptableRenderPipeline,
                "_SPECULARHIGHLIGHTS_OFF","_ENVIRONMENTREFLECTIONS_OFF","_ADDITIONAL_LIGHTS")));
            var graphics=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            var preloaded=graphics.FindProperty("m_PreloadedShaders");
            Assert.IsTrue(Enumerable.Range(0,preloaded.arraySize).Any(i=>preloaded.GetArrayElementAtIndex(i).objectReferenceValue==variants));
        }
        [Test] public void ShipGuardRunsAfterUrpAndRemovesItsRecreatedVignette()
        {
            var packageType = System.Type.GetType("UnityEditor.Rendering.Universal.URPPreprocessBuild, Unity.RenderPipelines.Universal.Editor", true);
            var guardType = System.Type.GetType("LighthouseKeepers.Editor.OneLighthouseShipGuard, Assembly-CSharp-Editor", true);
            var package = (UnityEditor.Build.IPreprocessBuildWithReport)System.Activator.CreateInstance(packageType, true);
            var guard = (UnityEditor.Build.IPreprocessBuildWithReport)System.Activator.CreateInstance(guardType);
            Assert.Greater(guard.callbackOrder, package.callbackOrder);
            packageType.GetMethod("EnsureVolumeProfile", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(null, null);
            guard.OnPreprocessBuild(null);
            NoVolumeProfileContainsVignette();
        }
        [Test] public void NoVolumeProfileContainsVignette()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:VolumeProfile", new[] { "Assets" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                Assert.IsFalse(AssetDatabase.LoadAssetAtPath<VolumeProfile>(path).components.Any(c=>c is Vignette || c is FilmGrain), path);
            }
        }
    }
}
