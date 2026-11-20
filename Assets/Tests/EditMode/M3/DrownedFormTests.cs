using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Object=UnityEngine.Object;
namespace LighthouseKeepers.Tests.M3
{
    public sealed class DrownedFormTests
    {
        static Type Runtime(string name)=>Type.GetType("LighthouseKeepers.WatchDemo."+name+", Assembly-CSharp",true);
        [Test] public void EveryFormIsDistinctFiniteAndUnderTheQuestGeometryBudget()
        {
            var t=Runtime("DrownedForms");var e=Runtime("DrownedForm");var sizes=new float[3];
            for(int i=0;i<3;i++){
                var m=(Mesh)t.GetMethod("Build").Invoke(null,new[]{Enum.ToObject(e,i)});
                try{
                    Assert.AreEqual(1,m.subMeshCount);Assert.LessOrEqual(m.triangles.Length/3,12000);Assert.Greater(m.triangles.Length/3,3000);
                    Assert.AreEqual(m.vertexCount,m.colors.Length);Assert.AreEqual(m.vertexCount,m.uv.Length);
                    foreach(var v in m.vertices)Assert.True(float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z));
                    sizes[i]=m.bounds.size.y;Assert.Less(m.bounds.size.x,1.2f);
                    // Visible outward-facing nose geometry must not have inverted normals.
                    var verts=m.vertices;var normals=m.normals;float maxZ=verts.Max(x=>x.z);int index=Array.FindIndex(verts,x=>x.z>=maxZ-.0001f);Assert.Greater(normals[index].z,-.2f);
                }finally{Object.DestroyImmediate(m);}
            }
            Assert.That(sizes[0],Is.InRange(1.7f,1.95f));Assert.That(sizes[1],Is.InRange(1.15f,1.4f));Assert.That(sizes[2],Is.InRange(2.15f,2.5f));
        }
        [Test] public void SameNightAndFaultAlwaysChooseTheSameFormForHostGuestRetryAndLateJoin()
        {
            var select=Runtime("DrownedForms").GetMethod("Select");
            for(int day=2;day<=5;day++)for(int fault=0;fault<5;fault++){
                var a=select.Invoke(null,new object[]{day,fault});for(int replay=0;replay<3;replay++)Assert.AreEqual(a,select.Invoke(null,new object[]{day,fault}));
            }
            Assert.AreEqual("SeaHag",select.Invoke(null,new object[]{2,1}).ToString());Assert.AreEqual("Doll",select.Invoke(null,new object[]{3,1}).ToString());Assert.AreEqual("Gaunt",select.Invoke(null,new object[]{4,1}).ToString());
        }
    }
}
