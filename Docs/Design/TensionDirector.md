# One rhythm for the watch

Sprint 5 step 1.2, issue #92. `WatchState` owns one `TensionDirector` for the campaign. The existing `NightSchedule` still resolves the authored dependencies; its pacing gates now come from the director. The old `StormDirector` is a presentation façade for that same state. Adaptive music follows the director's requested layer at its existing bar boundaries. Rain, waves, lightning and lamp motion already read the storm stage. No sky or avatar assets changed.

`Assets/WatchDemo/Resources/Sprint5Tuning.asset` enables this foundation. Disabling `tensionDirector` restores the previous campaign pacing and weather/music decisions. Archived non-campaign catalogs retain their rules. The host chooses; guests only apply validated snapshots.

| Rule | Before | Sprint 5 |
| --- | --- | --- |
| New authored problems | 12 seconds apart | At least 15 seconds; explicit Night 5 climax can overlap |
| Recovery after a completed job | Schedule 15 s; storm separately 20 s | One authored interval clamped to 10–20 s; current nights use 15 s |
| Recovery presentation | Independent weather/music decisions | Storm one stage lower, music resolves at next bar, no new scheduled fault |
| Night 2 arrival fuel | Runs out near 75 s unless refilled | Same except if a fix's recovery overlaps it: consumption slows, then the gauge reaches E and the engine stops naturally |
| Scare preparation | No central permission | Minimum 60 s elapsed; no tutorial/grace; darkness and a reachable escape required |
| Big-scare lead-in | Not implemented | 2.5 s request/reservation; Music and Ambience duck 24 dB, lightning waits; launch rechecks safety |

The last rule is an implemented, tested integration point for later sabotage/entity work. **No entity, jump scare or chase has been added yet.** A fix cancels a pending scare. Player mistakes such as manually overloading the breaker are not suppressed by the authored-fault scheduler.

The phase is CALM, DREAD, SABOTAGE, HUNT or RELIEF; tension stays in 0–1. Storm stage and the target music layer are evaluated together. Blackout interior fog follows tension without changing the camera, FOV or skybox. Background narration waits during the scare preparation/hunt; real danger warnings retain priority. Mixer ducking never mutes crew voice or critical feedback.

## Transport

New protocol: **LK-S5-TENSION1 / LK-Lab-Coop-v23**. Both players must update. The snapshot includes the tension frame and actual engine consumption so the guest's fuel warning agrees during recovery. Existing schedule timing fields mirror the single director and contradictory snapshots are rejected. Retry starts a fresh frame; late join applies it without independently scheduling anything.

## Verification limits

Focused model/runtime/voice/audio regressions: 82/82 pass. One existing runtime assertion now checks that the radio remains pending at 12 seconds and opens at 15; this is the new required spacing. Task-only first-timer models pass all five nights plus Demo in solo and duo; they do not include ships or horror and are not final pacing acceptance. Full graphics and crew results are in `Docs/Verification/S5/Tension/` as they finish.

No connected Quest, no on-device GPU claim, no subjective headset acceptance. Later entity work must supply a real escape-route result to the permission API and demonstrate it in Play Mode; this foundation does not assert unbuilt hiding places exist.
