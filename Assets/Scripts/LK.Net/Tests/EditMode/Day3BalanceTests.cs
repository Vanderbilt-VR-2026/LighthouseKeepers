using System;
using System.Reflection;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class Day3BalanceTests
 {
  static Type T(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
  static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
  static V Get<V>(object o,string n)=>(V)o.GetType().GetProperty(n).GetValue(o);
  static void Set(object o,string n,object v)=>o.GetType().GetProperty(n).SetValue(o,v);
  S4Tuning tuning;
  [SetUp]public void Setup(){tuning=UnityEngine.Object.Instantiate(Resources.Load<S4Tuning>("S4Tuning"));}
  [TearDown]public void Teardown(){UnityEngine.Object.DestroyImmediate(tuning);}
  sealed class Crew:ICoopWatchLink
  {public bool Ready=>true;public bool Send(CoopAction a,float v=0,bool p=false)=>true;public bool RemoteHeld(Component p)=>false;public bool RemoteMicrophone(Component p,out bool t){t=false;return false;}}
  object New(int day=3,bool crew=false){var cat=Resources.Load("WatchDaysS4M2");Assert.NotNull(cat);var days=cat.GetType().GetField("days").GetValue(cat);var s=Activator.CreateInstance(T("WatchState"),days,day,tuning,Resources.Load<S4M2Tuning>("S4M2Tuning"),Resources.Load<ImmersionTuning>("ImmersionTuning"));if(crew)Set(s,"Coop",new Crew());return s;}
  void Tick(object s,double seconds)=>Call(s,"Tick",seconds);
  void StartEngine(object s)
  {
   for(int i=0;i<100;i++)Call(s,"PourFuel",.1f);
   for(int i=0;i<3;i++){Assert.True((bool)Call(s,"PrimeEngine"));Tick(s,.4);}
   Assert.True((bool)Call(s,"SetChoke",true));
   for(int i=0;i<3;i++){Assert.True((bool)Call(s,"StarterPulled"));Tick(s,.4);}
   Assert.True((bool)Call(s,"SetChoke",false));
  }
  [Test]public void PipeFirstPendingJobsDoNotTickOrAllowSleep()
  {
   var s=New();Assert.AreEqual(1,Get<int>(s,"Day3Stage"));Assert.True(Get<bool>(s,"BeaconShining"));Assert.AreEqual("On",Get<object>(s,"Power").ToString());
   Tick(s,10);Assert.Zero(Get<int>(s,"CompletedTasks"));Assert.False((bool)Call(s,"Sleep"));
   foreach(var id in new[]{"STN_GENERATOR","STN_RADIO_CONSOLE","STN_BEACON_CONTROL"})Assert.True((bool)Call(s,"IsPending",id));
   Assert.False((bool)Call(s,"IgniteBeacon",false));Assert.False((bool)Call(s,"AlignBeacon",.1f));
   Assert.AreEqual(1,Get<int>(s,"ObjectiveIndex"));Assert.Zero(Get<double>(s,"BeaconDarkElapsed"));
  }
  [Test]public void TimedOutageAndEarlyRepairActivateExactlyOnceThenUpperJobsWaitForChokeOpen()
  {
   var s=New();Tick(s,74);Assert.AreEqual(1,Get<int>(s,"Day3Stage"));Tick(s,2);Assert.AreEqual(2,Get<int>(s,"Day3Stage"));Assert.AreEqual("Off",Get<object>(s,"Power").ToString());
   Assert.AreEqual(89,Get<double>(s,"BeaconGraceRemaining"),.01);Assert.AreEqual(0,Get<int>(s,"ObjectiveIndex"));
   StartEngine(s);Assert.AreEqual(3,Get<int>(s,"Day3Stage"));Assert.False((bool)Call(s,"IsPending","STN_RADIO_CONSOLE"));Assert.False(Get<bool>(s,"BeaconShining"));
   Tick(s,1);Assert.AreEqual(3,Get<int>(s,"Day3Stage"));Assert.AreEqual("On",Get<object>(s,"Power").ToString());
   var early=New();Tick(early,20);Call(early,"CompleteRepair");Assert.AreEqual(2,Get<int>(early,"Day3Stage"));Assert.AreEqual(90,Get<double>(early,"BeaconGraceRemaining"),.01);
  }
  [Test]public void GraceIsFiniteSloppyRunLosesAndRetryRestoresPipeFirst()
  {
   var s=New();Tick(s,75);Tick(s,90);Assert.Zero(Get<double>(s,"BeaconDarkElapsed"));Tick(s,119);Assert.False(Get<bool>(s,"Failed"));Tick(s,1.01);Assert.True(Get<bool>(s,"BeaconDarkFailed"));
   Call(s,"Retry");Assert.False(Get<bool>(s,"Failed"));Assert.AreEqual(1,Get<int>(s,"Day3Stage"));Assert.Zero(Get<double>(s,"BeaconGraceRemaining"));Assert.True(Get<bool>(s,"BeaconShining"));
   Call(s,"ObserveWater",3.1d,.56f);Assert.True(Get<bool>(s,"HighWaterFailed"));
  }
  [Test]public void SoloAndCrewHaveSeparateBudgetsAndGuestCannotAdvanceStages()
  {
   var solo=New();var host=New(3,true);Assert.AreEqual(390,Get<double>(solo,"Duration"));Assert.AreEqual(330,Get<double>(host,"Duration"));Assert.AreEqual(.65f,Get<float>(solo,"FloodRiseMultiplier"));Assert.AreEqual(.85f,Get<float>(host,"FloodRiseMultiplier"));
   var guest=New();Call(guest,"SetRemoteReplica",true);Tick(host,80);var snap=Call(host,"Capture",.2f);Assert.True((bool)Call(guest,"ApplyRemote",snap));Assert.AreEqual(Get<int>(host,"Day3Stage"),Get<int>(guest,"Day3Stage"));Assert.AreEqual(Get<double>(host,"Duration"),Get<double>(guest,"Duration"));Assert.AreEqual(Get<double>(host,"BeaconGraceRemaining"),Get<double>(guest,"BeaconGraceRemaining"));
   Tick(guest,100);Call(guest,"CompleteRepair");Call(guest,"Retry");Assert.AreEqual(80,Get<double>(guest,"Elapsed"));Assert.False(Get<bool>(guest,"Repaired"));
   StartEngine(host);Assert.True((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.2f)));Assert.AreEqual(3,Get<int>(guest,"Day3Stage"));Assert.AreEqual(Get<int>(host,"ObjectiveIndex"),Get<int>(guest,"ObjectiveIndex"));
  }
  [Test]public void BadStagesGraceAndOldProtocolAreRejected()
  {
   var host=New();var guest=New();Call(guest,"SetRemoteReplica",true);
   foreach(var pair in new[]{("day3Stage",(object)4),("day3GraceUntil",(object)double.NaN),("day3GraceUntil",(object)999d),("protocol",(object)"LK-Lab-Coop-v7")})
   {var snap=Call(host,"Capture",.02f);snap.GetType().GetField(pair.Item1).SetValue(snap,pair.Item2);Assert.False((bool)Call(guest,"ApplyRemote",snap));}
  }
  [Test]public void DaysOneAndTwoAndDisabledDayThreeKeepTheirExistingRules()
  {
   foreach(int day in new[]{1,2}){var enabled=New(day);tuning.day3Balance=false;var disabled=New(day);tuning.day3Balance=true;Assert.AreEqual(Get<double>(enabled,"Duration"),Get<double>(disabled,"Duration"));Assert.AreEqual(Get<object>(enabled,"Power"),Get<object>(disabled,"Power"));Assert.AreEqual(1,Get<float>(enabled,"FloodRiseMultiplier"));Assert.Zero(Get<int>(enabled,"Day3Stage"));Tick(enabled,12);Tick(disabled,12);Assert.AreEqual(Get<double>(enabled,"BeaconDarkElapsed"),Get<double>(disabled,"BeaconDarkElapsed"));}
   tuning.day3Balance=false;var old=New();Assert.AreEqual(315,Get<double>(old,"Duration"));Assert.AreEqual("Off",Get<object>(old,"Power").ToString());Assert.Zero(Get<int>(old,"Day3Stage"));
  }
 }
}
