using LighthouseKeepers.Core.Simulation;
using LighthouseKeepers.Simulation.Unity;
using NUnit.Framework;
using UnityEditor;

namespace LighthouseKeepers.Tests.Simulation
{
    public sealed class FloodBalanceTests
    {
        FloodParameters tuning;

        [SetUp]
        public void LoadPublishedTuning()
        {
            var asset = AssetDatabase.LoadAssetAtPath<FloodTuning>("Assets/Data/Tuning/FloodTuning.asset");
            Assert.That(asset, Is.Not.Null, "Tests must exercise the published asset, not copied defaults.");
            tuning = asset.ToParameters();
        }

        SimulationHost Create(double volume = 0, double? plinth = 3.5) =>
            new SimulationHost(tuning, new FixedSimulationGeometry(0, plinth), new LocalAuthority(), volume);

        [Test]
        public void MaximumLegalManualPumpingBuysTimeButNeverBeatsLeakOver120Seconds()
        {
            double throughput = tuning.ManualPumpStrokeM3 / tuning.ManualPumpMinStrokeSeconds;
            Assert.That(throughput / tuning.LeakInflowM3PerSec, Is.InRange(.85, .95));
            var idle = Create();
            var pumped = Create();
            // The first stroke follows a full interval: a dry-start stroke must not be banked.
            double nextStroke = tuning.ManualPumpMinStrokeSeconds;
            for (int tick = 0; tick < 1200; tick++)
            {
                if (tick * SimulationHost.TickSeconds + 1e-6 >= nextStroke)
                {
                    pumped.Enqueue(new SimCommand(SimCommandKind.ManualPumpStroke, actorId: 1));
                    nextStroke += tuning.ManualPumpMinStrokeSeconds;
                }
                idle.Advance(SimulationHost.TickSeconds);
                pumped.Advance(SimulationHost.TickSeconds);
            }
            Assert.That(pumped.Current.WaterVolumeM3, Is.GreaterThan(0));
            Assert.That(pumped.Current.WaterVolumeM3, Is.LessThan(idle.Current.WaterVolumeM3 * .20));
            Assert.That(pumped.Current.LeakActive, Is.True);
            Assert.That(pumped.Current.Phase, Is.EqualTo(RunPhase.Running));
        }

        [Test]
        public void RepairedLeakAndElectricPumpRecoverFromFailHeightInUnder90Seconds()
        {
            var host = Create(tuning.FailHeightM * tuning.FloorAreaM2);
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak));
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator));
            host.Enqueue(new SimCommand(SimCommandKind.SetElectricPump, value: 1));
            double elapsed = 0;
            while (host.Current.Phase == RunPhase.Running && elapsed < 90)
            {
                host.Advance(SimulationHost.TickSeconds);
                elapsed += SimulationHost.TickSeconds;
            }
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Succeeded));
            Assert.That(host.Current.WaterHeightM, Is.LessThanOrEqualTo(tuning.SafeHeightM));
            Assert.That(elapsed, Is.LessThan(90));
            double manualRate = tuning.ManualPumpStrokeM3 / tuning.ManualPumpMinStrokeSeconds;
            Assert.That(tuning.ElectricPumpM3PerSec / manualRate, Is.InRange(2.8, 3.3));
        }

        [Test]
        public void DryIdleRunFailsBetweenThreeAndAHalfAndFourAndAHalfMinutes()
        {
            var host = Create();
            while (host.Current.Phase == RunPhase.Running && host.Current.Tick < 3000)
                host.Advance(SimulationHost.TickSeconds);
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Failed));
            Assert.That(host.Current.Tick * SimulationHost.TickSeconds, Is.InRange(210, 270));
            Assert.That(host.Current.WaterHeightM, Is.GreaterThan(tuning.FailHeightM));
        }

        [Test]
        public void RecoverableVolumePowerLeakGridCanReachNegativeNetRate()
        {
            // At zero power optimal play repairs first, then pumps manually. A flooded
            // generator is not a soft lock while the repair and hand pump remain available.
            foreach (double height in new[] { 0, .05, .30, .89, .95, 3.51 })
            foreach (bool power in new[] { false, true })
            foreach (bool leak in new[] { false, true })
            {
                var host = Create(height * tuning.FloorAreaM2, plinth: .92);
                if (power)
                {
                    host.Enqueue(new SimCommand(SimCommandKind.StartGenerator));
                    host.Enqueue(new SimCommand(SimCommandKind.SetElectricPump, value: 1));
                }
                if (!leak) host.Enqueue(new SimCommand(SimCommandKind.RepairLeak));
                host.Advance(SimulationHost.TickSeconds);
                if (host.Current.Phase == RunPhase.Succeeded) continue;
                double before = host.Current.WaterVolumeM3;
                host.Enqueue(new SimCommand(SimCommandKind.RepairLeak));
                host.Enqueue(new SimCommand(SimCommandKind.ManualPumpStroke, actorId: 1));
                host.Advance(SimulationHost.TickSeconds);
                if (before == 0)
                {
                    // The volume floor forbids negative flow from an already dry room.
                    // Repairing at this boundary must instead finish successfully.
                    Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Succeeded));
                    Assert.That(host.Current.WaterVolumeM3, Is.Zero);
                    continue;
                }
                Assert.That(host.Current.WaterVolumeM3, Is.LessThan(before),
                    $"Unrecoverable height={height}, requested power={power}, leak={leak}");
                Assert.That(host.Current.InflowM3PerSec - host.Current.OutflowM3PerSec,
                    Is.LessThan(0), "Optimal available repair/pump action must give negative net flow.");
            }
        }
    }
}
