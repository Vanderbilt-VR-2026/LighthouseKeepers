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


## 2026-10-13 — Environment contract v1.0

- [VERIFIED] Freeze the approved layout with unchanged footprints/names and maximum station movement 0.23 m; preserve production loading and all teammate systems.
- [VERIFIED] Use curved 1.30 m landings with 0.08 m wall reveal; retain F3 Ø7.30 because the incoming stair outer radius3.525 cannot fit Ø7.00.
- [VERIFIED] F4 stance35°/r1.95 and raised panels provide approximately0.516 m contact reach; this approved specific reach requirement supersedes the earlier0.80 m front-clearance rule for those panels only.
- [VERIFIED] Preserve F3 ceiling throughY9.55; use a distinct0.60 m corbel band below galleryY9.90 rather than tapering through headroom.
- [PROPOSED] Eli owns dry-versus-wet bilge operability and defeat-sustain allowance; publish geometric bounds4.10/4.35/4.45/10.90 m without changing simulation.


### 2026-10-14 — Batch A isolated shell validation

- [VERIFIED] Keep production loading/build settings and legacy content unchanged; use an explicit-scene Environment Lab APK for the greybox gate.
- [VERIFIED] Convert Unity XYZ to Blender (-X,-Z,Y) before the approved FBX export; imported reference bounds prove scale and handedness.
- [VERIFIED] F1 route approaches the main stair through its clear north side instead of crossing the railing; station coordinates remain frozen.
- [BUILT-UNVERIFIED] Use smooth stair collision ramps and continuous safety barriers; render-only tread/rail proxies do not create hand-catching collision detail.
- [BUILT-UNVERIFIED] Increase only F2/F3 emergency fixture intensity from 0.5 to 1.2 after fixed-reference floor samples failed; thresholds and exposure remain unchanged. Initial calibration was corrected to explicitly apply powered light probes before publication.
- [BUILT-UNVERIFIED] Bake two complete combined-Lab presets including probe coefficients; production additive renderer/lightmap binding remains Nirvaan's separate adoption step.
- [VERIFIED] Repeated unchanged-input generation preserves committed scene bytes via an output-verified build stamp; independent clean-room Unity local-file-ID determinism is not claimed.
- [PROPOSED] Headset comfort, appearance, visibility culling and sustained 72 Hz require actual device/user acceptance; automated geometry and brightness tests do not establish them.

- [BUILT-UNVERIFIED] Enable baked fixture shadows to prevent illumination passing through solid walls; no realtime shadow lights are added. Remove Unity-inferred FBX LOD groups before authoring the explicit island group to prevent duplicate renderer ownership.

- [BUILT-UNVERIFIED] Extend luminance sampling beyond room grids to the passage, stair routes and full gallery ring. Add two attached battery-backed gallery fixtures so the outdoor exit remains readable in both lighting states.

- [BUILT-UNVERIFIED] Replace disconnected closed floor wedges with one watertight slab per floor. Coincident internal wedge faces caused radial bake seams; the corrected topology preserves every opening and finished-floor height while removing 3,912 triangles.

### Batch A bake comparison precision
The two independent progressive light bakes meet all 1,990 floor brightness checks. Compare emergency-versus-powered brightness with 2% relative or 0.0001 absolute numerical tolerance (whichever is larger), because one gallery sample differs by 1.8% between independent bakes. This does not change either minimum brightness threshold or the fixed powered reference/exposure. Final emergency fixture intensity is 3.6 on F2 and 1.2 on F3; other floor mains are 4. Device acceptance remains pending.

Lighting measurement cameras disable baked occlusion culling so the measured floor patch cannot become a zero-colour culling artifact after the visibility bake. Gameplay cameras retain occlusion culling. Prebuild validator errors use BuildFailedException so Unity cannot continue silently after an ordinary callback exception.
