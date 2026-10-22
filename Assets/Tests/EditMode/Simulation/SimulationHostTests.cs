using System;
using LighthouseKeepers.Core.Simulation;
using LighthouseKeepers.Simulation.Unity;
using NUnit.Framework;
using UnityEditor;

namespace LighthouseKeepers.Tests.Simulation
{
    public sealed class SimulationHostTests
    {
        FloodParameters tuning;
        [SetUp] public void Load() => tuning = AssetDatabase.LoadAssetAtPath<FloodTuning>(
            "Assets/Data/Tuning/FloodTuning.asset").ToParameters();
        SimulationHost Create(double volume = 0, double? plinth = 3.5) =>
            new SimulationHost(tuning, new FixedSimulationGeometry(0, plinth), new LocalAuthority(), volume);

        [Test] public void AccumulatorEmitsExactlyTenTicksPerSecondAndPreservesRemainder()
        {
            var host = Create(); int notifications = 0; host.OnTick += _ => notifications++;
            for (int i = 0; i < 144; i++) host.Advance(1.0 / 144);
            Assert.That(notifications, Is.EqualTo(10));
            Assert.That(host.Current.WaterVolumeM3, Is.EqualTo(tuning.LeakInflowM3PerSec).Within(1e-8));
            host.Advance(.04); host.Advance(.06);
            Assert.That(notifications, Is.EqualTo(11));
        }

        [Test] public void VolumeHasNoHeightCeilingOrHeightDependentEasing()
        {
            var tall = new FloodParameters(tuning.LeakInflowM3PerSec, tuning.ManualPumpStrokeM3,
                tuning.ManualPumpMinStrokeSeconds, tuning.ElectricPumpM3PerSec, tuning.FloorAreaM2,
                100, tuning.SafeHeightM, tuning.FailSustainSeconds, tuning.ElectricPumpStartPower, tuning.InitialFuelSeconds,
                tuning.GeneratorFaultSeepM3PerSec);
            var host = new SimulationHost(tall, new FixedSimulationGeometry(0, 200), new LocalAuthority(), 20 * tuning.FloorAreaM2);
            double before = host.Current.WaterVolumeM3; host.Advance(120);
            Assert.That(host.Current.WaterVolumeM3 - before,
                Is.EqualTo(120 * tuning.LeakInflowM3PerSec).Within(1e-7));
            Assert.That(host.Current.WaterHeightM, Is.GreaterThan(20));
        }

        [Test] public void FailRequiresThreeSecondsAndDroppingBelowResetsTimer()
        {
            // A tiny live leak lets one legal stroke cross back below the line
            // after 2.9 s above it, independently of the game-balance rates.
            var slowLeak = new FloodParameters(.001, tuning.ManualPumpStrokeM3,
                tuning.ManualPumpMinStrokeSeconds, tuning.ElectricPumpM3PerSec, tuning.FloorAreaM2,
                tuning.FailHeightM, tuning.SafeHeightM, tuning.FailSustainSeconds,
                tuning.ElectricPumpStartPower, tuning.InitialFuelSeconds, tuning.GeneratorFaultSeepM3PerSec);
            var host = new SimulationHost(slowLeak, new FixedSimulationGeometry(0, 3.5),
                new LocalAuthority(), tuning.FailHeightM * tuning.FloorAreaM2);
            host.Advance(2.9); Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
            host.Enqueue(new SimCommand(SimCommandKind.ManualPumpStroke)); host.Advance(.1);
            Assert.That(host.Current.SecondsAboveFail, Is.Zero);
            host.Advance(.7); Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
            var sustained = Create(tuning.FailHeightM * tuning.FloorAreaM2);
            sustained.Advance(3); Assert.That(sustained.Current.Phase, Is.EqualTo(RunPhase.Failed));
        }

        [Test] public void PumpCooldownCannotBeBypassedByAlternatingActorsOrQueuedSpam()
        {
            var host = Create(5);
            for (int i = 0; i < 100; i++) host.Enqueue(new SimCommand(SimCommandKind.ManualPumpStroke, actorId: i % 2));
            host.Advance(.1);
            Assert.That(host.Current.WaterVolumeM3, Is.EqualTo(5 - tuning.ManualPumpStrokeM3 + tuning.LeakInflowM3PerSec * .1).Within(1e-8));
            Assert.That(host.Current.LastRejectReason, Does.Contain("too fast"));
            host.Advance(1.1);
            host.Enqueue(new SimCommand(SimCommandKind.ManualPumpStroke, actorId: 1)); host.Advance(.1);
            Assert.That(host.Current.LastRejectReason, Is.Empty);
        }

        [Test] public void CrossingMidTickDoesNotCountAnEntireTickTowardFailure()
        {
            var host = Create(tuning.FailHeightM * tuning.FloorAreaM2 - tuning.LeakInflowM3PerSec * .05);
            host.Advance(3);
            Assert.That(host.Current.SecondsAboveFail, Is.EqualTo(2.95f).Within(1e-5));
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
            host.Advance(.1);
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Failed));
        }

        [Test] public void DryStrokeCannotBankWaterRemovalAndVolumeNeverGoesNegative()
        {
            var host = Create(); host.Enqueue(new SimCommand(SimCommandKind.ManualPumpStroke)); host.Advance(.1);
            Assert.That(host.Current.WaterVolumeM3, Is.EqualTo(tuning.LeakInflowM3PerSec * .1).Within(1e-8));
            Assert.That(host.Current.OutflowM3PerSec, Is.Zero);
        }

        [Test] public void PowerLossStopsElectricPumpWithoutAutomaticRestart()
        {
            var host = Create(10);
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator));
            host.Enqueue(new SimCommand(SimCommandKind.SetElectricPump, value: 1)); host.Advance(.1);
            Assert.That(host.Current.PowerAvailable, Is.EqualTo(1)); Assert.That(host.Current.ElectricPumpRunning, Is.True);
            host.ForceGeneratorFault(); host.Advance(.1);
            Assert.That(host.Current.PowerAvailable, Is.Zero); Assert.That(host.Current.ElectricPumpRunning, Is.False);
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator)); host.Advance(.1);
            Assert.That(host.Current.PowerAvailable, Is.EqualTo(1)); Assert.That(host.Current.ElectricPumpRunning, Is.False);
        }

        [Test] public void FloodedAndMissingPlinthRejectGeneratorWithReadableReason()
        {
            var flooded = Create(tuning.FailHeightM * tuning.FloorAreaM2, plinth: .5);
            flooded.Enqueue(new SimCommand(SimCommandKind.StartGenerator)); flooded.Advance(.1);
            Assert.That(flooded.Current.GeneratorFlooded, Is.True);
            Assert.That(flooded.Current.LastRejectReason, Does.Contain("flooded"));
            var unknown = Create(0, plinth: null);
            unknown.Enqueue(new SimCommand(SimCommandKind.StartGenerator)); unknown.Advance(.1);
            Assert.That(unknown.Current.LastRejectReason, Does.Contain("unavailable"));
            Assert.That(unknown.Current.PowerAvailable, Is.Zero);
        }

        [Test] public void FuelExhaustionCutsPowerAndRejectsAnotherStart()
        {
            var littleFuel = new FloodParameters(tuning.LeakInflowM3PerSec, tuning.ManualPumpStrokeM3,
                tuning.ManualPumpMinStrokeSeconds, tuning.ElectricPumpM3PerSec, tuning.FloorAreaM2,
                tuning.FailHeightM, tuning.SafeHeightM, tuning.FailSustainSeconds, tuning.ElectricPumpStartPower, .15,
                tuning.GeneratorFaultSeepM3PerSec);
            var host = new SimulationHost(littleFuel, new FixedSimulationGeometry(0, 3.5), new LocalAuthority(), 5);
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator));
            host.Enqueue(new SimCommand(SimCommandKind.SetElectricPump, value: 1)); host.Advance(.2);
            Assert.That(host.Current.FuelRemainingSeconds, Is.Zero);
            Assert.That(host.Current.PowerAvailable, Is.Zero); Assert.That(host.Current.ElectricPumpRunning, Is.False);
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator)); host.Advance(.1);
            Assert.That(host.Current.LastRejectReason, Does.Contain("fuel"));
        }

        [Test] public void BeginDayPreservesVolumeAndStartsHealthyWhileResetRunRestoresDryFixture()
        {
            var host = Create(10); host.Enqueue(new SimCommand(SimCommandKind.RepairLeak)); host.Advance(.1);
            double before = host.Current.WaterVolumeM3;
            host.Enqueue(new SimCommand(SimCommandKind.BeginDay, targetId: 2)); host.Advance(.1);
            Assert.That(host.Current.WaterVolumeM3, Is.LessThan(before)); Assert.That(host.Current.LeakActive, Is.False);
            Assert.That(host.Current.DayIndex, Is.EqualTo(2)); Assert.That(host.Current.GeneratorRunning, Is.True);
            host.Enqueue(new SimCommand(SimCommandKind.ResetRun)); host.Advance(.1);
            Assert.That(host.Current.WaterVolumeM3, Is.EqualTo(tuning.LeakInflowM3PerSec * .1).Within(1e-8));
            Assert.That(host.Current.Tick, Is.EqualTo(3));
        }

        sealed class ReplicaAuthority : IAuthority { public bool IsAuthoritative => false; }
        [Test] public void ReplicaCannotMutateOrAdvanceSimulation()
        {
            var host = new SimulationHost(tuning, new FixedSimulationGeometry(0, 3.5), new ReplicaAuthority());
            host.Enqueue(new SimCommand(SimCommandKind.RepairLeak)); host.ForceGeneratorFault(); host.Advance(10);
            Assert.That(host.Current.Tick, Is.Zero); Assert.That(host.Current.LeakActive, Is.True);
            Assert.That(host.Current.WaterVolumeM3, Is.Zero);
        }

        [Test] public void InvalidInputsAreRejectedWithoutPoisoningVolume()
        {
            var host = Create();
            Assert.Throws<ArgumentOutOfRangeException>(() => host.Advance(double.NaN));
            host.Enqueue(new SimCommand(SimCommandKind.SetElectricPump, value: float.NaN)); host.Advance(.1);
            Assert.That(host.Current.LastRejectReason, Does.Contain("finite"));
            Assert.That(double.IsNaN(host.Current.WaterVolumeM3), Is.False);
        }

        [Test] public void WorldHeightUsesInjectedFloorOriginAndMeasuredPlinth()
        {
            var host = new SimulationHost(tuning, new FixedSimulationGeometry(4, 4.5),
                new LocalAuthority(), .6 * tuning.FloorAreaM2);
            Assert.That(host.Current.WaterHeightM, Is.EqualTo(4.6f).Within(1e-6));
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator)); host.Advance(.1);
            Assert.That(host.Current.GeneratorFlooded, Is.True);
            Assert.That(host.Current.PowerAvailable, Is.Zero);
        }

        [Test] public void WaterCrossingPlinthImmediatelyCutsPowerAtNextSnapshot()
        {
            var host = Create(.49 * tuning.FloorAreaM2, plinth: .5);
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator)); host.Advance(.1);
            Assert.That(host.Current.GeneratorRunning, Is.True);
            host.Advance(3);
            Assert.That(host.Current.GeneratorFlooded, Is.True);
            Assert.That(host.Current.GeneratorRunning, Is.False);
            Assert.That(host.Current.PowerAvailable, Is.Zero);
        }

        [Test] public void RepairAtSafeHeightSucceedsAndFailedRunRequiresReset()
        {
            var safe = Create(); safe.Enqueue(new SimCommand(SimCommandKind.RepairLeak)); safe.Advance(.1);
            Assert.That(safe.Current.Phase, Is.EqualTo(RunPhase.Succeeded));
            var failed = Create(tuning.FailHeightM * tuning.FloorAreaM2); failed.Advance(3);
            failed.Enqueue(new SimCommand(SimCommandKind.BeginDay, targetId: 2)); failed.Advance(.1);
            Assert.That(failed.Current.Phase, Is.EqualTo(RunPhase.Failed));
            failed.Enqueue(new SimCommand(SimCommandKind.ResetRun)); failed.Advance(.1);
            Assert.That(failed.Current.Phase, Is.EqualTo(RunPhase.Running));
        }

        [Test] public void FailEtaDoesNotTreatOneStrokeAsContinuousPumping()
        {
            var host = Create(5);
            host.Enqueue(new SimCommand(SimCommandKind.ManualPumpStroke)); host.Advance(.1);
            Assert.That(float.IsPositiveInfinity(host.Current.TimeToFailSeconds), Is.False);
            var draining = Create(.91 * tuning.FloorAreaM2);
            draining.Enqueue(new SimCommand(SimCommandKind.StartGenerator));
            draining.Enqueue(new SimCommand(SimCommandKind.SetElectricPump, value: 1)); draining.Advance(.1);
            Assert.That(float.IsPositiveInfinity(draining.Current.TimeToFailSeconds), Is.True,
                "Current drainage crosses below the failure line before the sustain expires.");
        }

        [Test] public void PauseFreezesVolumeFuelAndFailureClockButContinuesSnapshots()
        {
            var host = Create(tuning.FailHeightM * tuning.FloorAreaM2);
            host.Enqueue(new SimCommand(SimCommandKind.StartGenerator)); host.Advance(.2);
            var before = host.Current; int notifications = 0; host.OnTick += _ => notifications++;
            host.Enqueue(new SimCommand(SimCommandKind.PauseSimulation)); host.Advance(5);
            Assert.That(host.Current.Paused, Is.True);
            Assert.That(host.Current.Tick, Is.EqualTo(before.Tick + 50)); Assert.That(notifications, Is.EqualTo(50));
            Assert.That(host.Current.WaterVolumeM3, Is.EqualTo(before.WaterVolumeM3));
            Assert.That(host.Current.FuelRemainingSeconds, Is.EqualTo(before.FuelRemainingSeconds));
            Assert.That(host.Current.SecondsAboveFail, Is.EqualTo(before.SecondsAboveFail));
            Assert.That(host.Current.InflowM3PerSec, Is.Zero); Assert.That(host.Current.OutflowM3PerSec, Is.Zero);
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
            host.Enqueue(new SimCommand(SimCommandKind.ResumeSimulation)); host.Advance(.1);
            Assert.That(host.Current.Paused, Is.False);
            Assert.That(host.Current.WaterVolumeM3, Is.GreaterThan(before.WaterVolumeM3));
            Assert.That(host.Current.FuelRemainingSeconds, Is.LessThan(before.FuelRemainingSeconds));
        }

        [Test] public void PausedGameplayIsRejectedAndPausedTimeCannotBypassPumpCooldown()
        {
            var host = Create(5);
            host.Enqueue(new SimCommand(SimCommandKind.ManualPumpStroke)); host.Advance(.1);
            host.Enqueue(new SimCommand(SimCommandKind.PauseSimulation)); host.Advance(10);
            double before = host.Current.WaterVolumeM3;
            host.Enqueue(new SimCommand(SimCommandKind.ManualPumpStroke)); host.Advance(.1);
            Assert.That(host.Current.LastRejectReason, Does.Contain("paused"));
            Assert.That(host.Current.WaterVolumeM3, Is.EqualTo(before));
            host.Enqueue(new SimCommand(SimCommandKind.ResumeSimulation));
            host.Enqueue(new SimCommand(SimCommandKind.ManualPumpStroke)); host.Advance(.1);
            Assert.That(host.Current.LastRejectReason, Does.Contain("too fast"));
        }

        [Test] public void ResumeNeverRevivesATerminalPhase()
        {
            var failed = Create(tuning.FailHeightM * tuning.FloorAreaM2); failed.Advance(3);
            failed.Enqueue(new SimCommand(SimCommandKind.PauseSimulation)); failed.Advance(.1);
            failed.Enqueue(new SimCommand(SimCommandKind.ResumeSimulation)); failed.Advance(.1);
            Assert.That(failed.Current.Phase, Is.EqualTo(RunPhase.Failed));
            var succeeded = Create(); succeeded.Enqueue(new SimCommand(SimCommandKind.RepairLeak)); succeeded.Advance(.1);
            succeeded.Enqueue(new SimCommand(SimCommandKind.ResumeSimulation)); succeeded.Advance(.1);
            Assert.That(succeeded.Current.Phase, Is.EqualTo(RunPhase.Succeeded));
        }

        [Test] public void CommandWireIdsAndHostOnlyPolicyRemainStable()
        {
            Assert.That((int)SimCommandKind.RepairLeak, Is.Zero);
            Assert.That((int)SimCommandKind.ResetRun, Is.EqualTo(6));
            Assert.That((int)SimCommandKind.ForceGeneratorFault, Is.EqualTo(7));
            Assert.That((int)SimCommandKind.PauseSimulation, Is.EqualTo(8));
            Assert.That((int)SimCommandKind.ResumeSimulation, Is.EqualTo(9));
            foreach (SimCommandKind kind in Enum.GetValues(typeof(SimCommandKind)))
                Assert.That(SimCommandPolicy.IsHostOnly(kind), Is.EqualTo((int)kind >= 5), kind.ToString());
            Assert.That(SimCommandPolicy.IsHostOnly((SimCommandKind)(-1)), Is.True);
            var compatible = new SimSnapshot(1, 2, 3, 4, 5, true, false, 0, false, RunPhase.Running);
            Assert.That(compatible.Paused, Is.False, "E's original ten-field constructor remains source-compatible.");
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        [Test] public void DevelopmentInitializationSetsVolumeAndRateWithoutWritingHeightOrTuning()
        {
            var host = new SimulationHost(tuning, new FixedSimulationGeometry(4, 10), new LocalAuthority());
            host.Enqueue(new SimCommand(SimCommandKind.PauseSimulation));
            host.Enqueue(new SimCommand(SimCommandKind.DebugSetWaterVolumeM3, value: 12));
            host.Enqueue(new SimCommand(SimCommandKind.DebugSetLeakInflowM3PerSec, value: .25f)); host.Advance(.1);
            Assert.That(host.Current.WaterVolumeM3, Is.EqualTo(12));
            Assert.That(host.Current.WaterHeightM, Is.EqualTo(4 + 12 / tuning.FloorAreaM2).Within(1e-6));
            Assert.That(host.Current.InflowM3PerSec, Is.Zero, "Paused snapshots report actual flow.");
            host.Enqueue(new SimCommand(SimCommandKind.ResumeSimulation)); host.Advance(.1);
            Assert.That(host.Current.WaterVolumeM3, Is.EqualTo(12.025).Within(1e-8));
            Assert.That(host.Current.InflowM3PerSec, Is.EqualTo(.25f));
            Assert.That(tuning.LeakInflowM3PerSec, Is.EqualTo((double).18f));
        }

        [Test] public void DevelopmentInitializationRequiresPausedRunningAndFiniteNonnegativeValues()
        {
            var host = Create(5);
            host.Enqueue(new SimCommand(SimCommandKind.DebugSetWaterVolumeM3, value: 10)); host.Advance(.1);
            Assert.That(host.Current.LastRejectReason, Does.Contain("paused Running"));
            host.Enqueue(new SimCommand(SimCommandKind.PauseSimulation)); host.Advance(.1);
            double before = host.Current.WaterVolumeM3;
            foreach (var kind in new[] { SimCommandKind.DebugSetWaterVolumeM3, SimCommandKind.DebugSetLeakInflowM3PerSec })
            foreach (float invalid in new[] { -1f, float.NaN, float.PositiveInfinity })
            {
                host.Enqueue(new SimCommand(kind, value: invalid)); host.Advance(.1);
                Assert.That(host.Current.LastRejectReason, Is.Not.Empty);
                Assert.That(host.Current.WaterVolumeM3, Is.EqualTo(before));
            }
            var failed = Create(tuning.FailHeightM * tuning.FloorAreaM2); failed.Advance(3);
            failed.Enqueue(new SimCommand(SimCommandKind.PauseSimulation));
            failed.Enqueue(new SimCommand(SimCommandKind.DebugSetWaterVolumeM3, value: 0)); failed.Advance(.1);
            Assert.That(failed.Current.LastRejectReason, Does.Contain("paused Running"));
            Assert.That(failed.Current.Phase, Is.EqualTo(RunPhase.Failed));
        }

        [Test] public void DevelopmentInitializationAllowsZeroWithoutCreatingNegativeVolumeOrFalseSuccess()
        {
            var host = Create(5); host.Enqueue(new SimCommand(SimCommandKind.PauseSimulation));
            host.Enqueue(new SimCommand(SimCommandKind.DebugSetWaterVolumeM3, value: 0));
            host.Enqueue(new SimCommand(SimCommandKind.DebugSetLeakInflowM3PerSec, value: 0));
            host.Enqueue(new SimCommand(SimCommandKind.ResumeSimulation)); host.Advance(5);
            Assert.That(host.Current.WaterVolumeM3, Is.Zero); Assert.That(host.Current.LeakActive, Is.True);
            Assert.That(host.Current.Phase, Is.EqualTo(RunPhase.Running));
            Assert.That(SimCommandPolicy.IsDevelopmentOnly(SimCommandKind.DebugSetWaterVolumeM3), Is.True);
        }

        [Test] public void DayTransitionPreservesPauseAndOverrideButResetRestoresPublishedRate()
        {
            var host = Create(5); host.Enqueue(new SimCommand(SimCommandKind.PauseSimulation));
            host.Enqueue(new SimCommand(SimCommandKind.DebugSetLeakInflowM3PerSec, value: .25f));
            host.Enqueue(new SimCommand(SimCommandKind.BeginDay, targetId: 2)); host.Advance(.1);
            Assert.That(host.Current.Paused, Is.True);
            host.Enqueue(new SimCommand(SimCommandKind.ResumeSimulation)); host.Advance(.1);
            Assert.That(host.Current.InflowM3PerSec, Is.Zero, "BeginDay waits for a scheduled incident.");
            host.ActivateDayLeak(2); host.Advance(.1);
            Assert.That(host.Current.InflowM3PerSec, Is.EqualTo(.25f));
            host.Enqueue(new SimCommand(SimCommandKind.PauseSimulation));
            host.Enqueue(new SimCommand(SimCommandKind.ResetRun)); host.Advance(.1);
            Assert.That(host.Current.Paused, Is.False);
            Assert.That(host.Current.InflowM3PerSec, Is.EqualTo((float)tuning.LeakInflowM3PerSec));
        }
#endif
    }
}
