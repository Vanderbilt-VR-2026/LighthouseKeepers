# Mono crew voice / echo cancellation

The Photon package rejects multichannel input in WebRtcAudioDsp. Crew voice now uses Photon's native microphone backend, which requests one channel on Android and reports one channel on macOS, instead of inheriting Unity microphone channel layout. Automatic fallback to Unity capture is disabled so a stereo device cannot silently disable processing. This does not make game output mono: stereo spatial playback is retained.

WebRTC AEC and noise suppression are enabled. Native Android AEC/AGC/NS are disabled to avoid two processors fighting each other; the existing received-speech leveler handles gain. Capture still starts only after microphone permission and only in a connected crew. Mute stops both recording and transmission. The output reference is rebound when the lobby listener is replaced by the Lab listener.

Executed in Unity 6000.3.23f1 on Mac: actual native source and encoded stream are mono; 48 kHz, 48 kbps, 20 ms. The WebRTC processor exists, AEC/NS are on, and the current listener has its output capture. Mute removes the local voice and stops recording/transmission; unmute recreates a mono stream. Host-alone lobby → Lab retains processing and rebinds to the master listener endpoint. Evidence: `artifacts/day3-balance/voice/mac-*.json` and Editor logs.

The voice-only checkpoint did not change replicated fields or gameplay and retained PAHUD1. The subsequent Day 3 work uses D3BAL1/v8; see [Day 3 evidence](Day3Balance.md).

Checkpoint `cca3ed4` built successfully and was installed/launched on the connected Quest. SHA-256 `03d626f02c067ede9cc404a7311450c9a50dd28aa7afc617383cbbbd3c823b74`. The user was unavailable for the headset check, so actual Android capture/AEC and audible speech in both directions remain NOT VERIFIED. Mac native capture, processing, mute/resume and listener handoff passed. Continuing to the authorized balance step with that limitation recorded.

The final two-Mac transport run verified both native captures/DSP configurations as mono with AEC/NS and listener references. Guest→host decoded mono 48 kHz actively; host→guest remote metadata was mono 48 kHz, but its silence sample was not playing. Speech intelligibility and real acoustic echo cancellation in either direction remain a listening check, especially on Quest. No claim that local feedback has been audibly eliminated.
