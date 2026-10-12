# Keeper lobby and cooperative sessions

[VERIFIED] Eli's lobby code through commit `4292781` is preserved in `LK.Lobby`, including the spatial keeper watch-room screen, feedback and local roster rules. Open `Assets/_LighthouseKeepers/Scenes/Development/LK_LobbyPreview.unity` to inspect that separate development preview. It is not the environment APK's entry scene.

[VERIFIED] `LocalLobbyTransport` simulates a roster in one process; it does not connect headsets. The environment still starts from `LK_Bootstrap`. LK.Lobby explicitly references UI, TextMeshPro and XRI for Eli's updated screen and feedback, without depending on the environment runtime assembly.

The runtime transport provider now supplies the included Photon Fusion launcher by default during play. The board supports real room joining, shared ready status and launch for up to four players. An explicitly assigned `LocalLobbyTransport` still takes precedence for offline UI checks. See [multiplayer setup and verification](../../../../../Docs/Multiplayer.md). Live four-device and headset acceptance remain unverified; keep the original local lobby tests as regression coverage.
