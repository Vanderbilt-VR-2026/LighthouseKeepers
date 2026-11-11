# November 11 playtest fixes — ACTIVE CHECKPOINT

Work on `s4/integrated` / PR #85, then merge the verified Job 1 into `m3/five-nights` / PR #87. Do not merge main. Preserve unrelated Wood.mat, TMP fallback atlas, Quest ray-width prefab edits and pipe-lever evidence. Editors must stay muted; do not launch the Quest without a request.

## Active checkpoint — Step 5 complete, Step 6 next

Steps 1, 3 and 4 are implemented and checked; step 2 was audited but the reported separate yellow remnant was not reproduced. Job 1 is NOT finished. Full job-end suites, solo 1–3/two-Mac smoke, APK and M3 merge have NOT run for this checkpoint. Job 2 has not started. The full remaining scope (including the new lobby and horror additions) is preserved in [PlaytestAndHorrorQueue.md](PlaytestAndHorrorQueue.md). Quest absent from ADB on November 11; no app launched.

## Current work

Step 1: solo valve clamp removed from rules, objectives and narration; solo retains the prior post-clamp inflow rate. Two-player hold/release and distinct worker validation remain; disconnect allows immediate solo repair. Valve sits beside the active bolt and moves only when unheld; hidden/non-interactable for one player. Focused pressure 4/4 and director 8/8 pass. Graphics actual-XRI Night 1 → Night 2 passed; live one→two→one valve visibility/placement passed (0.43 m from active bolt). Step 1 complete; evidence in Docs/Verification/PlaytestFixes/step1/. No new replicated fields; network stays LK-S5-VOICE1 / v19 for now. Lobby changes will require a new matchmaking version.

## Next in order

2. Pole audit complete (PlayMode 1/1; inspected F3 screenshot). No obsolete shaft or separate yellow remnant reproduced. Location clarification/device confirmation pending; working pole and safety hatch preserved. Evidence: Docs/Verification/PlaytestFixes/step2/.
3. DONE: grip-only pressure primer at chest height, larger bulb, release/refill, tally and feedback. Focused 3/3 + engine 6/6; left/right actual-XRI graphics pass, screenshots inspected. See InteractionInputs.md and Docs/Verification/PlaytestFixes/step3/.
4. Implemented: short action/object + specific place/floor; nearby controller GRIP/TRIGGER cue. Director tests 9/9 and near/far graphics check pass; final rendered icon verified and screenshot inspected. Director tests now 10/10, including actual rendered mesh. See Docs/Verification/PlaytestFixes/step4/.
5. DONE: separate yellow generator and dark-green wall POWER PANEL, over 4 m apart, one ceiling feed. Focused graphics test 1/1 and inspected screenshot: Docs/Verification/PlaytestFixes/step5/. Previous survey: Current panel is WatchPowerPanel.Create at STN_GENERATOR_PANEL; layout JSON anchor (3.031089,3.3,1.75), yaw 240. Generator mesh is reused by Editor/OneLighthouseM2.cs from the fixture prefab (Diesel crankcase, Alternator cover, Radiator grille). Inspect both world bounds before moving; runtime is generated, so fix the source/anchors and test rebuild. Integrated has ConnectedMachinePresentation legacy riser; M3 replaces it with baked TowerWiring resources, so re-bake that authoring after merge rather than duplicating conduit.
6. Island edge protection and sea-loss/co-op return; test.
Lobby addition: PLAY flow, host-alone/START without READY, offline fallback, dynamic crew count, join/rejoin, README and version bump.
7. Latest lean scope: focused tests only, one graphics solo Night 1–2 and one two-Mac smoke; no full suites. Build/install AND launch if Quest visible (explicitly requested this turn); update PR #85. Merge into M3 only after this job is verified.
Job 2: finish M3 ship first; write TheDrowned.md, then implement the latest horror brief in numbered checkpoints. Do not claim ships/horror/finale/progression done; M3 currently has scheduling and wiring plus B recall.

## DEVICE CHECKS PENDING

- Current job has no APK yet. Verify no-clamp solo repair, two-player valve reach/role split and disconnect.
- Grip squeeze/release feel, primer visibility; readable objects/locations/button cues.
- F3 pole remnant removed without losing floor exits/fall protection.
- Updated lobby with Quest Wi-Fi off and two matching builds online; rejoin/leave.
- Island edge walk, sea recovery; human voice; worn GPU/CPU/readability.

## Exact commands / evidence

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
python3 artifacts/playtest-fixes/tests.py PressureValveTests pressure.json
python3 artifacts/playtest-fixes/tests.py MasterDirectorTests director.json
python3 artifacts/playtest-fixes/solo.py
```
Additional focused commands: `python3 artifacts/playtest-fixes/pole-test.py OnlyCurrentDescentShaftAndThreeLandingsExist pole-test.json`; `python3 artifacts/playtest-fixes/primer-tests.py GripSqueezeTests grip-cycle.json`; `python3 artifacts/playtest-fixes/primer-live.py`; `python3 artifacts/playtest-fixes/objective-tests.py MasterDirectorTests director-locations.json`; `python3 artifacts/playtest-fixes/hud-live.py`. These use saved scene/input fixtures and must not be called human/device evidence. The primer runner injects grip after HandRig updates; old EngineLive/Days12 trigger-based primer harnesses must be updated before step 7 (do not change gameplay back for the harness).

Results/runners: `artifacts/playtest-fixes/`. Do not overwrite completed evidence; choose fresh output paths for reruns. Full suites only at job end. Preserve the earlier baseline below; its solo-clamp requirement is superseded by this playtest.

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

For M3, merge this branch into `/Users/aneeshvasamreddy/LighthouseKeepers-M3` / `m3/five-nights`, preserving all night/string assets. Continue A2/A3 runtime schedules, then A4 measurement, ships, doors/mirror, and remaining parts in `M3-FiveNights.md`. The A1 campaign is still disabled. No five-night timing, ship payoff, physical-door or mirror acceptance is claimed. Keep PR #87 draft and never merge main.

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
