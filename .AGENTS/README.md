# Unity and VR development guidance

Adapted from the user-supplied *AI Game Development Lexicon* on October 13, 2026. These are the repo's working guidelines; the adjacent reference preserves the vocabulary and examples. Examples illustrate instruction quality, not additional tasks or authorization to expand scope. Follow the user's current request and higher-priority instructions.

## Before implementation

- Inspect the current diff, scene hierarchy, prefabs, dependencies and existing systems before editing. Preserve unrelated working changes and asset GUIDs.
- For nontrivial work, state a short plan covering intent, context, behavior, architecture, constraints, performance and verification. Describe observable acceptance criteria.
- Define the core loop and state transitions: trigger, permitted interaction, feedback, failure, recovery and reset. Distinguish desired experience from implemented behavior.
- Reuse the installed XR Interaction Toolkit, OpenXR, Input System and URP. Check `Packages/manifest.json`, the lockfile and local package source before selecting APIs. Do not add redundant systems or dependencies without a task-specific need.
- Keep a version-control checkpoint before substantial changes and a reviewable diff afterward. Do not reset other people's changes.

## Implementation

- Keep one authoritative source of gameplay state. Separate rules, input and scene feedback; prefer events for discrete state changes and polling for continuous tracked poses or timers.
- Expose important design parameters in the Inspector, with units, sensible bounds and defaults. Read those same values in instructions, feedback and tests instead of duplicating thresholds.
- Specify direct versus ray interaction and hover, select and activate behavior. Document the rotation axis, proximity limits, release behavior and feedback. Verify required references and components before entering play or building.
- Preserve existing XR Origin, controller bindings and shared prefab references. Do not move the tracked camera as a gameplay effect. Explicit player-requested reset/teleport may reposition the rig; retain tracking offsets.
- Keep feedback lightweight and readable in world space. Treat controller reach, haptics, both-eye readability, comfort and embodiment as headset checks, not conclusions from Editor screenshots.
- Minimize the change surface. Keep experimental entry scenes isolated from the existing environment and build list unless the task requires changing them.

## Standalone VR performance

- Avoid per-frame object creation, avoidable allocations and unnecessary scene searches. Prefer shared materials, bounded effects and existing rendering systems.
- Profile on the target headset before claiming a bottleneck or performance improvement. Record CPU/GPU frame times, draw calls, GC allocations, headset/refresh rate, scene/workload and run duration. Compare the same workload before and after changes.
- Frame budgets are 13.89 ms at 72 Hz, 11.11 ms at 90 Hz and 8.33 ms at 120 Hz. A successful build or high Editor FPS is not device performance evidence. Record thermal or sustained-performance limitations.
- Consider whether an effect earns its rendering cost before adding a more complex optimization. Do not broaden a requested gameplay change into speculative optimization work.

## Verification and reporting

- Verify compilation, serialized references, available Play Mode behavior and the relevant regression tests before reporting implementation complete. Build the changed player when delivering an APK; never label an older binary as the current source.
- Exercise invalid input, repeated input/cooldowns, release/regrip, premature progression, failure and retry when relevant. Use meaningful behavioral tests rather than tests that repeat implementation details.
- Distinguish the evidence: (1) implementation report, (2) compilation, (3) Editor Play Mode, (4) target-headset execution, (5) measured usability/performance, (6) integration/regression evidence. Report each performed check and its limits; these are not interchangeable.
- Include reproducible commands or steps, results and unverified conditions. Never infer headset comfort, network synchronization or performance from Editor tests.
- Before a PR, review the complete diff, preserve Unity `.meta` files, exclude generated caches/build outputs and unrelated asset churn, and describe the final behavior and actual validation.

## Project entry points

- Unity version: `ProjectSettings/ProjectVersion.txt`.
- Gameplay code: `Assets/_LighthouseKeepers/Scripts/Runtime`.
- First Watch entry and launch/build menu: `Assets/_LighthouseKeepers/Scripts/Editor/FirstWatchAuthoring.cs`.
- First Watch loop, controls and acceptance criteria: `Docs/FirstWatch.md`.
- Executed evidence and limitations: `Docs/Verification/FirstWatch/README.md`.
- Use the installed `unity` CLI / Pipeline bridge for Editor inspection where available. Distinguish Editor tooling from runtime/player access; do not enable player tooling implicitly.

## Agreed Storm Watch scope

Exactly two players; one continuous three-watch run targeting roughly ten minutes total. Day 1 teaches bracing/wrench repair with normal power and visible drainage. Day 2 splits manual pumping and generator restoration. Day 3 combines faults and radio-to-beacon relay across all four floors. Repairs open physical routes; refuges offer repair help or rescue protection while water approaches. Setbacks lose only the unfinished step. Lifeline rescue is recoverable; both players must be alive at the top after restoring the beacon. Pressure regulation and Days 4–5 are deferred. It is nighttime: no persistent exterior daylight source.
