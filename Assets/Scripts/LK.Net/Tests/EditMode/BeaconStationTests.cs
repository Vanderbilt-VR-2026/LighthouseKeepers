using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class BeaconStationTests
 {
  S4M2Tuning tuning;BeaconStationState beacon;
  [SetUp] public void Setup(){tuning=ScriptableObject.CreateInstance<S4M2Tuning>();beacon=new BeaconStationState(tuning);beacon.Reset(false,.18f,.5f,.3f,.001f);}
  [TearDown] public void Cleanup(){Object.DestroyImmediate(tuning);}
  [Test] public void PowerIgnitionAndAlignmentAreAllRequired()
  {Assert.IsFalse(beacon.Ignite(true,false));Assert.IsFalse(beacon.Shining(true));beacon.Ignite(true,true);Assert.IsFalse(beacon.Shining(true));beacon.Align(.5f);Assert.IsTrue(beacon.Shining(true));Assert.IsFalse(beacon.Shining(false));beacon.Align(.6f);Assert.IsFalse(beacon.Shining(true));}
  [Test] public void SootDimsAndMovementCleansButStationaryRagDoesNot()
  {beacon.Ignite(true,true);beacon.Align(.5f);float before=beacon.Brightness;beacon.Tick(60,true);Assert.Less(beacon.Brightness,before);float soot=beacon.Soot;beacon.Wipe(0);Assert.AreEqual(soot,beacon.Soot);for(int i=0;i<10;i++)beacon.Wipe(.1f);Assert.AreEqual(0,beacon.Soot);Assert.AreEqual(1,beacon.Brightness);}
  [Test] public void ReplicaAndInvalidAlignmentCannotMutateState()
  {Assert.IsFalse(beacon.Align(float.NaN));Assert.IsFalse(beacon.Align(2));beacon.ReadOnly=true;Assert.IsFalse(beacon.Ignite(true,true));Assert.IsFalse(beacon.Align(.5f));beacon.Wipe(1);beacon.Tick(10,true);Assert.AreEqual(.3f,beacon.Soot);beacon.ApplyReplica(true,.5f,.5f,.1f);Assert.IsTrue(beacon.Shining(true));}
 }
}
