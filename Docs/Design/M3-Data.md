# A1: authored night data

The five campaign `DayDefinition` assets and demo are in `Assets/Data/M3Days/`. `M3Campaign.asset` references them. Its enable switch is **off**. The current watch still loads its unchanged catalog: this checkpoint does not yet make five nights playable or spawn ships.

Each day has an optional `NightScenario`: stable ID/name key, solo/crew duration and flood modifiers, no-fail/grace/pacing, storm stage/peak fraction, active station steps and information sites, fault events with conditions, ship approach/window/grace/preview/finale data, opening/tonight cues and event cues. Existing day consumers retain their behavior. The new model has no transport dependency or replicated state; network version remains LK-S5-VOICE1 / v19 until runtime state is added.

`Lighthouse Keepers/M3/Create missing night data` creates missing assets without overwriting existing day edits. The original catalogs are untouched. Titles and ship names are in KeeperStrings. Demo is a separate reference using the same types, not a sixth progression night.

## Timing status — not an M3 balance claim

| Data | Solo seconds | Crew seconds | Basis |
|---|---:|---:|---|
| Night 1 | 315 | 315 | Existing base duration; no-fail flag |
| Night 2 | 315 | 315 | Existing base duration |
| Night 3 | 450 | 330 | Existing accepted Day 3 tuning |
| Nights 4–5 | 450 | 450 | Temporary inherited all-jobs solo envelope; NOT measured for these nights |
| Demo | 360 | 360 | User’s six-minute requirement; NOT measured |

Every profile has `timingApproved = false`. Ship slots are authoring examples that fit these envelopes, not measured first-timer pacing. A4 must measure travel/actions, and passage timing must be rechecked with B2 before enabling the campaign. Do not label these prototypes tuned or publish them as completed nights.

## Remaining integration

A2/A3 must consume the schedules through the existing host-owned watch and Objective Director, replace night-number branching for campaign mode, preserve legacy behavior when disabled, and synchronize progress with a version bump. A4 records real bot timings. B supplies actual ship rules/art; C–G supply the finale, progression, split information, sound and delivery checks. Doors and any budget-approved mirror remain separate pending work.

Verification: three focused tests pass for all six assets, unchanged legacy catalogs, and rejection of invalid passage/dependency data. Unity compilation passed in the second Editor. Base solo/co-op regression evidence is recorded under `artifacts/m3/before/`; no claim of five-night runtime execution is made.

## A2/A3 runtime checkpoint

`NightSchedule` now drives the existing host WatchState when a configured night is selected. The normal catalog stays selected while M3Campaign is disabled. Events are one-shot, future jobs cannot be ticked or slept past, the opening/recovery intervals defer new faults, and only the explicitly authored finale permits coincident repeat failures. Upper jobs wait for an actual outage and settled generator restart. A low-fuel event reduces fuel without resetting the running engine; its objective remains Fill until the authored refill threshold. Retry clears schedule state.

Schedule mask, witnessed outage, relief, next-fault boundary and event grace are replicated in snapshot v20, matchmaking **LK-M3-NIGHTS1**. Remote clients cannot advance the schedule. The old three-day stages remain used only by legacy assets. No campaign-enabling, ship or five-night completion claim yet.

Unity compiled; 16 M3, six existing Day3 balance and 23 voice tests pass. These are rule/serialization checks, not physical interaction or transport evidence. Results: `Docs/Verification/M3/scheduling/`. Next gate: instruction-only bot measurements and graphics regression.
