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
