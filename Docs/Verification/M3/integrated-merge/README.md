# Integrated fixes merged into M3

PR #85 source d5051f7 plus delivery documentation 3a6e940. All merge conflicts reviewed; latest no-clamp solo, grip-pressure primer, physical choke, fuel-warning presentation, PLAY lobby, sea safety, watch and rebaked wiring retained. Campaign schedules/replica fields retained.

Network **LK-M3-CAMPAIGN2 / LK-Lab-Coop-v22** combines the campaign schedule and sea-recovery snapshot. Both players need this M3 build; it does not match PR #85.

**68 focused EditMode cases pass**, covering campaign data/schedule/replica, current roster valve policy, Day 3, director and voice. Online host alone stays solo; joining/leaving adjusts valve roles; retry preserves the authored night variant. The new protocol rejects older snapshots.

Remaining Step 2a: reconcile campaign Night 2's older timer outage with accepted fuel-exhaustion behavior; rerun timing bot and graphics Nights 1–3. The previously enabled campaign is a preview, not a completed five-night game. Ships/doors/mirror/horror are not implemented. No new M3 APK is delivered by this merge.
