# Tower movement and footsteps — October 7, 2026

Issues: #32 (tower movement) and #13 (footsteps). Base: `2c9b923` on `main`; implementation branch: `codex/tower-movement-footsteps`. Unity: **6000.3.23f1**.

## Reproduced defect

The shared player inherited a GravityProvider ground-check mask of `51` (Default, TransparentFX, Water, UI). All tower floors and stair colliders use **Environment (layer 8)**, which that mask excluded. GravityProvider therefore reported airborne while the character stood on a solid floor; fall velocity could keep accumulating and the move provider continued applying its airborne input response.

`BaselinePlayMode.xml` records the original configuration: the new standing/grounding regression failed; the original traversal check passed. This separates being able to reach a floor from having correct grounding and stopping behavior.

The first per-flight audio check then exposed a second problem: the horizontal-only move could lose support while descending the ramp. `DescentBeforeSlopeFollow.xml` records **zero footsteps** on the top descent at both 30 and 72 FPS after the mask fix alone. `GroundedSlopeMovement` now follows the supporting plane before XRI applies its queued motion, preserving horizontal speed and collision while leaving independent gravity and headset tracking intact. It only acts when grounded, checks support close to the capsule, and rejects surfaces above the controller's slope limit.

## Changes

- Ground checks include Default and Environment (`257`), ignore triggers, and exclude Water, UI, Interactable and PlayerBody. Updated the shared player, unpacked DevGym rig and fallback rig authoring.
- Supported walking follows the slope through XRI’s existing body-transform queue. Flying, falls and unsupported edges retain the normal gravity path; no extra direct CharacterController.Move or camera smoothing is introduced.
- PlayerFootsteps uses completed horizontal movement after collision, a 0.65 m stride, four original 0.24-second boot impacts and the existing Player mixer group. It compensates for turning around an offset headset, excludes head bob/lean, and resets cadence for idle movement, airborne states, pauses, long frame gaps and teleports.
- Original audio is mono 24 kHz PCM, decompressed on load, about 46 KB of source WAV files. No added packages, networking changes or new audio listener.
- Authoring menu: **Lighthouse Keepers → Player → Configure ground detection and footsteps**. It updates the existing prefab/DevGym player and keeps team geometry intact.

## Executed verification

- **EditMode.xml: 38/38 passed**, including existing environment/lobby/flood tests, audio asset/routing checks for both player rigs, and distance cadence at 30/72/120 FPS.
- **PlayMode.xml: 7/7 passed** with the final slope-following code: additive bootstrap, grounded standing and stopping, actual travel versus idle/wall/fall audio, an offset-head snap turn, house/balcony doorways in both directions, and all three stair flights up/down at both 30 and 72 FPS. Each individual flight must produce at least twelve footsteps; every sampled stair waypoint must be within 0.18 m of its intended floor height.
- Authoring command completed successfully after reapplying the configuration; no duplicate audio setup.
- **Android build succeeded: zero errors, one warning.** The development APK is `Builds/LighthouseKeepers-MovementFootsteps.apk` (90,019,845 bytes); ZIP inspection confirms only `arm64-v8a` native libraries and `libil2cpp.so`. `AndroidBuild.txt` records the Unity build summary, actual APK size and SHA-256. Unity's larger `Bytes` figure includes build output beyond the compressed APK.
- The Android warning concerns existing Diagnostics Data settings: debug symbols must be SymbolTable or Full to resolve crash-report stacks. It does not prevent packaging; diagnostics settings were not changed.
- `BaselinePlayMode.xml` and `DescentBeforeSlopeFollow.xml` are intentional **before-fix failures**, retained to document the two reproduced defects.
- Logs: `/tmp/lk-player-authoring-final.log`, `/tmp/lk-player-editmode.log`, `/tmp/lk-player-playmode.log`, `/tmp/lk-player-android.log`. Reports above are the durable repo evidence.

## CLI reproduction

The installed `unity` wrapper was used for the failing baseline and prefab authoring. Its registry lookup repeatedly blocked reading unrelated iCloud projects, even with an absolute project path. The remaining runs use the same installed editor directly in batch mode. `unity run` manages `-quit` itself; the Editor executable does not.

```sh
unity --non-interactive run /absolute/path/to/LighthouseKeepers \
  --editor-path /Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app \
  -- -nographics -executeMethod LighthouseKeepers.Editor.LighthousePlayerMovementAudio.Apply \
  -logFile /tmp/lk-player-authoring.log

unity --non-interactive test /absolute/path/to/LighthouseKeepers \
  --editor-path /Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app \
  --mode EditMode --output /tmp/lk-player-editmode.xml \
  -- -nographics -logFile /tmp/lk-player-editmode.log

unity --non-interactive test /absolute/path/to/LighthouseKeepers \
  --editor-path /Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app \
  --mode PlayMode --output /tmp/lk-player-playmode.xml \
  -- -nographics -logFile /tmp/lk-player-playmode.log
```

## Device acceptance

ADB reported no connected device. These editor tests cannot certify subjective headset comfort or final sound balance. On Quest, walk the keeper-house door, all three flights in both directions, each landing and the balcony door. Release the stick on flat floors and ramps; hold against a wall; turn with the headset offset from the tracking origin. Listen for footsteps only during grounded travel, with no phantom steps on turns or while blocked, and check the mix under rain/thunder. Sustained frame rate and thermal behavior remain device checks.

Editor command-line fallback (run from the repo root):

```sh
/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform PlayMode \
  -testResults /tmp/lk-player-playmode.xml -logFile /tmp/lk-player-playmode.log
```

Use `-testPlatform EditMode` for the Edit Mode suite. Do not add `-quit` to test runs; the Unity Test Framework exits after writing the report.

Android command executed:

```sh
/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$PWD" -buildTarget Android \
  -executeMethod LighthouseKeepers.Editor.Sprint1Integration.BuildDiagnosticAndroid \
  -quit -logFile /tmp/lk-player-android.log
```

The existing build helper writes `Builds/LighthouseKeepers-Sprint1-Environment.apk`; that newly built file was renamed to `LighthouseKeepers-MovementFootsteps.apk` for this verification. It includes the seven enabled environment scenes and uses the project's ARM64 IL2CPP configuration.
