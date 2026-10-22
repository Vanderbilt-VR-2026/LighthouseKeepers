# B → A: interaction integration

Status: [PROPOSED] pending merge and headset checks.

## Round 2 correction

B's bridge now forwards pump/repair only. D owns validation of prime/choke/fuel and enqueues StartGenerator once; do not rely on B forwarding OnGeneratorStartCompleted, since that bypasses D. Bind A's physical repair gate continuously to D.CanRepairPipe and route the validated completion through the shared facade. B rechecks the injected actor predicate before clearing the leak. Preserve existing event names and tuning reads.

Both F1 hand pump and F2 manual bilge may emit the same stroke contract; B retains one shared hydraulic throughput budget/cooldown so simultaneous actors/stations cannot overpower an active fault. Normal leak=0.180, slower generator-fault seep=0.176 m³/s; maximum hand throughput stays below both. No hand/rig/prefab files were edited by B. Install actual hand/wrench/pump components in the Shell/Dress scenes reached from Play Solo, not Storm Watch.

B's R8 test invokes your public read-only `StationInteractionBinding.CollectMissingShipBindings(List<string>)` after the real lobby route. Missing animated hands/wrench/pipe/pump/generator bindings fail the test; B does not call Bind to patch them up during evidence capture. This prevents a screenshot of populated scenery from masking code that was never installed.

- Keep the three shared `Action<int>` events and actor IDs exactly as specified. `InteractionEventBridge` subscribes and unsubscribes to those events, enqueuing the matching commands. No pump/repair/generator gesture implementation was added by B.
- Read `FloodTuning.manualPumpMinStrokeSeconds`: final proposed minimum is **1.20 s**, preserving 0.200 m³/stroke. The supplied 0.55 s would overpower the 0.180 m³/s leak. The simulation enforces one cooldown for the single physical pump, shared across actors.
- B uses reflection to remain compilable before A merges. Preserve `LighthouseKeepers.Gameplay.Interaction.InteractionEvents` and all three event add/remove accessors for IL2CPP (`[UnityEngine.Scripting.Preserve]` or A's `link.xml`); confirm the events survive a Quest build. After assembly availability, `SimulationHostBehaviour.RebindInteractionEvents()` can explicitly rebind without duplicate listeners. A typed subscription can replace this integration adapter after merge.
- A completed generator-start gesture sends `StartGenerator` only. D explicitly requests `SetElectricPump(Value=1)` after power returns. Read `LastRejectReason` for flooding, missing geometry and exhausted fuel.
- E must route client commands to the host rather than subscribing both a local bridge and an independent networking bridge on the authority.
