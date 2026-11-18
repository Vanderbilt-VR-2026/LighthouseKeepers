# Current checkpoint — paired-station playtest fixes (#142)

Branch `sprint-5`, PR #89. APK source **ad33062a** is built and installed on Quest 3, **not launched**. Network **LK-S5-COOPFIX1** / **LK-Lab-Coop-v50**; both players must update.

## Done this pass

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

Full-code readability/entry, role-specific clues and Owen walkie reply, mic grip/hold trigger; valve reach and fixed position; lantern bail pose/set-down/submersion; belt carry; generator paired hold; F3 wall/Owen route; face seam; lantern darkness/comfort and sustained Quest CPU/GPU. Audible two-headset voice, full solo nights and human co-op runs remain unverified.

## Build and exact commands

Worktree: `/Users/aneeshvasamreddy/LighthouseKeepers-M3`. Keep `EditorUtility.audioMasterMute=true`. Do not launch the Quest app without a request.

APK: `Builds/LighthouseKeepers-Sprint5-CoopFix-ad33062a.apk` (IL2CPP ARM64, `com.lighthousekeepers.game`). SHA-256: `d667f80dca4b6d9ce2bdc5982ad594cc902fd8d1fbcf04905931e0a2e103b1f0`. Installed copy hash verified.

Focused bridge checks: `python3 artifacts/s5/modes/check.py RadioRelayTests TimedStarterTests EquipmentWorldTests`. Completed build/delivery scripts and receipts: `artifacts/s5/interaction-fixes/{build.py,deliver.py,build/delivery.json}`. Use fresh backup/output paths for any subsequent build; do not reuse the completed build backup over new source changes.

Reinstall only: `/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb install -r Builds/LighthouseKeepers-Sprint5-CoopFix-ad33062a.apk`.

Workstation: both runaway Unity Bug Reporters stopped, logs saved locally; Editors closed; batch build finished. Avoid reopening stale crash reports during testing. Other user applications were left alone.
