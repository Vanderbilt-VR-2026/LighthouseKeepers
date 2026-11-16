using NUnit.Framework;
using LighthouseKeepers.Core.Progression;
using UnityEngine;
namespace LighthouseKeepers.Tests.M3
{
    public sealed class FlareTests
    {
        [Test] public void SharedGunHasOneRoundAndReservedSlotCannotFire()
        {
            var s=new FlareState();Assert.False(s.Fire(2,Vector3.zero,Vector3.forward,10));Assert.False(s.Fire(0,Vector3.zero,Vector3.zero,10));Assert.False(s.Fire(0,new Vector3(float.NaN,0,0),Vector3.forward,10));Assert.True(s.Loaded(0));
            Assert.True(s.Fire(0,new Vector3(0,1,0),Vector3.forward,10));Assert.False(s.Fire(0,Vector3.zero,Vector3.forward,10));Assert.False(s.Loaded(1));Assert.False(s.Fire(1,Vector3.zero,Vector3.forward,11));
        }
        [Test] public void ReplicaCannotFireSpendOrMoveAFlareAndRejectsMalformedFrames()
        {
            var h=new FlareState();h.Fire(0,new Vector3(1,2,3),Vector3.forward,20);var g=new FlareState{ReadOnly=true};Assert.True(g.Apply(h.Capture(),20));Assert.False(g.Fire(0,Vector3.zero,Vector3.forward,21));Assert.False(g.Stop(0,Vector3.one,1,true));
            var f=h.Capture();f.first.direction=new Vector3(float.NaN,0,0);Assert.False(g.Apply(f,21));Assert.False(g.Loaded(0));f=h.Capture();f.first.at=22;Assert.False(g.Apply(f,21));
            h.Stop(0,new Vector3(1,2,5),.2f,true);Assert.True(g.Apply(h.Capture(),21));Assert.True(g.Shot(0).repelled);Assert.AreEqual(h.Shot(0).stop,g.Shot(0).stop);
        }
        [Test] public void ImpactStopsTrajectoryAndOnlyFirstImpactWins()
        {
            var s=new FlareState();s.Fire(0,Vector3.up,Vector3.forward,0);var p=FlareState.Position(s.Shot(0),.3f);Assert.Greater(p.z,4);Assert.Less(p.y,1);
            Assert.True(s.Stop(0,p,.3f,false));Assert.False(s.Stop(0,p+Vector3.forward,.4f,true));Assert.AreEqual(p,FlareState.Position(s.Shot(0),3));Assert.True(s.Capture().Valid(3));
        }
        [Test] public void FlareOnlyRepelsAnActiveHostEncounterAndPreventsImmediateRepeat()
        {
            var traces=new SabotageState(new NightScenario{faults=new[]{new NightFault{stationId="engine"}}});var d=new DrownedState(traces,false);Assert.False(d.Repel(80));traces.Warn(0,80);d.Tick(80,true,false,false);traces.Broke(0,83);d.Tick(84,true,false,false);Assert.AreEqual(DrownedPhase.Manifest,d.Phase);
            var g=new DrownedState(traces,false){ReadOnly=true};g.Apply(d.Capture(),84);Assert.False(g.Repel(84));Assert.True(d.Repel(85));Assert.AreEqual(DrownedPhase.Retreat,d.Phase);d.Tick(100,true,false,false);d.Tick(101,true,false,false);Assert.AreEqual(DrownedPhase.Dormant,d.Phase);
        }
        [Test] public void CollisionSegmentCannotReachThroughAnEarlierWall()
        {Assert.True(FlareState.NearSegment(Vector3.zero,new Vector3(0,0,5),new Vector3(0,0,4),.6f));Assert.False(FlareState.NearSegment(Vector3.zero,new Vector3(0,0,2),new Vector3(0,0,4),.6f));}
        [Test] public void OriginalGunIsDetailedButBoundedAndFinite()
        {
            var t=System.Type.GetType("LighthouseKeepers.WatchDemo.FlareGunMesh, Assembly-CSharp",true);var meshes=(Mesh[])t.GetMethod("Build").Invoke(null,null);int count=0;
            try{Assert.AreEqual(3,meshes.Length);foreach(var mesh in meshes){count+=mesh.triangles.Length/3;foreach(var v in mesh.vertices)Assert.True(FlareShot.Finite(v));Assert.Less(mesh.bounds.size.magnitude,.5f);}Assert.Greater(count,1000);Assert.Less(count,7000);}finally{foreach(var m in meshes)Object.DestroyImmediate(m);}
        }
    }
}
