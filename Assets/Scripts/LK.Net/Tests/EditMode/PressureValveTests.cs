using System;
using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class PressureValveTests
 {
  static Type T(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
  static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
  static V Get<V>(object o,string n)=>(V)o.GetType().GetProperty(n).GetValue(o);
  static object New(){var c=Resources.Load("WatchDaysS4M2");return Activator.CreateInstance(T("WatchState"),c.GetType().GetField("days").GetValue(c),1,Resources.Load<S4Tuning>("S4Tuning"),Resources.Load<S4M2Tuning>("S4M2Tuning"),Resources.Load<ImmersionTuning>("ImmersionTuning"));}
  [Test] public void TwoDistinctActorsMustOverlapAndDroppedHeartbeatReleasesPressure()
  {
   var v=new PressureValveState(.55f);v.SetCrew(true);Assert.False(v.Allows(2));Assert.True(v.Hold(1,true));Assert.False(v.Allows(1));Assert.True(v.Allows(2));Assert.False(v.Hold(2,true));
   v.Tick(.3f);v.Hold(1,true);v.Tick(.3f);Assert.True(v.Allows(2));v.Tick(.3f);Assert.False(v.Reduced);Assert.True(v.Hold(2,true));Assert.True(v.Hold(2,false));Assert.False(v.Reduced);
  }
  [Test] public void DisconnectImmediatelyAllowsSoloRepairAndRejoinRequiresTwoActors()
  {
   var v=new PressureValveState(.55f);v.SetCrew(true);v.Hold(2,true);v.SetCrew(false);Assert.True(v.Reduced);Assert.False(v.Clamp());v.Tick(1000);Assert.True(v.Allows(0));v.SetCrew(true);Assert.False(v.Clamped);Assert.False(v.Allows(1));
   var guest=new PressureValveState(.55f){ReadOnly=true};Assert.False(guest.Clamp());Assert.False(guest.Hold(1,true));Assert.True(guest.Apply(true,false,2));guest.Tick(10);Assert.AreEqual(2,guest.Holder);Assert.False(guest.Apply(true,true,2));
  }
  [Test] public void ReleasePreservesBoltProgressBlocksSameKeeperAndReplicates()
  {
   var s=New();Call(s,"SetKeeperCount",2);var v=Get<PressureValveState>(s,"Pressure");var p=Get<object>(s,"Pipe");s.GetType().GetProperty("PipeWorkerActor").SetValue(s,2);v.Hold(1,true);
   Call(p,"Observe",.3f,15f,true,true,true);float progress=Get<float>(p,"StrokeDegrees");Assert.Greater(progress,0);float reduced=Get<float>(s,"LeakStrength");
   v.Hold(1,false);Call(p,"Observe",.3f,15f,true,true,true);Assert.AreEqual(progress,Get<float>(p,"StrokeDegrees"));Assert.Greater(Get<float>(s,"LeakStrength"),reduced);
   v.Hold(2,true);Call(p,"Observe",.3f,15f,true,true,true);Assert.AreEqual(progress,Get<float>(p,"StrokeDegrees"));v.Hold(2,false);v.Hold(1,true);
   var replica=New();Call(replica,"SetRemoteReplica",true);Assert.True((bool)Call(replica,"ApplyRemote",Call(s,"Capture",.1f)));Assert.AreEqual(1,Get<PressureValveState>(replica,"Pressure").Holder);Assert.AreEqual(progress,Get<float>(Get<object>(replica,"Pipe"),"StrokeDegrees"));
   Call(p,"Observe",.3f,15f,true,true,true);Assert.Greater(Get<int>(p,"TurnsDone"),0);Call(s,"Retry");Assert.False(v.Reduced);Assert.AreEqual(0,Get<int>(p,"BoltsDone"));
  }
  [Test] public void DirectorAssignsDifferentValveAndWrenchRoles()
  {
   var s=New();var v=Get<PressureValveState>(s,"Pressure");v.SetCrew(true);v.Hold(2,true);s.GetType().GetProperty("PipeWorkerActor").SetValue(s,1);var d=Get<object>(s,"Director");Call(d,"Tick",1f,1,Vector3.zero,2,Vector3.right,true);Assert.AreEqual("TightenBolt",Get<object>(d,"First").ToString());Assert.AreEqual("HoldValve",Get<object>(d,"Second").ToString());
  }
  [TestCase(4,false)][TestCase(4,true)][TestCase(5,false)][TestCase(5,true)]
  public void CrewRepairsPipeTogetherAfterStartingEngineEvenIfBeaconIsDark(int day,bool swapDuties)
  {
   var c=Resources.Load("M3Campaign");var s=Activator.CreateInstance(T("WatchState"),c.GetType().GetField("nights").GetValue(c),day,Resources.Load<S4Tuning>("S4Tuning"),Resources.Load<S4M2Tuning>("S4M2Tuning"),Resources.Load<ImmersionTuning>("ImmersionTuning"));
   Call(s,"SetKeeperCount",2);for(int i=0;i<150;i++)Call(s,"PourFuel",.1f);
   for(int i=0;i<3;i++){for(int n=0;n<100&&!Get<bool>(s,"StarterInWindow");n++)Call(s,"Tick",.05d);Assert.True((bool)Call(s,"HoldStartAssist",2,true));Assert.True((bool)Call(s,"PullStarterAs",1));Call(s,"Tick",.4d);}
   Assert.True(Get<WatchEngineState>(s,"Engine").Running);Assert.False(Get<bool>(s,"BeaconShining"));Assert.False(Get<bool>(s,"ShipGuidanceDue"));
   var d=Get<object>(s,"Director");d.GetType().GetField("DutyForActor").SetValue(d,new Func<int,CrewDuty>(a=>(a==1)^swapDuties?CrewDuty.Machinist:CrewDuty.Lightkeeper));
   Call(d,"Tick",1f,1,Vector3.zero,2,Vector3.zero,false);
   var first=Get<object>(d,"First").ToString();var second=Get<object>(d,"Second").ToString();
   CollectionAssert.AreEquivalent(new[]{"GrabWrench","HoldValve"},new[]{first,second});
   int worker=first=="GrabWrench"?1:2;s.GetType().GetProperty("PipeWorkerActor").SetValue(s,worker);
   Call(d,"Tick",2f,1,Vector3.zero,2,Vector3.zero,true);
   Assert.AreEqual("TightenBolt",Get<object>(d,worker==1?"First":"Second").ToString());
   Assert.AreEqual("HoldValve",Get<object>(d,worker==1?"Second":"First").ToString());
   Call(s,"CompleteRepair");Call(d,"Tick",3f,1,Vector3.zero,2,Vector3.zero,false);
   Assert.AreNotEqual("HoldValve",Get<object>(d,"First").ToString());Assert.AreNotEqual("HoldValve",Get<object>(d,"Second").ToString());
  }
 }
}
