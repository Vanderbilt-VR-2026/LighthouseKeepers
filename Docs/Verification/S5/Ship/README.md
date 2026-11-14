# Phase 2.1 — coastal steamer asset and route

Original 52 m steamer, named **Mary Ellen** in the reviewed preview. Baked hull/wheelhouse, lifeboats, funnel, two masts, brass-rimmed emissive portholes, port/starboard lights and a sweeping searchlight. Two shared shaders/materials; no realtime lights or colliders. GPU vertex animation drives bounded funnel smoke and bow spray. Distant LOD is a 204-triangle navigation-light cluster.

Budget: **11,760 triangles** including name, cone and particle quads; 72 smoke + 32 spray particles; two materials. GPU cost is **not measured** without Quest. No texture downloads, new sky, ocean or avatar changes.

The route runs west of the island: horizon → rocks → departure. Tests sweep the entire 1.25× finale hull through 241 route samples, verify island/coast clearance, far clip, continuous position/heading, geometry and shader validity. The first model exceeded 12k and was reduced; the raw failed result remains in artifacts.

Muted graphics Play Mode: zero logged errors. Reviewed horizon, near approach, broadside/peak and supplementary model-detail images. Gallery views are at (-3.6, 11.55, -0.6), player eye height; model-detail is explicitly an external inspection view. The same authored route is sampled for presentation evidence, not a played ship-survival night. After review, horizon lights were made legible, distant LOD corrected, the beam softened and painted hull lettering enlarged.

Normal play remains unchanged: `Sprint5Tuning.shipVisuals` is OFF until host-owned outcomes/window rules pass. Ship pass/wreck, P21–P24, sleep gate and ship-inclusive night timing are next (#95/#96), not claimed complete here. Protocol remains LK-S5-FEAR1 / v24.

Reproduce: menu **Lighthouse Keepers/Ships/Bake coastal steamer**; `python3 artifacts/remaining/m3/tests.py ship-model-final 'SteamerTests;QualityBaselineTests;StormSeascapeTests'`; `python3 artifacts/s5/ship/preview.py` (muted).

Device pending: both-eye visibility, LOD transition, shader appearance/Vulkan, sea/wave intersection in motion, name legibility, GPU/CPU with peak storm.
