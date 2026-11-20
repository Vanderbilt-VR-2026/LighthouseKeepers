# Day 2 nightmare — two-Editor co-op checkpoint (#144)

Both clients joined through actual Photon with source protocol **LK-S5-NIGHTMARE1 / v53**, and both Unity Editors were muted. The feature remains **OFF** in the serialized tuning asset. These are controlled Editor fixtures, not two-headset or full-night acceptance.

- `results/two-peer-1`: the host waited until **both** players looked toward the child; the child then waited for the distant keeper and resumed after the partner approached. The host AI caught the guest, the survivor escaped, and actual XR direct grip began revival. Releasing paused progress; regripping completed it on both clients. Player positions and the transition to the escape beat were staged.
- `results/two-walk-1`: both keepers used the actual XRI locomotion provider, character capsules and the production stamina path from F2, down the authored stairs and through both doors. The host used XR direct grip/pull; the guest crossed each synchronized opening. Both reached outside in about 11 seconds without a catch. Virtual stick, A button and hand poses were injected; their initial positions and the chase beat were staged.
- `results/rejoin-1`: a real guest left and rejoined during the active return. The host restarted the scene at the safe wake state without a camera teleport; the rejoined client received the same new state.
- `results/retry-1`: with both catches staged, failure replicated to both clients. An explicit host retry cleared the downed states and placed both rigs at the separate F3 wake points. This test does not claim the entire Day 2 was played first.

Earlier solo route, gaze, dry-return and flashlight evidence: [RouteAndWake](../RouteAndWake/README.md). Still required before enabling: natural full Day 2 bedtime→follow→turn→escape→return→revive without staged beats, catch-lunge and ceiling/drop presentation review, audio balance, worn comfort/interaction checks and device GPU. Days 3–5 are not implemented.

## Reproduce

Open the sprint-5 worktree and a freshly backed-up matching peer worktree in Unity 6000.3.23f1. Set `EditorUtility.audioMasterMute=true` in each before Play. The Python fixtures use the existing `/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop/mac_pair.py` bridge. From the sprint-5 root, each `LK_TEST_OUT` must be a fresh absolute path:

```
LK_TEST_OUT="$PWD/artifacts/s5/nightmare-walk/repro-gaze" python3 Docs/Verification/S5/NightmareFoundation/Coop/fixtures/two-peer.py
LK_TEST_OUT="$PWD/artifacts/s5/nightmare-walk/repro-walk" python3 Docs/Verification/S5/NightmareFoundation/Coop/fixtures/two-walk.py
LK_TEST_OUT="$PWD/artifacts/s5/nightmare-walk/repro-rejoin" python3 Docs/Verification/S5/NightmareFoundation/Coop/fixtures/rejoin.py
LK_TEST_OUT="$PWD/artifacts/s5/nightmare-walk/repro-retry" python3 Docs/Verification/S5/NightmareFoundation/Coop/fixtures/retry.py
```

Run in order: the walk fixture creates the shared Return state consumed by rejoin; retry consumes the rejoined wake state. Do not launch either Editor's audio or the Quest app. Peer overlay was backed up under `artifacts/s5/final-scaring/peer-backup-4` before testing.
