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

Recorded lines are now shipped. The old TTS generator is retained as a development fallback; do not run it over the selected recordings.

## Recorded takes

Teammate recordings replace TTS. Automatic selection uses clipping, edge noise and middle-length preference; subjective delivery awaits headset listening. Every output is one take, mono 44.1 kHz, high-pass 80 Hz, gentle denoise/compression, two-pass −18 LUFS normalization, true peak ≤ −1 dBTP. Unity adds the radio filter. Originals are untouched.

| Line | Take | Seconds | LUFS | True peak dBTP |
|---|---:|---:|---:|---:|
| `keeper_all_done` | 1 | 3.74 | -18.0 | -1.9 |
| `keeper_arrival` | 2 | 8.76 | -18.0 | -3.1 |
| `keeper_early_bed` | 2 | 2.02 | -18.0 | -3.2 |
| `keeper_first_bolt` | 2 | 4.94 | -18.0 | -2.7 |
| `keeper_generator` | 3 | 5.68 | -18.0 | -3.9 |
| `keeper_hint_beacon` | 1 | 4.92 | -18.0 | -3.9 |
| `keeper_hint_bed` | 2 | 2.72 | -18.0 | -4.2 |
| `keeper_hint_generator` | 2 | 7.14 | -18.0 | -3.2 |
| `keeper_hint_pipe` | 2 | 3.72 | -18.0 | -3.2 |
| `keeper_hint_power` | 3 | 3.70 | -18.0 | -1.5 |
| `keeper_hint_radio` | 3 | 5.10 | -18.0 | -3.4 |
| `keeper_lose` | 2 | 3.80 | -18.0 | -4.2 |
| `keeper_next_night` | 3 | 4.28 | -18.0 | -1.6 |
| `keeper_night_three` | 3 | 4.08 | -18.0 | -3.2 |
| `keeper_night_two` | 3 | 4.90 | -18.0 | -2.8 |
| `keeper_win` | 3 | 4.26 | -18.0 | -2.9 |
| `keeper_wrench` | 2 | 4.20 | -18.0 | -3.2 |

Selection reason for each row: unclipped, cleanly bounded take with a middle-length preference and measured edge-noise penalty. Change that line’s `take` (1–3) in `Tools/S4/radio_takes.json`, then rerun `Tools/S4/process_radio_takes.py` with the source folder. All alternatives are in gitignored `Recordings/Radio/takes/`. These take files can also be copied individually into `Recordings/Radio/` for the Unity import menu; never import a three-take source as a finished line.

## Status + danger — master pass placeholders

Original teammate recordings are unchanged. The following **offline macOS Daniel TTS placeholders** are mono 44.1 kHz, normalized to -18 LUFS / -1 dBTP, with the existing PA radio filtering. Replace WAVs at `Assets/Audio/Radio/Resources/MasterRadio/` with same-named teammate recordings; captions live in the central KeeperStrings table. One file is one take.

| File | Line |
|---|---|
| `keeper_good.wav` | Good. |
| `keeper_holds.wav` | That will hold. |
| `keeper_engine_caught.wav` | There she goes. |
| `keeper_light_burning.wav` | Light is burning. |
| `keeper_time_three.wav` | Three hours to dawn. |
| `keeper_time_two.wav` | Two hours to dawn. |
| `keeper_time_one.wav` | One hour to dawn. |
| `keeper_time_almost.wav` | Almost dawn. Hold on. |
| `keeper_water_knee.wav` | Water is at your knees, keeper. |
| `keeper_water_generator.wav` | It is reaching the generator! |
| `keeper_water_critical.wav` | You are going under, keeper! |
| `keeper_light_out.wav` | Your light is out. There are ships out there! |
| `keeper_light_thirty.wav` | Thirty seconds, keeper! |
| `keeper_light_ten.wav` | Ten seconds! |
| `keeper_grip_only.wav` | Hold grip to take the wrench. Bring its head to the bolt, then pull toward you. |
| `keeper_loop_jobs.wav` | Repair tonight's jobs. Sleep upstairs when they are done. |
| `keeper_loop_win.wav` | Three nights. Keep the light burning and the water down. Bring every ship home. |
| `keeper_leave_wrench.wav` | Leave the wrench, keeper. Follow the lamps to your next job. |
| `keeper_next_pipe.wav` | The pipe needs you. Take the wrench downstairs. |
| `keeper_next_generator.wav` | Next, the generator. Follow the lamps upstairs. |
| `keeper_next_radio.wav` | Head to the radio. Send the storm warning. |
| `keeper_next_beacon.wav` | Up to the lantern, keeper. Light it and line up the marks. |
| `keeper_next_power.wav` | Check the breakers beside the generator. |
| `keeper_next_sleep.wav` | Jobs done. Grip your bunk upstairs. |
| `keeper_soft_warning.wav` | Follow the hiss, keeper. Take your time with the wrench. |
