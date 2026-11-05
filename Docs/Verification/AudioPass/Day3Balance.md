# Day 3 balance and mono voice follow-up

Branch: `s4/immersion`, draft PR #79. No merge. Days 1–2 retain their existing rules and station gestures. No avatar/body, flood water visual, or hand rotation changes.

## What changed

`S4Tuning.day3Balance` enables a staged Day 3. Arrive with power, pump and beacon working and the pipe as the active job. The generator fails at 75 seconds or after completing the pipe, whichever happens first. Restarting it and opening the choke activates the beacon and radio jobs. Pending jobs remain incomplete, are labelled LATER on the board, and cannot allow early sleep. The HUD uses the host's current objective.

The host owns stage, solo/crew budget and outage-grace deadline. These fields use the existing authenticated snapshot; guests continue sending interactions to the host. Network version is **LK-S4-M3-D3BAL1**, snapshot **LK-Lab-Coop-v8**. Both players need this build. Old versions cannot join the same room.

## Tuning — all new values in Assets/WatchDemo/Resources/S4Tuning.asset

| Day 3 setting | Before | Solo | Crew |
|---|---:|---:|---:|
| Watch duration | 315 s | 390 s | 330 s |
| Generator failure | Arrival | 75 s or pipe finished | Same |
| Beacon outage grace | 0 s | 90 s | 60 s |
| Beacon dark limit after grace | 120 s | 120 s | 120 s |
| Flood rise multiplier | 1 | 0.65 | 0.85 |
| Pump-on unrepaired rise | 0.002 m/s | 0.0013 m/s | 0.0017 m/s |
| Pump-off unrepaired rise | 0.006 m/s | 0.0039 m/s | 0.0051 m/s |
| Fuel at arrival | 0.04, stopped | 0.30, running | Same |
| Fuel at scheduled failure | — | At most 0.04 | Same |
| Fuel drain while running | 0.0025/s | 0.0015/s | Same |
| Water loss | 0.55 m for 3 s | Unchanged | Unchanged |
| Repaired drain | 0.012 m/s | Unchanged | Unchanged |

An outage grace expires even if the engine is never restarted. Delaying still loses to flood, darkness or the watch deadline. Crew settings remain for the current watch if a player leaves; the next reset/day chooses the connected mode. Turning off `day3Balance` restores the previous Day 3 rules.

## Measurement, assumptions and limits

This is a **paced automated first-timer scenario**, not a newly observed human playtest. The player was unavailable. Reading, hesitation and route-choice budgets below are explicit assumptions; they must not be presented as measured human times. No fabricated co-op first-timer result.

Before editing balance, the existing graphics baseline's physical XRI inputs measured: pipe **26.86 s**; fuel/prime/choke/pulls/open **18.76 s**; beacon, breaker and radio inputs combined **11.25 s**. These are expert automation lower bounds, not useful first-timer targets by themselves. Evidence: `artifacts/audio-pass/before/solo/loop-evidence.txt`.

A fresh continuous-locomotion measurement at 65% stick, configured speed 1.6 m/s, measured House route **7.86 s**, F1 approach **7.97 s**, F1–F2 stairs **5.89 s**, F2 transfer **6.52 s**, F2–F3 stairs **6.20 s**, F3 transfer around the bunk **10.32 s**, F3–F4 stairs **5.63 s**, F4 gallery **2.96 s**. Only the clock/flood were paused for route measurement. The old test waypoint hit the moved bunk; the test route was corrected around its outside, without changing gameplay or geometry.

| Task, including navigation/read allowance | Solo scenario budget | Crew scheduling estimate |
|---|---:|---:|
| Arrival, board, bench, House → F1 | 25 s | 25 s |
| Pipe, locating bolts, short pulls/regrips | 80 s | 80 s |
| F1 → F2 and approach | 12 s | Partner can stage there during pipe |
| Fuel, plate, prime, choke, pull, open | 55 s | 55 s, begins at outage |
| Breakers/read/allocate | 15 s | 15 s |
| F2 → F4 and approach | 25 s | Partner can stage upstairs |
| Beacon ignition/alignment | 35 s | 35 s |
| Radio, including a wrong code and released PTT | 45 s | 45 s |
| F4 → bunk, find/read sleep action | 20 s | 20–35 s, both keepers |
| Total | About 312 s | About 240–255 s with division of work |

The graphics run injects actual XRI hand/grip/trigger movements. Between stations it uses the measured walking allowance plus reading time, then positions the rig; it is not a continuous human walk-through. The crew estimate is a dependency/scheduling model, not a measured two-person first-timer run. Final run results and release APK are recorded below when complete.

## Executed balance result

19 focused rule checks passed (6 new Day 3 tests plus HUD/PA, engine, grid and emergency pulse regressions). They cover early/timed failure, pending tasks, finite grace, flood/dark loss and retry, solo/crew budget and guest read-only snapshot application, malformed/old protocol rejection and Days 1–2 unchanged.

Graphics Play Mode **passed, zero runtime errors**: pipe complete at 102.83 s; generator settled 169.99 s; beacon segment finished 245.07 s; radio segment complete 290.08 s; reached bunk with **79.92 s remaining**, then slept into authored Day 4. Peak recorded water at pipe completion was 0.14213 m. Actual scenario route total was 310.08 s. These elapsed checkpoints include deliberately paced waits; they are not pure task interaction durations. Screenshots of arrival, completion and survived page were inspected.

[Arrival / pipe objective](day3-balance/arrival.png) · [4/4 complete](day3-balance/complete.png) · [Night 3 survived](day3-balance/survived.png). [Focused results](day3-balance/focused.txt) · [Graphics run](day3-balance/solo.txt) · [Walking measurement](day3-balance/walk.txt).

Headset usability, two-way listening on the new mono build, and a new human solo/co-op first-timer timing remain **NOT VERIFIED**: the player explicitly became unavailable. The earlier accepted Mac+Quest tests belong to the previous build.
