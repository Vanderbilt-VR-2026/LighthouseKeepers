using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class TowerWiringTests
    {
        static Type T(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
        static object Field(object o,string n)=>o.GetType().GetField(n).GetValue(o);
        static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
        static object State(){var c=Resources.Load("WatchDaysS4M2");return Activator.CreateInstance(T("WatchState"),Field(c,"days"),1,Resources.Load<S4Tuning>("S4Tuning"),Resources.Load<S4M2Tuning>("S4M2Tuning"),Resources.Load<ImmersionTuning>("ImmersionTuning"));}
        static bool Fed(object state,int circuit)=>(bool)T("TowerWiringPresentation").GetMethod("Fed").Invoke(null,new[]{state,(object)circuit});
        [Test] public void AnOpenCircuitGoesDarkWithoutExtinguishingOtherFeeds()
        {
            var host=State();for(int c=0;c<4;c++)Assert.True(Fed(host,c));
            Call(host,"SetCircuit",0,false);Assert.False(Fed(host,0));Assert.True(Fed(host,1));Assert.True(Fed(host,2));Assert.True(Fed(host,3));Assert.True(Fed(host,4));
            Call(host,"SetCircuit",2,false);Assert.False(Fed(host,2));Assert.True(Fed(host,3));
            Call(host,"SetCircuit",0,true);Assert.True(Fed(host,0));
        }
        [Test] public void GuestWiringUsesTheHostCircuitSnapshotAndCannotChangeItLocally()
        {
            var host=State();var guest=State();Call(guest,"SetRemoteReplica",true);
            for(int off=0;off<4;off++)
            {
                Call(host,"SetCircuit",off,false);var snapshot=Call(host,"Capture",.1f);
                Assert.True((bool)Call(guest,"ApplyRemote",snapshot));
                for(int c=0;c<5;c++)Assert.AreEqual(Fed(host,c),Fed(guest,c));
                Assert.False((bool)Call(guest,"SetCircuit",off,true));Assert.False(Fed(guest,off));
            }
        }
        [Test] public void BakedRoutesKeepAllFourCircuitsAndFitTheQuestGeometryBudget()
        {
            var layout=Resources.Load("TowerWiring/Layout");Assert.NotNull(layout);
            var meshes=(Mesh[])Field(layout,"floors");Assert.AreEqual(5,meshes.Length);int count=meshes.Sum(m=>m.triangles.Length/3);Assert.AreEqual((int)Field(layout,"triangles"),count);Assert.LessOrEqual(count,15000);
            Assert.True(((Material)Field(layout,"material")).mainTexture);
            var runs=(Array)Field(layout,"runs");var circuits=new System.Collections.Generic.HashSet<int>();
            foreach(var r in runs){circuits.Add((int)Field(r,"circuit"));var p=(Vector3[])Field(r,"points");Assert.GreaterOrEqual(p.Length,2);Assert.True(p.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z)));}
            CollectionAssert.AreEquivalent(new[]{0,1,2,3,4},circuits);
            var endpoints=(Array)Field(layout,"endpoints");var names=endpoints.Cast<object>().Select(e=>(string)Field(e,"name")).ToArray();
            Assert.Contains("F1 electric pump terminal",names);Assert.Contains("Beacon motor terminal",names);Assert.Contains("Radio cabinet terminal",names);
            for(int i=0;i<6;i++)Assert.Contains("Brass PA speaker "+i,names);
        }
    }
}
