# Install Lighthouse Keepers

[← Back to downloads](../README.md#download--play)

## Before you start

You need a Quest 3, the phone paired to it, a Mac or Windows computer, and a USB cable that transfers data. A charging-only cable will not work. Download the **APK** from the project home page; the MP4 is a video, and the source ZIP is for developers.

For current co-op, install **the same Sprint 4 release APK on both headsets**. Different sprint/network versions cannot share a room.

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

1. Install the same release APK on both Quests; turn internet/Wi-Fi on.
2. Host: **PLAY → HOST / PLAY ALONE**. Read the crew code to your friend.
3. Guest: **PLAY → JOIN CREW**. Enter the code and join.
4. Host: when both names appear, press **START**. No READY step.

A host can START alone immediately. Without internet use START on **No connection — Play offline**. If a code no longer exists, the host can create a fresh crew and share the new code. [Back to the current download](../README.md#download--play).
