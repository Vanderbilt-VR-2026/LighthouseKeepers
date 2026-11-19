using NUnit.Framework;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Tests.M3
{
 public sealed class LanternTests {
  [Test] public void FlashlightUsesOnlyOffAndHigh(){var s=new LanternState(unlimited:true,focusOnly:true);for(int i=0;i<10;i++){Assert.True(s.Cycle(0));Assert.AreEqual(LanternWick.High,s.Wick(0));Assert.True(s.Cycle(0));Assert.AreEqual(LanternWick.Off,s.Wick(0));}}
  [Test] public void UnlimitedTestingKeepsBothLampsFullThroughLongNightsAndStuns(){
   var s=new LanternState(unlimited:true);s.Cycle(0);s.Cycle(1);s.Cycle(1);s.Tick(86400);
   Assert.True(s.Lit(0));Assert.True(s.Lit(1));Assert.AreEqual(1,s.Oil(0));Assert.AreEqual(1,s.Oil(1));Assert.Zero(s.CanOil);Assert.False(s.Pour(0,4));
   for(int i=0;i<30;i++)Assert.True(s.Expose(1,2,true,true,true));Assert.AreEqual(1,s.Oil(1));
   var guest=new LanternState(unlimited:true){ReadOnly=true};Assert.True(guest.Apply(s.Capture()));guest.Tick(86400);Assert.AreEqual(1,guest.Oil(1));Assert.AreEqual(LanternWick.High,guest.Wick(1));Assert.False(guest.Cycle(0));
   Assert.True(s.Extinguish(1));Assert.False(s.Lit(1));Assert.True(s.Cycle(1));Assert.True(s.Lit(1));
  }
  [Test] public void MorseUsesReadableMarksGapsAndSoftEdges(){
   var s=new MorseFilament("E T",.4f);Assert.AreEqual(2,s.MarkCount);Assert.AreEqual(7.2f,s.Duration,.0001f);
   Assert.That(s.Sample(.2f),Is.EqualTo(1).Within(.0001f));Assert.Less(s.Sample(.02f),s.Sample(.10f));
   Assert.That(s.Sample(1),Is.EqualTo(.06f));Assert.That(s.Sample(3.6f),Is.EqualTo(1));Assert.That(s.Sample(4.6f),Is.EqualTo(.06f));
   Assert.AreEqual(s.Sample(.2f),s.Sample(s.Duration+.2f),.0001f);Assert.Throws<System.ArgumentOutOfRangeException>(()=>new MorseFilament("E",.1f));
   Assert.AreEqual(15,new MorseFilament("666",.43f).MarkCount);
  }
  [Test] public void WaterExtinguishesWithoutLosingOilAndReplicates(){var s=new LanternState();s.Cycle(0);s.Cycle(0);Assert.True(s.Extinguish(0));Assert.False(s.Lit(0));Assert.AreEqual(1,s.Oil(0));Assert.False(s.Extinguish(-1));var r=new LanternState{ReadOnly=true};Assert.True(r.Apply(s.Capture()));Assert.False(r.Lit(0));Assert.False(r.Extinguish(0));Assert.True(s.Cycle(0));Assert.True(s.Lit(0));}
  [Test] public void WickOffUsesNoOilAndLowLastsTwiceHigh(){var s=new LanternState();s.Tick(100);Assert.AreEqual(1,s.Oil(0));s.Cycle(0);s.Cycle(1);s.Cycle(1);s.Tick(90);Assert.AreEqual(.5f,s.Oil(0),.0001);Assert.AreEqual(0,s.Oil(1),.0001);Assert.False(s.Lit(1));s.Tick(90);Assert.False(s.Lit(0));}
  [Test] public void OneCanRefillsOnlyOneLampAndCannotBeReused(){var s=new LanternState();s.Cycle(0);s.Cycle(1);s.Tick(90);Assert.True(s.Pour(0,.5f));Assert.False(s.Pour(1,4));Assert.True(s.Pour(0,4));Assert.AreEqual(1,s.Oil(0));Assert.AreEqual(0,s.CanOil);s.Tick(30);Assert.False(s.Pour(0,4));}
  [Test] public void ReplicasCannotSpendOilChangeWickOrStun(){var s=new LanternState();s.Cycle(0);s.Cycle(0);var r=new LanternState{ReadOnly=true};Assert.True(r.Apply(s.Capture()));Assert.False(r.Cycle(0));r.Tick(900);Assert.AreEqual(1,r.Oil(0));Assert.False(r.Pour(0,4));Assert.False(r.Expose(0,2,true,true,true));}
  [Test] public void HighNeedsContinuousHeldAimAndCostsFifteenPercent(){var s=new LanternState();s.Cycle(0);s.Cycle(0);Assert.False(s.Expose(0,1,true,true,true));Assert.False(s.Expose(0,.1f,true,false,true));Assert.False(s.Expose(0,1,true,true,true));Assert.True(s.Expose(0,1,true,true,true));Assert.AreEqual(.85f,s.Oil(0),.0001);Assert.False(s.Expose(0,2,false,true,true));}
  [Test] public void LowAndEmptyCannotRepel(){var s=new LanternState();s.Cycle(0);Assert.False(s.Expose(0,2,true,true,true));s.Cycle(0);s.Tick(90);Assert.False(s.Expose(0,2,true,true,true));}
  [Test] public void InvalidFrameAndSlotAreRejectedWithoutMutation(){var r=new LanternState{ReadOnly=true};var bad=new LanternState().Capture();bad.oil0=float.NaN;Assert.False(r.Apply(bad));bad.oil0=1;bad.wick1=3;Assert.False(r.Apply(bad));bad.wick1=0;bad.refillSlot=2;Assert.False(r.Apply(bad));var s=new LanternState();Assert.False(s.Cycle(-1));Assert.False(s.Cycle(2));s.Tick(float.PositiveInfinity);Assert.AreEqual(1,s.Oil(0));}
  [Test] public void SnapshotIsCopiedAndNightStartsFullUnlit(){var s=new LanternState();var f=s.Capture();f.oil0=0;Assert.AreEqual(1,s.Oil(0));Assert.AreEqual(LanternWick.Off,s.Wick(0));Assert.AreEqual(1,s.CanOil);Assert.True(new LanternFrame().Valid());}
 }
}
