# Two-mode verification

Implementation checkpoint: 55 focused EditMode tests passed (SurvivalMode 6, CampaignOrder 5, RadioRelay 4, ShipAiming 12, CrewCompanion 5, VoicePass 23). Three rendered PlayMode lobby tests passed, including actual tracked UI ray hits and the mode-choice flow. Evidence: `artifacts/s5/modes/tests/`.

Rules tests exercise all stations from Day 1, timed starter, paired information, only flood failure, retry, safe ship waiting then passage, day 6→7, increasing difficulty, Day 100 replica, rejected cross-mode snapshots, and unchanged Campaign tutorial/horror.

Pending at this checkpoint: live solo and two-Editor transport regression, screenshots, current-source APK, headset acceptance. Editor tests do not establish controller feel, audible multiplayer voice, readability or device performance.
