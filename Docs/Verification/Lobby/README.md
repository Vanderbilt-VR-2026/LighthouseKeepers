# Lobby verification — September 29, 2026

Unity **6000.3.23f1**, macOS Editor. Base: `main` at `41c9819` (PR #20).

- **EditMode: 25 passed, 0 failed.** See `EditMode.xml`. Covers existing foundation/assets/weather/lobby tests and new unreachable flood-height, crew-capacity, preferred transport, subscription cleanup and ready-state regressions.
- **Rendered Play Mode: PASS, zero runtime errors.** See `PlayMode.txt`. The final capture run checked initial local-practice wording and nonempty button labels, exercised practice, ready/unready, leave, guest simulation and entry, then confirmed seven environment scenes and no remaining lobby screen.
- Screenshots are actual Unity desktop renders of the runtime UI. For reproducible capture, the verifier renders the same Canvas through the main camera at 1280×860 and rebuilds text for that capture scale. They are not headset captures.
- Visually inspected offline and ready screens for legibility, labels and clipping. The concept art in DesignReferences is a separate proposal, not evidence of in-game environment art.
- `git diff --check` passed.

The first graphics run encountered Unity's startup search-index exception. Deferring Play Mode until the next editor callback resolved it. Initial screen/transport initialization order and missing initial button-label assignment were fixed during visual review. The final record and images correspond to the corrected source.

## Captures

1. [Offline / open practice](01-offline.png)
2. [Hosted / not ready](02-not-ready.png)
3. [Ready / enter lighthouse](03-ready.png)
4. [Simulated guest](04-guest-preview.png)

## Remaining checks

No Quest build, stereo rendering, controller-ray selection, virtual keyboard, seated/standing comfort, recenter behavior, on-device frame timing or multiplayer was verified. Button callbacks were exercised programmatically; physical pointer input still needs manual testing. Eight-row capacity is supported by the layout but these captures use the default four slots.

Run instructions and the device checklist are in `Docs/LobbyAndFloodIteration.md`.
