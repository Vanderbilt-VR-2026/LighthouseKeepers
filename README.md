# Lighthouse Keepers — Sprint 1

A standalone Quest 3 lighthouse environment foundation. Open with **Unity 6000.3.23f1 (Unity 6.3 LTS)** and install **Android Build Support, SDK/NDK Tools and OpenJDK** through Unity Hub.

Open `Assets/_LighthouseKeepers/Scenes/Bootstrap/LK_Bootstrap.unity` and press Play. It loads six content scenes additively. Do not also load a second player rig. For isolated tests open `Assets/_LighthouseKeepers/Scenes/Development/LK_DevGym.unity`.

Editor preview without a headset: WASD walks; Q/E snap-turn. This preview uses character collision and gravity; it does not emulate tracked controller grabbing. Test interaction on Quest. In the Inspector, the persistent systems expose flood height, pause/reset and day selection. The DevGym overlay offers mouse-operated test controls. Select the ComfortSettings asset to configure movement, snap/smooth turning and vignette intensity. Changes to these shared settings affect the team; use a branch.

Quest controls: left stick walks relative to the headset, right stick snap-turns, grip grabs nearby interactables. Use the Quest system recenter gesture; a RecenterPlayer inspector action is also provided. No jumping, climbing, voice or networking is implemented.

## Android build and USB test
1. Open File → Build Profiles, select Android and Switch Platform.
2. Confirm `LK_Bootstrap` is first in the scene list. All eight scenes are registered; the automated APK uses the seven environment scenes.
3. Enable Development Build; use ARM64/IL2CPP and the installed OpenXR loader. Build to `Builds/LighthouseKeepers-Development.apk`.
4. Connect Quest 3 via USB, allow debugging, keep it awake and turn on both controllers.
5. Use Build And Run, or `adb install -r Builds/LighthouseKeepers-Development.apk`, then `adb shell am start -n com.lighthousekeepers.game/com.unity3d.player.UnityPlayerGameActivity`.
6. Verify the checklist in `Docs/ManualVerification.md`. An APK build alone does not prove comfort or 72 Hz.

The game is **Lighthouse Keepers**, package `com.lighthousekeepers.game`; it does not replace Spear Throw VR.

## Team entry points
Read EnvironmentArchitecture, TeamWorkflow, PuzzleSocketGuide and Sprint1Handoff in Docs. `Lighthouse Keepers → 03 Validate Sprint Foundation` checks authored assets. `02 Create Missing Sprint Scenes` is an authoring fallback, not a runtime generator. Existing scenes are preserved. Runtime code is under Scripts/Runtime; editor tooling stays out of the player build.

Audio clips prefixed `Placeholder_` are original procedural placeholders. They demonstrate routing and timing, not final sound design. Geometry/materials are original stylized foundation assets. The supplied reference image was not actually available in this session; the written direction guided the work. Final art, audio, balancing, multiplayer and completed puzzles remain future work.

Common failures: wrong Unity version; missing Android modules; another Editor holding the project lock; USB debugging not authorized; starting a content scene without Bootstrap; stale package import after switching platform. Allow package resolution and compilation to complete. Do not install Unity AI Assistant to fix game setup. Keep asset/meta pairs together and never commit Library or Builds.

Exact automated results and remaining checks are in Docs/Verification and Docs/Sprint1Handoff.md.

The team-compatible editor version is now **6000.3.23f1**. Do not accept an automatic upgrade to 6000.5 when opening this branch. See Docs/Unity63Migration.md for compatibility changes and verification. Historical Sprint 1 test records describe their original editor versions.

## Inspect the September 23 environment integration

Use **Lighthouse Keepers → Integration → Open complete environment** to load the seven authored scenes in the editor. Press Play, click Game view, then use WASD/Q/E for the desktop preview. Play starts from LK_Bootstrap to keep one persistent rig and audio hierarchy.

Selected Asset Store props, offshore fog and the streamed horror ambience are now included; the supplied water shader has an editor comparison material and a Quest-safe runtime fallback. See [integration changes](Docs/AssetIntegrationStatus.md), [asset provenance](Docs/ThirdPartyAssets.md) and [verification](Docs/Verification/AssetIntegrationValidation.md). Final sound balance, both-eye effects and sustained Quest performance still need headset testing.
