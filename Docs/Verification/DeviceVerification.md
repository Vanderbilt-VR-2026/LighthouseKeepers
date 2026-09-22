# Quest 3 device record

Replacement APK installed successfully with `adb install -r Builds/LighthouseKeepers-Development.apk` and launched with `adb shell am start -n com.lighthousekeepers.game/com.unity3d.player.UnityPlayerGameActivity` on September 21, 2026. Product remains **Lighthouse Keepers**, independent of Spear Throw VR.

- Android development build: succeeded, zero errors, seven warnings, 4m28s. ARM64/IL2CPP, minimum API32, resolved target API36, Quest Android14/API34.
- Runtime log confirms six additive content scenes loaded, safe spawn established, move and turn providers/actions enabled.
- The replacement capture reports `Mounted state at BeginSession: False` and head/left/right tracking false. It therefore does **not** verify physical stick input, headset rendering or worn-headset performance. Application telemetry is approximately72 FPS while unworn, which is not accepted as the performance target being met.
- The earlier worn first-build view reported72Hz but the user rejected its colours, controls and hands; they explicitly reported no falling or black screen.
- The user has been asked to test the replacement. Visual/controller acceptance is pending; do not mark the original failures resolved on-device from these logs alone.
- A startup Unity-tagged Java `ClassNotFoundException` names `com.google.android.play.core.assetpacks.AssetPackManager`. Startup continues afterward and reaches LK_READY. This non-fatal observed message remains documented; no unrelated Google Play asset-delivery dependency was added to the Quest APK. The project does not use split application binaries.
- Quest OS logs also report unavailable head/hand tracking streams while unworn. Do not infer working tracked input from enabled action maps.
- ADB screencap previously returned black although the user could see the app. It is not usable visual evidence here.

Sanitized game telemetry is in QuestRuntimeExcerpt.txt. Exact log capture: `adb logcat -d --pid=29165 -t 5000 > /tmp/lk-device-repair.txt`. PID is session-specific. Full local log is deliberately not committed because it contains device/network metadata.

APK SHA-256: `5087da3dea87e6528338d51ee25e714b7d8c0a40b41abdb721809a2018bad2d5`.

Still required: user confirmation of cold colours, left walking, right turning and glove visibility; full-route collision, grabbing, both-eye vignette, seated reach, audio acceptance, and a ten-minute thermal/frame-time test.

Follow-up at22:07 local: head tracking became true; both controller tracking flags remained false, and both sampled stick values remained zero. Move/turn providers and action maps stayed enabled. The user was prompted to wake both controllers. This narrows the pending hardware check but does not establish successful locomotion.
