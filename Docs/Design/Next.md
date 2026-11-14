# Sprint 5 checkpoint — November 13

Worktree `~/LighthouseKeepers-M3`, branch `sprint-5`, draft PR #89. Sprint 4 PR #88 was merged by a teammate into main at `14634af`; that merge is included and #89 now targets main. The new Sprint5-Master.md, including §14, supersedes the historical M3 ordering below.

## Latest headset steering — November 13

The lead launched/playtested ship APK005e641 and confirmed the ship appears. Distant box/light LOD, active wheel tracking, generator realism and watch face orientation need correction before continuing toward the Drowned. The distant hull correction passes five focused tests and reviewed graphics views (`Docs/Verification/S5/Ship/playtest`). Next: host-owned beam aiming/hold, generator model, watch face180°, then Phase3 sabotage / Phase4 lantern / Phase5 Drowned. Historical “not launched” receipts below predate the explicitly authorized playtest.

## Current work

- **1.0 implementation complete; device acceptance OPEN (#90):** active Android QuestPipeline MSAA4/scale1, Vulkan, fixed FFR0.5, on-tile grade, mipmaps/ASTC; one-click menu includes fixed-scene baked lighting/static/probe setup. Progressive GPU powered/emergency bakes: two maps / 4.93 MiB each, 444 probes. See QuestQuality.md.
- Full EditMode 396/396 pass; expanded quality checks 3/3 pass. Before/after actual-XRI graphics Nights1–3 pass into Night4, zero logged errors; inspected images in Docs/Verification/S5/Quality. Focused PlayMode 19/23; all four failures reproduce identically on pre-quality 785f2d1, so this set has no new failures. Pre-change full PlayMode stalled in retired Bootstrap tests; not a full-suite pass.
- Clean-source `6c13885` APK built and published as an explicitly unfinished Sprint5 preview. Public download hash verified. Ships/horror/lantern/progression are not finished. No gameplay/network changes in quality pass: **LK-M3-CAMPAIGN2 / v22**.
- Grading cleanup complete: exactly main and sprint-1..5 remotely; eight old branch tips archived and containment verified. Sprint4 release `sprint4-final` and Sprint5 prerelease `sprint5-preview-20261112` are public. Sprint5 milestone December3; 40 implementation/ownership issues (GitHub milestone totals also count PRs).

## Next in order

**Quality regression DONE:** fresh two-Mac join/avatars/mono-AEC configuration/proximity/both radios/mute/shared task/shared sleep/rejoin pass. Separate Night3 host-valve + guest-wrench transport passes. See Docs/Verification/S5/Quality/crew/. **1.1 DONE (#91):** Sprint5.md, 28-row FeelChecklist.md, 32-event-family SoundMap.md are committed skeletons; implementation vs planned work is explicit. Next #92 TensionDirector and #93 fear system; re-read the relevant master sections before each phase. Do not jump to historical ship-first instructions.

## Next implementation boundary

**1.2 implemented (#92):** one host TensionDirector owns fault spacing (at least 15 s), 10–20 s relief, storm stage and requested music layer. Scare permission/2.5 s quiet and tutorial/escape guards are integration points; no entity exists yet. Physical fuel exhaustion respects recovery without faking power. Network is now **LK-S5-TENSION1 / LK-Lab-Coop-v23**; both clients must update. 82 focused tests pass; actual-XRI graphics Nights1–3 → Night4 passes with zero errors; inspected images and task-only models in Docs/Verification/S5/Tension/. The previous 12 s fault-gap assertion was updated to the new required 15 s, not a gameplay workaround.

**1.3 implemented and automated regression passed; device acceptance OPEN (#93):** host per-keeper fear, heartbeat/breath/undertone, saved heartbeat-haptics toggle, world filtering excluding voice/radio/critical sounds. Source **859f8b0**, protocol **LK-S5-FEAR1 / LK-Lab-Coop-v24**; both clients must update. Final full EditMode **416/416**. Focused PlayMode19/23: the same four baseline failures. Actual-XRI solo Nights1–3 reaches Night4; appended probe had an overly broad clip selector, then the corrected isolated fear probe passes. The Sound page was reflowed inside its backing; bounds test and reviewed final screenshot confirm the fit. Final menu-probe tail was interrupted by Mac sleep; preserve its raw failure.

**Phase1 automated regression gate PASSED:** fresh live two-Mac host-alone/late join, both avatars, Voice joined + mono/AEC/NS configuration, cross-floor falloff, both handheld radio routes, mute, guest pipe task, shared sleep to Night2, leave/rejoin. Separate Night3 host-held valve + guest three-bolt repair passes; per-keeper lower-floor/upstairs fear matches both replicas and the saved haptics toggle works. Both Editors muted, so human speech/comfort remains unverified.

A test-only movement error was isolated after sea recovery: its direct pose jump while the character capsule was enabled left the camera at the doorway. The corrected test temporarily disables that capsule during reposition and verifies the requested head pose; the unchanged pipe gameplay then passes. No gameplay workaround was made. The earlier sleep-interrupted runs and raw failure remain archived. Reproduction: `caffeinate -i python3 artifacts/s5/fear/crew-driver.py`; always use fresh output folders. The phase-end416 EditMode and19/23 PlayMode results already cover current source; no additional full rerun is needed for evidence-only changes.

Build done from clean source **805e779** (includes the final Sound-page fix): `Builds/LighthouseKeepers-Sprint5-Foundation-805e779.apk`; SHA-256 `c1a987c01e4c47e5522adefab7a76ca95dd084d22f641a50e98a636affc2e9a7`, 123,912,774 bytes, IL2CPP ARM64/Vulkan, zero build errors. Exact-source metadata is in `Docs/Verification/S5/Fear/final-apk.*.json`. Foundation APK installed successfully via non-streamed ADB on November13; no app launched.

**2.1 implemented (#94):** baked original 52 m steamer, named hull, emissive lamps, searchlight, bounded GPU smoke/spray, 204-triangle distant LOD, continuous west-side route clear of the island/coast. Full close model 11,760 triangles, two materials, zero realtime lights. Graphics Play Mode zero errors; inspected horizon/approach/near/peak images in `Docs/Verification/S5/Ship/`. Normal-play `shipVisuals` remains OFF pending outcomes. No protocol change.

**2.2 implemented (#95), feature still OFF pending phase gate:** host ship windows/pass/wreck, safe preview, broadcast approach extension, shared sleep gate, P21–P24, horn/crash/flare, named logbook loss and retry. 72/72 focused cases; muted graphics pass and reviewed images (`Docs/Verification/S5/Ship/outcomes`). Snapshot now **LK-S5-SHIPS1 / LK-Lab-Coop-v25**. Both players must update when the phase build is delivered. Latest built APK805e779 is still foundation v24.

**2.3 implemented (#96):** 83/83 focused checks and muted graphics Play Mode pass (zero errors). Darkness is a warning outside near-rocks/passage windows; the ship-specific grace owns a named loss inside them. Night1 preview remains safe. Evidence: `Docs/Verification/S5/Ship/window-rules/`. Protocol unchanged from2.2.

**2.4 implemented (#97); phase2 automated regression PASSED:** gallery searchlight sweep, repeated spatial horn, original engine/hull loop, existing comfort-limited lightning and pass relief/sting. Normal campaign ship visuals/rules are ON. Full EditMode438/438; focused PlayMode19/23, same four inherited failing names (the obsolete beacon test stops earlier at ship-gated sleep). Nights1–2 real-XRI pass; Night3 caught a fuel exhaustion regression during the walk to bed. Campaign Night3 fuel drain0.0025→0.0015 fixes it; focused real-XRI replay now reaches Night4, zero errors. Twelve ship/task timing models pass; final phase6 headroom targets remain unapproved. See Docs/Playtests/telemetry-s5.md and Docs/Verification/S5/Ship/gallery/. Full legacy PlayMode remains blocked by retired Bootstrap tests; not a full-suite pass.

Live two-Mac join/avatars/mono-AEC configuration/proximity/both radios/mute/guest task/shared ship outcome/shared sleep/leave/rejoin PASS. Separate Night3 host valve + guest wrench and per-keeper fear PASS. Source network remains **LK-S5-SHIPS1 / LK-Lab-Coop-v25**. Human voice, Quest GPU/CPU and usability remain pending. Clean-source ship APK005e641 (v25) is now installed, device APK hash matches the local build, and the app was not launched. Never launch it without request.

**NEXT: Phase3.1 (#98)** sabotage pre-sounds/evidence. Re-read master §2/§3/§9/§13. Reuse NightSchedule/WatchState.Campaign and TensionDirector; a host-owned telegraph must precede the fault by2–5s, evidence persists through repair then fades20s later. Preserve physical fuel exhaustion and the authored initial tutorial leak. Implement duplicate/retry/late-join validation and any new snapshot fields behind a switch; bump the network version for those fields. Do not build a second fault scheduler. No sabotage, lantern or entity implementation is claimed yet.

Reproduction: `caffeinate -i python3 artifacts/s5/ship/night3.py` (use a fresh output path); two-Editor `crew-driver.py` in that folder; `python3 artifacts/remaining/m3/tests.py ship-final ''` for EditMode. The graphics wrappers keep Editors muted. Current board-only spacing change was checked visually after the438-case run; no repeat of the whole suite is needed for whitespace alone.

The six-branch grading cleanup is complete. A teammate merged PR88. Small docs-only PR134 adds the new preview download and removes the private-board link from main; it needs teammate review, not self-merge. Draft PR89 targets main. Sprint5 milestone due date was corrected to December3 (GitHub normalized the earlier midnight input to December2).

## DEVICE CHECKS PENDING

Installed November13: `Builds/LighthouseKeepers-Sprint5-Ships-005e641.apk`, clean source005e641, IL2CPP ARM64/Vulkan, 124,410,010 bytes. SHA-256 `d810d621440287eef54cf33a88d14e3243f21b12ad58d81ce047fd5f85eb33ea`. Non-streamed install succeeded and the installed APK hash matches. **App not launched.** Network **LK-S5-SHIPS1 / v25**; both crew clients need this build. Build/packaging/install receipts: `Docs/Verification/S5/Ship/gallery/final-apk.*.json` and `install.json`. The public quality preview remains v22 and is not this ship build.

When the lead authorizes launch: watch the Night1 preview from the gallery after repairing the pipe; verify P21/P23 and horn location; keep the beacon shining for both Night3 passages, then walk to bed without a fuel stop; test a named wreck/retry. Confirm both-player ship ticks, radio falloff, valve repair and shared sleep. Measure the actual worn-headset views before changing quality settings.

OVR Metrics/Profiler: 72Hz, peak-storm GPU≤11ms, CPU main≤9ms; compare MSAA4 to prior2, scale1.0, fixed foveation0.5, on-tile Render Graph passes, text and powered/blackout bake. No Editor FPS substitutes for these measurements. Human two-way voice, controller comfort, offline Wi-Fi-off and current-build two-Quest play remain pending. Ship/lantern/Drowned worst-case profiling follows their implementation.

## Commands

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-M3
# Unity 6000.3.23f1; EditorPlaytestAudio mutes every play/test session.
# Menu: Lighthouse Keepers/Apply Quest Quality Baseline (includes Lab lighting/probe settings)
# Menu: Lighthouse Keepers/Quality/Rebake Quest Lighting (regenerates both variants)
python3 artifacts/s5/quality/replay.py  # actual-XRI Nights1–3; closes its muted Editor afterward
# Fresh transport: artifacts/s5/quality/crew.py (two muted, matching-source Editors required).
# Evidence: artifacts/s5/quality/ and Docs/Verification/S5/Quality/
```

---

## Historical checkpoints (new brief above takes precedence)

# Full remaining plan — clean checkpoint after Step 2a (November 11)

Branch `sprint-5`, draft PR #89. Nothing merged to main. Editor muted throughout; all test Editors closed. Quest was installed but **not launched**.

## Done

- **1a–1c:** audited existing accepted fixes; repaired the tally-counter obstruction of the starter. Full EditMode 372/372. Full PlayMode 52/83 initially passed; six corrected cases passed focused rerun, leaving 25 inherited failures (listed in evidence). Graphics Nights 1–3 and live two-Mac join/avatars/valve/both radio routes/mute/sleep/leave/rejoin passed. PR #85 ready for review. APK d5051f7 installed, installed hash matches, app closed. See Docs/Verification/RemainingPlan/Step1/.
- **Merged PR #85 into M3:** preserved current PLAY/host-alone/offline lobby, no-clamp solo, physical generator, sea recovery, watch and latest wiring. Campaign valve roles follow authored night AND current roster; join/leave/retry covered. Merge 0b6de74.
- **2a runtime reconciliation:** campaign Night 2 now consumes fuel to E with sputter and mains loss, then asks for refill; data-authored FuelExhaustion replaces the old silent timer reset. Solo clamp metadata removed. 69 focused cases pass. All 12 instruction-only task models pass (five nights + Demo, solo + duo). Actual-XRI graphics Nights 1–3 pass through sleep into Night 4, zero logged errors; inspected generator and Night 3 board. Docs/Verification/M3/scheduling-current/ and Docs/Playtests/telemetry-m3.md.

M3 network **LK-M3-CAMPAIGN2 / LK-Lab-Coop-v22** (campaign + sea-recovery fields). PR #85 installed on Quest is **LK-S5-PLAYTEST1 / v21**. Both clients must use the same build. Campaign remains the previously enabled task preview; timingApproved remains false. Final ship-inclusive 25–35% / 15–25% headroom is NOT yet approved. No night duration/flood retuning this pass.

## Next — Step 2b, then 2c, then horror

1. Implement named ship pass/wreck loop from NightScenario.ships (P21–P24 = cue IDs 96–99). Host owns passage outcomes/dark-window grace; guests consume snapshots. Add snapshot fields/version bump. Keep legacy catalogs outside campaign untouched. Ships must gate sleep; safe Night 1 preview; sighting → rocks → away, spatial foghorn, named loss, pass relief/sting, HUD/board state. Retime provisional authored windows against the current timing trace — existing windows are NOT approved for shipping. Reuse Eli's StormSeascape/ocean, no sky or avatar edits.
2. Re-run instruction-only models including ship waiting/pressure; graphics pass/wreck + screenshots; live campaign co-op. Then physical doors/mirror (2c).
3. Step 3 horror slices 1–3; Step 4 finale/progression; Step 5 horror 4–6; Step 6 split-information/final verification. **None implemented this pass.** No new M3 APK has been delivered since the merge; build/install when the next completed part is ready.

## DEVICE CHECKS PENDING

- Real Quest Wi-Fi OFF HOST / PLAY ALONE → START.
- Wiring GPU ≤0.3 ms and full scene GPU/CPU at 72 Hz; Mac timing cannot establish Quest cost.
- Missing lobby hands / board-like own-body report; physical grip/choke/pour/starter/watch comfort; B recall/text readability.
- Human voice intelligibility, Mac+Quest/2-Quest current-build transport and radio falloff.
- Ships/doors/mirror/horror budgets and comfort only after those systems exist.

Install allowed; **do not launch the Quest** without a new explicit request. Root integrated worktree still has unrelated user Wood.mat, Quest prefab ray-width edits and pipe-lever evidence; preserve them. M3-generated Wood change was backed up and restored after graphics capture.

## Exact commands

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-M3
python3 artifacts/remaining/m3/tests.py scheduling-final
LK_M3_TIMING_OUTPUT="$PWD/artifacts/remaining/m3/timing.json" /Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity -batchmode --burst-disable-compilation -quit -projectPath "$PWD" -buildTarget StandaloneOSX -executeMethod LighthouseKeepers.Editor.M3CampaignBot.RunBatch -logFile "$PWD/artifacts/remaining/m3/timing.log"
# Current mechanics replay: artifacts/remaining/m3/Nights123.cs, entry ImmersionSoloReplay.Run(output).
# replay.py launches a muted batch graphics Editor; inspect/use the corrected Nights123.cs,
# whose pending-stage waits account for authored relief periods.
# No Quest launch command here.
```

## Twelve-step headset checklist

Use the installed PR #85 build for 1–8; 9–12 need later M3 systems and are NOT ready today.

1. PLAY → HOST / PLAY ALONE → START; confirm hands and House.
2. With Wi-Fi off, confirm the same offline route.
3. Tap/hold B for objective + last line; double-tap to replay; adjust objective height.
4. Grip wrench, repair without a solo clamp, drop/recover it, then sleep.
5. Night 2: watch fuel reach E, hear sputter, see mains/pump/beacon cut out.
6. Pour fuel; squeeze primer three times; pull choke up; pull cord three times; push choke down.
7. Find separate POWER PANEL, trace subtle cables, check left-wrist time/objective.
8. Two matching builds: join/start, avatars, voice/radios, shared valve repair and sleep; leave/rejoin.
9. PENDING ship: gallery sighting → beam-lit passage and horn; test named wreck/retry.
10. PENDING doors/mirror: hand-swing doors and inspect Will's avatar from three angles.
11. PENDING horror: apparition, telegraphed hunt, sprint/hide; Scare Off removes it.
12. PENDING Demo: six-minute loop through ship moment, then continue/night-select persistence.

---

# Generator, watch and wiring delivered — November 11

Branch `s4/integrated`, PR #85. Latest scope: [GeneratorWatchWiring.md](GeneratorWatchWiring.md). Work in numbered order, focused tests + one muted graphics solo Night 2 replay per item, commit/push each. No full suites or horror work. Preserve unrelated Wood.mat, Quest prefab ray widths and pipe-lever evidence.

1. DONE: actual Night 2 fuel exhaustion, three-second sputter, empty gauge, mains outage and cause-first narration. Existing network fields only; `LK-S5-PLAYTEST1 / v21` unchanged. MasterDirector 12/12, VoicePass 23/23 pass. Final graphics Night 2 replay passed with cue 4 before refuelling, zero logged errors. Evidence: Docs/Verification/GeneratorWatchWiring/step1/.
2. DONE: mounted numbered generator control layout, fuel can on floor. Focused scene test and solo Night 2 pass; evidence step2/.
3. DONE: physical choke detents; existing pour, starter and grip-pressure primer preserved. Both-hand focused test + graphics Night 2 pass. Evidence step3/.
4. DONE: small dorsal left-wrist digital watch, corrected accessory orientation and gaze backlight. Focused recall/watch checks and Night 2 replay pass; inspected images step4/.
5. DONE: restored/rebaked M3 wiring; no duplicate feed. 13,894 triangles, five meshes, one material, zero unexpected clearance contacts. Four focused cases and Night 2 pass. Wiring was developed only on sprint-5, not dropped by a merge. Evidence step5/.
6. DONE: source **c7b9a99** built successfully. Installed on the connected Quest; installed APK SHA-256 matches. App deliberately left closed.

APK: `/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/Builds/LighthouseKeepers-GeneratorWatch-c7b9a99.apk` (134,388,246 bytes). SHA-256 `757b9d404bb060355e7e9d06fcbd9092eb367bf56c108479859d027ef90dd0c2`. [Delivery/evidence](../Verification/GeneratorWatchWiring/README.md). PR #85 updated; no merge. All five implementation items are done; the listed device checks and separate missing-lobby-hands report remain. Do not resume M3/horror without the next request.

Reinstall without launching:
```sh
'/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb' install -r '/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/Builds/LighthouseKeepers-GeneratorWatch-c7b9a99.apk'
```

## DEVICE CHECKS PENDING (current pass)

- Missing lobby hands / board below body: not reproduced or resolved. Preserve tracked hand rotations and Will's avatar; compare worn headset after START. First-frame logs alone do not establish the cause.
- Night 2 fuel warning, actual blackout, speech timing and reachable controls.
- Both-hand physical choke/pour/starter feel; watch fit/readability/orientation.
- Wiring visibility/clearance and GPU cost <=0.3 ms; complete scene target 72 Hz.
- Two-headset fuel state/presentation, voice/tasks/sleep regression (current pass has replica-focused evidence only).

## Exact current check commands

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
python3 artifacts/playtest-fixes/lobby-state-tests.py MasterDirectorTests generator-step1.json
python3 artifacts/playtest-fixes/lobby-state-tests.py VoicePassTests generator-step1-voice.json
# Stop Play Mode before starting a fresh replay; helpers explicitly mute the Editor.
python3 artifacts/generator-pass/solo.py step1-final
```

---

# Job 1 delivered — STOP before horror (November 11)

On `s4/integrated` / PR #85. Steps 5–6 and the simplified lobby are committed and pushed. Shipping code: **dff6e4b**. Generator vs POWER PANEL are distinct; island rails/sea recovery and one PLAY flow are implemented. No main merge and no M3/horror work in this turn.

**Verification:** 21 focused cases pass, one actual-XRI graphics solo Night 1–2 replay passes, and one live two-Mac smoke passes (host alone, late join, avatars, guest sea recovery, shared valve task, shared sleep, leave/rejoin). Post-smoke defensive recovery and keypad-clearance changes passed focused tests; the entire smoke was not repeated. No full suites, per the lean request. Both test Editors were muted and are now closed. Evidence: [delivery report](../Verification/PlaytestFixes/final/README.md).

**Delivered:** `/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/Builds/LighthouseKeepers-Playtest-dff6e4b.apk`; 133,792,150 bytes; SHA-256 `28b7df5f9f990c0f0e00b01e89c605968206a5dcd6721b407913819f29f4981d`. Installed and launched on the connected Quest as explicitly requested this turn; process alive after 15 seconds. No human headset acceptance implied. Build audio unchanged.

**Network:** LK-S5-PLAYTEST1 / LK-Lab-Coop-v21. Both players must update. Sea-recovery snapshot added; no READY requirement. One player gets solo puzzles; joins/leaves adapt roles immediately. Current night time is preserved on roster changes; the next night uses the new roster's budget.

## Next job (not started)

Merge verified Job 1 into `sprint-5`; re-bake its existing TowerWiring authoring for the moved power panel (do not duplicate the feed). Finish the M3 ship prerequisite, then the queued horror job. Full scope: [PlaytestAndHorrorQueue.md](PlaytestAndHorrorQueue.md). Stop here until asked to continue.

## DEVICE CHECKS PENDING

- ADB screenshot was black; confirm the rendered lobby while wearing the Quest. Startup activity/OpenXR and live process were confirmed.
- Human reach/readability: generator vs separate POWER PANEL, grip-only primer, B recall/objective height.
- Actual Quest Wi-Fi OFF: PLAY → HOST / PLAY ALONE → START on the offline message. Editor forced-unreachable path passed; device acceptance pending.
- Worn island edge traversal; sea fall comfort and ten-second return with a partner; dropped tools and tracked hands.
- Two-Quest real voice listening, shared tasks/sleep/rejoin; worn GPU/CPU and frame budgets.
- Locate the reported separate F3 yellow remnant; the all-floor audit did not reproduce it.

## Exact delivery commands

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
# Reinstall the matching build without erasing app data:
'/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb' install -r 'Builds/LighthouseKeepers-Playtest-dff6e4b.apk'
# Launch only when requested (authorized for this completed delivery):
'/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb' shell am start -n 'com.lighthousekeepers.game/com.unity3d.player.UnityPlayerGameActivity'
```

---

## Earlier checkpoints (historical; latest scope above takes precedence)

# November 11 playtest fixes — ACTIVE CHECKPOINT

Work on `s4/integrated` / PR #85, then merge the verified Job 1 into `sprint-5` / PR #89. Do not merge main. Preserve unrelated Wood.mat, TMP fallback atlas, Quest ray-width prefab edits and pipe-lever evidence. Editors must stay muted; do not launch the Quest without a request.

## Active checkpoint — Steps 5–6 and lobby implemented; final checks/build next

Steps 1, 3 and 4 are implemented and checked; step 2 was audited but the reported separate yellow remnant was not reproduced. Job 1 is NOT finished. Full job-end suites, solo 1–3/two-Mac smoke, APK and M3 merge have NOT run for this checkpoint. Job 2 has not started. The full remaining scope (including the new lobby and horror additions) is preserved in [PlaytestAndHorrorQueue.md](PlaytestAndHorrorQueue.md). Quest absent from ADB on November 11; no app launched.

## Current work

Step 1: solo valve clamp removed from rules, objectives and narration; solo retains the prior post-clamp inflow rate. Two-player hold/release and distinct worker validation remain; disconnect allows immediate solo repair. Valve sits beside the active bolt and moves only when unheld; hidden/non-interactable for one player. Focused pressure 4/4 and director 8/8 pass. Graphics actual-XRI Night 1 → Night 2 passed; live one→two→one valve visibility/placement passed (0.43 m from active bolt). Step 1 complete; evidence in Docs/Verification/PlaytestFixes/step1/. No new replicated fields; network stays LK-S5-VOICE1 / v19 for now. Lobby changes will require a new matchmaking version.

## Next in order

2. Pole audit complete (PlayMode 1/1; inspected F3 screenshot). No obsolete shaft or separate yellow remnant reproduced. Location clarification/device confirmation pending; working pole and safety hatch preserved. Evidence: Docs/Verification/PlaytestFixes/step2/.
3. DONE: grip-only pressure primer at chest height, larger bulb, release/refill, tally and feedback. Focused 3/3 + engine 6/6; left/right actual-XRI graphics pass, screenshots inspected. See InteractionInputs.md and Docs/Verification/PlaytestFixes/step3/.
4. Implemented: short action/object + specific place/floor; nearby controller GRIP/TRIGGER cue. Director tests 9/9 and near/far graphics check pass; final rendered icon verified and screenshot inspected. Director tests now 10/10, including actual rendered mesh. See Docs/Verification/PlaytestFixes/step4/.
5. DONE: separate yellow generator and dark-green wall POWER PANEL, over 4 m apart, one ceiling feed. Focused graphics test 1/1 and inspected screenshot: Docs/Verification/PlaytestFixes/step5/. Previous survey: Current panel is WatchPowerPanel.Create at STN_GENERATOR_PANEL; layout JSON anchor (3.031089,3.3,1.75), yaw 240. Generator mesh is reused by Editor/OneLighthouseM2.cs from the fixture prefab (Diesel crankcase, Alternator cover, Radiator grille). Inspect both world bounds before moving; runtime is generated, so fix the source/anchors and test rebuild. Integrated has ConnectedMachinePresentation legacy riser; M3 replaces it with baked TowerWiring resources, so re-bake that authoring after merge rather than duplicating conduit.
6. DONE: perimeter rails + 160-direction sweep; solo sea loss/retry + host-owned 10 s per-keeper crew recovery. Focused rules 4/4, graphics edge/sea retry 1/1; Docs/Verification/PlaytestFixes/step6/. Network LK-S5-PLAYTEST1 / v21; live transport check queued.
Lobby implemented: one PLAY flow, host-alone/START without READY, existing offline loader and keypad, dynamic puzzle roles; docs updated. Focused 13/13 passed. Final graphics Night 1–2 / live two-Mac / build pending.
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

For M3, merge this branch into `/Users/aneeshvasamreddy/LighthouseKeepers-M3` / `sprint-5`, preserving all night/string assets. Continue A2/A3 runtime schedules, then A4 measurement, ships, doors/mirror, and remaining parts in `M3-FiveNights.md`. The A1 campaign is still disabled. No five-night timing, ship payoff, physical-door or mirror acceptance is claimed. Keep PR #89 draft and never merge main.

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

Final screenshot review also caught main-page Close reappearing over Sound settings on live state refresh. WatchMenu now keeps its main buttons hidden until Back. The new regression and rendered follow-up pass; no synced fields changed. The `859f8b0` APK predates this menu-only correction; next APK must include it.
