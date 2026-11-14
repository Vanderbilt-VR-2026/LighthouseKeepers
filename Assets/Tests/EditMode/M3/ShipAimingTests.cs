using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Tests.M3 {
 public sealed class ShipAimingTests {
  static NightShipEvent Event(bool preview=false)=>new NightShipEvent{id="aim",nameKey="Mary Ellen",sightedAtSeconds=10,approachSeconds=10,windowSeconds=10,darkGraceSeconds=5,safePreview=preview};
  [Test] public void PoweredButOffTargetStopsTravelAndWrecks(){var s=new ShipPassageState(new[]{Event()},0,true);s.TickGuided(0,24,(i,t)=>false,false);Assert.False(s.Lost);Assert.That(s.VisualAge(0,24),Is.EqualTo(10).Within(.01));s.TickGuided(24,26,(i,t)=>false,false);Assert.True(s.Lost);Assert.False(s.Finished);}
  [Test] public void ReacquiringBeamResumesAfterSteadyHoldAndNeverRewinds(){var s=new ShipPassageState(new[]{Event()},0,true);s.Tick(0,22,false,false);double at=s.VisualAge(0,22);s.Tick(22,22.3,true,false);Assert.That(s.VisualAge(0,22.3),Is.EqualTo(at).Within(.02));s.Tick(22.3,23,true,false);Assert.Greater(s.VisualAge(0,23),at);s.Tick(23,34,true,false);Assert.True(s.Finished);Assert.False(s.Lost);}
  [Test] public void SafePreviewDoesNotRequireSteering(){var s=new ShipPassageState(new[]{Event(true)},0,true);s.Tick(0,31,false,false);Assert.True(s.Finished);Assert.Zero(s.Capture().delay[0]);}
  [Test] public void RouteAndWheelBearingsMatchForEveryApproachAndPassagePoint(){for(int t=0;t<150;t++){var p=ShipNavigation.Route(t,75,55,55).position;float wheel=ShipNavigation.WheelFor(p);Assert.True(ShipNavigation.Aimed(wheel,p,8));Assert.False(ShipNavigation.Aimed(Mathf.Clamp01(wheel+.1f),p,8));}}
  [Test] public void StationaryWheelCannotGuideWholePassage(){var s=new ShipPassageState(new[]{Event()},0,true);s.TickGuided(0,40,(i,t)=>ShipNavigation.Aimed(.5f,ShipNavigation.Route((float)s.VisualAge(i,t),(float)s.Approach(i),10,55).position,8),false);Assert.True(s.Lost);Assert.False(s.Finished);}
  [Test] public void ContinuouslyTrackingWheelAllowsPassAndGuestSeesExactProgress(){var h=new ShipPassageState(new[]{Event()},0,true);var g=new ShipPassageState(new[]{Event()},0,true){ReadOnly=true};for(int k=0;k<340;k++){double a=k*.1,b=a+.1;h.TickGuided(a,b,(i,t)=>{var p=ShipNavigation.Route((float)h.VisualAge(i,t),(float)h.Approach(i),10,55).position;return ShipNavigation.Aimed(ShipNavigation.WheelFor(p),p,8);},false);Assert.True(g.Apply(h.Capture(),b));Assert.AreEqual(h.PassedMask,g.PassedMask);Assert.AreEqual(h.VisualAge(0,b),g.VisualAge(0,b));}Assert.True(h.Finished);Assert.False(g.Tick(34,35,false,false));}
  [Test] public void DelayedFirstShipDoesNotForceSimultaneousTracking(){var a=Event();var b=Event();b.sightedAtSeconds=21;var s=new ShipPassageState(new[]{a,b},0,true);s.Tick(0,22,false,false);Assert.Greater(s.WindowStart(1),31);s.Tick(22,35,true,false);Assert.AreEqual(1,s.TrackingShip(35));}
  [Test] public void CorruptDelayIsRejectedAndSnapshotIsDeepCopied(){var h=new ShipPassageState(new[]{Event()},0,true);h.Tick(0,22,false,false);var g=new ShipPassageState(new[]{Event()},0,true){ReadOnly=true};var f=h.Capture();f.delay[0]=float.NaN;Assert.False(g.Apply(f,22));f=h.Capture();f.delay[0]=50;Assert.False(g.Apply(f,22));Assert.Less(h.Capture().delay[0],3);}
 }
}
