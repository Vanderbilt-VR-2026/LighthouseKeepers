# Repeatable Sprint 3 Quest APK build

This pipeline builds the current ship player from the current checkout: the Network Lobby followed by the existing Lab world. It does not establish headset launch, controller usability, two-device networking, or comfort acceptance. Record those separately in the Sprint 3 submission evidence.

## Prerequisites

Use Unity **6000.3.23f1**, with Android Build Support, SDK, NDK and OpenJDK installed. Complete package import first. Python 3 is needed for the final binary verifier; set `LK_PYTHON` to its absolute executable path if `python3` is unavailable to the Editor. The build uses the SDK/JDK configured in Unity External Tools.

In Android XR Plug-in Management, keep OpenXR and **Initialize XR on Startup** enabled. Enable **Meta Quest Support**, at least one target Quest device, and **Oculus Touch Controller Profile** in Android OpenXR. Keep Internet permission enabled for networking. Player settings must use IL2CPP and include ARM64. Disable App Bundle and Export Project for this APK command. The guard reports configuration problems; it does not silently rewrite XR settings.

## Build

Finish and review changes before building. Close other Editors using this checkout. Run the installed Unity Editor in batch mode (or pass the equivalent arguments through the installed Unity CLI):

```sh
"/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit -projectPath "$PWD" -buildTarget Android \
  -executeMethod LighthouseKeepers.Editor.StormWatchAuthoring.BuildQuest \
  -logFile /tmp/LighthouseKeepers-Sprint3-build.log
```

After switching the active platform to Android, these entry points all use the same validated pipeline and two-scene manifest:

- **Lighthouse Keepers → Storm Watch → Build Quest APK** (`StormWatchAuthoring.BuildQuest`). The menu name is retained for compatibility; it builds the current ship world.
- **Lighthouse Keepers → Sprint 3 → Networking → Build Quest APK** (`MultiplayerBuild.BuildQuest`).
- **Lighthouse Keepers → Sprint 3 → Networking → Build release Quest APK** (`MultiplayerBuild.BuildReleaseQuest`).
- `LighthouseKeepers.Editor.OneLighthouseM4.Build`.

Every ship APK must contain exactly `Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity` first and `Assets/Scenes/Lab/LK_Lab_Environment.unity` second. The same existing Lab world is used by both launch modes. Both committed scene files must exist; building does not author or copy a replacement world. `ShipWorld` holds the central scene paths and `ShipSceneManifest` validates the actual `BuildPlayerOptions.scenes`, including custom Android build callers. Correct global Build Settings cannot hide a lobby-only options array, an omitted Lab, a wrong entry, extra scenes or reversed order.

Networking prefab validation is read-only during builds. Use the separate **Bake network prefabs** authoring menu if baking is needed, then review and commit those changes before building.

The default entry points create a Development APK; the Networking release menu omits the Development flag. Both modes retain clean build caching and identical verification, using the project's configured signing settings.

Each invocation defaults to a new `Builds/Sprint3-<commit8>-<UTC>/LighthouseKeepers-Sprint3-<commit8>-<UTC>.apk`. Set `LK_APK_OUTPUT` to choose a different **fresh** `.apk` path. Existing output is rejected, so a failed build cannot accidentally hand off an old binary. Clean build cache is requested. A zero-error successful BuildReport and an actual output file are required.

The sibling `.build.json` records the commit, dirty checkout status, exact scene list, Unity/player versions, actual build options and timestamps. A dirty checkout is explicitly recorded; a commit in a filename alone is not proof that the binary contains only that commit. Preserve this evidence with the APK.

## Packaging gate

After Unity builds, `Tools/verify_quest_apk.py` checks the actual binary using Android SDK `aapt2` and `apksigner`:

- ZIP CRC integrity and required ARM64 OpenXR/IL2CPP libraries.
- Expected application id and multiplayer Internet permission.
- Required headtracking feature, configured Quest device metadata, VR category on every Unity MAIN/LAUNCHER filter, and focus awareness on the launch activity.
- APK signature validity and SHA-256.

The final Gradle hook preserves valid existing markers and repairs missing Quest markers only on standard Unity Activity/GameActivity launchers, using configured target devices. Unsupported launch activities fail for review. It runs after Unity package hooks; no custom manifest template or redundant XR package is added.

On success the verifier writes a sibling `.verification.json` including the successful build record. A failed packaging check fails the build command even if compilation and Gradle succeeded. To independently repeat verification:

```sh
python3 Tools/verify_quest_apk.py /absolute/path/to/fresh.apk \
  --build-tools /absolute/path/to/Android/SDK/build-tools/36.0.0 \
  --java-home /absolute/path/to/AndroidPlayer/OpenJDK \
  --source-record /absolute/path/to/fresh.build.json
```

The verifier defaults to package `com.lighthousekeepers.game`; pass `--package` if intentionally changing the application id. Published APKs and generated build artifacts stay out of Git. Upload the new APK and evidence to the Sprint 3 release/submission location and link them from the Sprint 3 README. Never relabel the earlier Main APK as this build.

## Regression and device acceptance

Run `python3 -m unittest discover -s Tools/tests -v`. Unity Edit Mode tests in `LK.QuestBuild.Tests` cover missing VR marker repair, idempotency, both supported activity classes, unrelated activity isolation and invalid launchers. Scene-manifest tests cover correct paths/order, omitted Lab despite correct global settings, wrong entry/world, extra/duplicate scenes and empty input. These are packaging regressions, not device acceptance.

Then install the freshly verified APK on the target Quest and confirm it opens as immersive VR. Complete controller-only crew entry, host/join, both players Signal Ready, Begin Watch, disconnect/retry and a two-device watch. Record headset/OS, APK SHA, exact steps/results and remaining failures. Capture a video demonstrating this same APK's changes. Link the APK, video, start sequence and acceptance results from the Sprint 3 README before marking submission complete.
