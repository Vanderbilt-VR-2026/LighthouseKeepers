# Station contract — v1.0

[BUILT-UNVERIFIED] Frozen following Aneesh’s geometry approval. Footprints, station names and all sixteen legacy socket names are unchanged from v0.9. Maximum station displacement is **0.23 m**. The station-box lab is an attachment reference, not playable gameplay. Shell/device acceptance is reported separately.

## Rules of engagement

- Aneesh owns meshes, anchor transforms and names. Bind to named children; never move, rename or reparent them.
- No hardcoded world positions. Request additional anchors or footprint changes from Aneesh.
- Every future change requires a version bump and team notice. Nirvaan alone chooses production scene loading.
- All existing legacy components/events remain in their original scenes; the lab has empty aliases only.

## Coordinates and ergonomics

+Y up, +Z north, metres, clockwise azimuth from north. Local +Z faces the operator. Unit-scale station roots use a bottom-centre-back pivot; W×H×D extends from (-W/2,0,0) to (W/2,H,D).

Reach is assessed at the declared stance, not every point of its 0.70 m clearance disc. No mandatory kneeling. F4 panels are wall-mounted above 0.95 m; the assigned worked panels may overhang that floor-clearance disc. The approved F4 stance at r1.95 is about 0.516 m horizontally from the contact points (local panel depth included), superseding the old 0.80 m front-clearance requirement for these two panels only. All other nominal front clearances remain 0.80 m.

## Floors and stairs

| Floor | Y | Interior diameter | Exterior radius at floor |
|---|---:|---:|---:|
| F1 | 0 | 7.8 | 4.4 |
| F2 | 3.3 | 7.4 | 4.15 |
| F3 | 6.6 | 7.3 | 4.05 |
| F4 | 9.9 | 5.4 | 3.1 |

Main flights: 320°→220° counterclockwise, r3.00, 1.05 m clear width, 19 risers /18 goings, going 0.291 m. Final: 320°→140°, r1.70, same width/riser count, going 0.297 m. Rise 3.30 m each. Curved landings reserve 1.30 m radially and at least 1.30 m along the inner arc, always 0.08 m inside the finished wall. Flat connectors meet the complete stair width. F3 remains Ø7.30: reducing it to Ø7.00 would exclude the main flight’s 3.525 m outer edge, before railing/reveal allowances.

Gallery: r3.10→4.25, Y9.90, opaque deck. Cornice band Y9.30→9.90; the shaft stays outside F3’s full clear height through Y9.55. Separate corbels and drip trim visibly carry the projecting gallery. House throat Z−5.55→−3.85, 1.10×2.60 m clear, level floor.

## House

| Station / owner | World XYZ | Yaw | W×H×D |
|---|---|---:|---|
| `STN_TOOL_WALL` — Tapan | (-2.400, 0.000, -5.050) | 180 | (2.000, 2.200, 0.900) |
| `STN_BRIEFING_BOARD` — Nirvaan | (1.800, 0.000, -5.050) | 180 | (1.200, 2.100, 0.120) |

| Stance | World XYZ | Facing |
|---|---|---:|
| `WP-HOUSE-A` | (-2.400, 0.000, -6.750) | 0 |
| `WP-HOUSE-B` | (1.800, 0.000, -5.970) | 0 |

## F1

| Station / owner | World XYZ | Yaw | W×H×D |
|---|---|---:|---|
| `STN_PIPE_MANIFOLD` — Tapan | (2.440, 0.000, 2.440) | 225 | (3.600, 2.800, 0.450) |
| `STN_ELECTRIC_PUMP` — Eli / Tapan | (3.263, 0.000, -1.521) | 295 | (1.000, 0.950, 0.500) |
| `STN_PUMP_CONTROL` — Eli | (3.036, 1.000, -1.416) | 295 | (0.650, 0.500, 0.250) |
| `STN_WALL_DEPTH_GAUGE` — Eli | (1.317, 0.000, -3.618) | 340 | (0.300, 2.400, 0.030) |

| Stance | World XYZ | Facing |
|---|---|---:|
| `WP-F1-A` | (2.121, 0.000, 0.990) | 45 |
| `WP-F1-C` | (0.990, 0.000, 2.121) | 45 |
| `WP-F1-B` | (2.085, 0.000, -0.972) | 115 |

## F2

| Station / owner | World XYZ | Yaw | W×H×D |
|---|---|---:|---|
| `STN_GENERATOR` — Tapan / Eli | (0.916, 3.300, 3.419) | 195 | (2.100, 2.300, 0.750) |
| `STN_GENERATOR_PANEL` — Eli | (3.031, 3.300, 1.750) | 240 | (0.800, 1.700, 0.250) |
| `STN_BREAKER_PANEL` — Eli | (3.447, 3.300, -0.608) | 280 | (0.750, 1.750, 0.250) |
| `STN_BILGE_PUMP_MANUAL` — Tapan / Eli | (2.008, 3.300, -2.867) | 325 | (1.100, 1.500, 0.650) |
| `STN_ISOLATION_MANIFOLD` — Tapan | (-0.150, 3.300, -1.100) | 0 | (1.200, 1.700, 0.450) |

| Stance | World XYZ | Facing |
|---|---|---:|
| `WP-F2-B` | (0.515, 3.300, 1.922) | 15 |
| `WP-F2-A` | (2.122, 3.300, 1.225) | 60 |
| `WP-F2-D` | (2.413, 3.300, -0.425) | 100 |
| `WP-F2-C` | (1.176, 3.300, -1.679) | 145 |
| `WP-F2-E` | (-0.150, 3.300, 0.150) | 180 |

## F3

| Station / owner | World XYZ | Yaw | W×H×D |
|---|---|---:|---|
| `STN_RADIO` — Nirvaan | (1.855, 6.600, 2.968) | 212 | (1.600, 1.500, 0.700) |
| `STN_FORECAST_BOARD` — Nirvaan | (3.520, 6.600, 0.000) | 270 | (1.400, 2.000, 0.100) |

| Stance | World XYZ | Facing |
|---|---|---:|
| `WP-F3-A` | (1.060, 6.600, 1.696) | 32 |
| `WP-F3-B` | (2.620, 6.600, 0.000) | 90 |

## F4

| Station / owner | World XYZ | Yaw | W×H×D |
|---|---|---:|---|
| `STN_BEACON_CONTROL` — Eli / Nirvaan | (1.749, 9.900, 1.974) | 215 | (0.500, 1.600, 0.250) |
| `STN_BEACON_ALIGNMENT` — Tapan | (1.257, 9.900, 2.318) | 215 | (0.500, 1.600, 0.250) |

| Stance | World XYZ | Facing |
|---|---|---:|
| `WP-F4-SHARED` | (1.118, 9.900, 1.597) | 35 |

## Named attachment transforms

### STN_PIPE_MANIFOLD

No kneeling required. A and C are simultaneous roles. Pressure instrument and repair indicator remain visible from both positions. Envelope includes every flange and wheel.

| Child | Local XYZ | Type |
|---|---|---|
| `VALVE_01` | (-0.800, 1.150, 0.450) | handwheel |
| `VALVE_02` | (0.800, 1.150, 0.450) | handwheel |
| `VALVE_03` | (-1.050, 1.050, 0.450) | lever |
| `VALVE_04` | (1.050, 1.050, 0.450) | handwheel |
| `VALVE_05` | (-0.550, 1.300, 0.450) | handwheel |
| `VALVE_06` | (0.550, 1.300, 0.450) | lever |
| `GAUGE_01` | (-0.800, 1.400, 0.450) | readout |
| `REPAIR_SITE_01` | (0.800, 0.950, 0.450) | repairSite |
| `BRACE_01` | (-0.800, 1.050, 0.450) | brace |
| `WRENCH_DOCK_01` | (1.000, 1.050, 0.450) | toolDock |
| `LEAK_EFFECT_01` | (0.800, 0.950, 0.430) | effect |
| `REPAIR_STATUS_01` | (0.800, 1.400, 0.450) | readout |

### STN_ELECTRIC_PUMP

Static mounting line at F1 +0.55 m. No water/power behaviour. Pump and elevated panel share one sequential position.

| Child | Local XYZ | Type |
|---|---|---|
| `MOTOR_HOUSING_LINE` | (0.000, 0.550, 0.450) | readout |
| `INTAKE_SERVICE` | (0.000, 0.800, 0.500) | service |

### STN_PUMP_CONTROL

Envelope begins 1.0 m above floor, above the pump; local anchor heights are relative to this elevated root.

| Child | Local XYZ | Type |
|---|---|---|
| `START` | (-0.160, 0.180, 0.250) | button |
| `STOP` | (0.160, 0.180, 0.250) | button |
| `PUMP_READY` | (0.000, 0.370, 0.250) | readout |

### STN_WALL_DEPTH_GAUGE

Passive wall graduations, 0.2 m increments from world Y=0. No dedicated operator exclusion zone.

| Child | Local XYZ | Type |
|---|---|---|
| `DEPTH_ZERO` | (0.000, 0.000, 0.030) | readout |
| `DEPTH_READOUT` | (0.000, 1.400, 0.030) | readout |

### STN_GENERATOR

Envelope includes 0.45 m plinth. Service face inward. Starter lives on the separate generator panel. No generator/power implementation.

| Child | Local XYZ | Type |
|---|---|---|
| `OIL_INTAKE` | (0.250, 1.200, 0.750) | service |
| `OIL_STORAGE` | (-0.250, 1.000, 0.750) | toolDock |
| `PRIMER` | (0.000, 1.150, 0.750) | lever |
| `PLINTH_TOP` | (0.000, 0.450, 0.400) | readout |

### STN_GENERATOR_PANEL



| Child | Local XYZ | Type |
|---|---|---|
| `STARTER` | (0.000, 1.150, 0.250) | button |
| `GENERATOR_STATUS` | (0.000, 1.400, 0.250) | readout |

### STN_BREAKER_PANEL

Dedicated emergency readability required; circuit taxonomy belongs to Eli.

| Child | Local XYZ | Type |
|---|---|---|
| `BREAKER_BANK` | (0.000, 1.200, 0.250) | switchBank |

### STN_BILGE_PUMP_MANUAL

Reserve a 1.0 m diameter vertical swing clearance centred at CRANK_AXIS, the clearance extends to h=1.55 m (0.05 m above the equipment envelope); actual moving hardware remains inside the envelope. Actual handle radius 0.25 m: grip spans h=0.80–1.30 m. Dedicated emergency illumination. Operator stands 0.80 m in front.

| Child | Local XYZ | Type |
|---|---|---|
| `CRANK_AXIS` | (0.000, 1.050, 0.650) | crank |
| `CRANK_GRIP` | (0.000, 1.300, 0.650) | grip |

### STN_ISOLATION_MANIFOLD

Freestanding floor-bolted pipe frame moved inboard to keep the southern stair landing clear. Dedicated emergency illumination. Visible 0.05 m plinth within the existing footprint; pipes penetrate floor. Plinth does not raise published handwheel axes.

| Child | Local XYZ | Type |
|---|---|---|
| `UPPER_VALVE_01` | (-0.360, 1.150, 0.450) | handwheel |
| `UPPER_VALVE_02` | (-0.120, 1.150, 0.450) | handwheel |
| `UPPER_VALVE_03` | (0.120, 1.150, 0.450) | handwheel |
| `UPPER_VALVE_04` | (0.360, 1.150, 0.450) | handwheel |

### STN_RADIO

Includes desk and radio. Standing use required; optional chair cannot obstruct this work area.

| Child | Local XYZ | Type |
|---|---|---|
| `PUSH_TO_TALK` | (-0.250, 1.050, 0.700) | button |
| `FREQUENCY_DIAL` | (0.000, 1.150, 0.700) | dial |
| `WALKIE_DOCK` | (0.250, 1.050, 0.700) | toolDock |

### STN_FORECAST_BOARD

Both legacy remote-information roles preserved. Information only, no forecast logic.

| Child | Local XYZ | Type |
|---|---|---|
| `REMOTE_PRESSURE_CHART` | (-0.350, 1.400, 0.100) | readout |
| `BEARING_INFORMATION` | (0.350, 1.400, 0.100) | readout |

### STN_BEACON_CONTROL

Shares WP-F4-SHARED sequentially with alignment; not two simultaneous operators. Emergency-readable controls. No state changes. Approved F4 reach exception: 0.42 m to the panel face, about 0.516 m diagonally to each control cluster. The assigned panel may overhang the operator floor-clearance disc above 0.95 m; no stand/plinth below it. The disc reserves foot space, not reach from every point.

| Child | Local XYZ | Type |
|---|---|---|
| `BEACON_LEVER` | (0.000, 1.100, 0.250) | lever |
| `RUNNING` | (-0.100, 1.400, 0.250) | readout |
| `FAULT` | (0.100, 1.400, 0.250) | readout |
| `BEACON_STATUS` | (0.000, 1.400, 0.250) | readout |

### STN_BEACON_ALIGNMENT

Shares WP-F4-SHARED sequentially; both clusters face the same stance. Existing bearing-control identity survives. Approved F4 reach exception: 0.42 m to the panel face, about 0.516 m diagonally to each control cluster. The assigned panel may overhang the operator floor-clearance disc above 0.95 m; no stand/plinth below it. The disc reserves foot space, not reach from every point.

| Child | Local XYZ | Type |
|---|---|---|
| `AZIMUTH_WHEEL` | (0.000, 1.050, 0.250) | handwheel |
| `ELEVATION_WHEEL` | (0.000, 1.350, 0.250) | handwheel |

### STN_TOOL_WALL

Includes bench. Touch targets on forward-accessible docks, decorative pegboard above.

| Child | Local XYZ | Type |
|---|---|---|
| `TOOL_DOCK_01` | (-0.250, 1.150, 0.900) | toolDock |
| `TOOL_DOCK_02` | (0.250, 1.150, 0.900) | toolDock |

### STN_BRIEFING_BOARD

Static board; no day sequencing or incident logic.

| Child | Local XYZ | Type |
|---|---|---|
| `KEEPER_WATCH` | (0.000, 1.400, 0.120) | readout |

## Legacy mapping — both names remain live

All aliases retain child `Interaction Anchor`. Exact source identities, roles, scene paths and fileIDs are in JSON.

| Exact legacy socket | STN parent | Floor | Owner |
|---|---|---|---|
| `Plumbing Repair - Leak` | `STN_PIPE_MANIFOLD` | F1 | Tapan |
| `Pressure Sequence - Lower gauges` | `STN_PIPE_MANIFOLD` | F1 | Tapan |
| `Plumbing Repair - Repair status` | `STN_PIPE_MANIFOLD` | F1 | Tapan |
| `Plumbing Repair - Wrench` | `STN_PIPE_MANIFOLD` | F1 | Tapan |
| `Plumbing Repair - Brace` | `STN_PIPE_MANIFOLD` | F1 | Tapan |
| `Generator Service - Starter` | `STN_GENERATOR_PANEL` | F2 | Eli |
| `Generator Service - Oil storage` | `STN_GENERATOR` | F2 | Tapan / Eli |
| `Generator Service - Oil intake` | `STN_GENERATOR` | F2 | Tapan / Eli |
| `Breaker Coordination - Breaker bank` | `STN_BREAKER_PANEL` | F2 | Eli |
| `Pressure Sequence - Remote instruction chart` | `STN_FORECAST_BOARD` | F3 | Nirvaan |
| `Radio Relay - Push to talk` | `STN_RADIO` | F3 | Nirvaan |
| `Lantern Alignment - Bearing information` | `STN_FORECAST_BOARD` | F3 | Nirvaan |
| `Radio Relay - Frequency dial` | `STN_RADIO` | F3 | Nirvaan |
| `Radio Relay - Walkie-talkie dock` | `STN_RADIO` | F3 | Nirvaan |
| `Breaker Coordination - Beacon status` | `STN_BEACON_CONTROL` | F4 | Eli / Nirvaan |
| `Lantern Alignment - Bearing controls` | `STN_BEACON_ALIGNMENT` | F4 | Tapan |

## Recovery and lighting handoff

Lowest operating contacts: bilge Y4.10; isolation Y4.35; breaker Y4.45; beacon Y10.90. Dry-grip defeat bound is strictly below Y4.10, including rise during the sustain delay. Eli may explicitly allow submerged crank grips and instead use the next limiting contact at Y4.35. These are environmental authoring constraints, not changes to simulation.

Four emergency-readable control groups: bilge, isolation, breaker, beacon. Lighting validation requires a fixed, once-calibrated powered grey-card reference and exposure. A missing/unmeasured reference is a failed check, never an invented pass.

The layout JSON is the generation source; contract JSON and lab must match it. No production build-list or load-order changes are made by this contract.
