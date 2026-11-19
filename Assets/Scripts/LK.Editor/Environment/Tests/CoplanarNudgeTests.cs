using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace LighthouseKeepers.Editor.Environment.Tests
{
    public sealed class CoplanarNudgeTests
    {
        // The committed shell prefabs must carry exactly the offsets LighthouseBuilder reapplies on rebuild,
        // so regenerating the shell cannot silently bring the z-fighting back.
        [Test] public void ReviewedMeshSeparationsSurviveShellRebuild()
        {
            foreach(var part in new[]{"F1","F2","F3","F4"})
            {
                var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/PF_Shell_"+part+".prefab");
                var copy=Object.Instantiate(source);
                try{LighthouseBuilder.ApplyVisualCorrections(copy);var a=source.GetComponentsInChildren<MeshFilter>(true);var b=copy.GetComponentsInChildren<MeshFilter>(true);Assert.AreEqual(a.Length,b.Length);for(int i=0;i<a.Length;i++)Assert.AreEqual(a[i].sharedMesh,b[i].sharedMesh,a[i].name);}
                finally{Object.DestroyImmediate(copy);}
            }
        }
        [Test] public void ShellPrefabsMatchTheBuilderNudgeTable()
        {
            int nudged=0;
            foreach(var part in new[]{"House","F1","F2","F3","F4"})
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/PF_Shell_"+part+".prefab");
                Assert.That(prefab,Is.Not.Null,part);
                foreach(var t in prefab.GetComponentsInChildren<Transform>(true))
                {
                    if(!t.GetComponent<MeshRenderer>()||t.parent!=prefab.transform)continue;
                    var expected=LighthouseBuilder.CoplanarNudge(t.name);
                    Assert.That(Vector3.Distance(t.localPosition,expected),Is.LessThan(1e-5f),t.name+" in PF_Shell_"+part);
                    if(expected!=Vector3.zero)nudged++;
                }
            }
            Assert.That(nudged,Is.EqualTo(10),"Throat, porch, two F1 reveal frames and six landings");
        }
        [Test] public void RebuildRestoresSolidFloorRenderersAndCollidersAfterPoleRemoval()
        {
            int visuals=0,colliders=0;
            string Key(string name){if(name.StartsWith("UCX_"))name=name.Substring(4);if(name.EndsWith("_01"))name=name.Substring(0,name.Length-3);return name;}
            foreach(var part in new[]{"F2","F3","F4"})
            {
                var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/PF_Shell_"+part+".prefab");
                var copy=Object.Instantiate(source);
                try{
                    var filters=copy.GetComponentsInChildren<MeshFilter>(true).Where(f=>LighthouseBuilder.SolidFloorMesh(f.name)).ToArray();
                    var solids=copy.GetComponentsInChildren<MeshCollider>(true).Where(c=>LighthouseBuilder.SolidFloorMesh(Key(c.name))).ToArray();
                    foreach(var f in filters){var old=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/WatchDemo/PoleShaftMeshes/"+f.name+".asset");Assert.NotNull(old,f.name);f.sharedMesh=old;}
                    foreach(var c in solids){var old=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/WatchDemo/PoleShaftMeshes/"+Key(c.name)+".asset");Assert.NotNull(old,c.name);c.sharedMesh=old;}
                    LighthouseBuilder.ApplyVisualCorrections(copy);
                    foreach(var f in filters){Assert.AreEqual(LighthouseBuilder.SolidFloorMesh(f.name),f.sharedMesh,f.name);visuals++;}
                    foreach(var c in solids){Assert.AreEqual(LighthouseBuilder.SolidFloorMesh(Key(c.name)),c.sharedMesh,c.name);Assert.False(c.convex,c.name);colliders++;}
                }finally{Object.DestroyImmediate(copy);}
            }
            Assert.AreEqual(4,visuals);Assert.AreEqual(4,colliders);
        }
    }
}
