# Sprint 1 local lobby preview

[VERIFIED] Eli's lobby code through commit `4292781` is preserved in `LK.Lobby`, including the spatial keeper watch-room screen, feedback and local roster rules. Open `Assets/_LighthouseKeepers/Scenes/Development/LK_LobbyPreview.unity` to inspect that separate development preview. It is not the environment APK's entry scene.

[VERIFIED] `LocalLobbyTransport` simulates a roster in one process; it does not connect headsets. The environment still starts from `LK_Bootstrap`. LK.Lobby explicitly references UI, TextMeshPro and XRI for Eli's updated screen and feedback, without depending on the environment runtime assembly.

The separate `LK_NetworkLobby` scene explicitly binds `FusionLobbyTransport` for a real Photon Host/Client crew roster, ready flow, and shared scene launch. Choose **Lighthouse Keepers → Networking → Play Photon crew lobby**. The installed Fusion package and existing Photon settings are used; no package installation is needed. See [the networking guide](../../../../../Docs/FusionNetworking.md) for setup, tests, and scope. Headset acceptance, networked avatars, and shared gameplay state remain pending. Keep the original lobby tests and newer polish tests as regression coverage.
