# Lighthouse Keepers

### Keep the light on. Keep each other afloat.

A VR lighthouse survival game for **Meta Quest 3**. Repair leaking pipes, restart the generator, and broadcast the storm warning before your watch runs out.

**[Get the co-op APK](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/coop-preview-20261102/LighthouseKeepers-Coop-Preview.apk)** · **[Watch / download co-op proof](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/coop-preview-20261102/LighthouseKeepers-Coop-Proof.mp4)** · **[Install help](Docs/INSTALL.md)**

## Choose your download

No Unity, source code, or GitHub account needed. **APK = game. MP4 = video.**

| I want to… | Download the game | Video | Start here |
| --- | --- | --- | --- |
| **Try co-op with a friend** — 2-player preview | [Co-op APK · 83 MB](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/coop-preview-20261102/LighthouseKeepers-Coop-Preview.apk) | [Networking proof · 12 MB](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/coop-preview-20261102/LighthouseKeepers-Coop-Proof.mp4) | Install this same APK on both headsets; follow **Play together** below. |
| **Grade Sprint 3** — submitted playable game | [Sprint 3 APK](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-final/LighthouseKeepers-Sprint3.apk) | [Sprint 3 submission video](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-final/LighthouseKeepers-Sprint3-Video.mp4) | Open **Lighthouse Keepers** → **PLAY SOLO**. |
| **Grade the Sprint 2 resubmission** — original playable lighthouse | [Sprint 2 APK](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint2-final-v2/LighthouseKeepers-Sprint2.apk) | — | Open **Lighthouse Keepers S2** → **TAKE YOUR POST** → **SIGNAL READY** → **BEGIN WATCH**. |

**Professor / TA:** Sprint 3 includes the playable APK **and** submission video. Sprint 2 is a **playable APK resubmission**. Both original releases remain available, separate from the co-op preview.

**Co-op preview status:** tested with **one Quest + one Mac Editor** in the same House, with visible avatars and received hand movement. Shared tasks and bed transitions passed between two Mac Editors. **Two actual headsets together and the full shared task loop on Quest still need testing.** This is a preview, not a finished multiplayer release. [Proof and known issues](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/blob/coop/basics/Docs/Verification/CoopBasics/README.md).

## Install on your Quest

You need a **Quest 3, its paired phone, a computer, and a USB data cable**. Repeat for each headset. First-time developer setup can take longer than five minutes.

1. **Enable Developer Mode.** In the Meta Horizon phone app, select your Quest → **Headset Settings → Developer Mode → On**. If it is missing, follow [Meta's setup guide](https://developers.meta.com/horizon/documentation/native/android/mobile-device-setup/).
2. **Download an APK** from the table. Install [SideQuest's computer installer](https://sidequestvr.com/setup-howto) with local APK installation support.
3. **Plug the Quest into the computer.** Put it on and accept **Allow USB debugging**. Choose **Always allow from this computer** on your own computer.
4. **Install the file.** In SideQuest, choose **Install APK file from folder**, then select the downloaded `.apk`. Wait for installation to finish. [Detailed help / ADB alternative](Docs/INSTALL.md).
5. **Open the game in VR.** In the Quest Library, find **Unknown Sources** (sometimes under the library filter or Applications) → **Lighthouse Keepers**. You can unplug the USB cable after installation.

**Which apps can live together?** Sprint 2 installs separately as **Lighthouse Keepers S2**. Sprint 3 and the co-op preview both use **Lighthouse Keepers**: installing one replaces the other. To return to Sprint 3, install its APK again. The saved GitHub releases are unchanged.

## Play together — two people

Use the **same co-op preview APK on both headsets**. Both need internet; use Wi-Fi. Decide who is **Host** and who is **Guest**.

| Step | Host | Guest |
| --- | --- | --- |
| 1 | Open the game. Press **HOST CREW**. | Open the game. Press **CREW OPTIONS**. |
| 2 | Read the **new six-character code** at the top of the board to your friend. | Select the code field. Enter your friend's code using the keypad. |
| 3 | Wait for **two names / 2 of 2 keepers** in the ledger. | Press **CLOSE KEYPAD**, then **JOIN CREW**. Typing the code alone does not join. |
| 4 | Press **SIGNAL READY**. | When both names appear, press **SIGNAL READY**. |
| 5 | When both names say **READY**, press **BEGIN WATCH**. | Wait. Both players load the same House. Look for your friend's grey avatar and name. |

**READY greyed out?** You have not joined yet, or the connection is still busy. Check that the ledger shows both names. If joining says **“Game does not exist”**, the host should leave the crew and **HOST CREW again**, then share the **new code**. Do not reuse a code from a screenshot. Keep the host/headset awake while your friend joins. [Troubleshooting](Docs/INSTALL.md#joining-a-friend).

For voice, use Quest party chat or speak in the same room; the game does not provide voice chat.

## Play solo

Sprint 3 and the co-op preview: **PLAY SOLO** works offline. No host, code or Photon connection required.

- **Day 1:** take the wrench from the House bench; repair three F1 pipe couplings and drain the water.
- **Day 2:** pull the F2 generator starter three times; repair the pipe and restore the light.
- **Day 3:** restore power, repair the pipe, and broadcast at the F4 radio: **POWER → TUNE → CODE → TRANSMIT**.
- Finish the board tasks, then sleep at the **F3 bed**. In co-op, **both keepers** must go to bed.

**Controls:** left stick = move · right stick = snap turn · grip = grab · trigger = interact / click menus · left menu button = Watch Menu (return, retry, replay).

## Explore the project

| Looking for | Go here |
| --- | --- |
| Install help and common problems | [Installation guide](Docs/INSTALL.md) |
| Exact downloads, checksums and version differences | [Releases](Docs/RELEASES.md) |
| Open or build the project in Unity | [Run from Unity](Docs/DEVELOPMENT.md) |
| Design and experience goals | [Team design document](Docs/DesignDocument.md) |
| Tests and co-op proof | [Verification report](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/blob/coop/basics/Docs/Verification/CoopBasics/README.md) |
| Branches and safe cleanup plan | [Repository guide](Docs/REPOSITORY.md) |
| Earlier milestones | [History](Docs/History/README-before-coop-preview.md) |

## Team

**Aneesh Vasamreddy** — lighthouse shell and integration · **Eli Gripenstraw** — flood, lobby and solo foundation · **William / Will Qian** — Fusion networking · **Tapan Sidhwani** — station interactions · **Nirvaan Nagaldinne** — integration and demo documentation.

Built with Unity **6000.3.23f1**, OpenXR, XR Interaction Toolkit, URP and Photon Fusion. A Vanderbilt VR Design team project.
