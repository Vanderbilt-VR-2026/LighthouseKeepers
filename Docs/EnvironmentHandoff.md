# Environment handoff — v0.9 draft

[BUILT-UNVERIFIED] The [station contract](StationContract.md), [JSON](StationContract.json) and `Assets/Scenes/Lab/LK_StationBoxes.unity` are published in [PR #42](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/42). Geometry awaits Aneesh's reconciliation-table sign-off. Footprint sizes and named transforms are stable through v1.0; positions may shift by at most ±0.30 m. The lab is for additive editor inspection, not a playable scene. No production switchover has occurred.

## For Nirvaan — opt-in scene loading

[VERIFIED] At baseline `98a1df2`, the first enabled build scene is `Assets/_LighthouseKeepers/Scenes/Bootstrap/LK_Bootstrap.unity`. Its EnvironmentBootstrap loads the legacy Exterior, Core and four Levels, then makes legacy Exterior active. This branch leaves that configuration untouched; it does not incorporate Aneesh's uncommitted local lobby/camera work.

[PROPOSED] Here is what to add **when you choose to switch over, after these scenes exist and pass acceptance**. Keep your integration root `LK_Bootstrap` and your chosen lobby flow. None of the new environment scenes contains another root player/manager. First load shell scenes in this order, then the relevant dress scenes. Select `LK_Shell_Exterior` as the environmental lighting/sky context when your lighting preset integration is ready.

| Order | Exact proposed build-list path | Role |
|---|---|---|
| 1 | Assets/Scenes/Shell/LK_Shell_Exterior.unity | Persistent exterior shell and atmosphere |
| 2 | Assets/Scenes/Shell/LK_Shell_House.unity | House shell, throat, anchors, collision |
| 3 | Assets/Scenes/Shell/LK_Shell_F1.unity | Plumbing shell and anchors |
| 4 | Assets/Scenes/Shell/LK_Shell_F2.unity | Power/recovery shell and anchors |
| 5 | Assets/Scenes/Shell/LK_Shell_F3.unity | Radio shell and anchors |
| 6 | Assets/Scenes/Shell/LK_Shell_F4.unity | Lantern shell, gallery and anchors |
| 7 | Assets/Scenes/Dress/LK_Dress_Exterior.unity | Exterior loose props |
| 8 | Assets/Scenes/Dress/LK_Dress_House.unity | House loose props |
| 9 | Assets/Scenes/Dress/LK_Dress_F1.unity | Plumbing loose props |
| 10 | Assets/Scenes/Dress/LK_Dress_F2.unity | Power/recovery loose props |
| 11 | Assets/Scenes/Dress/LK_Dress_F3.unity | Radio loose props |
| 12 | Assets/Scenes/Dress/LK_Dress_F4.unity | Lantern loose props |

[PROPOSED] Keep collision-bearing shell scenes resident initially. Preload the current and adjacent floors' dress scenes before entering a stair; profile before introducing shell streaming. Never unload a stair/landing while occupied. The old and new shells overlap in world space: choose one environment set per session, not both. Preserve the old scenes as a fallback; switching loading is your decision and may legitimately never happen. Lab scenes are not production build entries. Your gameplay/station scenes attach through the contract and remain your team's responsibility.

## For Eli — dimensions and lighting seam

[PROPOSED] World heights, in metres, measured from the F1 finished floor:

| Quantity | Value |
|---|---|
| F1 floor / wall depth graduation zero | 0.00 |
| F2 floor / proposed F1-to-F2 spill threshold | 3.30 |
| F3 floor | 6.60 |
| F4 floor / gallery | 9.90 |
| Generator plinth top (F2 + 0.45) | 3.75 |
| F1 electric pump motor housing centre | 0.55 |
| Manual crank pivot (F2 + 1.05) | 4.35 |
| Manual crank grip travel | 4.10–4.60 |

[VERIFIED] No existing flood threshold, water-state writer, power condition or pump behaviour has been edited. These numbers are environment targets, not a simulation migration. In particular, **F2 is not inherently permanently dry**: if water can enter F2, your recovery rules must keep the bilge/isolation controls useful or provide another reachable recovery path. The contract makes no claim that anti-deadlock is already proved.

[PROPOSED] Future presentation component: `BeaconAndMainsLightingGroup`, one public flag `Powered`. Presets: `LightingPreset_Powered` and `LightingPreset_Emergency`, each a complete lightmap set plus realtime-light enable state. No LK.Core dependency or simulation ownership in that component. It is not implemented in this draft. Emergency coverage includes manual bilge, upper isolation, main breaker and beacon control: four control groups. The house oil lamp remains lit in both states. We will use baked/self-lit emergency fixtures within the two-realtime-light budget.

[PROPOSED] Readability calibration uses one fixed powered-reference capture of a 0.18-albedo grey card on F1. Floor minima are 0.12 of reference powered and 0.04 emergency; powered readouts 0.9; text contrast 4.5:1 in linear space. Reference exposure must stay fixed between states; a near-black reference fails calibration rather than making a dark room pass. Headset tuning remains pending.

## For Tapan — bind transforms, retain names

[BUILT-UNVERIFIED] Use the per-station tables in [StationContract.md](StationContract.md#named-attachment-transforms) and the complete `interactiveAnchors` arrays in [StationContract.json](StationContract.json). Each modern anchor is a direct child of its STN root. All sixteen legacy aliases also exist as `STN_…/<exact socket GameObject>/Interaction Anchor`; the [mapping table](StationContract.md#legacy-mapping--both-names-remain-live) preserves all six identity strings and every role verbatim.

| Station | Named interactive transforms / readouts |
|---|---|
| STN_PIPE_MANIFOLD | VALVE_01–06, GAUGE_01, REPAIR_SITE_01, BRACE_01, WRENCH_DOCK_01, LEAK_EFFECT_01, REPAIR_STATUS_01 |
| STN_ELECTRIC_PUMP | MOTOR_HOUSING_LINE, INTAKE_SERVICE |
| STN_PUMP_CONTROL | START, STOP, PUMP_READY |
| STN_WALL_DEPTH_GAUGE | DEPTH_ZERO, DEPTH_READOUT |
| STN_GENERATOR | OIL_INTAKE, OIL_STORAGE, PRIMER, PLINTH_TOP |
| STN_GENERATOR_PANEL | STARTER, GENERATOR_STATUS |
| STN_BREAKER_PANEL | BREAKER_BANK |
| STN_BILGE_PUMP_MANUAL | CRANK_AXIS, CRANK_GRIP |
| STN_ISOLATION_MANIFOLD | UPPER_VALVE_01–04 |
| STN_RADIO | PUSH_TO_TALK, FREQUENCY_DIAL, WALKIE_DOCK |
| STN_FORECAST_BOARD | REMOTE_PRESSURE_CHART, BEARING_INFORMATION |
| STN_BEACON_CONTROL | BEACON_LEVER, RUNNING, FAULT, BEACON_STATUS |
| STN_BEACON_ALIGNMENT | AZIMUTH_WHEEL, ELEVATION_WHEEL |
| STN_TOOL_WALL | TOOL_DOCK_01–02 |
| STN_BRIEFING_BOARD | KEEPER_WATCH |

[VERIFIED] Numeric checks place every declared touch anchor 0.75–1.45 m above its standing floor; repair is at 0.95 m. Passive depth marks/plinth lines are not touch targets. Actual seated/standing reach is not device-verified. The nominal work stance has 0.80 m front clearance; the surrounding radius-0.70 m disc reserves floor space, not reach from every point in it. F1 pump/control and F4 control/alignment share sequential positions; manifold A/C remain separate simultaneous positions. Build hardware inside its box, including moving parts, and do not move/rename/reparent the published transforms. Request any additional anchor from Aneesh.

[VERIFIED] The lab contains no live PuzzleSocket components, repair behaviour or copied UnityEvents. Existing legacy components remain in their original scenes. Binding your system to the new transforms and migrating existing references is an explicit integration step; it is not performed silently by this environment draft.
