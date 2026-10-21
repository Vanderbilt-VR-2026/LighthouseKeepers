# B → D: progression and legacy authority migration

Status: [PROPOSED] pending merge. No existing progression or Storm Watch file was edited by B.

- Reference pure assembly `LK.Core.Simulation`, namespace `LighthouseKeepers.Core.Simulation`. Consume `ISimulationHost.Current` / `OnTick`; send `SimCommand` through `Enqueue`. `ForceGeneratorFault()` queues the Day 2 fault for the next 10 Hz tick; D chooses when to call it.
- `ResetRun` starts dry with an active leak, generator OFF, pump OFF and a full 900-second tank. Day 1 normal-power setup must enqueue `StartGenerator` then `SetElectricPump(Value=1)` when appropriate; arrange leak pressure/objectives intentionally because the proposed powered pump exceeds an active leak. A restored generator does not automatically restart its pump.
- `BeginDay(TargetId=day index)` preserves water, fuel and generator/pump state, reopens the leak and resumes `Running`. It resets the failure sustain timer. `Succeeded` freezes this day's integration until BeginDay/ResetRun; `Failed` requires ResetRun. Snapshot tick remains monotonic through resets. D owns scheduling and validates day transitions.
- Success is **repair AND depth ≤ 0.15 m**; failure is **depth ≥ 0.90 m sustained for 3 s**. The threshold-crossing fraction of a tick is counted, so the crossing tick cannot shorten the grace period. Do not declare progression success from a repair event alone.
- Existing `StormWatchRules.Tick` still directly integrates/clamps `State.water`, and `StormWatchDirector` still calls `flood.SetHeight(s.water)`. The legacy `FloodController` also caps its own rise. Those files are not in B's exclusive write lane. Integration must replace those writes with snapshot reads; B cannot truthfully claim the headset plateau has been removed from the current scenes yet. Coordinate legacy-file ownership with the lead if your lane also excludes them.
- The event bridge reports generator start, not electric-pump activation. Explicitly issue the latter after power recovery and surface `LastRejectReason` on presentation.

The core is injected and has no Unity or networking references. Tuning and verification commands: `Docs/Sprint3/Tuning.md`.
