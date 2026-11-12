# Audio mix

## Sources

The team supplied **Rain Heavy Loud**, **Rain Heavy Quiet Interior**, and **Rain On Rooftop** from the YouTube Audio Library sound-effects tab. Their source screenshot is saved locally in `artifacts/audio-pass/rain-source-screenshot.png`. The team reports free use; the screenshot does not display licence text. They are not labelled Creative Commons or CC0 here. Exact off-platform licence terms remain unverified. [YouTube's Audio Library help](https://support.google.com/youtube/answer/3376882?hl=en) distinguishes standard Audio Library terms from Creative Commons entries.

Voice: original teammate recordings, one selected take per line. Music and stings: team-supplied Google Gemini/Lyria generation, documented in [Music.md](Music.md). Window taps, metal patter, puddle splashes, drips and gust wind are original procedural synthesis (`Tools/S4/process_rain.py`); no third-party samples. Earlier S4 original synthesized station sounds remain in use.

## Rain layers

The three long beds are mono, Vorbis streaming, normalized to a common −18 LUFS reference before mixing, with one-second overlap loop seams. Decoded measurements: exterior −18.10 LUFS / −0.40 dBTP; interior −18.18 / −0.66; roof −18.23 / −0.28. Codec overshoot is below full scale; source gains and the master ceiling govern final output. Originals remain untouched outside Assets.

Head position samples authored House, tower, roof and open-doorway trigger regions; transitions take about 1.5 seconds. The current doors are open routes, with partial exterior leakage at the doorway. There is no new door mechanic. House/tower interior bed gain .45, outside .65, roof .55. Bed filters: interior 2.8 kHz, roof 9 kHz, exterior 16 kHz. Rain volume is independent of flood rendering.

Four pooled spatial impact emitters: outdoors 2–5 events/second, indoors 1–2.5; window taps at nearby wall openings, F4 metal patter, exterior puddles and interior ceiling drips. Pitch .85–1.15, gain .08–.16, linear rolloff 1–8 m. Short impacts use RMS/peak levelling because integrated LUFS is undefined for <400 ms sounds. Gusts every 15–40 seconds, 5–9-second smooth envelopes, muffled indoors (1.2 kHz vs 7 kHz open air). Thunder follows the comfort-safe lightning envelope with a 1.2-second propagation delay, approximately 400 m. The old static rain beds and duplicate shell thunder are stopped while the new soundscape is enabled; rain particles and flood visuals are retained.

## Controls and verification

`AudioPassSettings` exposes separate music and layered-rain switches. Software checks are recorded in `Docs/Verification/AudioPass/README.md`. Headset mix acceptance, final co-op smoke test, master capture and device CPU cost are pending the final build. Audio sources' loudness measurements are not measurements of the complete headset mix.

## Final mixer

| Bus | Base gain | Priority | Speech duck |
|---|---:|---:|---:|
| VoiceChat | 0 dB | 0 | — |
| Radio | +1.7 dB | 10 | −3 dB while a keeper speaks |
| SFX_Critical | −3 dB | 40 | — |
| SFX_World | −6 dB | 90 | — |
| Ambience | −6.5 dB | 140 | −5 dB during radio; −3 dB during voice alone |
| Music | −2 dB | 200 | −9 dB during radio; −7 dB during voice alone |

With normalized source references, music's .23 source gain sits below the PA's .95 speech feed. PA level is about 5 dB above the prior .65 receiver (3.3 dB source increase plus 1.7 dB bus gain). Rain is 2.5 dB below its earlier calibration. These are calibrated gain targets, not measured per-bus playback LUFS. The final PA filter/mix settings are listed below; the old belt receiver is disabled when PA speakers are enabled.

Voice chat retains Photon open-mic permissions and head-position spatialization. The final codec is mono 48 kHz / 48 kbps Opus with 20 ms packets (previously 24 kHz / 24 kbps / 40 ms). Photon renames linked Speaker objects; a persistent speech component now identifies the VoiceChat bus across scene changes instead of relying on the object name. There is no binaural/HRTF plug-in configured: current 3D audio is head-position panning and distance attenuation. A post-decode speech leveler targets .09 RMS with gain .5–6, a .002 silence gate, .8 peak protection and 50 ms reduction / 600 ms recovery. The existing .85 source gain puts normal voice close to the Radio reference. Local microphone activity and remote decoded speech both drive ducking; no microphone capture is enabled in solo.

Ducking traverses its full dB range in 50 ms attack / 600 ms release. Four local, saved sliders are under Watch Menu → SOUND LEVELS: Music, Effects (world/critical/ambience), Radio, Voice Chat. Saving is debounced 500 ms. Bus routing scans only around scene startup; late voice sources register directly. Exactly 24 real voices and 64 virtual slots; priority preserves voice and guidance over beds/music.

Master: native safety compressor plus a final four-times interpolated sample ceiling at −1.5 dBFS (margin toward the requested ≤−1 dBTP output). The listener has a dedicated child at the identical tracked-head pose because Unity cannot attach a scripted master filter to a GameObject containing both a listener and a source. There is one active listener; cameras, controllers, hands and avatars retain their transforms. Quiet data below the ceiling passes unchanged. Exported-capture true peak is measured separately; the interpolator is not a claim of an ITU-certified limiter.

Development Quest logs emit `AUDIO_PERF`: MasterDSP time when the platform exposes that recorder, the isolated ceiling callback's mean CPU milliseconds, and the real-voice cap. `WATCH_PERF` retains GPU/main-thread timing. Unsupported counters must be reported as unverified, never as zero CPU cost. Raw-source/processed-clip LUFS is not a substitute for measured per-bus playback loudness.

Headset feedback follow-up: pitched rain transients sounded like xylophone notes. Window/metal/drip impacts now use damped broadband noise rather than isolated oscillators, at .08–.16 source gain. The demonstration bell is not part of normal gameplay; the test recorder was returned to Microphone and its temporary clip/loop fields cleared. Shared sleep advanced the live crew into Day 2 after Mac Host supplied the second bed vote.

## PA / objective follow-up

Six low-poly brass horns replace the belt receiver: House, F1–F4 and exterior entrance. Speech starts together using the DSP clock; the current room is full level and neighbouring rooms are reduced, with 1.5–12 m spatial rolloff and restrained .35 s reverb. Physical REPLAY LAST buttons use the existing host command/revision/proximity checks (3 s replay cooldown). The House receiver remains decoration.

PA speech uses 350–3500 Hz filters, +3 dB presence at 2.5 kHz, gentle saturation and peak compression. Opening squelch/click .3 s; closing burst/tail .4 s; faint hiss underneath. Crackle/dropout windows come only from measured pauses, never randomly over instructions. PA source .95 versus old receiver .65 plus Radio bus +1.7 dB gives about +5 dB feed gain. VoiceChat still ducks Radio by 3 dB. Radio activity ducks Music by 9 dB and Ambience by 5 dB (50/600 ms); voice alone retains 7/3 dB. Ambience base −6.5 dB reduces rain by 2.5 dB. These are gain settings, not per-bus playback LUFS measurements.

Music has 20–40 s storm-only rests every 180 s, driven by shared day/time. Crisis always bypasses the rest. Two-second rest fades preserve quiet sections. No Day 3 balance changes.

## Crew echo cancellation

Crew voice uses Photon native mono capture (48 kHz Opus / 48 kbps / 20 ms), with WebRTC AEC and noise suppression ON. Native Android AEC/AGC/NS are off to avoid stacked processing; received-speech gain control is retained. Output remains spatial stereo. The DSP output reference follows the active listener across lobby/Lab changes. See [execution evidence](../Verification/AudioPass/VoiceEcho.md).

## Recorded keeper voice pass (PR #85)

94 single takes: −18.34 to −17.74 LUFS, peaks ≤−1.35 dBTP. Offline +2.5 dB at 2.8 kHz with gentle de-essing. PA band-pass widened to 250–4500 Hz; live presence reduced to +1 dB, saturation softened, .22 s short reverb, speech hiss .003 (was .008), noise follows room falloff. Existing voice-first ducking remains. See `Docs/Audio/radio-ab.md`: five-line offline ASR A/B did not establish a consistent word-recognition gain; the lead subsequently confirmed clear, single-take speech on the installed c716030 Quest build. New timing/pole/UI feedback is documented in Docs/Audio/verification.md.
