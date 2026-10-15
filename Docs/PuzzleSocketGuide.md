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


## Environment station contract — additive names, October 13, 2026

[BUILT-UNVERIFIED] [StationContract.md](StationContract.md#legacy-mapping--both-names-remain-live) and [StationContract.json](StationContract.json) publish the v0.9 environment attachment contract. All six identities and sixteen exact socket GameObject/role names above remain live; each retains its `Interaction Anchor` child beneath a new additive STN root. The lab has empty aliases only, not duplicate PuzzleSocket components. Existing scene references and UnityEvents are untouched. Bind by named local transforms, never hardcode world positions. No legacy naming is deprecated by this contract.
