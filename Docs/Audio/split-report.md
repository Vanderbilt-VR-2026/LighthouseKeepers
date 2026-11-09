# Numbered memo split report

All inference ran locally using OpenAI Whisper small.en, with medium.en rechecks for non-exact matches. The original memos supplied the cut points; identical cuts were applied to converted copies. Original files were never changed. Cuts and raw alternatives are gitignored under Recordings/Radio/voice-pass. Word timestamps and exact mappings are in voice-manifest.json.

Duration differences (decoded PCM): Memo 1 +0.0278 s, Memo 2 +0.0540 s, Memo 3 +0.0314 s, Memo 4 +0.0453 s. All within ±0.1 s. FFmpeg reported an ALAC trailing-packet warning; decoded duration remains within tolerance and every numbered sentence was checked separately.

Line30 uses attempt2, following “again, thirty.” Other occurrences of “again” are part of scripted sentences, not retake commands. Number-slating ASR corrections for53 and69 are explicit in Tools/VoicePass/cut_overrides.json. Quiet edge analysis uses the converted copies, not just the originals. The match score ignores punctuation, number spelling and equivalent contraction spellings; it is ASR evidence, not a human listening verdict.

| No. | Game ID | File | Memo start–end (s) | Attempt | Duration | Word match |
|---:|---|---|---|---:|---:|---:|
| 1 | 1 | `Assets/Audio/Radio/keeper_arrival.wav` | 1: 7.680–19.470 | 1 | 11.79 s | 94% |
| 2 | 34 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_loop_jobs.wav` | 1: 25.620–29.110 | 1 | 3.49 s | 88% |
| 3 | 35 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_loop_win.wav` | 1: 32.630–37.720 | 1 | 5.09 s | 100% |
| 4 | 2 | `Assets/Audio/Radio/keeper_wrench.wav` | 1: 40.920–45.600 | 1 | 4.68 s | 100% |
| 5 | 33 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_grip_only.wav` | 1: 48.980–53.300 | 1 | 4.32 s | 94% |
| 6 | 3 | `Assets/Audio/Radio/keeper_first_bolt.wav` | 1: 56.590–62.500 | 1 | 5.91 s | 100% |
| 7 | 36 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_leave_wrench.wav` | 1: 65.660–69.540 | 1 | 3.88 s | 100% |
| 8 | 13 | `Assets/Audio/Radio/keeper_night_two.wav` | 1: 72.560–77.390 | 1 | 4.83 s | 100% |
| 9 | 4 | `Assets/Audio/Radio/keeper_generator.wav` | 1: 80.440–85.750 | 1 | 5.31 s | 100% |
| 10 | 14 | `Assets/Audio/Radio/keeper_night_three.wav` | 1: 88.880–93.060 | 1 | 4.18 s | 100% |
| 11 | 15 | `Assets/Audio/Radio/keeper_next_night.wav` | 1: 96.140–101.350 | 1 | 5.21 s | 89% |
| 12 | 37 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_next_pipe.wav` | 1: 104.190–106.970 | 1 | 2.78 s | 100% |
| 13 | 38 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_next_generator.wav` | 1: 110.120–113.320 | 1 | 3.20 s | 100% |
| 14 | 39 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_next_radio.wav` | 1: 116.570–119.470 | 1 | 2.90 s | 100% |
| 15 | 40 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_next_beacon.wav` | 1: 122.400–125.670 | 1 | 3.27 s | 100% |
| 16 | 41 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_next_power.wav` | 1: 128.620–130.510 | 1 | 1.89 s | 100% |
| 17 | 42 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_next_sleep.wav` | 1: 133.130–136.710 | 1 | 3.58 s | 100% |
| 18 | 7 | `Assets/Audio/Radio/keeper_hint_pipe.wav` | 1: 140.180–143.430 | 1 | 3.25 s | 89% |
| 19 | 8 | `Assets/Audio/Radio/keeper_hint_generator.wav` | 1: 147.130–155.450 | 1 | 8.32 s | 93% |
| 20 | 9 | `Assets/Audio/Radio/keeper_hint_power.wav` | 1: 158.570–161.910 | 1 | 3.34 s | 100% |
| 21 | 10 | `Assets/Audio/Radio/keeper_hint_beacon.wav` | 1: 164.710–169.110 | 1 | 4.40 s | 100% |
| 22 | 11 | `Assets/Audio/Radio/keeper_hint_radio.wav` | 1: 172.840–178.010 | 1 | 5.17 s | 88% |
| 23 | 12 | `Assets/Audio/Radio/keeper_hint_bed.wav` | 1: 181.080–183.880 | 1 | 2.80 s | 100% |
| 24 | 16 | `Assets/Audio/Radio/keeper_early_bed.wav` | 1: 186.990–189.370 | 1 | 2.38 s | 100% |
| 25 | 20 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_holds.wav` | 1: 192.450–193.360 | 1 | 0.91 s | 100% |
| 26 | 21 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_engine_caught.wav` | 1: 196.180–197.420 | 1 | 1.24 s | 100% |
| 27 | 22 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_light_burning.wav` | 1: 200.120–201.230 | 1 | 1.11 s | 100% |
| 28 | 5 | `Assets/Audio/Radio/keeper_all_done.wav` | 1: 203.970–206.930 | 1 | 2.96 s | 90% |
| 29 | 17 | `Assets/Audio/Radio/keeper_win.wav` | 1: 209.650–213.250 | 1 | 3.60 s | 77% |
| 30 | 44 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_0_0.wav` | 2: 17.950–22.160 | 2 | 4.21 s | 100% |
| 31 | 45 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_0_1.wav` | 2: 25.360–28.790 | 1 | 3.43 s | 100% |
| 32 | 46 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_1_0.wav` | 2: 32.130–36.240 | 1 | 4.11 s | 100% |
| 33 | 47 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_1_1.wav` | 2: 39.330–43.020 | 1 | 3.69 s | 100% |
| 34 | 48 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_2_0.wav` | 2: 46.100–49.820 | 1 | 3.72 s | 100% |
| 35 | 49 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_2_1.wav` | 2: 52.880–56.480 | 1 | 3.60 s | 100% |
| 36 | 50 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_3_0.wav` | 2: 59.490–63.120 | 1 | 3.63 s | 100% |
| 37 | 51 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_0_3_1.wav` | 2: 66.250–70.360 | 1 | 4.11 s | 100% |
| 38 | 52 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_0_0.wav` | 2: 72.880–76.380 | 1 | 3.50 s | 100% |
| 39 | 53 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_0_1.wav` | 2: 79.080–82.240 | 1 | 3.16 s | 100% |
| 40 | 54 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_1_0.wav` | 2: 84.470–88.070 | 1 | 3.60 s | 100% |
| 41 | 55 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_1_1.wav` | 2: 90.750–94.330 | 1 | 3.58 s | 100% |
| 42 | 56 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_2_0.wav` | 2: 96.620–100.490 | 1 | 3.87 s | 100% |
| 43 | 57 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_2_1.wav` | 2: 103.250–107.840 | 1 | 4.59 s | 100% |
| 44 | 58 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_3_0.wav` | 2: 110.240–114.340 | 1 | 4.10 s | 100% |
| 45 | 59 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_1_3_1.wav` | 2: 116.960–121.510 | 1 | 4.55 s | 100% |
| 46 | 60 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_0_0.wav` | 2: 123.840–127.290 | 1 | 3.45 s | 100% |
| 47 | 61 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_0_1.wav` | 2: 129.420–132.790 | 1 | 3.37 s | 100% |
| 48 | 62 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_1_0.wav` | 2: 134.980–138.910 | 1 | 3.93 s | 100% |
| 49 | 63 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_1_1.wav` | 2: 141.460–144.660 | 1 | 3.20 s | 96% |
| 50 | 64 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_2_0.wav` | 2: 147.080–150.840 | 1 | 3.76 s | 100% |
| 51 | 65 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_2_1.wav` | 2: 153.570–157.170 | 1 | 3.60 s | 100% |
| 52 | 66 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_3_0.wav` | 2: 160.280–164.050 | 1 | 3.77 s | 100% |
| 53 | 67 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_2_3_1.wav` | 2: 166.480–170.120 | 1 | 3.64 s | 100% |
| 54 | 68 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_0_0.wav` | 2: 172.970–176.260 | 1 | 3.29 s | 100% |
| 55 | 69 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_0_1.wav` | 2: 178.720–181.680 | 1 | 2.96 s | 100% |
| 56 | 70 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_1_0.wav` | 2: 183.990–187.700 | 1 | 3.71 s | 100% |
| 57 | 71 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_1_1.wav` | 2: 189.820–193.400 | 1 | 3.58 s | 100% |
| 58 | 72 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_2_0.wav` | 2: 196.150–200.000 | 1 | 3.85 s | 100% |
| 59 | 73 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_2_1.wav` | 2: 202.610–206.630 | 1 | 4.02 s | 100% |
| 60 | 74 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_3_0.wav` | 2: 209.070–212.800 | 1 | 3.73 s | 100% |
| 61 | 75 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_status_3_3_1.wav` | 2: 215.850–219.580 | 1 | 3.73 s | 90% |
| 62 | P1 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_night_three_pipe.wav` | 3: 8.100–11.970 | 1 | 3.87 s | 100% |
| 63 | P2 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_night_three_upper_v2.wav` | 3: 14.910–19.930 | 1 | 5.02 s | 88% |
| 64 | P3 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_set_wrench_down.wav` | 3: 23.420–26.150 | 1 | 2.73 s | 100% |
| 65 | P4 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_start_grace.wav` | 3: 30.750–34.320 | 1 | 3.57 s | 100% |
| 66 | P5 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_tonight_pipe_generator.wav` | 3: 38.590–42.330 | 1 | 3.74 s | 95% |
| 67 | P6 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_tonight_full.wav` | 3: 45.590–50.090 | 1 | 4.50 s | 74% |
| 68 | P7 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_valve_hold.wav` | 3: 53.890–57.230 | 1 | 3.34 s | 92% |
| 69 | P9 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_clamp_solo.wav` | 3: 61.040–64.120 | 1 | 3.08 s | 100% |
| 70 | P10 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_radios_tip.wav` | 3: 67.580–71.430 | 1 | 3.85 s | 100% |
| 71 | P11 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_pole_tip.wav` | 3: 75.670–78.730 | 1 | 3.06 s | 90% |
| 72 | P12 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_breaker_tripped.wav` | 3: 83.220–85.420 | 1 | 2.20 s | 80% |
| 73 | P13 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_fuel_low.wav` | 3: 89.870–93.360 | 1 | 3.49 s | 78% |
| 74 | P14 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_engine_stalled.wav` | 3: 97.650–99.950 | 1 | 2.30 s | 83% |
| 75 | P15 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_lens_fouled.wav` | 3: 103.480–106.440 | 1 | 2.96 s | 100% |
| 76 | P16 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_radio_wrong_code.wav` | 3: 110.640–112.990 | 1 | 2.35 s | 100% |
| 77 | P17 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_radio_received.wav` | 3: 116.640–119.420 | 1 | 2.78 s | 100% |
| 78 | P18 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_close_call.wav` | 3: 123.320–126.110 | 1 | 2.79 s | 100% |
| 79 | P19 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_night_four.wav` | 3: 129.980–136.000 | 1 | 6.02 s | 84% |
| 80 | P20 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_night_five.wav` | 3: 139.730–145.790 | 1 | 6.06 s | 100% |
| 81 | P23 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_ship_passed.wav` | 3: 149.240–151.510 | 1 | 2.27 s | 100% |
| 82 | P25 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_rescue_dawn.wav` | 3: 154.810–160.370 | 1 | 5.56 s | 93% |
| 83 | 27 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_water_knee.wav` | 4: 7.470–8.970 | 1 | 1.50 s | 100% |
| 84 | 28 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_water_generator.wav` | 4: 11.880–13.300 | 1 | 1.42 s | 100% |
| 85 | 29 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_water_critical.wav` | 4: 16.440–17.960 | 1 | 1.52 s | 100% |
| 86 | 30 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_light_out.wav` | 4: 21.530–23.500 | 1 | 1.97 s | 80% |
| 87 | 31 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_light_thirty.wav` | 4: 26.620–27.970 | 1 | 1.35 s | 100% |
| 88 | 32 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_light_ten.wav` | 4: 31.050–32.300 | 1 | 1.25 s | 100% |
| 89 | P8 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_valve_slipped.wav` | 4: 35.380–37.200 | 1 | 1.82 s | 100% |
| 90 | P21 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_ship_sighted.wav` | 4: 40.350–42.570 | 1 | 2.22 s | 100% |
| 91 | P22 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_ship_close.wav` | 4: 46.110–48.640 | 1 | 2.53 s | 100% |
| 92 | P26 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_partner_down.wav` | 4: 51.140–53.030 | 1 | 1.89 s | 100% |
| 93 | P24 | `Assets/Audio/Radio/Resources/MasterRadio/keeper_ship_lost.wav` | 4: 56.070–57.840 | 1 | 1.77 s | 100% |
| 94 | 18 | `Assets/Audio/Radio/keeper_lose.wav` | 4: 61.760–65.110 | 1 | 3.35 s | 100% |

## NEEDS CHECK / RE-RECORD

These are explicitly flagged, not certified exact readings. ASR can mishear accents/consonants; compare the isolated original and converted copies before deciding a retake is necessary. No clip was sped up or truncated to satisfy the duration limit.

- **1 (1)** — Keeper, you hear me? Storm's right on top of you. Keep that light burning till dawn. Lose the light, or let the water take the tower, and we're all done for.
  - ASR/script wording differs: review recording; Over 10 s; retained at natural speed. ASR: “Keeper, you hear me? Storms right on top of you. Keep that line burning till dawn. Lose the light or let the water take this tower and we're all done for.” (94%).
- **2 (34)** — Fix tonight's jobs. Sleep upstairs when they're done.
  - ASR/script wording differs: review recording. ASR: “Thanks tonight's jobs. Sleep upstairs when they're done.” (88%).
- **5 (33)** — Hold grip to take the wrench. Bring its head to the bolt, then pull toward you.
  - ASR/script wording differs: review recording. ASR: “Hold grip to take the wrench. Bring its head to the bolt, then pull towards you.” (94%).
- **11 (15)** — Still with us, keeper? Check the board. It's going to be a long one.
  - ASR/script wording differs: review recording. ASR: “Still with us, Keeper? Check the board. It's gonna be a long one.” (89%).
- **18 (7)** — Follow the hiss, keeper. Wrench on the bolt. Pull.
  - ASR/script wording differs: review recording. ASR: “Follow the his keeper, wrench on the bolt, pull.” (89%).
- **19 (8)** — The plate on the generator. Fuel, prime three, choke, pull three, choke down.
  - ASR/script wording differs: review recording. ASR: “the plate on the generator, fuel, prime, three times, choke, pull three times, choke down.” (93%).
- **22 (11)** — Four steps on the radio. Power, tune, code, then hold the mic.
  - ASR/script wording differs: review recording. ASR: “Four steps on the radio. Power and tune, code and hold the mic.” (88%).
- **28 (5)** — That'll hold. Get to your bunk, keeper. You'll need it.
  - ASR/script wording differs: review recording. ASR: “That'll hold. Get to your bug keeper. You'll need it.” (90%).
- **29 (17)** — We see your light, keeper. Every ship made it through. Get some rest.
  - ASR/script wording differs: review recording. ASR: “We see you at Lightkeeper. Every ship made it through. Get some rest.” (77%).
- **49 (63)** — Keeper — water's at your knees, one hour to dawn, the light's out.
  - ASR/script wording differs: review recording. ASR: “Keeper, water's at your knees. One hour to dawn, lights out.” (96%).
- **61 (75)** — Keeper — water's taking the tower, almost dawn, the light's out.
  - ASR/script wording differs: review recording. ASR: “Keeper, water's taking the tower, almost down. The light's out.” (90%).
- **63 (P2)** — Power's back. Now get that lantern lined up and send the storm warning.
  - ASR/script wording differs: review recording. ASR: “Power's back! Now get that lantern aligned and send the storm warning.” (88%).
- **66 (P5)** — Tonight it's the pipe and the generator. Pipe first.
  - ASR/script wording differs: review recording. ASR: “Tonight, it's just the pipe and the generator. Pipe first.” (95%).
- **67 (P6)** — Tonight it's everything, keeper. Pipe, power, lantern and the radio.
  - ASR/script wording differs: review recording. ASR: “Tonight is everything, Keeper. Pipe, power, lantern, end the radio.” (74%).
- **68 (P7)** — One of you hold that valve. The other, get on the bolt.
  - ASR/script wording differs: review recording. ASR: “One of you hold that valve, the other get on the boat.” (92%).
- **71 (P11)** — Quickest way down is the pole. Grip it and slide.
  - ASR/script wording differs: review recording. ASR: “Quickest way down is the pull. Grip it and slide.” (90%).
- **72 (P12)** — Breaker's tripped. Too much on the line.
  - ASR/script wording differs: review recording. ASR: “Break is tripped. Too much on the line.” (80%).
- **73 (P13)** — Fuel's low. Top up that tank before she dies.
  - ASR/script wording differs: review recording. ASR: “Fuel is low. Top of the tank before she dies.” (78%).
- **74 (P14)** — She's stalled. Start her up again.
  - ASR/script wording differs: review recording. ASR: “She stalled. Start her up again.” (83%).
- **79 (P19)** — Fourth night. The sea's angrier than I've ever heard it. Everything's going to go at once.
  - ASR/script wording differs: review recording. ASR: “Fourth night sees anger than I've ever heard it. Everything's going to go at once.” (84%).
- **82 (P25)** — Boat's coming for you at first light. You held the light, keeper. All five nights.
  - ASR/script wording differs: review recording. ASR: “Boats coming for you at first light. You held a light keeper all five nights.” (93%).
- **86 (30)** — Your light's out. There are ships out there!
  - ASR/script wording differs: review recording. ASR: “Your light's out! There's ships out there!” (80%).
