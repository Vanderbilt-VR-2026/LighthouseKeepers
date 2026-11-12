# Part B tuning and switches

| Setting | Before | Part B |
|---|---|---|
| Wrench attachment | Grip + trigger clutch | Grip-only auto-snap; pull arm, release/reposition; hand rotation retained |
| Guided generator | Choke deadline / extra-input penalties | No hidden choke deadline; priming and starter counts remain 3 |
| Guided fuel target | Minimum start fuel | 65% for the full upper-floor job sequence |
| Night 1 failure | Timed/flood/beacon loss possible | Soft tutorial; no failure; local flood capped at 0.28 m |
| Night 2 outage | Starts off | Starts on, fails at 75 s |
| First generator beacon grace | Existing short/no tutorial grace | 120 s after tutorial outage |
| Night 3 solo budget | 390 s | 450 s |
| Night 3 crew budget | 330 s | 330 s |
| Walking | 1.6 m/s | 1.92 m/s |
| Turn | Snap | Snap default, optional saved smooth 45°/s |
| Pole | None | 2 m/s maximum, 4 m/s² ease, no rotation; two upper stair drops |
| Instruction pulse | Separate station systems | 1.2 s, warm current control only |
| Idle tick | None | After 10 s untouched, at most every 4 s |
| Crew reassignment | One shared job | Stable nearest available assignment, re-evaluate every 5 s |
| Radio spacing | Existing hint/once-per-night rules | At least 8 s quiet between ordinary lines; critical warnings preempt |
| Status interval | No status request | 120 s automatic silence interval, X/Y or PA request |
| Storm recovery | Independent weather | Shared stage falls one level for 20 s after a completed job |
| Player step offset | Source rig default | 0.10 m, existing stair ramps retained |

`Assets/WatchDemo/Resources/MasterPassTuning.asset` holds feature switches and new tutorial/movement values. `S4Tuning.asset` retains existing shared flood/day tuning; no new flood rate change. Switches preserve old mechanics where applicable. Both clients must use the same build and switch set. New protocol is LK-S4-MASTER-B6 / snapshot v14.

Rules-bot measured simulation: Night 1 118 s, Night 2 167 s, Night 3 345.2 s including read/navigation allowances and return to bed. Night 3 spare 104.8 s, 30.4% headroom. These are not human timings. Separate graphics runs exercise XRI mechanics, with paced travel waits. See `Docs/Playtests/telemetry-firsttimer.md`.
