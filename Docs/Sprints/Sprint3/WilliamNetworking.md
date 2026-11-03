# William Sprint 3 networking: core validation pass

This source branch builds on the networking already merged into Sprint 3. It is a movement/grabbing validation pass. Shared Lab objectives and day progression remain deferred until the core has passed Unity and independent-device testing.

## Existing networking

- Photon Fusion Host/Client sessions, one persistent crew runner, six-character room codes, names, readiness and reconnect/leave handling.
- `CrewPoseStream`: head and controller poses at 20 Hz, bounded interpolation, stale-pose hiding, and visual-only remote keepers.
- `FusionGrabAuthority`: host-owned exclusive leases, epoch checks, constrained ownership requests, disconnect/timeout cleanup, and prop state updates.
- William's older `KeeperReplicationSession` remains an opt-in legacy development helper. It is not installed alongside the ship-world session.
- The Lab's mission state, day clock, faults, radio progress and day transitions are local. Ordinary multi-player gameplay launch remains blocked.

## Changes in this pass

The lobby now admits up to four keepers. In the Editor or a development build, an all-ready host can choose **NETWORK PRACTICE · OBJECTIVES PAUSED**. This loads the current Lab on the existing runner. Clock, water evolution, objective completion, retry/replay and sleep/day advancement are paused; guests fail closed even if the launch snapshot arrives after the scene. This does not enable unreplicated co-op mission gameplay. **PLAY SOLO** retains its normal loop. Release builds do not expose or permit network practice.

The prop registry now covers the Lab's authored `XRGrabInteractable` props, including the radio microphone. Before selecting a network prop, every peer confirms a SHA-256 fingerprint of the same ordered hierarchy paths. Duplicate paths fail closed. Per-world generations reject stale requests/states after a scene reload. The host still arbitrates one owner and bounds hand proximity.

Release packets include sampled linear/angular throw velocities. The host applies capped momentum and simulates released rigidbodies; disconnect/lease expiry drops them without retaining an old velocity. Release is deferred until LateUpdate so XRI's detach finishes first. Microphone return and wrench recovery no longer overwrite another keeper's replicated transform. A host can recover a remotely released wrench after its normal return delay.

## Required core acceptance

Use matching builds, Photon settings and region. Start with two instances, then repeat with four. The previously published October 27 APK predates this source change; build a new development APK using the existing pipeline.

1. Host a room and join the same six-character code. Ready everyone, then choose **NETWORK PRACTICE** on the host. Verify exactly one local rig/camera/listener and one connected runner on each device. A fifth keeper must be refused.
2. Walk, turn and move both controllers on every device. Each other keeper must see the right actor's head/hands. Check stairs and collision on the accepted local rig. Keyboard movement alone does not validate tracked hand interaction.
3. Grip, rotate, move, release and throw the wrench from each guest and the host. All devices must agree after landing. Confirm remote throws retain momentum and a guest's idle recovery script cannot pull the wrench away from its owner.
4. Repeat with the microphone: remote-held microphone and cord remain visible, release returns it to its socket, and another keeper can select it afterward.
5. Have two keepers grab the same object together; only one may retain it. Rapidly release/re-grab to exercise delayed grants/releases. Disconnect a guest while holding a prop and verify another keeper can acquire it. Wait through the wrench recovery delay.
6. Return to the lobby and repeat. A mismatched build/layout must not grant ownership of a different object. Check the `FusionGrabAuthority.LayoutStatus` inspector value if selection remains blocked.
7. Verify the practice board says objectives are paused. Waiting, repairing, radio input and bed grip must not advance objectives, the clock or day. Leave, choose **PLAY SOLO**, and confirm the ordinary Day 1–3 loop still works.
8. Repeat under the existing simulated 80 ms added RTT / 1% loss option. Record actual RTT and device workload; the impairment setting is not a measurement or a comfort/performance result.

## Evidence from this cloud task

- **19 selected NUnit/.NET tests passed**, zero failures/skips: existing ownership rules, deterministic/mismatched prop layouts, four-player practice readiness/capacity, mode snapshots, cancellation and rejection of mode changes in client ready packets. Unity's JSON codec is not exercised by the standalone state tests.
- C# syntax checks passed for the changed files with both release and editor/development symbols. This is not Unity compilation or Fusion/Unity runtime execution.
- Added Unity Play Mode checks for release momentum/caps and for frozen practice clock, pipe/radio actions and day advancement. These have **not run here**.
- Unity's official editor/release endpoints return a network-policy `403` in this running machine; no editor is installed. The required Unity domains are already present in the saved environment draft, but that does not establish running-instance access. Full Editor import, Play Mode, Photon traffic, APK building and headset tests remain unrun for this change.

## Shared objectives follow-up

Only after the core acceptance above passes: make one host own the Lab clock, water, pipe/generator/radio progress, failure, retries and day transitions. Guests must submit bounded interactions tied to authenticated actors and current prop leases; they must apply authoritative snapshots rather than tick their own mission state. Add loading barriers, a matching-build/epoch handshake, invalid/stale-command tests and two/four-peer progression tests before relaxing the normal gameplay launch guard. This pass deliberately does not claim that work is finished.
