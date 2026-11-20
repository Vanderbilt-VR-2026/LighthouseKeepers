# Temporary demo soundtrack — November 16

The user supplied `IT Trailer Music Version.mp3`, subsequently described it as a similar-sounding sample, and explicitly requested that the source and edited music be committed to GitHub for the demo. Archived unchanged as `Docs/Audio/Sources/DemoHorrorSample.mp3`; SHA-256 `6cbfc9a211c4aff8298e329711ecbe11f71bfa680c5ccba50cf161fc7a57d2a2`. Credit: user-supplied temporary demo sample. Composer/licence independently unverified; not labelled team-composed or CC0. Replace with the commissioned score for a published game.

| Use | Source interval | In-game duration | Measured LUFS-I | True peak |
|---|---|---:|---:|---:|
| Quiet tension / lobby | 0:00.5–0:14.5 | 12.5 s loop | -24.96 | -13.04 dBTP |
| Fault tension | 0:40–1:10 | 28.5 s loop | -24.39 | -5.28 dBTP |
| Crisis / Drowned appearance | 1:22–1:54 | 30.5 s loop | -23.09 | -6.77 dBTP |
| Appearance hit | 1:10.9–1:14.4 | 3.5 s once | -18.02 | -3.92 dBTP |

Loops crossfade their circular ends for 1.5 seconds without changing speed/pitch. Modes crossfade over two seconds at two-second transition boundaries; no claim of beat-synchronized stems. The quiet interludes now retain a low bed at 30% instead of silence. Recorded voices, speech ducking, settings sliders and critical spatial effects stay in place. The one-shot replaces the existing apparition reveal cue, so repeated frames/late join do not retrigger it. Existing positive/loss stings remain distinct feedback.

`AudioPassSettings.demoSoundtrack` enables the replacement. Turn it off to restore the prior soundtrack; missing clips fall back safely. Three stereo loops stream as Vorbis, the short spatial scare is imported mono. No extra continuously playing voices: the same two music decks and two Drowned sound sources are used. The original MP3 is outside Assets and is not packaged.

Rebuild: `python Tools/Audio/import_demo_score.py` (numpy + ffmpeg required). Individual decoded-clip loudness/peaks are measured after encoding; these are **not** whole-game output-bus or Quest listening measurements. Editors stayed muted. Headset balance and loop listening acceptance are pending.
