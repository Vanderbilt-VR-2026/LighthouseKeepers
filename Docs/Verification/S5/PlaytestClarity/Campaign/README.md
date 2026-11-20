# Radio clearance and ordered watches — November 17

Nights 1–3 follow engine (when broken) → pipe → pump → radio clearance → beacon/ship → rest. Moving floors does not reshuffle preparation. Recovery/possession takes priority. Once a cleared ship needs guiding, its lightkeeper stays at the beam while the partner can repair. Manual out-of-order repairs remain possible. Nights 2–3 panel capacity is 3.25, enough for pump, beacon, radio and lights; no unexplained pump-shedding lesson during introduction.

Ships remain visible offshore until the actual radio transmission completes. Preparation time is added to the watch budget; physical fuel and flood still run. The first ship requires steering; it waits at the rocks when the beam misses and cannot wreck. This is the aiming tutorial, not the unfinished full-generator Night 1 rehearsal. Tutorial off-target delay also extends its budget. The first ship announcement waits for clearance. Night 2 summons acknowledges an already-restarted engine rather than claiming it is dead.

New replicated clearance/wait fields: **LK-S5-CLEARANCE1 / LK-Lab-Coop-v43**. Both clients must update. Guest snapshots validate timing and preserve host authority. Added regression for fractional clock accumulation discovered by integration tests. Legacy ungated ship fixtures remain covered; their Night 2 expected ship name corrected to Northern Star.

## Verification

- 88 focused checks passed; receipts in this directory.
- Actual muted Night 1 Play scene: repair, radio power/tune/code/transmit, hold offshore, miss aim without loss, guide, completion. State/control inputs automated, not a human physical-control test.
- Inspected both screenshots. First shows Mary Ellen waiting; second shows cleared sea after she has left this view. These are environment evidence, not headset HUD/comfort proof.
- Live two-Editor host/join, clearance replication, leave/rejoin, guided pass and retry PASS (`crew/`). The fixture initially tried to advance before the arrival acknowledgement released its hold; rerunning preparation after acknowledgement corrected the test setup. No human speech/readability/GPU claims.
- `timing.json` is the instruction-only rule simulation, with assumed travel and gesture durations. Nights 1–3 pass solo and duo. Their elapsed totals are approximately 299, 418, 508 seconds. These are not measured first-timer times; preparation is now player-paced.
- Nights 4–5 retain their prior authored schedule/objective policy. Their duo simulations and solo Demo do not currently pass; remain explicit unfinished timing work. Do not claim a verified complete five-night campaign from these results.

## Remaining from the headset feedback

#137 timed starter replacing primer/choke; full opposite-role automatic station solver; walkie belt carry; visible eye blood/beard-mouth correction; full Night 1 generator rehearsal. Current companion supports valve, fetched clues, following and rescue, not every station. Watch offset and wrist fit need worn acceptance. New hunter/hiding scenes are not enabled by this change.

Clean source f170da4e built and installed on Quest 3 without launch. Package/signature/ARM64/CRC checked; local and installed SHA-256 match. See ../Delivery and ../Headset-Check.md. No worn test was performed.
