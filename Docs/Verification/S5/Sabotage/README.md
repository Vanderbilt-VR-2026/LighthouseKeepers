# Sabotage prototype checkpoint — November 13

**Not enabled in the delivered game.** `Sprint5Tuning.sabotageTraces=false`. The existing ship, pipe and generator rules remain active. This is partial phase3.1, not a completed Drowned encounter.

Implemented behind the switch: the existing host NightSchedule records warning/broken/repaired times; guests accept validated copies, with retry/late-join safeguards. Authored clock warnings start3s ahead where possible. Dependency-triggered faults wait3s after readiness. Physical fuel exhaustion is not postponed; its gurgle predicts the remaining tank time. The tutorial arrival leak already exists when the keeper arrives, so no invented pre-arrival sound is played.

Four bounded spatial warning sources and four combined wet-trace meshes (480triangles total, one material, zero new lights/colliders). Pipe footprints lead toward the House/sea door; the generator has an oily tank print and floor puddle. Radio/beacon trace anchors are prototype-only and still need close-up art review. Evidence remains through the repair, holds20s and fades over2s. The tank print conforms to its curved surface. Original mono synthesis, not downloaded audio; see SoundMap.md.

81 focused tests pass, including disabled-mode replication, invalid receipts, duplicate/retry, immutable guest data, warning lead and evidence fade. Graphics Night2 with the switch explicitly enabled passes with zero errors; the two screenshots here were inspected. The first graphics run exposed a Unity MaterialPropertyBlock field-initializer error, fixed by constructing it during Build. A second test fixture lost its requested day at domain reload; the corrected run uses the actual lobby Night2 route. Raw failures remain in artifacts/s5/sabotage/.

**Activation gate failed:** `timing-prototype-failure.json` is11/12: Night5 duo floods. The extra warning time changes dependency ordering enough that pipe valve+wrench and active ship steering can demand three simultaneous roles. Clock lookahead preserves scheduled fault deadlines and recovery quiet, but does not resolve that dependency case. Do not turn the switch on or hide this failure by relaxing a test. Review director urgency/ship scheduling so two keepers have a fair order, then rerun the failed case and the12-run model. Final headroom still needs the planned phase6 balance pass.

**Shipping gate passes:** with the switch OFF, all12 original workload models pass (`timing-shipping.json`). These are scripted task/travel estimates, not human first-timer timings. The current network schema is LK-S5-TRACES1 / LK-Lab-Coop-v27; both clients must match even with the prototype off.

Next: finish this gate, then phase3.2 cable reconnect/lens wipe/fuel evidence,3.3 explanations, phase4 lantern, then phase5 Drowned. No hunter, chase, hiding or combat is claimed. Quest GPU/audio/comfort and a human two-way voice check remain pending.

Final shipping configuration passed live two-Mac host-alone/latejoin, both avatars, mono/AEC/NS configuration, both cross-floor handheld radio routes, mute, guest pipe repair, shared ship outcome, shared sleep, leave/rejoin. The prototype is off on both. Reviewed the two avatar views and the shared Night2 board. Human speech was not auditioned because both Editors stayed muted; enabled prototype live transport is still an activation check.
