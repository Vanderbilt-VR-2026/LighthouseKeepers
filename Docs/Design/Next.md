# Active checkpoint — Day 2 nightmare #144 (continued)

Worktree `/Users/aneeshvasamreddy/LighthouseKeepers-M3`, branch `sprint-5`, PR #89. Source protocol **LK-S5-NIGHTMARE1 / LK-Lab-Coop-v53**. The bedtime nightmare is **OFF** in the asset pending the checks below; the earlier generator-room possession and revive lesson remains enabled, and cannot overlap it. No new APK has been delivered; installed source **bf86ddd0** is still the tool-belt build.

This pass: the child now reaches the wall, rotates and crawls along a collision-checked ceiling path; 10 controlled scene checks, a matching two-Editor pose and 59 focused tests passed ([evidence](../Verification/S5/NightmareFoundation/CeilingCrawl/README.md)). Baked the six stair marks so playback no longer cooks tower mesh colliders; the first presentation measured 23.1 ms before and 5.3 ms after in the Mac Editor, with 46 focused tests and a two-Editor mesh/catch/revival check passing ([evidence](../Verification/S5/NightmareFoundation/HandprintBake/README.md)). Fixed stair handprints that were stretching onto other floors; six rail-mesh checks and 40 focused EditMode checks pass ([evidence](../Verification/S5/NightmareFoundation/Handprints/README.md)). A single 16-spark generator-room bulb pop uses the existing URP material with no added realtime light; 8 controlled Play Mode checks and 63 focused EditMode checks passed ([evidence](../Verification/S5/NightmareFoundation/BulbBurst/README.md)). A measured inner-ring drop search now places the child behind the crew while clearing both escape routes; a five-second chase lead (the earlier 2.5-second lead caught a simulated guest at the first door); a brief shared-catch lunge with wall and 0.5 m viewer-clearance checks; a wall/ceiling movement pass and flashlight lighting for the child; keep the blackout through failure; preserve the existing Floor 2 lesson and its rescue. **94 focused EditMode checks passed.** Controlled play checks covered the existing lesson, actual bedtime/gaze, water restoration and transition into Day 3. Two muted Editors passed shared catch, grip/release/regrip rescue, physical-input escape doors, rejoin and retry. Separate natural two-Editor wake-to-return runs passed with virtual input and a retry checkpoint. After the drop change both roles also escaped in staged solo Editor runs; 50 focused checks passed. See [drop evidence](../Verification/S5/NightmareFoundation/Drop/README.md) and [earlier evidence](../Verification/S5/NightmareFoundation/Continuation/README.md).

## Next, in order

1. Finish #144. Complete an unstaged Night 2 for both roles (including the existing lesson and bedtime), review the stylized child model, worn movement and audio; inspect the corrected wet prints and new bulb pop in the headset. The inner-ring drop clears both calculated routes in the tested poses; it deliberately stays at the corner if no safe candidate exists. Check lunge/door reach/audio in both eyes, and a full two-human escape, caught-player rescue and retry. Keep the flag off while behavior/presentation is not ready. When ready enable `Assets/WatchDemo/Resources/HorrorTuning.asset` `nightmareEnabled`, rerun relevant regressions, build IL2CPP ARM64 and install without launching. Keep #144 open for worn device acceptance.
2. Implement #145 Day 3 cursed ship, then #148 Day 4 flood/crew chase, then #149 Day 5 finale from the two copied scare briefs, preserving Survival and the existing radio/authentication/ship flows. Each is more than a device check. Commit and push each step, update this checkpoint, and bump the network version for any synced data.
3. Final full-campaign solo/co-op, failure/retry/rejoin, screenshots, headset comfort/performance and PR update. Do not claim headset results from Editor checks.

## Exact restart commands

Use Unity 6000.3.23f1 on the worktree; set `UnityEditor.EditorUtility.audioMasterMute=true` before **each** Play Mode or test run. Focused EditMode: `python3 artifacts/s5/modes/check.py NightmareIntegrationTests ScareSequenceTests CampaignOrderTests CrewRecoveryTests SurvivalModeTests VoicePassTests StoryImportTests CoopCommandWindowTests StationGuidanceTests HouseFloodTests`. For two muted Editors, freshly back up `/Users/aneeshvasamreddy/LighthouseKeepers-S5-Peer` before syncing; see `Docs/Verification/S5/NightmareFoundation/Coop/README.md` and `Continuation/README.md`. The latest peer overlay backup is `artifacts/s5/final-scaring/peer-backup-11`. Use fresh output and backup paths. Build/delivery templates are `artifacts/s5/tool-belt/build.py` and `deliver.py`; change paths and version, close Editors first, **install but do not launch**.

## DEVICE CHECKS PENDING

Full Day 2 comfort, gaze, doors, catch clearance, rescue, audio, child motion and rail-print visibility in both eyes; profile first-scene presentation CPU cost on Quest; both flashlights/shader on Android; two-headset join/escape/rejoin; sustained Quest 72 Hz and GPU ≤11 ms in the worst scene. Prior tool-belt, pink-material and eye-comfort checks remain pending below. Do not describe the unbuilt Day 3–5 scenes as device-only gaps.

---

# Current checkpoint — Day 2 nightmare #144 in progress

Branch **sprint-5**, PR **#89**. Read/copied the Day 4–5 brief and created **#148** and **#149**. Continue **#144 → #145 → #148 → #149**. Day 3–5 set pieces are not implemented. The older Day 2 possession lesson is separate from the new nightmare.

Source network: **LK-S5-NIGHTMARE1 / LK-Lab-Coop-v53**. `HorrorTuning.nightmareEnabled` remains **OFF**. The child starts on the F3 stair landing, waits for gaze before the first line, and follows the authored route. Retry points are separate and clear of the F3 stair liner; late-joining replicas no longer reposition on an old retry. The flashlight now illuminates the Drowned shader. During return/relief, flood visuals are hidden without losing the actual water state. The chase uses a safe-side drop away from each known exit route, a 2.5 s reaction window and 2.15/2.30 m/s pursuit; tune further from real players.

**92 focused EditMode checks passed.** Both solo roles and a live two-Editor crew escaped in automation using XRI movement, stamina and grip/pull doors; button and hand poses were injected. Bed/gaze/dry-return/restoration checks passed; screenshots were inspected with the Quest URP asset in the Editor. [Evidence and exact limitations](../Verification/S5/NightmareFoundation/RouteAndWake/README.md). The two-Editor gaze/wait, pursuit catch, release/regrip revive, shared escape, leave/rejoin and explicit retry checks are [here](../Verification/S5/NightmareFoundation/Coop/README.md). Scene beats and poses were staged. None is a complete human/two-headset run. No new APK built or installed; **bf86ddd0** remains on Quest, not launched.

## Next — finish #144 before proceeding

1. Run a natural full Day 2 through bedtime, follow, pursuit, both exits and a caught-player rescue without staging scene beats or positions. The two-Editor fixtures now exercise gaze/wait, AI catch, both physical movement routes, release/regrip, leave/rejoin and explicit retry with controlled setup. Keep the feature OFF until the complete flow and presentation are safe.
2. Finish and inspect wall→ceiling→safe drop and shared catch-lunge, with flashlight and wet handprints; check audio audibility and light bursts. Run the complete Day 2 and verify the ordinary Day 3 transition. No camera movement or face intersection.
3. After those checks, enable the asset, rerun impacted checks, build IL2CPP ARM64 and install on the Quest **without launch**. Keep #144 open for worn acceptance. Then implement #145 cursed ship, #148 flood/crew chase, and #149 finale, reusing the kit. Flashlights/unlimited power supersede old lantern/oil language. Deliver the eight-step Day 4–5 headset test with the final build.

## Exact commands / state

Worktree `/Users/aneeshvasamreddy/LighthouseKeepers-M3` on sprint-5; do not change the main folder's coop/two-headsets branch. Open Unity 6000.3.23f1 on that worktree. Before **every** test/Play Mode instance: `UnityEditor.EditorUtility.audioMasterMute=true`.

Focused EditMode: `python3 artifacts/s5/modes/check.py NightmareIntegrationTests ScareSequenceTests CampaignOrderTests CrewRecoveryTests SurvivalModeTests VoicePassTests StoryImportTests CoopCommandWindowTests StationGuidanceTests HouseFloodTests`.

Play: `python3 artifacts/s5/scare-scenes/start.py`, wait for Campaign Day 2. Pipeline `run_script` accepts `Docs/Verification/S5/NightmareFoundation/RouteAndWake/fixtures/GeneratorProbe.cs` / `GeneratorStartProbe.Begin`, `WakeDryProbe.cs` / `WakeDryProbe.Begin`, `SpawnProbe.cs` / `SpawnProbe.Run`, or `FlashlightChildProbe.cs` / `FlashlightChildProbe.Begin`; pass one fresh absolute output path. Stop Play between fixtures. The button and head/hand poses are simulated as noted in the evidence. For the earlier child-route fixture use `Docs/Verification/S5/NightmareFoundation/fixtures/NightmareProbe.cs` / `NightmareProbe.Begin`.

Peer worktree: `/Users/aneeshvasamreddy/LighthouseKeepers-S5-Peer`. Sync template `artifacts/s5/final-scaring/sync-peer.py`; use a **new backup path**, since `peer-backup`, `peer-backup-2`, `peer-backup-3` already contain prior work. Current co-op fixtures and ordered commands are in `Docs/Verification/S5/NightmareFoundation/Coop/README.md`; use fresh output paths and mute both Editors.

After the feature is ready and committed, copy `artifacts/s5/tool-belt/build.py` and `deliver.py` into a fresh folder and update all output/backup paths, APK name and v53 receipt. Close both Editors. Delivery installs with adb and verifies hash; **do not launch**.

## DEVICE CHECKS PENDING

Quest was connected. New nightmare worn comfort/readability, door reach, audio, full two-headset escape/revival, Android shader correctness and sustained 72 Hz / GPU ≤11 ms are **NOT VERIFIED**. Day 3–5 still have implementation work, not merely device checks. Keep prior tool-belt and pink-rendering acceptance below.

---

# Current delivery — tool belt and Day 1 guidance (#147)

Branch sprint-5, PR #89. Implemented dedicated waist clips (walkie right, flashlight left front, wrench left side), host carry validation and Day 1 local-objective-only writing/control glow in both modes. Lighting and hand poses unchanged. Network **LK-S5-TOOLBELT1 / LK-Lab-Coop-v52**.

12 final focused tests + 28 earlier voice/version checks passed; two unchanged legacy director wording assertions remain (10/12 director checks pass). 19 actual XRI belt checks, 34 rendered guidance checks, and live two-Editor grab/clip/regrip/drop for all three tools passed. Screenshots inspected. [Evidence and five-step headset test](../Verification/S5/ToolBelt/README.md).

APK **bf86ddd0** built (IL2CPP ARM64, normal package), installed on Quest 3 with matching device SHA-256, **not launched**. SHA-256 `9d60ac9a9a53e59adc8a607a95f0806e0529d7c0c68a8c729bed52a114daeeb9`. [Receipt](../Verification/S5/ToolBelt/delivery.json).

APK: `Builds/LighthouseKeepers-Sprint5-ToolBelt-bf86ddd0.apk`. Reinstall only: `/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb install -r Builds/LighthouseKeepers-Sprint5-ToolBelt-bf86ddd0.apk`. Editors closed; build settings and importer churn restored. Next: headset acceptance for #147; then resume #144 Day 2 nightmare and #145 Day 3 cursed ship. Neither new scripted scene is implemented. Build scripts in `artifacts/s5/tool-belt/`; use fresh backup directories for another build.

## DEVICE CHECKS PENDING

#147: worn clip reach and body clearance while moving/crouching; all three tools together; both-eye local Day 1 text; partner carry appearance; disconnect/rejoin; full-night and sustained device performance checks. Keep #147 open. Prior pink-material correction and reported eye discomfort still need headset acceptance; no lighting change in this task.

---

# Current correction — pink Quest environment (#146)

**Do not use APK 335a1ff0:** the user reported an all-pink environment. Removed the runtime room-material replacement and its custom shader. Original URP materials restored; bulb Morse remains, baked pools stay steady. Flashlights/grips/gameplay unchanged. Seven focused tests and 21 rendered checks passed; House/F2 captures inspected. [Correction evidence](../Verification/S5/Flashlights/PinkFix/README.md).

APK **592673ef** built, installed on Quest 3; installed SHA-256 matched, **not launched**. SHA-256 `97b7ea2d2a5c11fc897e75cc648d8024279e58a3bfeae3d5205d928a49a1224e`. [Receipt](../Verification/S5/Flashlights/PinkFix/delivery.json).

APK: `Builds/LighthouseKeepers-Sprint5-FlashlightFix-592673ef.apk`. Reinstall only: `/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb install -r Builds/LighthouseKeepers-Sprint5-FlashlightFix-592673ef.apk`. Build asset report excludes the removed shader. Completed scripts: `artifacts/s5/pink-fix/{build.py,deliver.py}`; choose fresh backup directories for future builds. Network remains **LK-S5-FLASHLIGHT1 / LK-Lab-Coop-v51**. Editors closed and build settings restored.

## DEVICE CHECKS PENDING

Reopen manually and verify House walls/floor, F2 generator and flashlight illumination render normally in both eyes. Quest rendering/performance is not established by Editor captures. Keep #146 open. Resume #144 then #145 only after this corrective delivery is complete.

---

# Current checkpoint — focused flashlights and Morse bulbs (#146)

Branch `sprint-5`, PR #89. Network **LK-S5-FLASHLIGHT1 / LK-Lab-Coop-v51**; both players must update. The user's flashlight request supersedes LOW/HIGH lantern operation: two metal flashlights, GRIP carry, TRIGGER ON/OFF, unlimited power every night in both modes. No new payload fields. Hand contacts are measured for both hands; no global avatar or wrench changes.

Implemented and checked: focused 28°/9 m beam; no oil can/gauge or fuel prompts; one supported bulb in House and each floor, different softened Morse patterns; House/F1 backup on, upper bulbs require LIGHTS; powered/emergency lightmaps rebaked. **78 focused tests, 12 actual XRI grip checks and 11 rendered checks passed.** Screenshots inspected. Full nights, worn comfort/performance and a new live co-op check are NOT VERIFIED. [Evidence and six-step test](../Verification/S5/Flashlights/README.md).

APK source **335a1ff0**, IL2CPP ARM64 / normal package, installed on Quest 3 with matching device SHA-256; **not launched**. SHA-256 `f5a72ba43be465fd3eff8a20d1fd458ee37510f90607da06588e499ce69ff532`. [Delivery receipt](../Verification/S5/Flashlights/delivery.json).

APK: `Builds/LighthouseKeepers-Sprint5-Flashlights-335a1ff0.apk`. Reinstall only: `/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb install -r Builds/LighthouseKeepers-Sprint5-Flashlights-335a1ff0.apk`.

Completed build/delivery scripts: `artifacts/s5/lantern-beams/{build.py,deliver.py}`; use a fresh backup/output directory for a future build. Editor closed; build settings restored; no Quest launch.

## Next

1. #146 implementation, build and delivery are complete. Headset acceptance remains open with needs-headset-check; run the six-step checklist and fix concrete failures.
2. Resume **#144 Day 2 nightmare**, then **#145 Day 3 cursed ship**, exactly as the detailed Part 0 checkpoint below. Re-read the complete `Docs/Design/S5-Day2-Day3-Scares.md`; replace lantern terminology with the new flashlight where needed. Neither new scripted scene has been implemented yet.

## DEVICE CHECKS PENDING

#146: both-hand grip during motion/crouching, close glare/far reach and both-eye readability, regrip/hooks, five Morse bulbs and comfort, two-player tool ownership/light appearance, sustained peak-storm 72 Hz / GPU ≤11 ms with both beams. No device profiling claimed. Existing #142/#143 acceptance and #144/#145 implementation remain outstanding below.

---

# Current checkpoint — Part 0 controls and lantern (#143)

Branch `sprint-5`, PR #89. Network **LK-S5-SCARES1 / LK-Lab-Coop-v51**. Both players must update. Part 0 implementation and focused Editor checks are complete. APK source **83883309** built and installed on Quest 3; installed hash verified; **not launched**. Stopped cleanly before #144; both scripted scenes remain unimplemented.

Done: A run (4.5 s / 3 s released refill), X breath, removed arm-swing; forward lantern and oil teaching (Day 1 unlimited); visible swept flare, moving-target hits, independent replicated flare clock during grace, shared impact and burning miss. 76 focused EditMode tests; 18 Play Mode collision cases; guest firing and breath lease in two muted Photon Editors; inspected screenshots. [Evidence and six-step test](../Verification/S5/ScareControls/README.md).


APK: `Builds/LighthouseKeepers-Sprint5-ScareControls-83883309.apk` (IL2CPP ARM64, normal package). SHA-256 `66e679f6706c88c6074892bd87bb268da90df9bfcbf4bf74185a3269c9bc4454`. Receipt: `Docs/Verification/S5/ScareControls/delivery.json`.

Reinstall only: `/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb install -r Builds/LighthouseKeepers-Sprint5-ScareControls-83883309.apk`.

Completed build scripts: `python3 artifacts/s5/scare-scenes/build.py` then `python3 artifacts/s5/scare-scenes/deliver.py`; **choose fresh backup/output directories before another build**, since build-final is a completed receipt. Both test Editors and their crash reporters are closed. Build audio was not changed and the Quest app was not launched.

## Next, in order — not yet implemented

1. **#144 Part 1, Day 2 nightmare.** Read `Docs/Design/S5-Day2-Day3-Scares.md` fully again. Replace the old mid-Day-2 possession lesson with the end-Day-2 sleep interception, 3 AM wake-up, child follow/wait/gaze, wall/ceiling turn, warned escape, caught-in-place possession, outside-then-return revival and wake-up retry. Reuse CrewRecoveryState/CrewRecovery presentation and HunterNavigation. Preserve Survival. Add host-owned scene clock/state, position/phase in WatchSnapshot with strict validation; bump both versions. Do not reuse the paused watch clock for scene animation. Solo escape/fail as brief; two-human victim remains at catch point. No camera movement. Reusable scare kit and dev buttons. New scene TTS only as specifically requested in this brief, catalogue exact text; routine narration remains disabled.
2. **#145 Part 2, Day 3 cursed ship.** After #144 is verified and pushed: challenge-word/password + real/fake audio tell and real cost; six non-repeating visible deck points; gaze blood rain; scripted wreck excluded from failure accounting; warned safe near-beacon reveal; relentless navigable chase; flare hit/one-shot miss outcomes, caught/revive; after-scene normal watch progression. Add dev buttons, branch/path/network tests and inspected captures. Preserve the existing radio code relay and unaffected ship outcomes.
3. Build/install each completed slice, never launch. Verify headset comfort/performance separately; keep issues open with needs-headset-check until accepted.

## Exact restart commands and tooling

Worktree `/Users/aneeshvasamreddy/LighthouseKeepers-M3`. `git switch sprint-5`; `git status --short`; read this file and `Docs/Design/S5-Day2-Day3-Scares.md`. Use the existing Unity 6000.3.23f1 project and Pipeline bridge. Always set `UnityEditor.EditorUtility.audioMasterMute=true` before tests/Play Mode. Peer worktree: `/Users/aneeshvasamreddy/LighthouseKeepers-S5-Peer`; previous peer changes preserved in `artifacts/s5/scare-scenes/peer-backup`.

Focused checks: `python3 artifacts/s5/modes/check.py DreadPlaytestTests FlareTests LanternTests SurvivalModeTests VoicePassTests StoryImportTests CoopCommandWindowTests CrewRecoveryTests QualityBaselineTests`. Play fixture: `python3 artifacts/s5/scare-scenes/start.py`, then Pipeline `run_script` entries `RunFlareTests.Begin` and `VisualProbe.Begin` from the files in `artifacts/s5/scare-scenes`; output paths must be fresh. Two-Editor fixture: `python3 artifacts/s5/scare-scenes/crew.py` with both Editors open and peer synced. Physical test platform is test-only, never serialized. Do not run a second fixture concurrently. A CoreAudio crash happened once on peer restart; preserve recovery backups and close only that test Editor's reporter.

Build scripts use fresh backup directories; never reuse a previous build-backup over newer source. Part 0 scripts/receipts: `artifacts/s5/scare-scenes/{build.py,deliver.py,build-final/}`. Completed build source must be clean before starting. Do not launch the Quest.

## DEVICE CHECKS PENDING

Part 0: six steps in ScareControls report; sustained GPU/CPU at peak storm with two HIGH lanterns and flare; both-eye beam/flare readability, grasp direction, run/breath comfort, co-op worn acceptance. Parts 1/2 all scene acceptance is pending implementation, not a device-only gap. Keep previous equipment/radio/lighting acceptance below pending too.

# Current checkpoint — House light and Survival practice day (#142)

Branch `sprint-5`, PR #89. APK source **cec81c8e** built and installed on Quest 3, **not launched**. Network **LK-S5-PRACTICE1 / LK-Lab-Coop-v50**; both players must update. No new snapshot fields.

## Current follow-up

Held walkies rotated 180° with preserved hand wrap. House instruction lamp always on, larger baked pool in both powered/blackout states; slow per-room flicker. Survival Day 1 is a practice watch: slow water capped at 0.22 m, no flood loss or oil drain; still requires all jobs and ship guidance. Day 2 restores pressure. 58 focused tests and 32 actual XRI checks pass; blackout and controller water-cap checks pass. Inspected images and limits: [HousePractice](../Verification/S5/HousePractice/README.md).

Build/install complete; device APK hash matched. PR #89 and issue #142 updated. Next action is headset acceptance of the five checks in HousePractice; keep #142 open until verified.

## Grip/darkness follow-up delivered (#142)

Implemented: wrap-around walkie contacts; quarter-turn lantern bail grip; beacon wheel contacts at any rim point; true blackout lightmaps and weak baked room bulbs; LOW/HIGH lantern range 0.8/1.5 m; Owen repair/tool gestures and narrower collar-covered neck; cooperative opening wording; early-night grace/flood cushion. 73 focused tests, 30 XRI grip checks, map switching and six live helper work checks pass. Images inspected. No new snapshot fields; network now **LK-S5-GRIPDARK1 / LK-Lab-Coop-v50** because grace validation uses matching tuning.

Build/install complete. Installed APK hash matches source build `823c5781`; app **not launched**. PR #89 and issue #142 updated; headset acceptance remains pending. [Evidence and checklist](../Verification/S5/GripDarkness/README.md).

## Previous paired-station pass

- Full ordered radio code, role-specific House clue, daily frequency stable on retry/rejoin, explicit F4 lever/mic instructions, transmit progress and limited B reminders.
- Fixed compact pipe bypass; top-bail lantern grip; persistent dropped lanterns/walkies; host-owned underwater extinguishing; visible right-hip walkie clip.
- Paired generator START ASSIST hold, preserving three timed pulls and Owen's complementary duty. Live transport exposed and fixed a missing command whitelist entry.
- Removed cloth/lens-wipe progression in both modes. 76 solid stair backing panels; 20/20 navigation/collision routes. Darker powered/emergency bakes. Owen's duplicate beard shell removed; fine facial shading remains a polish check.
- 40 combined focused EditMode checks, then 9 related checks after the transport fix (overlapping groups); 16 actual XR input-path Play Mode checks. Owen physically navigated to assist and fetched the whole code. Two live muted Editors passed host/join, physical guest assist/valve inputs, role clue, lantern/radio sync, rejoin, ship passage, shared sleep into Day 2 and retry. Full suites and worn acceptance were not performed.

[Evidence, inspected images and headset checklist](../Verification/S5/EquipmentRelay/README.md). [Playtest report](../Playtests/2026-11-18-equipment-and-radio.md).

## Next, in order

1. Headset acceptance of #142 and existing modes #141. Keep both open with needs-headset-check until verified. Apply concrete failures from the checklist before expanding scope.
2. Remaining Campaign gaps: older six-minute Demo timing, Northern Star watcher and the warned Night 3 bunk/hiding lesson. Preserve the no-horror Survival mode.
3. Final voice recording is #122, last. No new AI narration.

## DEVICE CHECKS PENDING

New: flipped walkie orientation, always-on House light and slow flicker, Survival practice Day 1 → real flood pressure on Day 2. Both-hand walkie/lantern grip, rim grabbing, much darker room navigation and LOW/HIGH usefulness, Owen work gestures/collar, Days 1–2 balance; full-code readability/entry, role-specific clues and Owen walkie reply, mic grip/hold trigger; valve reach and fixed position; lantern bail pose/set-down/submersion; belt carry; generator paired hold; F3 wall/Owen route; face seam; lantern darkness/comfort and sustained Quest CPU/GPU. Audible two-headset voice, full solo nights and human co-op runs remain unverified.

## Build and exact commands

Worktree: `/Users/aneeshvasamreddy/LighthouseKeepers-M3`. Keep `EditorUtility.audioMasterMute=true`. Do not launch the Quest app without a request.

APK: `Builds/LighthouseKeepers-Sprint5-Practice-cec81c8e.apk` (IL2CPP ARM64, `com.lighthousekeepers.game`). SHA-256: `2c2f4e236326c657caab13feeb3591f479ac40acd6cc8382691c8a43f30490c9`. Installed copy hash verified; game not launched.

Focused bridge checks: `python3 artifacts/s5/modes/check.py SurvivalModeTests RadioRelayTests TimedStarterTests QualityBaselineTests LanternTests VoicePassTests StoryImportTests`. Play Mode and XRI checks: `python3 artifacts/s5/interaction-fixes/start.py` then `python3 artifacts/s5/house-practice/play.py` with the muted Editor open. Completed build/delivery scripts and receipts: `artifacts/s5/house-practice/{build.py,deliver.py,build/delivery.json}`. Use fresh backup/output paths for any subsequent build; do not reuse the completed backup over new source changes.

Reinstall only: `/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb install -r Builds/LighthouseKeepers-Sprint5-Practice-cec81c8e.apk`.

Workstation: no interactive Editor or crash reporter left running; batch build finished. Other user applications were left alone.
