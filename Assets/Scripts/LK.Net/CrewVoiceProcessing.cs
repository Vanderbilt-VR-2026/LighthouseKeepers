using Photon.Voice.Unity;
using UnityEngine;

namespace LighthouseKeepers.Net
{
    // Photon native capture requests one channel on Android and macOS. Unity's
    // microphone may return stereo, which WebRtcAudioDsp explicitly rejects.
    public sealed class CrewVoiceProcessing : MonoBehaviour
    {
        Recorder recorder;
        WebRtcAudioDsp dsp;
        AudioListener reference;
        float nextCheck;
        bool created;
        public int Channels { get; private set; }
        public bool ReferenceReady => reference && reference.isActiveAndEnabled && reference.GetComponent<Photon.Voice.Unity.AudioOutCapture>();

        public static void Configure(Recorder recorder)
        {
            recorder.SourceType = Recorder.InputSourceType.Microphone;
            recorder.MicrophoneType = Recorder.MicType.Photon;
            recorder.UseMicrophoneTypeFallback = false; // Never silently fall back to stereo capture.
            recorder.SetAndroidNativeMicrophoneSettings(false, false, false); // One DSP chain, below.
            var dsp = recorder.gameObject.AddComponent<WebRtcAudioDsp>();
            dsp.AEC = true;
            dsp.NoiseSuppression = true;
            dsp.HighPass = true;
            dsp.AGC = false; // The existing received-speech leveler already handles gain.
            dsp.VAD = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            dsp.VoiceLogger.LogLevel = Photon.Voice.LogLevel.Info;
#endif
            var monitor = recorder.gameObject.AddComponent<CrewVoiceProcessing>();
            monitor.recorder = recorder;
            monitor.dsp = dsp;
        }

        void PhotonVoiceCreated(PhotonVoiceCreatedParams value)
        {
            Channels = value.Voice.Info.Channels;
            created = Channels == 1 && value.Voice is Photon.Voice.LocalVoiceAudioShort;
            reference = null;
            nextCheck = 0;
            Debug.Log($"CREW_VOICE_DSP sourceChannels={value.AudioDesc.Channels} encodedChannels={Channels} rate={value.Voice.Info.SamplingRate} monoShort={created} aec={dsp.AEC} ns={dsp.NoiseSuppression}");
            if (!created) Debug.LogError("CREW_VOICE_DSP expected mono short capture; echo processing unavailable.");
        }
        void PhotonVoiceRemoved() { created = false; Channels = 0; reference = null; }
        void Update()
        {
            if (!created || !recorder.RecordingEnabled || Time.unscaledTime < nextCheck) return;
            nextCheck = Time.unscaledTime + .5f;
            if (ReferenceReady) return;
            // The lobby listener is replaced on entering the Lab. Rebind the
            // vendor DSP to the new output reference without restarting capture.
            foreach (var listener in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            {
                if (!listener.isActiveAndEnabled) continue;
                dsp.AEC = false;
                dsp.AEC = true;
                reference = listener;
                Debug.Log($"CREW_VOICE_DSP reference={listener.name} capture={ReferenceReady}");
                break;
            }
        }
    }
}
