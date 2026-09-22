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
