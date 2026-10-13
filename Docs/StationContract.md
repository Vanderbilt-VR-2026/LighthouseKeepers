# Station contract — 0.9-draft

**DRAFT. Footprints and operator positions are stable enough to build against. Exact world positions may shift by up to ±0.3 m before v1.0. Build inside the boxes and bind to the named transforms; do not hardcode world coordinates.**

[BUILT-UNVERIFIED] Environment attachment envelopes, not playable stations. Published from team main `98a1df2`. No production scene, rig, simulation, build list, package or teammate file is changed. Final geometry awaits Aneesh’s dimension-table sign-off.

## Ready to build against

- Aneesh owns every mesh, anchor transform and transform name in this contract.
- Bind to named child transforms; never move, rename or reparent them. Request missing anchors from Aneesh.
- Footprint dimensions and transform names are promises through v1.0. Positions may shift by at most 0.30 m; keep implementations local to their STN root.
- Report an insufficient footprint now. Do not silently scale the root or put controls outside the box.
- Any contract change increments its version and is announced in its PR. Nirvaan alone chooses the production switchover.
- Load `Assets/Scenes/Lab/LK_StationBoxes.unity` additively into your own test scene. It has no rig, camera, gameplay components or physical collision. Its visual floor markers are not a playable floor.
- Six legacy station identities, all sixteen exact GameObject names/role strings, and each child named `Interaction Anchor` remain live. The lab provides empty transform aliases only. Existing serialized events/references remain in the untouched legacy scenes; automatic migration is not claimed.

## Coordinate and footprint convention

One unit is one metre; +Y up, +Z north, clockwise azimuth from north. House centre is (0, 0, -7.35). Station rotations are Unity yaw; local +Z faces the operator. Every station root has unit scale. A footprint starts at local `(-W/2, 0, 0)` and ends at `(W/2, H, D)`; it is **not** a centre-pivoted cube. All hardware, flanges, parts and moving sweeps must fit inside it. Elevated pump-panel root is deliberately at Y=1.0. Child anchor coordinates are relative to their station root.

Work discs reserve radius 0.70 m of clear floor; front clearance is at least 0.80 m at the nominal stance. They do not promise all controls are reachable from every edge of that disc. F1 pump/control and F4 control/alignment deliberately share one sequential stance each. Other work discs are separate. Standing operation is mandatory; seated comfort and actual reach await headset testing.

## Floor plans

```text
                         NORTH +Z
    House: tools W | north passage | briefing E
    F1:    stair W | clear centre  | manifold NE, pump ESE
    F2:    stair W | isolation S   | generator NNE, panels E, bilge SE
    F3:    stair W | clear centre  | radio NE, forecast E; loose props later
    F4:    stair W | central lens | shared controls NE, gallery exit E
                         SOUTH -Z
```

These diagrams are orientation aids; JSON coordinates and the matching lab boxes are authoritative. Stair, route and room dimensions are proposals, not authored walkable geometry.

### House

| Station / owner | World anchor XYZ / yaw | Envelope W×H×D | Work positions |
|---|---|---|---|
| `STN_TOOL_WALL` — Tapan | (-2.4, 0, -5.05) / 180° | (2, 2.2, 0.9) | WP-HOUSE-A |
| `STN_BRIEFING_BOARD` — Nirvaan | (1.8, 0, -5.05) / 180° | (1.2, 2.1, 0.12) | WP-HOUSE-B |

| Work position | World XYZ | Facing | Radius |
|---|---|---|---|
| `WP-HOUSE-A` | (-2.4, 0, -6.75) | 0° | 0.70 m |
| `WP-HOUSE-B` | (1.8, 0, -5.97) | 0° | 0.70 m |

### F1

| Station / owner | World anchor XYZ / yaw | Envelope W×H×D | Work positions |
|---|---|---|---|
| `STN_PIPE_MANIFOLD` — Tapan | (2.475, 0, 2.475) / 225° | (3.6, 2.8, 0.45) | WP-F1-A, WP-F1-C |
| `STN_ELECTRIC_PUMP` — Eli / Tapan | (3.263, 0, -1.521) / 295° | (1, 0.95, 0.5) | WP-F1-B |
| `STN_PUMP_CONTROL` — Eli | (3.036, 1, -1.416) / 295° | (0.65, 0.5, 0.25) | WP-F1-B |
| `STN_WALL_DEPTH_GAUGE` — Eli | (1.317, 0, -3.618) / 340° | (0.3, 2.4, 0.03) | Passive; no reserved stance |

| Work position | World XYZ | Facing | Radius |
|---|---|---|---|
| `WP-F1-A` | (2.157, 0, 1.025) | 45° | 0.70 m |
| `WP-F1-C` | (1.025, 0, 2.157) | 45° | 0.70 m |
| `WP-F1-B` | (2.085, 0, -0.972) | 115° | 0.70 m — shared sequential |

### F2

| Station / owner | World anchor XYZ / yaw | Envelope W×H×D | Work positions |
|---|---|---|---|
| `STN_GENERATOR` — Tapan / Eli | (0.919, 3.3, 3.429) / 195° | (2.1, 2.3, 0.75) | WP-F2-B |
| `STN_GENERATOR_PANEL` — Eli | (3.031, 3.3, 1.75) / 240° | (0.8, 1.7, 0.25) | WP-F2-A |
| `STN_BREAKER_PANEL` — Eli | (3.447, 3.3, -0.608) / 280° | (0.75, 1.75, 0.25) | WP-F2-D |
| `STN_BILGE_PUMP_MANUAL` — Tapan / Eli | (2.008, 3.3, -2.867) / 325° | (1.1, 1.5, 0.65) | WP-F2-C |
| `STN_ISOLATION_MANIFOLD` — Tapan | (-0.15, 3.3, -1.3) / 0° | (1.2, 1.7, 0.45) | WP-F2-E |

| Work position | World XYZ | Facing | Radius |
|---|---|---|---|
| `WP-F2-B` | (0.518, 3.3, 1.932) | 15° | 0.70 m |
| `WP-F2-A` | (2.122, 3.3, 1.225) | 60° | 0.70 m |
| `WP-F2-D` | (2.413, 3.3, -0.425) | 100° | 0.70 m |
| `WP-F2-C` | (1.176, 3.3, -1.679) | 145° | 0.70 m |
| `WP-F2-E` | (-0.15, 3.3, -0.05) | 180° | 0.70 m |

### F3

| Station / owner | World anchor XYZ / yaw | Envelope W×H×D | Work positions |
|---|---|---|---|
| `STN_RADIO` — Nirvaan | (1.8, 6.6, 3.118) / 210° | (1.6, 1.5, 0.7) | WP-F3-A |
| `STN_FORECAST_BOARD` — Nirvaan | (3.65, 6.6, 0) / 270° | (1.4, 2, 0.1) | WP-F3-B |

| Work position | World XYZ | Facing | Radius |
|---|---|---|---|
| `WP-F3-A` | (1.05, 6.6, 1.819) | 30° | 0.70 m |
| `WP-F3-B` | (2.75, 6.6, 0) | 90° | 0.70 m |

### F4

| Station / owner | World anchor XYZ / yaw | Envelope W×H×D | Work positions |
|---|---|---|---|
| `STN_BEACON_CONTROL` — Eli / Nirvaan | (1.88, 9.9, 2.163) / 215° | (0.5, 1.6, 0.25) | WP-F4-SHARED |
| `STN_BEACON_ALIGNMENT` — Tapan | (1.389, 9.9, 2.507) / 215° | (0.5, 1.6, 0.25) | WP-F4-SHARED |

| Work position | World XYZ | Facing | Radius |
|---|---|---|---|
| `WP-F4-SHARED` | (1.032, 9.9, 1.474) | 35° | 0.70 m — shared sequential |

## Named attachment transforms

Names are case- and space-sensitive. Modern anchors are direct children of the STN root. Legacy aliases are `STN_…/<exact socket name>/Interaction Anchor`; use either path without renaming either. No interaction state machines are included.

### STN_PIPE_MANIFOLD

No kneeling required. A and C are simultaneous roles. Pressure instrument and repair indicator remain visible from both positions. Envelope includes every flange and wheel.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `VALVE_01` | (-0.8, 1.15, 0.45) | handwheel | WP-F1-A |
| `VALVE_02` | (0.8, 1.15, 0.45) | handwheel | WP-F1-C |
| `VALVE_03` | (-1.05, 1.05, 0.45) | lever | WP-F1-A |
| `VALVE_04` | (1.05, 1.05, 0.45) | handwheel | WP-F1-C |
| `VALVE_05` | (-0.55, 1.3, 0.45) | handwheel | WP-F1-A |
| `VALVE_06` | (0.55, 1.3, 0.45) | lever | WP-F1-C |
| `GAUGE_01` | (-0.8, 1.4, 0.45) | readout | WP-F1-A |
| `REPAIR_SITE_01` | (0.8, 0.95, 0.45) | repairSite | WP-F1-C |
| `BRACE_01` | (-0.8, 1.05, 0.45) | brace | WP-F1-A |
| `WRENCH_DOCK_01` | (1, 1.05, 0.45) | toolDock | WP-F1-C |
| `LEAK_EFFECT_01` | (0.8, 0.95, 0.43) | effect | WP-F1-C |
| `REPAIR_STATUS_01` | (0.8, 1.4, 0.45) | readout | WP-F1-C |

### STN_ELECTRIC_PUMP

Static mounting line at F1 +0.55 m. No water/power behaviour. Pump and elevated panel share one sequential position.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `MOTOR_HOUSING_LINE` | (0, 0.55, 0.45) | readout | WP-F1-B |
| `INTAKE_SERVICE` | (0, 0.8, 0.5) | service | WP-F1-B |

### STN_PUMP_CONTROL

Envelope begins 1.0 m above floor, above the pump; local anchor heights are relative to this elevated root.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `START` | (-0.16, 0.18, 0.25) | button | WP-F1-B |
| `STOP` | (0.16, 0.18, 0.25) | button | WP-F1-B |
| `PUMP_READY` | (0, 0.37, 0.25) | readout | WP-F1-B |

### STN_WALL_DEPTH_GAUGE

Passive wall graduations, 0.2 m increments from world Y=0. No dedicated operator exclusion zone.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `DEPTH_ZERO` | (0, 0, 0.03) | readout | read from route |
| `DEPTH_READOUT` | (0, 1.4, 0.03) | readout | read from route |

### STN_GENERATOR

Envelope includes 0.45 m plinth. Service face inward. Starter lives on the separate generator panel. No generator/power implementation.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `OIL_INTAKE` | (0.25, 1.2, 0.75) | service | WP-F2-B |
| `OIL_STORAGE` | (-0.25, 1, 0.75) | toolDock | WP-F2-B |
| `PRIMER` | (0, 1.15, 0.75) | lever | WP-F2-B |
| `PLINTH_TOP` | (0, 0.45, 0.4) | readout | WP-F2-B |

### STN_GENERATOR_PANEL

Environment mount only; owning teammate supplies behavior.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `STARTER` | (0, 1.15, 0.25) | button | WP-F2-A |
| `GENERATOR_STATUS` | (0, 1.4, 0.25) | readout | WP-F2-A |

### STN_BREAKER_PANEL

Dedicated emergency readability required; circuit taxonomy belongs to Eli.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `BREAKER_BANK` | (0, 1.2, 0.25) | switchBank | WP-F2-D |

### STN_BILGE_PUMP_MANUAL

Reserve a 1.0 m diameter vertical swing clearance centred at CRANK_AXIS, the clearance extends to h=1.55 m (0.05 m above the equipment envelope); actual moving hardware remains inside the envelope. Actual handle radius 0.25 m: grip spans h=0.80–1.30 m. Dedicated emergency illumination. Operator stands 0.80 m in front.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `CRANK_AXIS` | (0, 1.05, 0.65) | crank | WP-F2-C |
| `CRANK_GRIP` | (0, 1.3, 0.65) | grip | WP-F2-C |

### STN_ISOLATION_MANIFOLD

Freestanding floor-bolted pipe frame moved inboard to keep the southern stair landing clear. Dedicated emergency illumination.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `UPPER_VALVE_01` | (-0.36, 1.15, 0.45) | handwheel | WP-F2-E |
| `UPPER_VALVE_02` | (-0.12, 1.15, 0.45) | handwheel | WP-F2-E |
| `UPPER_VALVE_03` | (0.12, 1.15, 0.45) | handwheel | WP-F2-E |
| `UPPER_VALVE_04` | (0.36, 1.15, 0.45) | handwheel | WP-F2-E |

### STN_RADIO

Includes desk and radio. Standing use required; optional chair cannot obstruct this work area.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `PUSH_TO_TALK` | (-0.25, 1.05, 0.7) | button | WP-F3-A |
| `FREQUENCY_DIAL` | (0, 1.15, 0.7) | dial | WP-F3-A |
| `WALKIE_DOCK` | (0.25, 1.05, 0.7) | toolDock | WP-F3-A |

### STN_FORECAST_BOARD

Both legacy remote-information roles preserved. Information only, no forecast logic.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `REMOTE_PRESSURE_CHART` | (-0.35, 1.4, 0.1) | readout | WP-F3-B |
| `BEARING_INFORMATION` | (0.35, 1.4, 0.1) | readout | WP-F3-B |

### STN_BEACON_CONTROL

Shares WP-F4-SHARED sequentially with alignment; not two simultaneous operators. Emergency-readable controls. No state changes.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `BEACON_LEVER` | (0, 1.1, 0.25) | lever | WP-F4-SHARED |
| `RUNNING` | (-0.1, 1.4, 0.25) | readout | WP-F4-SHARED |
| `FAULT` | (0.1, 1.4, 0.25) | readout | WP-F4-SHARED |
| `BEACON_STATUS` | (0, 1.4, 0.25) | readout | WP-F4-SHARED |

### STN_BEACON_ALIGNMENT

Shares WP-F4-SHARED sequentially; both clusters face the same stance. Existing bearing-control identity survives.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `AZIMUTH_WHEEL` | (0, 1.05, 0.25) | handwheel | WP-F4-SHARED |
| `ELEVATION_WHEEL` | (0, 1.35, 0.25) | handwheel | WP-F4-SHARED |

### STN_TOOL_WALL

Includes bench. Touch targets on forward-accessible docks, decorative pegboard above.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `TOOL_DOCK_01` | (-0.25, 1.15, 0.9) | toolDock | WP-HOUSE-A |
| `TOOL_DOCK_02` | (0.25, 1.15, 0.9) | toolDock | WP-HOUSE-A |

### STN_BRIEFING_BOARD

Static board; no day sequencing or incident logic.

| Child transform | Local XYZ | Type | Assigned stance |
|---|---|---|---|
| `KEEPER_WATCH` | (0, 1.4, 0.12) | readout | WP-HOUSE-B |

## Legacy mapping — both names remain live

[VERIFIED] Exact names were read from the four legacy level scenes at `98a1df2`, not reconstructed from role descriptions. Every entry retains child name `Interaction Anchor`. The JSON also records the source scene and component fileID.

| Exact legacy socket GameObject | Station identity / role | New parent | Floor | Owner |
|---|---|---|---|---|
| `Plumbing Repair - Leak` | Plumbing Repair / Leak | `STN_PIPE_MANIFOLD` | F1 | Tapan |
| `Pressure Sequence - Lower gauges` | Pressure Sequence / Lower gauges | `STN_PIPE_MANIFOLD` | F1 | Tapan |
| `Plumbing Repair - Repair status` | Plumbing Repair / Repair status | `STN_PIPE_MANIFOLD` | F1 | Tapan |
| `Plumbing Repair - Wrench` | Plumbing Repair / Wrench | `STN_PIPE_MANIFOLD` | F1 | Tapan |
| `Plumbing Repair - Brace` | Plumbing Repair / Brace | `STN_PIPE_MANIFOLD` | F1 | Tapan |
| `Generator Service - Starter` | Generator Service / Starter | `STN_GENERATOR_PANEL` | F2 | Eli |
| `Generator Service - Oil storage` | Generator Service / Oil storage | `STN_GENERATOR` | F2 | Tapan / Eli |
| `Generator Service - Oil intake` | Generator Service / Oil intake | `STN_GENERATOR` | F2 | Tapan / Eli |
| `Breaker Coordination - Breaker bank` | Breaker Coordination / Breaker bank | `STN_BREAKER_PANEL` | F2 | Eli |
| `Pressure Sequence - Remote instruction chart` | Pressure Sequence / Remote instruction chart | `STN_FORECAST_BOARD` | F3 | Nirvaan |
| `Radio Relay - Push to talk` | Radio Relay / Push to talk | `STN_RADIO` | F3 | Nirvaan |
| `Lantern Alignment - Bearing information` | Lantern Alignment / Bearing information | `STN_FORECAST_BOARD` | F3 | Nirvaan |
| `Radio Relay - Frequency dial` | Radio Relay / Frequency dial | `STN_RADIO` | F3 | Nirvaan |
| `Radio Relay - Walkie-talkie dock` | Radio Relay / Walkie-talkie dock | `STN_RADIO` | F3 | Nirvaan |
| `Breaker Coordination - Beacon status` | Breaker Coordination / Beacon status | `STN_BEACON_CONTROL` | F4 | Eli / Nirvaan |
| `Lantern Alignment - Bearing controls` | Lantern Alignment / Bearing controls | `STN_BEACON_ALIGNMENT` | F4 | Tapan |

## Integration numbers — proposed, no simulation edits

| Quantity | World metres |
|---|---|
| F1 finished floor / depth zero | 0.00 |
| F2 finished floor / proposed spill level | 3.30 |
| Generator plinth top | 3.75 |
| F1 pump motor housing centre | 0.55 |
| F3 / F4 floors | 6.60 / 9.90 |

Eli owns adoption of these values, generator recovery, flood limits and emergency conditions. A pump positioned on F2 cannot be promised permanently dry if his flood can rise above F2; that requires his explicit recovery rules. Nirvaan owns scene loading/build entries. Tapan binds repairs and service actions to these transforms.

## Validation and limitations

[VERIFIED] Structural checks: fifteen STN roots, sixteen legacy aliases, six legacy identities, unique child names within each station, local-to-world consistency, touch-anchor height bands, non-overlapping independent operator discs, and station box corners within the proposed room boundary. See the PR for exact executed scene serialization checks.

[PROPOSED] Full shell navigation, stair clearances, realistic equipment fit, lighting presets, hands reach and device comfort require subsequent builds. This lab is a contract visualization only; no Blender art or gameplay has been added. Do not use it as a production level.

Powered/emergency luminance checks will use a fixed powered reference exposure and a 0.18 grey card on F1, not cd/m². The four emergency-readable control groups are manual bilge, isolation, breaker and beacon control. Production loading and lightmaps remain untouched.
