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

## Solo and Quest status

The prior candidate passed the user’s one-minute offline Quest Day 1 check:
“yes everything works normally.” The full suites and identical solo Day 1–3
replay are being rerun on `8e3b98a`; final results will be recorded here.
The rebuilt Quest co-op join and moving-avatar headset check are still pending.
A Mac-only pass does not establish headset acceptance.
