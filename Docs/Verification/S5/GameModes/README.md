# Two-mode verification

56 focused EditMode tests passed: SurvivalMode 7, CampaignOrder 5, RadioRelay 4, ShipAiming 12, CrewCompanion 5, VoicePass 23. Three rendered PlayMode lobby tests passed, including tracked UI ray hits and the mode-choice flow. Evidence: `artifacts/s5/modes/tests/`.

Rules tests cover all stations from Day 1, timed starter, paired information, flood-only failure, sea recovery, retry, safe ship waiting then passage, day 6→7, increasing difficulty, Day 100 replica, rejected cross-mode snapshots, correct mode string data, and unchanged Campaign tutorial/horror.

Live muted offline Survival passed: immediate briefing (three pages, no text overflow), Owen spawned in the other duty and began fuel work, pipe valve and code relay enabled, original music selected, no Drowned/possession component. Scripted station actions released and guided the convoy, then advanced Day 1→2. This is a runtime smoke, not human controller acceptance or a full autonomous Owen run. Evidence: `artifacts/s5/modes/solo/`.

Two live muted Editors passed: host Survival overrides guest Campaign selection; lantern state sync; shared preparation wait; radio clearance; guest leave/rejoin with state restored; guided ship pass; both players vote to sleep and advance to Day 2; retry reset. See `two-editor-result.json` and `artifacts/s5/modes/crew/events.jsonl`. Station actions were scripted; shared sleep used the actual vote/pose path. Audible voice and physical hand interactions were not re-certified.

A live scene caught a wrong field name in the new string entries; fixed before delivery, with a non-empty/formatted-string regression test. Early peer runs were restarted after an Editor domain reload interrupted UI listeners. The corrected solo and completed two-Editor runs have no new runtime exceptions after their watch-start markers.

Inspected images: `mode-selection.png`, `survival-briefing.png`, plus all three briefing pages in the solo artifacts. Worn stereo readability, difficulty, audio quality, Owen path/feel and sustained Quest CPU/GPU remain NOT VERIFIED.

APK **70977af6** built successfully (IL2CPP ARM64, normal package, VR manifest, valid signature and ZIP). See `delivery.json`. Quest returned and this APK was installed on request; installed SHA-256 matches. Not launched. Neither the Editor checks nor APK packaging are device performance evidence.
