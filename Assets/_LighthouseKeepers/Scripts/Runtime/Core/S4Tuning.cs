using UnityEngine;

namespace LighthouseKeepers.Core
{
    /// <summary>Opt-in S4 settings. The S3 day assets and tracked hand rig are never rewritten.</summary>
    [CreateAssetMenu(menuName = "Lighthouse/S4 hardening tuning")]
    public sealed class S4Tuning : ScriptableObject
    {
        public bool enabledForWatch = true;
        public bool difficulty = true, highWaterLoss = true, blackout = true;
        public bool starterGrip = true, soundPass = true, nightSequence = true, crewVoice = true;
        public bool desktopSleep = true;
        [Header("Difficulty")]
        [Range(.25f, 1)] public float dayLengthMultiplier = .75f;
        [Range(.25f, 1)] public float beaconLimitMultiplier = .8f;
        [Min(0)] public float poweredRiseMPerSecond = .002f;
        [Min(0)] public float unpoweredRiseMPerSecond = .006f;
        [Min(0)] public float drainMPerSecond = .012f;
        [Range(0, 1)] public float slipRetainedFraction = .5f;
        [Header("Flood loss")]
        public float failWaterHeightM = .55f;
        [Min(.1f)] public float failWaterSustainSeconds = 3;
        [Header("Blackout")]
        [Range(.01f, 1)] public float emergencyMinimum = .10f;
        [Range(.01f, 1)] public float emergencyMaximum = .15f;
        [Min(2)] public float emergencyCycleSeconds = 2;
        [Min(1)] public float warmRecoverySeconds = 1;
        public Color emergencyTint = new Color(1, .22f, .13f);
        [Min(0)] public float guideLampIntensity = .22f;
        [Min(.1f)] public float guideLampRangeM = 1.5f;
        [Header("Night")]
        [Min(.1f)] public float fadeOutSeconds = 1, nightHoldSeconds = 2, fadeInSeconds = 1;
        [Header("Sound")]
        [Range(0, 1)] public float effectsVolume = .3f, weatherVolume = .12f, waterVolume = .12f;
        [Range(0, 1)] public float alarmVolume = .04f, alarmHumVolume = .012f;
        [Min(1)] public float alarmFadeSeconds = 10;
        [Header("Crew voice (public Photon application identifier)")]
        public string voiceAppId;
        [Min(.1f)] public float voiceNearM = 2, voiceFarM = 12;
        [Range(0, 1)] public float voiceVolume = .85f;

        static S4Tuning current;
        public static S4Tuning Current
        {
            get
            {
#if UNITY_EDITOR
                // Reproduce the exact S3 baseline without modifying assets or build settings.
                if (System.Environment.GetEnvironmentVariable("LK_S4_DISABLED") == "1") return null;
#endif
                if (!current) current = Resources.Load<S4Tuning>("S4Tuning");
                return current && current.enabledForWatch ? current : null;
            }
        }
        public float EmergencyPulse(float seconds) => Mathf.Lerp(emergencyMinimum, emergencyMaximum,
            .5f + .5f * Mathf.Sin(seconds * Mathf.PI * 2 / Mathf.Max(2, emergencyCycleSeconds)));
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetCache() => current = null;
    }
}
