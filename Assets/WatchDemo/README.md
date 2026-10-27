# Local watch demo

This isolated prototype adds the requested repair/sleep loop to `LK_Lab_Environment`, reached through `LK_Lobby_Environment`. It does not replace teammate simulation, lobby, networking, or the production scene list. No new packages or downloaded assets are required.

## Play

Open `Assets/Scenes/Lab/LK_Lobby_Environment.unity`, enter Play, host/ready/start using the existing lobby. Spawn remains in the house. Read the board, grip the desk wrench, carry it to the single pipe on F1, and press trigger while the wrench is within 0.8 m of the leak. Hold it nearby for the automatic 3.5-second tightening animation; dropping or moving away cancels it. The board checks the repair and directs you to the third-floor bed. Near the bed, release then press grip to sleep. A two-second fade advances the day immediately without moving the player.

Each watch has a 420-second repair deadline; its elapsed timer clamps at 07:00. If the pipe remains unrepaired at that point, the flood pauses and trigger retries the current day. A repaired watch can be ended immediately by sleeping; there is no seven-minute wait. Days 2 and 3 repeat the same task and sound, not new content. Session state is not saved.

## Implementation

- `WatchState` owns only the local watch timer/checklist/day state. `WatchDemoController` connects that state to controller input and the authored objects.
- Eli's unchanged `FloodController` and `FloodSurface` remain the flood implementation. Initial water height 0.02 m; rise 0.0015 m/s with 0.1 surge per metre; maximum 0.65 m. Repair stops inflow and drains at 0.012 m/s to -0.03 m. This is a simple height-based demonstration, not the older volume/pump prototype.
- Wrench uses XRI `XRGrabInteractable`, an authored grip attachment, Rigidbody and simple collider. The automatic animation moves its visual child only. No finger rig or multiplayer repair is implied.
- The sleep transition is one second out, 0.2 seconds black, 0.8 seconds in. Head/player transforms are never moved. Battery bars are decorative, not a real headset charge reading.
- Original rain material, procedural cloudy storm sky, storm-ocean shader and rain/ocean/thunder/drip clips are reused. Weather culls indoor rain and changes indoor/outdoor filtering. Whoosh is an original deterministic generated WAV, not a downloaded recording.
- Grain is a subtle stereo-aware overlay without edge darkening. Software tunneling vignette objects are disabled in preview scenes. Existing unrelated comfort edits are preserved.
- Separate Half lighting presets contain half-energy baked maps, ambient and probes; authored light intensities and preview fixture emission are halved idempotently. The source EXRs are read as uncompressed linear HDR, and the output preserves the original lightmap import encoding. Original presets are retained.

## Authoring / verification

`LighthouseKeepers.WatchDemo.Editor.WatchDemoBuilder.Build` regenerates only the demo root and its HUD in the existing environment preview. It preserves the shell, weather, lighting and rig. Do not run the full shell generator to update this demo: that generator deliberately reconstructs the preview scene.

Tests cover deadline/retry/immediate sleep, XRI selection plus repair/sleep state transitions, lobby entry and walking the authored route. Reports: `artifacts/atmosphere/{editmode,playmode}.xml`; build log: `artifacts/atmosphere/build.log`. APK target: `artifacts/builds/LighthouseKeepers-Environment-BatchA.apk`.

Headset verification remains required: physical grip pickup/attachment, trigger repair, controller comfort, task text visibility, flood appearance, bed approach, same-position waking, grain tolerance, weather/audio balance and sustained frame rate. The Quest was disconnected during implementation. Automated selection/state tests do not verify controller feel.

## Dim-light target change

The requested 50% lighting preview uses floor targets of 0.06 (powered) and 0.02 (emergency) times the original frozen reference, at unchanged exposure. It is not certified against the older 0.12/0.04 targets: the original-target run reported 78 emergency samples below that threshold, with powered samples passing. That result is retained in `artifacts/atmosphere/original-brightness-target-check.json`. The production/full-bright presets retain their original checks. Do not treat the preview thresholds as headset acceptance.

Command-line checks (Unity 6000.3.23f1; run with the editor closed):

```sh
Unity -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults artifacts/atmosphere/editmode.xml -logFile artifacts/atmosphere/editmode.log
Unity -batchmode -nographics -projectPath . -runTests -testPlatform PlayMode -testFilter LighthouseKeepers.Presentation.Environment.Tests -testResults artifacts/atmosphere/playmode.xml -logFile artifacts/atmosphere/playmode.log
Unity -batchmode -quit -projectPath . -executeMethod LighthouseKeepers.Editor.Environment.EnvironmentBuild.BuildAndroid -logFile artifacts/atmosphere/build.log
```

Use the installed Unity executable in place of `Unity`; the build command needs graphics for its lighting capture. When a Quest is connected and USB debugging is authorized:

```sh
adb install -r artifacts/builds/LighthouseKeepers-Environment-BatchA.apk
adb shell monkey -p com.lighthousekeepers.game -c android.intent.category.LAUNCHER 1
```

## Haptics

Wrench grip sends one 60 ms pulse at 0.3 amplitude; completing repair sends one 120 ms pulse at 0.6 to the selecting hand. There is no vibration while holding or tightening. Default rig hover/grab feedback is disabled on this preview instance to prevent duplicate pulses; shared player prefabs and lobby feedback are unchanged. Play Mode verifies pulse requests/counts, not physical haptic feel.
