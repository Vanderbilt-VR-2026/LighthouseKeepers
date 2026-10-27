# B → A: interaction integration

Status: [PROPOSED] pending merge and headset checks.

- Keep the three shared `Action<int>` events and actor IDs exactly as specified. `InteractionEventBridge` subscribes and unsubscribes to those events, enqueuing the matching commands. No pump/repair/generator gesture implementation was added by B.
- Read `FloodTuning.manualPumpMinStrokeSeconds`: final proposed minimum is **1.20 s**, preserving 0.200 m³/stroke. The supplied 0.55 s would overpower the 0.180 m³/s leak. The simulation enforces one cooldown for the single physical pump, shared across actors.
- B uses reflection to remain compilable before A merges. Preserve `LighthouseKeepers.Gameplay.Interaction.InteractionEvents` and all three event add/remove accessors for IL2CPP (`[UnityEngine.Scripting.Preserve]` or A's `link.xml`); confirm the events survive a Quest build. After assembly availability, `SimulationHostBehaviour.RebindInteractionEvents()` can explicitly rebind without duplicate listeners. A typed subscription can replace this integration adapter after merge.
- A completed generator-start gesture sends `StartGenerator` only. D explicitly requests `SetElectricPump(Value=1)` after power returns. Read `LastRejectReason` for flooding, missing geometry and exhausted fuel.
- E must route client commands to the host rather than subscribing both a local bridge and an independent networking bridge on the authority.
