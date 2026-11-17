# Hunter foundation — NOT a completed Tier1 hunter

Implemented and checked: `HorrorTuning` asset with requested timings/night quotas; host-only FSM contract for sabotage/stalk/telegraph/quiet/chase/search/retreat; darkness/witness/light/hidden protection; explicit catch feature gate; validated replica-copy contract.17 rule tests pass. This frame is not in WatchSnapshot yet, so network remains LK-S5-LANTERN1/v35.

Baked existing collision geometry into a44KB NavMesh asset. All20 directed House/F1/F2/F3/F4 routes are complete and capsule-clear. A real NavMeshAgent walked House→F1→F2→F3→F4→House at1.2m/s, with no stuck, wall or teleport steps. See `agent-walk.json`. The House's low doorway prevents upright2.3m traversal; navigation uses1.8m and the rig hunches. Production adapter still needs to drive this hunch before crossing.

Walter prototype: one5,373-triangle skinned mesh/material, wet dark oilskin, shoulder patch, joint-driven gait/neck/hunch. Inspected front/side/hunch screenshots. Fixed exposed under-coat skin and fingers accidentally weighted to knees; a rig test guards hand bindings and hunched height (~1.81m). This is a development prototype, not accepted final horror art.

Final focused run:17hunter rules +1rig +2existing form +4voice-scope tests pass. The new PlayMode geometry regression passes. Its first fixture accidentally loaded Night1's closed tutorial door and correctly reported blocked passage; fixed the fixture to Night3 (the hunter's first eligible night). No door/gameplay rule changed. Earlier oversized F3 nearest-point probe snapped to stairs; final sampling refuses wrong-floor snaps.

**Still OFF / unfinished:** live WatchState/snapshot integration, actual keeper LOS/light observations, host NavMesh movement/guest interpolation, warning sound/lamp/music/haptics, live escape gate and headset chase check. No new catches or hunts were enabled. Sprint/hiding/defense/recovery follow in Tier1 order. Existing non-contact apparitions and lantern game remain as before. Do not close#105/#113 or label Tier1 complete.

Reproduce: load actual Night3 in mutedPlayMode, menu LighthouseKeepers/Horror/Bake hunter navigation from PlayMode. `HunterRoutesRespectFloorsAndClearTheActualWalls` is the durable PlayMode check. Run `Tools/Horror/VerifyAgentRoute.cs` through bridge entry `HunterRouteWalk.Run(output)` for actual agent traversal; `Tools/Horror/CaptureWalter.cs`, entry `WalterReview.Run(output)`, for graphics. Tests HunterRulesTests, WalterRigTests, DrownedFormTests. No deviceGPU/comfort or fresh two-client hunter claim.

Fresh soloNight1 from lobby→physical pipe→preview ship→shared-bed flow in solo→Night2 passes, zeroerrors (`solo-night1.json/txt`). This driver retains older day3 screenshot filenames internally; the actual tested watch wasNight1. This is not a new fullsuite or two-Mac hunter check.
