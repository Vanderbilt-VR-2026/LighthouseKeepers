# Environment handoff — v1.0

[BUILT-UNVERIFIED] The approved v1.0 [station contract](StationContract.md), [JSON](StationContract.json) and station-box lab are published through [PR #42](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/42). Maximum station movement from v0.9 is 0.23 m; footprints and transform names are unchanged. Shell/device acceptance remains separate. No production switchover has occurred.

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

[VERIFIED] Frozen authoring heights, in metres, measured from the F1 finished floor:

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

[BUILT-UNVERIFIED] Presentation component: `BeaconAndMainsLightingGroup`, one public flag `Powered`. Presets: `LightingPreset_Powered` and `LightingPreset_Emergency`, each a complete lightmap set with probe coefficients and ambient state; mains emission switches with the flag. Batch A adds no realtime lamp. No LK.Core dependency or simulation ownership in that component. It is implemented for the isolated Lab; production additive bindings remain a separate integration step. Emergency coverage includes manual bilge, upper isolation, main breaker and beacon control: four control groups. The house oil lamp remains lit in both states. We will use baked/self-lit emergency fixtures within the two-realtime-light budget.

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

[VERIFIED] Numeric checks place every declared touch anchor 0.75–1.45 m above its standing floor; repair is at 0.95 m. Passive depth marks/plinth lines are not touch targets. Actual seated/standing reach is not device-verified. The nominal work stance has 0.80 m front clearance except the approved F4 wall-mounted shared controls (about 0.516 m contact reach); the surrounding radius-0.70 m disc reserves floor space, not reach from every point in it. F1 pump/control and F4 control/alignment share sequential positions; manifold A/C remain separate simultaneous positions. Build hardware inside its box, including moving parts, and do not move/rename/reparent the published transforms. Request any additional anchor from Aneesh.

[VERIFIED] The lab contains no live PuzzleSocket components, repair behaviour or copied UnityEvents. Existing legacy components remain in their original scenes. Binding your system to the new transforms and migrating existing references is an explicit integration step; it is not performed silently by this environment draft.


## Frozen recovery heights and threshold handoff

[VERIFIED] Lowest operable contacts: bilge grip **Y4.10 m** at bottom of rotation; isolation handwheel rim **Y4.35 m**; breaker control **Y4.45 m**; beacon lever **Y10.90 m**. These are geometric authoring bounds, not simulation changes.

[PROPOSED] The F2 defeat threshold must sit below Y4.10 m if the complete bilge rotation must remain dry. Eli may instead declare the bottom of the grip rotation operable underwater; the next geometric bound is approximately Y4.35 m at the isolation handwheel. Include water rise during the defeat-delay timer: triggering below the bound is insufficient if eight seconds of continued inflow carry the water above it before the run ends. Eli owns that operating policy and threshold.

[VERIFIED] F3 remains Ø7.30 m because the incoming main stair outer edge is r3.525 m, already outside a Ø7.00 m room. Landings follow the wall with 0.08 m reveal, 1.30 m radial width and 1.30 m minimum inner-arc length. F4 shared stance is 35° at r1.95. The corbel band occupies Y9.30–9.90 and preserves F3's clear ceiling to Y9.55.

## Batch A shell lab — 2026-10-14

[BUILT-UNVERIFIED] Six `LK_Shell_*` scenes and six initially empty `LK_Dress_*` scenes now exist alongside the legacy environment. Seven original FBX families provide the house/throat, four tower floors, stairs and curved landings, gallery, lantern frame/roof, corbel/drip course and rock base. This is the greybox shell gate, not the detailed machinery or atmosphere batches. No production scene list, loading code, player prefab, simulation or vendor original was changed.

[BUILT-UNVERIFIED] Open `Assets/Scenes/Lab/LK_Lab_Environment.unity` and choose **Lighthouse > Play Environment Lab (temporary)**. This bypasses a configured lobby Play Mode start scene only for this test and restores that setting afterward. The lab instantiates the existing team XR rig; hand behavior and locomotion code are unchanged. The lab spawns inside the house facing the tower passage.

[BUILT-UNVERIFIED] `BeaconAndMainsLightingGroup.Powered` now swaps complete powered/emergency Lab lightmaps, probe coefficients and mains emission. These presets own the whole Lab's global lightmap array. They are not yet a production additive-lighting solution: Nirvaan must coordinate renderer bindings and lightmap ownership before switching environments. Shell scenes themselves are not individually baked; the combined Lab is. There are no station readouts or recovery-control meshes in Batch A, so their dedicated illumination/readability remains later-batch work.

[VERIFIED] The F1 route was adjusted around the stair railing, without changing any station transform, name, footprint or operator position. Curved landings retain the approved 0.08 m reveal. The exported geometry totals 70,452 triangles including all island LODs and non-rendered safety/ramp meshes; this is an authored count, not a measured visible-triangle or draw-call count.

[PROPOSED] Device walk: from the house, enter F1; find the west stair departure; climb every flight to F4; step onto the gallery; return to the house. Observe doorway lips, stair continuity, arrival-floor gaps, railing attachment, headroom and tracking. Stop if there is a fall or collision trap. Record feedback rather than assuming a clearance test proves comfort. This lab has no gameplay stations, storm/ocean presentation or finished textures. The exterior rock perimeter is not approved for free exploration.

[PROPOSED] Follow with powered/emergency visual comparison, renderer visibility profiling between floors, sustained frame timing and memory measurements. A baked occlusion asset does not prove the other floors are culled. The zero-allocation test measures only the new presentation component's steady-state update, not the whole legacy rig or Unity editor.

[VERIFIED] Desktop validation on 2026-10-14: 45/45 Edit Mode tests and 9/9 Play Mode tests passed (2 environment, 7 legacy). The environment test drove the unchanged rig along the house-to-gallery route. Geometry checks passed 1,700 navigation and 2,475 work-position samples. Lighting passed 1,990 samples before the build recheck exposed a measurement-camera occlusion artifact; the measurement camera now disables occlusion and the rerun is recorded separately below. No headset comfort or sustained 72 Hz claim follows from these checks.

[VERIFIED] Final deployment results: all three validators pass (navigation 1,700, work positions 2,475, lighting 1,990 samples). The corrected lighting measurement also passes after occlusion baking. Edit Mode rerun: 45/45; Play Mode: 2/2 environment and 7/7 legacy. Android ARM64/IL2CPP development build succeeded with zero errors and one warning about diagnostic crash-report symbols. Existing Fusion import scheduling emits an additional editor warning; no networking code/package was added.

[VERIFIED] APK: `artifacts/builds/LighthouseKeepers-Environment-BatchA.apk` (approximately 133 MiB; Unity's aggregate build size is not APK size). `adb install -r` returned Success on Quest `2G0YC1ZG32048T`. Direct launch is waiting behind the Quest Guardian/boundary dialog; no running app or climb recording is claimed. Confirm the boundary in the headset, then open Lighthouse Keepers. This installs the isolated preview under the existing app identifier, replacing that device's previous APK without changing the repository's production scene list.

[BUILT-UNVERIFIED] Human climb, headset lighting/comfort, draw calls, culling effectiveness and sustained 72 Hz remain pending. Editor captures show visible wall-lightmap seams and uneven indirect lighting; these remain greybox presentation limitations, not final art acceptance. There is no human climb video yet.

[VERIFIED] Reproduction: use Unity 6000.3.23f1 with `-batchmode -projectPath <repo> -executeMethod LighthouseKeepers.Editor.Environment.EnvironmentBatchAValidation.Prepare -quit -logFile artifacts/logs/environment-finalize.log`; omit `-nographics` for render measurements. Run `-runTests -testPlatform EditMode -testResults artifacts/tests/environment-all-editmode.xml`, and PlayMode separately with `-testFilter LighthouseKeepers.Presentation.Environment.Tests` and `-testFilter LighthouseKeepers.Tests`. Build with `-executeMethod LighthouseKeepers.Editor.Environment.EnvironmentBuild.BuildAndroid -quit`. Exact executed argument arrays, XML, validator reports and logs are local under ignored `artifacts/`. `tools/blender/verify_floor_topology.py` confirms four connected watertight floor meshes; repeated unchanged-input generation preserves scene bytes.
