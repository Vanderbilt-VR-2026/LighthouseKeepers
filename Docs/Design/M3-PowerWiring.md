# M3 addition — diegetic tower wiring

Requested after the current scheduling part, before Part C. Extend/replace `Assets/WatchDemo/Runtime/ConnectedMachinePresentation.cs`, not a second parallel system. Its current riser is one cylinder and twelve separately coloured inspection sleeves; the beacon-proof instrument is separate and must stay. No new gameplay or synchronized fields. Editor output remains muted.

1. Generator → F2 breakers → wall riser beside stairs → floor penetrations with metal collars → junction on each floor. Branch to every practical lamp, F1 electric pump/gauge (not the leaking pipe), F4 lamp/motor and radio, PA speakers, and exterior weatherproof feed to the House.
2. Galvanised conduit, elbows, straps about every 0.6 m; short cloth-braided lamp drops on white ceramic insulators. Cast screw-lid boxes with short stencils in the central string table. Use existing aged metals. Keep wall/ceiling edges, doors, windows, pole openings and stair headroom clear. No head-height runs through rooms.
3. Circuit-fed 1.5 s warm travel pulse from generator floor outward; powered lamps follow in order. Unfed circuits stay dark. Junction hum follows power. Breaker trip uses a bang and no more than 30 spark particles. Preserve the existing power/blackout machinery.
4. Repeatable Editor generator from a compact waypoint list; 6–8-sided tubes, a few combined meshes per floor, one shared material/atlas, static batching, no realtime lights or route-blocking colliders. Maximum 15,000 triangles total. The ≤0.3 ms GPU target needs Quest measurement; do not claim it from an Editor frame.
5. Run existing overlap/z-fighting audit and inspect collisions with stairs, walls, doors, poles, props and window apertures.
6. Inspect screenshots: riser through two floors; F1 pump/junction; labelled F2 panel; mid-travel glow; House exterior feed; powered/blackout pair.

Commit and push this step, then resume the ship/doors/M3 queue. Device checks: both-eye clipping/headroom sweep, readable labels, 72 Hz GPU cost and pulse clarity. Do not launch the Quest until explicitly asked.

## Reuse audit / implementation seams

- `ConnectedMachinePresentation.Build` creates `Tower power conduit`, a riser, twelve saddles/sleeves, and the beacon-proof instrument on the radio. Replace only its conduit generation; retain proof lamp/flag semantics. Its current sleeves stay lit based on generator power alone, so the replacement must use `WatchState.HasPower(PowerCircuit)` per branch.
- `PAIntercom` already has six speaker roots (House, F1–F4, outside) and small local conduit pieces. Extend those drops to the generated route; remove duplicate local segments only where the new run takes over. Speakers on tower floors use `WallFixtureMount.Place`, so capture final mounted transforms instead of relying on their old authored coordinates.
- `PipeWallLayout.WallSurface` supplies the curved interior wall and normal; avoid treating the wall as a flat rectangle. Existing riser uses its 1.7 m sample.
- Power panel, beacon controls and some fixtures are built at runtime. The repeatable authoring pass must resolve their final builder anchors (or record them from an authoring preview) before baking waypoint meshes. A simple line between room centres would violate the routing request.
- Keep shader/material per-circuit power driven by existing state, and retain per-floor mesh groups for culling. Per-vertex circuit/route-distance data can supply one-material pulse routing without one renderer/material per strap.

## Implemented route pass (2026-11-10)

The old `Tower power conduit` cylinder/sleeves are replaced. `TowerWiringAuthoring` captures final Lab mounts, saves 129 waypoint runs and 57 terminals in `Resources/TowerWiring/Layout.asset`, and bakes five floor meshes. Rebuild those saved waypoints with **Lighthouse Keepers → M3 → Wiring → Rebuild Saved Waypoints**; after changing station/shell mounts, run the Lab preview and use **Capture Lab Routes (Play Mode)** first. The junction stencil atlas is regenerated from `KeeperStrings.json` (`wiring.box0`–`box4`), with mipmaps/Android ASTC 6×6.

- F2 generator feeder enters the existing breaker cabinet. Four separately driven circuit routes follow wall/ceiling edges; a northwest riser sits beside the stairwell, with paired slab collars and a ceiling offset where the lantern floor narrows. Pump wiring stops at the motor terminal and its instrument, never at a leaking bolt. The existing electric-pump anchors had no visible motor: a non-colliding finned motor housing now makes that load identifiable.
- Existing practical/legacy lamp housings, service bulbs and all six PA horns have feeds. Short lamp drops use twisted dark strands and ceramic stand-offs. The House feed follows the exterior side wall, enters through a gland, and branches under the eaves; gallery drops pass through the opaque lantern header, not its glass.
- Six-sided conduit, eight-sided collars/motor, cast screw-lid junctions and shared stencil atlas. Shared meshes are combined per floor and marked static. No new colliders, realtime lights, or sky/avatar changes.
- The one material reads Pump/Beacon/Radio/Lights from `WatchState.HasPower`; the generator feeder reads the existing engine power. A local 1.5 s UV/vertex-route pulse follows restoration. Unfed branches have no emission; metal also darkens with the existing lights circuit. Central emergency-light restoration uses the same generator-outward floor delay. The accepted radio beacon-proof lamp/flag remains.
- One quiet nearby-junction hum source (original synthesized 100/200 Hz, spatial 0.2–2.5 m) stops when feeds are off. A trip emits 18 sparks, maximum 24 alive; the existing panel supplies its bang. No duplicate trip sound. PA announcements retain their accepted backup behaviour so turning off RADIO never silences essential tutorial/danger guidance; this pass adds their visible cable, not new audio gating.
- No new synchronized fields or gameplay authority. Network remains `LK-M3-NIGHTS1` / v20. A replica test verifies that host circuit snapshots drive the same feeds on the guest and local guest writes remain rejected.

### Budget and evidence

14,634 triangles for the complete wiring/pump presentation; five combined floor meshes, one shared wiring material and one 512×1024 atlas. Sparks use their own existing URP particle shader/material and at most 24 quads (48 additional triangles). One bounded hum source. There are no per-strap renderers or realtime lights. **GPU ≤0.3 ms is a target, NOT VERIFIED on Quest.** No Editor frame rate is substituted for that measurement.

The route audit lists intentional service penetrations and contacts with invisible safety volumes separately. It reports no unexpected contacts, including no window-glass/door/stair-mesh intersections. These are not a substitute for the pending both-eye/headroom sweep. The existing all-scene coplanar audit reports 36 scenes, zero conflicts, zero edits. Screenshots and focused/regression results are in `Docs/Verification/M3/wiring/`.
