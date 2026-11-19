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
