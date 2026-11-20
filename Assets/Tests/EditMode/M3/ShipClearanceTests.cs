using NUnit.Framework;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Tests.M3 {
 public sealed class ShipClearanceTests {
  static NightShipEvent Ship(bool tutorial=false)=>new NightShipEvent{id="test",nameKey="test",sightedAtSeconds=0,approachSeconds=10,windowSeconds=10,darkGraceSeconds=4,safePreview=tutorial,aimingTutorial=tutorial};
  [Test]public void ShipsWaitForRadioWithoutStartingDarkTimerThenDepart(){
   var h=new ShipPassageState(new[]{Ship()},0,true,.6f,true);
   h.Tick(0,200,false,false);Assert.False(h.ClearanceGranted);Assert.AreEqual(-1,h.TrackingShip(200));Assert.False(h.AnyWindow(200));Assert.False(h.Lost);Assert.That(h.DepartureWait,Is.EqualTo(200).Within(.001));Assert.That(h.VisualAge(0,200),Is.EqualTo(0).Within(.001));
   h.Tick(200,201,false,true);Assert.True(h.ClearanceGranted);Assert.AreEqual(0,h.TrackingShip(201));h.Tick(201,211,true,true);Assert.True(h.AnyWindow(211));h.Tick(211,222,true,true);Assert.True(h.Finished);
  }
  [Test]public void LateJoinKeepsWaitClearanceAndReadonlyAuthority(){
   var h=new ShipPassageState(new[]{Ship()},0,true,.6f,true);var g=new ShipPassageState(new[]{Ship()},0,true,.6f,true){ReadOnly=true};
   h.Tick(0,180,false,false);Assert.True(g.Apply(h.Capture(),180));Assert.False(g.Tick(180,200,true,true));Assert.False(g.ClearanceGranted);
   h.Tick(180,181,true,true);Assert.True(g.Apply(h.Capture(),181));Assert.True(g.ClearanceGranted);Assert.AreEqual(h.WindowStart(0),g.WindowStart(0));
  }
  [Test]public void TutorialRequiresAimingButCannotWreckOrConsumeLearningBudget(){
   var h=new ShipPassageState(new[]{Ship(true)},0,true,.6f,true);h.Tick(0,100,false,false);h.Tick(100,180,false,true);
   Assert.False(h.Finished);Assert.False(h.Lost);Assert.That(h.TutorialDelay,Is.GreaterThan(69));Assert.False(h.HoldForCrew(180,181,45,100));
   var g=new ShipPassageState(new[]{Ship(true)},0,true,.6f,true){ReadOnly=true};Assert.True(g.Apply(h.Capture(),180));h.Tick(180,192,true,true);Assert.True(h.Finished);Assert.True(g.Apply(h.Capture(),192));
  }
  [Test]public void WaitingFrameSurvivesFractionalClockAccumulation(){var h=new ShipPassageState(new[]{Ship()},0,true,.6f,true);h.Tick(0,80,false,false);var g=new ShipPassageState(new[]{Ship()},0,true,.6f,true){ReadOnly=true};Assert.True(g.Apply(h.Capture(),80));}
  [Test]public void RejectInvalidClearanceWithoutChangingGuest(){
   var h=new ShipPassageState(new[]{Ship()},0,true,.6f,true);var g=new ShipPassageState(new[]{Ship()},0,true,.6f,true){ReadOnly=true};h.Tick(0,100,false,false);
   foreach(var bad in new[]{double.NaN,-1d,101d}){var f=h.Capture();f.departureWait=bad;Assert.False(g.Apply(f,100));}
   var premature=h.Capture();premature.entered=1;Assert.False(g.Apply(premature,100));Assert.Zero(g.DepartureWait);
  }
 }
}
