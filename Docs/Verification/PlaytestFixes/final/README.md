# Job 1 delivery verification — November 11

## Works in the recorded checks

- Focused checks: 21 unique cases passed. Generator/panel placement 1; island sweep/solo sea retry 1; sea recovery rules 5; local-only recovery fade 1; controller keypad 2; offline launcher/shutdown 2; host start policy 2; Day 3/count rules 7. No full suites, as requested.
- One graphics actual-XRI solo replay: Night 1 grip-only pipe → drop/recovery → drain → bed → Night 2 scheduled outage → three grip-pressure primer squeezes → choke/starter → power restored → pipe → bed → staged Night 3. Zero replay errors. This is scripted Editor input, not a first-time human timing study.
- One live two-Mac Fusion room (`3Z7T5T`): online host START alone without READY; guest joins ongoing watch; remote avatars; mono/AEC/noise-suppression setup; guest sea fall returns to the House after ten seconds while host continues; guest bolt repair with host holding valve updates both boards; shared bed advances both to Night 2; guest leaves, host gets solo valve rules; guest rejoins the same Night 2.
- A stale script class name interrupted the smoke harness before guest repair. Corrected the entry name and resumed the same room; the original error remains in events.jsonl. No gameplay workaround.
- Post-smoke defensive follow-up: recovery follows the surviving actor if the host leaves; host recovery fade never enters the shared sleep snapshot. Focused 5/5 + 1/1 cover these cases; the entire smoke was not repeated. Larger menu labels passed tracked-pointer/text fit tests 2/2. Screenshot review then caught keypad overlap with the code field; its position was corrected and both focused cases passed again, including a new bounds assertion.

## Not verified

Quest controller feel, worn readability/performance, real Wi-Fi-off play, human voice intelligibility and two-Quest execution. Editors stayed muted; audio configuration is not a listening claim. The earlier separate yellow F3 remnant remains unreproduced, not claimed removed. No new horror, campaign, ship, door or mirror work in this chunk.

## Delivery

Network **LK-S5-PLAYTEST1 / LK-Lab-Coop-v21**. Both players must update. Final APK source **dff6e4b**; build/install receipts are included here. The first packaging attempt refused an existing output filename; rebuilt to a fresh commit-labelled path, with no pipeline bypass. Changes remain on s4/integrated / PR #85; main untouched. The previously recorded full-suite inherited failures remain documented in older reports; no full-suite claim for this lean pass.

The final solo/crew screenshots and the station/lobby captures were visually inspected. The pre-font-enlargement host-code shot is retained only in raw artifacts; final lobby screenshots show the shipping labels.

Final source **dff6e4b**, APK `LighthouseKeepers-Playtest-dff6e4b.apk` (133,792,150 bytes).
SHA-256: `28b7df5f9f990c0f0e00b01e89c605968206a5dcd6721b407913819f29f4981d`.
Installed with `adb install -r` and launched successfully on the connected Quest; process remained alive after 15 seconds. No uninstall or data wipe. This is installation/startup evidence, not a worn-headset gameplay or performance check.

Quest startup logs reach OpenXR FOCUSED with no NullReference/MissingReference/FATAL startup lines found. The ADB screenshot was black, so rendered lobby/headset readability are NOT confirmed by that capture. Raw capture/log remain under artifacts/playtest-fixes/final/device/.
