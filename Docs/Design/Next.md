# Current checkpoint — Campaign / Survival modes (#141)

Branch **sprint-5 / PR #89**. Current APK source **70977af6**, `Builds/LighthouseKeepers-Sprint5-Modes-70977af6.apk`. IL2CPP ARM64, normal package, signature and ZIP validation passed. SHA-256: `e05c1cb017ab6afb7e3168035e34630d210b6f975cfd4fe8d76ff927015509ca`. **Installed on Quest 3; installed APK SHA-256 verified. Not launched.**

Network **LK-S5-MODES1 / LK-Lab-Coop-v49**. Both players must update. Mode is host-owned in the Photon session and game snapshot.

## Done

PLAY → Campaign / Survival → existing host/join/duty flow. Campaign preserved. Survival: all stations Day 1, new fuel/timed-pull generator, co-op valve and House/F4 code relay, Owen's complementary duty from Day 1, original adaptive soundtrack, no horror or narration, only flood loss, safe ship waiting when off beam, elapsed-time wristwatch, unlimited watches after shared sleep, daily flood/aim escalation. All tuning in `Assets/WatchDemo/Resources/SurvivalTuning.asset`. See `Docs/Design/GameModes.md`.

56 focused EditMode tests, 3 rendered PlayMode lobby tests, live muted offline smoke and two-Editor transport passed. Two-Editor checks: mode inheritance, lantern/radio sync, rejoin, ship pass, shared sleep into Day 2, retry. Screenshots inspected. A wrong new string field caught in the live run was fixed and regression-tested. Source commits `4d2bdfc5`, `70977af6`. Build receipt and scope of evidence: `Docs/Verification/S5/GameModes/`.

## Next

1. APK installed and verified; do not launch without a request. Use `Docs/Playtests/2026-11-18-survival-modes.md` for the human check. Issue #141 stays open with needs-headset-check.
2. Take feedback on Survival duty clarity, Owen, pressure and wheel difficulty before broadening it.
3. Deferred Campaign work remains: Northern Star watcher; warned Night 3 bunk/hunter/hiding lesson; older six-minute Demo pacing. The old Demo still fails its timing model; Survival is the new judges' mode. Final voice recordings #122 remain LAST. No AI speech.

## DEVICE CHECKS PENDING

Mode flow and correct host mode on both headsets; offline Survival with Owen; all four stations on Day 1; paired valve and code relay; timed starter and circular beacon turning; shared sleep and daily difficulty; flood-only loss/retry and sea recovery; original soundtrack/no horror in Survival; Campaign horror/score unchanged; B recall and elapsed wristwatch; spoken multiplayer voice, lantern visibility, comfort and sustained GPU/CPU. Full autonomous Owen playthrough remains unverified. No Editor result is headset acceptance.

## Exact commands

Worktree: `cd /Users/aneeshvasamreddy/LighthouseKeepers-M3`. Leave the Desktop checkout untouched. Keep Editor audio muted via `EditorUtility.audioMasterMute = true` before every play/test.

Focused checks: `python3 artifacts/s5/modes/check.py SurvivalModeTests CampaignOrderTests RadioRelayTests ShipAimingTests CrewCompanionTests VoicePassTests`.

Live offline: `python3 artifacts/s5/modes/solo.py`. Two-Editor: `python3 artifacts/s5/modes/crew.py`; both editors fully imported and muted, source and backed-up peer overlay synchronized before Play Mode. Never copy peer changes back onto source. Evidence includes the final clean restart after an Editor domain reload interrupted the first harness attempt.

Lobby tests: Pipeline `run_tests`, mode `playmode`, filter `CrewLobbyBoardTests`, `async_tests: true`; read `Temp/pipeline_test_status.json`. No Asset edits while Play Mode/tests/build are running.

Build driver: `artifacts/s5/modes/build.py` (use a NEW output/backup directory before another build). Package validation / optional install: `python3 artifacts/s5/modes/deliver.py` (restores only documented builder settings from the backup).

Install existing APK without launch:

```sh
"/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb" devices -l
"/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb" -s 2G0YC1ZG32048T install -r Builds/LighthouseKeepers-Sprint5-Modes-70977af6.apk
```
