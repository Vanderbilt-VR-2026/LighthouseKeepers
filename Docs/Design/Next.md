# M3 checkpoint — five nights and the ship

## Current override — five-night preview requested by lead

Campaign is now **ON** at the lead’s explicit request. The five-night task schedules are selected; ships, rescue ending, saved progression, doors/mirror and final ship-inclusive timing are unfinished. Earlier OFF entries below describe historical checkpoints. Current network **LK-M3-CAMPAIGN1 / v20** (compatibility label only; no new snapshot fields). Wiring is darker, textured and 15% slimmer, with reduced glow. See `Docs/Verification/M3/campaign-on/`. Next: ship implementation.

42 focused tests and all 12 task-model simulations pass. Actual solo Night 1 repair/sleep and live two-Mac shared repair/sleep into Night 2 pass. Editors muted. Quest is connected; prior b6ca4b3 was installed without launch. Activation/finish APK `Builds/LighthouseKeepers-M3-f5d2746.apk` built successfully and installed on Quest 3 2G0YC1ZG32048T, without launch. SHA-256 `527dae0eeea86f3ac2247fb4bff28e907c81578b08e292939389fe94b36f29aa`. See `Docs/Verification/M3/campaign-on/build.json` and `install.json`. Never launch without a new request.


Branch `m3/five-nights`, based on `s4/integrated` at `ddc25f0` because PR #85 was still unmerged when work began. Separate worktree: `/Users/aneeshvasamreddy/LighthouseKeepers-M3`. Do not merge main. PR #85 remains the Milestone 2 merge candidate; no M3 gameplay goes into it.

## Done / current

- Job 1 class-playtest fix from PR #85 is merged through d432c20. New recall build 573cf72 passed focused checks, full suites with documented reruns, Night 1 and two-Mac smoke. M3 now adds switched host scheduling and a task-timing bot; the normal three-night catalog remains selected. Ships, doors and mirror are not implemented yet.
- Editor audio stays muted. Do not launch the Quest app unless the lead asks. USB is currently absent; install the built recall APK when it returns, without launching.

- North star, per-night tension graphs, reuse boundaries and verification gates: [M3-FiveNights.md](M3-FiveNights.md).
- Will’s PR #86 body/flood work is already integrated in the base. Do not remodel his body or sky.
- Base follow-ups implement intermediate pole exits, remove the bedroom cage, retain a flush safety hatch, and remove floating dotted rain panes.
- Base regression is complete and copied to `artifacts/m3/before/`: 351 EditMode passes; full PlayMode plus the corrected hatch rerun preserves 48 passing cases, the same 24 inherited failures and 3 skips; graphics Nights 1–3 and live two-Mac valve/radio/sleep/rejoin pass. Source a9d5f2d. PR #85 is Ready for review, not merged.
- **A1 complete:** optional night schema, five assets plus demo, disabled catalog, editor authoring and three focused passing tests. See [M3-Data.md](M3-Data.md). No five-night gameplay is active yet; timings remain unapproved.
- **A2/A3 runtime scheduling implemented:** host-owned one-shot fault gates, opening/recovery breaks, low-fuel event, authored durations/flood/grace/storm/opening cues and retry/replica validation. Campaign catalog remains OFF. Focused checks: M3 16/16, legacy Day3 6/6, voice 23/23. Initial A4 instruction-only task timing now passes all 12 solo/duo profiles; see telemetry-m3.md. Campaign remains OFF until ship-inclusive pacing and regression gates pass. Graphics/crew baseline passed. Initial task timing is complete; ship-inclusive approval and physical doors remain pending.

## Latest verified checkpoint (2026-11-10)

Runtime commits: `31cff1a` schedules, `b3bb144` task-model timing/guidance. Full EditMode 371/371; PlayMode 49 pass / 24 inherited fail / 3 skip, no lost passing cases. Graphics legacy Nights 1–3 and live two-Mac join/avatars/mono-AEC setup/recall/shared valve-pipe/shared sleep pass. Evidence: `Docs/Verification/M3/scheduling/`. Editors muted throughout. First Night 3 capture timed out loading; only its harness allowance changed before a passing rerun.

APK: `/Users/aneeshvasamreddy/LighthouseKeepers-M3/Builds/LighthouseKeepers-M3-b3bb144.apk` (133795458 bytes), SHA-256 `d6c4cafb1f974afad160c30b35ec32c3a26e6a5581769810498bce17026cdb9f`. Campaign OFF, network `LK-M3-NIGHTS1` / v20. USB absent, not installed or launched. A4 final approval still waits for ship-inclusive timings.

The added diegetic wiring pass is implemented; see `M3-PowerWiring.md` and `Docs/Verification/M3/wiring/`. Next: B1 ship implementation/ship-inclusive A4 timing, doors + mirror, remaining M3 parts. Campaign activation has now been requested by the lead; unfinished features remain pending. The original Editor source is restored after the smoke; guest Editor retains the tested M3 overlay and is stopped. Preserve the original worktree's unrelated prefab ray-width edit.

Raw runners: `artifacts/m3/a2/run.py` (focused tests + guest overlay), `bot.py`, `graphics.py`, `graphics-retry.py`, `full.py`, `overlay-root.py`, `restore-root.py`, `crew.py`, `build.py`. Use fresh evidence destinations when rerunning. `overlay-root.py` backs up only candidate paths before temporarily testing them in the original Editor. Do not run `a1-check.py`: it copies guest-authored assets back into this worktree.

## Wiring checkpoint / next commands

Wiring runtime commit `b6ca4b3` is pushed. Android build succeeded: `/Users/aneeshvasamreddy/LighthouseKeepers-M3/Builds/LighthouseKeepers-M3-b6ca4b3.apk`, SHA-256 `d6a47ad0a726f0ed2839127723c23d84fa7d426a3ad84d738b2efab4533dc501`. Receipt: `Docs/Verification/M3/wiring/build.json`. No install/launch; ADB absent. Clean stop after scheduling + wiring; **next is B1–B4 ships**, followed by the requested doors/mirror before Part C. PR #87 remains draft and updated.

The scheduling part is followed by the requested wiring addition before Part C. The old cylinder/sleeves are replaced by saved Editor-generated routes: 57 terminals, 14,634 triangles, five combined meshes, one atlas/material; no gameplay fields or new network version. Circuit/guest-readonly tests, clearance audit, all-scene coplanar scan, graphics replay are recorded in `Docs/Verification/M3/wiring/`. Keep campaign OFF until ships and ship-inclusive timing pass. PA guidance intentionally retains its existing backup behaviour.

Local runners (fresh evidence folders on repeats):
```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-M3
python3 artifacts/m3/wiring/tests.py TowerWiringTests ConnectedTasksTests PowerGridTests
python3 artifacts/m3/wiring/graphics.py
python3 artifacts/m3/wiring/build.py
```
Editor authoring menu: `Lighthouse Keepers/M3/Wiring/Capture Lab Routes (Play Mode)`, `Rebuild Saved Waypoints`, then `Audit Preview Clearances`. Source is in `Assets/WatchDemo/Editor/TowerWiring*.cs`. Do not copy an old guest mesh back after a new bake; preserve asset GUIDs and save the generated meshes explicitly. Both Editors remain muted. Never launch the Quest without a new request.

DEVICE CHECKS PENDING (wiring addition): install matching APK when authorized/available; sustained 72 Hz wiring-off/on GPU comparison (target ≤0.3 ms), both-eye collars/glass/headroom sweep, gallery/exterior/House feeds, nearby hum and trip bang, 1.5 s pulse/readability and restored lamps, Mac+Quest circuit changes. USB was absent at the last check. No device performance or human audio claim.

## Ten-step headset checklist for this checkpoint / later M3 acceptance

Use matching builds on both clients. This checkpoint retains the three-night game; campaign is OFF. Do not launch without the lead asking. Steps 8–10 require future implementation and are explicitly blocked today.

1. In lobby confirm `LK-M3-NIGHTS1`; PLAY SOLO must still work offline.
2. Tap/hold B near the House board and pipe: immediate objective and last line below gaze; double-tap replays once. X/Y and wrist glance still recall it.
3. Set Watch Menu → Objective height, restart when authorized, and verify the saved height and left wrist objective.
4. Complete Night 1: clamp, grip-only wrench, short clockwise ratchet pulls, visible bolt movement; drop/recovery, flood drain, bed.
5. In Night 2 trace generator → breaker → riser → F1 pump/upper floors. Inspect floor collars, labels, lamp drops, outside House feed, clear stairs/windows/pole reach.
6. Restore power and switch circuits: travelling warm pulse, floor-by-floor lamps, only fed branches glowing, quiet nearby hum; trip/reset the breaker. Capture matching-view GPU with wiring enabled/disabled (≤0.3 ms target, not yet measured).
7. Matching Mac+Quest: join/avatars, clear two-way voice with headphones, both valve roles, guest repair, shared bed and rejoin; confirm circuit switches match both views. Editors stay muted unless the lead explicitly permits a listening check.
8. **Pending ship implementation:** gallery horizon → rocks passage, beacon beam, horn of thanks; separately verify named wreck/retry.
9. **Pending doors/mirror implementation:** grab/swing/latch doors from both peers; inspect Will’s body in House mirror from three angles.
10. **Pending final campaign:** Nights 4–5 and Demo, finale/rescue, logbook/progression; profile ship-at-peak-storm at 72 Hz (GPU≤11 ms).

## Ordered implementation queue

A1. DONE (data only). Extend day data for five nights: duration, storm stage, faults/conditions, active stations, ships, crew roles, solo modifiers, grace and radio/opening cues. Preserve accepted legacy behavior behind the feature switch.
A2. Arc: Leak (safe preview), Blackout (one consequential ship), Warning (two ships/broadcast extension), Everything Breaks (fuel/power/soot), Long Night (late repeated failures and large ship).
A3. Data schedules: 30–60 s opening, 10–20 s relief, no ten-second fault pileup except finale.
A4. Initial task-only model DONE for 12 profiles; Nights 3–5 reach requested spare-time ranges after modeled bed travel. Ship-inclusive pacing and final approval PENDING. Nights 1–2 retain inherited budgets until ships are included. See `Docs/Playtests/telemetry-m3.md`; these are model execution times, not human measurements.
Wiring addition DONE after Part A verification: see `M3-PowerWiring.md`. Existing circuits only, no new network fields; device cost pending.
B1. Quest steamer art (8–12k triangles, 1–2 materials), emissive lights, mesh searchlight, ≤150 smoke particles, storm bow spray, distant light LOD.
   Reuse audit: `NightScenario.NightShipEvent` already has approach/window/grace/broadcast-extension/preview/finale data; `M3NightAuthoring` authors the names and cue mappings 96–99 (P21–P24). Recorded `keeper_ship_sighted/close/passed/lost.wav` and `Resources/ImmersionAudio/foghorn.wav` already exist. No steamer/boat mesh was found. Files called ShipScene/ShipWorld refer to shipping/build routing, not a vessel. Add ship simulation as host state, then snapshot/Fusion/version tests; do not create a second clock. Existing ship offsets are provisional and must be retimed against the new longer N3–5 budgets.
B2. Host ship approach/window/pass/wreck rules; warning 60–90 s ahead, named ships, beacon grace/loss only near passage. Test pass, recovery and wreck.
B3. Shared passage payoff: horn/voice/music/storm relief for both keepers, visible from gallery/windows.
B4. One urgent ship objective/progress and chalkboard ship ticks.
After B, before C1 (new Job 2-C): physical hand/handle doors with bounded hinges, authoritative angle, latch and route/rain compatibility. Add the requested small House mirror, culled when distant/not viewed; inspect Will’s body from three angles and measure on Quest.
C1. Night 5 climax: repeated generator/pipe failure and final large ship, tuned from duo bot.
C2. Dawn/rescue boat, warm window/fog light (not sky edits), calmer sea/gulls, P25, skippable team credits.
D1. Shared morning logbook: night/name, named ships, peak water, close calls, job times, descriptive grade.
D2. Local saved unlocks/continue/night select shared across solo/crew per device; retry at night start.
D3. Data-only Demo (6 min), one ship payoff around five minutes.
E1. Split-information generator N2, House radio code N3, radio bearing/beacon N4, all N5; local equivalents in solo and role-specific instructions.
E2. Existing crew valve puzzle on N3–5, solo clamp otherwise.
E3. P26 only with an actual partner-danger state; otherwise explicitly leave unwired.
F1. P19–P25 triggers and existing word timing/priority/spacing; P18 close-call morning. Never overlap voice lines.
F2. Original/CC0 ship and dawn sounds with sources; ship danger music, pass resolve, hopeful dawn.
G. Full regression, solo bot 1–5/demo, duo 3/5, ship outcomes/save/continue/version tests; inspect screenshots; Quest profile/install/Night1/demo; final timings and ten-step headset checklist.

Commit and push each numbered step. Recheck solo/crew at each part boundary. Bump the network version for new replicated state; current branch is **LK-M3-NIGHTS1 / v20** (new schedule snapshot fields; both crew clients must update). Keep raw evidence out of claims until checked.

## DEVICE CHECKS PENDING

- Class-recall APK 573cf72 is built; install is pending because adb sees no Quest. Do not launch. Human pole/fixture/avatar and B-recall acceptance is pending; five-night gameplay is still disabled.
- See Will’s Captain/fisherman across peers; inspect head, hands, arms, crouch and own-body hiding.
- Two-way mono/AEC speech, near/far/handheld routing, valve roles, shared task/bed and rejoin.
- 72 Hz: GPU ≤11 ms, CPU work ≤9 ms, per-floor/sea/peak-storm measurements; no locked-frame-rate claim yet.
- For M3: physical doors without body/head pops; ship/peak-storm GPU; Night 1 and Demo payoff, subtitles, saved continue and final dawn.

## Exact continuation commands

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-M3
git status --short
git fetch team
# Review the checkpoint and preserve the separate integration workspace’s local prefab edit.
cat Docs/Design/M3-Data.md
cat Docs/Verification/M3/baseline.json
```

The second Editor at `/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Build` currently has the PR85 recall source overlay (not M3) and is stopped. Its bridge helper is `artifacts/coop/mac_pair.py` in `/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics`; do not assume that guest worktree is a clean Git checkout. A1 runner: `python3 artifacts/m3/a1-check.py` from this M3 worktree (copies authored data back and runs focused tests). Integration baseline scripts/results are in the integration workspace’s `artifacts/pole-avatar/final/`; use fresh output folders for reruns.

For a clean M3 Unity run (6000.3.23f1):
```sh
mkdir -p artifacts/m3/after
"/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity" -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode -testResults "$PWD/artifacts/m3/after/editmode.xml" -logFile "$PWD/artifacts/m3/after/editor.log"
```

Build only after the next runtime step passes, with a fresh output filename:
```sh
mkdir -p Builds
LK_APK_OUTPUT="$PWD/Builds/LighthouseKeepers-M3-$(git rev-parse --short HEAD).apk" "/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity" -batchmode -quit -projectPath "$PWD" -buildTarget Android -executeMethod LighthouseKeepers.Net.Editor.MultiplayerBuild.BuildQuest -logFile "$PWD/artifacts/m3/after/build.log"
```

## Release action held for user confirmation

After the lead explicitly says PR #85 is merged: fetch main, verify its merge, tag `sprint4-m2` without moving an existing tag, build that exact commit, publish “Sprint 4 — Milestone 2” with APK/SHA-256/five plain-English lines, update README downloads. Close #81–#83 with a comment linking #85; **ask before closing Eli’s #84**. Do not publish or close ahead of that confirmation.

---

# Class playtest recall checkpoint — 2026-11-10

Job 1 on `s4/integrated` / PR #85 is implemented, committed and verified; APK built from **573cf72**. The Quest disappeared from `adb devices` before installation. **Do not launch the Quest app unless the lead asks.** All Editor runs must remain muted with `EditorUtility.audioMasterMute = true`; `EditorPlaytestAudio` handles Editor initialization/Play Mode transitions, and `artifacts/coop/mac_pair.py` also mutes before runs. No player audio settings changed.

Done: below-eye 1.5 m lazy-follow objective, five-second presentation, saved -20 to -5 degree height, instant B tap/hold, double-tap local replay, three-line history, wrist objective, existing X/Y/wrist recall, one-time Night 1 B reminder. Network remains **LK-S5-VOICE1 / snapshot v19**; no synchronized fields changed.

Verification: EditMode 355 passing cases after the single FMOD scene-audit rerun; PlayMode 49 passing cases with the same 24 inherited failures and 3 skips after replacing the obsolete world-fixed assertion with the requested lazy-follow test. Original full results and focused reruns stay separate. Physical Night 1 and live two-Mac join/avatar/local replay/shared pipe/shared sleep pass. Mono/AEC setup passes, but no human listening claim while Editors are muted. See `Docs/Verification/ObjectiveRecall/README.md`.

## DEVICE CHECKS PENDING

- Connect the Quest over USB, install the exact APK below; **do not launch without a new request**.
- When the lead is ready: press B near the board, pipe and generator; verify same-frame full objective and last line below gaze, hold, tap expiry, double-tap audio replay.
- Turn the head quickly and slowly: lazy follow must be comfortable and leave the centre clear. Adjust Objective height in Watch Menu settings and restart to check persistence.
- Read the current job on the left watch, check X/Y and wrist glance, and the one-time Night 1 B reminder.
- Human two-way voice and Mac+Quest task/sleep smoke; on-device worn-headset CPU/GPU/readability/comfort remain unverified.

## Exact continuation

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
"/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb" devices -l
"/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb" install -r Builds/LighthouseKeepers-Recall-573cf72.apk
# STOP here: do not run am start or monkey without the lead asking.
```

This fix is now merged into `/Users/aneeshvasamreddy/LighthouseKeepers-M3` / `m3/five-nights`, preserving all night/string assets. Continue A2/A3 runtime schedules, then A4 measurement, ships, doors/mirror, and remaining parts in `M3-FiveNights.md`. The A1 campaign is still disabled. No five-night timing, ship payoff, physical-door or mirror acceptance is claimed. Keep PR #87 draft and never merge main.

Raw runners/results: `artifacts/class-recall/`; `tests.py` (EditMode), `play-focused.py`, `play.py` (full PlayMode), `solo.py` (Night 1), `crew.py` (two-Mac smoke), `build.py` (exact-source Android build). Before repeating a run choose a fresh output directory; inspect the script's existing destination. Both Editors were stopped after the smoke test. Preserve the unrelated Quest prefab ray-width edit and `Docs/Verification/Immersion/pipe-lever/`.

---

# Integrated checkpoint — s4/integrated

This branch reconciles #81–#84. Do not merge or close those PRs. Network **LK-S5-INTEGRATED1 / snapshot 18**; both clients need the same APK. Selected exterior: Eli’s ocean, mountain coast and opt-in sky, per the lead. Our newer interior, narration/PA, pole, connected power and valve puzzle remain. See [Reconcile-PR84.md](Reconcile-PR84.md).

Post-cleanup focused7/7, graphics solo Nights1–3 and live two-Mac valve/radio/sleep/rejoin PASS (artifacts/reconcile/cleanup-graphics and cleanup-crew). Earlier full-suite evidence: 305/305 EditMode; full PlayMode plus focused reruns preserve every pass from both baselines (47 pass, 24 inherited common failures in the aggregate). Graphics Nights1–3 and live two-Mac valve/radios/sleep/rejoin pass. Cleanup APK built successfully fromd3d6f6f (white board/black text, House clutter removed, fixtures clear of rails,2×MSAA). Latest hash/path: Docs/Verification/Integrated/build-result.json. The lead requested installation; adb installedd3d6f6f successfully on Quest2G0YC1ZG32048T and relaunched it. Receipt: Docs/Verification/Integrated/install-d3d6f6f.txt. Human visual checks and2×MSAA profiling remain pending. Draft PR85 supersedes81–84 without closing them.

## DEVICE CHECKS PENDING — integrated

- Installed `Builds/LighthouseKeepers-Integrated.apk` fromd3d6f6f. Confirm `LK-S5-INTEGRATED1` in lobby during the human check.
- Profile House, F1, F2, F4, gallery sea and strongest storm at 72 Hz: GPU ≤11 ms, CPU work ≤9 ms, draws/triangles/voices. Installed cleanup build: scale1.0, MSAA2, fixed foveation0.5. Device samples exceeded11ms, so the authorized2× fallback needs a matching-view check. Keep pacing wait time separate from CPU work.
- Continuous wall/doorway sweep; no head pop at bench, centre aisle, F1, dropped wrench or disabled Support foot. F3/F4 edges blocked.
- Solo valve clamp → grip-only wrench snap → visible clockwise turn; drop/recovery; tracker fixed until recall; grace understandable; glass stereo sorting/text clarity.
- F4 enclosed rooftop rain → exposed gallery louder exterior rain; F1 outside unchanged. Recorded/new placeholder announcements audible.
- Mac+Quest: join, avatars, both voice directions with headphones, cross-floor falloff, both handheld radio modes, both valve roles, shared task/sleep, disconnect/rejoin. Follow [TwoHeadsetSession.md](TwoHeadsetSession.md).

## Reproduce current checks

Run from `/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics`. Existing raw scripts/results are in `artifacts/reconcile/`: `integrated_graphics.py`, `integrated_crew.py`, `build_integrated.py`. Use new output directories for new runs. Finish Play Mode before rebuilding. Preserve the unrelated user ray-width change saved in `preserved-user-rig.patch` and restore it after exact-source builds.

---

# Part E checkpoint — s4/connected

Stacked draft [PR #83](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/83), based on s4/visuals / PR #82. E1 dependencies, E2 pressure valve + solo clamp, E3 proximity/handheld voice are committed. E4 device plan is [TwoHeadsetSession.md](TwoHeadsetSession.md). Part D: graphics solo Nights 1–3 and two-Mac valve/radio/sleep/rejoin checks pass. 280 EditMode passes; full PlayMode plus corrected clamp-prerequisite rerun preserves all 40 prior passes, with the same 25 inherited failures. [Final evidence](../Verification/S4Connected/final/README.md).

APK built successfully from `ce3b8f7`: `Builds/LighthouseKeepers-S4-PartE.apk`. SHA-256 `dca2a05ece210708281331ff327d630f694c0eda2f0dc0b559d32e1bb79cb4d3`. Later delivery commits only add documentation/evidence.

Network **LK-S4-CONNECTED2 / v17**. Both clients must update. New replicated fields are valve ownership/clamp and handheld transmitter IDs. Solo microphone remains off. Will's skybox/avatar body and local hand rotation are untouched.

## DEVICE CHECKS PENDING — Part E

- Install `Builds/LighthouseKeepers-S4-PartE.apk` on both Quests; exact source/hash in `Docs/Verification/S4Connected/final/build-result.json`. No install during this offline turn.
- Execute all ten steps in [TwoHeadsetSession.md](TwoHeadsetSession.md): setup/join, both valve roles, release surge, shared tasks, dependency chain, cross-floor direct/radio speech, mute, shared sleep, disconnect/rejoin.
- Human intelligibility/echo cancellation in both directions, radio grip/head proximity comfort, wall/floor attenuation, actual device audio CPU and voice count.
- All existing visual/comfort/performance checks below remain pending; Mac tests cannot certify them.

## Part E local commands

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
python3 artifacts/s4-connected/after/sync_tests.py
python3 artifacts/s4-connected/after/exact_full_run.py
python3 artifacts/s4-connected/after/solo.py
python3 artifacts/s4-connected/after/crew-fixed.py
python3 artifacts/s4-connected/build.py
```

Use fresh output folders, run the crew script only with both Editors ready, and never refresh scripts during Play Mode. Build wrapper preserves the unrelated Quest player prefab ray-width edit. Device session remains the next action; stop after this chunk.

---

# Previous Part C checkpoint

Worktree: `/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics`. Branch **s4/visuals**, draft [PR #82](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/82), stacked on `s4/master` / draft [PR #81](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/81). Never merge main. Will's skybox and avatar body are unchanged.

Network version **LK-S4-VISUALS-C0**, snapshot v15. Both crew clients must update. The bump covers the composite spoken-status cue vocabulary (IDs 44–75); C0–C5 rendering/audio additions consume existing authoritative state and add no replicated fields.

## Completed this chunk

- Part B follow-up (`37a0447`): one-sentence spoken time/water/beacon status, central legacy menu/readout strings, sampled walkable-boundary audit and five F3/F4 guard repairs. Actual House→gallery route and two pole descents pass. Evidence in `Docs/Verification/S4Master/part-c/b-leftovers.md`.
- C0 (`132bfef`): Android's active QuestPipeline uses MSAA4x, render scale1.25 and fixed foveation0.5. TMP SDF HUD/subtitles; fixed mipmapped world writing. All 36 authored scenes scanned; detected coplanar pairs 1509→0 without moving colliders. Realtime light inventory retained.
- C1 (`62a6d85`): opaque double-sided shell walls, .04 m near clip, wall LOD fade/culling fixes, occlusion off. 72 rendered views across House/floors/stairs/gallery reviewed; not a headset continuous-motion certification.
- C2 (`77909e5`): CC0 limewashed stone, weathered wood, rusty metal/brass, wet stone; metre UVs, packed masks, ASTC and mipmaps. Sources/derivations in `AssetSources.md`.
- C3 (`fef69ae`): actual CPU light bake, 2 lightmaps, 99 probes and 5 room reflections; one enabled realtime beacon point light, no realtime shadows. Practical housings, window haze, interior/exterior fog and dawn tint. Existing blackout/restore still drives presentation.
- C4 (`3366396`): dual-normal flood, contact ripples/debris, four-wave ocean and rocky inlet, small soft droplets/scrolling active jet, nightly rain/seep/runoff and wave impacts. Host flood rules and damage are untouched.
- C5 (`c0e67dc`): transparent lantern/Fresnel glass, amber core, window streaks, station wear and nonblocking keeper props. Original sound-gap cues. Local hand illumination now uses baked probes plus a small tan visibility floor; pose/rotation/grip unchanged. Clean Night 1 replay passes.
- C6: `M2-Plan.md` records remaining per-floor art, safe exterior-route work, model-source options and budget allocations. `WorldAudioGaps.md` records additions/reuse/licences.

## Evidence and limits

Per-step evidence is in `Docs/Verification/S4Master/part-c/`; raw runs in `artifacts/s4-master/part-c/`. C4/C5 each pass 31 focused checks and physical Night 1 repair/drop/recovery/sleep. Final evidence is saved in `part-c/final/`: 272/272 EditMode pass; full PlayMode 39/65, then an ordered test-cleanup rerun restores the avatar pass (40 passing cases, the same 25 inherited failures). Solo Nights 1–3, two-Mac guest repair/shared sleep/rejoin, and the 72-view wall sweep pass. See the regression notes for the full-run versus rerun distinction.

Known visual limits: first-pass low-poly props; analytic flood boundary foam does not trace every arbitrary prop; conventional bounded Unity droplets rather than compute-particle simulation; no new walkable coastline without support/guards. Render scale1.25 is provisional, not performance-approved. Composition-layer HUD fallback remains conditional on observed headset shimmer. See `MasterPassRemaining.md` for acceptance work.

## Built APK

`Builds/LighthouseKeepers-S4-PartC.apk`, runtime source **a8688d4**, 125,947,726 bytes. SHA-256: `543d5acc763f2c474c88eebf0c837389f92efacd184aafed06da06918bb2c2d2`.
Later commits only change test teardown/evidence; the runtime source is unchanged. Android IL2CPP build and both Vulkan/GLES shader compilation succeeded. Installation was skipped as requested. [Final report](../Verification/S4Master/part-c/final/README.md).

## Exact local commands

Use fresh output directories on reruns; wrappers intentionally preserve old evidence.

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
python3 artifacts/s4-master/part-c/sync_tests.py
python3 artifacts/s4-master/part-c/exact_full_run.py
python3 artifacts/s4-master/part-c/solo_after.py
python3 artifacts/s4-master/part-c/launch_coop.py
python3 artifacts/s4-master/part-c/build.py
```

Unity6000.3.23f1. Run batch graphics with `--burst-disable-compilation` for this Mac's test environment; no `-nographics` for captures. Never edit/refresh scripts while playing. Build method `LighthouseKeepers.Net.Editor.MultiplayerBuild.BuildQuest`, env `LK_APK_OUTPUT`. Close the primary Editor before building. Preserve the unrelated `LK_QuestPlayer.prefab` ray-width delta and `Docs/Verification/Immersion/pipe-lever/`; the build wrapper saves/restores that delta. Secondary worktree has source overlaid for tests, with its previous patch saved; do not reset it.

## DEVICE CHECKS PENDING — NOT VERIFIED

The current instruction says build but skip installs. No device results are inferred from Editor tests.

- Install `Builds/LighthouseKeepers-S4-PartC.apk`; source/hash in `Docs/Verification/S4Master/part-c/final/build-result.json` (built successfully).
- Profile House, F1–F4, outside and peak storm: sustained72fps; GPU≤11ms, CPU main≤9ms; ~150draws/eye, ≤300k visible triangles, audio≤24 voices; water≤2ms. Capture the same workload with visual switches off/on to isolate cost.
- Compare scale1.0/1.25/1.4 at MSAA4x; select within budget. Fall back to2x only if needed. Verify fixed foveation and both-eye SDF/physical text shimmer.
- Check glass lens/rain/foam transparency in both eyes, ocean silhouette against Will's skybox, practical powered/blackout readability, tan-hand brightness, strong/weak pipe spray.
- Grip-only wrench, clockwise turn/sink, both hands, water drop/recovery; no pose regression.
- First-time Nights1–3: current-control guidance, one-sentence status, PA audible on stairs, wake/slate guidance, reasonable Night3 timing, cause-specific fail/retry.
- Pole reach/comfort/release/landing, smooth stairs and bunk clearance; continuous doorway/wall sweep and ledge safety on every walk route.
- Mac+Quest join, avatars, two-way audible voice/echo processing, guest task, split objective, remote pole movement, shared sleep/rejoin. Two-Mac transport tests do not certify human speech or headset tracking.

Then stop and await the user's next chunk. No main merge is authorized.
