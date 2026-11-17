# Recovery preparation — route measured, rescue not implemented

November 17: a real NavMeshAgent walked F4 → F1 at the helper's 1.8 m/s speed, using the current baked stairs. Capsule clearance was checked every frame; no wall crossing, stuck timeout or unexpected displacement occurred.

| Measurement | Result |
| --- | --- |
| Route length | 33.34 m |
| Actual Editor travel | 19.45 s |
| Proposed hand grip | 3 s (not implemented) |
| Existing rescue allowance | 40 s |
| Remaining before deadline | 17.55 s |

This establishes a feasible helper route under this fixture, not first-time human navigation, VR comfort or a working rescue. Keep 40 seconds provisional until the actual downed/hand-grip implementation and a headset check. Lethal catches remain OFF.

Reproduce: muted Editor, Night 3, existing open routes, bridge `run_script` with `file=Tools/Coop/MeasureRecoveryRoute.cs`, `entry=CrewRecoveryRouteProbe.Run`, args containing a fresh output folder. Do not reuse a prior receipt.

Next #107 acceptance: host-owned downed/recovered frames; explicit F1 recovery position and local-only fade; completed work retained; partner location/hand cue; three-second grip with release/regrip progress; only an actually nearby living keeper can rescue; helper interrupts work and reaches the hand; safe recovery grace; expiry/both humans down causes clear failure; retry and disconnect/rejoin cleanly restore state. Bump the protocol only when that live state is added. No camera dragging and no remote instant resurrection.
