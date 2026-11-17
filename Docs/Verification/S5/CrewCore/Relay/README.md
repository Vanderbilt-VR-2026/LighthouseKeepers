# Telegram relay — November 17

Two humans receive a House telegram and F4 radio input. The Lightkeeper operates; the Machinist reads. Other tools remain usable. Code symbols have both numbers and colours, vary by night and survive retry/rejoin. The HUD and glow no longer give away the answer. Alone, the clue stays beside the console until a real companion exists. Existing GUIDs and button controls are retained.

Network: **LK-S5-RELAY1 / LK-Lab-Coop-v37**. `ReadCode` extends the replicated instruction enum; the existing keeper count now restores relay policy on a guest. Both players need the new build.

## Evidence

- Four new focused relay tests pass: duty assignment/no answer leakage; stable retry/replica code; wrong-symbol recovery preserving tuning; leave-mid-code policy.
- Connected tasks 2/2, M3 runtime 22/22, crew duties 8/8 and two focused instruction checks pass.
- Actual two-Editor test passes: join, correct duties, shared clue, guest XRI wrong/correct code presses, actual leave/rejoin preserving the partial code and duty, guest leased microphone completing the shared broadcast. Earlier tasks were restored on the host to isolate the relay: this is not a full physical Night 3 run.
- Both Editors muted. Human voice clarity, headset readability/feel and GPU cost are NOT VERIFIED.
- Final three screenshots were inspected. The first House paper was too large and overlapped the wall radio; corrected to a smaller unlit paper beneath the PA button. The network run predates this cosmetic placement change; final placement was checked in separate Play Mode.

## Timing-model limits

`M3CampaignBot` is a rules simulation with assumed movement/reading/gesture durations. It is neither a navigating companion nor a human measurement. It now supplies the lobby duties used in the actual game; the previous fixture supplied neither. It reads the relay only after modeled travel to the House and waits three seconds before the operator can use it.

| Scenario | Solo | Duo |
| --- | --- | --- |
| Night 1 | pass, 245.1 s | pass, 245.1 s |
| Night 2 | pass, 369.1 s | pass, 305.1 s |
| Night 3 | pass, 562.6 s | pass, 558.5 s; 66.5 s spare |
| Night 4 | pass, 607.6 s | FAIL: first ship wreck, 445.0 s |
| Night 5 | pass, 761.0 s | FAIL: flood, 330.2 s |
| Demo | FAIL: timeout, 400 s | pass, 330.1 s |

A controlled run temporarily disabled only the relay data in memory, restored it in `finally`, and saved no assets. Night 4–5 duo and solo Demo fail at exactly the same times without the relay. Night 3 duo without relay takes 521.0 s. No gameplay timer was changed to make a test pass. The remaining timing failures need investigation before campaign-wide acceptance.

## Reproduce

In the muted root Editor, run the four `RadioRelayTests`; `artifacts/s5/crew-core/check.py` drives the local Pipeline bridge. `relay-crew.py` runs the two-Editor fixture; `relay-review.py` makes the visual captures; `timing.py` and `timing-comparison.py` produce the model reports. Helpers are ignored local artifacts. Relevant committed code: `RadioRelayTests`, `M3CampaignBot`, `CodeSource`, `ObjectiveDirector`.
