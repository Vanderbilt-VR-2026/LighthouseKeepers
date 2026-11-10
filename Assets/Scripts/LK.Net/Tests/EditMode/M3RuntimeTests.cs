using System;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class M3RuntimeTests
 {
  static Type T(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
  static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
  static V Get<V>(object o,string n)=>(V)o.GetType().GetProperty(n).GetValue(o);
  static void Set(object o,string n,object v)=>o.GetType().GetProperty(n).SetValue(o,v);
  sealed class Crew:ICoopWatchLink
  {public bool Ready=>true;public bool Send(CoopAction a,float v=0,bool p=false)=>true;public bool RemoteHeld(Component p)=>false;public bool RemoteMicrophone(Component p,out bool t){t=false;return false;}}
  object New(int day=3,bool crew=false,bool demo=false)
  {
   var cat=Resources.Load("M3Campaign");Assert.NotNull(cat);var defs=cat.GetType().GetField("nights").GetValue(cat);
   if(demo){var d=cat.GetType().GetField("demo").GetValue(cat);var a=Array.CreateInstance(d.GetType(),1);a.SetValue(d,0);defs=a;}
   var state=Activator.CreateInstance(T("WatchState"),defs,day,Resources.Load<S4Tuning>("S4Tuning"),Resources.Load<S4M2Tuning>("S4M2Tuning"),Resources.Load<ImmersionTuning>("ImmersionTuning"));
   if(crew)Set(state,"Coop",new Crew());return state;
  }
  void Tick(object s,double t)=>Call(s,"Tick",t);
  void Start(object s)
  {
   for(int i=0;i<100;i++)Call(s,"PourFuel",.1f);
   for(int i=0;i<3;i++){Call(s,"PrimeEngine");Tick(s,.4);}
   Call(s,"SetChoke",true);for(int i=0;i<3;i++){Call(s,"StarterPulled");Tick(s,.4);}Call(s,"SetChoke",false);
  }
  [TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)]
  public void AuthoredNightsStartWithOnlyThePipeAndRetryRestoresIt(int day)
  {
   var s=New(day);Assert.True(Get<bool>(s,"Campaign"));Assert.False(Get<bool>(s,"Day3Balance"));Assert.False(Get<bool>(s,"Repaired"));Assert.True(Get<bool>(s,"BeaconShining"));
   Assert.AreEqual(1,Get<int>(s,"CampaignFaultMask"));Tick(s,30);Assert.False((bool)Call(s,"Sleep"));
   Call(s,"CompleteRepair");Tick(s,50);Call(s,"Retry");Assert.Zero(Get<double>(s,"Elapsed"));Assert.AreEqual(1,Get<int>(s,"CampaignFaultMask"));Assert.False(Get<bool>(s,"Repaired"));
  }
  [Test] public void PipeCompletionWaitsForOpeningThenEngineThenUpperJobsWithBreathers()
  {
   var s=New();Tick(s,20);Call(s,"CompleteRepair");Tick(s,24);Assert.AreEqual("On",Get<object>(s,"Power").ToString());Tick(s,1);
   Assert.AreEqual("Off",Get<object>(s,"Power").ToString());Assert.True((bool)Call(s,"IsPending","STN_RADIO_CONSOLE"));Start(s);
   Assert.AreEqual("On",Get<object>(s,"Power").ToString());Assert.True((bool)Call(s,"IsPending","STN_BEACON_CONTROL"));
   Tick(s,16);Assert.False((bool)Call(s,"IsPending","STN_BEACON_CONTROL"));Tick(s,12);Assert.False((bool)Call(s,"IsPending","STN_RADIO_CONSOLE"));
   Assert.False(Get<bool>(s,"Complete"));Assert.False(Get<bool>(s,"Failed"));
  }
  [Test] public void CrewSnapshotAndRetryCarryScheduleAndGuestsCannotFireEvents()
  {
   var h=New(3,true);Tick(h,80);var g=New();Call(g,"SetRemoteReplica",true);
   Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",.1f)));Assert.AreEqual(Get<int>(h,"CampaignFaultMask"),Get<int>(g,"CampaignFaultMask"));Assert.AreEqual(330,Get<double>(g,"Duration"));
   Tick(g,100);Call(g,"CompleteRepair");Assert.AreEqual(80,Get<double>(g,"Elapsed"));Assert.False(Get<bool>(g,"Repaired"));
   foreach(var field in new[]{"nightFaultMask","nightGraceUntil","campaignEnabled"})
   {var snap=Call(h,"Capture",.1f);var f=snap.GetType().GetField(field);f.SetValue(snap,field=="nightFaultMask"?(object)int.MaxValue:field=="campaignEnabled"?false:(object)999d);Assert.False((bool)Call(g,"ApplyRemote",snap));}
   Call(h,"Retry");Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",0f)));Assert.AreEqual(1,Get<int>(g,"CampaignFaultMask"));
  }
  [Test] public void DemoConsumesItsOwnDataAndNightOneCannotLose()
  {
   var demo=New(3,false,true);Assert.AreEqual(360,Get<double>(demo,"Duration"));Tick(demo,75);Assert.AreEqual("Off",Get<object>(demo,"Power").ToString());
   var one=New(1);Call(one,"EngageWatch");Tick(one,400);Call(one,"ObserveWater",10d,1f);Assert.False(Get<bool>(one,"Failed"));
  }
  [Test] public void LowFuelEventDoesNotRestartEngineAndCannotCompleteBeforeRefilling()
  {
   var s=New(4);Tick(s,75);Call(s,"CompleteRepair");Start(s);Tick(s,95);
   Assert.True(Get<bool>(s,"FuelJobPending"));Assert.AreEqual("On",Get<object>(s,"Power").ToString());Assert.False((bool)Call(s,"IsDone","STN_GENERATOR"));
   var director=Get<object>(s,"Director");Assert.AreEqual("Fuel",Call(director,"StepForStation","STN_GENERATOR",false).ToString());
   for(int i=0;i<100;i++)Call(s,"PourFuel",.1f);Tick(s,.3);Assert.False(Get<bool>(s,"FuelJobPending"));Assert.True((bool)Call(s,"IsDone","STN_GENERATOR"));
  }
 }
}
