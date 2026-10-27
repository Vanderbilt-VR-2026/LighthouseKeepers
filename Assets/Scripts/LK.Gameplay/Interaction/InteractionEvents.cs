using System;
using UnityEngine;

namespace LighthouseKeepers.Gameplay.Interaction
{
    public interface IHandPoseTarget { HandPose RequiredPose { get; } }
    public enum HandPose { Open, Fist, WrapGrip, Pinch, PumpHandle }

    // Discrete local intent. Simulation/progression/network owners subscribe and validate authority.
    public static class InteractionEvents
    {
        public static event Action<int> OnPumpStrokeCompleted;
        public static event Action<int> OnLeakRepairCompleted;
        public static event Action<int> OnGeneratorStartCompleted;
        public static event Action<int> OnPrimerStrokeCompleted;
        public static event Action<int, bool> OnChokeChanged;
        public static event Action<int> OnGeneratorStartFailed;
        internal static void Pump(int actor) => OnPumpStrokeCompleted?.Invoke(actor);
        internal static void Repair(int actor) => OnLeakRepairCompleted?.Invoke(actor);
        internal static void StartGenerator(int actor) => OnGeneratorStartCompleted?.Invoke(actor);
        internal static void Primer(int actor) => OnPrimerStrokeCompleted?.Invoke(actor);
        internal static void Choke(int actor, bool closed) => OnChokeChanged?.Invoke(actor, closed);
        internal static void FailedCrank(int actor) => OnGeneratorStartFailed?.Invoke(actor);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            OnPumpStrokeCompleted = null; OnLeakRepairCompleted = null;
            OnGeneratorStartCompleted = null; OnPrimerStrokeCompleted = null;
            OnChokeChanged = null; OnGeneratorStartFailed = null;
        }
    }
}
