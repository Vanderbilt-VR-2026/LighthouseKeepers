# Sprint 4 master pass — checkpoint

Branch `s4/master`, based on main / PR #79 merge `5dc5e56`. Do not merge. Preserve unrelated untracked `Docs/Verification/Immersion/pipe-lever/` in the local worktree.

## Done

- 0–1: design pillars/guardrails recorded in `Pillars.md`.
- A1: tagged exact PR #79 merge as `sprint4-m1`, built and published **Sprint 4 — Milestone 1 (Immersion + Audio)**. APK SHA-256 `f2c0e07762ae8db2f19bde140b719b8f3e607f758604c4bba45a9a84811d4872`.
- Release: https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint4-m1
- Main download row and pillars: docs PR #80, awaiting human approval/merge. Sprint 1/2/3 rows retained. Merged `s4/immersion` branch deleted; merge/tag preserve its history.
- A2: `s4/master` created; draft PR #81: https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/81. Team playtest recorded before gameplay changes.
- Baseline recorded and committed under `Docs/Verification/S4Master/before/`: EditMode 262/262; PlayMode 34/61 (27 named failures before any gameplay edits); graphics Nights 1–3 PASS, paced Day 3 79.96 s spare; two-Mac join/avatars/mono voice plumbing/guest task/shared sleep/rejoin PASS. No human listening acceptance.
- Public release download verified against its SHA-256. No new master-pass gameplay or visuals have been applied. B1–B6 and C1–C6 remain TODO; D is not complete.

## Next, in order

Baseline is complete; **start B1 next**. Read `B1-ImplementationPlan.md`, `MasterPassRemaining.md`, the full user brief and the 27 baseline failures before changing code. Retain every passing baseline test. The live room is closed; create a fresh room for after checks. No device dependency blocks B1.

B1 objective director/string table/single-step cues and assignments; B2 pressure/status; B3 storm clock; B4 onboarding and instruction-driven bot; B5 routes/pole; B6 collisions/walls/labels/panels. Commit and push each completed step. Rerun baseline at each part boundary. Then branch `s4/visuals` from master for C1–C6, separate draft PR. Finish D evidence/report.

## Reproduction commands (local Mac)

The `before` directories contain completed evidence. **Before rerunning, copy the wrappers and change output roots to a fresh `after-PART` directory**; do not reuse old `result.json` files as new passes. `launch_coop.py` waits for the full-suite summary and runs the two-Mac smoke only after the solo Day 3 result passes. Original harnesses are intentionally local ignored artifacts. Capture/commit new evidence separately.

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
python3 artifacts/s4-master/sync_tests.py
python3 artifacts/s4-master/full_tests.py
python3 artifacts/s4-master/solo_before.py
python3 artifacts/s4-master/launch_coop.py
# Build exact tagged milestone (do not run on changed master and mislabel it):
# git checkout --detach sprint4-m1; python3 artifacts/s4-master/release/build.py
```

Unity is `/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity`. Build entry: `LighthouseKeepers.Net.Editor.MultiplayerBuild.BuildQuest`, output set with `LK_APK_OUTPUT`. Never run batch Unity against a project already open in an Editor. The second checkout is `/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Build`; its overlaid source/patch is saved by the sync harness, not a clean branch checkout.

## DEVICE CHECKS PENDING — NOT VERIFIED

Quest unavailable by user instruction; do not block Editor work or APK builds.

- Install latest completed APK when connected (currently milestone release `Builds/Sprint4-M1/LighthouseKeepers-Sprint4-M1.apk`).
- Per-floor/outside/peak-storm GPU ≤11 ms, CPU main ≤9 ms, 72 fps; draw calls/triangles/audio voices; water GPU ≤2 ms.
- Bolt turning/clockwise sinking, trigger clutch, both hands, held-underwater recovery and readable fixed labels on device.
- Mac+Quest join/avatars, audible voice with echo processing, shared task, split objective/partner line, pole visibility, shared sleep and rejoin (new systems only after implementation).
- Wall visibility sweep and doorway/ledge/collider sweep on device.
- Comfort, text/station readability and first-timer Nights 1–3 timing; no Editor result substitutes for these.

ADB: `/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb devices`. Install with `adb -s SERIAL install -r APK`; launch `adb -s SERIAL shell am start -S -n com.lighthousekeepers.game/com.unity3d.player.UnityPlayerGameActivity`. Check serial/authorization first. Both co-op clients must match **LK-S4-M3-FLOOD1** until a new protocol is implemented.
