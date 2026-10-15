# First Watch verification — October 8, 2026

Base `8ba1154`; implementation branch `codex/first-watch-loop`; Unity **6000.3.23f1**.

- `EditMode.xml`: 43/43 passed. Includes the five new rule tests and existing environment, lobby, flood and movement/audio asset checks.
- `PlayMode.xml`: 9/9 passed. Includes the initial two First Watch scene-flow tests and all seven existing bootstrap/traversal tests.
- `FocusedPlayMode.json`: final 3/3 passed after adding spray shutdown/reset, seal haptics, corrected valve text bounds, and simulated two-hand interaction. The Pipeline collector repeats each result record; its summary and the three unique test names are authoritative, not the array length.
- The physical-interaction check selects the authored brace and wheel through XRI, rotates a nearby simulated hand until the seal locks, and verifies that moving the brace hand away releases support. It does not establish controller comfort or haptic feel.
- Live CLI inspection: seven scenes, one XR Origin, four physical interactables (three valves plus brace), three world-space boards. Final TMP overflow check returned an empty list.
- `Briefing.png`, `SealControls.png`, and `Landing.png`: rendered from representative standing viewpoints in the running scene. These are in-engine captures, not concept art. Quest eye resolution, reach and comfort remain unverified.

The first interactive test attempt was cancelled because the preview's `playModeStartScene` override redirected Unity away from its test scene. The First Watch launch helper now clears that override and uses the already-open entry scene. The subsequent focused run passed.

Logs remain local: `/tmp/lk-firstwatch-author.log`, `/tmp/lk-firstwatch-editmode.log`, `/tmp/lk-firstwatch-playmode.log`, `/tmp/lk-firstwatch-preview.log`. The first authoring import logged an existing Photon configuration creation exception; the scene was authored successfully and subsequent executed tests passed. No Photon session is launched by this slice.

Android development build succeeded with zero errors and two warnings. The 114,606,554-byte APK contains ARM64 IL2CPP and the First Watch entry in level0. Build details, warning explanations and SHA-256 are recorded in `AndroidBuild.txt`. Installing and testing on Quest remains pending.

Reproduce the build from the project root with the Editor closed:

```sh
/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$PWD" -buildTarget Android \
  -executeMethod LighthouseKeepers.Editor.FirstWatchAuthoring.BuildAndroid \
  -quit -logFile /tmp/lk-firstwatch-android.log
```

The initial detached build in the interactive Editor was cancelled after it stopped making progress during the test-to-build handoff. Packaging was restarted through the batch command above. The Pipeline bridge remains configured for Editor inspection; runtime Pipeline access is not enabled in the Quest player.
