using NUnit.Framework;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Tests.M3
{
    public sealed class ShipPassageTests
    {
        static NightShipEvent Event(bool preview = false) => new NightShipEvent { id = "test", nameKey = "Mary Ellen", sightedAtSeconds = 10, approachSeconds = 75, windowSeconds = 55, darkGraceSeconds = 30, broadcastExtensionSeconds = 20, safePreview = preview };
        [Test] public void SafePassHasSightWindowThenOutcomeAndCannotRepeat()
        {
            var s = new ShipPassageState(new[] { Event() });
            s.Tick(0, 9, true, false); Assert.Zero(s.SeenMask); Assert.False(s.Finished);
            Assert.True(s.Tick(9, 10, true, false)); Assert.AreEqual(1, s.SeenMask);
            s.Tick(10, 85, true, false); Assert.True(s.AnyWindow(85));
            s.Tick(85, 139, true, false); Assert.False(s.Finished);
            Assert.True(s.Tick(139, 140, true, false)); Assert.True(s.Finished);
            Assert.False(s.Tick(140, 150, false, false)); Assert.False(s.Lost);
        }
        [Test] public void SustainedDarknessWrecksAtExactGraceAndStopsOutcomes()
        {
            var s = new ShipPassageState(new[] { Event() });
            s.Tick(0, 100, false, false); Assert.AreEqual(15, s.DarkRemaining(0));
            s.Tick(100, 120, false, false); Assert.True(s.Lost); Assert.AreEqual(115, s.WreckAt);
            Assert.AreEqual(0, s.WreckIndex); Assert.False(s.Finished); Assert.False(s.Tick(120, 200, true, false));
        }
        [Test] public void LightRestoredWithinGraceAllowsRecoveryAndNamedPass()
        {
            var s = new ShipPassageState(new[] { Event() });
            s.Tick(0, 110, false, false); Assert.AreEqual(5, s.DarkRemaining(0));
            s.Tick(110, 111, true, false); Assert.AreEqual(30, s.DarkRemaining(0));
            s.Tick(111, 139, false, false); Assert.False(s.Lost);
            s.Tick(139, 140, true, false); Assert.True(s.Finished);
        }
        [Test] public void TutorialShipPassesEvenWithNoBeacon()
        { var s = new ShipPassageState(new[] { Event(true) }); s.Tick(0, 200, false, false); Assert.True(s.Finished); Assert.False(s.Lost); Assert.Zero(s.Danger); }
        [Test] public void WarningBroadcastSlowsApproachButDoesNotChangeAnEnteredWindow()
        {
            var s = new ShipPassageState(new[] { Event() }); s.Tick(0, 85, true, true);
            Assert.AreEqual(105, s.WindowStart(0)); Assert.False(s.AnyWindow(85));
            s.Tick(85, 160, true, true); Assert.True(s.Finished);
            var late = new ShipPassageState(new[] { Event() }); late.Tick(0, 90, true, false); late.Tick(90, 91, true, true); Assert.AreEqual(85, late.WindowStart(0));
        }
        [Test] public void GuestsOnlyConsumeHostFramesAndCannotMutateThem()
        {
            var h = new ShipPassageState(new[] { Event() }); h.Tick(0, 100, false, false);
            var g = new ShipPassageState(new[] { Event() }) { ReadOnly = true };
            Assert.True(g.Apply(h.Capture(), 100)); Assert.AreEqual(h.DarkRemaining(0), g.DarkRemaining(0));
            Assert.False(g.Tick(100, 200, true, false)); Assert.False(g.Finished);
            var copy = h.Capture(); copy.dark[0] = 0; Assert.AreEqual(15, h.DarkRemaining(0));
        }
        [Test] public void InvalidFramesCannotInventPassesWrecksOrNanTimers()
        {
            var g = new ShipPassageState(new[] { Event() }) { ReadOnly = true };
            var h = new ShipPassageState(new[] { Event() }); h.Tick(0, 100, false, false);
            var frame = h.Capture(); frame.passed = 1; Assert.False(g.Apply(frame, 100));
            frame = h.Capture(); frame.dark[0] = float.NaN; Assert.False(g.Apply(frame, 100));
            frame = h.Capture(); frame.seen = 4; Assert.False(g.Apply(frame, 100));
            frame = h.Capture(); frame.wreck = 0; frame.wreckAt = 101; Assert.False(g.Apply(frame, 100));
        }
        [Test] public void LargeTickAndFrameTicksProduceTheSamePassOrWreck()
        {
            foreach (bool shining in new[] { true, false })
            {
                var a = new ShipPassageState(new[] { Event() }); var b = new ShipPassageState(new[] { Event() });
                a.Tick(0, 160, shining, false); for (int i = 0; i < 640; i++) b.Tick(i * .25, (i + 1) * .25, shining, false);
                Assert.AreEqual(a.PassedMask, b.PassedMask); Assert.AreEqual(a.WreckIndex, b.WreckIndex); Assert.AreEqual(a.WreckAt, b.WreckAt);
            }
        }
        [Test] public void AllShipsMustPassBeforeTheWatchFinishes()
        {
            var second = Event(); second.id = "second"; second.sightedAtSeconds = 160;
            var s = new ShipPassageState(new[] { Event(), second }); s.Tick(0, 140, true, false); Assert.False(s.Finished);
            s.Tick(140, 290, true, false); Assert.True(s.Finished); Assert.AreEqual(3, s.PassedMask);
        }
    }
}
