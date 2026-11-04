# Lab UI inventory — before S4-M3

Observed in the unchanged main-game runtime, Mac host + Quest guest on November 3. [Machine-readable inventory](../Verification/Immersion/before/ui-inventory.json) includes 73 text/canvas components (including inactive menu labels). Reviewed the captures below. Runtime diagnostics live in `artifacts/immersion/before/ui/`.

| Visible object / group | Creator / owner | S4-M3 treatment | Before |
| --- | --- | --- | --- |
| `Watch camera HUD/Watch readout`: day, countdown, tasks, loss message | WatchDemoBuilder / WatchDemoController | Remove camera readout; goal/tasks/time belong on chalkboard; loss on logbook | [HUD](../Verification/Immersion/before/view-hud.png) |
| `Main Camera/Local crew debug readout`: region, code, names/count | WatchCoopSession / networking presentation | Lobby only; remove Lab debug text, retain transport | [HUD](../Verification/Immersion/before/view-hud.png) |
| `Watch demo/Repair feedback`: billboard coupling/hold/turn/progress prompt | WatchPipeRepair | Remove billboard; chalk on coupling, physical motion/gauge, sound/haptics | [Pipe](../Verification/Immersion/before/pipe-ui.png) |
| `Watch demo/Sleep prompt`: turn-to-view bed text | WatchDemoController | Remove; physical bunk plate, lamp and spoken early/ready cue | [Bed](../Verification/Immersion/before/bed-ui.png) |
| `Night survived readout`: text parented to camera | S4WatchPresentation | Replace with ink on the logbook during existing comfort fade | Solo night capture in artifacts |
| `STN_GENERATOR/Starter instruction`: floating state/sequence/flooded prompt | WatchGenerator / WatchEngineControls | Remove; riveted sequence plate, five lamps, two tally wheels; no flooded state | [Generator](../Verification/Immersion/before/generator-ui.png) |
| Generator loose primer/choke/starter/filler captions, fuel percentage | WatchEngineControls | Mount short labels on equipment; physical gauge/tallies communicate state | [Generator](../Verification/Immersion/before/generator-ui.png) |
| `S4 breaker panel`: heading, circuit/reset labels, meter legend/instruction | WatchPowerPanel | Retain cabinet; turn labels into mounted stencils/plates, keep needle/lamps | [Breaker](../Verification/Immersion/before/breaker-ui.png) |
| `S4 beacon station`: heading, instructions, ignition/wheel/rag labels, `Lens wipe label` | WatchBeaconStation | Mounted instructions/engraved brass marker; remove floating lens caption | [Beacon](../Verification/Immersion/before/beacon-ui.png) |
| Radio header, steps, knob/button captions, scales, CRT and solo code card | WatchRadioConsole / WatchRadioBuilder | Paint/stencil captions onto metal and physical card; retain CRT/analogue scales and functional lamps | [Radio](../Verification/Immersion/before/radio-ui.png) |
| `Coupling marker` A/B/C beside each pipe bolt | WatchPipeBuilder | Move onto coupling as chalk; chalk directional mark for active coupling | [Pipe](../Verification/Immersion/before/pipe-ui.png) |
| `House wall briefing board/Watch checklist` | WatchDemoBuilder / WatchDemoController | Keep flush wall placement, restyle as chalkboard, clear goal/jobs/loss rule | [House](../Verification/Immersion/before/house-board.png) |
| `Watch action menu` (inactive until opened or terminal state) | WatchMenu | Preserve actions/mute/XR input; weathered logbook page | Deliberately opened before/after capture required |
| Remote keeper floating name label | William’s avatar/body ownership | Leave avatar implementation untouched; record as ownership exception, not a gameplay guidance panel | Mac+Quest avatar capture |
| Lobby crew UI | CrewLobbyBoard | Retain; add NETWORK VERSION here only | Baseline lobby capture |

Status: replacements implemented behind `ImmersionTuning` and checked in graphics Play Mode on `841b790`. The old camera readout, crew debug line, repair/sleep billboards, starter instruction and floating lens caption are inactive. The chalkboard, mounted equipment labels, lobby and deliberately opened logbook remain. Remote name tags are unchanged under William’s ownership. Device readability and the post-change live co-op check are tracked separately in the verification report.
