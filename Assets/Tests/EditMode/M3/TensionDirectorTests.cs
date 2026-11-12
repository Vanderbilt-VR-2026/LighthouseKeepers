using System;
using System.Collections.Generic;
using LighthouseKeepers.Core.Progression;
using NUnit.Framework;
namespace LighthouseKeepers.Tests.M3
{
 public sealed class TensionDirectorTests
 {
  NightScenario Night()=>new NightScenario{openingSeconds=45,reliefSeconds=15,faultSpacingSeconds=12,stormBaseStage=1,peakStormAfterFraction=1};
  [Test] public void RecoveryAndMinimumGapAreOneSharedScheduleGate()
  {
   var n=Night();n.faults=new[]{new NightFault{id="pipe",stationId="pipe"},new NightFault{id="engine",stationId="engine",atSeconds=75},new NightFault{id="radio",stationId="radio",atSeconds=75}};
   var t=new TensionDirector(n);var s=new NightSchedule(n,t);var fired=new List<string>();
   s.Advance(0,true,_=>false,f=>fired.Add(f.id));s.Completed(70);
   s.Advance(75,true,_=>false,f=>fired.Add(f.id));Assert.AreEqual(1,fired.Count);
   s.Advance(85,true,_=>false,f=>fired.Add(f.id));Assert.AreEqual(2,fired.Count);
   s.Advance(99.99,true,_=>false,f=>fired.Add(f.id));Assert.AreEqual(2,fired.Count);
   s.Advance(100,true,_=>false,f=>fired.Add(f.id));Assert.True(s.Finished);
   Assert.AreEqual(t.NextProblemAt,s.NextFaultAt);Assert.AreEqual(t.ReliefUntil,s.ReliefUntil);
  }
  [TestCase(0,10)][TestCase(15,15)][TestCase(99,20)] public void ReliefIsBoundedAndDropsStormAndResolvesMusic(float authored,float expected)
  {
   var n=Night();n.reliefSeconds=authored;var t=new TensionDirector(n);t.Tick(300,300,600,.8f,.8f,true,19,false,true);Assert.AreEqual(3,t.StormStage);Assert.AreEqual(2,t.MusicLayer);
   t.Fixed(300);t.Tick(300,300,600,.8f,.8f,true,19,false,true);Assert.AreEqual(300+expected,t.ReliefUntil);Assert.AreEqual(2,t.StormStage);Assert.AreEqual(0,t.MusicLayer);Assert.AreEqual(TensionPhase.Relief,t.Phase);
   t.Tick(321,279,600,.8f,.8f,true,19,false,true);Assert.AreEqual(3,t.StormStage);Assert.AreEqual(2,t.MusicLayer);
  }
  [Test] public void ScareRequiresSilenceDarknessEscapeAndNoTutorialAtBothEnds()
  {
   var t=new TensionDirector(Night());Assert.False(t.PrepareScare(59,false,true,true,true));Assert.False(t.PrepareScare(61,true,true,true,true));Assert.False(t.PrepareScare(61,false,false,true,true));Assert.False(t.PrepareScare(61,false,true,false,true));
   Assert.True(t.PrepareScare(61,false,true,true,true));Assert.True(t.Quiet(62));Assert.False(t.CanProblem(62));Assert.False(t.LaunchScare(63,false,true,true));Assert.False(t.LaunchScare(64,false,false,true));Assert.False(t.Quiet(64));
   Assert.True(t.PrepareScare(65,false,true,true,true));Assert.True(t.LaunchScare(67.5,false,true,true));t.Tick(67.5,500,600,.1f,.1f,true,40,false,true);Assert.AreEqual(TensionPhase.Hunt,t.Phase);Assert.AreEqual(1,t.Value);Assert.AreEqual(2,t.MusicLayer);
   t.Retreat(70);t.Tick(70,500,600,.1f,.1f,false,40,false,true);Assert.AreEqual(TensionPhase.Relief,t.Phase);
  }
  [Test] public void FixCancelsPreparedScareAndNightOneNeverScares()
  {
   var n=Night();var t=new TensionDirector(n);Assert.True(t.PrepareScare(65,false,true,true,true));t.Fixed(66);Assert.False(t.LaunchScare(69,false,true,true));
   n.noFail=true;t=new TensionDirector(n);Assert.False(t.PrepareScare(100,false,true,true,false));
  }
  [Test] public void OnlyAuthoredClimaxCanBypassPacing()
  {
   var n=Night();var t=new TensionDirector(n);t.Problem(80);t.Fixed(81);Assert.False(t.CanProblem(82,true));n.finaleFaultPileup=true;t=new TensionDirector(n);t.Problem(80);t.Fixed(81);Assert.True(t.CanProblem(82,true));
  }
  [Test] public void ReplicaNeverAuthorsAndRejectsPoisonedOrAliasedFrames()
  {
   var t=new TensionDirector(Night());t.Problem(75);t.Tick(75,525,600,.2f,.1f,true,50,false,true);var guest=new TensionDirector(Night()){ReadOnly=true};var f=t.Capture();Assert.True(guest.Apply(f,75));f.value=.99f;Assert.AreNotEqual(.99f,guest.Value);
   guest.Fixed(80);guest.Problem(80);guest.Tick(80,1,600,1,1,true,0,false,true);Assert.AreEqual(90,guest.NextProblemAt);Assert.False(guest.PrepareScare(100,false,true,true,true));
   f=t.Capture();f.value=float.NaN;Assert.False(guest.Apply(f,75));f=t.Capture();f.quietUntil=999;Assert.False(guest.Apply(f,75));Assert.False(t.Apply(t.Capture(),75));
  }
 }
}
