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
            host.ActivateDayLeak(0); host.Advance(60);
            Assert.That(host.Current.LeakActive, Is.True); Assert.That(host.Current.ElectricPumpRunning, Is.False);
            Assert.That(host.Current.WaterVolumeM3, Is.GreaterThan(tuning.SafeHeightM * tuning.FloorAreaM2));
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak, actorId: 9)); host.Advance(.1);
            Assert.That(host.Current.LastRejectReason, Does.Contain("authorized")); Assert.That(host.Current.LeakActive, Is.True);
            permitted = true;
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak, actorId: 9)); host.Advance(.1);
            Assert.That(host.Current.LeakActive, Is.False); Assert.That(host.Current.ElectricPumpRunning, Is.True);
            while (host.Current.WaterHeightM > tuning.SafeHeightM && host.Current.Tick < 1500) host.Advance(.1);
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running), "D owns watch completion.");
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
            host.Advance(10); Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running), "D owns watch completion.");
        }

        [Test] public void DayThreePipeAndPowerFaultsEachRequireTheirOwnRecoveryAction()
        {
            var host = Create(); Begin(host, 3); host.ActivateDayLeak(0); host.ForceGeneratorFault(); host.Advance(20);
            Assert.That(host.Current.InflowM3PerSec, Is.EqualTo((float)(tuning.LeakInflowM3PerSec * tuning.GetDay(3).InflowMultiplier)));
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak, actorId: 1)); host.Advance(.1);
            Assert.That(host.Current.GeneratorFaultActive, Is.True); Assert.That(host.Current.LeakActive, Is.True);
            Assert.That(host.Current.InflowM3PerSec, Is.EqualTo((float)tuning.GeneratorFaultSeepM3PerSec));
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator, actorId: 2)); host.Advance(.1);
            Assert.That(host.Current.GeneratorFaultActive, Is.False); Assert.That(host.Current.LeakActive, Is.False);
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running), "D owns watch completion.");
        }

        [Test] public void RestoringPowerDoesNotRepairTheDayThreePipe()
        {
            var host = Create(); Begin(host, 3); host.ActivateDayLeak(0); host.ForceGeneratorFault(); host.Advance(1);
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

        [Test] public void ScheduledLeakUsesCouplingIdAndDuplicateActivationIsIdempotent()
        {
            var host = Create(); Begin(host, 1); host.ActivateDayLeak(1); host.Advance(.1);
            Assert.That(host.Current.LeakActive, Is.False); Assert.That(host.Current.LastRejectReason, Does.Contain("coupling"));
            host.ActivateDayLeak(0); host.Advance(.1);
            host.Enqueue(new SimCommand(SimCommandKind.SetElectricPump, value: 1)); host.Advance(.1);
            host.ActivateDayLeak(0); host.Advance(.1);
            Assert.That(host.Current.ElectricPumpRunning, Is.True, "Duplicate scheduling must not retrip an already restored pump.");
        }

        [Test] public void WatchRepairFailsClosedWithoutProgressionValidation()
        {
            var host = new SimulationHost(tuning, new FixedSimulationGeometry(0, 3.75), new LocalAuthority());
            Begin(host, 1); host.ActivateDayLeak(0); host.Advance(.1);
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak)); host.Advance(.1);
            Assert.That(host.Current.LeakActive, Is.True); Assert.That(host.Current.LastRejectReason, Does.Contain("authorized"));
        }

        [Test] public void BeginDayRequiresMeasuredAvailablePowerAndValidWatchIndex()
        {
            var unknown = new SimulationHost(tuning, new FixedSimulationGeometry(0, null), new LocalAuthority());
            Begin(unknown, 1); Assert.That(unknown.Current.DayIndex, Is.Zero);
            Assert.That(unknown.Current.LastRejectReason, Does.Contain("unavailable"));
            var host = Create(); Begin(host, 0);
            Assert.That(host.Current.DayIndex, Is.Zero); Assert.That(host.Current.LastRejectReason, Does.Contain("1 to 7"));
        }

        [Test] public void BrokenRepairValidationRejectsInputWithoutAbortingTheTick()
        {
            var host = Create(_ => throw new InvalidOperationException("Director unavailable")); Begin(host, 1);
            host.ActivateDayLeak(0); host.Advance(.1); int before = host.Current.Tick;
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak)); host.Advance(.1);
            Assert.That(host.Current.Tick, Is.EqualTo(before + 1));
            Assert.That(host.Current.LeakActive, Is.True); Assert.That(host.Current.LastRejectReason, Does.Contain("unavailable"));
        }

        [Test] public void RepairAndPowerRecoveryNeverCompleteTheDirectorsWatch()
        {
            var host = Create(); Begin(host, 1); host.ActivateDayLeak(0); host.Advance(.1);
            host.Enqueue(new SimCommand(SimCommandKind.StopGenerator));
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak)); host.Advance(.1);
            Assert.That(host.Current.LeakActive, Is.False); Assert.That(host.Current.PowerAvailable, Is.Zero);
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator)); host.Advance(.1);
            Assert.That(host.Current.ElectricPumpRunning, Is.True); Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running), "D owns watch completion.");
        }

        [Test] public void RecoveryKeepsDrainingPastSafeHeightWhileDirectorRetainsControl()
        {
            var host = Create(); Begin(host, 1); host.ActivateDayLeak(0); host.Advance(60);
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak)); host.Advance(.1);
            while (host.Current.WaterHeightM > tuning.SafeHeightM && host.Current.Tick < 1500) host.Advance(.1);
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running), "D owns watch completion.");
            double atSuccess = host.Current.WaterVolumeM3;
            Assert.That(atSuccess, Is.GreaterThan(0));
            host.Advance(5);
            Assert.That(host.Current.WaterVolumeM3, Is.LessThan(atSuccess), "Safe height is not a water ceiling/freeze point.");
            Assert.That(host.Current.OutflowM3PerSec, Is.GreaterThan(0)); Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running), "D owns watch completion.");
            host.Advance(30);
            Assert.That(host.Current.WaterVolumeM3, Is.Zero); Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running), "D owns watch completion.");
            Begin(host, 2); Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
            Assert.That(host.Current.LeakActive, Is.False);
        }
        [Test] public void ThreeCouplingsIntegrateIndependentlyAndEachRepairRemovesOneShare()
        {
            var host = Create(); Begin(host, 5);
            ISimulationHost endpoint = host;
            for (int id = 0; id < 3; id++) endpoint.ActivateDayLeak(id);
            host.Advance(2);
            double share = tuning.LeakInflowM3PerSec * tuning.GetDay(5).InflowMultiplier;
            Assert.That(host.Current.ActiveLeakMask, Is.EqualTo(7));
            Assert.That(host.Current.ActiveLeakCount, Is.EqualTo(3));
            Assert.That(host.Current.InflowM3PerSec, Is.EqualTo(3 * share).Within(1e-6));
            // Keep pumping off to measure the integrated inflow independently.
            for (int id = 2; id >= 0; id--)
            {
                host.Enqueue(new SimCommand(SimCommandKind.RepairLeak, actorId: 8, targetId: id));
                host.Enqueue(new SimCommand(SimCommandKind.SetElectricPump, value: 0));
                double before = host.Current.WaterVolumeM3; host.Advance(.1);
                Assert.That(host.Current.ActiveLeakCount, Is.EqualTo(id));
                Assert.That(host.Current.InflowM3PerSec, Is.EqualTo(id * share).Within(1e-6));
                Assert.That(host.Current.WaterVolumeM3 - before, Is.EqualTo(id * share * .1).Within(1e-8));
                Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
            }
            Assert.That(host.Current.RepairedLeakCount, Is.EqualTo(3));
            host.ActivateDayLeak(2); host.Advance(.1);
            Assert.That(host.Current.ActiveLeakMask, Is.Zero, "A retried schedule must not reopen a repaired coupling.");
            Begin(host, 7); host.ActivateDayLeak(2); host.Advance(.1);
            Assert.That(host.Current.ActiveLeakMask, Is.EqualTo(4), "The next day clears coupling history.");
        }

        [Test] public void WrongOrInactiveRepairCannotCloseAnotherCoupling()
        {
            var host = Create(); Begin(host, 5); host.ActivateDayLeak(1); host.Advance(.1);
            foreach (int id in new[] { -1, 0, 2, 3 })
            {
                host.Enqueue(new SimCommand(SimCommandKind.RepairLeak, targetId: id)); host.Advance(.1);
                Assert.That(host.Current.ActiveLeakMask, Is.EqualTo(2));
                Assert.That(host.Current.LastRejectReason, Is.Not.Empty);
            }
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak, targetId: 1)); host.Advance(.1);
            Assert.That(host.Current.ActiveLeakCount, Is.Zero);
            Assert.That(host.Current.RepairedLeakCount, Is.EqualTo(1));
        }

        [Test] public void AllSevenProfilesKeepOneLeakStrongerThanMaximumManualPumping()
        {
            Assert.That(tuning.DayCount, Is.EqualTo(7));
            for (int day = 1; day <= 7; day++)
            {
                var plan = tuning.GetDay(day);
                Assert.That(plan.DayIndex, Is.EqualTo(day));
                Assert.That(plan.LeakCount, Is.InRange(1, 3));
                Assert.That(plan.DayLengthSeconds, Is.GreaterThan(0));
                double singleLeak = tuning.LeakInflowM3PerSec * plan.InflowMultiplier;
                double manual = tuning.ManualPumpStrokeM3 / tuning.ManualPumpMinStrokeSeconds;
                Assert.That(manual / singleLeak, Is.InRange(.85, .95), "Day " + day);
                var host = Create(); Begin(host, day); host.ActivateDayLeak(0);
                host.Enqueue(new SimCommand(SimCommandKind.SetElectricPump, value: 0));
                for (int tick = 0; tick < 1200; tick++)
                {
                    if (tick % (int)Math.Ceiling(tuning.ManualPumpMinStrokeSeconds / SimulationHost.TickSeconds - 1e-6) == 0) host.Enqueue(new SimCommand(SimCommandKind.ManualPumpStroke));
                    host.Advance(.1);
                }
                Assert.That(host.Current.WaterVolumeM3, Is.GreaterThan(0));
                Assert.That(host.Current.WaterVolumeM3, Is.LessThan(singleLeak * 120 * .2));
            }
        }

        [Test] public void BeaconUsesGeneratorAndStrictPowerThreshold()
        {
            foreach (bool running in new[] { false, true })
            foreach (float power in new[] { 0, .5f, .5001f, 1 })
            {
                var snapshot = new SimSnapshot(1, 0, 0, 0, 0, false, running, power, false, RunPhase.Running);
                Assert.That(snapshot.BeaconLit, Is.EqualTo(running && power > .5f));
            }
            ISimulationHost endpoint = Create(); endpoint.Enqueue(new SimCommand(SimCommandKind.BeginDay, targetId: 1));
            ((SimulationHost)endpoint).Advance(.1); Assert.That(endpoint.Current.BeaconLit, Is.True);
            endpoint.ForceGeneratorFault(); ((SimulationHost)endpoint).Advance(.1);
            Assert.That(endpoint.Current.BeaconLit, Is.False);
        }

        [Test] public void DayTuningRejectsInvalidCountsDurationsMultipliersAndIncompleteCatalogs()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayTuning(0, 1, 1, false, 180));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayTuning(1, 4, 1, false, 180));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayTuning(1, 1, double.NaN, false, 180));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DayTuning(1, 1, 1, false, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => tuning.GetDay(8));
            var profiles = new DayTuning[7];
            for (int i = 0; i < 7; i++) profiles[i] = tuning.GetDay(i + 1);
            Func<DayTuning[], FloodParameters> copy = input => new FloodParameters(tuning.LeakInflowM3PerSec,
                tuning.ManualPumpStrokeM3, tuning.ManualPumpMinStrokeSeconds, tuning.ElectricPumpM3PerSec,
                tuning.FloorAreaM2, tuning.FailHeightM, tuning.SafeHeightM, tuning.FailSustainSeconds,
                tuning.ElectricPumpStartPower, tuning.InitialFuelSeconds, tuning.GeneratorFaultSeepM3PerSec, input);
            Assert.Throws<ArgumentException>(() => copy(new DayTuning[6]));
            var immutable = copy(profiles); profiles[0] = profiles[1];
            Assert.That(immutable.GetDay(1).DayIndex, Is.EqualTo(1));
            Assert.Throws<ArgumentException>(() => copy(profiles));
            profiles[0] = default;
            Assert.Throws<ArgumentOutOfRangeException>(() => copy(profiles));
        }
        [Test] public void SevenDayFuelBudgetIncludesScheduledWatchesAndMaximumRecoveryPerWatch()
        {
            double scheduledSeconds = 0;
            for (int day = 1; day <= 7; day++) scheduledSeconds += tuning.GetDay(day).DayLengthSeconds;
            double oneRecovery = (tuning.FailHeightM - tuning.SafeHeightM) * tuning.FloorAreaM2 / tuning.ElectricPumpM3PerSec;
            Assert.That(tuning.InitialFuelSeconds, Is.GreaterThan(scheduledSeconds + 7 * oneRecovery));
        }

        [Test] public void ThreeLeakRecoveryGridReachesNegativeNetFlowAfterTargetedRepairsAndPowerRestoration()
        {
            foreach (double depth in new[] { .1, .5, .89 })
            foreach (bool power in new[] { false, true })
            for (int count = 0; count <= 3; count++)
            {
                var host = Create(); Begin(host, 7);
                for (int id = 0; id < count; id++) host.ActivateDayLeak(id);
                if (!power) host.ForceGeneratorFault();
                host.Enqueue(new SimCommand(SimCommandKind.PauseSimulation));
                host.Enqueue(new SimCommand(SimCommandKind.DebugSetWaterHeight, value: (float)depth));
                host.Advance(.1);
                host.Enqueue(new SimCommand(SimCommandKind.ResumeSimulation));
                for (int id = 0; id < count; id++) host.Enqueue(new SimCommand(SimCommandKind.RepairLeak, targetId: id));
                host.Enqueue(new SimCommand(SimCommandKind.StartGenerator));
                double before = host.Current.WaterVolumeM3; host.Advance(.1);
                Assert.That(host.Current.WaterVolumeM3, Is.LessThan(before));
                Assert.That(host.Current.InflowM3PerSec - host.Current.OutflowM3PerSec, Is.LessThan(0));
                Assert.That(host.Current.ActiveLeakCount, Is.Zero);
            }
        }
    }
}
