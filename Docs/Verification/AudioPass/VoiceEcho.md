# Mono crew voice / echo cancellation

The Photon package rejects multichannel input in WebRtcAudioDsp. Crew voice now uses Photon's native microphone backend, which requests one channel on Android and reports one channel on macOS, instead of inheriting Unity microphone channel layout. Automatic fallback to Unity capture is disabled so a stereo device cannot silently disable processing. This does not make game output mono: stereo spatial playback is retained.

WebRTC AEC and noise suppression are enabled. Native Android AEC/AGC/NS are disabled to avoid two processors fighting each other; the existing received-speech leveler handles gain. Capture still starts only after microphone permission and only in a connected crew. Mute stops both recording and transmission. The output reference is rebound when the lobby listener is replaced by the Lab listener.

Executed in Unity 6000.3.23f1 on Mac: actual native source and encoded stream are mono; 48 kHz, 48 kbps, 20 ms. The WebRTC processor exists, AEC/NS are on, and the current listener has its output capture. Mute removes the local voice and stops recording/transmission; unmute recreates a mono stream. Host-alone lobby → Lab retains processing and rebinds to the master listener endpoint. Evidence: `artifacts/day3-balance/voice/mac-*.json` and Editor logs.

Quest/native processing and both-direction speech acceptance are pending the matching build. No replicated fields or gameplay behavior changed; the lobby network version remains PAHUD1.
