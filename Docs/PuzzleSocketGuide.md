# Puzzle socket guide

PuzzleSocket has a station identity, interaction role, required anchor, future-intent inspector note and UnityEvent. SignalInteraction invokes the event; it does not validate participants, solve a puzzle or synchronize anything. Cyan gizmos and labels exist only in the editor. Use the reusable LK_PuzzleSocket prefab and give each new role a clear anchor.

| Station | Locations / roles |
|---|---|
| Plumbing Repair | L1 wrench, brace, leak, repair-state anchor |
| Generator Service | L2 intake, separate starter, can storage |
| Radio Relay | L3 PTT, dial, handheld dock |
| Pressure Sequence | L1 gauges and L3 remote instruction chart |
| Lantern Alignment | L3 bearing information and L4 controls |
| Breaker Coordination | L2 breakers and L4 beacon status |

There is no assumption of exactly two players. Later scripts can count required roles/participants independently. Keep grab props separate from station state. Wrench, oil can and radio demonstrate XRI grabbing; voice transmission, pouring validation and cooperative completion are deliberately absent.

## Basic local interaction authoring (Sprint 3)

The legacy environment now has reusable `ValveHandwheel`, `LeverSwitch`, `RotaryDial`, and `PushToTalkButton` scripts adapted from the team's interaction PRs #48, #54, #55 and #56. These scripts are local interaction hooks. Storm Watch builds its own networked controls and does not consume these events. A socket signal does not prove a repair, change shared power, transmit voice or solve a bearing clue.

To apply the scripts to the existing legacy scenes, open the four `LK_Level01_Plumbing` through `LK_Level04_Lantern` scenes additively, outside Play Mode. Choose **Lighthouse Keepers → Development → Bind legacy basic interactions in loaded scenes**, review the changed scenes and combined mesh assets, then save them. The utility operates only on loaded level scenes; it does not change the gameplay entry or build list. Running it again reuses the same controls. It keeps all sixteen socket identities and anchors, reuses the three red valves and starter lever, adds functional primitive dials/one beacon lever at the existing socket anchors, and attaches press-to-talk to the existing handheld radio. It rebuilds static render batches so moving source renderers do not leave immobile copies behind. Applying the utility and testing its results are separate acceptance steps; source availability does not mean an existing APK contains these controls.

For repeatable batch authoring, invoke Unity with `-batchmode -quit -projectPath <checkout> -executeMethod LighthouseKeepers.Editor.LegacyInteractionAuthoring.BindAndSaveLegacyScenes -logFile <log>`. This entry opens only those four existing levels additively, checks exact socket bindings/identity preservation and Default-layer colliders, saves the four scenes and only their static batch meshes, and restores the initial scene setup. It refuses to run with dirty open scenes. Static meshes are updated in place with existing GUIDs; only levels containing previously static controls need their batches rebuilt. Run `BasicInteractionSceneTests.AuthoredLegacyScenesPreserveSocketContractsAndReachableBindings` afterward to check the saved result.

| Control | Input and release behavior | Existing socket |
|---|---|---|
| Three plumbing handwheels | Grip within 0.65 m, twist around local Y in the positive direction. Reverse motion undoes unfinished progress. Default 1.5 turns; completion latches and signals once. Release/regrip preserves progress. | Plumbing Repair / Wrench |
| Starter lever | Grip and twist around local X towards −60°. On at 60% travel, off below 50%; release retains position. Both state changes signal; `OnStateChanged(bool)` distinguishes them. | Generator Service / Starter |
| Frequency dial | Grip and twist around local Y. Signed angle wraps at 360° and remains usable in either direction. `OnAngleChanged(float)` exposes degrees; release retains angle. | Radio Relay / Frequency dial |
| Handheld radio | Grab the existing radio, then hold Activate (trigger) to press. Trigger release, dropping, or disabling releases it. `OnTalkingChanged(bool)` exposes both edges; the socket signals once per press. | Radio Relay / Push to talk |
| Bearing wheel | Same unlatched dial behavior as frequency control; no target-bearing rule. | Lantern Alignment / Bearing controls |
| Beacon status lever | Same basic lever behavior as starter; no power or beacon rule. | Breaker Coordination / Beacon status |

Fixed controls use `XRSimpleInteractable`: Select/grip holds them in place, and controller wrist rotation turns them. Near direct selection and a ray with the physical controller inside the reach bound are accepted; a far ray's attach point cannot substitute for physical reach. Per-frame jumps above 90° are ignored. Hover and Activate do not turn fixed controls. The scripts do not reposition the rig. Required headset checks include reach, collider targeting, clear turn direction, release/regrip, and seeing moving wheels/spokes without static duplicates.

The Play Mode `BasicInteractionTests` cover signed wheel progress and one-shot completion, regripping, perpendicular swing/reach rejection, lever hysteresis, dial reversal after full turns, and balanced radio press/release/drop/disable. Those tests exercise local XRI events and transforms; they do not establish headset usability or transport synchronization.


## Environment station contract — additive names, October 13, 2026

[BUILT-UNVERIFIED] [StationContract.md](StationContract.md#legacy-mapping--both-names-remain-live) and [StationContract.json](StationContract.json) publish the v0.9 environment attachment contract. All six identities and sixteen exact socket GameObject/role names above remain live; each retains its `Interaction Anchor` child beneath a new additive STN root. The lab has empty aliases only, not duplicate PuzzleSocket components. Existing scene references and UnityEvents are untouched. Bind by named local transforms, never hardcode world positions. No legacy naming is deprecated by this contract.
