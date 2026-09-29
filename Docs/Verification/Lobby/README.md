# Spatial lobby verification — September 29, 2026

Unity **6000.3.23f1**, macOS Editor. Base: `main` at `41c9819` (PR #20).

- **EditMode: 25 passed, 0 failed.** See [EditMode.xml](EditMode.xml). Covers foundation/assets/weather/lobby tests and unreachable flood-height, crew-capacity, preferred transport, subscription cleanup and ready-state regressions.
- **Rendered Play Mode: PASS, zero runtime errors.** See [PlayMode.txt](PlayMode.txt). The check exercised host, ready/unready, leave, guest simulation, board height and entry, then confirmed seven environment scenes and no remaining lobby screen.
- The verifier uses EventSystem raycasts and pointer enter/down/up/click/exit events. It checks that each selected button is active, interactable and the first raycast hit. This exercises the desktop-style UI path, not a physical tracked controller.
- World-space rendering and camera binding are asserted. Rotating the head leaves the board fixed. Lowering moves the complete board by 25cm without moving the camera, and raising restores its position.
- Eight occupied crew rows are checked for presence and text truncation. Actual ready, seated, options and full-crew renders were visually inspected for contrast and clipping.
- Screenshots are **actual Unity desktop camera renders at 1600×1000**, with no screen-space overlay substitution. The seated view places the camera at 1.2m; the wider room view uses an 80-degree camera field of view. They are not headset captures. Generated concept art in DesignReferences is separate visual exploration.
- `git diff --check` passed.

Play Mode entry is deferred to the next editor callback to avoid Unity's startup search-index exception. Newly revealed UI is rendered before checking raycast depth. The final records and images correspond to the corrected source.

The captures were refreshed after removing decorative and explanatory subtext. The rendered interaction check passed again with the simplified layout.

## Captures

1. [Offline / take your post](01-offline.png)
2. [Hosted / not ready](02-not-ready.png)
3. [Ready / begin watch](03-ready.png)
4. [Simulated guest](04-guest-preview.png)
5. [Seated camera / lowered board](05-seated.png)
6. [Wider keeper watch room](06-watch-room.png)
7. [Optional crew settings](07-crew-options.png)
8. [Full eight-person crew ledger](08-full-crew.png)

## Remaining checks

No Quest build, stereo rendering, physical controller-ray selection, haptic sensation, virtual keyboard, seated/standing comfort, recenter behavior, on-device frame timing or real multiplayer was verified. The lower-board behavior is mechanically checked; headset comfort still requires the device pass.

Run instructions and the device checklist are in [LobbyAndFloodIteration.md](../../LobbyAndFloodIteration.md).
