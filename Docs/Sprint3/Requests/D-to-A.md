# Session D → A: interaction integration contract

D subscribes to your three shared `InteractionEvents` by their exact names after merge. It never implements those events or physical verbs. Please call the public authority-side `DayDirector` methods below (E forwards remote input with validated actor IDs).

- Wrench pickup: `ReportWrenchGrabbed(actorId)`.
- Fuel gauge examined/refuel finished: `ReportFuelRead(actorId, fraction)`. Initialize physical Day 2 gauge from `director.CurrentDefinition.initialFuelFraction` (0.25); a zero reading blocks a start until refueled.
- Accepted primer down-stroke: `ReportPrimerStroke(actorId)`; choke detent: `ReportChoke(actorId, closed)`. Do not emit duplicate primer reports from both a shared extension event and a direct callback.
- Physical starter pull: `ReportStarterPull(actorId)` returns RestartResult with the reason (NoFuel, NeedsPrimer, NeedsChoke, Cooldown, Requested). D holds a 2-second cooldown and sends StartGenerator only for valid attempts. Shared `OnGeneratorStartCompleted` is also accepted as the validated pull signal; choose one route per pull. Listen to `RestartFeedback` for failed-crank/reason audio; engine-catch audio should follow observed running state, not merely a requested command. Play a failed crank for NeedsPrimer/NeedsChoke/NoFuel, and distinct contextual cues/voice so the cause is audible. Keep the engine's fuel/choke state synchronized with D; open-choke is allowed only after `Flow.CanOpenChoke` (~10 seconds).
- Isolation valve: continuously report `ReportIsolation(actorId, held, pressureSafe)` at >=10 Hz while held, and immediately on release/unsafe pressure. Authority rejects stale reports after 0.3 seconds. A owns actual drifting valve motion and safe-band feedback. Both A and B must call `CanRepairPipe(repairActorId)` BEFORE applying repair effort/state, using different holder/repairer IDs. No solo helper on Day 3.
- Near-bed grip: `ReportBedGrip(actorId, handWorldPosition)`. D checks the STN_BED anchor distance and rejects early grips rather than banking them. Keep the affordance reachable without crouching.

Do not send duplicate simulation commands: B handles pump/repair; D handles validated StartGenerator. D's restart state resets per day/fault and accepts contributions from either keeper.
