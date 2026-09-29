# Lobby and flood iteration — September 29, 2026

Started from `main` at `41c9819`, including Eli's merged PR #20, on `codex/flood-lobby-polish`.

## Repository map

- `Scripts/Runtime/Flood`: bounded rising water, accelerating rise rate, head-clearance danger stages, surface darkening and room threshold events.
- `Scripts/Runtime/Lobby`: crew state, transport interface, local simulation, ready rules and runtime UI.
- `Scripts/Runtime/Core/EnvironmentBootstrap`: loads the six content scenes around the single persistent player and systems scene.
- `Scenes/Levels/LK_Level01_Plumbing`: flood surface, plumbing and repair anchors. Floors rise in 3.2 metre increments.
- `Scripts/Tests`: EditMode regression tests. `Scripts/Editor`: authoring, launch and verification tools.

All paths above are under `Assets/_LighthouseKeepers/`. Unity version is **6000.3.23f1** as recorded in ProjectSettings, rather than the shorthand version in PR #20's description.

## Try the lobby

Choose **Lighthouse Keepers → Play lobby preview**. It opens `LK_LobbyPreview`, a separate development scene with the existing Quest rig. The tool creates that scene only if missing. The environment launch menu still opens Bootstrap. No shared content scenes or player prefabs are regenerated.

1. Optionally edit the keeper name; choose **Open practice**.
2. Read the crew status and disabled-entry explanation. Select **I'm ready**, then **Enter lighthouse**.
3. Entry loads the existing Bootstrap and its additive environment. Flooding remains paused according to existing game behavior.
4. **Unready** revokes entry. **Leave crew** resets the session. **Preview guest** explicitly shows simulated guest state; it does not connect to another headset. Leave the guest preview and open practice to enter the environment.

Desktop uses mouse UI and the input system's event module. In XR the board uses world-space Canvas, tracked-device raycasting and the current main camera. It is placed once, 2.2 metres ahead, rather than attached to the head. The existing rig has UI-enabled interactors. Default keeper name allows button-only local practice without typing on Quest; headset keyboard entry is not yet verified.

The normal build list is unchanged. To test the preview on Quest, use a temporary Build Profile scene list with `LK_LobbyPreview` first and retain all seven environment scenes so entry can load Bootstrap. Do not replace the team's main build list just for this test.

## Corrections in this pass

- Use Unity's supported built-in legacy font, initialize UI input when absent, initialize the name field, and fit eight crew rows in a fixed layout.
- Make local simulation explicit; show ready/unready state, current crew count, start requirements and failed connection/scene-load feedback.
- Prefer an explicitly assigned transport and unsubscribe its events when the manager is destroyed. Clamp minimum required crew to capacity.
- Typing in a desktop input field no longer drives WASD movement or Q/E snap turns.
- Report infinite time to a target above maximum flood height. Previously a safe head position above 10.5m incorrectly counted down to maximum water height.
- Stop emitting height events every frame after the flood reaches its cap or has a zero rise rate.
- Initialize lifecycle dependencies explicitly in EditMode tests; those tests do not receive Play Mode's Awake/OnEnable calls.

## Visual direction

`DesignReferences/FloodLobby/keeper-briefing-and-flood-concepts.png` is generated concept art, **not an in-game capture**. It proposes a physical crew board, a depth ruler near the pump, amber early warnings, and readable high-ground signage with restrained red emergency lighting. Detailed props, water reflections, environmental depth markers and the physical brass frame shown there are not implemented by this pass. The runtime lobby adopts the navy, cream and amber palette and briefing hierarchy.

The merged `FloodDangerMonitor` is not attached to an authored scene yet. Its stage and countdown API is tested, but wiring it to visible player warnings is a next integration step. For manual stage inspection, add it to the flood systems object in Play Mode.

A real network transport and synchronized scene launch remain separate future work. The existing transport interface models roster operations but does not yet broadcast a shared start request.

## Quest 3 test pass

- Confirm the board appears at a comfortable distance, remains stationary while looking around, and is readable in both eyes.
- Test each controller ray: hover, select, Ready, Unready, Leave and Enter. Confirm one rig and listener remain after entry.
- Repeat seated and standing; check that system recentering leaves the board reachable.
- Check default-name practice before trying text entry; note any virtual keyboard limitations.
- In the environment, use flood controls to inspect dry, watch, warning, critical and submerged stages; repeat while moving up stairs and crouching.
- Verify that a head above 10.5m has no submersion countdown, and resetting the flood pauses and lowers it.
- Check water rendering in both eyes, warning readability, comfort, audio changes and frame timing on the headset. Desktop captures do not establish any of these results.

## Repeatable verification

EditMode: run all project tests in Unity Test Runner. Rendered integration: invoke `LighthouseKeepers.Editor.LighthouseLobbyVerification.RunBatch` from a **separate batch-mode Unity process with graphics enabled** and this project closed in other editors. It writes four rendered lobby states and a result to `Docs/Verification/Lobby`, exercises the buttons and verifies Bootstrap loads seven scenes, then exits Unity. Do not invoke that batch entry point inside an editor holding unsaved work.

Latest recorded result: **25/25 EditMode tests passed** and the rendered lobby-to-environment check passed with **zero runtime errors**. See [verification results and actual captures](Verification/Lobby/README.md). Quest checks remain unverified.
