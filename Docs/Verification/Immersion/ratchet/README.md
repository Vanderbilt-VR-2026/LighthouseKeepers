# Short-stroke wrench follow-up — November 4

Headset feedback: the wrench snap and bolt animation were welcome, but resetting the controller unwound the partial turn, making repair feel like one continuous 180-degree movement. [Playtest note](../../../Playtests/2026-11-04-wrench.md).

Runtime source: `d383fb7`, on `s4/immersion`, follow-up to draft PR #79. In the enabled immersion repair mode, a short tightening stroke adds progress and the reverse stroke freewheels. Every partial degree is kept, including across pulling away/re-seating. Either initial tightening direction is supported. The wrench still moves with the wrist and the bolt stays tightened while the handle comes back. Existing host authority and the serialized state contract are unchanged; use matching updated builds on both clients.

The first-bolt and idle pipe hints now say to use short turns, reset the wrist and keep cranking. The existing total 2/3/4 quarter-turn requirements, click/haptic feedback, snap distance and hand rotations remain unchanged.

## Verification

- EditMode: **238/238 passed**, including short-stroke tests in both tightening directions.
- Full PlayMode: **39/58 passed**. The same 18 known baseline failures remained, plus `PhotonCloudTimeout / ClientTimeout` in `PhotonPeersShareRosterReadyLateJoinKickAndDisconnect`. That test passed on an unchanged targeted retry (**1/1**, 5.24 s). Both immersion interaction tests passed. [Full results/failure names](tests.json), [cloud retry](cloud-retry.json).
- Real graphics solo Days 1–3: **PASS**, zero logged errors, using repeated 30-degree cranks for every pipe repair. Other stations, drainage, bed transitions and actual beacon-loss/retry remain covered by the existing replay. [Result](solo-result.json), [steps](solo-loop-evidence.txt).
- Inspected images: [left-hand tightened bolt](day-one-wrench-LeftHand-turned.png), [right-hand tightened bolt](day-one-wrench-RightHand-turned.png).
- Two-Mac Fusion run: **PASS**. Guest short-crank repairs updated the host; host generator power reached the guest; bed votes/shared sleep advanced both clients into Days 2 and 3; guest fuel pouring still worked. [Result](coop-result.json), [host board](coop-03-guest-bolt-3-host.png), [guest board](coop-03-guest-bolt-3-guest.png). Live Mac+Quest acceptance remains pending.
- APK `Builds/Immersion-d383fb7/LighthouseKeepers-Immersion.apk` built with the repository's `MultiplayerBuild.BuildQuest` pipeline and packaged validation, then installed with `adb install -r` and launched on the connected Quest. [Build/hash](build.json), [install](install-launch.json). SHA-256: `a5d8806e90a7bf4869b71e5934ae6026558afc41a84ac05cc8e401d8c6c4e75a`.


The real XRI replay and two-Mac drivers use repeated **30-degree out-and-back** controller rotations. There is no large continuous sweep or direct mutation of repair progress. Unit tests additionally verify that return strokes do not undo partial tightening or restore leak flow, re-seating preserves progress, and holding still cannot repair. The both-hand PlayMode test finishes an entire coupling with each hand using the same bounded stroke.

Physical Quest comfort remains unverified until the lead tries this build. Automated controller motion does not establish comfortable human reach or grip.
