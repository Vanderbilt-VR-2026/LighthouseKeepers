# Keeper watch room and flood iteration

Base: `main` at `41c9819`, including Eli's PR #20. Work continues on PR #24, `codex/flood-lobby-polish`, with Unity **6000.3.23f1**.

## Spatial lobby design

The lobby is an authored keeper room, using the same worn timber, slate, brass, vendor furniture, cold ocean material and warm practical lighting as the environment. A physical, framed watch board holds a paper crew ledger. Sea and rain ambience come from the window. The scene has a clear floor area in front of the board and solid window boundaries.

Both desktop and XR render the **same world-space board**. There is no screen-space overlay or panel that follows head rotation. Leaning or walking gives real parallax between the controls, frame, fittings and room. The starting distance is about 2.8m; the player can approach. The board is 2.56m wide and 1.92m tall. Main controls are at least 70cm wide and 16cm high, positioned near 1m above the floor. TextMesh Pro's existing distance-field font keeps lettering legible as viewing distance changes.

The large default controls are **Take your post → Signal ready → Begin watch**. Status uses words and a colored watch indicator. Button hover/press changes color, the label depresses slightly within a fixed hit target, selection plays a quiet spatial click, and tracked-pointer events request short haptic impulses from that controller. Hardware sensation remains to be verified.

**Lower board** moves the whole physical frame and canvas down 25cm for seated use; **Raise board** restores its authored position. It never moves the camera or XR origin. **Crew options** contains optional name entry and the explicitly labeled guest simulation, keeping setup fields out of the default view. Local practice requires no keyboard.

## Try it

Choose **Lighthouse Keepers → Play lobby preview**. The menu opens `LK_LobbyPreview`; it only creates the scene if absent. Existing shared environments and the player prefab are untouched.

1. Point a controller at **Take your post** and select with the trigger, or click with the desktop mouse.
2. Select **Signal ready**, then **Begin watch** to load Bootstrap and its six content scenes.
3. **Stand down** revokes readiness. **Leave watch** resets the crew. **Crew options → Simulate a guest** is an offline roster preview, not a connection to another headset.
4. Try **Lower board** from both standing and seated positions; check legibility and controller reach.

The default build list is unchanged. For Quest, use a temporary Build Profile with `LK_LobbyPreview` first and retain all seven environment scenes. The normal **Play environment** menu still opens Bootstrap.

## Runtime ownership

- `Scripts/Runtime/Lobby`: roster rules, local transport, world-space board and pointer feedback.
- `Scripts/Editor/LighthouseLobbyPreview`: authors only the isolated preview scene. The explicit batch entry `RebuildWatchRoom` replaces that preview, so do not run it on an editor containing unsaved work. Normal Play does not regenerate existing scenes.
- `Scripts/Editor/LighthouseLobbyVerification`: captures the actual spatial UI and checks mouse-style raycasts, pointer events, board height, ready/guest flow and scene transition.
- `Scripts/Runtime/Core/EnvironmentBootstrap`: one rig and systems scene, with six additive content scenes.
- `Scripts/Runtime/Flood`: one bounded global water height, accelerating rise rate, surface tone, room thresholds and head-clearance danger API. It is not sealed-room fluid simulation.

## Retained flood and usability fixes

Time to a target above the maximum flood height is infinite. Reaching the cap or using a zero rise rate no longer publishes redundant height events every frame. Minimum crew requirements cannot exceed capacity; assigned transports take precedence and subscriptions are cleaned up. Desktop typing in either legacy or TMP input fields suppresses WASD/QE movement.

`FloodDangerMonitor` is not attached to an environment scene yet. Connecting its stages to visible warnings remains the next flood integration step. A real network transport and synchronized launch also remain future work.

## Visual references and evidence

`DesignReferences/FloodLobby/keeper-briefing-and-flood-concepts.png` is the earlier generated concept board, not a screenshot. This revision implements the physical room/board direction using repository assets; the concept's environmental depth rulers, flood signage and reflective water remain proposals. Current in-engine captures and results are in [Verification/Lobby](Verification/Lobby/README.md).

## Quest 3 checklist

- Verify both-eye text and water rendering, pointer hover/trigger selection with each controller, and short haptic feedback.
- Approach and lean around the board; ensure it stays anchored and room boundaries prevent leaving the standing area.
- Test the full sequence seated and standing. Lower/raise must move the entire frame by 25cm without moving the view.
- Confirm the main controls are readable and reachable; check system recentering and near-surface clipping.
- Check selection/sea/rain audio balance, default-name practice and optional virtual keyboard behavior.
- Enter the lighthouse and confirm one rig/listener. Flooding should retain its existing paused default.
- Inspect head-clearance stages with a monitor added in Play Mode, repeat on stairs/crouching, and check reset and safe heights above 10.5m.
- Profile frame timing on the device. The preview uses two unshadowed point lights, the existing ocean shader and no post-processing or planar reflections; desktop results do not establish Quest performance.
