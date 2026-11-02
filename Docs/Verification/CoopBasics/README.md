# Co-op basics verification

## Two independent Mac Editors — PASS

Unity 6000.3.23f1; runtime source `8e3b98a`; normal Fusion HOST CREW / join code /
READY / BEGIN WATCH. Actual cloud session `D8QFXM`, fixed region `us`, two clients
in separate project directories and OS processes. No headset participated in this
run. Both clients loaded `LK_Lab_Environment`; the host applied all shared rules.

The external test driver moved Editor controllers through actual XRI grabs,
wrist rotations and starter pulls. It positioned the Editor rig between stations;
this proves networking and interactions, not human stair traversal or headset feel.
It did not assign task-completion flags. Board evidence uses each client's own
camera moved temporarily to the same readable board view; the avatar screenshots
use the actual House spawn cameras facing one another.

| Check | Host view | Guest view |
| --- | --- | --- |
| Other keeper visible in House | [Host sees Guest](02-house-avatar-host.png) | [Guest sees Host](02-house-avatar-guest.png) |
| Guest repairs first bolt; both boards show 1/3 | [Host board](03-guest-pipe-bolts-1-host.png) | [Guest board](03-guest-pipe-bolts-1-guest.png) |
| One keeper waits for the other at bed | [Host waiting](04-waiting-for-other-keeper-host.png) | [Guest waiting](04-waiting-for-other-keeper-guest.png) |
| Day 2 generator off, emergency lights | [Host F2](05b-generator-emergency-host.png) | [Guest F2](05b-generator-emergency-guest.png) |
| Host's three starter pulls tick both boards | [Host task](06-host-generator-complete-host.png) | [Guest task](06-host-generator-complete-guest.png) |
| Power on, lights warm | [Host F2](06b-generator-warm-host.png) | [Guest sees host at generator](06b-generator-warm-guest.png) |
| Both finish Day 2 and wake on Day 3 | [Host Day 3](08-both-wake-day-three-host.png) | [Guest Day 3](08-both-wake-day-three-guest.png) |

All linked screenshots were opened and inspected. The run also checked that own
avatars are hidden, the remote avatar has two tan hand meshes and no cameras or
audio listeners. A separate head-tilt input left the remote torso upright
(head up dot 0.785, torso up dot 1.0). Guest departure removed the avatar and left
the host playing with the explicit disconnected message.

[Machine-readable observations](mac-evidence.jsonl) include both clients' rule
snapshots. [Runtime source comparison](runtime-source-match.json) confirms that
the guest's copied runtime changes match the committed source. Full attempts,
logs and all frames remain in `artifacts/coop/mac-only/`; failed attempts were
retained. The successful continuous run reports `passed: true`.

## Bugs exposed by real two-client execution

Continuous pose, prop and watch messages previously accumulated in the reliable
transfer queue: guest state fell tens of seconds behind. The fix reuses the
existing Fusion channel but coalesces the latest sample per actor/prop and allows
one acknowledged batch in flight. Discrete commands and ownership decisions
remain reliable. Packet parsing and stale interaction checks stay bounded.
Remote visuals retain their last pose during a brief packet delay rather than
blinking; stale hands still cannot interact.

An RPC alternative encountered an access error inside the installed Fusion SDK
and was removed. No SDK, package, scene, local hand rotation or solo rules changed.
An initial test-driver reach error was corrected: the driver now approaches the
bench before grabbing, waits for the host to observe the hand and grant ownership,
then carries the wrench to the pipe. Reach checks were not weakened.

## Regression checks — final runtime `8e3b98a`

Unity 6000.3.23f1, same full suites and Photon-cloud opt-in as the untouched baseline.
[Exact results and failure names](regression-results.json):

| Suite | Untouched baseline | Final candidate |
| --- | --- | --- |
| EditMode | 200 passed, 0 failed | 218 passed, 0 failed |
| Graphics PlayMode | 25 passed, 15 failed | 38 passed, same 15 failed |

Every previously passing test still passes; no new failures. The existing failures
are BootstrapIntegration (1), FirstWatch (5), PlayerTraversal (6), and StormWatchScene
(3). Unrelated fixtures were not changed. Full XML/logs remain in
`artifacts/coop/final/`; the baseline is preserved in `artifacts/coop/baseline/`.

The identical graphics solo harness passed with **zero logged errors**: PLAY SOLO,
Day 1 three bolts/drain/bed, Day 2 generator/pipe/bed, Day 3 actual 75-second beacon
loss/retry, POWER/TUNE/CODE/PTT, water drain, board 3/3, and bed into the existing
Day 4 placeholder. [Recorded assertions](solo-final-evidence.txt). No Photon crew
was created by this solo run. This uses the same injected XRI inputs and test-only
pipe seed as the baseline, with no authored asset changes.

I opened the before/after House, emergency F2, beacon-failure, tuned-radio,
completed-Day-3 and next-day frames. Layout, controls, task state and solo look
are preserved. Pulse phase, moving rain and frame timing differ; the images are
not claimed to be pixel-identical. Serialized scene/prefab/material/day assets,
project settings, packages and local HandRig source have no diff from the base.

| Solo comparison | Before | After |
| --- | --- | --- |
| House | [Baseline](solo-before-house.png) | [Final](solo-after-house.png) |
| Radio locked | [Baseline](solo-before-radio.png) | [Final](solo-after-radio.png) |
| Day 3 complete | [Baseline](solo-before-day-three-complete.png) | [Final](solo-after-day-three-complete.png) |

## Quest install — successful; co-op acceptance — NOT YET PROVEN

The prior candidate passed the user's one-minute offline Quest Day 1 check:
“yes everything works normally.” The new APK was built from clean runtime commit
`8e3b98a655617d4ea4d4e3b9d8728539a896e68f`, using the repo's ship pipeline:
Unity 6000.3.23f1, development IL2CPP, zero errors and seven warnings.

- File: `LighthouseKeepers-Coop-8e3b98a.apk`, 83,369,234 bytes.
- SHA-256: `76f2e69b9f82891e300e8657eac664f3208b41a1ddfeec14111188aecd78b9fa`.
- Package: `com.lighthousekeepers.game`, version `0.1.0` (1).
- `adb install -r` succeeded; reading the installed `base.apk` back from the Quest
  produced the identical SHA-256. The app launched and OpenXR reached FOCUSED.

A fresh Editor host opened room `JFPTKH`, region `us`, and displayed the code in
large type. During the monitored 60-second interval the roster stayed at **Mac Host,
1/2**. The Quest had validated Wi-Fi. Logcat contained no Photon/Fusion connection
activity or Unity exception; tracking-service permission warnings were present,
but their effect has not been established. ADB's screen capture was black and is
not treated as headset visual proof. Logs and room observations are in
`artifacts/coop/final/`.

The first check is to close the code keypad and explicitly press **JOIN**, then
**SIGNAL READY**. No observed connection attempt means there is not enough evidence
to blame a region/version mismatch or claim a successful Quest join. The requested
headset sighting of the grey keeper and Editor capture of moving Quest hands remain
pending. A Mac-only pass does not establish headset acceptance.

Guest radio end-to-end operation has not been exercised in the paired Mac run;
its replica/command paths are implemented and covered in model tests. The paired
run specifically proves guest pipe, host generator, shared bed and departure.
