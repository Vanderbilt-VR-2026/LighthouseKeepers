# Instruction-only timing pass — Part B

`LighthouseKeepers.Editor.FirstTimerDirectorBot.Run(path)` follows only the director's current step. It advances the real day/pipe/engine/grid/radio/beacon rule objects, including flood pressure. It budgets four seconds to read each step, twelve seconds per floor of travel, five-second pull attempts plus resets, and explicit fuel/tuning time. **These are deterministic simulated task times, not measured human usability or a physical locomotion test.** Graphics/XRI runs separately exercise the actual interaction implementation.

The first simulation exposed a guidance bug: the fuel instruction ended at the minimum starting fuel, which then ran dry during the upper-floor jobs. The guided instruction now stays on FILL until 65% fuel; physical pouring and engine consumption are unchanged.

| Night | Modeled job time, including navigation/read and return to bed | Original budget | New budget | Headroom |
|---|---:|---:|---:|---:|
| 1 | 118 s | 315 s with failure | No failure | Tutorial |
| 2 | 167 s | 315 s; immediate outage | 315 s; outage at 75 s + 120 s beacon grace | Deliberately generous first generator lesson |
| 3 solo | 345.2 s | 390 s | 450 s | 30.4% above modeled task time |
| 3 crew | Not remeasured here | 330 s | 330 s | Night 1 shared-task/bed/rejoin integration passed; crew Night 3 timing not remeasured |

Night 3's previous 60–90-second spare target is superseded here by the latest request for 30–40% first-timer headroom: the modeled spare is now 104.8 seconds. No flood rise or co-op timer tightening was introduced. Night 2 was not shortened merely to meet the headroom percentage; reducing its working tutorial budget would conflict with the request to stop punishing learners. All changed values are in the existing S4Tuning asset or the new master-pass feature/tutoring tuning asset.

Per-step trace: `artifacts/s4-master/part-b/firsttimer-final.txt`. Quest reach, travel time, grip usability and human first-timer results remain NOT VERIFIED. The bot does not prove a novice can find every control; the final headset checklist must cover that.
