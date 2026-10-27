# Sprint 3 merge review — 2026-10-21

Fetched with `git fetch team --prune`. Ordered dry-run from `sprint3/base`
(`6f1447b416ed`) using `git merge-tree --write-tree`; disposable merge objects
supplied the accumulated parents. No branch, index, scene or prefab was changed.

| Order | Team branch | Reviewed tip | Dry-run | Integration |
| --- | --- | --- | --- | --- |
| B | `team/sprint3/simulation` | `5d6b7fa1d7fc` | Clean | Already merged |
| D | `team/sprint3/progression` | `44e0b2c88715` | Clean | Already merged |
| A | `team/sprint3/hands` | `642a22ae2ef6` | Clean | Already merged |
| C | `team/sprint3/environment` | `2d33be810aed` | Clean | Already merged |
| E | `team/sprint3/networking` | `da268180d45e` | Clean | Already merged |

`integrate/sprint-3` already contained all five tips at `d4132351b310`.
Its existing first-parent `--no-ff` merge order is B `fd61b60`, D `f07235c`,
A `37cb8b0`, C `2d728bd`, E `9a9722c`. No redundant merges or history rewrite
were needed. No merge conflicts were found. Existing untracked numbered copies
and the older daily report were preserved.

## Review findings

These block claiming complete Sprint 3 gameplay; the current production APK uses
the inherited Storm Watch. They are not resolved by a clean textual merge.

- **P1 — D cannot start its leak days with B.**
  `SharedSimulationPort.cs:42,58` requires `ActivateDayLeak(int)`; neither B host
  implements it. D catches that missing capability and makes the watch unavailable.
- **P1 — E’s simulation adapter cannot be enabled as-is.**
  `FusionSimulationAdapter.cs:3,14` is compiled out; `LK.Net.asmdef` has no B
  reference. The class also lacks `ISimulationHost.ForceGeneratorFault()`.
  Adding only the symbol would break compilation; authority injection, station
  validation and scene composition also remain required.
  A review-only compile with the symbol and actual B/E assemblies confirmed
  `FusionSimulationAdapter.cs(14,66): CS0535`, missing ForceGeneratorFault.
  The project symbol/asmdef were not changed.
- **P1 — Wiring the current B/D event subscribers would bypass D’s rules.**
  `InteractionEventBridge.cs:23–30` directly enqueues RepairLeak/StartGenerator;
  D subscribes to the same events in `DayDirector.BindSimulation`. A’s repair
  does not call `CanRepairPipe` before completion. Use one validated command path
  before accepting Day 3 partner-gate or generator-prerequisite behavior.
- **P1 — A’s interaction actor IDs are not connected to E’s identity.**
  `HandRig.cs:27,41` retains its serialized actorId (prefabs default to zero).
  E binds only `ActorIdentity` on the rig (`FusionMessageChannel.cs:62`); no
  composition assigns HandRig.ActorId. Distinct-actor rule correctness is unproven.
- **P1 for environment acceptance — C is not adopted by the production entry.**
  The networking build helper excludes `/Lab/`; C’s new fixtures, board and bed
  are Lab-only. New powered/fault lighting bakes and their measured luminance
  remain pending. This APK cannot establish C’s environmental acceptance.
- **P2 — Remote gloves/poses remain placeholders.**
  `PF_PlayerRig.prefab:67` has null left/right hand visuals and no pose listeners;
  `CrewAvatarView.Hand` falls back to cubes. No caller updates SetHandPose.
  Transmitting grip values alone does not render A’s remote animated gloves.

## Validation boundary

The refreshed APK build, installation status and complete encounter-order checklist
are recorded in [DailyReport.md](DailyReport.md). Existing same-source evidence
is retained separately from this invocation; no two-headset, comfort or performance
acceptance is inferred from a build or unit test.
