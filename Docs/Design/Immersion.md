# S4-M3 — Immersion and clarity

Target: a tense, near-horror storm thriller played with a friend, with practical recovery moments. Half-Life: Alyx, Lethal Company and Alien: Isolation are mood references, not sources of assets/audio. The lighthouse teaches through objects and consequences.

## Intentional design — professor’s five asks

1. Add a brief Design section to the README describing considered choices.
2. Explain the first scene: the keeper lobby establishes the role and gives a clear solo/crew choice.
3. Explain what comes next: arrive in the House, hear the harbor master, read tonight’s jobs, pick up the wrench.
4. Describe direct versus distance interaction and deliberate tuning: physical tasks use direct grab/use, forgiving wrench snapping, persistent repair progress; rays remain for intentional menus.
5. Explain embodied hands: preserve the current tan hands, orientation and finger blending; held tools use existing grip poses. William owns body/avatar work.

## World-only guidance

No floating panels, screen HUD, airborne progress bars, glowing arrows or diagnostic text in the Lab. Mount instructions onto objects as plates, stencils, chalk/paint, engravings, gauges, tally counters and lamps. Keep the physical House chalkboard. Lobby UI is allowed. Watch Menu appears only deliberately, as a weathered logbook. Win/loss/sleep use a physical logbook page during a comfort fade; no forced camera movement.

## Core loop and feedback

Hear/locate the fault → read the equipment → repair/allocate power → observe lower pressure/water and restored light → finish tonight’s jobs → both keepers sleep → next night. Chalkboard goal: “Keep the light. Survive till dawn.” Small loss rule: “Water too high or light dark too long = lost.”

- House wall radio plus quieter belt receiver: short reactive harbor-master lines, 45 s idle hint; host decides shared cues. Original offline TTS placeholders remain plain so replacement recordings receive in-engine 300–3400 Hz filtering, mild distortion, static/crackle and squelch.
- Broken equipment emits locatable hiss/cough/static/whine and subtle practical-light flicker; fixed equipment stops the fault noise and becomes steady. Keep working engine ambience separate from fault sounds. Guide stair lighting remains readable and comfort-safe.
- Generator brass plate: “1 FUEL  2 PRIME ×3  3 CHOKE ▲  4 PULL ×3  5 CHOKE ▼”. Five lamps and primer/starter tally wheels. No “engine flooded” or extra-pull penalty.
- Breaker labels, needle and lamps remain on its cabinet; radio steps become painted labels/lamps; beacon wheel gets an engraved alignment marker.
- Pipe: one active chalk-lettered A/B/C coupling on top of a wall-mounted run. Grip holds the wrench; trigger locks its jaw within about 8 cm (clunk/haptic). A short arm pull around the upright bolt tightens it; wrist roll does not. Release trigger to readjust, then lock and pull again. Three tunable 30-degree clicks per bolt, earned across any number of smaller pulls. No hold-steady variant, reverse loosening or progress loss on separation. The bolt visibly turns/sinks, spray weakens and pressure falls. Preserve base hand rotation; releasing grip drops the wrench under host-owned physics.
- Chalkboard has chalk writing/ticks, tonight’s jobs and the goal/loss rule. No detached instruction canvas.
- Start: gentle lightning/thunder, NIGHT chalk and radio. Complete: radio + warm bunk lamp. Each keeper’s lamp shows their sleep vote. Early sleep gets a spoken hint.
- Sleep: about four seconds, fade/storm, ink logbook “NIGHT 1 — SURVIVED”, dawn, radio. Both keepers share the transition. Loss: power/roar/heartbeat, fade, physical logbook cause and TRY THE NIGHT AGAIN / RETURN TO LOBBY actions.
- Atmosphere uses wind, metal creak, distant foghorn, drips, gentle window lightning and thunder haptics. No strobe, camera shake or full art overhaul. Stations and stairs must remain findable.

## Architecture and ownership

New behavior is switched by an immersion tuning asset. Reuse existing XR Interaction Toolkit, grab ownership, host-owned day/station state, crew commands, Photon Voice, power grid and sounds. New replicated cue/state fields use that same authoritative path; show NETWORK VERSION only in lobby and bump the protocol if its payload changes. Solo must stay offline. Do not edit William Qian’s avatars/body or flood WATER VISUALS (renderers, surface, water shader/materials). Flood gameplay rules/tuning are permitted. Do not change hands, controller alignment, XR Origin or voice transport.

## Verification and cost

Before gameplay edits: fresh original solo Days 1–3 and Mac-host + Quest-guest join, avatars, two-way voice, shared task and shared bed. Save to `artifacts/immersion/before/`; after repeats under `after/`. Inventory all visible UI before replacement. Capture and inspect House radio, generator plate/tallies, both hands snapping/turning bolts, chalkboard, bunk lamp, win/lose logbook, blackout stairs and lobby network version. Test invalid/repeated inputs, early bed, fail/retry, late join and synchronized cues. Build/install the final APK, push `s4/immersion`, open draft PR, never merge it.

Prefer shared materials and bounded/poolable sound/feedback, no per-frame allocations. Report measured Quest performance separately from Editor behavior. Claim only tests actually performed; list unverified headset comfort/readability and any baseline failures plainly.
