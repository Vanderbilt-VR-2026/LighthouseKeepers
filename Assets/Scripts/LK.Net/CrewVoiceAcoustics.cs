using UnityEngine;

namespace LighthouseKeepers.Net
{
    public enum CrewVoiceRoute { None, Proximity, Radio }

    /// <summary>Pure receive-side rules for where a remote keeper's voice is heard. No network state; both peers evaluate them locally.</summary>
    public static class CrewVoiceAcoustics
    {
        /// <summary>Cutoffs at or above this are treated as unfiltered, Hz.</summary>
        public const float BypassCutoffHz = 18000;

        /// <summary>0-based floor from head height; matches ObjectiveHud.FloorAt (which is 1-based).</summary>
        public static int FloorIndex(float headY, CrewVoiceAcousticsSettings s) =>
            Mathf.Clamp(Mathf.FloorToInt((headY - s.floorThresholdOffsetM) / s.floorHeightM), 0, s.floorCount - 1);

        public static bool InHouse(Vector3 head, CrewVoiceAcousticsSettings s) =>
            FloorIndex(head.y, s) == 0 && head.z < s.houseNorthEdgeZ;

        /// <summary>Gain and low-pass cutoff for a voice separated by floors and/or the House doorway.</summary>
        public static void Occlusion(int floorSeparation, bool acrossHouseDoor, CrewVoiceAcousticsSettings s, out float gain, out float cutoffHz)
        {
            floorSeparation = Mathf.Abs(floorSeparation);
            if (floorSeparation == 0) { gain = 1; cutoffHz = s.clearCutoffHz; }
            else if (floorSeparation == 1) { gain = s.oneFloorGain; cutoffHz = s.oneFloorCutoffHz; }
            else { gain = s.twoFloorsGain; cutoffHz = s.twoFloorsCutoffHz; }
            if (acrossHouseDoor) { gain *= s.houseDoorGain; cutoffHz = Mathf.Min(cutoffHz, s.houseDoorCutoffHz); }
        }

        public static void Occlusion(Vector3 listenerHead, Vector3 speakerHead, CrewVoiceAcousticsSettings s, out float gain, out float cutoffHz) =>
            Occlusion(FloorIndex(listenerHead.y, s) - FloorIndex(speakerHead.y, s), InHouse(listenerHead, s) != InHouse(speakerHead, s), s, out gain, out cutoffHz);

        /// <summary>Unity's AudioRolloffMode.Linear curve, 0-1.</summary>
        public static float DistanceGain(float distanceM, float nearM, float farM) =>
            farM <= nearM ? (distanceM <= nearM ? 1 : 0) : Mathf.Clamp01((farM - distanceM) / (farM - nearM));

        /// <summary>
        /// crewVoice is false in solo (no CrewVoiceSession): nothing plays. Otherwise the radio path is used only
        /// when the local keeper holds a radio, the remote keeper does too (unless not required), and the
        /// proximity voice would be too quiet to follow.
        /// </summary>
        public static CrewVoiceRoute Route(bool crewVoice, bool localHoldsRadio, bool remoteHoldsRadio, bool requireBothHolding,
            float proximityAudibility, float radioBelow)
        {
            if (!crewVoice) return CrewVoiceRoute.None;
            if (localHoldsRadio && (remoteHoldsRadio || !requireBothHolding) && proximityAudibility < radioBelow) return CrewVoiceRoute.Radio;
            return CrewVoiceRoute.Proximity;
        }
    }

    /// <summary>Voice-activity gate for radio squelch clicks. Step returns +1 on open, -1 on close, 0 otherwise.</summary>
    public struct CrewSquelchGate
    {
        public bool Open { get; private set; }
        float quiet;
        public int Step(float rms, float dt, float openRms, float closeRms, float hangSeconds)
        {
            if (!Open)
            {
                if (rms < openRms) return 0;
                Open = true; quiet = 0; return 1;
            }
            if (rms >= closeRms) { quiet = 0; return 0; }
            quiet += dt;
            if (quiet < hangSeconds) return 0;
            Open = false; quiet = 0; return -1;
        }
        public void Reset() { Open = false; quiet = 0; }
    }

    /// <summary>Two cascaded one-pole low-passes (12 dB/oct) per channel: voice muffled through stone. Allocation-free Process.</summary>
    public sealed class CrewVoiceLowPass
    {
        const int MaxChannels = 8;
        readonly float[] first = new float[MaxChannels], second = new float[MaxChannels];
        float coefficient = 1;
        bool bypass = true;
        public bool Bypassed => bypass;

        public static float Coefficient(float cutoffHz, int sampleRate) =>
            cutoffHz >= sampleRate * .5f ? 1 : 1 - Mathf.Exp(-2 * Mathf.PI * Mathf.Max(10, cutoffHz) / sampleRate);

        public void SetCutoff(float cutoffHz, int sampleRate)
        {
            coefficient = Coefficient(cutoffHz, sampleRate);
            bypass = cutoffHz >= CrewVoiceAcoustics.BypassCutoffHz;
        }

        public void Process(float[] data, int channels)
        {
            if (channels <= 0) return;
            int n = Mathf.Min(channels, MaxChannels);
            float k = coefficient;
            if (bypass)
            {
                // Track the signal so re-enabling the filter does not click.
                int tail = data.Length - channels;
                if (tail >= 0) for (int c = 0; c < n; c++) first[c] = second[c] = data[tail + c];
                return;
            }
            for (int i = 0; i < data.Length; i += channels)
                for (int c = 0; c < n; c++)
                {
                    first[c] += k * (data[i + c] - first[c]);
                    second[c] += k * (first[c] - second[c]);
                    data[i + c] = second[c];
                }
        }
    }

    /// <summary>Handheld radio voice: bit-crush, band-pass (RBJ high/low biquads) and soft-clip, plus optional hiss. Allocation-free Process.</summary>
    public sealed class CrewRadioStrip
    {
        const int MaxChannels = 8;
        struct Biquad { public float b0, b1, b2, a1, a2; }
        readonly float[] hx1 = new float[MaxChannels], hx2 = new float[MaxChannels], hy1 = new float[MaxChannels], hy2 = new float[MaxChannels];
        readonly float[] lx1 = new float[MaxChannels], lx2 = new float[MaxChannels], ly1 = new float[MaxChannels], ly2 = new float[MaxChannels];
        readonly float[] hold = new float[MaxChannels];
        Biquad high, low;
        float crushStep = 1, crushPhase, levels = 64, drive = 1, driveNorm = 1, hiss;
        uint noise = 0x9E3779B9u;
        /// <summary>Input RMS of the last processed block (before the radio colouring).</summary>
        public float InputRms { get; private set; }
        public bool HissEnabled { get; set; }

        public void Configure(float lowCutHz, float highCutHz, float crushRateHz, int bits, float softClipDrive, float hissAmplitude, int sampleRate)
        {
            float nyquist = sampleRate * .45f;
            high = Design(Mathf.Clamp(lowCutHz, 20, nyquist), sampleRate, true);
            low = Design(Mathf.Clamp(highCutHz, 20, nyquist), sampleRate, false);
            crushStep = Mathf.Clamp01(crushRateHz / sampleRate);
            levels = Mathf.Pow(2, Mathf.Clamp(bits, 2, 16) - 1);
            drive = Mathf.Max(1, softClipDrive); driveNorm = (1 + drive) / drive; // unity gain at full scale; louder input clamps
            hiss = Mathf.Max(0, hissAmplitude);
        }

        static Biquad Design(float hz, int rate, bool highPass)
        {
            float w = 2 * Mathf.PI * hz / rate, cos = Mathf.Cos(w), alpha = Mathf.Sin(w) / (2 * .7071f), a0 = 1 + alpha;
            float b = highPass ? (1 + cos) * .5f : (1 - cos) * .5f;
            return new Biquad { b0 = b / a0, b1 = (highPass ? -2 * b : 2 * b) / a0, b2 = b / a0, a1 = -2 * cos / a0, a2 = (1 - alpha) / a0 };
        }

        public void Process(float[] data, int channels)
        {
            if (channels <= 0) return;
            int n = Mathf.Min(channels, MaxChannels);
            double sum = 0;
            for (int i = 0; i < data.Length; i++) sum += data[i] * data[i];
            InputRms = data.Length == 0 ? 0 : (float)System.Math.Sqrt(sum / data.Length);
            float hissNow = HissEnabled ? hiss : 0;
            for (int i = 0; i < data.Length; i += channels)
            {
                crushPhase += crushStep;
                bool sample = crushPhase >= 1;
                if (sample) crushPhase -= 1;
                float grain = 0;
                if (hissNow > 0) { noise ^= noise << 13; noise ^= noise >> 17; noise ^= noise << 5; grain = ((noise & 0xFFFFFF) / 16777216f * 2 - 1) * hissNow; }
                for (int c = 0; c < n; c++)
                {
                    if (sample) hold[c] = Mathf.Round(data[i + c] * levels) / levels;
                    float x = hold[c] + grain;
                    float y = high.b0 * x + high.b1 * hx1[c] + high.b2 * hx2[c] - high.a1 * hy1[c] - high.a2 * hy2[c];
                    hx2[c] = hx1[c]; hx1[c] = x; hy2[c] = hy1[c]; hy1[c] = y;
                    float z = low.b0 * y + low.b1 * lx1[c] + low.b2 * lx2[c] - low.a1 * ly1[c] - low.a2 * ly2[c];
                    lx2[c] = lx1[c]; lx1[c] = y; ly2[c] = ly1[c]; ly1[c] = z;
                    float d = z * drive;
                    data[i + c] = Mathf.Clamp(d / (1 + Mathf.Abs(d)) * driveNorm, -1, 1);
                }
            }
        }
    }
}
