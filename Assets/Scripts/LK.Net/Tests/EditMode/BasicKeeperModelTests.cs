using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class BasicKeeperModelTests
    {
        [TestCase("PF_BasicCaptain")]
        [TestCase("PF_BasicFisherman")]
        public void BundledModelImportsWithValidAnchorsAndReadableGeometry(string name)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Avatars/{name}.prefab");
            Assert.NotNull(prefab);
            var model=prefab.GetComponent<CrewAvatarModel>();
            Assert.NotNull(model);Assert.IsTrue(model.TryValidate(out var error),error);
            Assert.IsEmpty(prefab.GetComponentsInChildren<Collider>(true));
            var filters=prefab.GetComponentsInChildren<MeshFilter>(true);
            Assert.IsNotEmpty(filters);
            long triangles=0;
            foreach(var filter in filters)
            {
                var mesh=filter.sharedMesh;Assert.NotNull(mesh);Assert.Greater(mesh.vertexCount,0);
                var materials=filter.GetComponent<MeshRenderer>().sharedMaterials;
                Assert.AreEqual(mesh.subMeshCount,materials.Length);
                foreach(var material in materials){Assert.NotNull(material);Assert.NotNull(material.shader);}
                Assert.AreEqual(mesh.vertexCount,mesh.normals.Length);
                foreach(var vertex in mesh.vertices)
                    Assert.IsTrue(float.IsFinite(vertex.x)&&float.IsFinite(vertex.y)&&float.IsFinite(vertex.z));
                for(int i=0;i<mesh.subMeshCount;i++)triangles+=mesh.GetIndexCount(i)/3;
            }
            Assert.That(triangles,Is.GreaterThan(0).And.LessThan(10000));
        }

        [Test] public void NetworkAvatarDefaultsProvideValidDistinctModelsForBothRoles()
        {
            var player=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Network/Resources/PF_PlayerRig.prefab");
            Assert.NotNull(player);
            using var fields=new SerializedObject(player.GetComponent<CrewAvatarView>());
            var captain=fields.FindProperty("captainModel").objectReferenceValue as GameObject;
            var fisherman=fields.FindProperty("fishermanModel").objectReferenceValue as GameObject;
            Assert.NotNull(captain);Assert.NotNull(fisherman);Assert.AreNotSame(captain,fisherman);
            foreach(var prefab in new[]{captain,fisherman})
            {
                var model=prefab.GetComponent<CrewAvatarModel>();Assert.NotNull(model);
                Assert.IsTrue(model.TryValidate(out var error),error);
            }
        }
    }
}
