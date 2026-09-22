# Unity 6.3 compatibility migration

The team target is **Unity 6000.3.23f1 (09d2ecc7fb28)**. Unity 6.3.23 refers to this exact editor release. The lighthouse scenes, assets and modular systems are retained.

## Preservation and workflow

The original 6000.5 project plus the five current local settings/material edits were checkpointed in `3525b74`. Local branch `backup/aneesh-unity-6000.5-before-downgrade` preserves that state. Untracked duplicate files were backed up under the ignored `Builds/BeforeUnity63Downgrade/Untracked/` directory and excluded from the isolated test checkout. No Spear Throw VR files were modified.

Migration was developed and tested in `/Users/aneeshvasamreddy/LighthouseKeepers-Unity63-Validation`, then applied to the normal Lighthouse Keepers checkout. Publication continues on the existing team branch `aneesh/lighthouse-sprint1-foundation`; shared main is not changed.

## Compatibility changes

- URP 17.5.0 → 17.3.0, UGUI 2.5.0 → 2.0.0, Test Framework 1.7.0 → 1.6.0, matching the team's 6000.3 starter manifest.
- Retained XRI 3.6.0, OpenXR 1.18.0, XR Management 4.7.0, Core Utils 2.6.0 and Input System 1.20.0. Their declared minimum editors are at or below Unity 6000.0; actual target-editor compilation is also verified.
- Removed unused experimental Unity Pipeline and the 6000.5-only PhysicsCore2D module; restored the VR built-in module expected by the 6.3 package set. Unity regenerated the lockfile.
- Updated object searches to the explicit unsorted overload supported by 6.3.
- Cleared an obsolete URP 17.5 path-tracing managed reference using Unity serialization APIs, preserving the global-settings asset and GUID.
- Reserialized game scenes/materials/prefabs and rebaked ten lightmap atlases in the target editor. Modified existing metadata retained its GUIDs.
- Set the character controller's minimum movement distance to zero in both rigs and the player prefab. The automated test exposed discarded submillimetre movement at high editor frame rates; this also preserves small stick inputs.
- Deferred the batch Play Mode test by one editor callback so Unity's initial search database exists before Play Mode. Earlier runs exposed a SearchDatabase indexing exception when the harness entered Play Mode before startup indexing. The final run passed without that exception.
- Imported the official TMP essentials for UGUI 2.0.0. No third-party game art or audio was downloaded.

## Validation

Actual editor: 6000.3.23f1. Static validation passed (seven loaded exploration scenes, one rig/listener, 16 sockets, four flood thresholds, five day profiles, 285 stair support/head-clearance samples). Play Mode passed movement, snap rotation, additive loading, flood reset and day changes with no runtime errors during the test; five target-editor previews were rendered. Edit Mode: four tests passed, zero failures.

The current results are in `Docs/Verification/AssetValidation.txt`, `PlayMode.txt`, `EditMode.xml`, `Lighting.txt` and `AndroidBuild.txt`. Historical device records describe the earlier 6000.5 APK and must not be interpreted as testing of the migrated APK. The migrated build still requires a Quest headset acceptance pass; stereo comfort and sustained 72 Hz are not certified by desktop tests.

Exact command entry points used with the 6000.3.23f1 editor:

```text
-batchmode -nographics -quit -projectPath <validation-copy> -executeMethod LighthouseKeepers.Editor.LighthouseValidation.Validate -logFile /tmp/lk63-import.log
-batchmode -buildTarget Android -projectPath <validation-copy> -executeMethod LighthouseKeepers.Editor.LighthouseUnity63Migration.ApplyAndVerify -logFile /tmp/lk63-migration-play2.log
-batchmode -buildTarget Android -projectPath <validation-copy> -executeMethod LighthouseKeepers.Editor.LighthousePlayVerification.Run -logFile /tmp/lk63-play-final.log
-batchmode -nographics -buildTarget Android -projectPath <validation-copy> -runTests -testPlatform EditMode -testResults <validation-copy>/Docs/Verification/EditMode.xml -logFile /tmp/lk63-editmode.log
-batchmode -quit -buildTarget Android -projectPath <validation-copy> -executeMethod LighthouseKeepers.Editor.LighthouseValidation.BuildAndroid -logFile /tmp/lk63-android.log
```

The migration menu is explicit and never runs automatically for teammates. Normal use is simply opening LK_Bootstrap and pressing Play.

## Local editor installation

Unity Hub's download failed DNS resolution and left the editor paused. A separate recovery download from Unity's official release URL matched the Hub-published size/checksum and passed macOS certificate verification. The editor and matching Android module were extracted from those verified Unity installers into:

`/Users/aneeshvasamreddy/UnityEditors/6000.3.23f1/`

The installed Android tools were cloned from the existing editor after verifying matching versions: NDK r27c (27.2.12479018), OpenJDK17.0.18+8, SDK Build Tools36.0.0. Original tools and Unity6000.5 remain untouched. These installation files are outside the repository.

If Unity Hub does not list this editor automatically, use **Installs → Locate** and choose `Unity.app` in that directory. Teammates who already have 6000.3.23f1 need only update the branch and open the project. Build support/SDK/NDK/OpenJDK are required for Android builds, not desktop environment preview.

## References

- Official release and installers: https://unity.com/releases/editor/whats-new/6000.3.23f1
- Unity's reference source explains why the default search index cannot be created after Play Mode starts: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/QuickSearch/Editor/Indexing/SearchDatabase.cs

No networking, voice, full puzzle logic, final audio or device-performance claims were added by this migration.

## Android build result

Unity 6000.3.23f1 produced a development ARM64 APK successfully: zero errors, one BuildReport warning (Android diagnostics requested separate debug symbols for crash stack traces), elapsed 7m52s; APK approximately 66 MiB. BuildReport totalSize includes build outputs and is not APK size. The log also includes package shader-stripping messages for unused terrain/spatial-mapping shaders and native/TMP compilation advisories. These do not establish a device rendering failure. No new headset test was performed.
