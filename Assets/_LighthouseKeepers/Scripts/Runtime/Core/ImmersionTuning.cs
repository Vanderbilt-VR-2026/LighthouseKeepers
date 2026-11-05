using UnityEngine;

namespace LighthouseKeepers.Core
{
    [CreateAssetMenu(menuName = "Lighthouse/Immersion tuning")]
    public sealed class ImmersionTuning : ScriptableObject
    {
        public bool enabledForWatch = true;
        public bool physicalGuidance = true, radioNarration = true, forgivingPipe = true;
        public bool triggerClutch = true;
        public bool pullTowardTightens = true;
        public bool leverPipe = true, physicalWrenchDrops = true, faultMist = true;
        [Min(.1f)] public float leverReleaseRadius = .48f;
        [Min(.05f)] public float leverReleaseHeight = .22f;
        [Min(1)] public float droppedWrenchReturnSeconds = 8;
        public bool guaranteedWrenchRecovery = true;
        [Min(.1f)] public float droppedWrenchKeeperRadius = 1.8f;
        public bool forgivingGenerator = true, atmosphere = true, logbook = true;
        [Min(.01f)] public float wrenchSnapMetres = .08f;
        [Min(.08f)] public float wrenchReleaseMetres = .18f;
        [Min(10)] public float idleHintSeconds = 90;
        [Range(0, 1)] public float wallRadioVolume = .65f, beltRadioVolume = .18f;
        [Min(.05f)] public float fuelFillRadius = .25f;
        [Range(30, 100)] public float fuelPourTiltDegrees = 60;
        [Range(.1f, 1)] public float interiorLightMultiplier = .6f;
        [Range(0, 1)] public float atmosphereVolume = .12f;
        [Min(1)] public float dawnWashSeconds = 6;
        [Range(0, .5f)] public float dawnWashIntensity = .22f;
        [Min(.05f)] public float chalkLetterSeconds = .16f;
        static ImmersionTuning current;
        public static ImmersionTuning Current
        {
            get
            {
#if UNITY_EDITOR
                if (System.Environment.GetEnvironmentVariable("LK_IMMERSION_DISABLED") == "1") return null;
#endif
                if (!current) current = Resources.Load<ImmersionTuning>("ImmersionTuning");
                return current && current.enabledForWatch ? current : null;
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetCache() => current = null;
    }

    public static class CrewNetworkVersion
    {
        // One identity for matchmaking, lobby display and the shared payload contract.
        public const string Matchmaking = "LK-S4-MASTER-B2";
        public const string Snapshot = "LK-Lab-Coop-v11";
    }
}
