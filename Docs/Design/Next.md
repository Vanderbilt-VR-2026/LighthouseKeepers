# Sprint 4 master pass — checkpoint

Branch `s4/master`, based on main / PR #79 merge `5dc5e56`. Do not merge. Preserve unrelated untracked `Docs/Verification/Immersion/pipe-lever/` in the local worktree.

## Done

- 0–1: design pillars/guardrails recorded in `Pillars.md`.
- A1: tagged exact PR #79 merge as `sprint4-m1`, built and published **Sprint 4 — Milestone 1 (Immersion + Audio)**. APK SHA-256 `f2c0e07762ae8db2f19bde140b719b8f3e607f758604c4bba45a9a84811d4872`.
- Release: https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint4-m1
- Main download row and pillars: docs PR #80, awaiting human approval/merge. Sprint 1/2/3 rows retained. Merged `s4/immersion` branch deleted; merge/tag preserve its history.
- A2: master branch created; review remains draft. Team playtest recorded before gameplay changes.

## Next, in order

Finish baseline under `artifacts/s4-master/before/`: full suites, graphics Nights 1–2 into staged Night 3, paced Night 3, then two-Mac join/avatar/voice plumbing/shared task/shared bed/rejoin. Inventory failures without silently fixing unrelated baseline issues.

B1 objective director/string table/single-step cues and assignments; B2 pressure/status; B3 storm clock; B4 onboarding and instruction-driven bot; B5 routes/pole; B6 collisions/walls/labels/panels. Commit and push each completed step. Rerun baseline at each part boundary. Then branch `s4/visuals` from master for C1–C6, separate draft PR. Finish D evidence/report.

## Reproduction commands (local Mac)

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
python3 artifacts/s4-master/sync_tests.py
python3 artifacts/s4-master/full_tests.py
python3 artifacts/s4-master/solo_before.py
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
