using System;using UnityEngine;using NUnit.Framework;using LighthouseKeepers.Core;using LighthouseKeepers.Core.Progression;using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.Net.Tests {
 public sealed class TimedStarterTests {
  static Type T=>Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true);
  static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
  static V Get<V>(object o,string n)=>(V)o.GetType().GetProperty(n).GetValue(o);
  static object New(int day=2){var c=Resources.Load("M3Campaign");return Activator.CreateInstance(T,c.GetType().GetField("nights").GetValue(c),day,Resources.Load("S4Tuning"),Resources.Load("S4M2Tuning"),Resources.Load("ImmersionTuning"));}
  static void Wait(object s){for(int i=0;i<100&&!Get<bool>(s,"StarterInWindow");i++)Call(s,"Tick",.05d);Assert.True(Get<bool>(s,"StarterInWindow"));}
  [Test]public void FuelAndThreeTimedStrokesStartWithoutPrimerOrChoke(){var s=New();var e=Get<WatchEngineState>(s,"Engine");Assert.True(Get<bool>(s,"TimedStarter"));Assert.False((bool)Call(s,"PrimeEngine"));Assert.False((bool)Call(s,"SetChoke",true));Call(s,"StarterPulled");Assert.Zero(e.Pulls);for(int i=0;i<100;i++)Call(s,"PourFuel",.1f);for(int i=0;i<3;i++){Wait(s);Call(s,"StarterPulled");Call(s,"Tick",.3d);}Assert.True(e.Running&&e.Settled);Assert.False(e.ChokeClosed);Assert.Zero(e.Primer);Assert.False((bool)Call(s,"StarterPulled"));Call(s,"Retry");Assert.Zero(e.Pulls);Assert.False(e.Running);}
  [Test]public void PartnerHoldIsRequiredDistinctExpiresAndReplicates(){var s=New();Call(s,"SetKeeperCount",2);for(int i=0;i<100;i++)Call(s,"PourFuel",.1f);Wait(s);Assert.False((bool)Call(s,"PullStarterAs",1));Assert.True((bool)Call(s,"HoldStartAssist",2,true));Assert.False((bool)Call(s,"PullStarterAs",2));Assert.True((bool)Call(s,"PullStarterAs",1));Assert.AreEqual(1,Get<WatchEngineState>(s,"Engine").Pulls);var g=New();Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",Call(s,"Capture",0f)));Assert.AreEqual(2,Get<PressureValveState>(g,"StartAssist").Holder);Assert.False((bool)Call(g,"HoldStartAssist",1,true));Call(s,"Tick",.7d);Assert.Zero(Get<PressureValveState>(s,"StartAssist").Holder);Call(s,"SetKeeperCount",1);Wait(s);Assert.True((bool)Call(s,"PullStarterAs",1));Call(s,"SetScriptedPartner",true);Call(s,"HoldStartAssist",2147483646,true);Call(s,"Retry");Assert.Zero(Get<PressureValveState>(s,"StartAssist").Holder);}
  [Test]public void MissesKeepProgressAndSpamCannotAddPulls(){var t=ScriptableObject.CreateInstance<S4M2Tuning>();try{var e=new WatchEngineState(t);e.Reset(1,0,false);e.TimedPull(true);Assert.AreEqual(1,e.Pulls);Assert.False(e.TimedPull(true));e.Tick(.4f);e.TimedPull(false);Assert.AreEqual(1,e.Pulls);Assert.AreEqual(1,e.CoughSerial);Assert.Zero(e.FloodRemaining);e.ReadOnly=true;e.Tick(5);Assert.False(e.TimedPull(true));Assert.AreEqual(1,e.Pulls);}finally{UnityEngine.Object.DestroyImmediate(t);}}
  [Test]public void GuestReceivesSameGaugeAndProgressButCannotMutate(){var h=New();for(int i=0;i<100;i++)Call(h,"PourFuel",.1f);Wait(h);Call(h,"StarterPulled");var g=New();Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",0f)));Assert.AreEqual(Get<float>(h,"StarterPhase"),Get<float>(g,"StarterPhase"),.001f);Assert.AreEqual(1,Get<WatchEngineState>(g,"Engine").Pulls);Assert.False((bool)Call(g,"StarterPulled"));}
  [TestCase(4,false)][TestCase(4,true)][TestCase(5,false)][TestCase(5,true)]
  public void BothHumanKeepersAreAssignedToTheTimedStartBeforePipeOrShip(int day,bool swapDuties){
   var tuning=Sprint5Tuning.Current;bool old=tuning.shipPassages;tuning.shipPassages=true;
   try{
    var host=New(day);Call(host,"SetKeeperCount",2);for(int i=0;i<150;i++)Call(host,"PourFuel",.1f);
    var director=Get<object>(host,"Director");director.GetType().GetField("DutyForActor").SetValue(director,new Func<int,CrewDuty>(a=>(a==1)^swapDuties?CrewDuty.Machinist:CrewDuty.Lightkeeper));
    Call(director,"Tick",0f,1,Vector3.zero,2,Vector3.zero,false);
    Assert.AreEqual(swapDuties?"HoldStarterAssist":"Starter",Get<object>(director,"First").ToString());
    Assert.AreEqual(swapDuties?"Starter":"HoldStarterAssist",Get<object>(director,"Second").ToString());
    var guest=New(day);Call(guest,"SetRemoteReplica",true);Assert.True((bool)Call(guest,"ApplyRemote",Call(host,"Capture",0f)));
    var replica=Get<object>(guest,"Director");Assert.AreEqual(Get<object>(director,"First"),Get<object>(replica,"First"));Assert.AreEqual(Get<object>(director,"Second"),Get<object>(replica,"Second"));
    int worker=swapDuties?2:1,helper=swapDuties?1:2;
    for(int i=0;i<3;i++){Wait(host);Assert.True((bool)Call(host,"HoldStartAssist",helper,true));Assert.True((bool)Call(host,"PullStarterAs",worker));Call(host,"Tick",.4d);}
    Assert.True(Get<WatchEngineState>(host,"Engine").Running);
    Call(director,"Tick",1f,1,Vector3.zero,2,Vector3.zero,false);
    Assert.AreNotEqual("HoldStarterAssist",Get<object>(director,"First").ToString());Assert.AreNotEqual("HoldStarterAssist",Get<object>(director,"Second").ToString());
   }finally{tuning.shipPassages=old;}
  }
  [TestCase(false)][TestCase(true)]public void DemoAssignsOnlyOneKeeperToPullAndTheOtherToAssist(bool swapDuties){
   var tuning=Sprint5Tuning.Current;bool old=tuning.shipPassages;tuning.shipPassages=true;
   try{
    var c=Resources.Load<FiveNightCatalog>("M3Campaign");var host=Activator.CreateInstance(T,new[]{c.demo},c.demo.day,Resources.Load("S4Tuning"),Resources.Load("S4M2Tuning"),Resources.Load("ImmersionTuning"));
    Call(host,"SetKeeperCount",2);for(int i=0;i<150;i++)Call(host,"PourFuel",.1f);
    var d=Get<object>(host,"Director");d.GetType().GetField("DutyForActor").SetValue(d,new Func<int,CrewDuty>(a=>(a==1)^swapDuties?CrewDuty.Machinist:CrewDuty.Lightkeeper));
    Call(d,"Tick",0f,1,Vector3.zero,2,Vector3.zero,false);
    var steps=new[]{Get<object>(d,"First").ToString(),Get<object>(d,"Second").ToString()};
    CollectionAssert.AreEquivalent(new[]{"Starter","HoldStarterAssist"},steps);
   }finally{tuning.shipPassages=old;}
  }
 }
}
