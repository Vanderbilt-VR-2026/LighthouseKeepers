# Sprint 1 local lobby preview

[VERIFIED] Eli's lobby code through commit `4292781` is preserved in `LK.Lobby`, including the spatial keeper watch-room screen, feedback and local roster rules. Open `Assets/_LighthouseKeepers/Scenes/Development/LK_LobbyPreview.unity` to inspect that separate development preview. It is not the environment APK's entry scene.

[VERIFIED] `LocalLobbyTransport` simulates a roster in one process; it does not connect headsets. The environment still starts from `LK_Bootstrap`. LK.Lobby explicitly references UI, TextMeshPro and XRI for Eli's updated screen and feedback, without depending on the environment runtime assembly.

[UNRESOLVED] Networking transport and headset acceptance of the lobby remain open. Do not infer multiplayer support or permission to install networking packages. Keep the original lobby tests and newer polish tests as regression coverage.
