# Phase 2.2 — host-owned ship outcomes

72/72 focused EditMode cases pass: passage timing, brief darkness/recovery, continuous-dark wreck, safe preview, radio warning slowing the approach, multiple ships, invalid snapshot rejection, read-only guest state, shared completion/sleep, named loss and retry. Legacy task checks are included with the feature OFF.

Muted graphics Play Mode: PASS, zero logged errors. The inspection advances the real host model explicitly with an authored 75 s approach/45 s window and an 8 s test grace; it pauses the controller tick for reproducible captures. This is outcome/presentation verification, not a human timing run. P21/cue96 → P23/cue98 on pass; fresh retry → P22/cue97 → P24/cue99 on wreck. Captions use the existing recorded lines and word timings. Wreck interrupts the danger line deliberately. Reviewed safe passage, distress flare and named loss page. An initial image exposed the logbook's old generic reason; fixed the display and added an assertion against its real text.

All ship results are in the host snapshot; guests do not run timers. Protocol **LK-S5-SHIPS1 / LK-Lab-Coop-v25**. Sleep waits for authored ships as well as jobs. The warning broadcast buys extra approach time if sent before the danger window. No new co-op transport was introduced.

Feature remains OFF in normal play until 2.3 removes the legacy global beacon deadline and 2.4/phase-end runs finish. The 2.2 inspection still exposes the old countdown cue32 during the compressed wreck setup; the next step explicitly replaces that global deadline. Live two-Mac ship passage, human listening, Quest GPU/comfort and final ship-inclusive night timing are NOT VERIFIED.

Reproduce: `python3 artifacts/remaining/m3/tests.py ship-integration-fixed 'ShipPassageTests;ShipWatchIntegrationTests;M3RuntimeTests;VoicePassTests;TensionDirectorTests;SteamerTests;QualityBaselineTests'`; `caffeinate -i python3 artifacts/s5/ship/outcomes.py`.
