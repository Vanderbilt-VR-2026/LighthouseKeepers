# Validation record — September 21, 2026

All commands ran from `/Users/aneeshvasamreddy/Desktop/Virtual Reality Design/Lighthouse Keepers`. Unity executable: `/Applications/Unity/Hub/Editor/6000.5.10f1/Unity.app/Contents/MacOS/Unity`. ADB executable: `/Applications/Unity/Hub/Editor/6000.5.10f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb`. Run only one Unity instance per project.

## Commands and evidence

The following are the actual argument forms, with `$UNITY` and `$PROJECT` standing for the exact paths above:

```sh
"$UNITY" -batchmode -nographics -projectPath "$PROJECT" -runTests -testPlatform EditMode -testResults "$PROJECT/Docs/Verification/EditMode.xml" -logFile /tmp/lk-editmode.log
"$UNITY" -batchmode -projectPath "$PROJECT" -executeMethod LighthouseKeepers.Editor.LighthousePlayerRepair.RepairAndPreview -logFile /tmp/lk-player-repair.log
"$UNITY" -batchmode -projectPath "$PROJECT" -executeMethod LighthouseKeepers.Editor.LighthousePlayVerification.Run -logFile /tmp/lk-movement-test.log
"$UNITY" -batchmode -projectPath "$PROJECT" -executeMethod LighthouseKeepers.Editor.LighthouseLighting.BakeAndPreview -logFile /tmp/lk-readable-lighting.log
"$UNITY" -batchmode -quit -buildTarget Android -projectPath "$PROJECT" -executeMethod LighthouseKeepers.Editor.LighthouseValidation.BuildAndroid -logFile /tmp/lk-android-repair.log
python3 Docs/validate_repository.py
```

The Play Mode harness exits Unity itself; do not add `-quit`. For future repeated Android validation, keep `-buildTarget Android` consistent to avoid unnecessary platform reimports. The build method runs setup and static scene validation before building.

- Edit Mode XML records four tests. PlayMode.txt records additive loading, action enablement, actual injected-input displacement/rotation, flood thresholds/reset and day application.
- AssetValidation.txt records missing-script/material/prefab checks, duplicates, groups/snapshots, sockets/profiles and 285 spiral support/head-clearance samples.
- Lighting.txt records the actual CPU bake result. Previews contains five rendered views, not headset screenshots.
- AndroidBuild.txt records the most recent build result, errors, warnings and elapsed time. Build artifacts are ignored by Git.
- DeviceVerification.md distinguishes installation/launch/log evidence from the user's visual and interaction acceptance.

## Diagnosed first-build failures

The user saw red/pink lighting, could not move/turn, and saw controller blocks. They explicitly did not report falling or a black screen.

- The right ControllerInputActionManager was incorrectly configured for smooth movement. In the Starter Assets implementation this disables its turn actions. The corrected rig enables left movement and right turning, changes manager configuration only when necessary, and explicitly leaves opposite-hand readers unused.
- Bootstrap now holds locomotion/gravity until all content scenes are present and resets to the authored safe spawn. An automated injected-input test proves editor movement and snap rotation; actual controller input still requires the device record.
- Replacing sky-derived ambient/reflection lighting with a cold flat ambient configuration, disabling environment reflections, and rebaking removed the global red/pink wash in the rendered previews. No unsupported claim of a Unity engine bug is made. Baked navigation fills improve interior readability without extra real-time lights.
- Rounded palm/finger/thumb/cuff meshes replace the box hand visuals. They are stylized static gloves, not articulated fingers or arms.

## Warnings and limits

The first build succeeded with zero errors and 977 reported warnings. Its log contained 970 shader-warning lines, largely from the unused AI inference/Sentis package. AI inference 2.6.1 was removed because no Lighthouse system uses it. AI Assistant was excluded from the initial setup as requested.

The template's experimental Unity Pipeline package can warn that no runtime pipeline configuration exists and will disable its own player integration. It is unrelated to the URP render pipeline and was not used by game systems. Unsupported unused terrain/spatial-mapping shader messages and lightmapper hardware-rasterization fallback messages were observed. These are not evidence that every visible material is correct; rendered and headset inspection remain necessary.

Short device frame-rate logs do not prove sustained performance. Stereo vignette, controller reach, audio quality, full-route collision and ten-minute thermal testing require a human headset pass. No network/voice gameplay was tested because it is intentionally absent.

## Final automated rerun

`Unity -batchmode -nographics -buildTarget Android -projectPath PROJECT -runTests -testPlatform EditMode -testResults PROJECT/Docs/Verification/EditMode.xml -logFile /tmp/lk-editmode-final.log`: 4/4 passed, zero failures. Exact executable/project paths are above.

Replacement build: succeeded, zero errors, seven warnings, 4m28s. The seven reported warnings were: one template Pipeline runtime-config warning; one Diagnostics Data/debug-symbol setting warning; and five IL2CPP large-method translation-unit notices (three TextMeshPro, two template Pipeline interpreter). No new shader-warning lines appeared in this build log. Separate unused shader support messages during import are retained as noted above.
