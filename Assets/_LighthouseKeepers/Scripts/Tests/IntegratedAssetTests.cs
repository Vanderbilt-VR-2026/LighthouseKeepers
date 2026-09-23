using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace LighthouseKeepers.Tests {
 public class IntegratedAssetTests {
  const string Root="Assets/_LighthouseKeepers/ThirdPartyVariants/";
  [Test] public void SoundtrackUsesStreaming(){var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"Audio/LK_Pressure_SeamlessLoop.wav");Assert.That(clip,Is.Not.Null);Assert.That(clip.loadType,Is.EqualTo(AudioClipLoadType.Streaming));Assert.That(clip.length,Is.GreaterThan(100));}
  [Test] public void OceanHasBoundedGeometryAndCompiles(){var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"Meshes/LK_OceanGrid.asset");Assert.That(mesh.vertexCount,Is.EqualTo(4225));var material=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/LK_QuestOcean.mat");Assert.That(ShaderUtil.ShaderHasError(material.shader),Is.False);Assert.That(material.renderQueue,Is.LessThan(2500));}
  [Test] public void VendorPropVariantsUseUrp(){foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{Root+"Prefabs"})){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));foreach(var renderer in prefab.GetComponentsInChildren<Renderer>())foreach(var mat in renderer.sharedMaterials){Assert.That(mat,Is.Not.Null);Assert.That(mat.shader.name.StartsWith("Universal Render Pipeline/"),Is.True,mat.name);}}}
 }
}
