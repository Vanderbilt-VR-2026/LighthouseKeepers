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
