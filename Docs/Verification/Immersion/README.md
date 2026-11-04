# S4-M3 verification — immersion and clarity

[November 4 follow-up: short-stroke wrench ratcheting](ratchet/README.md). The report below records the original immersion build; the follow-up has its own source, test and APK evidence.

Runtime source: `3ba74c1`, branch `s4/immersion`, based on `team/main` `0ed2a95` (same game as `3f882fe`). Unity **6000.3.23f1**. Main is untouched. This page distinguishes automated/Editor evidence from headset acceptance.

## Before

- Full EditMode: **233/233 passed**. Full PlayMode: **38/56 passed**, 18 failures already present before this work. Compatibility suites use the same `LK_S4M2_DISABLED=1`, `LK_IMMERSION_DISABLED=1` environment before/after; actual ship-game graphics runs have both systems enabled.
- Graphics solo: PLAY SOLO → Day 1 pipe/drain/bed → Day 2 generator/pipe/bed → Day 3 fuel/grid/beacon/radio/pipe/drain/bed. Passed, zero logged errors. A real beacon-dark failure and retry were included.
- Live Mac host + Quest guest, room `3DAALN`: user confirmed the avatar, transmitted chime, and shared Day 2 arrival. Guest repaired all three bolts; host state/board updated. Both voted for bed. Incoming Quest voice was measured at the Mac speaker (`512` received frames, nonzero output). [Accepted baseline](before/mac-quest/accepted-result.json). An earlier attempt flooded while waiting; it is retained as a failed attempt rather than counted as acceptance.

## After — recorded evidence

- Graphics solo on `3ba74c1`: **PASS, zero logged errors**. Same Days 1–3 route, actual injected XRI grip/twist/primer/choke/starter/knob/button/mic/pour interactions, actual water drainage, beacon loss/retry, then Day 4 reset. [Result](after/solo/result.json), [step log](after/solo/loop-evidence.txt).
- Both-hand fuel and wrench interaction tests passed in the focused run. They cover a real carry-handle collider, free carrying, valid tilted pour changing host fuel, mirrored grabbing, wrench snap/twist and retained progress after pulling away. A high-frame-rate precision/rate-limit bug found by the wrench test was fixed in the new snap path; the original feature-off path remains intact.
- Final full suites on `3ba74c1`: EditMode **236/236 passed**; PlayMode **40/58 passed**, with **the same 18 baseline failures and zero regressions among baseline passes**. Both new interaction tests and both Fusion cloud tests passed. [After summary and failure names](after/tests/summary.json), [before summary](before/tests/summary.json). An earlier `841b790` cloud timeout passed on a targeted retry and did not recur in this final full run.
- Two independent Mac clients on final `3ba74c1`: **PASS**. Both saw the other avatar; host-selected radio cue/age matched; guest repaired pipe bolts and poured fuel through the host; host started the generator and guest received power; bed votes and shared sleep advanced both through Days 2 and 3. [Result](after/mac-pair/result.json), [host view](after/mac-pair/02-avatar-host.png), [guest view](after/mac-pair/02-avatar-guest.png), [shared night](after/mac-pair/05-shared-night-guest.png). This is transport/Editor evidence, not replacement headset acceptance.
- Quest development APK built successfully through `MultiplayerBuild.BuildQuest`, including the pipeline's packaged-APK validation. Runtime source `3ba74c1`, network version **LK-S4-M3**, snapshot **LK-Lab-Coop-v3**. Both players need this version; old builds deliberately cannot join this crew version.

## Inspected screenshots

These are Unity camera renders, not headset acceptance. The full capture set remains in `artifacts/immersion/after/solo/`.

| View | Evidence |
| --- | --- |
| House arrival: chalkboard goal, jobs and wall radio | [House](after/solo/02-day-one-house.png) |
| Generator equipment plate, mounted primer, tally windows and filler during pouring | [Close-up](after/solo/fuel-pour-day-3.png) |
| Can on its own service shelf, generator and breaker | [F2](after/solo/06f-warm-f2.png) |
| Left-hand seated/turned bolt | [First person](after/solo/day-one-wrench-LeftHand-first-person.png), [side](after/solo/day-one-wrench-LeftHand-side.png), [turned](after/solo/day-one-wrench-LeftHand-turned.png) |
| Right-hand seated/turned bolt | [First person](after/solo/day-one-wrench-RightHand-first-person.png), [side](after/solo/day-one-wrench-RightHand-side.png), [turned](after/solo/day-one-wrench-RightHand-turned.png) |
| Bunk ready lamp | [Bunk](after/solo/bunk-lamp-ready.png) |
| Brief dawn recovery light (state asserted; image is a dark bedroom view) | [Wake](after/solo/dawn-after-night-one.png) |
| Survived logbook | [Night](after/solo/night-1.png) |
| Beacon-dark loss logbook | [Loss](after/solo/06d-beacon-dark-failure.png) |
| Day 3 complete, power allocation and all four tasks | [Board](after/solo/12-day-three-board-4-of-4.png) |
| Blackout stairs, viewed from the actual F2 landing | [Stairs](after/solo/stairwell-blackout.png) |
| Generator instructions from standing reach | [Plate](after/details/generator.png) |
| Lobby network version, fully inside the board frame | [Lobby](after/solo/01-lobby-play-solo.png) |

The initial stairwell camera was blocked by the inner liner. The recapture above uses the actual F2 landing: the stair edges and practical guide lamp are visible, but dark-route comfort/readability still needs the headset check. Existing minor wrench/hand intersections and translucent shell walls remain deferred. No avatar/body, base-hand rotation, water mesh, water shader or water material was edited.

## What changed and how to try it

Start PLAY SOLO or use a same-version host/guest crew. The House chalkboard and radio explain the goal. Take the wrench to the spraying, chalk-marked pipe coupling; bring the jaw within roughly 8 cm, then roll the wrist. Short repeated cranks accumulate tightening; bringing the wrist back freewheels without loosening the bolt. Pulling away preserves partial progress too. Couplings take 2/3/4 quarter-turns, visibly turn/sink, and reduce spray/pressure with each click.

On the generator, follow its brass plate: fuel → prime three times → choke up → pull three times → choke down. The fuel can rests separately from the engine; grip its carry bar and tip its spout down over the visible filler opening. A valid pour has a stream/sound and raises the gauge. The primer has a mounting plate, retaining collar and hose. Extra starter pulls no longer create the hidden “engine flooded” penalty. Existing choke deadline and all day/power/beacon tuning remain in place.

Complete the jobs, then grip near the F3 bunk. In a crew, each connected keeper must rest. The book appears during the existing comfort fade; no camera movement is imposed. A six-second warm dawn wash follows day advancement, then fades out completely. Its light-state assertion passed; how clearly it reads through the shell still needs headset review. NIGHT lettering writes onto the board over about a second using the same host day/time; jobs stay visible. A terminal failure presents its cause and retry/lobby choices on the same logbook styling. Watch Menu remains available deliberately, including microphone mute and return/rejoin.

## UI and audio

[Full removed/replaced UI inventory](../../Design/ImmersionUiInventory.md). Removed Lab guidance: camera day/task HUD, local crew debug text, repair billboard, sleep billboard, old camera night text, starter instruction panel and floating lens caption. Retained: mounted chalkboard/equipment instructions, deliberate logbook, lobby. William's avatar name tags are an explicit ownership exception.

[All 16 radio lines, filenames and triggers](../../Design/RadioLines.md); [audio/font sources](../../Design/ImmersionSources.md). Plain offline TTS recordings receive 300–3400 Hz filtering, mild distortion, static/crackle and squelch in-engine. The host chooses cues and replicates serial/age through the existing authoritative watch snapshot. Wall and quieter belt receivers use that cue. Photon Voice remains separate. A lifecycle race exposed during regression testing was fixed: Voice now waits for a valid Fusion room before following it, instead of forcing a join while Fusion is shutting down. Audio encoding, spatial settings and microphone behavior are unchanged; the post-change live voice check remains required.

## Switches and reproducibility

`Assets/WatchDemo/Resources/ImmersionTuning.asset` switches the overall feature plus physical guidance, narration, forgiving pipe/generator, atmosphere and logbook separately. The snapshot/matchmaking version remains the branch's protocol identity even when presentation switches are off.

Baseline and final XML/logs: `artifacts/immersion/{before,after}/tests/`. Graphics driver: `artifacts/immersion/AfterReplay.cs`; pair driver: `artifacts/immersion/mac_pair_after.py`. The tests use a second clean worktree; the pair uses two independent graphical Unity processes and the real Fusion service. Test-only input injection/pose moves are evidence harness operations, not shipping shortcuts or proof of physical reach/comfort.

APK: `Builds/Immersion-3ba74c1/LighthouseKeepers-Immersion.apk` (91,881,340 bytes). SHA-256: `be5395192589b34762b832218b07554debe4f9d13dbfad56031963ee302ed4fa`. No release/submission assets were replaced.

Installed via `adb install -r` on connected Quest `2G0YC1ZG32048T`, preserving app data, then launched. Android logs show an OpenXR session reaching FOCUSED. The unattended screenshot was black and is not presented as visual acceptance.

Not yet claimed: headset audibility/readability/comfort, measured Quest CPU/GPU cost, two physical Quests, or post-change live co-op acceptance. The draft must remain unmerged pending those checks.
