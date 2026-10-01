# Sprint 1 headset walkthrough repairs — September 29

[VERIFIED] The project lead confirmed walking works after the controllers were awakened. No movement bindings or XR packages were changed for that report.

[BUILT-UNVERIFIED] Targeted scene edits follow the subsequent headset feedback. They are not a replacement environment or a new sprint. William's recovery scene and Eli's flood/lobby work are preserved.

| Area | Change awaiting headset review |
|---|---|
| Entrance | Disable the two exterior branch props inside the house; keep the locked storm door and its brace. Move coats to an end-wall rack with hooks and place boots beneath them. |
| Player | Hide the visible stabilized chest-proxy renderer in Bootstrap; retain the component and XR rig. Hands are unchanged. |
| Plumbing | Remove the decorative motor from the playable room. Add floor-fed pipe risers, collars, flanges, repair-run supports, gauge takeoffs and valve feed connections. Replace solid valve discs with red handwheel rings/spokes. Add lamp hangers. |
| Second floor | Disable room dressing, generator/motor assemblies and generator hum. Preserve architecture, sockets, flood threshold, lights and acoustic systems. Source prefabs and vendor files are not deleted. |
| Stairs/core | Separate invisible smooth collision from solid visual treads. Narrow the spiral's outer radius from 2.60 to 2.30 m while retaining a 1.30 m stair width; extend upper annular floors inward from radius 2.65 to 2.35 m. Reposition shaft guards; add floor-height arrival landings at all three upper levels. No global scene scaling. |
| Communications | Move bed and desk toward the wall, add bunk legs, remove obstructive chair/shelving, and move radio contents and their socket positions together. Add lamp hangers. |

[BUILT-UNVERIFIED] Approximate radial furniture clearances: bed from 0.18 m to 1.22 m; radio desk from 0.31 m to 1.26 m. These are simple geometry cross-sections, not claims of comfortable headset reach. A sampled 0.55 m capsule route and stair-support/headroom checks provide additional automated evidence.

[PROPOSED] Acceptance walkthrough: look down and around for the former headset-following block; inspect the blocked entrance door and missing branch bars; walk the first stair flight while watching the treads; walk off the top stair onto the lantern floor; inspect the empty second floor; reach the bed and desk; inspect pipe connections down to the floor and the valve/gauge attachments. Report remaining floating objects or snag points by floor.

[UNRESOLVED] Final headset acceptance, sustained performance, visual legibility and comfort remain pending. This does not implement functional pipe puzzles or a generator game. Reused procedural architecture was repaired through Unity Editor tooling; no vendor source files or packages were changed.

[PROPOSED] Reproduction order: WalkthroughRepairs.Apply, FinishTreads, AnchorFixtures, ConnectValveFeeds (includes BakeAndPreview). Tools preserve scene GUIDs and use revision markers to prevent duplicate additions. Do not rerun old full-environment generators over these authored scenes.

## Automated evidence

[VERIFIED] 25/25 Edit Mode tests passed after the final pipe edits (artifacts/tests/editmode-walkthrough.xml). New tests sample support and 0.55 m capsule headroom along all three flights, support across each upper landing, collision-only stair ramps and the communications circulation route. One Play Mode integration test passed after structural repairs, retaining six content scenes, one XR Origin/listener/flood authority, 16 sockets and four thresholds. Final pipe-feed edits did not move sockets or change runtime logic.

[VERIFIED] Final CPU light bake completed with eight lightmaps. Five editor review images are under artifacts/verification/walkthrough-previews/. They were inspected; they do not prove stereo quality or headset comfort. No new realtime lights or runtime update scripts were added; new pipe/tread geometry is statically combined. Quest performance remains unmeasured.

[VERIFIED] The preceding repair APK was installed and inspected by the user. The refinement below supersedes it; installation of that newer APK is pending.

## September 29 headset refinement

[VERIFIED] User reported walking works and praised the repaired pipe appearance and stair texture. They requested a smaller, player-facing pipe assembly, two-sided valve wheels, thicker floors, seated railings, and attached ceiling fixtures.

[BUILT-UNVERIFIED] `WalkthroughRepairs.RefineAfterHeadset` groups the plumbing manifold and associated puzzle anchors at a floor-level pivot, rotates it 180 degrees toward the room and uniformly scales it to 0.75. The room and flood geometry are not scaled. Valve rings now have 35 mm physical depth (before assembly scale). Floor slabs and arrival landings extend 180 mm downward; walking elevations and existing stair materials remain unchanged. Stair posts extend into their treads with mounting shoes, and the inner handrail follows continuous 11.25-degree segments.

[BUILT-UNVERIFIED] `MountCeilingFixtures` adds steel suspension rods and mounting roses to existing lamp fixtures, using their renderer bounds and the authored ceiling elevations. Existing lamp sources remain in place. `RefreshClosedSurfaces` regenerates closed floor/wheel meshes from triangle topology, including Unity UV-unwrapped sources, then rebakes lighting. No vendor assets, input settings, packages or engine versions changed.

[PROPOSED] Headset check: view the pipe controls from the aisle and walk around the wheels; inspect lamp mounts and floor undersides while climbing; cross each landing. Confirm the smaller station remains comfortably reachable. Editor geometry checks do not establish stereo appearance or comfort.

[VERIFIED] Initial refinement Edit Mode run passed 27/27 (`artifacts/tests/editmode-refinements.xml`). Two editor-helper compilation mistakes during authoring (missing local Polar helper and treating void Pipe as a return value) were corrected before scene execution. Closed surfaces now use triangle topology rather than assuming UV-unwrapped vertex ordering. `ExposeValveControls` additionally moves wheels and spokes 0.5 m toward the aisle and rebuilds connected stems so the mains do not obscure them.

## September 30 final validation

[VERIFIED] Final Edit Mode: 27/27 passed, `artifacts/tests/editmode-refinements.xml`; Play Mode: 1/1 passed, `artifacts/tests/playmode-refinements.xml`. Unity compilation succeeded. Final lighting bake: nine lightmaps. Android development build succeeded with zero errors and one diagnostic-symbol warning; report `artifacts/verification/integration-build.txt`, log `artifacts/logs/android-refinements.log`. APK: `Builds/LighthouseKeepers-Sprint1-Environment.apk` .

[VERIFIED] Commands used with Unity 6000.3.23f1: `-batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults artifacts/tests/editmode-refinements.xml -logFile artifacts/logs/editmode-refinements.log`; repeat with `PlayMode` and the corresponding playmode filenames. Android: `-batchmode -quit -buildTarget Android -projectPath . -executeMethod LighthouseKeepers.Editor.Sprint1Integration.BuildDiagnosticAndroid -logFile artifacts/logs/android-refinements.log`.

[UNRESOLVED] New APK not installed or recorded: ADB reports no devices and the user elected to test later. All new visual changes await headset acceptance; no 72 FPS or stereo/comfort claim. No commit or push performed for these repairs.
