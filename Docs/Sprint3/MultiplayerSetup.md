> **Current PR #65 candidate:** the shipped Lab world is solo. Two-keeper gameplay launch is disabled until shared Lab state is implemented. The networking setup below is historical/diagnostic and does not establish current co-op gameplay. See [current Sprint 3 README](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/blob/codex/sprint3-technical-fixes/Docs/Sprints/Sprint3/README.md).

# Sprint 3 multiplayer setup — Session E

STATUS: **[PROPOSED] for integrated/device acceptance.** No APK was produced in this sandbox. The source/API checks below are real; they are not Unity import, Photon-session or headset evidence.

The code extends William's Fusion Host/Client transport and the existing physical lobby board. E0 merges `8c5d1e4` with `3f2e26d` in its history, starting at `sprint3/base` (`4391afd`). No new package is required.

## 1. Project account and App ID

This build uses **Photon Fusion**. There are **no Unity Dashboard services to enable** for its connection flow. A Unity Services project ID is not used for this transport. Keep the existing Unity project association; do not paste a Photon App ID into Unity Services.

1. Open the [Photon Dashboard](https://dashboard.photonengine.com/) and sign in with the team's Photon account.
2. Choose **YOUR → APPS → Development**. Open the team's existing **Fusion 2** application. If the team has no application, choose **CREATE A NEW APP → Photon SDK: Fusion → SDK Version: Fusion 2**, enter `Lighthouse Keepers`, then **CREATE**.
3. Copy that application's **App ID**. Use the same application for the Editor and both headsets. These are the current [official Photon App ID steps](https://doc.photonengine.com/fusion/v2/getting-started/appid-instructions).
4. In Unity **6000.3.23f1**, choose **Tools → Fusion → Realtime Settings**. In the Inspector expand **App Settings** and paste into **App Id Fusion**. This selects `Assets/Photon/Fusion/Resources/PhotonAppSettings.asset`. **Tools → Fusion → Fusion Hub** also exposes the Fusion App ID entry.
5. In the same asset set **Fixed Region** to the same supported region on both builds, for example `us`. Keep **App Version** identical. Leave custom authentication unconfigured for this prototype; the existing Fusion connection needs no player account login screen. Ask the Photon project owner about authentication/CCU errors rather than creating a second App ID for the guest.
6. Unity's **Window → General → Console** must contain no compile errors before building. The installed SDK's own static RPCs still require its existing `LK.Networking` weave setting. E's adapters use the existing runner's bounded data streams and add no weave setting.

The team has no confirmed Vivox-plan access in this session. The lobby displays **“Day 3 requires voice chat — use a party.”** Start external voice and verify that both people hear each other before readying up. In-game voice has not been added.

## 2. Build and install the same APK on two headsets

1. In **Unity Hub → Installs → 6000.3.23f1 → gear/three dots → Add modules**, confirm **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK** are installed.
2. In Unity choose **File → Build Profiles → Android → Switch Platform**. In **Scene List**, `LK_NetworkLobby` must be first, followed by `LK_StormWatch`, `LK_Bootstrap`, and the six environment/content scenes already in this repository.
3. Choose **Lighthouse Keepers → Sprint 3 → Networking → Validate network prefabs**. It checks and bakes only E's `PF_PlayerRig` and `PF_GrabAuthority` assets. It does not open A's hand prefabs or replace the tracked XR Origin.
4. Choose **Lighthouse Keepers → Sprint 3 → Networking → Build Quest APK**. Expected output: `Builds/LighthouseKeepers-Sprint3-Networking.apk`. Treat it as **[BUILT-UNTESTED]** only after Unity reports success. **Build release Quest APK** produces the separate `-Release.apk` without development bypass code.
5. Connect each Quest by USB, keep it awake and accept its USB debugging prompt. From a terminal opened in the repository, run the commands below. Replace `HOST_SERIAL` and `GUEST_SERIAL` with the values printed by `adb devices -l`.

```sh
adb devices -l
adb -s HOST_SERIAL install -r Builds/LighthouseKeepers-Sprint3-Networking.apk
adb -s GUEST_SERIAL install -r Builds/LighthouseKeepers-Sprint3-Networking.apk
```

For one attached headset, the exact install command is:

```sh
adb install -r Builds/LighthouseKeepers-Sprint3-Networking.apk
```

On this Mac, ADB's absolute path is `/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb`. Launch the installed Lighthouse Keepers application from the headset's applications list. Keep both clients on the same source revision, App ID, App Version and region.

## 3. Host, join, ready and recover

1. On the host's board, open **CREW OPTIONS**, enter an optional keeper name, then choose **HOST CREW**. Read the six-character **JOIN CODE** at the top.
2. On the guest's board, open **CREW OPTIONS**, select the code field and use the in-world keypad to enter that code. **BACKSPACE** corrects a character; **CLOSE KEYPAD** returns to the board. Choose **JOIN CREW**. Desktop users can type in the same field.
3. Both boards must show both roster entries. Each person chooses **SIGNAL READY**. **STAND DOWN** revokes readiness. The host's **BEGIN WATCH** must remain unavailable until both are ready; a guest cannot start the watch.
4. The existing Fusion scene manager launches the watch; its bootstrap loads the lighthouse content additively. There must be one local tracked camera per device, one borrowed session runner, and one remote keeper with an oilskin-colored torso and nameplate. The name begins fading past 6 m.
5. **LEAVE WATCH**, host loss or partner loss ends the inherited watch. After returning to the lobby, the original host chooses **RECONNECT** first; the guest chooses **RECONNECT** after the host is online. The code and names are remembered in the running application. Ready up again for a **fresh watch**. This does not restore an interrupted day or migrate the host. Restarting the application clears reconnect memory; enter the host's current code again.

Code entry has a controller keypad. Keeper-name text entry still uses the existing input field; leaving the default name is supported. Reach, keyboard focus and both-eye readability await headset acceptance.

## 4. Test alone: Editor host + Quest client

1. Build/install the development APK from section 2. Keep Unity open on that same revision.
2. Choose **Lighthouse Keepers → Play game**. The Game view opens the crew lobby. Use the mouse to choose **HOST CREW**; write down the displayed code.
3. Put on the Quest, enter that code in **CREW OPTIONS**, and choose **JOIN CREW**.
4. On the Editor board click **SIGNAL READY**. On Quest choose **SIGNAL READY**. In the Editor click **BEGIN WATCH**.
5. The Editor controls remain the inherited desktop rehearsal controls: click Game; **WASD** moves, **Q/E** turns. Near stations: **B** braces, **R** repairs, **P** pumps and **G** services the generator. Desktop keys do not emulate tracked grabbing or prove the physical interactions.
6. Watch the Editor while moving your Quest head and hands. Pick up the Quest wrench and observe it on the Editor. Return to the headset and confirm that the room/camera never moves because the other keeper moved.
7. After D binds the new Day 3 gate, **Lighthouse Keepers → Sprint 3 → Networking → Use strict two-actor gate (Play Mode)** disables its development bypass on the host. One actor's two hands must then fail the two-station gate. Stop/re-enter Play Mode to restore the development default.

`DebugSoloOverride.Enabled` defaults true only in Editor/development code. Release code has no setter or backing field. The two-ready-player lobby rule remains intact; the Editor is the second connection for a one-person rehearsal. The current inherited Storm Watch mechanics are distinct from D's pending Day 3 valve integration.

## 5. Error messages

| Board message | Action |
|---|---|
| Enter the six-character join code… | Copy the complete code from the host. Spaces/short codes are refused before contacting Photon. |
| No crew has that code… | Host first; check code, App ID, version and Fixed Region. Joining never creates a room silently. |
| That crew is full… | Capacity includes the host: two keepers. Leave an old connection before joining another. |
| That watch has started… | Ask the host to return to the lobby, then reconnect. Gameplay late join is closed. |
| That code is in use… | Choose HOST CREW to generate a different code. |
| No internet connection… | Reconnect Wi-Fi; use RECONNECT. A connected Wi-Fi icon alone does not prove Photon is reachable. |
| Connection timed out… / Connection lost or unavailable… | Check internet and the shared Photon settings. RECONNECT becomes available after the runner finishes shutting down. Cancellation is not a second runner. |
| Photon setup needs attention… | Recheck App Id Fusion and Fixed Region in Tools → Fusion → Realtime Settings. |
| The project's Photon player limit is reached… | End an unused test session or ask the project's owner about its player limit. |
| The host left or the connection was lost… | Host reconnects first, then guest; both ready again. |
| Your partner left… | Reconnect to the same code for a fresh watch. |
| Crew transport is missing… | Reopen the production LK_NetworkLobby; do not use the offline lobby preview. |
| Add … to Build Settings… / Could not load the lighthouse… | Restore the normal scene list and rebuild both devices. |

## 6. Headset acceptance — all items remain unrun here

1. On two devices, host/join using the keypad. Both boards show both names and the same readiness. Enter a bad code once and observe a readable error.
2. Ready only one keeper: BEGIN WATCH remains disabled. Ready both: only the host can begin. Stand down once and confirm start is blocked again.
3. Begin together. Both spawn in the house. Move your head and each hand; the partner sees matching movement, one torso and a nameplate. Stand still and inspect remote hands for jitter. The camera stays controlled by its wearer.
4. Grab the wrench; the other keeper sees it move. Keep holding it for over eight seconds: it must not snap back to its table. Release/re-grab. Try grabbing it simultaneously: exactly one keeper retains it; the rejected hand detaches cleanly. Repeat on pump/starter after their final handle bindings are merged.
5. Finish Day 1 together: both boards, leak/pumping feedback and progression agree. After B/D integration, verify the client runs no local simulation and receives the host's snapshots.
6. At Day 3 with the override disabled (or a release build), one keeper holds two controllers on the two stations: progress must be refused. Two distinct keepers holding the required stations must permit repair. Release either station or disconnect its keeper: the gate closes. This item depends on D's gate binding.
7. Interrupt the guest's connection, then the host's in a separate attempt. Observe a readable message, return to the lobby and reconnect in host-first order with the same code. A fresh ready-up must be required and no old avatar/lease may remain.
8. Use standing reach throughout. Check controller/keypad reach, both-eye text, sound/party voice, frame timing and comfort through a full watch. No crouching or forced camera motion is an acceptance condition.

## 7. Latency and loss: what was measured

The deterministic rule model ran for 60 simulated seconds with **80 ms modeled RTT**, **40 ms request travel**, **2,400 heartbeat packets**, and **24 deliberately dropped packets (1%)**. Results: **1,199/1,200** gate samples open after initial delivery; **0/1,200** false solo passes; stale holds refused after the timeout; **1/2** simultaneous grab requests granted; **1/1** old-owner updates rejected. This model exercises the actual gate/William lease rules, not sockets, Fusion retransmission, Unity timing or headset rendering.

**Live Photon pairs tested: 0. Measured live RTT samples: 0. Live packet-loss samples: 0. Day 3 device trials: 0.** No live median/p95 RTT, grab rejection delay or hand jitter value is available. What breaks under real 80 ms/1% conditions is therefore unknown. The reliable pose stream may stall behind a lost packet; buffered interpolation does not prove that this is imperceptible.

To run the actual transport check:

1. Enter Editor Play Mode at the lobby, before hosting. Choose **Lighthouse Keepers → Sprint 3 → Networking → Simulate 80 ms added RTT and 1 percent loss (Play Mode)**. Keep the other peer's simulation off. This sets 0.04 s delay and 0.01 loss probability; measure total RTT rather than equating a setting with measured delay. See [Fusion Network Conditions](https://doc.photonengine.com/fusion/v2/manual/network-project-config).
2. In Hierarchy select **Fusion crew session → Crew Network Metrics → Record Samples**. Host and join from Quest. Collect at least 120 seconds of the `LK_NET_METRIC` lines in Console/Editor.log. Use Fusion Statistics for packet loss; the E logger reports RTT only.
3. Repeat headset checklist items 4 and 6 at least 20 times. Record successful gates/attempts, false solo gates, stuck grabs, median/p95 RTT and observed rejection delay in milliseconds. Capture video for jitter review. A real two-person valve test still needs two independently operated stations.
4. Leave the room; choose **Clear simulated network conditions (Play Mode)**. Stop Play Mode. These menu changes affect runtime config only and do not edit the shared project-config asset.

## 8. Reproducible checks and current evidence

Run these commands from the repository in a normal licensed terminal. `Unity` is not on PATH here, so the exact installed binary is shown. Close another Editor using this same worktree first.

```sh
mkdir -p artifacts
/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -nographics -projectPath . -logFile artifacts/compile.log
/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -assemblyNames LK.Net.EditModeTests -testResults artifacts/net-editmode.xml -logFile artifacts/net-editmode.log
/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath . -runTests -testPlatform PlayMode -assemblyNames LK.Net.PlayModeTests -testResults artifacts/net-playmode.xml -logFile artifacts/net-playmode.log
/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -nographics -projectPath . -buildTarget Android -executeMethod LighthouseKeepers.Net.Editor.MultiplayerBuild.BuildQuest -logFile artifacts/android-build.log
```

Also run the existing local lobby, Fusion lobby-state and grab-authority tests. The old integration fixture's direct-transport/solo-preview assumptions need an authorized update described in [E-to-F](Requests/E-to-F.md); it was not rewritten by E. Run `ReleaseSoloOverrideTests` in a non-development test player as well as Editor; an Editor run alone takes the development branch of its assertion.

Evidence from this session:

| Check | Actual result and limit |
|---|---|
| William integration | Merge commit `5b2032c`; both README additions retained. William's ancestry preserved. |
| Separate C# API compilation | Photon.Realtime, Fusion.Unity, LK.Lobby, LighthouseKeepers.Runtime, LK.Networking and new LK.Net compiled against the installed Unity 6000.3/XRI/Fusion binaries; new Editor helper source also compiled. Does not run Unity import/weaving. |
| Standalone NUnit method harness | 32 selected cases passed; 0 failed; 1 interpolation case requires Unity native math and was not run. Includes William's eight existing lease tests. Existing GameObject-based lobby tests were preserved but not run by this harness. |
| Release variant | Actual new LK.Net source compiled with development/editor symbols absent. ReleaseSoloOverrideTests passed (1/1). Attempting to assign Enabled=true failed compilation with CS0200. This is not a release APK test. |
| Unity batch compile / Android build | Attempted; package manager cannot bind `/tmp/Unity-Upm-*.sock` (`listen EPERM`). Android build never reached BuildPlayer. No APK is claimed. |
| ADB | Installed; `adb devices -l` failed because the sandbox prevents its listener (`Operation not permitted`). No device was enumerated or installed. |
| Headsets and live Photon impairment | Unrun: 0 devices tested and 0 live sessions measured. |

Logs are in ignored `artifacts/`: `compile.log`, `android-build.log`, individual assembly compiler logs, `model-tests.log`, `release-tests.log`, and `mutation-probe.log`. No older APK is substituted for this source.

## 9. Integration boundaries and branch recovery

The new simulation contracts, IAuthority signature, progression director and hand prefabs are absent from E's base. Requests: [A](Requests/E-to-A.md), [B](Requests/E-to-B.md), [C](Requests/E-to-C.md), [D](Requests/E-to-D.md), [F](Requests/E-to-F.md). `FusionSimulationAdapter.cs` is guarded by `LK_SPRINT3_SIMULATION` until B is merged and its actual assembly reference/constructors are confirmed. Enable it only with the physical-command validator installed and the legacy water/progression authority removed from the composed scene. It does not supply a substitute simulation. Day 3's new gate is implemented as a rule/API, but it is not bound into the inherited day director. A's hand-state animator bindings remain pending; the default representation uses fallback gloves.

The linked worktree's shared Git directory is outside the writable sandbox. E therefore kept commits under ignored `artifacts/networking.git` on **sprint3/networking**. The normal worktree pointer still shows the base. Use the committed branch/bundle rather than recommitting the whole apparent worktree diff and losing William's history. All pushes target **team**, never origin.

`git push team sprint3/networking` was attempted and failed with `Could not resolve host: github.com`. Recover the preserved commits from `artifacts/sprint3-networking.bundle` on this machine. In a normal terminal with the base repository available:

```sh
git fetch '/Users/aneeshvasamreddy/.codex/worktrees/5b03/Lighthouse Keepers/artifacts/sprint3-networking.bundle' sprint3/networking:sprint3/networking
git push team sprint3/networking
```

The bundle requires the existing `sprint3/base` ancestry; it contains E's commits and William's unmerged ancestry. It is a source-history handoff, not an APK. The human integration owner can merge `sprint3/networking` into the agreed sprint branch from their normal worktree. No force push is required.
