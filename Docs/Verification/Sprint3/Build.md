# Fresh One Lighthouse APK — October 27, 2026

[Candidate release](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint3-one-lighthouse-candidate-20261027) · [APK](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-one-lighthouse-candidate-20261027/LighthouseKeepers-Sprint3-89199c5a-20261027T222901Z.apk) · [checksums](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-one-lighthouse-candidate-20261027/SHA256SUMS.txt) · [Unity BuildReport summary/source record](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-one-lighthouse-candidate-20261027/LighthouseKeepers-Sprint3-89199c5a-20261027T222901Z.build.json) · [actual-binary verifier record](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-one-lighthouse-candidate-20261027/LighthouseKeepers-Sprint3-89199c5a-20261027T222901Z.verification.json).

| Field | Observed result |
| --- | --- |
| Source | `89199c5a10e5a077758e6c0b2710595afab60826`; `sourceStatus` empty |
| Unity | 6000.3.23f1 |
| Entry point | `LighthouseKeepers.Editor.OneLighthouseM4.Build` → `QuestApkPipeline` |
| Build | Ordinary **Succeeded** BuildReport; **0 errors, 7 warnings** |
| Configuration | Development + CleanBuildCache; Android ARM64, IL2CPP, OpenXR; 0.1.0(1) |
| APK size | 83,275,214 bytes |
| SHA-256 | `b6701f239acfe88c7e5be7e6d4c98ecd211f33cafa99824a025d9123ef5eba4c` |
| Actual archive | ZIP CRC passed; required ARM64 OpenXR/IL2CPP/Unity libraries present |
| Actual manifest | `com.lighthousekeepers.game`; UnityPlayerGameActivity launcher with Quest VR category/headtracking/supported-device/focus-aware entries; verifier passed |
| APK signature | Verified; APK Signature Scheme v2 |
| Upload | GitHub asset digest matches local SHA-256 |
| Post-build checkout | Clean; XR preloaded assets restored by normal successful build cleanup |
| Device | ADB reported 0 connected/unauthorized/offline devices; no install or target-headset run performed |

Actual `BuildPlayerOptions.scenes`, recorded in the source sidecar, and ASCII paths read from the APK's `assets/bin/Data/globalgamemanagers` agree on exactly:

1. `Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity`
2. `Assets/Scenes/Lab/LK_Lab_Environment.unity`

No old Bootstrap/Storm Watch content scene is present in this candidate. The three content days plus Day4 placeholder use the Lab; co-op launch remains blocked pending M5. PLAY SOLO is the supported playable route.

The seven BuildReport warnings are disclosed, not suppressed. Build output also includes retained Fusion obsolete-callback/unused-field and generated native/codegen diagnostics. The build completed normally; this is not a salvaged package or reused old APK.

Reproduce from a clean checkout of the source above (set PROJECT to its absolute path; install Unity's Android SDK/NDK/OpenJDK modules):

```sh
LK_PYTHON=/absolute/path/to/python3 /Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -projectPath "$PROJECT" -buildTarget Android \
  -executeMethod LighthouseKeepers.Editor.OneLighthouseM4.Build \
  -job-worker-count 2 -logFile /tmp/one-lighthouse-build.log
```

Use a fresh output path; the pipeline rejects existing APKs. APKs, build caches and raw Unity logs remain outside Git. Raw logs can carry authentication values; only the selected provenance records and sanitized test reports are published.

Headset immersive cold launch/restart, physical controller reach/readability/stations, sound balance, performance and real Sprint3 footage remain pending. Successful packaging and Editor tests do not establish those checks. Final main merge, `sprint3-final` tagging and final-release device/fresh-clone acceptance remain team work tracked by #61.
