using System;
using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Tests.M3
{
 public sealed class ShipWatchIntegrationTests
 {
  DayDefinition[] days;bool old;float oldLead;
  static Type State=>Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true);
  static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
  static T Get<T>(object o,string n)=>(T)o.GetType().GetProperty(n).GetValue(o);
  [SetUp] public void Setup(){old=Sprint5Tuning.Current.shipPassages;oldLead=Sprint5Tuning.Current.shipApproachDangerSeconds;Sprint5Tuning.Current.shipApproachDangerSeconds=0;Sprint5Tuning.Current.shipPassages=true;var catalog=Resources.Load<FiveNightCatalog>("M3Campaign");days=new[]{UnityEngine.Object.Instantiate(catalog.nights[0]),UnityEngine.Object.Instantiate(catalog.nights[1])};days[0].scenario.ships=new[]{new NightShipEvent{id="test",nameKey="m3.ship.maryEllen",sightedAtSeconds=1,approachSeconds=3,windowSeconds=8,darkGraceSeconds=4,safePreview=true}};}
  [TearDown] public void Cleanup(){Sprint5Tuning.Current.shipPassages=old;Sprint5Tuning.Current.shipApproachDangerSeconds=oldLead;foreach(var d in days)UnityEngine.Object.DestroyImmediate(d);}
  object New(int day=1)=>Activator.CreateInstance(State,days,day,Resources.Load("S4Tuning"),Resources.Load("S4M2Tuning"),Resources.Load("ImmersionTuning"));
  [Test] public void CompletingPipeWaitsForPreviewThenHostAndGuestCanSeeSleepAndRetry()
  {
   var h=New();Call(h,"CompleteRepair");Assert.False(Get<bool>(h,"Complete"));Assert.False((bool)Call(h,"Sleep"));
   Call(h,"Tick",12d);Assert.True(Get<bool>(h,"Complete"));
   var g=New();Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",.1f)));Assert.True(Get<bool>(g,"Complete"));
   Assert.False((bool)Call(g,"Sleep"));Assert.True((bool)Call(h,"Sleep"));Assert.AreEqual(2,Get<int>(h,"Day"));
   Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",.1f)));Assert.AreEqual(2,Get<int>(g,"Day"));
   Call(h,"Retry");Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",0f)));Assert.Zero(Get<ShipPassageState>(g,"Ships").PassedMask);
  }
  [Test] public void WreckNamesTheShipReplicatesAndRetryClearsIt()
  {
   days[0].scenario.ships[0].safePreview=false;var h=New();Call(h,"SetCircuit",1,false);Call(h,"Tick",9d);
   Assert.True(Get<bool>(h,"Failed"));StringAssert.Contains("Mary Ellen",Get<string>(h,"FailureMessage"));
   var g=New();Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",.1f)));Assert.AreEqual(Get<string>(h,"FailureMessage"),Get<string>(g,"FailureMessage"));
   Call(h,"Retry");Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",0f)));Assert.False(Get<bool>(g,"Failed"));Assert.AreEqual(-1,Get<ShipPassageState>(g,"Ships").WreckIndex);
  }
  [Test] public void InvalidShipPayloadIsRejectedBeforeGuestStationStateChanges()
  {
   var h=New();Call(h,"Tick",5d);var g=New();Call(g,"SetRemoteReplica",true);var snap=Call(h,"Capture",0f);
   var ships=(ShipFrame)snap.GetType().GetField("ships").GetValue(snap);ships.passed=1;
   Assert.False((bool)Call(g,"ApplyRemote",snap));Assert.Zero(Get<double>(g,"Elapsed"));Assert.False(Get<bool>(g,"Repaired"));
  }
  [Test] public void AuthoredNightTwoDoesNotLoseToBeaconBeforeShipThreatButWrecksIfIgnored()
  {
   Sprint5Tuning.Current.shipApproachDangerSeconds=15;var h=New(2);Call(h,"Tick",200d);
   Assert.False(Get<bool>(h,"BeaconShining"));Assert.False(Get<bool>(h,"Failed"));Assert.Zero(Get<double>(h,"BeaconDarkElapsed"));Assert.Zero(Get<double>(h,"BeaconDarkLimit"));
   Call(h,"Tick",60d);Assert.False(Get<bool>(h,"Failed"));Assert.AreEqual(5,Get<double>(h,"BeaconDarkRemaining"));
   var g=New(2);Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",.1f)));Assert.AreEqual(5,Get<double>(g,"BeaconDarkRemaining"));
   Call(h,"Tick",6d);Assert.True(Get<bool>(h,"Failed"));Assert.False(Get<bool>(h,"BeaconDarkFailed"),"The ship outcome, not the legacy timer, owns this loss");
   StringAssert.Contains("Mary Ellen",Get<string>(h,"FailureMessage"));Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",.1f)));Assert.True(Get<bool>(g,"Failed"));
  }
 }
}
