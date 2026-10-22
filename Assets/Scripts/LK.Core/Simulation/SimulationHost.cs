using System;
using System.Collections.Generic;

namespace LighthouseKeepers.Core.Simulation
{
    /// <summary>Single-threaded 10 Hz authority. Call Advance with elapsed seconds from the driver.
    /// Commands apply in FIFO order at the next tick, at most 100 ms after receipt.
    /// The cooldown belongs to the one physical pump, so alternating actors cannot bypass it.</summary>
    public sealed class SimulationHost : ISimulationHost
    {
        public const double TickSeconds = .1;
        const double Epsilon = 1e-7;
        readonly FloodParameters tuning;
        readonly ISimulationGeometry geometry;
        readonly IAuthority authority;
        readonly Func<int, bool> canRepairPipe;
        readonly Queue<SimCommand> commands = new Queue<SimCommand>();
        double accumulator, volume, fuel, aboveFail, activeSeconds, leakRate, lastStrokeAt = double.NegativeInfinity;
        int tick;
        int dayIndex;
        int activeLeaks = 1, activatedLeaks = 1, configuredLeakCount = 1;
        double leakMultiplier = 1;
        bool generator, electricPump, flooded, geometryAvailable, paused;
        bool generatorFault;
        RunPhase phase;
        string reject = "";
        public SimSnapshot Current { get; private set; }
        public event Action<SimSnapshot> OnTick;

        public SimulationHost(FloodParameters tuning, ISimulationGeometry geometry,
            IAuthority authority, double initialVolumeM3 = 0, Func<int, bool> canRepairPipe = null)
        {
            this.tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            this.geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            this.authority = authority ?? throw new ArgumentNullException(nameof(authority));
            this.canRepairPipe = canRepairPipe;
            if (!FixedSimulationGeometry.Finite(initialVolumeM3) || initialVolumeM3 < 0 ||
                !FixedSimulationGeometry.Finite(geometry.FloorWorldY))
                throw new ArgumentOutOfRangeException(nameof(initialVolumeM3));
            volume = initialVolumeM3; fuel = tuning.InitialFuelSeconds; leakRate = tuning.LeakInflowM3PerSec;
            RefreshPower(); Publish(0);
        }

        public void Enqueue(SimCommand cmd)
        {
            if (authority.IsAuthoritative) commands.Enqueue(cmd);
        }

        public void ForceGeneratorFault() => Enqueue(new SimCommand(SimCommandKind.ForceGeneratorFault));
        public void ActivateDayLeak(int leakId) => Enqueue(new SimCommand(SimCommandKind.ActivateDayLeak, targetId: leakId));

        public void Advance(double elapsedSeconds)
        {
            if (!FixedSimulationGeometry.Finite(elapsedSeconds) || elapsedSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (!authority.IsAuthoritative) { accumulator = 0; commands.Clear(); return; }
            accumulator += elapsedSeconds;
            while (accumulator + Epsilon >= TickSeconds)
            {
                accumulator = Math.Max(0, accumulator - TickSeconds);
                Step();
            }
        }

        double Depth => volume / tuning.FloorAreaM2;
        double Power => generator && geometryAvailable && !flooded ? 1 : 0;
        int ActiveLeakCount => (activeLeaks & 1) + ((activeLeaks >> 1) & 1) + ((activeLeaks >> 2) & 1);
        // Each physical coupling contributes one share. Fault seep is the same circuit's
        // residual flow when all couplings are sealed; power loss never creates a fourth pipe.
        double ActiveInflow => activeLeaks != 0 ? ActiveLeakCount * leakRate * leakMultiplier :
            generatorFault ? tuning.GeneratorFaultSeepM3PerSec : 0;

        void RefreshPower()
        {
            geometryAvailable = geometry.TryGetGeneratorPlinthTopWorldY(out double plinth) &&
                FixedSimulationGeometry.Finite(plinth) && plinth > geometry.FloorWorldY;
            flooded = geometryAvailable && geometry.FloorWorldY + Depth >= plinth;
            if (flooded || !geometryAvailable || fuel <= 0) generator = false;
            if (Power == 0) electricPump = false;
        }

        void Step()
        {
            tick++;
            RefreshPower();
            double manualRemoved = 0;
            // Event callbacks enqueue for the following tick; never process a growing queue.
            int count = commands.Count;
            for (int i = 0; i < count; i++) Apply(commands.Dequeue(), ref manualRemoved);
            double outflow = manualRemoved / TickSeconds;
            if ((phase == RunPhase.Running || phase == RunPhase.Succeeded) && !paused)
            {
                double startDepth = Depth;
                double inflow = phase == RunPhase.Running ? ActiveInflow : 0;
                double electricRate = electricPump ? tuning.ElectricPumpM3PerSec * Power : 0;
                // Run only for remaining fuel when the tank empties within a tick.
                double poweredSeconds = generator ? Math.Min(TickSeconds, fuel) : 0;
                double electricRemoved = Math.Min(volume + inflow * TickSeconds, electricRate * poweredSeconds);
                volume = Math.Max(0, volume + inflow * TickSeconds - electricRemoved);
                outflow += electricRemoved / TickSeconds;
                fuel = Math.Max(0, fuel - poweredSeconds);
                activeSeconds += TickSeconds;
                RefreshPower();
                if (phase == RunPhase.Running)
                {
                    if (Depth < tuning.FailHeightM) aboveFail = 0;
                    else if (startDepth < tuning.FailHeightM)
                        // Count only the part of the crossing tick actually above the line.
                        aboveFail = TickSeconds * (Depth - tuning.FailHeightM) / (Depth - startDepth);
                    else aboveFail += TickSeconds;
                    if (aboveFail + Epsilon >= tuning.FailSustainSeconds) phase = RunPhase.Failed;
                    // D owns all begun-watch completion. Repairing a coupling must never
                    // complete a watch or bypass its clock, board tasks or bed interaction.
                    else if (dayIndex == 0 && activeLeaks == 0 && !generatorFault && Depth <= tuning.SafeHeightM)
                        phase = RunPhase.Succeeded;
                }
            }
            Publish(outflow);
            OnTick?.Invoke(Current);
        }

        void Apply(SimCommand cmd, ref double manualRemoved)
        {
            if (!FixedSimulationGeometry.Finite(cmd.Value)) { reject = "Command value must be finite."; return; }
            if (cmd.Kind == SimCommandKind.PauseSimulation || cmd.Kind == SimCommandKind.ResumeSimulation)
            {
                paused = cmd.Kind == SimCommandKind.PauseSimulation; reject = ""; return;
            }
            if (cmd.Kind == SimCommandKind.ResetRun)
            {
                volume = 0; fuel = tuning.InitialFuelSeconds; aboveFail = 0;
                activeLeaks = activatedLeaks = 1; configuredLeakCount = 1; leakMultiplier = 1;
                generator = electricPump = false; phase = RunPhase.Running;
                paused = false; activeSeconds = 0; leakRate = tuning.LeakInflowM3PerSec;
                dayIndex = 0; generatorFault = false;
                lastStrokeAt = double.NegativeInfinity; manualRemoved = 0; reject = "";
                RefreshPower(); return;
            }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (cmd.Kind == SimCommandKind.DebugSetWaterVolumeM3 || cmd.Kind == SimCommandKind.DebugSetLeakInflowM3PerSec)
            {
                if (phase != RunPhase.Running || !paused)
                { reject = "Development initialization requires a paused Running simulation."; return; }
                if (cmd.Value < 0)
                { reject = "Development volume and leak rate must be nonnegative."; return; }
                if (cmd.Kind == SimCommandKind.DebugSetWaterVolumeM3)
                {
                    volume = cmd.Value; aboveFail = 0; manualRemoved = 0; RefreshPower();
                }
                else leakRate = cmd.Value;
                reject = ""; return;
            }
            if (cmd.Kind == SimCommandKind.DebugSetLeak)
            {
                if (cmd.Value != 0 && cmd.Value != 1) { reject = "Leak value must be 0 or 1."; return; }
                activeLeaks = cmd.Value == 1 ? (1 << configuredLeakCount) - 1 : 0;
                activatedLeaks |= activeLeaks; reject = ""; return;
            }
            if (cmd.Kind == SimCommandKind.DebugSetWaterHeight)
            {
                volume = Math.Max(0, cmd.Value - geometry.FloorWorldY) * tuning.FloorAreaM2;
                aboveFail = 0; reject = ""; RefreshPower(); return;
            }
#endif
            if (phase == RunPhase.Failed) { reject = "Run failed. Reset the run first."; return; }
            if (cmd.Kind == SimCommandKind.BeginDay)
            {
                if (cmd.TargetId < 1 || cmd.TargetId > 7) { reject = "Watch index must be 1 to 7."; return; }
                if (tuning.DayCount != 7) { reject = "Seven day tuning profiles are required."; return; }
                RefreshPower();
                if (!CanStartGenerator()) return;
                DayTuning plan = tuning.GetDay(cmd.TargetId);
                dayIndex = cmd.TargetId; configuredLeakCount = plan.LeakCount; leakMultiplier = plan.InflowMultiplier;
                activeLeaks = activatedLeaks = 0; generatorFault = false;
                generator = electricPump = true; aboveFail = 0; phase = RunPhase.Running; reject = ""; return;
            }
            if (paused) { reject = "Simulation is paused."; return; }
            if (phase != RunPhase.Running) { reject = "Day succeeded. Begin the next day or reset."; return; }
            switch (cmd.Kind)
            {
                case SimCommandKind.ActivateDayLeak:
                    if (dayIndex == 0 || cmd.TargetId < 0 || cmd.TargetId >= configuredLeakCount)
                    { reject = "Leak coupling is outside the current day's configured range."; return; }
                    int bit = 1 << cmd.TargetId;
                    if ((activatedLeaks & bit) == 0)
                    { activeLeaks |= bit; activatedLeaks |= bit; electricPump = false; }
                    break;
                case SimCommandKind.RepairLeak:
                    if (cmd.TargetId < 0 || cmd.TargetId >= configuredLeakCount || (activeLeaks & (1 << cmd.TargetId)) == 0)
                    { reject = "Leak coupling is invalid or not active."; return; }
                    if (dayIndex > 0)
                    {
                        bool authorized;
                        try { authorized = canRepairPipe != null && canRepairPipe(cmd.ActorId); }
                        catch (Exception) { reject = "Pipe repair validation unavailable."; return; }
                        if (!authorized) { reject = "Pipe repair is not authorized for this actor."; return; }
                    }
                    activeLeaks &= ~(1 << cmd.TargetId);
                    if (dayIndex > 0 && Power >= tuning.ElectricPumpStartPower && Power > 0) electricPump = true;
                    break;
                case SimCommandKind.ManualPumpStroke:
                    double now = activeSeconds;
                    if (now - lastStrokeAt + Epsilon < tuning.ManualPumpMinStrokeSeconds)
                    { reject = "Pump stroke too fast."; return; }
                    lastStrokeAt = now;
                    double removed = Math.Min(volume, tuning.ManualPumpStrokeM3);
                    volume -= removed; manualRemoved += removed; RefreshPower();
                    break;
                case SimCommandKind.StartGenerator:
                    RefreshPower();
                    if (!CanStartGenerator()) return;
                    generator = true;
                    if (dayIndex > 0) { generatorFault = false; electricPump = true; }
                    break;
                case SimCommandKind.StopGenerator:
                    generator = electricPump = false; break;
                case SimCommandKind.ForceGeneratorFault:
                    generator = electricPump = false;
                    if (dayIndex > 0) generatorFault = true;
                    break;
                case SimCommandKind.SetElectricPump:
                    if (cmd.Value != 0 && cmd.Value != 1) { reject = "Pump value must be 0 or 1."; return; }
                    if (cmd.Value == 1 && (Power == 0 || Power < tuning.ElectricPumpStartPower))
                    { reject = "Electric pump requires generator power."; return; }
                    electricPump = cmd.Value == 1; break;
                default: reject = "Unknown simulation command."; return;
            }
            reject = "";
        }

        bool CanStartGenerator()
        {
            if (!geometryAvailable) { reject = "Generator plinth height unavailable."; return false; }
            if (flooded) { reject = "Generator is flooded."; return false; }
            if (fuel <= 0) { reject = "Generator fuel exhausted."; return false; }
            return true;
        }

        void Publish(double outflow)
        {
            double inflow = phase == RunPhase.Running && !paused ? ActiveInflow : 0;
            // ETA assumes no future manual strokes and that current power persists.
            // A stroke is an impulse, not a promise of continuous 2 m³/s pumping.
            double net = inflow - (phase == RunPhase.Running && electricPump ? tuning.ElectricPumpM3PerSec * Power : 0);
            double eta = double.PositiveInfinity;
            if (phase == RunPhase.Failed) eta = 0;
            else if (phase == RunPhase.Running && !paused)
            {
                if (Depth >= tuning.FailHeightM)
                {
                    double remaining = Math.Max(0, tuning.FailSustainSeconds - aboveFail);
                    double timeBelow = net < 0 ? (volume - tuning.FailHeightM * tuning.FloorAreaM2) / -net : double.PositiveInfinity;
                    if (timeBelow >= remaining) eta = remaining;
                }
                else if (net > 0) eta = (tuning.FailHeightM * tuning.FloorAreaM2 - volume) / net + tuning.FailSustainSeconds;
            }
            Current = new SimSnapshot(tick, volume, (float)(geometry.FloorWorldY + Depth),
                (float)inflow, (float)outflow, activeLeaks != 0 || generatorFault, generator, (float)Power, electricPump, phase, reject,
                flooded, geometryAvailable, (float)fuel, (float)aboveFail, (float)eta, paused, dayIndex, generatorFault, activeLeaks, activatedLeaks, configuredLeakCount);
        }
    }
}
