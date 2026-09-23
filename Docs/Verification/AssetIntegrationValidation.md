# Asset integration validation — 2026-09-23

Main project: `/Users/aneeshvasamreddy/Desktop/Virtual Reality Design/Lighthouse Keepers`.
Isolated inspection checkout: `/Users/aneeshvasamreddy/LighthouseKeepers-Unity63-Validation`.
Editor: **6000.3.23f1 (09d2ecc7fb28)**. Executable: `/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity`; the prior `~/UnityEditors/6000.3.23f1` path is a symlink to the same installation.

## Execution record

All commands used the Unity executable above (or its symlink), `-projectPath` with the indicated project, and `-buildTarget Android`.

| Command arguments / context | Log | Result |
|---|---|---|
| Isolated: `-batchmode -nographics -executeMethod LighthouseKeepers.Editor.LighthouseVendorImport.ImportAndInspect` | `/tmp/lk-vendor-import.log` | PASS: selected archives imported sequentially using import-completion callbacks; exact prefab/dependency inventory produced. |
| Isolated: `-batchmode -quit -executeMethod LighthouseKeepers.Editor.LighthouseVendorVariants.Preview` | `/tmp/lk-vendor-preview.log` | PASS: twelve optimized prefab candidates rendered and visually inspected. |
| Isolated: `-batchmode -quit -executeMethod LighthouseKeepers.Editor.LighthouseAssetIntegration.Apply` | `/tmp/lk-assets-apply2.log` | PASS after correcting the editor-only mixer reflection call to use AudioGroupParameterPath. |
| Isolated: `-batchmode -executeMethod LighthouseKeepers.Editor.LighthouseAssetIntegration.LightingTouchup` | `/tmp/lk-assets-final-play.log` | PASS: baked practical fills and initial integrated Play Mode checks. |
| Isolated: `-batchmode -quit -executeMethod LighthouseKeepers.Editor.LighthouseIntegratedValidation.Export` | `/tmp/lk-assets-export.log` | PASS: 345 MiB selected package exported through Unity, no vendor demo scenes/scripts. |
| Isolated: `-batchmode -nographics -runTests -testPlatform EditMode -testResults .../Docs/Verification/EditMode.xml` | `/tmp/lk-integration-editmode.log` | **9/9 passed**. |
| Main: `-batchmode -executeMethod LighthouseKeepers.Editor.LighthouseEditorTask.ImportPrepared` | `/tmp/lk-main-import.log` | PASS: import-completion callback confirmed; no dirty scenes overwritten. Main asset hashes were checked before import. |
| Main: `-batchmode -executeMethod LighthouseKeepers.Editor.LighthouseIntegratedValidation.RunAndPlay` | `/tmp/lk-main-final-play.log` | PASS initial main-project runtime/structural checks. |
| Main: `-batchmode -executeMethod LighthouseKeepers.Editor.LighthouseDynamicBatchRepair.RepairAndVerify` | `/tmp/lk-main-dynamic-repair.log` | **PASS** after finding/fixing static batching of flood/lens visuals; rebaked lighting and explicitly verified visible flood movement and rotating lens renderers. |
| Main: `-batchmode -quit -executeMethod LighthouseKeepers.Editor.LighthouseIntegratedValidation.BuildFinal` | `/tmp/lk-main-corrected-build.log` | **PASS: zero errors, one BuildReport warning; corrected APK built directly from main project.** |

The initial isolated Android candidate succeeded (zero errors, one debug-symbol warning) but is **superseded** because the dynamic-rendering regression was subsequently found. An earlier main build was deliberately stopped to apply that fix; it is not an accepted deliverable. No regression is concealed by the earlier passing control-only tests.

## Verified behavior and limits

- Seven scenes load through Bootstrap; one XR Origin, AudioListener and InputActionManager.
- Exactly sixteen puzzle sockets, six station identities, four flood thresholds and five ordered day profiles. Existing anchors/serialized events compared before/after placements.
- Continuous movement and snap turn with injected stick input; synthetic keyboard W/E through DesktopPreview; existing smooth-turn/gravity/vignette configuration preserved. Human WASD/Q/E feel and both-eye comfort remain manual.
- Flood thresholds/reset/day changes; visible flood surface follows height; lens visuals remain dynamic and rotate.
- One streamed looping soundtrack starts/fades in, uses Ambience Music with exposed -20 dB gain. Existing acoustic routing/snapshots retained. Human content/loudness/loop audition is pending.
- Three offshore fog volumes, cap8 each, outside building bounds. Ocean grid4225 vertices, opaque URP shader with stereo macros. Desktop viewpoints rendered; headset stereo and GPU cost unverified.
- Balcony rain cap220, ring coverage, no particle collision; automated outdoor test observed109 particles, no particles inside shelter, then zero after re-entering indoors. This does not prove every balcony viewpoint looks good in a headset.
- Both door spans >=1.2m, 1.2m arrival lane and east landing collision checks;285 spiral support/head-clearance samples.
- No missing scene scripts/prefabs/materials or active shader compiler errors in validation. No tracked asset GUID changes; no orphan meta files in the final file audit.

## Import/build observations

- Unity replaced the GUID of the newly authored, untracked **editor-only** LighthouseVendorImport helper during package import. No gameplay asset referenced it; tracked `.meta` files remained unchanged.
- Duplicate generated lightmap copies were found unreferenced by scene/prefab dependencies and preserved, with matching metas, under ignored `Builds/RecoveredDuplicateLighting-20260923`. Nothing was deleted. Unrelated ProjectSettings duplicates remain untouched.
- The factory inspection package automatically added Post Processing3.5.4 in the isolated checkout. It was removed from that checkout’s manifest and was never added to the main project. Main manifest/lock and URP/XR versions remain unchanged.
- Headless Unity sometimes logs an unavailable licensing access-token refresh and shutdown `build-server`/thread-finalization messages; these did not prevent licensed editor execution. They are not C# or runtime scene failures.
- The earlier keyboard test failure was fixed by routing synthetic keyboard input to Game view in Unity's test harness; it was not proof of a headset controller regression.

## Remaining acceptance

Install the corrected APK on Quest3 and check audio content/loop/mix, both-eye vignette/water/fog/rain, controller reach, the complete route and ten-minute thermal frame timing. No claim of sustained72 FPS or final artistic/audio acceptance is made. The user's “think we're good” reply refers to the earlier repaired build, not this new asset revision.

## Final main-project results

- Corrected Android development build: **Succeeded**, zero errors, one BuildReport warning, 23.16 seconds on the warmed build cache.
- Final main-project Edit Mode run: **9/9 passed**, zero failed/skipped, 2026-09-23 20:15 UTC. Command: `-batchmode -nographics -runTests -testPlatform EditMode -testResults <main>/Docs/Verification/EditMode.xml`; log `/tmp/lk-main-editmode.log`.
- Play Mode repair run passed, including visible flood surface and rotating beacon, with no runtime errors captured.
- BuildReport warning: Diagnostics Data requests SymbolTable/Full debug symbols for useful crash stack traces. Also logged: unused URP Terrain/Lit and SpatialMapping/Wireframe variants stripped/unsupported; no shader compile error was reported for the used materials.
- Package manifest/lock, Unity version and tracked asset GUIDs unchanged. No vendor scripts/demo scenes or orphan metas found.

APK: `Builds/LighthouseKeepers-Development.apk`

File size: 85,726,671 bytes (81.76 MiB). SHA-256: `1169db8ce7ca26066f1b872567164cbc3a6b20dac673de9d4ea0053a3a12f068`. Native ABIs: arm64-v8a. `aapt2 dump badging` confirms `com.lighthousekeepers.game`, label Lighthouse Keepers, debuggable, minimumAPI32, targetAPI36, ARM64 and VR head tracking.

This corrected APK has **not** been installed or tested on Quest in this integration pass.

Git review: `git diff --check` reports trailing blanks in Unity-generated serialized empty `m_Name:` / prefab `value:` fields. Scene/mixer YAML was left as Unity serialized it; no direct scene-YAML cleanup was performed. No C# whitespace error was reported. Existing unrelated ProjectSettings edits/duplicates and verification-file copies remain unstaged.

Editor handoff confirmed in `/tmp/lk-editor-handoff.log`: `LK HANDOFF: complete environment open; Play starts at Bootstrap.` Unity was left out of Play Mode with all seven content/Bootstrap scenes open.
