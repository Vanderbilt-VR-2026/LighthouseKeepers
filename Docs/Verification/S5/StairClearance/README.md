# Stair-liner clearance audit

The muted Editor audit opened the saved Lab scene, temporarily moved the 56 F2–F3 and F3–F4 liner panels and their attached colliders, ran the existing navigation and work-position validators, then reopened the prior scene without saving. No gameplay geometry or baked lighting was changed.

| Temporary adjustment | Navigation edge intersections | Work-position intersections | Blocked player capsules |
|---|---:|---:|---:|
| Original scene | 61 | 5 | 0 |
| F2 liners 0.095–0.105 m outward; F3 unchanged | 1 | 5 | 0 |
| F2 0.100 m and F3 0.040 m outward | 1 | 3 | 0 |
| F2 0.120 m outward; F3 unchanged | 203 | 5 | 0 |

The 60 F2 reports are along the inside of the one-metre F2 transfer route; the remaining navigation report is at the F3 transfer. The five previously unreported work-position intersections are also F3 liner contacts. The full-suite assertion stopped at the navigation errors before it could report the work checks. Moving the F2 wall too far produces new intersections with another stair route, so the sweep does **not** justify blindly applying an offset.

The F3 work-position metadata still names an older radio and forecast board, whereas the current campaign uses that floor as the bedroom and places the radio on F4. Compare those discs with the actual bunk and current player interactions before deciding whether the metadata or geometry should change. Also inspect liner-to-railing contact, z-fighting, the effect on baked light, and real walking in both directions and with two keepers. The existing centre-line simulated traversal passed, but it does not prove the full one-metre width or worn comfort. [Detailed sweep](sweep.txt) · [additional wider-offset samples](sweep-wide.txt).

Reproduce in Unity 6000.3.23f1 with its Pipeline bridge running and the Editor out of Play Mode. The fixture refuses to replace a dirty scene and mutes the Editor. Use a fresh output path:

```sh
python3 Docs/Verification/S5/StairClearance/fixtures/sweep.py "$PWD/artifacts/s5/stair-audit/repro.txt"
```
