using LighthouseKeepers.Core;
using UnityEngine;

namespace LighthouseKeepers.Net
{
    /// <summary>
    /// Receive-side only. Sits on a remote keeper's Photon voice source (3D at their head) and applies floor/House
    /// occlusion. When CrewVoiceAcoustics.Route chooses the radio, a second source replays the same Photon clip,
    /// sample-aligned, from the local keeper's held handset through the radio strip, with squelch clicks.
    /// Created only by CrewVoiceSession, so none of this runs in solo.
    /// </summary>
    public sealed class CrewVoiceSpatializer : MonoBehaviour
    {
        public int Actor;
        CrewVoiceAcousticsSettings settings;
        AudioSource voice, radio, squelch;
        CrewVoiceHead head;
        CrewVoiceOcclusionFilter occlusion;
        CrewRadioFilter radioFilter;
        CrewRadioHandset lampOn;
        CrewSquelchGate gate;
        Transform listener;
        float baseVolume, nearM, farM, gain = 1, cutoff, appliedCutoff = -1, radioLevel;
        int rate;
        public CrewVoiceRoute Route { get; private set; } = CrewVoiceRoute.Proximity;
        public float OcclusionGain => gain;
        public float CutoffHz => cutoff;

        static AudioClip openClick, closeClick;

        public static CrewVoiceSpatializer Attach(AudioSource voiceSource, int actor, S4Tuning tuning)
        {
            var s = voiceSource.gameObject.AddComponent<CrewVoiceSpatializer>();
            s.Actor = actor; s.voice = voiceSource; s.settings = CrewVoiceAcousticsSettings.Current;
            s.baseVolume = voiceSource.volume; s.nearM = tuning.voiceNearM; s.farM = tuning.voiceFarM;
            s.head = voiceSource.GetComponent<CrewVoiceHead>();
            // After the SpeechLeveler/meter KeeperAudioMix.Route added, so levelling sees the unfiltered voice.
            s.occlusion = voiceSource.gameObject.AddComponent<CrewVoiceOcclusionFilter>();
            s.rate = AudioSettings.outputSampleRate; s.cutoff = s.settings.clearCutoffHz;
            return s;
        }

        void BuildRadio()
        {
            var go = new GameObject("Remote keeper voice radio " + Actor); go.transform.SetParent(transform.parent, false);
            radio = go.AddComponent<AudioSource>(); radio.playOnAwake = false; radio.loop = true; radio.dopplerLevel = 0;
            radio.spatialBlend = settings.radioSpatialBlend; radio.rolloffMode = AudioRolloffMode.Linear; radio.minDistance = .3f; radio.maxDistance = 3;
            radio.volume = 0;
            KeeperAudioMix.Route(radio, "VoiceChat");
            if (!go.GetComponent<SpeechLeveler>()) go.AddComponent<SpeechLeveler>();
            radioFilter = go.AddComponent<CrewRadioFilter>();
            radioFilter.Strip.Configure(settings.radioLowCutHz, settings.radioHighCutHz, settings.radioCrushRateHz, settings.radioBits, settings.radioDrive, settings.radioHiss, rate);
            var click = new GameObject("Radio squelch"); click.transform.SetParent(go.transform, false);
            squelch = click.AddComponent<AudioSource>(); squelch.playOnAwake = false; squelch.dopplerLevel = 0;
            squelch.spatialBlend = settings.radioSpatialBlend; squelch.minDistance = .3f; squelch.maxDistance = 3;
            KeeperAudioMix.Route(squelch, "VoiceChat");
            if (!openClick) { openClick = SquelchClip("Radio squelch open", .045f, 1); closeClick = SquelchClip("Radio squelch close", .12f, 2); }
        }

        // Deterministic short click + band-limited noise tail, generated once per session.
        static AudioClip SquelchClip(string name, float seconds, uint seed)
        {
            const int sampleRate = 22050;
            int n = Mathf.CeilToInt(seconds * sampleRate);
            var data = new float[n]; uint x = 0x2545F491u * seed; float lp = 0;
            for (int i = 0; i < n; i++)
            {
                x ^= x << 13; x ^= x >> 17; x ^= x << 5;
                float noise = (x & 0xFFFF) / 32768f - 1; lp += .45f * (noise - lp);
                float t = (float)i / n, env = Mathf.Exp(-4 * t) * (1 - t);
                data[i] = (i < 40 ? .9f * (1 - i / 40f) : 0) + lp * .6f * env;
            }
            var clip = AudioClip.Create(name, n, 1, sampleRate, false); clip.SetData(data, 0); return clip;
        }

        void Update()
        {
            if (!voice) return;
            if (!listener) { var cam = Camera.main; if (!cam) return; listener = cam.transform; }
            var s = settings;
            float targetGain = 1, targetCutoff = s.clearCutoffHz, audible = 1;
            if (head && head.HasPose)
            {
                Vector3 me = listener.position, them = transform.position;
                CrewVoiceAcoustics.Occlusion(me, them, s, out targetGain, out targetCutoff);
                audible = targetGain * CrewVoiceAcoustics.DistanceGain(Vector3.Distance(me, them), nearM, farM);
            }
            var held = CrewRadioHandset.LocalHeld();
            bool replicated = CrewRadioHandset.OwnershipReplicated;
            Route = CrewVoiceAcoustics.Route(true, held, CrewRadioHandset.HeldBy(Actor), s.requireBothHolding && replicated, audible, s.radioWhenProximityBelow);

            float dt = Time.unscaledDeltaTime, k = 1 - Mathf.Exp(-dt / s.occlusionFadeSeconds);
            gain = Mathf.Lerp(gain, targetGain, k);
            cutoff = Mathf.Exp(Mathf.Lerp(Mathf.Log(Mathf.Max(10, cutoff)), Mathf.Log(Mathf.Max(10, targetCutoff)), k));
            float volume = baseVolume * gain;
            if (Mathf.Abs(voice.volume - volume) > .0005f) voice.volume = volume;
            if (Mathf.Abs(cutoff - appliedCutoff) > appliedCutoff * .01f) { appliedCutoff = cutoff; occlusion.LowPass.SetCutoff(cutoff, rate); }

            UpdateRadio(held, dt);
        }

        void UpdateRadio(CrewRadioHandset held, float dt)
        {
            var s = settings;
            bool want = Route == CrewVoiceRoute.Radio && held;
            if (!radio) { if (!want) return; BuildRadio(); }
            radioLevel = Mathf.MoveTowards(radioLevel, want ? 1 : 0, dt / s.radioFadeSeconds);
            if (radioLevel <= 0)
            {
                if (radio.isPlaying) radio.Stop();
                if (gate.Open) gate.Reset();
                radioFilter.Strip.HissEnabled = false;
                Lamp(null);
                return;
            }
            if (held) radio.transform.position = held.Speaker.position;
            float v = s.radioVolume * radioLevel;
            if (Mathf.Abs(radio.volume - v) > .0005f) radio.volume = v;
            Sync();
            int edge = want ? gate.Step(radioFilter.Strip.InputRms, dt, s.squelchOpenRms, s.squelchCloseRms, s.squelchHangSeconds) : 0;
            if (edge != 0 && squelch && s.squelchVolume > 0) squelch.PlayOneShot(edge > 0 ? openClick : closeClick, s.squelchVolume);
            radioFilter.Strip.HissEnabled = gate.Open;
            Lamp(want && gate.Open ? held : null);
        }

        // Photon writes a looping clip and plays it from voice.timeSamples; replay that clip at the same position.
        void Sync()
        {
            var clip = voice.clip;
            if (radio.clip != clip) { radio.Stop(); radio.clip = clip; }
            if (!clip || !voice.isPlaying) { if (radio.isPlaying) radio.Stop(); return; }
            int target = voice.timeSamples;
            if (!radio.isPlaying) { radio.timeSamples = target; radio.Play(); return; }
            int drift = Mathf.Abs(target - radio.timeSamples);
            drift = Mathf.Min(drift, clip.samples - drift);
            if (drift > clip.frequency / 30) radio.timeSamples = target; // > ~33 ms: realign (rare)
        }

        void Lamp(CrewRadioHandset on)
        {
            if (lampOn == on) return;
            if (lampOn) lampOn.SetReceiving(false);
            lampOn = on;
            if (on) on.SetReceiving(true);
        }

        void OnDestroy()
        {
            Lamp(null);
            if (radio) Destroy(radio.gameObject);
        }
    }

    public sealed class CrewVoiceOcclusionFilter : MonoBehaviour
    {
        public readonly CrewVoiceLowPass LowPass = new();
        void OnAudioFilterRead(float[] data, int channels) => LowPass.Process(data, channels);
    }

    public sealed class CrewRadioFilter : MonoBehaviour
    {
        public readonly CrewRadioStrip Strip = new();
        void OnAudioFilterRead(float[] data, int channels) => Strip.Process(data, channels);
    }
}
