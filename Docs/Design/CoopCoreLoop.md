# Two keepers, one watch — core gameplay first

November 17, 2026. Latest user direction supersedes the hunter-first ordering and the decision to defer every solo companion. This is the **next playable target**, not a claim that roles, a companion or revive are already in the installed build. Keep the accepted body, physical tools, lanterns, objective recall and co-op transport.

## The promise

> You know something your partner needs. They can do something you need. The storm splits your attention; the thing in the dark tries to split you apart.

The player should enjoy fixing the lighthouse with a friend before the Drowned is enabled. Test that first. Each repair should have a legible physical action, a useful callout, visible cause and effect, and a recovery if someone gets it wrong. Repeated errands, memorizing a script and waiting helplessly are not teamwork.

## Two duties, chosen in the lobby

Both characters are keepers. **Lightkeeper** and **Machinist** are duties, not combat classes or senior/junior ranks.

| Duty | Lobby description | Leads | Helps the other keeper |
| --- | --- | --- | --- |
| Lightkeeper | Ships, beacon and radio | Find approaching ships, aim the beam, operate the radio | Read the engine indication; hold the pressure valve; carry light during a repair |
| Machinist | Engine, pumps and repairs | Fuel/start the generator, repair the pipe, allocate circuits | Read the telegram and bearing; keep power on the beacon; call warnings from below |

Use the existing PLAY → HOST / PLAY ALONE / JOIN CREW flow. Add two clear duty cards to the crew screen, with the selected keeper name on each. Host chooses either; a joining player takes the vacant duty and can request a swap before START. START with one human creates the scripted partner in the other duty, online or fully offline. Never imply that an invisible bot is ready. Show its name and duty only once the companion can actually spawn and help.

Duties change default objectives and information responsibilities, **not access to tools**. Both can refuel, wrench, use the lantern, aim the beam, revive and sleep. A player can cover for a partner or abandon the plan in an emergency. Assignment should follow a player who takes over; never keep nagging them to leave a job they are doing. Both see the same jobs board, their own current instruction and a short partner activity. Change duties between watches; avoid mid-grab reassignment.

## Make communication necessary, but easy

Deliver three pair interactions before expanding across all five nights. Teach each physical control locally before adding its information split. A first-time player must not need another person to explain which button grips a tool.

| Pair interaction | What one player knows/does | What the other knows/does | Recovery / fairness |
| --- | --- | --- | --- |
| Pipe / pressure | Lightkeeper holds the adjacent valve and reads the pressure response. | Machinist sees active bolt, grips wrench and pulls to tighten. | Release surges pressure and pauses progress; completed work stays. Neither player just waits for a long bar. |
| Engine / power panel | Lightkeeper reads the panel's starting indication and sees whether the circuit receives power. | Machinist fuels and operates the physical starter. The requested replacement is three timed starter pulls (#137). | Keep the existing sequence working until the timed-starter replacement passes. The observer's information must affect a real decision; do not invent a remote gauge that only repeats a fixed HUD instruction. Introduce one variable at a time, with feedback and no instant punishment for a wrong call. |
| Ship / radio | Lightkeeper sees the ship, keeps the beam on it and sends the report. | Machinist reads the House telegram and repeater, reports code/bearing and balances pump versus beacon power. | Clear numbers/symbols, repeatable information, handheld radios that work across floors. An off-target beam gives a warning and recoverable window. A fixed compass call must match the actual sea path. |

The telegram is the first information split: the House holds the code, F4 holds the input. **The operator HUD and harbor master must not print/speak the answer.** They may say where the clue is and whom to ask. Keep code clues in the world for both humans to inspect; no magical role-only invisibility. Vary the short code between watches, hold it stable for retries, and use symbols as well as color.

The beacon should remain a satisfying visible aiming task. A bearing call helps find the ship; do not make it impossible to aim without continuously reading a remote number. Assess actual wheel/ship visibility before adding more required inputs.

## A useful, limited solo partner

Bring back **a simple scripted second keeper** now. The elaborate Owen story/animation system is still stretch. The bot fills the other selected duty and can be called Owen without a new exposition sequence.

- Same stations, clues, pressures and recovery rules as two humans. Solo may have measured timing headroom for communication/navigation; no more duplicating all clues beside the operator or reintroducing the solo clamp.
- A small command set: **Follow**, **Wait**, **Help here**, **Work your duty**. Reuse the Watch Menu initially; audit controller bindings before adding a point-and-button shortcut. A is already reserved by proposed hiding/breath interactions.
- Bounded behavior: walk a verified route; read a real clue and show it on a readable slate; take a valid support position; hold the valve; operate a requested circuit; bring light; attempt a rescue. Add timed engine/ship assistance only after these work. Do not build conversational AI, speech recognition or a generalized physics agent.
- Reads information only after reaching its source. It can repeat the last code/bearing on its slate. Confirm commands with a nod, a point and a short status. It never silently knows a code from across the building.
- Does not automatically solve every main task, instantly finish a station, teleport through walls or block a doorway. The human can always take over. If a route fails, it stops safely, tells the player and remains recallable; no remote completion to hide a path bug.
- When the human chooses either duty, the other duty must provide useful help throughout the test. Test both choices; do not label a pipe-only bot a completed solo campaign.
- No escort failure condition. The bot is not another fragile person the solo player must babysit. Any protection it provides requires actual position/line of sight, not a global safety flag.
- Human joins: release bot holds and retire it before activating that person's station authority. Human leaves: preserve progress and leased tools; replace the vacant duty only after roster/ownership cleanup. One owner per station action, never a bot plus a human simultaneously. Late join, host loss and rejoin require explicit regression checks.
- Remain fully offline. Never require a cloud AI service, microphone input or a network session to play alone.

## Recovery before lethal horror

Use a common **downed → rescue → recovered** flow for human/human and human/bot. Wire it into the real transport before enabling catches.

1. A fully telegraphed catch fades and muffles sound without pulling or rotating the camera. Existing plan returns the downed keeper to a reachable F1 recovery spot; keep the hand accessible above the water and out of floor geometry.
2. Partner receives one clear location and a world signal. A starting rescue window is 40 seconds, already present in `HorrorTuning`; time real paths from F4 and increase it if the route plus interaction cannot fit fairly.
3. Reach the keeper and **hold grip on their offered hand** for a short tuned interval (start at three seconds). World progress, sound and haptics confirm the rescue. Releasing pauses the attempt instead of silently losing the entire rescue.
4. Recovery grants a short safety interval; never immediately catch the same player again. Keep completed repairs. Both downed, or an expired rescue, ends the watch with an explicit cause and retry.
5. In solo, the bot prioritizes the rescue over work, uses the same reachable route and hand interaction, and does not resurrect the player remotely. Do not keep the old separate solo instant-respawn/catch-count rule once the companion recovery is enabled. Until then, do not enable new lethal catches at all.

Lantern, hiding, sprint, bell and flare remain escape options. The Drowned only takes a keeper who is in darkness **and unwitnessed by the partner**, human or bot. A partner's presence in another room is not line of sight. Room light and the HIGH lantern remain useful even when the pair splits up.

## One enemy, expressed as storm and Drowned

The storm creates water, exposure and poor visibility. The Drowned exploits it: drained fuel, torn cables, handprints on glass. One host encounter scheduler owns both pressure and relief; two unrelated timers must not sabotage or hunt at once accidentally.

- **Work:** useful repair/communication in warm light.
- **Tell:** wet steps or dragging at the future fault, lamp dip, machinery strain. Players can point to the source.
- **Break:** one physical cause, audible failure, actual circuit/station consequences, one short what/where announcement.
- **Decision:** one player can stabilize water/power while the other investigates, or both can take the lantern and stay safe. Existing faults can coexist; it is not a compulsory single-objective conveyor belt.
- **Threat:** working hands versus watching the dark. Telegraph before pursuit. Recovery and defenses work before damage is enabled.
- **Relief:** repair produces a clear clunk, returning light, pump/engine response and a genuine 10–20 second breather.
- **Payoff:** keep the ship lit while a fault threatens its power. Passing horn and calmer storm reward coordination.

Preserve at least 15 seconds between new problems, first-control tutorial grace and no scare in the first 60 seconds. Never fake intensity by speeding rain, waves or cloud animation. The storm and entity share a pressure budget; don't make the clock, flood, ship and hunter four independent punishments.

## Surface story, not homework

**Saltmere depends on this light to guide ships past the Kettles. The last pair vanished; you are the relief crew for five storm nights. Whatever comes with the storm attacks the machinery and waits for a keeper alone in the dark.**

That is enough to play. Elias teaches the job and warns the crew, then leaves room for conversation. A brief arrival can communicate the job, the pair rule and the approaching storm. No required confession chain, new cutscene or lore quiz. Keep the four existing notes and six visual clues as optional rewards. Owen's deafness/backstory, the full historical reveal and extra recordings cannot block the prototype.

Keep the 52-line voice selection and staleness/priority/spacing rules. Do not enable companion-dependent lines before the bot exists. Don't ask for a new large recording session until the short crew loop survives a human playtest.

## Implementation order and acceptance gate

The next target is one **six-to-eight-minute crew prototype**, using existing spaces and controls. This is a focused test scenario, not a replacement for the committed six-minute Demo contract or all five nights.

1. [#136] Host-authoritative duties in the lobby, stable through START/rejoin; per-player objectives follow them without hard-locking controls. New replicated fields require a network version bump.
2. Actual telegram information split, current valve puzzle and useful engine/panel coordination. Remove HUD/voice answer leakage; two players can explain what each needs from the other.
3. Minimal solo partner completes the same support interactions for either human duty. Offline run, doors/stairs, command cancellation, late join/leave and ownership transitions must pass before calling solo parity complete.
4. Downed/rescue with a human or the bot. Measure the worst station-to-F1 rescue route; confirm interruption, timeout, both down and retry. Then connect the already-tested hunter foundation to the live world.
5. Add one warned blackout/hunt and one ship event to the crew test. Only expand pacing across Nights 2–5 after this is fun and recoverable in headsets.

Record paired human playtest data: time until first useful callout; time spent understanding versus physically operating; repeated help prompts; idle time by duty; failures caused by confusion; successful rescues; moments where players chose together versus apart. Ask each player to describe what their partner contributed. The prototype fails if one person does all meaningful work, if silence is as effective as communication, or if the bot feels like a remote cheat.

### Current implementation audit (November 17)

| Area | Actual status | Evidence / relevant source |
| --- | --- | --- |
| Valve + wrench | Live two-person hold/operate gate; accepted physical wrench. | `WatchState.Connected.cs`, `WatchDemoCoop.cs`, earlier connected/co-op evidence. |
| Role selection | Implemented: duties, mutual swaps, host assignment, start/rejoin, soft objective preferences. Live two-Editor tests pass; worn acceptance pending. | `LobbyPlayer.cs`, `FusionLobbyState.cs`, `CrewLobbyBoard.cs`. |
| Split clues | Radio telegram relay implemented and transport-tested; numbered buttons, no answer in HUD/glow. Engine/panel and bearing relay remain pending. | `NightScenario.cs`, `WatchState.Campaign.cs`, `ObjectiveDirector.Instruction`. |
| Solo companion | No runtime companion. | Latest `Next.md`, story trigger holds; #135. |
| Revive | A 40-second tuning value exists, but no live downed/hand-revive flow. | `HorrorTuning.cs`, current `WatchSnapshot` and command adapter. |
| Hunter | Tested isolated FSM, baked routes and rig; not attached to gameplay or replicated. | `Docs/Verification/S5/HunterFoundation/`. |
| Lantern | Existing shared oil/wick/hooks/refill and apparition stun; worn acceptance still pending. | `Docs/Verification/S5/Lantern/`. |

Current source: LK-S5-RELAY1 / v37. Evidence in `Docs/Verification/S5/CrewCore/`. Installed binary is tracked separately in Next.md; no headset acceptance is inferred from Editor tests. Voice recordings are last; all guidance is caption-only while mechanics are changing. User requested timed generator pulls in #137; add interaction feedback everywhere, while keeping spoken guidance selective to avoid the previous narration spam.
