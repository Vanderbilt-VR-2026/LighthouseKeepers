# Phase 2.3 — ship-only beacon loss

83/83 focused EditMode cases pass. Muted graphics Play Mode passes with zero logged errors: dark before approach has no deadline; the near-rocks grace starts; restoring the light saves the ship; darkness after passage cannot cause a loss; ignoring the approach produces the named wreck and retry clears it. The controller clock was explicitly advanced for this rule check. It is not a human timing or headset test.

`Sprint5Tuning.shipApproachDangerSeconds = 15` starts exposure just before the physical window. Each ship still owns its authored continuous-dark grace. Night 1 safe previews never threaten a loss. The legacy deadline remains unchanged when ship passages are disabled. Snapshot structure is unchanged from 2.2: LK-S5-SHIPS1 / v25.

Reproduce: `caffeinate -i python3 artifacts/s5/ship/window-rules.py` and `python3 artifacts/remaining/m3/tests.py ship-window-loss 'ShipPassageTests;ShipWatchIntegrationTests;M3RuntimeTests;Day3BalanceTests;VoicePassTests;TensionDirectorTests;SteamerTests;QualityBaselineTests'`. Ship flags remain off until the phase gate; timing and the worn-headset check remain pending.
