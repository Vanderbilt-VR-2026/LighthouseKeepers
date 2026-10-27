# Sprint 3 daily report — 2026-10-20

## Integration status

No consolidated APK or Quest install was produced today. The requested integration ref
`integrate/sprint-3` is absent locally and on `team`; `git fetch team --prune` cannot
write `.git/FETCH_HEAD` in this workspace. An isolated dry-run based on `sprint3/base`
accepted B (simulation) but stopped on D (progression) with a text conflict in
`Docs/DECISIONS.md`. B and D add separate `Assets/Scripts/LK.Core/Simulation` and
`Assets/Scripts/LK.Core/Progression` trees; there is no `LK.Core` path conflict.

Do not merge E until its legacy replication path is removed or changed to consume the
authoritative simulation snapshot. `Assets/_LighthouseKeepers/Scripts/Networking/KeeperReplicationSession.World.cs`
disables `FloodController` and sets `RiseSpeed`, `Paused`, and `SetHeight` directly.
That is a second water-state writer outside `LK.Core/Simulation`.

All checklist items below are pending in a merged Android build on two connected Quests.
Offline checks, source inspection, and an Editor run do not satisfy them.

## Headset checklist — lobby to result

1. From the lobby, have one keeper host and the other join the same crew. Confirm the
   room caps at two, both ready states are visible, and Begin watch remains unavailable
   until both are ready.
2. Begin once from the host. Confirm both keepers enter the same Storm Watch with the
   existing XR rig, no second in-game Host/Join/Begin controls, distinct authenticated
   actor identities, and no early air/watch-time consumption while either player loads.
3. Walk to the first pipe station. Confirm direct hands can grab the brass coupling and
   wrench, the UI ray can operate boards without grabbing world tools, hover/hold/
   progress feedback is readable, and release/regrip or a dropped wrench recovers
   correctly.
4. Complete Day 1 with different keepers bracing and wrenching. Confirm one keeper
   cannot perform both roles; accepted repair stages visibly stop spray, lower water,
   and open the first stair gate for both clients.
5. At the first refuge, make each shared choice once and confirm the same repair-help or
   rescue-protection result appears to both players. Resume together; do not allow a
   client-only choice or an indefinite safe wait.
6. During Day 2, verify the leak warning, generator outage, manual pump stroke (motion,
   not a held handle), generator controls, support grace, power return, and route unlock.
   Release/regrip, invalid/rapid strokes, generator failure, and recovery must preserve
   completed work while resetting only the unfinished step.
7. During Day 3, verify combined pump, generator, and pipe work still requires valid
   contributors. Then verify radio-holder-to-beacon-operator relay for SUN / WAVE / STAR:
   the lantern does not reveal the code, wrong switches reset only unfinished beacon
   progress, and in-game relay controls work without external voice software.
8. Exercise flooding and rescue: submersion air countdown, downed state, yellow belt
   aim/release, catch grip, pull, recovery placement, expiry, both-downed loss, and
   disconnect return-to-lobby explanation. Check reach, catch affordance, fade, and
   recovery for comfort; no forced sustained camera movement.
9. Finish with both keepers alive at the lantern. Confirm beacon feedback, victory only
   when both reach the top, result-board return to lobby, and retry reset behavior.
10. Across the entire run, record Quest model, refresh rate, CPU/GPU frame time, draw
    calls, GC allocations, thermal/sustained behavior, network transport behavior, and
    ten-minute two-player completion time. Record failures separately from editor and
    transport evidence.
