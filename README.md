# Lighthouse Keepers

Unity **6000.3.23f1** · Meta Quest · OpenXR / XR Interaction Toolkit · ARM64 IL2CPP.

This Sprint 3 candidate uses the teammate's **One Lighthouse Lab world**, based on `sprint3/one-lighthouse` at `d2e30665a44b29a4e3bf84a7bfb7b049f93317b4`, reconciled in [PR #65](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/65). Its current playable loop is **solo**: four authored days with pipe repair, generator restoration, radio broadcast and the F3 bed. **Co-op gameplay is disabled until the Lab's shared state is implemented.** Connecting two clients does not turn the local Lab simulation into a shared game.

The earlier two-player, three-watch [Storm Watch](Docs/StormWatch.md) and local [First Watch](Docs/FirstWatch.md) remain historical implementations on disk, excluded from this candidate's ship scenes. Their tests/evidence do not validate this Lab candidate.

## Sprint downloads and submission

| Sprint | README | Artifact status |
| --- | --- | --- |
| 1 | [Sprint 1](Docs/Sprints/Sprint1/README.md) | Historical environment APK and video. |
| 2 | [Sprint 2](Docs/Sprints/Sprint2/README.md) | Historical main APK differs from the final Sprint 2 source; video missing. |
| 3 | [Sprint 3](Docs/Sprints/Sprint3/README.md) | Fresh reconciled APK/build evidence will be linked after verification; headset acceptance/video pending. |

Sprint 3 is due **Thursday evening, October 29, 2026 (America/Chicago)**; exact cutoff not supplied. [Submission issue and agent handoff #61](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/61) · [Milestone](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/milestone/1) · [Project](https://github.com/orgs/Vanderbilt-VR-2026/projects/15).

## Start the candidate

1. Open this checkout in Unity 6000.3.23f1 and let compilation finish. Choose **Lighthouse Keepers → Play game** to enter `LK_NetworkLobby`.
2. Choose **PLAY SOLO**. It works offline and opens `Assets/Scenes/Lab/LK_Lab_Environment.unity`; hosting is unnecessary.
3. Read the House task board. Grip the desk wrench, seat its jaw on the glowing F1 coupling and make the prompted wrist turns. Later days add the F2 generator starter and F4 radio. Finish the listed tasks, then grip near the F3 bed to sleep.
4. Left stick walks, right stick snap-turns, grip selects nearby tools/controls, trigger activates the held microphone. Keep the accepted tracked rig and hand orientation. Editor desktop preview uses WASD/Q/E.
5. Open **Watch Menu** with the left controller menu button (Escape in Editor) to return to the lobby. On failure, retry the same day; after the final day, replay from Day 1 or return. Menu actions preserve tracked camera/rig transforms.

[Full lobby controls and troubleshooting](Docs/Sprints/LobbyGuide.md) · [One Lighthouse station details](Docs/OneLighthouse/M4.md).

**Network diagnostics:** Host crew generates a six-character code. A second client uses **Crew options → room field → controller keypad** to enter it and joins; keeper names can remain at their defaults. Roster/readiness remain testable. A ready host may enter alone for debugging; **two connected keepers cannot launch the unreplicated Lab**. Use Play Solo for the playable candidate. No shared progression, two-person station gates or voice transport is claimed.

## Build and install

Install matching Android Build Support, SDK/NDK Tools and OpenJDK. **Lighthouse Keepers → Storm Watch → Build Quest APK**, **One Lighthouse M4 Build**, and both **Sprint 3 → Networking** Quest build menus now use one pipeline. The historical Storm Watch menu name is retained for compatibility; its build contains the current Lab.

The actual APK scene list must be exactly **Network Lobby → Lab**. Builds reject omissions/replacements even when global Build Settings look correct. Output is a fresh source-stamped APK under `Builds/Sprint3-<commit8>-<UTC>/`; no APKs/caches are committed. See the [pipeline and binary checks](Docs/Verification/StormWatch/QuestBuildPipeline.md).

Download only the exact candidate linked in the [Sprint 3 README](Docs/Sprints/Sprint3/README.md), verify its checksum, then use `adb install -r <downloaded-candidate.apk>` and `adb shell am start -n com.lighthousekeepers.game/com.unity3d.player.UnityPlayerGameActivity`.

[Current verification and limits](Docs/Verification/Sprint3/README.md) distinguish compilation, Editor execution, transport, Android packaging and target-headset acceptance. Check immersive cold launch/restart, controller entry/menu, physical station use, sound and performance on the downloaded APK. Two-device co-op acceptance remains blocked by the unfinished shared Lab simulation.
