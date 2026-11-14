# TensionDirector evidence — step 1.2 / issue #92

- Unity 6000.3.23f1, muted Editor, sprint-5. 82/82 focused checks pass (`focused-tests.json`).
- Actual XRI graphics replay: offline lobby, solo Nights 1–3, physical pipe/fuel/primer/choke/starter/beacon/radio, sleep into Night4; zero logged errors. `result.json` and `loop-evidence.txt`.
- Reviewed House arrival, generator view and Night3 completed board images. The legacy filename `05-emergency-f2.png` is captured before Night2's fuel exhaustion; it is **not** proof of blackout lighting.
- `task-timing.json`: all 12 task-only instruction simulations pass. No ships, human novice timing or horror acceptance implied.
- New protocol LK-S5-TENSION1 / v23. Live transport rerun follows step1.3 at the Phase1 boundary. Device rendering/fog/audio comfort and GPU are pending; no Quest was detected or launched.

Reproduce from the checkpoint commands in Next.md. Original attempts and logs are retained under artifacts/s5/tension/ and artifacts/remaining/m3/tension-*. The initial timing test still expected the former 12-second spacing; the current assertion checks the required 15-second gap.
