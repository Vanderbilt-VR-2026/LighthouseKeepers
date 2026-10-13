# Sprint 1 integration and repair decisions

- [VERIFIED] 2026-09-29: Start from latest team/main 41c9819; project lead explicitly superseded the instruction to merge local Phase 1.
- [PROPOSED] Evolve Eli's flood implementation into volume authority; preserve his presentation features and test intent instead of importing local prototypes.
- [PROPOSED] Preserve William's recovery scene as LK_Recovery and retain existing LK_Bootstrap; avoids replacing established XR/additive loading. Project lead approved; explain in GitHub PR.
- [PROPOSED] Keep offline lobby unwired in LK.Lobby; networking remains an open decision.
- [VERIFIED] Project lead permits essential navigation fixes before integration acceptance, development solo override for the repair gate, one realtime light per lantern, and relative rendered luminance checks rather than uncalibrated cd/m² claims.
- [PROPOSED] Keep strict primitive cleanup as a shipping-environment gate; do not claim a baseline diagnostic APK satisfies the new cleanliness contract.
- [UNRESOLVED] Networking transport, voice solution, player-count tuning, persistence, final art realism, permanent vendor-prop selection and future lantern cost remain open.

[VERIFIED] Project lead clarified this work is still Sprint 1. Current branch, checkout, build tooling and APK names now consistently identify Sprint 1.

[VERIFIED] September 30: retain Tapan audio as opt-in and Eli lobby as a separate development preview; merge their current code and tests without changing the approved environment entry flow. Use Eli TestLifecycle helper for both overlapping test fixes. Rename checkout/build/helper/merge documentation to Sprint 1; preserve original script GUID. Upper-floor furnishing remains deferred.


## Environment contract draft — October 13, 2026

- [VERIFIED] Create `aneesh/environment` in an isolated worktree from freshly fetched team main `98a1df2`; the personal `origin` is a different repository, so fetch/push uses `team`. Leave the dirty camera/lobby checkout untouched.
- [BUILT-UNVERIFIED] Publish contract v0.9 and matching editor-serialized lab first, commit `02165d9`, PR #42; fifteen station envelopes, sixteen legacy aliases and six exact identities, without gameplay or production-loading changes.
- [VERIFIED] Unity 6000.3.23f1 saved/reopened the lab with matching transforms and valid materials; numeric checks cover touch heights, room-bound corners, disjoint independent work discs, disc-to-equipment clearance and non-overlapping equipment volumes. These checks do not establish headset reach or shell traversal.
- [PROPOSED] Interior diameters F1/F2/F3/F4: 8.0/8.0/7.8/7.7 m, preserving floor Y=0/3.3/6.6/9.9. Ergonomic station and western stair clearance governs exterior shape. Wall thickness at floor lines: 0.50/0.45/0.40/0.35 m, continuous outer profile through radii 4.50/4.45/4.30/4.20 m; gallery outer radius 5.40 m. No art or shell generated yet.
- [PROPOSED] Western stair stays 200–340 degrees; walk radius 3.30 m, clear width 1.05 m, inner/outer 2.775/3.825. Nineteen risers at 0.173684 m with eighteen between-nosing goings at 0.447968 m. Two 1.30 m square landings centred at 188/352 degrees, radius 3.10 m, replace the impossible mid-flight landing at 270 degrees. Target minimum overhead clearance 2.60 m.
- [PROPOSED] House stays centred at (0,0,-7.35), with 7×5 m interior. Recess the central passage mouth 0.70 m into the house and end it 0.15 m inside F1, producing a real 1.70 m throat from Z=-5.55 to -3.85. Clear width 1.10 m, height 2.60 m, level floor; three circulation doors target 1.10×2.10 m clear openings.
- [PROPOSED] Generator shifts to azimuth 15 degrees/radius 3.55; its panel to 60/3.50 and breaker to 100/3.50. Bilge is at 145/3.50 with a 0.25 m crank radius centred 1.05 m above floor, reserving a 1.0 m diameter arm-swing clearance. Isolation becomes a floor-bolted frame at (-0.15,3.30,-1.30) to protect the stair landing and bilge work disc.
- [PROPOSED] Manifold reserves 3.60×2.80×0.45 m at 45/3.50; its simultaneous operator centres are 1.60 m apart. Repair point is 0.95 m high, no kneeling. Elevated pump panel and pump share a sequential stance. Radio moves to 30/3.60; forecast to 90/3.65. F4 control/alignment share one stance at 35/1.80; their separate roots flank the inward-facing 35/2.85 support line.
- [PROPOSED] Footprint sizes and names are promises through v1.0; world positions may change at most 0.30 m after review. Work discs reserve space; only declared stance centres, not every point in the discs, define reach checks. Actual device reach remains pending.
- [PROPOSED] Lighting will swap two complete lightmap presets, never pretend disabling a baked Light removes GI. Relative luminance uses one fixed powered reference/exposure and rejects a near-black reference. Four emergency-readable control groups: bilge, isolation, breaker, beacon control. Use baked/emissive emergency fixtures to preserve the realtime budget.
- [UNRESOLVED] Aneesh's dimension-table sign-off gates v1.0 and Blender Batch A. Eli must specify how F2 recovery remains available if water reaches F2; environment placement alone cannot guarantee a permanently dry recovery floor. Nirvaan owns the optional production switchover.
