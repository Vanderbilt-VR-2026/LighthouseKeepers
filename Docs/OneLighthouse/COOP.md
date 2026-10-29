# Co-op step 1 — Editor host + Quest guest

Branch `coop/two-headsets` starts at Will’s PR #67 (`a8fde691`), based on merged PR #65. Main is untouched. Read PR #67 body and `Docs/Sprints/Sprint3/WilliamNetworking.md` before implementation.

Reuse: existing FusionLobbyTransport/CrewLobbyTransport, six-character join code and controller keypad; CrewPoseStream 20 Hz interpolated head/hands; FusionMessageChannel on the existing runner; Will’s host-owned prop leases, layout fingerprints, world generation and throw/recovery handling. PR #67 only enables development network practice with objectives frozen; 19 portable tests reported, but no Unity compilation or APK/device testing.

Requested step 1: one CoopEnabled flag, maximum two keepers, normal Host/Join/Ready/Begin Watch path into the same Lab, separated House spawns, tan remote hands/simple head, Editor WASD/mouse look, host-owned day and read-only guest snapshots. Guest station actions disabled; authoritative guest interaction commands are co-op step 2. Solo must retain the existing loop. No main changes or merges.

Verification and device results will be appended after execution; no co-op headset acceptance inferred from source or Editor tests.

## Implementation

- `ShipWorld.CoopEnabled` defaults ON on this branch; max crew is 2. Flag OFF restores the prior multi-keeper Lab guard. Host-alone remains available, solo still closes/avoids Photon. Normal Begin Watch requires every connected keeper ready; the separate practice button is hidden while the flag is ON.
- Existing `FusionMessageChannel` carries a versioned, scene-generation-bound host snapshot at 5 Hz. Guest acknowledgements gate the host clock while its partner is loading/stale. Day/time, water, power (driving beacon and emergency lights), faults/checkmarks, pipe and radio display progress are host-owned. Guest snapshot application validates values and rejects stale/wrong-world packets; local guest clock and station mutations are disabled. No new runner or networking package.
- Host/guest House positions are (-0.9, 0.04, -9.15) and (-0.9, 0.04, -7.65), facing the existing wall board. The XR Origin moves once at network spawn; tracked camera/controller transforms and accepted hand rotations remain unchanged.
- Remote keepers reuse the existing interpolated pose stream and visual-only head/jacket. Remote hands copy only the accepted tan skinned glove hierarchy/material, with no cameras, UI, audio or interactors. Editor pose packets use head-relative default hand offsets; hold right mouse to look and WASD to move in co-op.
- Will’s wrench/microphone recovery now uses narrow runtime ownership interfaces instead of importing networking types into gameplay. The adapter remains in LK.Net; the architecture test is preserved, not weakened.
- **Co-op step 2:** guest station commands/ownership validation for shared task progress. For this test all guest world interactables are disabled; Watch Menu/return remains available. The Editor host is the day authority. This is not a completed two-person station game.

## Validation so far

- Unmodified PR #67 imported/compiled in Unity 6000.3.23f1; baseline solo/menu/station PlayMode **11/11 passed**.
- Initial co-op focused PlayMode **16/16 passed**, including the unchanged 11 cases, host-alone launch, replica safety and practice freeze.
- Full EditMode initially **211/212**: networking boundary identified direct gameplay references (including PR #67 recovery code). Moved dependencies behind interfaces; full rerun **212/212 passed**.
- Connected Quest is the lead’s device `2G0YC1ZG32048T`; authorized USB confirmed. Build/install and physical co-op acceptance are still pending at this point.

- Final focused PlayMode **19/19 passed**, zero skips, with `LK_FUSION_CLOUD_TESTS=1`: unchanged solo/menu/station cases, read-only replica behavior, practice freeze, host-alone real Lab scene launch, both spawn floor/standing clearances, remote tan mesh/no camera/audio/interactor checks, disabled-flag guard, and two real Photon peers launching normal Lab mode and delivering a host state snapshot over the existing runner. The traffic-only two-peer test does not replace physical Editor+Quest acceptance.
- Test artifacts: `artifacts/coop/solo-baseline.xml`, `solo-and-coop.xml`, `editmode-final.xml`, `playmode-final.xml` and matching logs. No test disables the architecture boundary or changes accepted solo assertions.
