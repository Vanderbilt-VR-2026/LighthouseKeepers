using NUnit.Framework;
using LighthouseKeepers.Audio;

namespace LighthouseKeepers.Tests
{
    public sealed class FootstepCadenceTests
    {
        [TestCase(30)]
        [TestCase(72)]
        [TestCase(120)]
        public void CadenceDependsOnDistanceAtDifferentFrameRates(int fps)
        {
            var cadence = new FootstepCadence();
            int steps = 0;
            for (int i = 0; i < fps * 5; i++)
                if (cadence.Advance(1.6f / fps, 1f / fps, true, .65f)) steps++;
            Assert.AreEqual(12, steps, "Eight metres should produce twelve .65m strides.");
        }

        [Test]
        public void StandingOrPushingIntoWallDoesNotProduceSteps()
        {
            var cadence = new FootstepCadence();
            for (int i = 0; i < 1000; i++)
                Assert.IsFalse(cadence.Advance(.0001f, 1f / 72, true, .65f));
        }

        [Test]
        public void FallingTeleportingAndPausingResetPartialStride()
        {
            foreach (var interruption in new[] { "air", "teleport", "pause", "hitch", "stop" })
            {
                var cadence = new FootstepCadence();
                for (int i = 0; i < 25; i++) Assert.IsFalse(cadence.Advance(.02f, .02f, true, .65f));
                Assert.IsFalse(cadence.Advance(interruption == "teleport" ? 10 : interruption == "stop" ? 0 : .02f,
                    interruption == "pause" ? 0 : interruption == "hitch" ? 1 : .02f, interruption != "air", .65f));
                for (int i = 0; i < 10; i++) Assert.IsFalse(cadence.Advance(.02f, .02f, true, .65f), interruption);
            }
        }
    }
}
