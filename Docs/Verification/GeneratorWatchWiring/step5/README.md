# Step 5 — restore the tower wiring

History: b6ca4b3 added full wiring on m3/five-nights; 0987356 refined the finish. Neither was in s4/integrated. No integrated merge deleted it. Reused those assets/authoring/runtime, replacing the older single riser and generator-to-panel placeholder. No campaign/ship code imported.

Rebaked for the moved power panel: generator -> panel -> wall risers/collared floor penetrations -> five junctions -> lamps, pump and gauge, radio, PA speakers, beacon and weatherproof House feed. Thin brown woven cables, slight sag, ceramic insulation, rusty brackets, salt/fibre texture and occasional tarred repair patches. One shared procedural atlas; no downloaded asset/licence additions.

129 runs, 57 endpoints, **13,894 triangles, five combined meshes, one shared material, zero wiring colliders/lights**. One nearby junction hum; <=24 short trip sparks. Restoration pulse and lamp delays read existing host circuit state; guest snapshot test passes. GPU <=0.3 ms is NOT VERIFIED without worn device profiling.

Focused wiring state/assets/guest replica 3/3; generated Lab scene test 1/1 verifies five meshes/material budget and removal of old duplicate feeds. Build preflight rejects missing/stale/over-budget resources. Clearance scan: zero unexpected contacts; intentional collars/feed-throughs listed. The moved panel's original rear-terminal routes intersected the wall; corrected them to the service side and ceiling route, then rebaked/retested.

Muted graphics solo Night 2 actual runout, refill/physical controls, power recovery, pipe and sleep into Day 3 passes, zero errors. Quiet opening accelerated; interactions at normal speed. Inspected riser, pump, House feed, pulse and blackout captures. Worn cable visibility/clearance and live two-device co-op still pending.
