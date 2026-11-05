# Wrench recovery and House flooding

The physical wrench now returns to its original bench pose eight seconds after release, even if a keeper remains beside it. Picking it up cancels recovery. Held tools remain under their existing XRI/Fusion lease; only the host simulates and recovers a released tool. An out-of-bounds tool returns immediately. Gravity, splash, accepted trigger clutch and hand rotation are unchanged.

The previous away-from-player timer reset whenever a keeper stood within 1.8 m. A submerged tool on the floor could therefore remain there forever. The reported water-contact detach was not reproduced: the graphics replay held the wrench underwater longer than the recovery deadline without losing selection.

| Setting | Before | After |
|---|---:|---:|
| Unheld wrench recovery | 20 s away from every keeper | 8 s, including nearby keepers |
| Powered flood rise | 0.002 m/s | 0.0019 m/s |
| Unpowered flood rise | 0.006 m/s | 0.0057 m/s |
| Drain | 0.012 m/s | unchanged |

Existing Day 3 solo/crew multipliers still apply. No timers, station rules, grips or day assets changed.

## Doorway flow

A host-owned receiving-water volume fills the doorway first, spreads across the House floor toward the board/bench, then rises. Flow responds to the difference between tower and House levels; lowering the tower lets it drain back. The additive view uses two four-vertex surfaces with moving ripples, no colliders and no shadows. Existing tower FloodController, FloodSurface, water materials and avatars were not edited.

This is a lightweight receiving-basin approximation with the existing tower height as its prescribed boundary. It is **not** a volume-conserving two-room fluid solver. The front has a simplified footprint; furniture does not displace it. The screenshots below show controlled 8 cm source water for inspection, not a timed first-timer run.

Switches: `ImmersionTuning.guaranteedWrenchRecovery`, `S4Tuning.houseDoorwayFlow`. House volume is added to the existing host snapshot, so both clients need **NETWORK VERSION LK-S4-M3-FLOOD1**, protocol `LK-Lab-Coop-v9`.

## Checks

- 23 focused checks passed: new flow/order/drain/reset/guest-authority/rate checks plus existing Day 3, objective, engine and grid checks. See [results](focused.txt).
- Graphics Play Mode: hold underwater >8 s, release nearby, recover after 8 s; doorway/House spread; real XRI three-bolt repair, drainage and bed into Day 2 passed with zero captured errors. See [trace](loop-evidence.txt).
- Screenshots inspected: [doorway](03-first-flow-doorway.png), [spreading front](04-house-spreading-front.png), [board room flooded](05-house-board-room-flooded.png), [returned wrench](02-recovered-on-bench.png).
- Headset user is unavailable. Controller feel, held-water repro on hardware and Quest visual/performance acceptance remain pending.
