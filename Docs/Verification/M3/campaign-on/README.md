# Five-night preview enabled / subtle wiring

The lead explicitly requested enabling the current five-night campaign. PLAY SOLO and crew now select Night01–05 from M3Campaign. This exposes the implemented staggered task schedules, not the unfinished ship encounters, rescue ending, saved progression, physical doors or mirror. `timingApproved` stays false. The original three-night assets remain intact as the switch-off fallback.

Wiring uses an original procedurally authored atlas: aged warm metal with grime/scratches, cloth-braid detail on short drops. Main conduit radii are 15% slimmer. Persistent emission drops from .025 to .004; restoration pulse strength .65 to .22. Paths, junction positions, circuit semantics, triangles and renderer count are unchanged. No new textures sampled per fragment, lights, colliders or synchronized fields. Regenerating saved waypoints preserves the finish.

Network **LK-M3-CAMPAIGN1 / snapshot v20**: matchmaking label changes to prevent enabled-campaign clients joining earlier three-night clients with the same wire format. Both players need this build.

## Executed

- Focused: 16 M3, 3 wiring, 23 voice/version checks pass (42 total).
- All 12 instruction-only solo/duo task simulations pass. Existing action-budget model, not human or ship-inclusive timing. See timing.json and Docs/Playtests/telemetry-m3.md.
- Actual Play Mode starts campaign Night 1. Graphics/XRI replay completes clamp, both-hand wrench repair, drop/recovery, drain, sleep and wake into campaign Night 2. Zero replay errors.
- Two-Mac live Fusion: join/ready, avatar visibility, mono/AEC/NS configuration, B recall/height, host clamp, guest three-bolt repair reflected on both boards, shared sleep into campaign Night 2. First attempt had an incorrect new assertion that both test profiles use the rigged avatar; corrected to the established profile-specific expectation and reran successfully. No avatar/gameplay change. Editors muted; no human audio-quality claim.
- Generated-route audit: no unexpected contacts. Powered, blackout, pulse, panel and pump screenshots inspected. Budget remains 14,634 triangles / five combined meshes.

## Pending

Ship-inclusive pacing and remaining M3 features. Quest comfort, finish/readability and sustained GPU cost; human audio and Mac+Quest acceptance. No full-suite rerun for this small activation/material pass; previous full-run evidence remains in scheduling/. Build/install receipts follow separately; never launch without the lead asking.
