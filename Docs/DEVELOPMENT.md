# Run from Unity

[← Project home](../README.md)

Players only need an APK. These instructions are for developers.

## Open the project

1. Install **Unity 6000.3.23f1** in Unity Hub, with **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK**. Keep this exact Editor version.
2. Clone the team repository into a local folder outside cloud sync:

   ```sh
   git clone https://github.com/Vanderbilt-VR-2026/LighthouseKeepers.git
   cd LighthouseKeepers
   ```

3. Choose the source you need. `main` contains the submitted solo game. For current co-op development, run `git switch coop/basics`. For the exact preview binary source, use `git switch --detach coop-preview-20261102`.
4. In Unity Hub, **Add project from disk** and choose the folder containing `Assets`, `Packages` and `ProjectSettings`. Open it and allow package download, import and compilation to finish.
5. Photon Fusion and its app settings are committed under `Assets/Photon`; package manifests are committed under `Packages`. Do not install another Fusion SDK or replace the shared app settings.
6. Choose **Lighthouse Keepers → Play game**. It opens `LK_NetworkLobby`. Press Play in the Editor if needed, then **PLAY SOLO** for offline Lab gameplay.

The shipped scene pair is **Network Lobby → LK_Lab_Environment**. Bootstrap and Storm Watch are historical scenes, not this download's game world.

## Editor + Quest co-op

Use `coop/basics` or the exact preview tag in the Editor and the matching preview APK on Quest. Follow the [host/guest steps](../README.md#play-together--two-people); make the Editor the host. Click the Game view for WASD movement; connected co-op enables right-mouse look. Default desktop hand offsets replicate, but a keyboard is not a substitute for tracked controller station interactions.

Both use fixed Photon region `us`, maximum two players. Keep the Mac awake and Unity in Play Mode while playing. Use Quest party voice or speak aloud; no in-game voice transport is provided.

## Build and verify

Choose **Lighthouse Keepers → Storm Watch → Build Quest APK**. The historical menu name invokes the current ship pipeline (`StormWatchAuthoring.BuildQuest`); it builds Network Lobby + Lab, not the old Storm Watch world. Output includes a source-stamped APK, build manifest and package verification. See [build pipeline](Verification/StormWatch/QuestBuildPipeline.md).

Open **Window → General → Test Runner** for EditMode and PlayMode. Latest recorded preview validation: EditMode **218/218**; PlayMode **38 passed and 15 pre-existing failures**, with no new regressions. The identical solo Day 1–3 graphics replay passed. [Exact failures and proof](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/blob/coop/basics/Docs/Verification/CoopBasics/README.md).

A successful compile or Editor run does not establish two-Quest acceptance. Preserve the accepted solo path, scene assets, hand rotation, and release tags when developing co-op. Read [agent/repository guidance](../.AGENTS/README.md) before automated changes.
