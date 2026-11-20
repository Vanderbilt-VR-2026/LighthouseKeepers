using System;using NUnit.Framework;using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class WalterRigTests
 {
  [Test] public void WalterIsOneBoundedSkinAndHandsStayBoundToArms()
  {
   var root=new GameObject("test Walter");
   try{
    var type=Type.GetType("LighthouseKeepers.WatchDemo.WalterRig, Assembly-CSharp",true);var rig=(Component)type.GetMethod("Create").Invoke(null,new object[]{root.transform});
    var renderer=rig.GetComponent<SkinnedMeshRenderer>();Assert.NotNull(renderer);Assert.AreEqual(1,rig.GetComponentsInChildren<SkinnedMeshRenderer>().Length);
    var mesh=renderer.sharedMesh;Assert.Less(mesh.triangles.Length/3,15000);Assert.AreEqual(1,renderer.sharedMaterials.Length);
    var positions=mesh.vertices;var weights=mesh.boneWeights;
    for(int i=0;i<positions.Length;i++){var v=positions[i];var w=weights[i];Assert.That(w.weight0+w.weight1,Is.EqualTo(1).Within(.001));if(Mathf.Abs(v.x)>.32f&&v.y>.16f&&v.y<.63f){if(w.weight0>.01f)StringAssert.Contains("arm",renderer.bones[w.boneIndex0].name);if(w.weight1>.01f)StringAssert.Contains("Elbow",renderer.bones[w.boneIndex1].name);}}
    type.GetMethod("Show").Invoke(rig,new object[]{true,1.2f,.3f,1f});var pose=new Mesh();renderer.BakeMesh(pose);Assert.LessOrEqual(pose.bounds.max.y,1.85f,"Hunch must fit the doorway clearance");UnityEngine.Object.DestroyImmediate(pose);
   }finally{UnityEngine.Object.DestroyImmediate(root);}
  }
 }
}
