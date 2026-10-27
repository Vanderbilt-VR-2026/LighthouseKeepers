# Lighthouse Keepers

Unity **6000.3.23f1** · Meta Quest · OpenXR / XR Interaction Toolkit · ARM64 IL2CPP.

This Sprint 3 candidate uses the teammate's **One Lighthouse Lab world**, based on `sprint3/one-lighthouse` through `7182583294c1fcf0962e755099887890f3bb5d94`, plus the `sprint-3` documentation at `e90e303`, reconciled in [PR #65](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/65). Its current playable loop is **solo**: three content days plus a Day 4 placeholder with pipe repair, generator restoration, radio broadcast and the F3 bed. **Co-op gameplay is disabled until the Lab's shared state is implemented.** Connecting two clients does not turn the local Lab simulation into a shared game.

The earlier two-player, three-watch [Storm Watch](Docs/StormWatch.md) and local [First Watch](Docs/FirstWatch.md) remain historical implementations on disk, excluded from this candidate's ship scenes. Their tests/evidence do not validate this Lab candidate.

## Sprint downloads and submission

| Sprint | README | Artifact status |
| --- | --- | --- |
| 1 | [Sprint 1](Docs/Sprints/Sprint1/README.md) | Historical environment APK and video. |
| 2 | [Sprint 2](Docs/Sprints/Sprint2/README.md) | Historical main APK differs from the final Sprint 2 source; video missing. |
| 3 | [Sprint 3](Docs/Sprints/Sprint3/README.md) | [Fresh Lab APK](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-one-lighthouse-candidate-20261027/LighthouseKeepers-Sprint3-89199c5a-20261027T222901Z.apk) · [checksum/build records](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint3-one-lighthouse-candidate-20261027); headset acceptance/video pending. |

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

## Open a fresh checkout


1. Clone the repository into a local folder outside cloud-sync directories:

   ```sh
   git clone --branch codex/sprint3-technical-fixes https://github.com/Vanderbilt-VR-2026/LighthouseKeepers.git
   cd LighthouseKeepers
   ```

   During this reconciliation review, use `codex/sprint3-technical-fixes` (PR #65). The planned final `sprint3-final` release has not been published; do not confuse it with the candidate release below.
2. In Unity Hub, install **Unity 6000.3.23f1**, including **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK**. Do not upgrade the project to a different Editor version.
3. Choose **Add project from disk** and select the repository root (the directory containing `Assets`, `Packages`, and `ProjectSettings`). Open with 6000.3.23f1. Let package resolution, asset import and C# compilation finish; internet is required for the first package download.
4. Photon Fusion is already in `Assets/Photon`. The configured `Assets/Photon/Fusion/Resources/PhotonAppSettings.asset` and its `.meta`, plus `Packages/manifest.json` and `packages-lock.json`, are committed. Do not import another Fusion SDK or replace shared app settings. All package references use registry/built-in sources, not a developer's local path.
5. Choose **Lighthouse Keepers → Play game**. It opens `Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity`. In the Game view, select **PLAY SOLO**; the destination is `Assets/Scenes/Lab/LK_Lab_Environment.unity`. The ship scene list contains these two scenes only.
6. Click the Game view to use **WASD** movement and **Q/E** turning for desktop inspection. Desktop movement does not emulate tracked controller grabbing; play the APK on Quest for the station interactions. Development builds also expose **DEV: PLAY DAY 2 / DAY 3** shortcuts.
7. For automated checks, open **Window → General → Test Runner**, then **Run All** in EditMode and PlayMode. Current results and test scope are in [reconciliation verification](Docs/Verification/Sprint3/README.md); the earlier unmodified-base failures are in [M7 verification](Docs/OneLighthouse/M7.md). A fresh clone/open of the exact merged release commit is a separate post-merge check; it is not inferred from this working checkout.


## Quest setup and installation help

Enable developer mode for the paired headset in the Meta Horizon app, connect a USB data cable, and accept USB debugging in the headset. Install the downloaded candidate with SideQuest or `adb install -r <downloaded-candidate.apk>`. Open **Library → Unknown Sources → Lighthouse Keepers**, then **PLAY SOLO**.


- In the Meta Horizon phone app, select the paired Quest 3 and open **Headset Settings → Developer Mode**. If the option is missing, confirm that the signed-in account is verified and belongs to a developer team, then reconnect the paired headset. [Meta developer-mode setup](https://developers.meta.com/vr/documentation/native/android/mobile-device-setup/).
- Keep the headset awake, reconnect a data-capable USB cable, and accept USB debugging. `adb devices` must show `device`; `unauthorized` means the headset prompt still needs approval.
- On Windows, install the [Meta/Oculus ADB driver](https://developers.meta.com/horizon/downloads/package/oculus-adb-drivers/) if the device is not detected. On macOS/Linux, use the SDK's `platform-tools/adb`.
- If Android reports `INSTALL_FAILED_UPDATE_INCOMPATIBLE`, an older build has a different signing key. Uninstall **Lighthouse Keepers** from the headset, or run `adb uninstall com.lighthousekeepers.game`, then install again. Uninstalling clears this app's local data.
- Confirm the download is the `.apk` asset, not GitHub's source ZIP, and compare its SHA-256 with the release notes. If installation succeeds but the app is not in the main grid, look under **Unknown Sources**.


## Credits and retained scope

Reused team work: **Eli Gripenstraw** (flood and lobby foundation), **Tapan Sidhwani** (interaction patterns including radio push-to-talk), **William / Will Qian (`canwer1`)** (Photon/Fusion foundation and Sprint 2 networking), and **Nirvaan Nagaldinne** (team integration and demo documentation). **Aneesh Vasamreddy** authored the shell/environment and integrated the one-lighthouse experience. Commit/PR references are recorded in the M7 report.

The first three days contain the current authored tasks; Day 4 is an explicit placeholder repeat. The teammate's original shipping plan and failure inventory remain in [M7](Docs/OneLighthouse/M7.md) as historical evidence. PR #65 supersedes PR #66 for this integration; no final release tag or main merge is performed here.
