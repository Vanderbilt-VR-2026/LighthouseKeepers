# Situational recording triggers

Host-owned narration only: existing cue/serial/age snapshot carries the same speech and word timing to both players. Cue range expands from 75 to 101, so both builds must use **LK-S5-VOICE1** (snapshot v19). No new station state or voice-chat transport.

P1–P18 obey once-per-night gates, except P8/P16 use 20/15-second cooldowns measured at actual playback. Pending stale instructions are discarded; danger interrupts existing speech, never layers a second line. Opening/grace retain priority; situational cues precede background tutorial prose. Night 2 uses its accurate job-set opening instead of falsely saying the generator has already stopped.

| Script number | Cue | File | Exact line | Trigger |
|---|---|---|---|---|
| 62 | P1 / 76 | `keeper_night_three_pipe` | Pipe's gone again, keeper. Get the wrench on it before the water climbs. | Night 3 staged-pipe opening; replaces cue 15 |
| 63 | P2 / 77 | `keeper_night_three_upper_v2` | Power's back. Now get that lantern lined up and send the storm warning. | Upper jobs unlock after generator restart; replaces cue 14 |
| 64 | P3 / 78 | `keeper_set_wrench_down` | Set the wrench down, keeper. You won't need it up there. | Pipe complete while either keeper still holds wrench; replaces silent leave-wrench caption |
| 65 | P4 / 79 | `keeper_start_grace` | Take a breath, keeper. The storm's waiting on you, but not for long. | Start grace, after opening; discarded if grace has already ended |
| 66 | P5 / 80 | `keeper_tonight_pipe_generator` | Tonight it's the pipe and the generator. Pipe first. | Waking with exactly pipe + generator jobs; Night 2 opening |
| 67 | P6 / 81 | `keeper_tonight_full` | Tonight it's everything, keeper. Pipe, power, lantern and the radio. | Waking with all four station jobs, except staged Night 3 uses only its current pipe opening |
| 68 | P7 / 82 | `keeper_valve_hold` | One of you hold that valve. The other, get on the bolt. | Two-keeper pipe valve puzzle active, with a keeper at the pipe |
| 69 | P9 / 84 | `keeper_clamp_solo` | Set the clamp on the valve, then work the bolt. | Solo clamp needed, keeper carrying the wrench at the pipe |
| 70 | P10 / 85 | `keeper_radios_tip` | Grab a handset off the bench. You'll hear each other across the floors. | First separation by two floors; no unrelated handset prompt at arrival |
| 71 | P11 / 86 | `keeper_pole_tip` | Quickest way down is the pole. Grip it and slide. | Keeper within 1.8 m of a pole and their current objective is downstairs |
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
| 89 | P8 / 83 | `keeper_valve_slipped` | Valve slipped! Hold it steady! | Valve released after repair progress, while pipe is unfinished; at most once per 20 seconds |
| 90 | P21 / 96 | `keeper_ship_sighted` | Ship on the horizon! Keep that light steady. | Imported only; future milestone, no runtime trigger |
| 91 | P22 / 97 | `keeper_ship_close` | She's close to the rocks. Don't you let that light go out. | Imported only; future milestone, no runtime trigger |
| 92 | P26 / 101 | `keeper_partner_down` | Your partner's in trouble. Get to them! | Imported only; future milestone, no runtime trigger |
| 93 | P24 / 99 | `keeper_ship_lost` | Oh no... she's on the rocks. | Imported only; future milestone, no runtime trigger |

## Headset feedback: timing and quiet (November 9)

Objective transitions may follow after one second, with at most two ordinary lines back-to-back. After two, allow 20 seconds of quiet; other ordinary instructions retain eight-second spacing. Current-job instructions expire from the queue after 20 seconds and are rechecked against the live job before playback. Urgent water/light warnings retain their interrupt priority; requested status and existing critical-warning spacing remain intact. Values are in MasterPassTuning. No replicated fields changed: LK-S5-VOICE1 remains required.

Removed the queued arrival lecture, duplicate staged-Night-3 summary and generic acknowledgements that repeated the next-job/rest line. Grip teaching happens only while still looking for the wrench near the bench. Clamp/valve instructions wait until a keeper reaches the pipe; pole guidance only occurs when descending helps the current job. HUD guidance remains immediate while spoken guidance is sparse. Explicit replay is still available.
