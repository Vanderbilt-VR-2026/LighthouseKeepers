# Sprint 4 master pass — Part B checkpoint

Branch `s4/master`, draft PR #81, based on PR #79 merge `5dc5e56`. Do not merge. Current network version **LK-S4-MASTER-B6**, snapshot v14. Both crew clients must update. Preserve unrelated local `Docs/Verification/Immersion/pipe-lever/`.

## Completed implementation checkpoints

- A: exact merge tagged `sprint4-m1`, APK published: https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint4-m1. SHA-256 `f2c0e07762ae8db2f19bde140b719b8f3e607f758604c4bba45a9a84811d4872`. README docs PR #80; no main merge performed by this agent.
- B1: host-owned Objective Director, short central instruction table, nearest-job crew assignments/partner line, current-control cues, grip-only automatic wrench snap, forgiving guided generator sequence.
- B2: wrist timepiece, waterline markings, host danger history, HUD danger strip, PA/controller status request, danger TTS and near-miss logbook note.
- B3: replicated four-stage storm drives rain, wind/thunder, ocean displacement, lamp motion, small ceiling dust/rattle, music; no skybox/body changes.
- B4: no-fail Night 1, pickup/30-second door gate, three-beat arrival explanation, fixed pipe/bunk slates, next-job/wake guidance; Night 2 outage at 75 s and 120 s learning grace. Instruction-driven rules bot includes travel assumptions, bed approach and sleep. Solo Night 3 450 s, modeled 345.2 s work/return, 104.8 s spare (30.4% headroom). Crew budget unchanged.
- B5: walking 1.6→1.92 m/s, default snap retained, opt-in smooth turn saved; bunk clears F3 route. Two tested pole drops F4→F3→F2 use existing stair openings; F2→House stays stairs because the next stair flight blocks full-body clearance for a third shaft. Release pauses; regrip continues; landing restores locomotion.
- B6: low House rocks nonblocking; throat visual separated 12 mm; exact duplicate shell renderers suppressed; continuous gallery guard with combined visible rails. New gallery capsule-sweep and bunk-clearance regression.

## Evidence / acceptance limits

Before: `Docs/Verification/S4Master/before/` (262 EditMode passes; 34 PlayMode passes and 27 pre-existing failures; graphics Nights 1–3 and two-Mac crew passed). Current results: `Docs/Verification/S4Master/part-b/`; raw runs `artifacts/s4-master/part-b/verified/`.

Final suite/network/build results are being collected. Graphics Nights 1–3 and the updated Night 1 readability replay pass. Four focused PlayMode checks pass (grip-only both-hand/three-position repair, delayed-outage starter, F3 traversal, gallery safety). Controlled Day 2 fixed-text comparison: 244→121 words, 50.4% reduction; not a whole-game localization audit. Rules-bot timings are **simulated, not human measurements**. Mac voice transport/DSP facts are not listening acceptance.

Still needs acceptance or fuller coverage: exhaustive edge walking on every exterior route; all-scene coplanar triangle audit; complete migration of legacy menu/readout strings; composite time/water/beacon status sentence (current request reports the most urgent channel); total first-appearance voice repetition audit. Practical upper-floor pole drops are implemented, not a continuous House-to-lantern shaft.

## Continue after this chunk

Review final results and remaining acceptance gaps before Part C. User will say “continue”; do not start Part C in the Part B turn. Then create `s4/visuals` from `s4/master` for C1–C6 and a separate draft PR. `MasterPassRemaining.md` retains the visual requirements. Will owns skybox and avatar body.

## Exact local commands

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
# Preserve completed outputs: choose a NEW output root in these wrappers before rerunning.
python3 artifacts/s4-master/part-b/sync_tests.py
python3 artifacts/s4-master/part-b/full_tests.py
python3 artifacts/s4-master/part-b/solo_after.py
python3 artifacts/s4-master/part-b/launch_coop.py
# Latest build wrapper, after closing the primary Editor:
python3 artifacts/s4-master/part-b/build.py
```

Unity 6000.3.23f1: `/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity`. Build method `LighthouseKeepers.Net.Editor.MultiplayerBuild.BuildQuest`; set `LK_APK_OUTPUT`. Never batch-open a GUI-open project. Secondary worktree `/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Build` contains overlaid source; sync harness preserves its prior patch. Do not reset it.

## DEVICE CHECKS PENDING — NOT VERIFIED

Quest is unavailable; builds proceed, installs deliberately skipped.

- Install latest **Part B** APK (path/hash/source will be in `Docs/Verification/S4Master/part-b/build-result.json`), not the unchanged milestone release.
- GPU ≤11 ms, CPU main ≤9 ms, sustained 72 fps; per-floor/outside/peak storm draws ≤150/eye, visible triangles ≤300k, audio voices ≤24. No device budget claim from Editor.
- Grip-only wrench snap, clockwise bolt turn/sink, release/regrab, water recovery, both hands; fixed chalk labels readable.
- First-time Night 1→3: one understandable instruction, generator controls, slates at wake, PA audible on stairs, clear warning/lose/retry and shared sleep.
- Pole reach, gates, comfort, safe release/landing; physical stairs and new bunk placement. No forced rotation.
- Mac+Quest join/avatars, audible two-way voice with echo processing, guest shared task, split objective/partner line, remote pole movement, shared sleep/rejoin.
- Doorway/wall visibility sweep, head-height bumps, gallery edge safety on device.

ADB path `/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb`. Check `devices`, then `-s SERIAL install -r APK`; launch `com.lighthousekeepers.game/com.unity3d.player.UnityPlayerGameActivity`.
