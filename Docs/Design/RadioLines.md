# Harbor master — S4-M3 radio lines

Original script supplied by the lead plus short original bridge/hint lines. Placeholder recordings use offline macOS text-to-speech, with no copyrighted game audio. Plain mono recordings live in `Assets/Audio/Radio/`; replace a recording with the same filename to keep the asset reference and receive the same in-engine radio treatment. Do not pre-filter replacement voices.

The host selects/reacts to cues; all keepers hear the same cue through the wall radio and quieter local belt receiver. Cues queue without overlapping dialogue. One gentle hint after 45 seconds without relevant progress; repeated hints are rate-limited. Later nights get one brief arrival forecast, with equipment-specific help only when needed. Voice chat remains independent.

| Filename | Trigger | Spoken line |
| --- | --- | --- |
| `keeper_arrival.wav` | First House arrival, Night 1 | Keeper, do you read? Storm’s on top of you. Keep that light burning till dawn. If the water takes the tower or the light goes dark, those ships are done for — and so are you. |
| `keeper_wrench.wav` | After arrival, wrench still on bench | Grab the wrench off the bench. I can hear a pipe going downstairs. |
| `keeper_first_bolt.wav` | First active bolt approached with wrench | Short turns, keeper. Turn the wrench, bring your wrist back, and turn again. The bolt stays tightened. |
| `keeper_generator.wav` | Generator fault introduced | Generator’s quit on you. Read the plate on its side, keeper. Prime it, choke it, pull. |
| `keeper_all_done.wav` | Tonight’s tasks complete | That’ll hold. Get to your bunk upstairs, you’ll need the rest. |
| `keeper_hint_wrench.wav` | 45 s idle before taking wrench | Wrench is on the workbench, keeper. Follow that hiss to the pipe. |
| `keeper_hint_pipe.wav` | 45 s idle on pipe | Find the spraying coupling. Seat the wrench, make a short turn, then bring your wrist back. Keep cranking; your progress stays. |
| `keeper_hint_generator.wav` | 45 s idle with engine stopped | Check the fuel. Three squeezes on the primer, choke up, three pulls, then choke down. It’s on the brass plate. |
| `keeper_hint_power.wav` | 45 s idle with insufficient station power | Check the breaker panel. You can’t feed everything at once. Keep the beacon burning; switch the others as you need them. |
| `keeper_hint_beacon.wav` | 45 s idle with beacon unready | Up to the lantern, keeper. Light it, then turn the brass wheel until the marks line up. |
| `keeper_hint_radio.wav` | 45 s idle on broadcast | The radio has four steps painted on its face. Power, tune, code, then hold the mic trigger to send. |
| `keeper_hint_bed.wav` | 45 s idle after completion | Work’s done. Your bunk’s on the third floor. Both keepers need their rest. |
| `keeper_night_two.wav` | Night 2 arrival | We saw your light last night. Another front’s coming. The generator’s quit, and that pipe is leaking again. |
| `keeper_night_three.wav` | Night 3 arrival | We saw your light last night. Fuel’s low. Align the lantern and send the storm warning. Watch how you share the power. |
| `keeper_next_night.wav` | Later repeat night | We saw your light last night. Keep at it, keeper. Check tonight’s jobs on the board. |
| `keeper_early_bed.wav` | Bed attempted before tasks complete | Not yet, keeper — there’s still work to do. |

## Audio implementation acceptance

Wall radio: spatial voice, band-pass about 300–3400 Hz, light distortion, quiet static/crackle and opening/closing squelch. The belt receiver repeats the same timing at a lower level on other floors. Clip playback must respect host cue identity and elapsed time, avoid replaying stale lines on rejoin, and remain separate from Photon microphone chat. All clips and their exact generating command/source are recorded with the implementation. Final audibility, balance and localization require Quest acceptance.
