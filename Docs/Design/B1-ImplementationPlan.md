# B1 continuation plan — not implemented

The release and before baseline are complete. No production B1 code has been applied. Uncompiled sketches in `artifacts/s4-master/b1/` are exploratory only; do not copy them into Assets as a finished feature. They omit wiring, string migration, validity checks and tests.

## Integrate through existing boundaries

- `WatchState.ObjectiveIndex` currently chooses one global job. `ObjectiveHud.Title`, `HarborRadio.CurrentJob`, `WatchEngineControls.Instruction`, radio/beacon/breaker readouts and the board each choose additional wording independently. Replace these choices with a single host-owned director, gated by a master tuning switch. Preserve the existing mechanics and disabled-switch path.
- `WatchState.Capture` / `ApplyRemote` and `WatchSnapshot.Valid` own snapshot rules. Assign per-player directive IDs through this path, never by an independent guest planner. Expose keeper IDs/head positions through a small optional interface in `CoopWatchBridge`; implement it on `WatchCoopSession` using the existing `CrewPoseStream`. Do not edit avatar/body or skybox.
- Select urgent actionable jobs first, then use actual station/head positions to assign different nearby jobs to two keepers where possible. Keep assignments stable while working; avoid flip-flopping every frame. Solo keeps one urgent instruction. Avoid assigning an unpowered upper station as a duplicate generator job.
- Keep the accepted XRI controls: generator `Primer`, `Choke`, `FuelCan`, `Filler`, `WatchGenerator.Starter`; panel `Control(index)`/`ResetControl`; radio `controls`/`microphone`; beacon `Ignition`/`Wheel`/`Rag`; pipe active bolt and leased wrench. Do not rewrite gestures or hand transforms.

## Complete B1 as a whole

1. Create the central `Assets/Data/Strings` table for player text. Migrate active Lab instructions/stencils/readouts plus existing UI sources; validate six-word instruction limits. Preserve spoken/subtitle sentences as dialogue, not six-word control instructions.
2. Director supplies current step to HUD, equipment cues, harbor hints and a jobs-only chalkboard. HUD also shows a short partner job. Future steps hidden; stale completion panels/paragraph labels removed. Audit before/after text count; reduce by at least half.
3. One current control pulses warmly at 1.2 s; untouched ≥10 s gives a quiet spatial tick every few seconds. Stop idle ticks while held/used. Completed controls have a non-colour-only mechanical/shape cue. Reuse bounded audio/haptics and current mixer routes.
4. Generator: fuel/check/refill → primer ×3 → choke closed → three starter pulls → choke open. Silently ignore extra primers/pulls in the friendly path; remove hidden penalties there, retaining the old disabled-switch behavior. Add the short “There she goes” placeholder/caption, use the existing radio import/filter chain. Power return fades upward by floor. Keep the accepted starter grip.
5. Radio/breaker/beacon/pipe read the same director. One code button may be highlighted at a time. Never create a second independent instruction policy inside a view.
6. New snapshot fields require a new NETWORK VERSION after FLOOD1/v9. Update strict validation, tests, lobby and documentation; both clients must match.
7. Focused tests: each sequence, ignored extras, interrupted/power-loss flow, late join/disconnect, guest cannot author state, nearest nonduplicate assignment, current-only cue visibility, strings and text reduction. Run graphics sequence screenshots and inspect them; two-Mac tasks and assignment checks. Build an APK with installs pending if absent. Commit + push only when B1 is complete and reviewable.

## Baseline caveats to retain

`Docs/Verification/S4Master/before/full-suite-summary.json` has 27 failures before B1. Several are legacy build-list/rehearsal tests; others reference older lighting, wrist twist, generator timing and menu/fail assumptions. The F3 transfer route fails at `(1.40, 6.61, -0.96)` toward `(1.40, 6.60, 0.00)`; investigate during B5/B6 instead of hiding it. Do not infer all failures are stale without examining each. Graphics Nights 1–3 and live two-Mac task/bed/rejoin passed separately.
