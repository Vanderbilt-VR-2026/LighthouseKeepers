# Current checkpoint — paired-station playtest fixes (#142)

Branch sprint-5 / PR #89. Installed APK remains Modes-70977af6; new changes are NOT installed yet.

## Done this pass

Radio code is one complete ordered line (including Owen reply/B recall); House clue is visible to the Machinist only when the relay is active. Radio destination, lever and mic instructions clarified; transmit progress added. Frequency changes each watch and stays stable on retry/rejoin. B teaching limited to two reminders. 20 focused EditMode checks passed, Editor muted. See Docs/Playtests/2026-11-18-equipment-and-radio.md.

## Next, in order

1. IMPLEMENTED: fixed compact pipe bypass, handle-constrained lantern grip, dropped lamps/walkies stay put, host-owned water extinguish, visible hip clip with click/haptic. 15 focused prop rule tests pass. Physical input/scene checks still due below.
2. IMPLEMENTED: generator compression-release assist; distinct keeper required for timed pulls, leased hold/release/timeout, bot support/duty instructions, replicated holder. Network LK-S5-COOPFIX1 / LK-Lab-Coop-v50. 39 focused checks pass (TimedStarter 4, Companion 5, Survival 7, VoicePass 23); live two-Editor check still due.
3. DONE: cloth progression removed, 76 solid stair backing panels, 20/20 nav/collision routes, darker powered/emergency bakes, Owen beard-shell clipping removed (fine facial shading/worn acceptance pending). 40 focused combined tests and 16 actual XR input-path checks pass; Owen walks to assist and fetches complete code.
4. Focused tests and muted live solo/co-op checks, screenshots inspected, build + install if attached (do not launch). Update PR #89 and issue #142 with evidence.

## DEVICE CHECKS PENDING

Full code readability/entry, role-specific clues and Owen walkie reply, mic hold/trigger; valve reach and stable position; lantern grip/set-down/submersion; belt carry; generator paired hold; F3 wall/Owen route; face; darkness and sustained Quest GPU/CPU. Prior modes acceptance #141 still open. No voice recording work until #122 at the end.

## Exact commands

Worktree: `/Users/aneeshvasamreddy/LighthouseKeepers-M3`. Keep `EditorUtility.audioMasterMute=true`. Focused bridge checks: `python3 artifacts/s5/modes/check.py RadioRelayTests CampaignOrderTests SurvivalModeTests RadioBeltTests`. Use fresh artifacts/s5/interaction-fixes paths for subsequent evidence/build backups. Last installed package com.lighthousekeepers.game, network LK-S5-MODES1; do not mislabel it as new source.
