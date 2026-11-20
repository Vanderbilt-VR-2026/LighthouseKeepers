# Paired pipe work after generator start — November 19

On Nights 4–5 the objective director could send one keeper to hold the pipe valve and the other to light the beacon, even with no ship needing guidance. The pipe could not be repaired while the first keeper waited alone. With a settled engine and a two-person leak, both keepers now receive the complementary wrench/valve steps. The earlier ship-window priority and generator start requirements remain in place. Solo companion behavior is unchanged. Network protocol **LK-S5-COOPPIPE1 / LK-Lab-Coop-v55**; both peers must update together.

## Evidence and limits

- The four new Night 4/5 role-order tests first failed with `HoldValve` and `BeaconLight`. After the fix, [71 focused EditMode tests](tests.json) passed with the Editor muted, including both role orders, timed starter, companion, radio, and Survival regressions.
- [Two live muted Editors](two-peer-events.jsonl) connected through Photon and saw the same starter/assist assignment, then the same wrench/valve assignment after the engine started. Fuel and the three pulls were staged on the host. This checks transport and objective parity, not a physical-input, full-night, voice or headset run. [Receipt](two-peer-result.json).
- The [rules model](model-after.json) completed Nights 1–5 for its solo and two-worker assumptions and completed the two-worker demo. Its solo demo still timed out. The modeled `DemoNight` is an older standalone asset which the current Campaign/Survival lobby does not launch; it is not the endless Survival tech-demo mode. The model uses assumed travel and gesture times and does **not** model Owen's navigation/work or human headset play, so it does not establish current Survival difficulty or full campaign completion.
- No APK was built or installed. The Day 2 nightmare remains disabled pending full-night and worn checks; later bespoke scare scenes are still to be implemented.

## Reproduce

Run the test filters in [tests.json](tests.json) in Unity 6000.3.23f1 with Editor audio muted. For the rule model use the command in [CoopStart](../CoopStart/README.md). For the Photon fixture, back up and sync a peer worktree first, open both Editors with audio muted, and choose a fresh result directory:

```sh
LK_TEST_OUT="$PWD/artifacts/s5/campaign-audit/two-peer-pipe-repro" python3 Docs/Verification/S5/CoopPipe/fixtures/two-peer.py
```

The fixture stops Play Mode; it does not launch the Quest.
