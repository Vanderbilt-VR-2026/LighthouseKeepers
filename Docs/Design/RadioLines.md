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

## Composite status — Part C preparation

Host chooses one sentence containing water, time-to-dawn and beacon status. IDs 44–75 encode 4 time buckets × 4 water bands × 2 beacon states; no new snapshot field. The expanded cue vocabulary requires LK-S4-VISUALS-C0 / v15 on both clients. Each file `keeper_status_T_W_B.wav` is one plain Daniel TTS take, mono 44.1 kHz, -18 LUFS/-1 dBTP processing; the normal PA filter remains. Replace using the same import pipeline. Captions and filenames live in KeeperStrings.json. Queued requests refresh their state when played, rather than announcing stale conditions.

## Converted keeper voice — PR #85

The 94-line script and exact mapping are in `Docs/Audio/recording-script.md`; cut/quality evidence is in `Docs/Audio/split-report.md`. All P-line triggers and imported-only future lines are listed below. Existing lines keep their current triggers unless explicitly replaced here.

Host-owned narration only: existing cue/serial/age snapshot carries the same speech and word timing to both players. Cue range expands from 75 to 101, so both builds must use **LK-S5-VOICE1** (snapshot v19). No new station state or voice-chat transport.

P1–P18 obey once-per-night gates, except P8/P16 use 20/15-second cooldowns measured at actual playback. Pending stale instructions are discarded; danger interrupts existing speech, never layers a second line. Opening/grace retain priority; situational cues precede background tutorial prose. Night 2 uses its accurate job-set opening instead of falsely saying the generator has already stopped.

| Script number | Cue | File | Exact line | Trigger |
|---|---|---|---|---|
| 62 | P1 / 76 | `keeper_night_three_pipe` | Pipe's gone again, keeper. Get the wrench on it before the water climbs. | Night 3 staged-pipe opening; replaces cue 15 |
| 63 | P2 / 77 | `keeper_night_three_upper_v2` | Power's back. Now get that lantern lined up and send the storm warning. | Upper jobs unlock after generator restart; replaces cue 14 |
| 64 | P3 / 78 | `keeper_set_wrench_down` | Set the wrench down, keeper. You won't need it up there. | Pipe complete while either keeper still holds wrench; replaces silent leave-wrench caption |
| 65 | P4 / 79 | `keeper_start_grace` | Take a breath, keeper. The storm's waiting on you, but not for long. | Start grace, after opening; discarded if grace has already ended |
| 66 | P5 / 80 | `keeper_tonight_pipe_generator` | Tonight it's the pipe and the generator. Pipe first. | Waking with exactly pipe + generator jobs; Night 2 opening |
| 67 | P6 / 81 | `keeper_tonight_full` | Tonight it's everything, keeper. Pipe, power, lantern and the radio. | Waking with all four station jobs |
| 68 | P7 / 82 | `keeper_valve_hold` | One of you hold that valve. The other, get on the bolt. | Two-keeper pipe valve puzzle active |
| 69 | P9 / 84 | `keeper_clamp_solo` | Set the clamp on the valve, then work the bolt. | Solo clamp needed on unfinished pipe |
| 70 | P10 / 85 | `keeper_radios_tip` | Grab a handset off the bench. You'll hear each other across the floors. | Both keepers in House in first 45 seconds, or first separation by two floors |
| 71 | P11 / 86 | `keeper_pole_tip` | Quickest way down is the pole. Grip it and slide. | Either keeper reaches a floor with an actual pole |
| 72 | P12 / 87 | `keeper_breaker_tripped` | Breaker's tripped. Too much on the line. | Breaker tripped |
| 73 | P13 / 88 | `keeper_fuel_low` | Fuel's low. Top up that tank before she dies. | Low fuel warning, when generator job is available |
| 74 | P14 / 89 | `keeper_engine_stalled` | She's stalled. Start her up again. | Running engine stops |
| 75 | P15 / 90 | `keeper_lens_fouled` | The lens is fouled with soot. Wipe it clean. | Beacon wipe step active |
| 76 | P16 / 91 | `keeper_radio_wrong_code` | That's not it. Try the code again. | Incorrect radio-code button; at most once per 15 seconds |
| 77 | P17 / 92 | `keeper_radio_received` | Copy, keeper. Warning's out. Good work. | Broadcast completed; replaces generic radio acknowledgement |
| 78 | P18 / 93 | `keeper_close_call` | That was close, keeper. Too close. | Night objectives won after peak danger >80%, before completion/rest line |
| 79 | P19 / 94 | `keeper_night_four` | Fourth night. The sea's angrier than I've ever heard it. Everything's going to go at once. | Imported only; future milestone, no runtime trigger |
| 80 | P20 / 95 | `keeper_night_five` | Last night, keeper. This is the big one. Hold on till dawn and the boat comes for you. | Imported only; future milestone, no runtime trigger |
| 81 | P23 / 98 | `keeper_ship_passed` | She's through. You did it, keeper. | Imported only; future milestone, no runtime trigger |
| 82 | P25 / 100 | `keeper_rescue_dawn` | Boat's coming for you at first light. You held the light, keeper. All five nights. | Imported only; future milestone, no runtime trigger |
| 89 | P8 / 83 | `keeper_valve_slipped` | Valve slipped! Hold it steady! | Valve released during unfinished repair; at most once per 20 seconds |
| 90 | P21 / 96 | `keeper_ship_sighted` | Ship on the horizon! Keep that light steady. | Imported only; future milestone, no runtime trigger |
| 91 | P22 / 97 | `keeper_ship_close` | She's close to the rocks. Don't you let that light go out. | Imported only; future milestone, no runtime trigger |
| 92 | P26 / 101 | `keeper_partner_down` | Your partner's in trouble. Get to them! | Imported only; future milestone, no runtime trigger |
| 93 | P24 / 99 | `keeper_ship_lost` | Oh no... she's on the rocks. | Imported only; future milestone, no runtime trigger |
