# Lighthouse Keepers — Voice Line Inventory

Source of truth: branch `team/s4/integrated` (head 4697d5d, the reconciled build of PRs #81–#84). Audio is the harbor master on the radio; Unity adds the radio filter, static and squelch, so **record plain voice with no effects**.

## Where lines live

| Range | Text source | Audio slot | How the game finds it |
|---|---|---|---|
| IDs 1–18 | `Assets/WatchDemo/Resources/HarborRadioLines.asset` (`subtitles`) — also documented in `Docs/Design/RadioLines.md` | `Assets/Audio/Radio/<file>.wav` | Referenced by GUID in `HarborRadioLines.asset` (`lines[]`). Replace the WAV **keeping the same filename** so the GUID/`.meta` stays. |
| IDs 19–75 | `Assets/Data/Strings/Resources/KeeperStrings.json` keys `radio.N` (caption) and `radio.file.N` (filename) | `Assets/Audio/Radio/Resources/MasterRadio/<file>.wav` | Loaded at runtime via `Resources.Load("MasterRadio/<file>")`. Same filename = automatically picked up. |
| ID 6 | — | empty slot | Retired (`keeper_hint_wrench`). Never played — **don't record**. |

Triggers come from `Assets/WatchDemo/Runtime/HarborRadio.cs` (cue IDs) and `ObjectiveDirector.cs` (hint IDs).

## Audio format (importer is picky)

- **Mono, 44.1 kHz, 16-bit PCM WAV, one take per file, under 10 seconds.** The importer rejects anything ≥10 s ("select one take first"), and a test asserts IDs 19–43 are <10 s.
- Plain voice, no reverb/radio effect (ElevenLabs output → export WAV, no added FX). The importer applies: high-pass 80 Hz, gentle denoise, edge-silence trim, light compression, loudness normalize to −18 LUFS / −1.2 dBTP.
- **IDs 1–18:** drop `<file>.m4a/.wav/.mp3` into `Recordings/Radio/` and run **Lighthouse Keepers → Import Radio Recordings** (the importer's name list covers only these 17 files).
- **IDs 19–75:** the Unity importer does **not** cover them. Either process them to the spec above and place them in `Assets/Audio/Radio/Resources/MasterRadio/` with the exact filename, or ask Codex to extend the importer's name list first (recommended).
- Captions must match what's spoken. If you change wording while recording, update the caption (asset `subtitles` for 1–18, `radio.N` in KeeperStrings.json for 19–75).


## 1. Night 1 — arrival & tutorial (onboarding)

| ID | Expected file | Line (verbatim) | Context / delivery | Current audio |
|---|---|---|---|---|
| 1 | `Assets/Audio/Radio/keeper_arrival.wav` | Keeper, you hear me? Storm's right on top of you. Keep that light burning till dawn. Lose the light, or let the water take the tower, and we're all done for. | Night 1 opening, first thing the player hears in the House (also Night 1 in onboarding mode, followed by 34 + 35). Steady, grave, a little breathless — shouting over the storm on an old set. Keep under 10 s. | HUMAN (teammate) |
| 34 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_loop_jobs.wav` | Repair tonight's jobs. Sleep upstairs when they are done. | Onboarding Night 1, right after 1. Plain, instructional, calm. | TTS placeholder |
| 35 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_loop_win.wav` | Three nights. Keep the light burning and the water down. Bring every ship home. | Onboarding Night 1, after 34 — explains the win condition. Weary resolve. | TTS placeholder |
| 2 | `Assets/Audio/Radio/keeper_wrench.wav` | There's a pipe giving out downstairs. Grab the wrench off the bench. | Night 1 after the arrival, only while the wrench hasn't been picked up. Urgent but practical. | HUMAN (teammate) |
| 33 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_grip_only.wav` | Hold grip to take the wrench. Bring its head to the bolt, then pull toward you. | Onboarding: first time the player picks up the wrench (teaches the grip-only wrench). Slow and clear — this is a tutorial line. | TTS placeholder |
| 3 | `Assets/Audio/Radio/keeper_first_bolt.wav` | Hold grip. Bring it to the bolt and pull toward you. Push back, pull again. | Night 1, non-onboarding mode: first time the held wrench gets within 1.3 m of the active bolt. Coaching, rhythmic ("pull... push back... pull"). SEE CONFLICT #1. | HUMAN (teammate) |
| 36 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_leave_wrench.wav` | Leave the wrench, keeper. Follow the lamps to your next job. | After the pipe is repaired and the next job is announced. Matter-of-fact. | TTS placeholder |
| 43 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_soft_warning.wav` | Follow the hiss, keeper. Take your time with the wrench. | Defined for Night 1 soft warnings (no fail on Night 1) — NO TRIGGER FOUND in code (see flag). Gentle, reassuring. | TTS placeholder |

## 2. Night openings (Nights 2–3 and later)

| ID | Expected file | Line (verbatim) | Context / delivery | Current audio |
|---|---|---|---|---|
| 13 | `Assets/Audio/Radio/keeper_night_two.wav` | We saw your light last night. Another front's coming in. Generator's down again. | Night 2 opening (followed by 4). Relief at first, then grim. | HUMAN (teammate) |
| 4 | `Assets/Audio/Radio/keeper_generator.wav` | Generator's dead. Read the plate on the side. Prime it, choke it, pull. | Night 2, right after 13; only while the generator is off. Clipped, urgent. | HUMAN (teammate) |
| 14 | `Assets/Audio/Radio/keeper_night_three.wav` | Fuel's running low. Line up the lantern and send the storm warning. | Night 3 'upper jobs' cue: plays when the beacon + radio jobs open (after the generator restart), or as the Night 3 opening if the pipe isn't staged first. SEE CONFLICT #2. | HUMAN (teammate) |
| 15 | `Assets/Audio/Radio/keeper_next_night.wav` | Still with us, keeper? Check the board. It's going to be a long one. | Opening for Night 3 (staged) and any later night. Tired, dry humour. | HUMAN (teammate) |

## 3. Next-job guidance (Objective Director, onboarding mode)

| ID | Expected file | Line (verbatim) | Context / delivery | Current audio |
|---|---|---|---|---|
| 37 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_next_pipe.wav` | The pipe needs you. Take the wrench downstairs. | When the current job becomes the pipe (also the opening on onboarding Nights 2+). Direct. | TTS placeholder |
| 38 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_next_generator.wav` | Next, the generator. Follow the lamps upstairs. | When the current job becomes the generator. | TTS placeholder |
| 39 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_next_radio.wav` | Head to the radio. Send the storm warning. | When the current job becomes the radio. | TTS placeholder |
| 40 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_next_beacon.wav` | Up to the lantern, keeper. Light it and line up the marks. | When the current job becomes the beacon. | TTS placeholder |
| 41 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_next_power.wav` | Check the breakers beside the generator. | When the current job becomes the breaker panel. | TTS placeholder |
| 42 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_next_sleep.wav` | Jobs done. Grip your bunk upstairs. | When all jobs are done and sleep is next. | TTS placeholder |

## 4. Idle hints (after 90 s with no progress on the current job; once per job per night)

| ID | Expected file | Line (verbatim) | Context / delivery | Current audio |
|---|---|---|---|---|
| 7 | `Assets/Audio/Radio/keeper_hint_pipe.wav` | Follow the hiss, keeper. Wrench on the bolt. Pull. | Pipe job idle. Patient, short. | HUMAN (teammate) |
| 8 | `Assets/Audio/Radio/keeper_hint_generator.wav` | The plate on the generator. Fuel, prime three, choke, pull three, choke down. | Generator job idle. Read like a checklist, steady rhythm. | HUMAN (teammate) |
| 9 | `Assets/Audio/Radio/keeper_hint_power.wav` | Check the breakers. You can't run everything at once. | Beacon has no power. | HUMAN (teammate) |
| 10 | `Assets/Audio/Radio/keeper_hint_beacon.wav` | Get up to the lantern. Light it, then turn the wheel till the marks line up. | Beacon not shining. | HUMAN (teammate) |
| 11 | `Assets/Audio/Radio/keeper_hint_radio.wav` | Four steps on the radio. Power, tune, code, then hold the mic. | Radio job idle. Checklist rhythm. | HUMAN (teammate) |
| 12 | `Assets/Audio/Radio/keeper_hint_bed.wav` | Work's done. Bunk's on the third floor. | All done but not in bed. | HUMAN (teammate) |
| 16 | `Assets/Audio/Radio/keeper_early_bed.wav` | Not yet. There's still work to do. | Player tries the bunk before tasks are done. Firm, slightly stern. | HUMAN (teammate) |

## 5. Task acknowledgements

| ID | Expected file | Line (verbatim) | Context / delivery | Current audio |
|---|---|---|---|---|
| 20 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_holds.wav` | That will hold. | After a task completes (default ack, e.g. the pipe). Satisfied, short. SEE CONFLICT #3. | TTS placeholder |
| 21 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_engine_caught.wav` | There she goes. | Generator successfully started. Relief. | TTS placeholder |
| 22 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_light_burning.wav` | Light is burning. | Beacon lit. Proud, quiet. | TTS placeholder |
| 19 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_good.wav` | Good. | Defined but NO TRIGGER FOUND (see flag). | TTS placeholder |
| 5 | `Assets/Audio/Radio/keeper_all_done.wav` | That'll hold. Get to your bunk, keeper. You'll need it. | Night complete, when another night follows. Warm, tired. | HUMAN (teammate) |

## 6. Danger warnings (pressure cues; interrupt lower-priority lines)

| ID | Expected file | Line (verbatim) | Context / delivery | Current audio |
|---|---|---|---|---|
| 27 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_water_knee.wav` | Water is at your knees, keeper. | Flood reaches 50% of the loss line. Alarmed. SEE CONFLICT #3. | TTS placeholder |
| 28 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_water_generator.wav` | It is reaching the generator! | Flood reaches 75%. Shouted. | TTS placeholder |
| 29 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_water_critical.wav` | You are going under, keeper! | Flood reaches 90% — CRITICAL, can interrupt anything. Desperate shout. | TTS placeholder |
| 30 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_light_out.wav` | Your light is out. There are ships out there! | Beacon goes dark. Urgent. | TTS placeholder |
| 31 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_light_thirty.wav` | Thirty seconds, keeper! | Beacon dark, 30 s left before loss. Shouted. | TTS placeholder |
| 32 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_light_ten.wav` | Ten seconds! | Beacon dark, 10 s left — CRITICAL. Panicked shout. | TTS placeholder |
| 23 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_time_three.wav` | Three hours to dawn. | Defined but NO TRIGGER FOUND — superseded by composite status (44–75). | TTS placeholder |
| 24 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_time_two.wav` | Two hours to dawn. | Defined but NO TRIGGER FOUND — superseded by composite status. | TTS placeholder |
| 25 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_time_one.wav` | One hour to dawn. | Defined but NO TRIGGER FOUND — superseded by composite status. | TTS placeholder |
| 26 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_time_almost.wav` | Almost dawn. Hold on. | Defined but NO TRIGGER FOUND — superseded by composite status. | TTS placeholder |

## 7. Status updates (composite: time × water × beacon; on Y/X button, PA tap, or every ~2 min)

| ID | Expected file | Line (verbatim) | Context / delivery | Current audio |
|---|---|---|---|---|
| 44 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_0_0.wav` | Keeper — water's low, three hours to dawn, the light's burning. | Time bucket 0 (three hours to dawn), water band 0, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 45 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_0_1.wav` | Keeper — water's low, three hours to dawn, the light's out. | Time bucket 0 (three hours to dawn), water band 0, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 46 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_1_0.wav` | Keeper — water's at your knees, three hours to dawn, the light's burning. | Time bucket 0 (three hours to dawn), water band 1, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 47 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_1_1.wav` | Keeper — water's at your knees, three hours to dawn, the light's out. | Time bucket 0 (three hours to dawn), water band 1, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 48 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_2_0.wav` | Keeper — water's reaching the generator, three hours to dawn, the light's burning. | Time bucket 0 (three hours to dawn), water band 2, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 49 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_2_1.wav` | Keeper — water's reaching the generator, three hours to dawn, the light's out. | Time bucket 0 (three hours to dawn), water band 2, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 50 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_3_0.wav` | Keeper — water's taking the tower, three hours to dawn, the light's burning. | Time bucket 0 (three hours to dawn), water band 3, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 51 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_3_1.wav` | Keeper — water's taking the tower, three hours to dawn, the light's out. | Time bucket 0 (three hours to dawn), water band 3, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 52 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_0_0.wav` | Keeper — water's low, two hours to dawn, the light's burning. | Time bucket 1 (two hours to dawn), water band 0, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 53 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_0_1.wav` | Keeper — water's low, two hours to dawn, the light's out. | Time bucket 1 (two hours to dawn), water band 0, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 54 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_1_0.wav` | Keeper — water's at your knees, two hours to dawn, the light's burning. | Time bucket 1 (two hours to dawn), water band 1, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 55 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_1_1.wav` | Keeper — water's at your knees, two hours to dawn, the light's out. | Time bucket 1 (two hours to dawn), water band 1, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 56 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_2_0.wav` | Keeper — water's reaching the generator, two hours to dawn, the light's burning. | Time bucket 1 (two hours to dawn), water band 2, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 57 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_2_1.wav` | Keeper — water's reaching the generator, two hours to dawn, the light's out. | Time bucket 1 (two hours to dawn), water band 2, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 58 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_3_0.wav` | Keeper — water's taking the tower, two hours to dawn, the light's burning. | Time bucket 1 (two hours to dawn), water band 3, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 59 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_3_1.wav` | Keeper — water's taking the tower, two hours to dawn, the light's out. | Time bucket 1 (two hours to dawn), water band 3, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 60 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_0_0.wav` | Keeper — water's low, one hour to dawn, the light's burning. | Time bucket 2 (one hour to dawn), water band 0, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 61 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_0_1.wav` | Keeper — water's low, one hour to dawn, the light's out. | Time bucket 2 (one hour to dawn), water band 0, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 62 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_1_0.wav` | Keeper — water's at your knees, one hour to dawn, the light's burning. | Time bucket 2 (one hour to dawn), water band 1, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 63 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_1_1.wav` | Keeper — water's at your knees, one hour to dawn, the light's out. | Time bucket 2 (one hour to dawn), water band 1, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 64 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_2_0.wav` | Keeper — water's reaching the generator, one hour to dawn, the light's burning. | Time bucket 2 (one hour to dawn), water band 2, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 65 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_2_1.wav` | Keeper — water's reaching the generator, one hour to dawn, the light's out. | Time bucket 2 (one hour to dawn), water band 2, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 66 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_3_0.wav` | Keeper — water's taking the tower, one hour to dawn, the light's burning. | Time bucket 2 (one hour to dawn), water band 3, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 67 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_3_1.wav` | Keeper — water's taking the tower, one hour to dawn, the light's out. | Time bucket 2 (one hour to dawn), water band 3, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 68 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_0_0.wav` | Keeper — water's low, almost dawn, the light's burning. | Time bucket 3 (almost dawn), water band 0, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 69 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_0_1.wav` | Keeper — water's low, almost dawn, the light's out. | Time bucket 3 (almost dawn), water band 0, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 70 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_1_0.wav` | Keeper — water's at your knees, almost dawn, the light's burning. | Time bucket 3 (almost dawn), water band 1, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 71 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_1_1.wav` | Keeper — water's at your knees, almost dawn, the light's out. | Time bucket 3 (almost dawn), water band 1, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 72 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_2_0.wav` | Keeper — water's reaching the generator, almost dawn, the light's burning. | Time bucket 3 (almost dawn), water band 2, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 73 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_2_1.wav` | Keeper — water's reaching the generator, almost dawn, the light's out. | Time bucket 3 (almost dawn), water band 2, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 74 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_3_0.wav` | Keeper — water's taking the tower, almost dawn, the light's burning. | Time bucket 3 (almost dawn), water band 3, beacon lit. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |
| 75 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_3_1.wav` | Keeper — water's taking the tower, almost dawn, the light's out. | Time bucket 3 (almost dawn), water band 3, beacon dark. Calm report when safe; tenser as water/time worsen; urgent when the light's out. | TTS placeholder |

## 8. Endings

| ID | Expected file | Line (verbatim) | Context / delivery | Current audio |
|---|---|---|---|---|
| 17 | `Assets/Audio/Radio/keeper_win.wav` | We see your light, keeper. Every ship made it through. Get some rest. | Final night won (dawn). Emotional relief, slow. | HUMAN (teammate) |
| 18 | `Assets/Audio/Radio/keeper_lose.wav` | Keeper? Keeper, do you read? ...Keeper? | Night lost (flood or beacon). Starts normal, gets quieter and more uncertain; long pause before the last 'Keeper?'. | HUMAN (teammate) |

## 9. PROPOSED future lines (not in code yet — record now so they're ready)

| ID | Expected file | Line (verbatim) | Context / delivery | Current audio |
|---|---|---|---|---|
| P1 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_night_three_pipe.wav  (proposed)` | Pipe's gone again, keeper. Get the wrench on it before the water climbs. | Dedicated Night 3 opening when the pipe is staged first (Eli follow-up; today Night 3 reuses 15). | PROPOSED — not in code |
| P2 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_night_three_upper_v2.wav  (proposed)` | Power's back. Now get that lantern lined up and send the storm warning. | Rewrite of cue 14 for when it plays after the generator restart (Eli follow-up / conflict #2). | PROPOSED — not in code |
| P3 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_set_wrench_down.wav  (proposed)` | Set the wrench down, keeper. You won't need it up there. | Voice for the existing caption integrated.wrenchDown. | PROPOSED — not in code |
| P4 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_start_grace.wav  (proposed)` | Take a breath, keeper. The storm's waiting on you, but not for long. | Start-grace period (clock waits until you start working). | PROPOSED — not in code |
| P5 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_tonight_pipe_generator.wav  (proposed)` | Tonight it's the pipe and the generator. Pipe first. | Voice for the 'Tonight:' wake caption (one per night's job set). | PROPOSED — not in code |
| P6 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_tonight_full.wav  (proposed)` | Tonight it's everything, keeper. Pipe, power, lantern and the radio. | 'Tonight:' line for a full night. | PROPOSED — not in code |
| P7 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_valve_hold.wav  (proposed)` | One of you hold that valve. The other, get on the bolt. | Co-op pipe puzzle intro (valve + tighten). | PROPOSED — not in code |
| P8 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_valve_slipped.wav  (proposed)` | Valve slipped! Hold it steady! | Co-op: valve released mid-repair, spray surges. | PROPOSED — not in code |
| P9 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_clamp_solo.wav  (proposed)` | Set the clamp on the valve, then work the bolt. | Solo version of the valve puzzle. | PROPOSED — not in code |
| P10 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_radios_tip.wav  (proposed)` | Grab a handset off the bench. You'll hear each other across the floors. | Co-op: teach the handheld radios. | PROPOSED — not in code |
| P11 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_pole_tip.wav  (proposed)` | Quickest way down is the pole. Grip it and slide. | First visit to an upper floor with a fireman's pole. | PROPOSED — not in code |
| P12 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_breaker_tripped.wav  (proposed)` | Breaker's tripped. Too much on the line. | Breaker overload. | PROPOSED — not in code |
| P13 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_fuel_low.wav  (proposed)` | Fuel's low. Top up that tank before she dies. | Generator fuel warning mid-night. | PROPOSED — not in code |
| P14 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_engine_stalled.wav  (proposed)` | She's stalled. Start her up again. | Generator stalls. | PROPOSED — not in code |
| P15 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_lens_fouled.wav  (proposed)` | The lens is fouled with soot. Wipe it clean. | Beacon soot builds up (later nights). | PROPOSED — not in code |
| P16 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_radio_wrong_code.wav  (proposed)` | That's not it. Try the code again. | Radio: wrong button in the code sequence. | PROPOSED — not in code |
| P17 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_radio_received.wav  (proposed)` | Copy, keeper. Warning's out. Good work. | Radio broadcast successfully sent (harbor master's reply). | PROPOSED — not in code |
| P18 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_close_call.wav  (proposed)` | That was close, keeper. Too close. | Night won after a near-loss (danger above 80%). | PROPOSED — not in code |
| P19 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_night_four.wav  (proposed)` | Fourth night. The sea's angrier than I've ever heard it. Everything's going to go at once. | M3: Night 4 opening. | PROPOSED — not in code |
| P20 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_night_five.wav  (proposed)` | Last night, keeper. This is the big one. Hold on till dawn and the boat comes for you. | M3: Night 5 (finale) opening. | PROPOSED — not in code |
| P21 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_ship_sighted.wav  (proposed)` | Ship on the horizon! Keep that light steady. | M3: ship event begins. | PROPOSED — not in code |
| P22 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_ship_close.wav  (proposed)` | She's close to the rocks. Don't you let that light go out. | M3: ship approaching the rocks. | PROPOSED — not in code |
| P23 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_ship_passed.wav  (proposed)` | She's through. You did it, keeper. | M3: ship passes safely. | PROPOSED — not in code |
| P24 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_ship_lost.wav  (proposed)` | Oh no... she's on the rocks. | M3: ship wrecked (beacon dark). Quiet, devastated. | PROPOSED — not in code |
| P25 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_rescue_dawn.wav  (proposed)` | Boat's coming for you at first light. You held the light, keeper. All five nights. | M3: Night 5 won, rescue at dawn. | PROPOSED — not in code |
| P26 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_partner_down.wav  (proposed)` | Your partner's in trouble. Get to them! | Co-op: other keeper in danger (future rescue mechanic). | PROPOSED — not in code |

## Conflicts (lines defined differently in more than one place)

1. **Cue 3 (`keeper_first_bolt`).** The in-game caption (`HarborRadioLines.asset`) says *"Hold grip. Bring it to the bolt and pull toward you. Push back, pull again."* `Docs/Design/RadioLines.md` still says *"Get it on the bolt and pull it toward you. Push it back, pull again."* The existing teammate recording follows the **old** doc wording. Eli's PR #84 also asked for a trigger-mention retake, which the integrated build dropped (grip-only wrench). **Recommendation: record the caption version.** Note it overlaps heavily with cue 33 (onboarding mode plays 33; non-onboarding plays 3).
2. **Cue 14 (`keeper_night_three`).** Text opens with *"Fuel's running low"*, but in the integrated build it plays when the beacon + radio jobs open — after the generator has been restarted (Eli flagged this). Proposed rewrite is **P2** in section 9; if you use it, update the asset caption.
3. **Contractions.** The TTS-era lines (19–43) are written without contractions ("That will hold.", "It is reaching the generator!", "You are going under") while the original lines use them ("That'll hold."). A weary keeper sounds more natural with contractions. If you speak them with contractions, update the matching `radio.N` captions so subtitles match.
4. **Night-complete lines.** Cue 5 and cue 20 both say "That'll/That will hold" and can play close together at the end of a night. Consider varying one.

## Flags

### 1. Referenced but no voice text / no audio slot yet (you'll need to write or wire these)
- `integrated.wrenchDown` caption ("Set the wrench down.") — caption only, no audio slot. Proposed voice: **P3**.
- `integrated.tonight` caption ("Tonight: {0} — {1}.") — templated caption shown on waking, no audio. Proposed voices: **P5/P6** (one per night's job set).
- Eli's PR #84 follow-up list (not yet in code): "Done. Next…" spoken version, start-grace line (**P4**), dedicated Night 3 pipe opening (**P1**), cue 14 rewording (**P2**).
- Defined text but **no trigger found in code** (won't currently play): **19** "Good.", **23–26** time lines (replaced by the composite status 44–75), **43** soft warning. Record them last, or skip and ask Codex to wire or remove them.
- All of section 9 (P1–P26) is proposed — text written here, but nothing in code yet. Wiring them needs a Codex task.

### 2. Audio slots already filled
Every active slot is filled. Since you're redoing everything, this is just so you know what you're replacing:
- **Human teammate recordings (17):** IDs 1, 2, 3, 4, 5, 7–18 in `Assets/Audio/Radio/`. Selected takes and loudness are in `Docs/Design/RadioLines.md`.
- **TTS placeholders (57):** IDs 19–75 in `Assets/Audio/Radio/Resources/MasterRadio/` (macOS "Daniel" voice). All should be replaced.
- **Empty:** ID 6 (retired, skip).

### 3. Job size

| Set | Lines | Words | Est. finished audio* |
|---|---:|---:|---:|
| Current lines in code (IDs 1–75, excl. retired 6) | 74 | 727 | ~6.3 min |
| …of which composite status (44–75) | 32 | 352 | ~3.0 min |
| …of which unused (19, 23–26, 43) | 6 | 27 | ~0.3 min |
| Proposed future lines (P1–P26) | 26 | 257 | ~2.2 min |
| **Everything (current + proposed)** | 100 | 984 | ~8.6 min |

*Estimate: ~130 words/min (slow, weary delivery) + ~0.6 s per line. ElevenLabs bills the audio you convert, so the gaps between lines in a single continuous take also count. Keep gaps around 1.5–2 s, or trim them before converting.

**Ways to cut cost:** skip the 6 unused lines (or have them removed); the 32 status combinations are about half the job, and Codex could change the status to play three short recorded segments instead (water + time + light), cutting 32 files to 10. That's a code change for later.

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

The November 9 headset timing revision is authoritative: see [trigger and quiet-period rules](../Audio/triggers.md#headset-feedback-timing-and-quiet-november-9). At most two ordinary lines play back-to-back, followed by 20 seconds of quiet; stale instructions expire and location-sensitive tips wait until relevant.
