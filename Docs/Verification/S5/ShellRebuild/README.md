# Solid floors survive a shell rebuild — November 19

The pole was removed and its floor openings were filled, but the shell builder still reapplied the old pole-hole meshes during regeneration. A rebuild could therefore reopen the holes, including in the collision meshes. The builder and the manual floor-restoration tool now share the same four UV-preserving solid floor meshes. Missing solid meshes fail the rebuild instead of silently restoring a hole. Existing scene and prefab assets were not regenerated or rebaked in this change.

The Editor regression replaces all four restored floor renderers and all four matching mesh colliders with the old hole meshes on disposable prefab copies, applies the builder correction, and verifies the solid meshes and non-convex colliders return. The existing shell-mesh rebuild check now passes. **27 focused tests passed**, including quality, station placement, pressure valve and timed starter.

## Full EditMode audit

The muted full suite ran twice: **703/711 passed before this correction; 705/712 passed after**, with one new floor regression test and the existing shell rebuild failure resolved. No new failure appeared. The eight previously failing cases were also reproduced against the earlier source `22af3830` in the backed-up peer; the recording sheet required by one test was temporarily copied there and removed after the check. [Names, summaries and baseline comparison](suite-comparison.json).

Seven cases remain open: the authored navigation-clearance test reports the stair liner intruding on its 1 m route; two sabotage warning/retry expectations disagree with current behavior; two legacy objective wording tests reject current steps; the recorded-script and current caption differ; and the networking-boundary test reports avatar/briefing dependencies. Each needs investigation before calling the suite clean. Passing the disposable-prefab checks does not establish that every live stair route is clear, and no Quest or full-night test was performed. No APK was built or installed.

Run `python3 artifacts/s5/modes/check.py CoplanarNudgeTests QualityBaselineTests EquipmentWorldTests PressureValveTests TimedStarterTests` with Unity 6000.3.23f1 and its Pipeline bridge open; the helper mutes the Editor before each test run. Full-suite runs must also mute the Editor. No network fields or version changed.
