# Oil lantern story checkpoint — November16

**WORKS in Editor:** two grip-held lanterns, OFF/LOW/HIGH wick, oil depletion, actual tilted-can refill, supported station hooks, gravity/recovery, watch oil bar, host replication. Prior accepted body/hands are untouched. Seven hooks: two House, pipe, generator, panel, radio, beacon. Original mesh under2,000 triangles per lamp including fittings; two lights maximum, shadowless. GPU cost is **not measured**.

**67 focused EditMode cases pass:** lantern7, Drowned10, night/runtime22, voice23, story-import5. One PlayMode budget/ignored-player-collision test passes. This is a focused run, not the full historical suites. Night/runtime includes all five authored arrivals, solo/crew variants, Night1 no-fail, new fuel/wick late-join/retry and invalid snapshot rejection.

**Actual XRI physical checks:** both hands × both lanterns (LOW/HIGH, hook, release gravity, unreachable recovery), plus held and tilted oil-can refill:5cases pass. Fresh solo Night1 uses lobby offline-test fallback, grip-only pipe, preview ship, shared-bed path into Night2; zero logged errors. The new optional lantern is hidden in Night1.

**Live two-Mac:** join, visible remote body, crouch/wrist poses, paper lease/drop, shared guest pipe, preview ship and shared sleep into Night2; then unleased wick request rejected, guest+host both hold HIGH, shared fuel and two-light cap agree, host hook/drop/20s recovery reaches guest. New story caption transport uses a silent temporary fixture, not a new actor recording. Human speech/echo/comfort is NOT VERIFIED; Editors muted. Night3 valve/radio gameplay was not replayed in this checkpoint.

**Failures found and corrected:** old render guard disabled lanterns; corrected whitelist + total2-light budget and regression. Oil can moved clear of engine log; radio hook moved clear of console; supply can ignores player capsule. First crew attempt ended at setup because the peer Editor process exited with its parent shell; restarted detached, complete retry passed. No gameplay changed to hide a test setup problem.

Screenshots inspected personally: House lanterns, globe/flame, oil shelf, both supported F4 hooks. Last follow-up darkens the oil-can handle to match its tank and ignores player collision. Desktop images do not certify stereo readability.

**NOT VERIFIED / unfinished:** worn Quest72Hz/GPU<=11ms/CPU<=9ms, hand comfort/seated reach/true pendulum motion, human voice listening, actual HIGH defense usability and oil balance. Core tests cover2s uninterrupted HIGH and15%cost/20s retreat; the runtime connects it to the existing non-contact apparition. LOW slowdown, protected catch rules, navigating hunter/hiding, Owen and complete story are future work. New actor files remain absent; legacy voices continue.

Network **LK-S5-LANTERN1 / LK-Lab-Coop-v35**. Both clients must update. See `install.json` when packaging completes. Never auto-launch the headset.

Repro: `artifacts/s5/lantern/final-check.py`, `crew.py` (new output directory on rerun), `Physical.cs`, `Review.cs`; focused bridge test filters listed above. Peer previous work is backed up under `artifacts/s5/lantern/peer-backup/`; do not reset it blindly.
