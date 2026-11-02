# Co-op basics — Lab lighthouse

Branch `coop/basics`; never merged or pushed to main. Baseline discovery and the
untouched solo results are in [COOP-BASELINE.md](COOP-BASELINE.md).

## Implementation under verification

`ShipWorld.CoopEnabled` is ON here. Only a connected normal crew gets the shared
Lab adapter. PLAY SOLO creates no runner or adapter and keeps its original rules.
Normal HOST CREW / code / CREW OPTIONS / READY / BEGIN WATCH launches at most two
keepers into the same Lab used by solo, with House spawns 1.5 m apart. Photon crew
connections use fixed region `us`. Historical practice remains separate.

Reused William's runner, keypad/room protocol, interpolated 20 Hz keeper poses,
prop layout/generation validation, first-grab leases and release physics. Reused
`coop/two-headsets` tan remote hand visuals, two spawns and host snapshot seam.
Eli Gripenstraw's latest main fixes supply the accepted solo lobby, offline entry
and Watch Menu; his independent network proof informed receive-side verification.
Remote keepers have no active camera, listener, UI or gameplay controllers.

The host owns day/time/water/power/beacon/lighting/task state. Guest starter,
radio and bed/retry/replay inputs use the existing reliable channel. Commands
carry scene nonce, host world, day/retry revision and monotonic sequence. The
host checks authenticated sender, bounds, rate, reach and prop lease where
applicable. The host measures pipe contact/twist from the leased wrench pose;
client models cannot finish tasks independently. Wrench/mic ownership uses the
existing exclusive lease; release returns simulation authority to the host.
Guest microphone trigger heartbeats expire after 0.5 s and require the mic lease.

Every connected keeper must vote at the bed after tasks are complete. The board
shows the waiting count; the host's existing sleep coroutine advances the day
and the guest follows state/fade snapshots. Departure removes that keeper's
vote/props/avatar. On host loss the guest retains the full latest rule snapshot
(including unfinished ratchet and remaining bolt order) and continues alone.
Intentional Watch Menu return still goes to the lobby.

The Editor uses WASD and right-mouse look; inactive XR hands use head-relative
pose offsets. Active test/controller hands replicate their actual transforms.
No accepted hand visual rotations, scene geometry, day assets or look assets
were changed. A connected-crew-only dev readout shows region/code/count/names.

## November 2 proof

Two independent Mac Editors passed normal lobby -> same House, guest pipe bolts
on both boards, host generator/power on both, two shared bed transitions through
Day 3, and guest departure with host continuation. See the
[paired screenshots and evidence](../Verification/CoopBasics/README.md).
Runtime source `8e3b98a` also adds the requested grey remote torso/head/visor and
name tag; the owner does not see their own avatar. Continuous samples now use
bounded, acknowledged latest-sample batches after the real test exposed a backlog.
Final full EditMode: 218/218 pass. Full PlayMode: 38 passed, the same 15 baseline
failures; zero regressions or new failures. The identical solo graphics Day 1–3
loop, loss/retry and final bed passed with zero logged errors; compared screenshots
preserve the solo layout/look. The exact runtime APK was rebuilt and installed;
the installed bytes match SHA-256 `76f2e69b9f82891e300e8657eac664f3208b41a1ddfeec14111188aecd78b9fa`.
Quest co-op remains unproven: fresh room JFPTKH stayed at Mac Host only during the
60-second monitor, with no Quest Photon/Fusion connection activity. See the linked
report for observations and explicit remaining acceptance checks.

## Earlier candidate validation

- Unity 6000.3.23f1 compilation passes.
- EditMode: 215/215 pass (baseline 200/200).
- Full graphics PlayMode: 38 passed, 15 failed; baseline 25 passed, the same 15
  failed. Every previously passing test remains passing; zero new failures.
  Actual Photon cloud lobby tests were enabled in both runs. Legacy Bootstrap,
  First Watch, traversal and Storm Watch fixture failures remain recorded in
  `artifacts/coop/after/comparison.json`; unrelated failures were not repaired.
- The identical external graphics replay passed Day 1 -> Day 2 -> Day 3 actual
  beacon loss/retry, physical XRI station inputs, board 3/3 and bed -> Day 4 with
  zero logged errors. Inspected House, pipe, emergency F2/F4, failure, tuned radio,
  transmission and completed-board screenshots against baseline. Layout and look
  are preserved; moving rain, pulse phase and timing prevent pixel identity.
  Baseline gameplay assertions passed too, but its raw result retains an unrelated
  Editor search-index exception; see the baseline report.
- Candidate APK built from clean `3a17683abf50cb9d2ba5334b01dcce18dc7c5b1f`:
  Unity 6000.3.23f1, development IL2CPP, 0 errors / 7 warnings, 83,341,122 bytes.
  SHA-256: `0b2cbca70b6b7feec15f2788c66e0e42b5946ed4ef0717e754244173da00c0bc`.
  Installed successfully on Quest 3 and launched with Wi-Fi disabled for the
  required user solo check. User confirmed "yes everything works normally" after
  the requested one-minute Day 1 check. Live Editor/Quest shared tasks are pending; automated model/transport checks do not establish acceptance.
- Evidence: `artifacts/coop/baseline/`, `artifacts/coop/after/` and
  `artifacts/coop/device/`. No serialized scene/day/material/hand assets changed.

## Live test order

1. Install the verified candidate. First check PLAY SOLO Day 1 on Quest for one
   minute, including offline entry, before testing crew mode.
2. Editor hosts with HOST CREW and reads its six-character code. Quest opens
   CREW OPTIONS, enters that same code, and joins. Both select SIGNAL READY.
3. Editor presses BEGIN WATCH. Verify two avatars in the same House; move Quest
   hands and inspect the received motion in the Editor.
4. Guest completes a task, then host completes a task; check both boards after
   each. Both go to the F3 bed; check the same next day/fade on both.
5. Inspect Unity console and adb logcat, and capture each acceptance case.

Do not call this accepted until those live checks have been performed.
