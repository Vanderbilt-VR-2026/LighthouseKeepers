using UnityEngine;

namespace LighthouseKeepers.Net
{
    /// <summary>Receive-side crew voice acoustics: floor occlusion and the handheld radio path. Read by CrewVoiceAcoustics rules and the remote-voice driver.</summary>
    [CreateAssetMenu(menuName = "Lighthouse/Crew voice acoustics")]
    public sealed class CrewVoiceAcousticsSettings : ScriptableObject
    {
        public const string ResourceName = "CrewVoiceAcoustics";

        [Header("Shell layout")]
        [Tooltip("Storey height of the tower shell, metres (floors at y 0, 3.3, 6.6, 9.9).")]
        [Min(1)] public float floorHeightM = 3.3f;
        [Tooltip("A head this far above a floor line still counts as the floor below, metres. .1 matches ObjectiveHud.FloorAt (head y < 3.4 is floor 1).")]
        [Range(0, 1)] public float floorThresholdOffsetM = .1f;
        [Tooltip("Number of floors, ground floor included.")]
        [Range(1, 8)] public int floorCount = 4;
        [Tooltip("A ground-floor head with world z below this is inside the keeper's House (House spans z -10.3 to -4.4), metres.")]
        public float houseNorthEdgeZ = -4.3f;

        [Header("Floor occlusion (multiplies the existing S4Tuning voice distance rolloff)")]
        [Tooltip("Linear gain one floor apart (.22 is about -13 dB): heard faintly through the stairwell and stone.")]
        [Range(0, 1)] public float oneFloorGain = .22f;
        [Tooltip("Low-pass cutoff one floor apart, Hz.")]
        [Range(100, 22000)] public float oneFloorCutoffHz = 550;
        [Tooltip("Linear gain two or more floors apart. 0 = silent; a radio is needed.")]
        [Range(0, 1)] public float twoFloorsGain = 0;
        [Tooltip("Low-pass cutoff two or more floors apart, Hz.")]
        [Range(100, 22000)] public float twoFloorsCutoffHz = 300;
        [Tooltip("Extra linear gain between the House and the tower (through the open throat doorway).")]
        [Range(0, 1)] public float houseDoorGain = .55f;
        [Tooltip("Low-pass cutoff between the House and the tower, Hz.")]
        [Range(100, 22000)] public float houseDoorCutoffHz = 2200;
        [Tooltip("Cutoff when nothing occludes, Hz. At or above BypassCutoffHz the filter is bypassed.")]
        [Range(1000, 22000)] public float clearCutoffHz = 22000;
        [Tooltip("Time to settle gain/cutoff after a floor change (stairs), seconds.")]
        [Min(.01f)] public float occlusionFadeSeconds = .35f;

        [Header("Handheld radios")]
        [Tooltip("Spawn the two House radios in the Lab. Both peers must use the same value (they join the shared grab layout).")]
        public bool handheldRadios = true;
        [Tooltip("Require the remote keeper to hold a radio too (uses replicated grab ownership). Falls back to local-only when the radios are not in the network layout.")]
        public bool requireBothHolding = true;
        [Tooltip("Route through the radio when the proximity voice's audibility (occlusion x distance rolloff, 0-1) is below this.")]
        [Range(0, 1)] public float radioWhenProximityBelow = .3f;
        [Tooltip("Radio path playback volume, 0-1.")]
        [Range(0, 1)] public float radioVolume = .8f;
        [Tooltip("0 = 2D, 1 = fully 3D at the held radio.")]
        [Range(0, 1)] public float radioSpatialBlend = .3f;
        [Tooltip("Radio band-pass lower edge, Hz.")]
        [Range(50, 1000)] public float radioLowCutHz = 300;
        [Tooltip("Radio band-pass upper edge, Hz.")]
        [Range(1000, 8000)] public float radioHighCutHz = 3400;
        [Tooltip("Bit-crush sample-and-hold rate, Hz.")]
        [Range(2000, 48000)] public float radioCrushRateHz = 8000;
        [Tooltip("Bit-crush quantisation depth, bits.")]
        [Range(3, 16)] public int radioBits = 7;
        [Tooltip("Soft-clip pre-gain (distortion), linear.")]
        [Range(1, 8)] public float radioDrive = 2.2f;
        [Tooltip("Carrier hiss while the squelch is open, linear amplitude.")]
        [Range(0, .1f)] public float radioHiss = .01f;
        [Tooltip("Received-speech RMS that opens the squelch.")]
        [Range(0, .2f)] public float squelchOpenRms = .02f;
        [Tooltip("RMS below which the squelch starts its hang time.")]
        [Range(0, .2f)] public float squelchCloseRms = .01f;
        [Tooltip("Silence required before the squelch closes, seconds.")]
        [Range(0, 2)] public float squelchHangSeconds = .35f;
        [Tooltip("Squelch click volume, 0-1.")]
        [Range(0, 1)] public float squelchVolume = .45f;
        [Tooltip("Radio path fade in/out time, seconds.")]
        [Min(.01f)] public float radioFadeSeconds = .15f;

        static CrewVoiceAcousticsSettings cached, fallback;
        public static CrewVoiceAcousticsSettings Current
        {
            get
            {
                if (cached) return cached;
                cached = Resources.Load<CrewVoiceAcousticsSettings>(ResourceName);
                if (cached) return cached;
                if (!fallback) { fallback = CreateInstance<CrewVoiceAcousticsSettings>(); fallback.hideFlags = HideFlags.HideAndDontSave; }
                return fallback;
            }
        }
    }
}
