using System;
using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Tests.M3
{
 public sealed class HunterRulesTests
 {
  HorrorTuning t;HunterState hunter;
  HunterObservation At(bool visible=true,bool light=false,bool watched=false,bool hidden=false,bool valid=true,bool escape=true,bool path=true,float distance=3,bool tutorial=false)=>new(1,valid,visible,path,escape,new KeeperExposure(valid,watched,light,false,hidden),distance,protectedStep:tutorial);
  [SetUp] public void Setup(){t=ScriptableObject.CreateInstance<HorrorTuning>();t.hunterEnabled=true;hunter=new HunterState(t,3);}
  [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(t);}
  void Chase(){Assert.True(hunter.BeginSabotage(60,At(),true));hunter.Tick(63,At());Assert.AreEqual(HunterPhase.Stalk,hunter.Phase);hunter.Tick(64,At());Assert.AreEqual(HunterPhase.Telegraph,hunter.Phase);hunter.Tick(66,At());Assert.AreEqual(HunterPhase.Quiet,hunter.Phase);hunter.Tick(68.5,At());Assert.AreEqual(HunterPhase.Chase,hunter.Phase);}
  [Test] public void ShippingFeatureFlagStaysOffUntilLiveEscapeIntegrationPasses(){Assert.False(HorrorTuning.Current.hunterEnabled);Assert.False(HorrorTuning.Current.catchesEnabled);t.hunterEnabled=false;Assert.False(hunter.BeginSabotage(60,At(),true));}
  [Test] public void FullTelegraphAndSilencePrecedeMovement(){Chase();Assert.AreEqual(3.4f,hunter.Speed);}
  [Test] public void LargeClockJumpCannotSkipBothWarningBeats(){hunter.BeginSabotage(60,At(),true);hunter.Tick(63,At());hunter.Tick(64,At());hunter.Tick(90,At());Assert.AreEqual(HunterPhase.Quiet,hunter.Phase);Assert.Zero(hunter.Speed);hunter.Tick(92.49,At());Assert.AreEqual(HunterPhase.Quiet,hunter.Phase);}
  [TestCase(1)][TestCase(2)] public void TutorialNightsNeverHunt(int night){hunter=new HunterState(t,night);Assert.False(hunter.BeginSabotage(200,At(),true));}
  [Test] public void FirstMinuteAndLowScaresBlockStart(){Assert.False(hunter.BeginSabotage(59.99,At(),true));Assert.False(hunter.BeginSabotage(70,At(),false));}
  [Test] public void UnknownAndUnreachableTargetsFailSafe(){Assert.False(hunter.BeginSabotage(70,At(valid:false),true));Assert.False(hunter.BeginSabotage(70,At(path:false),true));Assert.False(hunter.BeginSabotage(70,At(escape:false),true));Assert.False(hunter.BeginSabotage(70,At(tutorial:true),true));}
  [Test] public void RoomLightOrPartnersActualEyesPreventPursuit(){Assert.False(hunter.BeginSabotage(70,At(light:true),true));Assert.False(hunter.BeginSabotage(70,At(watched:true),true));Chase();hunter.Tick(69,At(watched:true));Assert.AreEqual(HunterPhase.Retreat,hunter.Phase);Assert.False(hunter.CatchRequested);}
  [Test] public void SoloDarknessNeedsNoCompanion(){var solo=new HunterObservation(0,true,true,true,true,new KeeperExposure(true,false,false,false),2);Assert.True(hunter.BeginSabotage(60,solo,true));}
  [Test] public void HighLanternProtectionCancelsPursuit(){Chase();hunter.Tick(69,new HunterObservation(1,true,true,true,true,new KeeperExposure(true,false,false,true),.1f));Assert.AreEqual(HunterPhase.Retreat,hunter.Phase);}
  [Test] public void UnseenHidingMakesItSearchThenLeave(){Chase();hunter.Tick(69,At(visible:false,hidden:true));Assert.AreEqual(HunterPhase.Search,hunter.Phase);hunter.Tick(80.9,At(visible:false,hidden:true));Assert.AreEqual(HunterPhase.Search,hunter.Phase);hunter.Tick(81,At(visible:false,hidden:true));Assert.AreEqual(HunterPhase.Retreat,hunter.Phase);}
  [Test] public void SearchReacquisitionWarnsAgainWithoutConsumingAnotherHunt(){Chase();hunter.Tick(69,At(visible:false));Assert.AreEqual(HunterPhase.Search,hunter.Phase);hunter.Tick(70,At());Assert.AreEqual(HunterPhase.Telegraph,hunter.Phase);Assert.AreEqual(1,hunter.Hunts);Assert.Zero(hunter.Speed);}
  [Test] public void CatchStaysDisabledUntilRecoveryIsVerified(){Chase();hunter.Tick(69,At(distance:.1f));Assert.False(hunter.CatchRequested);Assert.AreEqual(HunterPhase.Chase,hunter.Phase);}
  [Test] public void EnabledCatchIsOneRequestFollowedByRetreat(){t.catchesEnabled=true;Chase();hunter.Tick(69,At(distance:.1f));Assert.True(hunter.CatchRequested);Assert.AreEqual(HunterPhase.Retreat,hunter.Phase);hunter.Tick(70,At(distance:.1f));Assert.False(hunter.CatchRequested);}
  [Test] public void DefensesHoldRetreatForTheirConfiguredWindow(){Chase();Assert.True(hunter.Repel(69,t.flareRetreat));hunter.Tick(128.9,At());Assert.AreEqual(HunterPhase.Retreat,hunter.Phase);hunter.Tick(129,At());Assert.AreEqual(HunterPhase.Dormant,hunter.Phase);Assert.False(hunter.BeginSabotage(130,At(),true));}
  [Test] public void ReplicaCannotAdvanceOrRetargetAndRejectsInvalidPose(){Chase();var copy=new HunterState(t,3){ReadOnly=true};Assert.True(copy.Apply(hunter.Capture(),69));copy.Tick(100,At());Assert.AreEqual(HunterPhase.Chase,copy.Phase);Assert.False(copy.Repel(69,20));Assert.False(copy.SetPose(1,2,3,0,69));var bad=hunter.Capture();bad.x=float.NaN;Assert.False(copy.Apply(bad,69));Assert.False(hunter.Apply(hunter.Capture(),69));}
  [Test] public void InvalidClockAndPoseCannotCorruptState(){Chase();hunter.Tick(double.NaN,At());hunter.Tick(20,At());Assert.AreEqual(HunterPhase.Chase,hunter.Phase);Assert.False(hunter.SetPose(float.NaN,0,0,0,69));Assert.False(hunter.Repel(double.PositiveInfinity,20));}
 }
}
