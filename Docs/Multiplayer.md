> **Current PR #65 candidate:** the shipped Lab world is solo. Two-keeper gameplay launch is disabled until shared Lab state is implemented. The networking setup below is historical/diagnostic and does not establish current co-op gameplay. See [current Sprint 3 README](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/blob/codex/sprint3-technical-fixes/Docs/Sprints/Sprint3/README.md).

# Four-player development environment

## Start a development crew

William's layer applies to the isolated `LK_Bootstrap` development environment. The default **Play game** entry retains main's exactly two-player Storm Watch experience. Both use `FusionLobbyTransport` as the sole owner of session startup, roster and shutdown. `KeeperReplicationSession` borrows that transport's runner for development-environment player/prop replication.

1. Use Unity **6000.3.23f1** and allow its package import and Fusion code weaving to finish. The existing Fusion SDK is used; no new networking package is required.
2. Check `Assets/Photon/Fusion/Resources/PhotonAppSettings.asset` in Unity. A Fusion App ID is already present in this checkout; it must belong to an active **Fusion** application with enough concurrent-player capacity. All four devices must use the same App ID, application version and region. An App ID is a public application identifier; never put Photon account credentials in the game.
3. Set the same explicit `FixedRegion` on every build so peers do not choose different best regions. The shared transport uses the existing Photon settings; this layer does not override the region or application version.
4. Use **Lighthouse Keepers → Development → Play environment** to open `Assets/_LighthouseKeepers/Scenes/Bootstrap/LK_Bootstrap.unity`, and enter a crew name and room name on the world-space crew board at the spawn. Choose **Host room** on one device. Choose **Join room** with that exact room name on up to three other devices. Joining never silently creates another room. Room capacity includes the host and is fixed at four.
5. Walk with the existing locomotion controls and grab authored interactable props with grip. The board can be hidden and reopened at the spawn. **Leave room** follows the shared transport's return-to-lobby flow. Reopen the development environment to start another four-player preview. The host leaving ends the session for guests; automatic host migration is not implemented.

The default `LK_NetworkLobby` watch room remains the two-player Storm Watch entry: host/join, signal ready, then **Begin watch** on the same connection. Its rules still require exactly two keepers, and partner loss ends that run. `LK_LobbyPreview` is explicitly local UI practice and `LK_DevGym` remains offline. See [Storm Watch](StormWatch.md) and [the shared transport guide](FusionNetworking.md).

Connections need Photon name-server, regional matchmaking and Fusion UDP/relay access. An HTTPS domain allowlist alone does not establish UDP support in a cloud machine. Physical headsets and normal Unity workstations must be able to reach the configured Photon region. No self-hosted server or port forwarding is required when Photon relay is available.

## What is synchronized

- Each player keeps one local XR rig, camera and listener. Remote keepers are visual-only heads, hands and torsos; they have no input components or colliders. Root, head and controller poses upload at 20 Hz and are relayed by the host with smoothing.
- The existing scene `XRGrabInteractable` props are registered after all six environment scenes finish loading. Every peer derives the same ordered identifiers from scene and hierarchy paths. A world-layout handshake rejects a peer whose registered props differ. All devices must use the same build; this handshake does not compare arbitrary game code or meshes.
- The host decides exclusive grab ownership. A local selection is optimistic while its request is in flight; a denied request cancels the selection. A prop already owned by another keeper cannot be selected. Remote handheld poses are validated against the owner's last head/hand pose, with a maximum six-metre head distance and five-metre hand distance to support near/far grabbing.
- The owner sends held poses; the host replicates object state at 15 Hz. Released rigidbodies simulate on the host, including collisions, gravity and capped throw velocities. Guests interpolate these states rather than simulating independent released copies. Only the host evaluates automatic socket placement. Grip and object poses remain responsive on the holding device.
- Lease epochs reject delayed held/release packets after ownership changes. Disconnects and two seconds without accepted held updates release a prop for the remaining crew. New arrivals receive current ownership and transforms. Flood height, pause, rise speed and day selection are also sent by the host.

This is a cooperative pose/prop transport built on Fusion static RPCs. It uses unreliable messages for frequent motion and reliable RPCs for ownership changes. The shared transport handles roster and scene flow through its existing reliable-data messages. It does not require rewriting or baking the authored props as Fusion NetworkObjects. `LK.Networking` is explicitly included in Fusion's `AssembliesToWeave` list, and its assembly references the runtime, XRI, UI and TMP dependencies used by this layer. This development layer does not add voice, adversarial movement validation, mission mechanics, arbitrary runtime object spawning, or automatic host migration; main's separate Storm Watch mission implementation is preserved. Static scenery remains static; the shared interaction set is the authored grabbable props.

## Verification status

Cloud checks for this change:

- **19 selected tests passed** under NUnit/.NET 8: eight ownership tests, seven existing lobby-state tests and four scene disconnect-policy cases, with zero failures or skips among selected tests. Two Unity JSON serialization tests were excluded from this standalone run and still require Unity. The production source and actual test files were used.
- All multiplayer sources and lobby sources passed a standalone C# compilation against the included Fusion DLLs, Photon Realtime source, pinned Unity engine reference assemblies, and the editor's UGUI/TextMeshPro sources. XR and environment boundaries used compile-time substitutes because the XR package download was denied. This is not a complete Unity project compilation, validation of the real XR APIs, or an RPC-weaving check.
- Repository asset/meta hygiene and whitespace checks passed.
- The official Unity 6000.3.23f1 editor was downloaded with its published checksum verified and its version confirmed. Batch import exited because no valid Unity Editor license was available. Unity package metadata is reachable, but `download.packages.unity.com` still returns a policy denial in this running instance; its required domain was added to the saved environment draft.
- **Unity Edit Mode/Play Mode integration, a live four-client room, Android builds and Quest testing remain unrun.** Historical verification reports are not evidence for these changes.

In a licensed editor, run the existing Edit Mode suite, including `GrabAuthorityTests`, and Bootstrap's existing Play Mode tests. Then run this acceptance pass with four instances of the same build:

1. Host and join three guests. Verify a fifth join is refused and every peer's roster agrees. Repeated connection clicks must not create another runner.
2. Walk and turn on each peer, then move both tracked hands. Verify the other three see each keeper and no extra local camera/listener is created.
3. Grab, move, rotate and throw the same authored prop from every peer in turn. Verify all peers agree after landing and the object remains available to the next keeper.
4. Have two peers grab the same prop together. Exactly one must retain ownership; the loser must release its private selection. Release and re-grab quickly to exercise delayed packets.
5. Disconnect a guest while holding a prop. Verify another keeper can grab it. Join a replacement guest after moving props and verify it sees their current positions, not their initial scene positions.
6. Leave and reconnect to the development environment; verify stale avatars and ownership are cleared. End the host session and verify guests can connect again. Separately verify that the normal lobby still launches Storm Watch only with exactly two ready keepers.

For an import or test log, use fresh output outside tracked `Docs/Verification`, for example an ignored `artifacts/Multiplayer/` directory. Do not publish a build with compilation, weaving or connection errors. Quest stereo rendering, interaction comfort and sustained performance require actual device checks.

## Merge integration checks

The conflict resolution retains main's `FusionLauncher` button adapter, `LobbyManager`, `LobbyScreen`, `ISessionLobbyTransport` and default transport provider. The old parallel session interface/provider was removed. `KeeperReplicationSession` never creates or shuts down a runner. The existing offline Bootstrap handoff Play Mode test now also checks that the replication helper borrows the crew runner and that destroying the helper leaves the transport connected. This extended Unity test has not been executed in this cloud instance.
