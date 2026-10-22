using System;
using LighthouseKeepers.Core.Simulation;
using LighthouseKeepers.Simulation.Unity;
using NUnit.Framework;
using UnityEditor;

namespace LighthouseKeepers.Tests.Simulation
{
    public sealed class WatchLifecycleTests
    {
        FloodParameters tuning;
        [SetUp] public void Load() => tuning = AssetDatabase.LoadAssetAtPath<FloodTuning>(
            "Assets/Data/Tuning/FloodTuning.asset").ToParameters();

        SimulationHost Create(Func<int, bool> repair = null) => new SimulationHost(tuning,
            new FixedSimulationGeometry(0, 3.75), new LocalAuthority(), canRepairPipe: repair ?? (_ => true));

        static void Begin(SimulationHost host, int day)
        { host.Enqueue(new SimCommand(SimCommandKind.BeginDay, targetId: day)); host.Advance(.1); }

        [Test] public void HealthyWatchWaitsForScheduledIncidentInsteadOfImmediatelySucceeding()
        {
            var host = Create(); Begin(host, 1); host.Advance(10);
            Assert.That(host.Current.DayIndex, Is.EqualTo(1));
            Assert.That(host.Current.LeakActive, Is.False); Assert.That(host.Current.GeneratorRunning, Is.True);
            Assert.That(host.Current.ElectricPumpRunning, Is.True); Assert.That(host.Current.PowerAvailable, Is.EqualTo(1));
            Assert.That(host.Current.WaterVolumeM3, Is.Zero); Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
        }

        [Test] public void PipeFaultRisesThenAuthorizedRepairRestoresAutomaticDrainage()
        {
            bool permitted = false; var host = Create(_ => permitted); Begin(host, 1);
            host.ActivateDayLeak(1); host.Advance(60);
            Assert.That(host.Current.LeakActive, Is.True); Assert.That(host.Current.ElectricPumpRunning, Is.False);
            Assert.That(host.Current.WaterVolumeM3, Is.GreaterThan(tuning.SafeHeightM * tuning.FloorAreaM2));
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak, actorId: 9)); host.Advance(.1);
            Assert.That(host.Current.LastRejectReason, Does.Contain("authorized")); Assert.That(host.Current.LeakActive, Is.True);
            permitted = true;
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak, actorId: 9)); host.Advance(.1);
            Assert.That(host.Current.LeakActive, Is.False); Assert.That(host.Current.ElectricPumpRunning, Is.True);
            while (host.Current.Phase == RunPhase.Running && host.Current.Tick < 1500) host.Advance(.1);
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Succeeded));
            Assert.That(host.Current.WaterHeightM, Is.LessThanOrEqualTo(tuning.SafeHeightM));
        }

        [Test] public void DayTwoGeneratorRecoveryClearsSeepWithoutAnExtraPipeRepair()
        {
            var host = Create(); Begin(host, 2); host.ForceGeneratorFault(); host.Advance(50);
            Assert.That(host.Current.GeneratorFaultActive, Is.True); Assert.That(host.Current.PowerAvailable, Is.Zero);
            Assert.That(host.Current.InflowM3PerSec, Is.EqualTo((float)tuning.GeneratorFaultSeepM3PerSec));
            Assert.That(host.Current.GeneratorRunning, Is.False); Assert.That(host.Current.ElectricPumpRunning, Is.False);
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator, actorId: 2)); host.Advance(.1);
            Assert.That(host.Current.GeneratorFaultActive, Is.False); Assert.That(host.Current.LeakActive, Is.False);
            Assert.That(host.Current.ElectricPumpRunning, Is.True);
            host.Advance(10); Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Succeeded));
        }

        [Test] public void DayThreePipeAndPowerFaultsEachRequireTheirOwnRecoveryAction()
        {
            var host = Create(); Begin(host, 3); host.ActivateDayLeak(3); host.ForceGeneratorFault(); host.Advance(20);
            Assert.That(host.Current.InflowM3PerSec, Is.EqualTo((float)tuning.LeakInflowM3PerSec));
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak, actorId: 1)); host.Advance(.1);
            Assert.That(host.Current.GeneratorFaultActive, Is.True); Assert.That(host.Current.LeakActive, Is.True);
            Assert.That(host.Current.InflowM3PerSec, Is.EqualTo((float)tuning.GeneratorFaultSeepM3PerSec));
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator, actorId: 2)); host.Advance(.1);
            Assert.That(host.Current.GeneratorFaultActive, Is.False); Assert.That(host.Current.LeakActive, Is.False);
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Succeeded));
        }

        [Test] public void RestoringPowerDoesNotRepairTheDayThreePipe()
        {
            var host = Create(); Begin(host, 3); host.ActivateDayLeak(3); host.ForceGeneratorFault(); host.Advance(1);
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator, actorId: 2)); host.Advance(5);
            Assert.That(host.Current.GeneratorFaultActive, Is.False); Assert.That(host.Current.LeakActive, Is.True);
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
        }

        [Test] public void BothFaultRatesKeepMaximumHandPumpingBelowInflow()
        {
            double manual = tuning.ManualPumpStrokeM3 / tuning.ManualPumpMinStrokeSeconds;
            foreach (double inflow in new[] { tuning.LeakInflowM3PerSec, tuning.GeneratorFaultSeepM3PerSec })
                Assert.That(manual / inflow, Is.InRange(.85, .95));
            Assert.That(tuning.GeneratorFaultSeepM3PerSec, Is.LessThan(tuning.LeakInflowM3PerSec));
        }

        [Test] public void ScheduledLeakMustMatchCurrentDayAndDuplicateActivationIsIdempotent()
        {
            var host = Create(); Begin(host, 1); host.ActivateDayLeak(3); host.Advance(.1);
            Assert.That(host.Current.LeakActive, Is.False); Assert.That(host.Current.LastRejectReason, Does.Contain("current watch"));
            host.ActivateDayLeak(1); host.Advance(.1);
            host.Enqueue(new SimCommand(SimCommandKind.SetElectricPump, value: 1)); host.Advance(.1);
            host.ActivateDayLeak(1); host.Advance(.1);
            Assert.That(host.Current.ElectricPumpRunning, Is.True, "Duplicate scheduling must not retrip an already restored pump.");
        }

        [Test] public void WatchRepairFailsClosedWithoutProgressionValidation()
        {
            var host = new SimulationHost(tuning, new FixedSimulationGeometry(0, 3.75), new LocalAuthority());
            Begin(host, 1); host.ActivateDayLeak(1); host.Advance(.1);
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak)); host.Advance(.1);
            Assert.That(host.Current.LeakActive, Is.True); Assert.That(host.Current.LastRejectReason, Does.Contain("authorized"));
        }

        [Test] public void BeginDayRequiresMeasuredAvailablePowerAndValidWatchIndex()
        {
            var unknown = new SimulationHost(tuning, new FixedSimulationGeometry(0, null), new LocalAuthority());
            Begin(unknown, 1); Assert.That(unknown.Current.DayIndex, Is.Zero);
            Assert.That(unknown.Current.LastRejectReason, Does.Contain("unavailable"));
            var host = Create(); Begin(host, 0);
            Assert.That(host.Current.DayIndex, Is.Zero); Assert.That(host.Current.LastRejectReason, Does.Contain("1 to 3"));
        }

        [Test] public void BrokenRepairValidationRejectsInputWithoutAbortingTheTick()
        {
            var host = Create(_ => throw new InvalidOperationException("Director unavailable")); Begin(host, 1);
            host.ActivateDayLeak(1); host.Advance(.1); int before = host.Current.Tick;
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak)); host.Advance(.1);
            Assert.That(host.Current.Tick, Is.EqualTo(before + 1));
            Assert.That(host.Current.LeakActive, Is.True); Assert.That(host.Current.LastRejectReason, Does.Contain("unavailable"));
        }

        [Test] public void SafeDryPipeStillRequiresRestoredPowerForWatchSuccess()
        {
            var host = Create(); Begin(host, 1); host.ActivateDayLeak(1); host.Advance(.1);
            host.Enqueue(new SimCommand(SimCommandKind.StopGenerator));
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak)); host.Advance(.1);
            Assert.That(host.Current.LeakActive, Is.False); Assert.That(host.Current.PowerAvailable, Is.Zero);
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator)); host.Advance(.1);
            Assert.That(host.Current.ElectricPumpRunning, Is.True); Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Succeeded));
        }

        [Test] public void SuccessfulWatchKeepsDrainingDuringRecoveryUntilTheFloorIsDry()
        {
            var host = Create(); Begin(host, 1); host.ActivateDayLeak(1); host.Advance(60);
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak)); host.Advance(.1);
            while (host.Current.Phase == RunPhase.Running && host.Current.Tick < 1500) host.Advance(.1);
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Succeeded));
            double atSuccess = host.Current.WaterVolumeM3;
            Assert.That(atSuccess, Is.GreaterThan(0));
            host.Advance(5);
            Assert.That(host.Current.WaterVolumeM3, Is.LessThan(atSuccess), "Safe height is not a water ceiling/freeze point.");
            Assert.That(host.Current.OutflowM3PerSec, Is.GreaterThan(0)); Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Succeeded));
            host.Advance(30);
            Assert.That(host.Current.WaterVolumeM3, Is.Zero); Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Succeeded));
            Begin(host, 2); Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
            Assert.That(host.Current.LeakActive, Is.False);
        }
    }
}
