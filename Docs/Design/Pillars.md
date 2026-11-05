# Sprint 4 master-pass design pillars

A tense, fair VR survival thriller for one or two keepers on Meta Quest 3. Keep a 1900s lighthouse running through a storm until dawn: repair the F1 pipe, manage F2 generation and power, rest in the F3 bunk, broadcast and maintain the F4 beacon. Flooding or prolonged beacon darkness loses the watch. Recovery moments make the pressure readable.

1. **Always know what to do next.** One actionable instruction at a time. Jobs belong on the chalkboard; the current action belongs on the object and transient objective line.
2. **Always feel the pressure.** Time, rising water and a dying beacon must be apparent through the world before players read a number.
3. **The world talks.** Machines, practical lights, sound and the harbor master explain consequences. Text is short, subordinate and temporary.
4. **Every action feels physical.** Preserve accepted hands and tool grips. Snaps, detents, ratchets, haptics and visible mechanical travel confirm work.
5. **Tense but fair.** Lose through slow or careless decisions, not obscure controls or unexplained penalties. Night 1 teaches; later nights combine understood verbs.
6. **Quest first.** Comfort and stable 72 fps take precedence over effects. Device budgets are targets until measured, never inferred from Editor frame rate.

Feedback order: object/light/motion → machine sound or sting → haptics → harbor voice → one HUD objective → chalkboard. No floating panels. Current guidance uses at most six words; station stencils use one to three. Future instructions stay hidden. Colour is never the only signal: use motion, shape, icons or readable labels too.

## Guardrails

- Start from main, small commits after each numbered step, draft PRs; never merge automatically. Features have switches. No macOS duplicate files (`* 2.*`). Repeatable authoring scripts are preferred.
- Host owns gameplay state, objectives and storm stage; guests request actions through the existing system. New replicated fields require a network-version bump and matching builds on both players.
- Preserve offline solo, days and task verbs, codes/rejoin/shared sleep, voice transport, hands/rotation, accepted trigger-clutch arm-pull ratchet, rain/audio zones, Watch Menu and no camera grain/vignette.
- William Qian owns skybox and avatar body. Do not edit either. Water presentation is now in scope; flood rules stay authoritative.
- No forced camera motion except a player-initiated pole slide; no flashes faster than 3 Hz or full-screen flashes brighter than the scene. Critical sound has a visual counterpart.
- Player-facing strings move to `Assets/Data/Strings`. Avoid per-frame allocations, duplicate sources of state, and visual effects without a measured budget.

## Performance targets and verification

Quest 3: 72 fps; GPU ≤11 ms; CPU main ≤9 ms; approximately ≤150 draw calls per eye view; ≤300k visible triangles; ≤24 audio voices; ASTC textures; at most one realtime shadow light. These are acceptance targets, not measured results.

Baseline before Part B and regression at each part boundary: graphics solo Nights 1–3, full-suite failure comparison, two-Mac crew join/avatars/voice plumbing/shared task/shared bed/rejoin. With no Quest connected, continue Editor work and APK builds; mark device tests NOT VERIFIED. Track install, device GPU/CPU/per-floor profiles, bolt turning, Mac+Quest listening/task/sleep and wall sweep in `Docs/Design/Next.md`.

The professor's intentional-design questions remain the framing: explain the lobby's first impression and next action, deliberate direct-versus-ray interactions, and embodied hand/tool choices. Concept-art floor arrangements are references; the shipped F3 bunk/F4 radio layout governs this implementation.
