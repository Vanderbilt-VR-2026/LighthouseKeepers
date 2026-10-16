# Keeper lobby and cooperative sessions

[VERIFIED] Eli's lobby code through commit `4292781` is preserved in `LK.Lobby`, including the spatial keeper watch-room screen, feedback and local roster rules. Choose **Development → Play local lobby preview** to inspect the explicitly configured local development preview. It is not the environment APK's entry scene.

[VERIFIED] `LocalLobbyTransport` simulates a roster in one process; it does not connect headsets. The environment still starts from `LK_Bootstrap`. LK.Lobby explicitly references UI, TextMeshPro and XRI for Eli's updated screen and feedback, without depending on the environment runtime assembly.

The default `LK_NetworkLobby` entry scene explicitly binds `FusionLobbyTransport` for a real Photon Host/Client crew roster, ready flow, and shared scene launch. Choose **Lighthouse Keepers → Play game** for the two-player Storm Watch experience. The installed Fusion package and existing Photon settings are used; no package installation is needed. See [the networking guide](../../../../../Docs/FusionNetworking.md) and [Storm Watch](../../../../../Docs/StormWatch.md) for scope and verification limits.

William's four-player development environment uses `LK_Bootstrap`. `KeeperReplicationSession` shares that same `FusionLobbyTransport` runner for head/hand poses and grabbable prop state; it does not register another default lobby provider or own another runner. See [the development multiplayer guide](../../../../../Docs/Multiplayer.md). Explicit `LocalLobbyTransport` assignment retains offline UI practice. Keep the existing local, asynchronous-session and gameplay tests as regression coverage.
