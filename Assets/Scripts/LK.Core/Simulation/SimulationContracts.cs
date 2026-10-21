using System;

namespace LighthouseKeepers.Core.Simulation
{
    public enum RunPhase { Running, Failed, Succeeded }

    public enum SimCommandKind
    {
        RepairLeak = 0,
        ManualPumpStroke = 1,
        StartGenerator = 2,
        StopGenerator = 3,
        SetElectricPump = 4,
        BeginDay = 5,
        ResetRun = 6,
        ForceGeneratorFault = 7,
        PauseSimulation = 8,
        ResumeSimulation = 9,
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        DebugSetLeak = 100,
        DebugSetWaterHeight = 101,
        DebugSetWaterVolumeM3 = 102,
        DebugSetLeakInflowM3PerSec = 103,
#endif
    }

    /// <summary>Transport authorization guidance. Actor identity is authenticated by E,
    /// not by the simulation. Unknown commands fail closed as host-only.</summary>
    public static class SimCommandPolicy
    {
        public static bool IsHostOnly(SimCommandKind kind)
        {
            switch (kind)
            {
                case SimCommandKind.RepairLeak:
                case SimCommandKind.ManualPumpStroke:
                case SimCommandKind.StartGenerator:
                case SimCommandKind.StopGenerator:
                case SimCommandKind.SetElectricPump: return false;
                default: return true;
            }
        }

        public static bool IsDevelopmentOnly(SimCommandKind kind)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            return kind == SimCommandKind.DebugSetLeak || kind == SimCommandKind.DebugSetWaterHeight ||
                kind == SimCommandKind.DebugSetWaterVolumeM3 || kind == SimCommandKind.DebugSetLeakInflowM3PerSec;
#else
            return false;
#endif
        }
    }

    public readonly struct SimCommand
    {
        public readonly SimCommandKind Kind;
        public readonly int ActorId;
        public readonly int TargetId;
        public readonly float Value;

        public SimCommand(SimCommandKind kind, int actorId = 0, int targetId = 0, float value = 0)
        { Kind = kind; ActorId = actorId; TargetId = targetId; Value = value; }
    }

    public readonly struct SimSnapshot
    {
        public readonly int Tick;
        public readonly double WaterVolumeM3;
        public readonly float WaterHeightM;
        public readonly float InflowM3PerSec;
        public readonly float OutflowM3PerSec;
        public readonly bool LeakActive;
        public readonly bool GeneratorRunning;
        public readonly float PowerAvailable;
        public readonly bool ElectricPumpRunning;
        public readonly RunPhase Phase;
        public readonly string LastRejectReason;
        public readonly bool GeneratorFlooded;
        public readonly bool GeneratorGeometryAvailable;
        public readonly float FuelRemainingSeconds;
        public readonly float SecondsAboveFail;
        public readonly float TimeToFailSeconds;
        public readonly bool Paused;

        public SimSnapshot(int tick, double waterVolumeM3, float waterHeightM,
            float inflowM3PerSec, float outflowM3PerSec, bool leakActive,
            bool generatorRunning, float powerAvailable, bool electricPumpRunning,
            RunPhase phase, string lastRejectReason = "", bool generatorFlooded = false,
            bool generatorGeometryAvailable = true, float fuelRemainingSeconds = 0,
            float secondsAboveFail = 0, float timeToFailSeconds = float.PositiveInfinity, bool paused = false)
        {
            Tick = tick; WaterVolumeM3 = waterVolumeM3; WaterHeightM = waterHeightM;
            InflowM3PerSec = inflowM3PerSec; OutflowM3PerSec = outflowM3PerSec;
            LeakActive = leakActive; GeneratorRunning = generatorRunning;
            PowerAvailable = powerAvailable; ElectricPumpRunning = electricPumpRunning;
            Phase = phase; LastRejectReason = lastRejectReason ?? "";
            GeneratorFlooded = generatorFlooded; GeneratorGeometryAvailable = generatorGeometryAvailable;
            FuelRemainingSeconds = fuelRemainingSeconds; SecondsAboveFail = secondsAboveFail;
            TimeToFailSeconds = timeToFailSeconds;
            Paused = paused;
        }
    }

    public interface ISimulationHost
    {
        SimSnapshot Current { get; }
        void Enqueue(SimCommand cmd);
        event Action<SimSnapshot> OnTick;
        void ForceGeneratorFault();
    }

    public interface IAuthority { bool IsAuthoritative { get; } }
    public sealed class LocalAuthority : IAuthority { public bool IsAuthoritative => true; }

    /// <summary>Published scene measurements, in world Y. Missing plinth data disables power.</summary>
    public interface ISimulationGeometry
    {
        double FloorWorldY { get; }
        bool TryGetGeneratorPlinthTopWorldY(out double height);
    }

    public sealed class FixedSimulationGeometry : ISimulationGeometry
    {
        public double FloorWorldY { get; }
        readonly double? plinth;

        public FixedSimulationGeometry(double floorWorldY, double? generatorPlinthTopWorldY)
        {
            if (!Finite(floorWorldY) || (generatorPlinthTopWorldY.HasValue &&
                (!Finite(generatorPlinthTopWorldY.Value) || generatorPlinthTopWorldY.Value <= floorWorldY)))
                throw new ArgumentOutOfRangeException(nameof(generatorPlinthTopWorldY));
            FloorWorldY = floorWorldY; plinth = generatorPlinthTopWorldY;
        }

        public bool TryGetGeneratorPlinthTopWorldY(out double height)
        { height = plinth.GetValueOrDefault(); return plinth.HasValue; }

        internal static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
