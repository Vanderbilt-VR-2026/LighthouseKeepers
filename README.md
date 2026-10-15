# Lighthouse Keepers

## Storm Watch

Use **Lighthouse Keepers → Play game**. Host or join one crew room, signal ready on both clients, then **Begin watch** to enter the continuous three-watch cooperative climb together. The lobby connection carries gameplay too; no second Host/Join step is required. See [Storm Watch](Docs/StormWatch.md) for controls, progression and verification limits.

## First playable watch

Use **Lighthouse Keepers → First Watch → Play plumbing escape** for the survival prototype: brace the pipe, turn three valves in the posted order, then escape to the first landing and recover. This is local solo practice with Quest hand interactions, a timed clamp option and desktop controls. See [First Watch](Docs/FirstWatch.md) for controls, scope and the dedicated Quest APK build command. The ordinary environment preview remains available below.

**Unity: `6000.3.23f1`. Connected lobby/gameplay integration: [PR #40](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/40).**

**[Download the Sprint 1 APK for Meta Quest 3](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint1-quest3-20261001/LighthouseKeepers-Sprint1-Environment.apk)** — Android ARM64/IL2CPP development build. See the [release notes and checksum](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint1-quest3-20261001). Headset walkthrough and performance verification remain pending.

Open this checkout, then use **Lighthouse Keepers → Play game**. The default entry scene is the Photon crew lobby, `Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity`. Choose **Host crew**, or enter an existing room name under **Crew options → Join crew**; signal ready on both clients and begin the watch. Fusion launches Storm Watch on the same session, which loads the six content scenes. Click Game and use WASD/Q/E for desktop exploration. Photon is the normal game transport; local simulation is available only in the Development preview.

This Sprint 1 version contains the repaired lighthouse environment, floor/stair circulation, compact grounded pipework, ceiling fixtures and team code integration. The second and third floors are reserved for further furnishing; existing third-floor items and puzzle anchors remain. It is an environment foundation, not a completed puzzle/survival game. See [Sprint 1 publication](Docs/Sprint1Publication.md) for contributor commits, validation, branch guidance and pending Quest checks.

The local working folder is now `/Users/aneeshvasamreddy/LighthouseKeepers-Sprint1`. Teammates can clone anywhere; that path is not a project dependency. Android output: `Builds/LighthouseKeepers-Sprint1-Environment.apk`.

## Original setup guide — Sprint 1

## Sprint 1 demo
Where Sprint 1 landed, recorded on Quest 3 (48 s). Click the image to play the video.

[![Sprint 1 demo on Quest 3](Docs/Media/Sprint1DemoPoster.jpg)](Docs/Media/Sprint1Demo.mp4)

A standalone Quest 3 lighthouse environment foundation. Open with **Unity 6000.3.23f1 (Unity 6.3 LTS)** and install **Android Build Support, SDK/NDK Tools and OpenJDK** through Unity Hub.

Use **Lighthouse Keepers → Play game** or press Play to enter the Photon crew lobby by default. For isolated environment work, use **Development → Play environment**; Bootstrap loads six content scenes additively. Do not also load a second player rig. For isolated tests open `Assets/_LighthouseKeepers/Scenes/Development/LK_DevGym.unity`.

Editor preview without a headset: WASD walks; Q/E snap-turn. This preview uses character collision and gravity; it does not emulate tracked controller grabbing. Test interaction on Quest. In the Inspector, the persistent systems expose flood height, pause/reset and day selection. The DevGym overlay offers mouse-operated test controls. Select the ComfortSettings asset to configure movement, snap/smooth turning and vignette intensity. Changes to these shared settings affect the team; use a branch.

Quest controls: left stick walks relative to the headset, right stick snap-turns, grip grabs nearby interactables. Use the Quest system recenter gesture; a RecenterPlayer inspector action is also provided. No jumping, climbing or voice is implemented. The default Photon crew lobby supports roster, ready state and shared Storm Watch launch. Storm Watch shares keeper poses, inputs, flood, progression and rescue state through that same session; arbitrary legacy interactables remain local.

## Lobby and flood iteration

Choose **Lighthouse Keepers → Development → Play local lobby preview** to enter the keeper watch room. Use **Take your post → Signal ready → Begin watch** on the physical board. **Lower board** adjusts its height for seated use. Crew options include an offline guest simulation. See [the iteration guide](Docs/LobbyAndFloodIteration.md) for visual concepts, implementation notes and the Quest 3 test checklist.

The normal **Lighthouse Keepers → Play game** entry uses Photon hosting and joining. Host a room on one client and join the same room name on another; names, ready states and the host's scene launch are shared. See [Photon networking setup and scope](Docs/FusionNetworking.md). The published Sprint 1 APK predates the connected lobby and Storm Watch flow.

## Android build and USB test
1. Open File → Build Profiles, select Android and Switch Platform.
2. Confirm `LK_NetworkLobby` is first in the scene list, followed by `LK_StormWatch`, the development `LK_Bootstrap`, and the six content scenes. This is the normal Photon game startup. The historical Sprint 1 environment diagnostic builder explicitly selects its seven environment scenes.
3. Use **Lighthouse Keepers → Sprint 1 → Build Android environment** to build the authored scenes without regenerating them. Output: `Builds/LighthouseKeepers-Sprint1-Environment.apk` (Development Build, ARM64/IL2CPP, OpenXR).
4. Connect Quest 3 via USB, allow debugging, keep it awake and turn on both controllers.
5. Install the downloaded APK with `adb install -r LighthouseKeepers-Sprint1-Environment.apk` (or use its `Builds/` path for a local build), then `adb shell am start -n com.lighthousekeepers.game/com.unity3d.player.UnityPlayerGameActivity`.
6. Verify the checklist in `Docs/ManualVerification.md`. An APK build alone does not prove comfort or 72 Hz.

The game is **Lighthouse Keepers**, package `com.lighthousekeepers.game`; it does not replace Spear Throw VR.

## Team entry points
Read EnvironmentArchitecture, TeamWorkflow, PuzzleSocketGuide and Sprint1Handoff in Docs. `Lighthouse Keepers → 03 Validate Sprint Foundation` checks authored assets. `02 Create Missing Sprint Scenes` is an authoring fallback, not a runtime generator. Existing scenes are preserved. Runtime code is under Scripts/Runtime; editor tooling stays out of the player build.

Audio clips prefixed `Placeholder_` are original procedural placeholders. They demonstrate routing and timing, not final sound design. Geometry/materials are original stylized foundation assets. The supplied reference image was not actually available in this session; the written direction guided the work. Final art, audio, balancing and headset acceptance remain future work; the current cooperative prototype is described above.

Common failures: wrong Unity version; missing Android modules; another Editor holding the project lock; USB debugging not authorized; starting a content scene without Bootstrap; stale package import after switching platform. Allow package resolution and compilation to complete. Do not install Unity AI Assistant to fix game setup. Keep asset/meta pairs together and never commit Library or Builds.

Exact automated results and remaining checks are in Docs/Verification and Docs/Sprint1Handoff.md.

The team-compatible editor version is now **6000.3.23f1**. Do not accept an automatic upgrade to 6000.5 when opening this branch. See Docs/Unity63Migration.md for compatibility changes and verification. Historical Sprint 1 test records describe their original editor versions.

## Inspect the September 23 environment integration

Use **Lighthouse Keepers → Development → Open complete environment** to load the seven authored scenes in the editor. Press Play, click Game view, then use WASD/Q/E for the desktop preview. This Development command explicitly starts from LK_Bootstrap for isolated environment work. Use **Play game** to return to the Photon lobby startup.

Selected Asset Store props, offshore fog and the streamed horror ambience are now included; the supplied water shader has an editor comparison material and a Quest-safe runtime fallback. See [integration changes](Docs/AssetIntegrationStatus.md), [asset provenance](Docs/ThirdPartyAssets.md) and [verification](Docs/Verification/AssetIntegrationValidation.md). Final sound balance, both-eye effects and sustained Quest performance still need headset testing.
