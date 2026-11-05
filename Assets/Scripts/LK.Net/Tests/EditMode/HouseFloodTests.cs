using System;
using LighthouseKeepers.Core;
using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class HouseFloodTests
 {
  [Test]public void DoorFillsBeforeHouseAndWaterSpreadsBeforeRising()
  {
   var t=ScriptableObject.CreateInstance<S4Tuning>();try{
    var f=new HouseFloodState(t);Assert.False(f.WetAt(new Vector3(0,0,-8)));
    f.Step(.1f,.1f,false);Assert.Greater(f.DoorFront,0);Assert.Less(f.DoorFront,HouseFloodState.DoorLength);Assert.Zero(f.RoomFront);
    for(int i=0;i<40;i++)f.Step(.1f,.1f,false);
    Assert.AreEqual(HouseFloodState.DoorLength,f.DoorFront,.001f);Assert.Greater(f.RoomFront,0);Assert.Less(f.RoomFront,HouseFloodState.HouseLength);Assert.LessOrEqual(f.Height,t.houseWaterFilmDepth+.0001f);
    Assert.False(f.WetAt(new Vector3(4,0,-7)),"Water crossed the side wall");
    for(int i=0;i<2000;i++)f.Step(.1f,.1f,false);
    Assert.AreEqual(HouseFloodState.HouseLength,f.RoomFront,.001f);Assert.Greater(f.Height,.02f);Assert.LessOrEqual(f.Height,.1f);Assert.True(f.WetAt(new Vector3(0,0,-8.4f)));
    f.Step(100,-.25f,true);Assert.Zero(f.Volume);Assert.False(f.WetAt(new Vector3(0,0,-8.4f)));
   }finally{UnityEngine.Object.DestroyImmediate(t);}
  }
  [Test]public void NoSourceNoWaterPressureDifferenceDrivesDirectionAndResetIsDry()
  {
   var t=ScriptableObject.CreateInstance<S4Tuning>();try{
    var f=new HouseFloodState(t);f.Step(60,-.2f,false);Assert.Zero(f.Volume);f.Step(5,.2f,false);float full=f.Volume;Assert.Greater(full,0);f.Step(2,0,false);Assert.Less(f.Volume,full);Assert.Less(f.Flux,0);f.Reset();Assert.Zero(f.Volume);
   }finally{UnityEngine.Object.DestroyImmediate(t);}
  }
  static Type W=>Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true);
  static object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n).Invoke(o,args);
  object New(){var c=Resources.Load("WatchDaysS4M2");return Activator.CreateInstance(W,c.GetType().GetField("days").GetValue(c),1,Resources.Load<S4Tuning>("S4Tuning"),Resources.Load<S4M2Tuning>("S4M2Tuning"),Resources.Load<ImmersionTuning>("ImmersionTuning"));}
  [Test]public void HostHouseVolumeReplicatesAndGuestCannotAdvanceIt()
  {
   var host=New();var guest=New();Call(guest,"SetRemoteReplica",true);Call(host,"AdvanceHouseFlood",5f,.2f);
   var snap=Call(host,"Capture",.2f);Assert.True((bool)Call(guest,"ApplyRemote",snap));
   var copy=(HouseFloodState)W.GetProperty("HouseFlood").GetValue(guest);float v=copy.Volume;Assert.Greater(v,0);Call(guest,"AdvanceHouseFlood",30f,1f);Assert.AreEqual(v,copy.Volume);
   snap.GetType().GetField("houseWaterVolume").SetValue(snap,float.NaN);Assert.False((bool)Call(guest,"ApplyRemote",snap));
   Call(host,"Retry");Assert.True((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.02f)));Assert.Zero(copy.Volume);
  }
  [Test]public void AuthoredFloodRatesAreFivePercentLowerWithDrainUnchanged()
  {var t=Resources.Load<S4Tuning>("S4Tuning");Assert.AreEqual(.002f*.95f,t.poweredRiseMPerSecond,.0000001f);Assert.AreEqual(.006f*.95f,t.unpoweredRiseMPerSecond,.0000001f);Assert.AreEqual(.012f,t.drainMPerSecond);}
 }
}
