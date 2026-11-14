using LighthouseKeepers.Core.Progression;
using NUnit.Framework;
namespace LighthouseKeepers.Tests.M3
{
 public sealed class KeeperFearTests
 {
  [Test] public void SourcesRaiseFearButAbsentThreatAndDistantDryRoomsDoNot()
  {
   Assert.Zero(KeeperFearState.Target(new FearInput()));
   Assert.Greater(KeeperFearState.Target(new FearInput{dark=true,isolation=1}),KeeperFearState.Target(new FearInput{dark=true}));
   Assert.Greater(KeeperFearState.Target(new FearInput{water=1}),.6f);Assert.Greater(KeeperFearState.Target(new FearInput{shipDanger=1}),.6f);
   Assert.AreEqual(1,KeeperFearState.Target(new FearInput{threatPresent=true,threatVisible=true,threatDistance=0}));
   Assert.Less(KeeperFearState.Target(new FearInput{threatPresent=true,threatVisible=false,threatDistance=2}),KeeperFearState.Target(new FearInput{threatPresent=true,threatVisible=true,threatDistance=2}));
   Assert.Zero(KeeperFearState.Target(new FearInput{water=float.NaN,shipDanger=float.NaN,threatPresent=true,threatDistance=float.NaN}));
  }
  [Test] public void PerKeeperValuesFollowActorNotSlotAndDisconnectClearsGuest()
  {
   var s=new KeeperFearState();var scary=new FearInput{water=1,hidden=true,holdingBreath=true};s.Observe(8,11,scary,22,new FearInput());float fear=s.ForActor(11);Assert.Greater(fear,.7f);Assert.Zero(s.ForActor(22));Assert.True(s.HoldingBreath(11));
   s.Observe(0,22,new FearInput(),11,scary);Assert.AreEqual(fear,s.ForActor(11));Assert.Zero(s.ForActor(22));
   s.Observe(0,11,scary,0,new FearInput());Assert.Zero(s.ForActor(22));Assert.True(s.Capture().Valid());
  }
  [Test] public void HostSmoothsRecoveryGuestOnlyAppliesAndInvalidSnapshotsAreRejected()
  {
   var host=new KeeperFearState();host.Observe(2,1,new FearInput{water=1},2,new FearInput());float first=host.ForActor(1);Assert.Greater(first,.5f);Assert.Less(first,.8f);
   var guest=new KeeperFearState{ReadOnly=true};var f=host.Capture();Assert.True(guest.Apply(f));guest.Observe(100,1,new FearInput(),0,new FearInput());Assert.AreEqual(first,guest.ForActor(1));f.first=0;Assert.AreEqual(first,guest.ForActor(1));
   host.Observe(1,1,new FearInput(),0,new FearInput());Assert.Less(host.ForActor(1),first);Assert.Greater(host.ForActor(1),0);
   f=host.Capture();f.first=float.NaN;Assert.False(guest.Apply(f));f=host.Capture();f.secondActor=f.firstActor;Assert.False(guest.Apply(f));Assert.False(host.Apply(host.Capture()));
  }
  [Test] public void BreathRequiresHidingAndComfortEndpointsAreBounded()
  {
   var s=new KeeperFearState();s.Observe(1,0,new FearInput{holdingBreath=true},0,new FearInput());Assert.False(s.HoldingBreath(0));
   Assert.AreEqual(62,KeeperFearState.BeatsPerMinute(0));Assert.AreEqual(140,KeeperFearState.BeatsPerMinute(1));Assert.AreEqual(.03f,KeeperFearState.HapticAmplitude(0),.0001f);Assert.AreEqual(.18f,KeeperFearState.HapticAmplitude(1),.0001f);
   Assert.AreEqual(22000,KeeperFearState.WorldCutoff(.6f));Assert.AreEqual(6000,KeeperFearState.WorldCutoff(1),.01f);
  }
 }
}
