# Install Lighthouse Keepers

[← Back to downloads](../README.md#choose-your-download)

## Before you start

You need a Quest 3, the phone paired to it, a Mac or Windows computer, and a USB cable that transfers data. A charging-only cable will not work. Download the **APK** from the project home page; the MP4 is a video, and the source ZIP is for developers.

For co-op, install **LighthouseKeepers-Coop-Preview.apk on both headsets**. An old Sprint 3 submission APK cannot join the shared co-op game.

## One-time headset setup

1. Open the **Meta Horizon** app on the paired phone.
2. Select your Quest, then **Headset Settings → Developer Mode**. Turn it on.
3. If Developer Mode is missing, complete [Meta's developer account/device setup](https://developers.meta.com/horizon/documentation/native/android/mobile-device-setup/), including any account verification it asks for. Then reopen the app. A parent or headset owner should handle account setup for a child.
4. Install [SideQuest on your computer](https://sidequestvr.com/setup-howto), choosing the desktop installer that supports installing a local APK (also called Advanced Installer).
5. Connect the Quest by USB. Put it on and accept **Allow USB debugging**. On your own computer, select **Always allow from this computer**.

## Install the downloaded game

1. Open SideQuest and wait until it shows the headset is connected.
2. Choose **Install APK file from folder** and select the downloaded game APK. Some SideQuest desktop versions also accept dragging the APK into the window.
3. Wait for the installation task to finish successfully. Do not unplug during installation.
4. Put on the Quest. In **Library**, find **Unknown Sources**. Depending on your Quest software, it may be in a filter or Applications view.
5. Open **Lighthouse Keepers**, or **Lighthouse Keepers S2** for Sprint 2. The app should fill your VR view, not run in a flat desktop/Link window. USB is not needed to play after installation.

[Play together: exact host/guest steps](../README.md#play-together--two-people).

## If the APK won't install

| What you see | What to do |
| --- | --- |
| No headset in SideQuest | Keep the Quest awake. Try another data cable or USB port. Check Developer Mode. |
| Unauthorized / waiting for permission | Put on the Quest and accept **Allow USB debugging**. Reconnect USB if the prompt disappeared. |
| Developer Mode is missing | Complete Meta's account setup above using the account that owns/pairs the headset. |
| Windows still cannot see it | Follow Meta's [ADB driver instructions](https://developers.meta.com/horizon/downloads/package/oculus-adb-drivers/). |
| Download is a ZIP | Download the link ending in **.apk**, not “Source code.” |
| App installed but not in the main grid | Find **Unknown Sources** in the Quest Library. |
| Different signature / UPDATE_INCOMPATIBLE | An older build used another signing key. Uninstall **Lighthouse Keepers**, then install the new APK. **Uninstalling erases that app's local data.** Sprint 2 is a separate app. |
| Flat window instead of VR | Exit Quest Link and open the installed standalone game from the headset Library. Use the APK linked in this repo. |

## Joining a friend

- Use the **same co-op APK**, with Wi-Fi/internet on for both players.
- The host presses **HOST CREW** and reads the current code out loud. The guest enters it under **CREW OPTIONS**, closes the keypad, then presses **JOIN CREW**.
- **SIGNAL READY is disabled until you actually join.** Both names must appear in the ledger; entering text is not enough.
- If you get **Game does not exist**, ask the host to leave and create a fresh crew. Enter the new code. We observed a stale code after Photon recovered the host connection; fresh hosting is the tested workaround.
- If the host is a Mac Editor, keep the Mac awake and Unity in Play Mode. Do not let either headset sleep while joining.
- Only the host presses **BEGIN WATCH**, after both players signal ready. This preview supports **two players**, not four.
- Co-op requires the internet. For offline play, choose **PLAY SOLO** instead.
- Use Quest party chat or speak aloud for voice.

## For people who already use ADB

With Android platform-tools installed, open a terminal in the download folder:

```sh
adb devices
adb install -r LighthouseKeepers-Coop-Preview.apk
```

`adb devices` must show `device`, not `unauthorized`. For Sprint 3 or Sprint 2, use that APK's filename instead. Then launch from **Unknown Sources**.

## Switching versions

Sprint 2 (`com.lighthousekeepers.sprint2`) can stay installed beside either other build. Sprint 3 and co-op both use `com.lighthousekeepers.game`; installing one replaces the other. To restore the graded Sprint 3 game, download and install its APK again. No GitHub release needs to be deleted or changed.

Official setup references: [Meta device setup](https://developers.meta.com/horizon/documentation/native/android/mobile-device-setup/) · [SideQuest installation](https://sidequestvr.com/setup-howto) · [SideQuest connection help](https://side.quest/support/700315/unauthorized-unable-to-connect-the-quest-apps-not-installing-cant-see-apps-once-installed-heres-why).
