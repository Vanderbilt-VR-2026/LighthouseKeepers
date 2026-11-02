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

## Validation so far

- Unity 6000.3.23f1 compilation passes.
- EditMode: 215/215 pass (baseline 200/200).
- Focused networking + solo menu/entry PlayMode: 22/22 pass.
- Full after-suite, graphics comparison, APK and actual Editor/Quest shared tasks
  are pending. Model/transport tests alone are not headset co-op acceptance.
- Original baseline full PlayMode has 15 historical fixture failures; they remain
  explicitly recorded rather than hidden by changing the build list.

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
