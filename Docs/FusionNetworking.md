# Photon Fusion crew lobby and Storm Watch

William's Fusion launcher and the physical watch board use one Host/Client crew session for the lobby and Storm Watch. Photon is the default game transport and startup flow, using the installed Fusion SDK and the project's existing Photon settings. No new networking package is required.

## Try it

1. Open Unity **6000.3.23f1**, allow compilation to finish, and choose **Lighthouse Keepers → Play game**. The dedicated scene is `Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity`.
2. Under **Crew options**, enter your keeper name and a room name. Choose **Host crew** on one client. On another client, enter exactly the same room name and choose **Join crew**. Both clients need internet access, a working Fusion App ID, compatible builds, and the same Photon region. Set a shared Fixed Region in Photon App Settings if clients choose different regions automatically.
3. Verify both names appear on both boards. Choose **Signal ready** on each board. Only the host can choose **Begin watch**, and both keepers must be ready. The production room capacity and minimum crew are both two.
4. The host starts `LK_StormWatch` through Fusion's scene manager. The same runner survives the board and lobby rig unloading. Storm Watch loads the six content scenes locally and binds its gameplay channel to the existing session; it never opens another room. Day 1 begins automatically once both environments send valid input.
5. **Leave watch** cancels a pending connection or leaves a lobby session. A host leaving ends the crew session. In gameplay, **Return to lobby** on each floor's instruction board ends the session and returns to the network lobby. A partner leaving or losing connection ends the current run and returns the remaining keeper too, with a reconnect explanation. The next crew uses a fresh runner and an empty roster.

`LK_NetworkLobby` is the first enabled build scene and the normal Editor Play entry point. The standard development APK build includes that lobby, Storm Watch, the development Bootstrap, and the six content scenes. Recovery scene setup preserves the Photon-first order. Hosting/joining uses Photon without a transport-mode choice or a silent local fallback. **Development → Play local lobby preview** explicitly selects simulation for isolated UI work; **Development → Play environment** skips the lobby for environment work. **Play game** returns to the normal Photon entry point. The historical **Sprint 1 → Build Android environment** command remains an explicitly named environment diagnostic build; the published Sprint 1 APK predates this networking work.

## What is shared

- A real roster with the host role, names, and ready states, including a full snapshot for late joins.
- Host-only launch and kick requests. The roster gates launch on the configured minimum crew and all-ready rule.
- Connection progress, errors, cancellation, session cleanup, and a fresh runner when reconnecting.
- The transition into Storm Watch through `NetworkRunner.LoadScene`. The host closes the room during launch, so an unready late join cannot race the scene change.

`StormWatchFusionLink` adds keeper poses, interaction inputs and host-owned progression/flood/supply/rescue snapshots on a dedicated reliable-data channel of the lobby runner. The gameplay link adds/removes callbacks but never starts or disposes that runner. A fresh, valid partner input gates the simulation during loading or a traffic stall. Local tool copies and arbitrary legacy interactables are not replicated; host migration and late-join gameplay recovery remain unsupported. Device execution and sustained Quest performance still need testing.

Kicking is currently exposed through `LobbyManager.Kick(playerId)` for UI integration; the watch board has no kick button yet.

## Implementation

`LK.Networking` references `LK.Lobby` and Fusion; the lobby assembly stays independent of Fusion. `ISessionLobbyTransport` adds asynchronous status and shared launch to the existing transport seam. `LocalLobbyTransport` is explicitly selected only by local development previews. A missing Photon adapter is reported rather than silently simulating a crew. The default game scene explicitly binds the real transport on its own persistent root, and runtime initialization registers Photon for lobbies created without an explicit transport. The authored network lobby binds `Settings/NetworkLobbyConfig.asset` (capacity two, minimum two, all ready, target `LK_StormWatch`); explicit local previews retain their development configuration. The transport also enforces the Storm Watch crew/ready rules, so an alternate caller cannot bypass them. William's `FusionLauncher.StartHost` and `JoinSession` button methods delegate to the same adapter.

Bootstrap's original Fusion debug launcher and dormant runner template are preserved in the authored scene. When an active crew enters Bootstrap or Storm Watch, the adapter disables those prototype components at runtime before their Start callbacks, so they cannot open a second session alongside the crew. Direct environment/prototype entry keeps its existing behavior.

The host owns `FusionLobbyState`. Clients send only their desired name and ready flag; the adapter identifies the row from Fusion's sending `PlayerRef`, ignoring identities and roles in a payload. Complete snapshots carry monotonic revisions, and requests have per-player revisions, so concurrent reliable streams arriving out of order cannot restore stale ready states or departed players. The host rejects updates from players absent from its roster, and freezes updates and new joins during launch. Names are limited to 24 characters and room names to 64. The versioned JSON protocol rejects malformed and oversized packets.

Lobby updates are small and infrequent. The adapter uses [Fusion reliable data streams](https://doc.photonengine.com/fusion/v2/manual/data-transfer/data-streaming), with a dedicated channel and a unique transfer key, avoiding a network prefab dependency for this first lobby iteration. Storm Watch carries bounded input/state messages on its own channel and accepts monotonic input/snapshot serials from the other connected player. Shared launch follows [Fusion's scene loading API](https://doc.photonengine.com/fusion/v2/manual/scene-loading).

## Verification

Run the Edit Mode suite for Photon-first build order, explicitly authored local practice, host roster rules, stale request/snapshot rejection, late joins, name limits, malformed packets, and the asynchronous manager contract, alongside the original local lobby tests.

`FusionLobbyIntegrationTests.DefaultLobbyUsesPhotonInsteadOfLocalSimulation` verifies that an unconfigured runtime lobby resolves to Photon without adding a local transport.

`FusionLobbyIntegrationTests.OfflineCrewRunnerHandsOffToWatchAndReturnsToFreshLobby` runs a real Fusion Single session and scene manager without Photon Cloud. It verifies the production lobby configuration, rejection of solo launch, real scene handoff after a test-only Single-mode approval, one rig/runner, no second Host/Join UI, paused gameplay while the partner loads, and a fresh lobby after leaving. `OfflineFusionSessionLoadsBootstrapAndSurvivesBoardUnload` retains coverage for explicit environment previews.

The opt-in Play Mode tests `ReadyCrewLaunchesWatchOnSameRunnersAndSharesGameplay` and `PhotonPeersShareRosterReadyLateJoinKickAndDisconnect` create temporary Photon Cloud peers using the existing App ID. Set `LK_FUSION_CLOUD_TESTS=1` when starting the Unity test runner to enable it. It clones the runtime config to allow multiple peers only inside that test; the checked-in configuration remains unchanged. The connected-watch test uses two real Photon peers to check all-ready gating, launch propagation, stable runner identities, gameplay commands/state on that connection, and session leave. The generic lobby test retains late-join, kicking and host-departure coverage. It isolates networking from graphics and authored scenes; actual scene loading is covered separately by the offline tests; this does not establish two independent processes completing a scene transition. Results and remaining device checks are recorded under `Docs/Verification/FusionNetworking`.

Before accepting this feature on Quest, test two independent clients (preferably two headsets): host/join with matching names and region, all-ready gating, leave/rejoin, invalid room, full room, host loss, and simultaneous Storm Watch entry with exactly one rig per client. Check a sustained network session on device and verify flood, interaction, rescue, retry and return-to-lobby behavior through the complete run.
