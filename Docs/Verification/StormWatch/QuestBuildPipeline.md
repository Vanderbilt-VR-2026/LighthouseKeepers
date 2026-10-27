# Repeatable Sprint 3 Quest APK build

This pipeline builds a fresh Storm Watch player from the current checkout. It does not establish headset launch, controller usability, two-device networking, or comfort acceptance. Record those separately in the Sprint 3 submission evidence.

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

The same entry is available under **Lighthouse Keepers → Storm Watch → Build Quest APK** after switching the active platform to Android. It supplies the lobby first, Storm Watch second, and required additive environment scenes explicitly. The local First Watch rehearsal is not the submission entry.

The command creates a Development APK using the project's configured signing settings.

Each invocation defaults to a new `Builds/Sprint3-<commit8>-<UTC>/LighthouseKeepers-Sprint3-<commit8>-<UTC>.apk`. Set `LK_APK_OUTPUT` to choose a different **fresh** `.apk` path. Existing output is rejected, so a failed build cannot accidentally hand off an old binary. Clean build cache is requested. A zero-error successful BuildReport and an actual output file are required.

The sibling `.build.json` records the commit, dirty checkout status, exact scene list, Unity/player versions and timestamps. A dirty checkout is explicitly recorded; a commit in a filename alone is not proof that the binary contains only that commit. Preserve this evidence with the APK.

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

Run `python3 -m unittest discover -s Tools/tests -v`. Unity Edit Mode tests in `LK.QuestBuild.Tests` cover missing VR marker repair, idempotency, both supported activity classes, unrelated activity isolation and invalid launchers. These are packaging regressions, not device acceptance.

Then install the freshly verified APK on the target Quest and confirm it opens as immersive VR. Complete controller-only crew entry, host/join, both players Signal Ready, Begin Watch, disconnect/retry and a two-device watch. Record headset/OS, APK SHA, exact steps/results and remaining failures. Capture a video demonstrating this same APK's changes. Link the APK, video, start sequence and acceptance results from the Sprint 3 README before marking submission complete.
