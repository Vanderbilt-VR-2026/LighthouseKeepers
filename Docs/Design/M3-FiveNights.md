# Five nights and the ship

> Water’s rising, the power’s out, your partner is shouting the code up the stairs, you get the beacon lit with seconds to spare — and you watch the ship’s lights slide safely past the rocks.

Players lose because they were slow or careless, never because they were confused. Each night teaches or twists one new thing. One current instruction, physical controls, clear cause and effect, and quiet recovery moments remain the foundation. The ship makes the lighthouse’s purpose visible.

This is the next milestone, separate from PR #85’s merge candidate. Baseline and implementation status must be recorded honestly; the following graphs describe intended tension, not measured timing.

## Tension graphs

Read left to right: arrival → first problem → recovery → escalation → passage → dawn. Height indicates relative pressure, from 1 to 5. Night 1’s pressure is theatrical; it cannot fail.

| Night | Intended tension | New lesson | Payoff |
|---|---|---|---|
| 1 · The Leak | ▁▂▃▁▂▃▁ | Grip, ratchet, then rest | A safe preview ship shows why the light matters |
| 2 · Blackout | ▁▂▄▂▃▄▁ | Restore power; read the generator information together | First passage whose safety depends on the crew |
| 3 · The Warning | ▁▃▂▄▂▅▁ | Relay the radio code and warn a ship | Two passages; the broadcast extends the second window |
| 4 · Everything Breaks | ▂▄▂▄▃▅▁ | Maintain fuel, allocate power, clean the lens | Closely spaced ships reward anticipation |
| 5 · The Long Night | ▂▃▂▄▅█▁ | Combine learned jobs under one final crisis | Large ship survives, then dawn and rescue |

## Rules for implementation

- Extend the existing day assets and host-owned watch, rather than building a second game loop. Night schedules, ships, split-information roles, grace and solo adjustments are data.
- Preserve accepted Nights 1–3 until replacement behavior passes the recorded baseline. Do not substitute faster test controls for the real grip, valve, starter, radio or shared-bed interactions.
- Calm opening 30–60 seconds; 10–20 seconds of storm-only breathing room after fixes. Only the finale intentionally stacks faults inside ten seconds.
- Ships are visible and audible before their danger window. Beacon loss is fatal only during or approaching an active passage; outside that period it is a warning. Every loss names its cause and supports retry.
- A completed job is not the same as surviving the watch. Morning reports and the sleep gate must account for scheduled passages and the dawn transition, with an explicit next instruction while waiting.
- Solo retains local information and repairs directly with the wrench; the removed clamp must not return. Crew information is deliberately separated, with each keeper told their own role. Existing handheld radios provide communication across floors.
- Will owns avatar meshes/rigging and the sky. Reuse his appearance prefabs and existing poses; do not alter his art. A mirror is optional and must earn its device GPU cost; no unprofiled continuous reflection camera.
- Doors should respond to the hand/forearm or a physical handle, with bounded hinges and host-authoritative motion. They must preserve tutorial access, clear routes, rain zones and rejoin state.
- Ship art uses emissive windows/mast lights, bounded smoke/spray, a mesh beam and a distant light-cluster LOD. No extra realtime ship lights.
- Keep mono capture, echo cancellation, proximity/PTT radio voice, recorded dialogue and timed captions. P19–P25 use existing imported recordings; P26 stays unused unless a real partner-danger state is implemented.
- Any new snapshot fields require a new lobby network version and matching APKs. No main merges without the lead’s approval.

## Measurement and delivery gates

Do not claim timing targets from guessed durations. Record per-step action and travel times for instruction-following solo and two-player bots, then tune remaining time to 25–35% for Nights 1–3 and 15–25% for Nights 4–5. Mark these as automated timing rather than human usability acceptance. The demo is a data-defined six-minute watch with its ship moment around minute five.

Test ship pass/wreck, retries, persistence/continue, shared progression, both valve roles, split information, subtitles, shared bed and rejoin. Inspect screenshots of every opening, ships/horizon/rocks, flare, dawn/rescue, morning reports and demo. On Quest, measure the ship in peak storm against 72 Hz and GPU ≤11 ms; an Editor capture is not device performance evidence.
