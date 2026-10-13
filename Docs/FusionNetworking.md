# Photon Fusion crew lobby — issue #33

This iteration extends William's Fusion launcher into a real Host/Client crew lobby. It uses the installed Fusion SDK and the project's existing Photon settings. No new networking package is required.

## Try it

1. Open Unity **6000.3.23f1**, allow compilation to finish, and choose **Lighthouse Keepers → Networking → Play Photon crew lobby**. The dedicated scene is `Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity`.
2. Under **Crew options**, enter your keeper name and a room name. Choose **Host crew** on one client. On another client, enter exactly the same room name and choose **Join crew**. Both clients need internet access, a working Fusion App ID, compatible builds, and the same Photon region. Set a shared Fixed Region in Photon App Settings if clients choose different regions automatically.
3. Verify both names appear on both boards. Choose **Signal ready** on each board. Only the host can choose **Begin watch**, and every player must be ready by default.
4. The host starts `LK_Bootstrap` through Fusion's scene manager. The crew follows into the environment, and the session survives the board and lobby rig unloading. Bootstrap continues to load its six authored content scenes locally.
5. **Leave watch** cancels a pending connection or leaves a lobby session. A host leaving ends the crew session. In the environment, the persistent **Fusion crew session** object's **Leave crew session** component context action returns to the network lobby; application exit also disconnects. An in-world session menu is future work.

The original **Play lobby preview** remains local practice. Bootstrap remains the first enabled build scene, so the existing environment entry point is preserved. The network lobby is registered at the end of Build Settings. For a multiplayer test build, put `LK_NetworkLobby` first in a separate build profile and include Bootstrap plus its six content scenes. The Sprint 1 environment APK builder explicitly selects environment scenes and does not include this lobby.

## What is shared

- A real roster with the host role, names, and ready states, including a full snapshot for late joins.
- Host-only launch and kick requests. The roster gates launch on the configured minimum crew and all-ready rule.
- Connection progress, errors, cancellation, session cleanup, and a fresh runner when reconnecting.
- The transition into Bootstrap through `NetworkRunner.LoadScene`. The host closes the room during launch, so an unready late join cannot race the scene change.

Player poses, grabbing, puzzles, flood/day progression, audio events, and host migration still need networking. Crew members currently explore independent copies of the environment after the shared scene launch. A connected lobby does not establish synchronized gameplay or Quest performance.

Kicking is currently exposed through `LobbyManager.Kick(playerId)` for UI integration; the watch board has no kick button yet.

## Implementation

`LK.Networking` references `LK.Lobby` and Fusion; the lobby assembly stays independent of Fusion. `ISessionLobbyTransport` adds asynchronous status and shared launch to the existing transport seam. `LocalLobbyTransport` remains the practice fallback. The authored network scene explicitly binds the real transport on its own persistent root. William's `FusionLauncher.StartHost` and `JoinSession` button methods delegate to the same adapter.

Bootstrap's original Fusion debug launcher and dormant runner template are preserved in the authored scene. When an active crew enters Bootstrap, the adapter disables those prototype components at runtime before their Start callbacks, so they cannot open a second session alongside the crew. Direct environment/prototype entry keeps its existing behavior.

The host owns `FusionLobbyState`. Clients send only their desired name and ready flag; the adapter identifies the row from Fusion's sending `PlayerRef`, ignoring identities and roles in a payload. Complete snapshots carry monotonic revisions, and requests have per-player revisions, so concurrent reliable streams arriving out of order cannot restore stale ready states or departed players. The host rejects updates from players absent from its roster, and freezes updates and new joins during launch. Names are limited to 24 characters and room names to 64. The versioned JSON protocol rejects malformed and oversized packets.

Lobby updates are small and infrequent. The adapter uses [Fusion reliable data streams](https://doc.photonengine.com/fusion/v2/manual/data-transfer/data-streaming), with a dedicated channel and a unique transfer key, avoiding a network prefab dependency for this first lobby iteration. Future gameplay state should use NetworkObjects, networked properties, input authority, and RPCs as appropriate. Shared launch follows [Fusion's scene loading API](https://doc.photonengine.com/fusion/v2/manual/scene-loading).

## Verification

Run the Edit Mode suite for host roster rules, stale request/snapshot rejection, late joins, name limits, malformed packets, and the asynchronous manager contract, alongside the original local lobby tests.

`FusionLobbyIntegrationTests.OfflineFusionSessionLoadsBootstrapAndSurvivesBoardUnload` runs a real Fusion Single session and scene manager without Photon Cloud. It exercises the authored lobby binding, ready flow, Bootstrap launch, persistent runner, and return to the lobby.

The opt-in Play Mode test `PhotonPeersShareRosterReadyLateJoinKickAndDisconnect` creates temporary Photon Cloud peers using the existing App ID. Set `LK_FUSION_CLOUD_TESTS=1` when starting the Unity test runner to enable it. It clones the runtime config to allow multiple peers only inside that test; the checked-in configuration remains unchanged. This test checks real reliable-data exchange, late joins, ready updates, host-only kicking, and host departure. It isolates networking from graphics and authored scenes; shared scene loading is covered separately by the offline test. Results and remaining device checks are recorded under `Docs/Verification/FusionNetworking`.

Before accepting this feature on Quest, test two independent clients (preferably two headsets): host/join with matching names and region, all-ready gating, leave/rejoin, invalid room, full room, host loss, and simultaneous Bootstrap entry with exactly one rig per client. Check a sustained network session on device and confirm the eventual gameplay synchronization work separately.
