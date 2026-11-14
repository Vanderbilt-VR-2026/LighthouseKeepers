# Sprint 5 — keep the light, survive together

Design skeleton, step 1.1 / issue #91. Authority: [Sprint5-Master.md](Sprint5-Master.md). This document describes the destination; implementation evidence lives in [Next.md](Next.md).

Two keepers maintain a storm-battered lighthouse while something from the sea attacks its machinery. Fixing a fault restores a practical light, gives a moment to breathe, and protects the next ship. The payoff is watching a named ship pass the rocks because the keepers restored the beacon in time. Solo uses the same machines with information close to the operator and no second-person valve requirement.

## The repeated loop

Hear a fault → find its source → see one needed control → perform one physical action → see and hear recovery. An objective names the object, place and action. B recalls it and the last harbor message. Never make a player lose because a hidden prerequisite or unreadable instruction prevented action.

The host owns night scheduling, tasks, water, power and outcomes. A single TensionDirector will arbitrate CALM → DREAD → SABOTAGE → HUNT → RELIEF. It reserves at least 15 seconds between new problems, 10–20 seconds after a fix, and a two-to-three-second quiet lead-in before a major scare. No scare in the first minute; no hunt during a first-time lesson. Presentation consumes that decision rather than running competing timers.

## Night arc (target, not completed acceptance)

| Night | Learn or twist | Pressure curve | Payoff |
|---|---|---|---|
| 1 — The Leak | Wrench, recovery, sleep; no timer loss | calm → pipe → relief → safe preview | A ship shows why the light matters |
| 2 — Blackout | Fuel failure, generator, scarce lantern | calm → darkness → restart → ship → relief | First real protected passage |
| 3 — The Warning | Radio/beacon and hiding; split information | pipe → restart → upper jobs → hunt/ship → relief | Partners relay instructions under pressure |
| 4 — Everything Breaks | Circuit choice, repeat sabotage, lens cleaning | spaced attacks → recovery → close ships | Tools buy time; choices matter |
| 5 — The Long Night | Known verbs under the final storm | breathers → rising attacks → final ship climax → dawn | Rescue and a shared logbook |
| Demo | Same data-driven verbs, six minutes | short lesson → blackout → brief hunt → ship | Judges reach the reason to care |

Keep existing Night 1–3 accepted actions and Night 3 timing until measured replacements pass. Current scheduling/task models do not prove ship-inclusive timing, horror fairness or headset acceptance. Each new state field changes the lobby network version; both clients must update.

## Responsibility and reuse

| Concern | Existing source to extend | Boundary |
|---|---|---|
| Host simulation | WatchState, NightScenario and existing crew snapshot | One authoritative clock/state; guests send actions |
| Guidance | ObjectiveDirector, KeeperStrings, HarborRadio | One current step; no independent instruction panels |
| Storm/music | StormDirector, adaptive music, rain zones | Tension decides when; these systems render/sound it |
| Physical tasks | Existing pipe, generator, power panel, beacon, radio | Preserve accepted grip, pull and pour verbs |
| Voice and avatars | CrewVoiceSession, handheld radios, Will's avatar | Preserve mono/AEC, ownership and current body |
| New gameplay | Ship, sabotage, oil lantern, Drowned | Feature switches; tests before enabling |
| Presentation | URP QuestPipeline, baked variants, shared materials | §14 quality baseline; no skybox/body redesign |

## Fairness, comfort and delivery

A dark room invites danger; light and a reachable hiding place provide answers. A chase must be telegraphed and escapable within the specified sprint/hide route budget. Fear uses breath, heartbeat and optional haptics, never camera motion, a vignette or FOV changes. Scare Off removes the entity; Low retains apparitions only. Critical audio has a visible cue and colours are backed by shape/motion.

Quest target: 72 Hz, GPU ≤11 ms, CPU main ≤9 ms, ≤24 audio voices. Two shadowless oil-lantern lights are the only realtime-light exception. Render scale stays at the documented baseline until device measurements justify a change. No Editor FPS claim substitutes for Quest profiling.

[FeelChecklist.md](FeelChecklist.md) records every verb's five beats. [SoundMap.md](SoundMap.md) maps events to sources/buses. [QuestQuality.md](QuestQuality.md) records settings and pending measurements. Evidence levels remain separate: unit tests, graphics Play Mode, live transport, headset function, and measured human comfort/performance.
