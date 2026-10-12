# Four-player cooperative play

## Start a crew

1. Use Unity **6000.3.23f1** and allow its package import and Fusion code weaving to finish. The existing Fusion SDK is used; no new networking package is required.
2. Check `Assets/Photon/Fusion/Resources/PhotonAppSettings.asset` in Unity. A Fusion App ID is already present in this checkout; it must belong to an active **Fusion** application with enough concurrent-player capacity. All four devices must use the same App ID, application version and region. An App ID is a public application identifier; never put Photon account credentials in the game.
3. If `FixedRegion` is empty, the launcher uses **us** for every peer, rather than choosing different best regions. Set the same explicit region on every build to use another location. The launcher appends its protocol version to the configured app version at runtime, without modifying the settings asset.
4. Open `Assets/_LighthouseKeepers/Scenes/Bootstrap/LK_Bootstrap.unity`, press Play, and enter a crew name and room name on the world-space crew board at the spawn. Choose **Host room** on one device. Choose **Join room** with that exact room name on up to three other devices. Joining never silently creates another room. Room capacity includes the host and is fixed at four.
5. Walk with the existing locomotion controls and grab authored interactable props with grip. The board can be hidden and reopened at the spawn. **Leave room** restores local practice. The host leaving ends the session for guests; automatic host migration is not implemented.

Alternatively, open the existing `LK_LobbyPreview` watch room. Set a room name in **Crew options**, select **Host crew** or **Join crew**, and have everyone **Signal ready**. The host's **Begin watch** loads Bootstrap for every peer while preserving the connection. New arrivals to an already-started room enter the environment automatically. `LK_DevGym` remains an offline isolated test scene.

Connections need Photon name-server, regional matchmaking and Fusion UDP/relay access. An HTTPS domain allowlist alone does not establish UDP support in a cloud machine. Physical headsets and normal Unity workstations must be able to reach the configured Photon region. No self-hosted server or port forwarding is required when Photon relay is available.

## What is synchronized

- Each player keeps one local XR rig, camera and listener. Remote keepers are visual-only heads, hands and torsos; they have no input components or colliders. Root, head and controller poses upload at 20 Hz and are relayed by the host with smoothing.
- The existing scene `XRGrabInteractable` props are registered after all six environment scenes finish loading. Every peer derives the same ordered identifiers from scene and hierarchy paths. A world-layout handshake rejects a peer whose registered props differ. All devices must use the same build; this handshake does not compare arbitrary game code or meshes.
- The host decides exclusive grab ownership. A local selection is optimistic while its request is in flight; a denied request cancels the selection. A prop already owned by another keeper cannot be selected. Remote handheld poses are validated against the owner's last head/hand pose, with a maximum six-metre head distance and five-metre hand distance to support near/far grabbing.
- The owner sends held poses; the host replicates object state at 15 Hz. Released rigidbodies simulate on the host, including collisions, gravity and capped throw velocities. Guests interpolate these states rather than simulating independent released copies. Only the host evaluates automatic socket placement. Grip and object poses remain responsive on the holding device.
- Lease epochs reject delayed held/release packets after ownership changes. Disconnects and two seconds without accepted held updates release a prop for the remaining crew. New arrivals receive current ownership and transforms. Flood height, pause, rise speed and day selection are also sent by the host.

This is a cooperative pose/prop transport built on Fusion static RPCs. It uses unreliable messages for frequent motion and reliable messages for session, roster and ownership changes. It does not require rewriting or baking the authored props as Fusion NetworkObjects. `Assembly-CSharp` must remain in the existing Fusion `AssembliesToWeave` list. It does not add voice, adversarial movement validation, completed puzzle mechanics, arbitrary runtime object spawning, or automatic host migration. Static scenery remains static; the shared interaction set is the authored grabbable props.

## Verification status

Cloud checks for this change:

- **8/8 ownership tests passed**, with zero failures or skips. The actual `GrabAuthorityTests.cs` ran under NUnit/.NET 8 against the actual `GrabAuthority.cs`, covering contested ownership, release/reacquisition, stale epochs, another player's unauthorized updates, disconnect cleanup, lease expiry, invalid identifiers and session reset.
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
6. Start from the lobby with all four ready and verify every device enters Bootstrap. Leave and reconnect; verify stale avatars and ownership are cleared. End the host session and verify guests can connect again.

For an import or test log, use fresh output outside tracked `Docs/Verification`, for example an ignored `artifacts/Multiplayer/` directory. Do not publish a build with compilation, weaving or connection errors. Quest stereo rendering, interaction comfort and sustained performance require actual device checks.
