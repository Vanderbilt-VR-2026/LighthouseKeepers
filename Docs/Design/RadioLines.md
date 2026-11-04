# Harbor master radio lines

Plain voice recordings; Unity adds the radio filter, static and squelch in 3D. Each cue plays at most once per night. Hints require 90 seconds without progress, at most one per job per night; holding the wrench or standing at the station suppresses hints. The host chooses cues for everyone.

| Filename | Cue ID | Spoken line |
|---|---|---|
| `keeper_arrival.wav` | 1 | Keeper, you hear me? Storm's right on top of you. Keep that light burning till dawn. Lose the light, or let the water take the tower, and we're all done for. |
| `keeper_wrench.wav` | 2 | There's a pipe giving out downstairs. Grab the wrench off the bench. |
| `keeper_first_bolt.wav` | 3 | Get it on the bolt and pull it toward you. Push it back, pull again. |
| `keeper_generator.wav` | 4 | Generator's dead. Read the plate on the side. Prime it, choke it, pull. |
| `keeper_all_done.wav` | 5 | That'll hold. Get to your bunk, keeper. You'll need it. |
| `keeper_hint_pipe.wav` | 7 | Follow the hiss, keeper. Wrench on the bolt. Pull. |
| `keeper_hint_generator.wav` | 8 | The plate on the generator. Fuel, prime three, choke, pull three, choke down. |
| `keeper_hint_power.wav` | 9 | Check the breakers. You can't run everything at once. |
| `keeper_hint_beacon.wav` | 10 | Get up to the lantern. Light it, then turn the wheel till the marks line up. |
| `keeper_hint_radio.wav` | 11 | Four steps on the radio. Power, tune, code, then hold the mic. |
| `keeper_hint_bed.wav` | 12 | Work's done. Bunk's on the third floor. |
| `keeper_night_two.wav` | 13 | We saw your light last night. Another front's coming in. Generator's down again. |
| `keeper_night_three.wav` | 14 | Fuel's running low. Line up the lantern and send the storm warning. |
| `keeper_next_night.wav` | 15 | Still with us, keeper? Check the board. It's going to be a long one. |
| `keeper_early_bed.wav` | 16 | Not yet. There's still work to do. |
| `keeper_win.wav` | 17 | We see your light, keeper. Every ship made it through. Get some rest. |
| `keeper_lose.wav` | 18 | Keeper? Keeper, do you read? ...Keeper? |

Cue 6 is retired (`keeper_hint_wrench`); it is never played. Win is the final authored night; intermediate completed nights use `keeper_all_done`.

Drop named .m4a, .wav or .mp3 recordings into `Recordings/Radio/`, then use **Lighthouse Keepers → Import Radio Recordings**. The importer uses FFmpeg (install on macOS with `brew install ffmpeg`), converts to mono 44.1 kHz WAV, trims leading/trailing silence, gently reduces noise and normalizes loudness. Asset GUIDs stay intact. Keep recordings plain: Unity supplies the radio sound.

TTS placeholders: macOS Daniel voice at 145 words/minute. Regenerate with `python3 Docs/Design/generate_radio_placeholders.py --force`.
