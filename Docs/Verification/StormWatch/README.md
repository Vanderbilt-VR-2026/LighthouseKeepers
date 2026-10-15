# Storm Watch verification

Executed with Unity **6000.3.23f1** on macOS on October 13, 2026. This is the isolated two-player `LK_StormWatch` entry. Reports omit runtime log output; counts, individual test results and assertion diagnostics are retained.

## Executed checks

- **Edit Mode: 61/61 passed** on the final gameplay source. Includes the three-watch flow, distinct player roles, proximity, blackout and pumping, support grace/release/cooldown, preserved repair checkpoints, shared refuge choices, radio dependency, both-survivor victory, lifeline aim/catch/pull, rescue failure/retry/replayed-command prevention, invalid tuning and disconnect pause, plus existing regressions. See `EditMode.xml`.
- **Full offline Play Mode: 14 passed, 0 failed, 1 online test deliberately skipped.** Exercises bootstrap, player traversal and grounding, earlier First Watch interactions, and Storm Watch scene loading, gate opening and physical pumping. See `PlayMode.xml`.
- **Final focused Play Mode with graphics and real Photon connectivity: 3/3 passed.** Rechecks the updated Storm Watch scene and physical pumping, verifies the accessible disconnect/Leave Crew flow and entry relocation, and connects two real Fusion host/client runners. Tests command retention across pose packets, authoritative state delivery and pause after the client leaves. Both runners use the production single-scene transport mode and unique temporary rooms; this is not a two-device network test. See `OnlineAndScene.xml`.
- **Rendered scene inspection:** `Entry.png`, `Plumbing.png`, `Generator.png`, `Radio.png`, `Lantern.png`, `NightExterior.png`. Removed the persistent exterior directional light, checked the night ocean, adjusted upper-floor board placement and button contrast. TMP overflow checks passed. Captures use an Editor verification camera and an accelerated Day 1 repair fixture; they are not screenshots of a complete human run.
- **Fresh Quest APK: packaged and verified.** ARM64 IL2CPP/OpenXR, valid APK v2 signature, archive CRC passed, internet permission present; 90,103,459 bytes. Unity compilation completed, but its Gradle packaging step ran out of disk space. After removing only disposable compiler objects from this worktree, the same generated Gradle project packaged successfully in 37 seconds. See `AndroidBuild.txt` for the exact recovery, source commit and SHA-256. This does not label the original failed Unity BuildReport as successful.
- `git diff --check` and script/meta pairing checked before publication.

The final focused pass follows the full offline pass because board placement and disconnect recovery were subsequently refined. The final Edit Mode pass additionally verifies that retained network commands cannot replay after a retry. The historical First Watch evidence directory describes its earlier local slice, not the new online experience.

## Reproduce

Use the installed Unity executable and this checkout as `-projectPath`:

```sh
Unity -batchmode -nographics -runTests -testPlatform EditMode -testResults /tmp/storm-edit.xml
Unity -batchmode -nographics -runTests -testPlatform PlayMode -testResults /tmp/storm-play.xml
LK_RUN_ONLINE_TESTS=1 LK_CAPTURE_WATCH=1 Unity -batchmode -runTests -testPlatform PlayMode -testFilter StormWatch -testResults /tmp/storm-online.xml
Unity -batchmode -nographics -executeMethod LighthouseKeepers.Editor.StormWatchAuthoring.BuildQuest -quit
```

The online test is opt-in and requires access to the project's configured Photon app. `LK_CAPTURE_WATCH=1` overwrites these scene captures; omit it for ordinary checks. The Android helper emits `Builds/LighthouseKeepers-StormWatch.apk` and a build summary. APKs and Unity caches are not committed.

## Headset acceptance still required

Run with two keepers on Quest/Quest or Quest/Editor. Verify physical reach, direct grip versus ray buttons, release/regrip, wrench targeting, radio calls, lifeline catch/pull and rescue blink in both eyes. Measure a complete three-watch run, targeting roughly ten minutes total, and adjust the exposed tuning from those observations. Check darkness and reading comfort, actual audio/haptics, packet delay/loss and disconnect recovery across devices. Profile sustained CPU/GPU frame times and allocations on the target headset before making performance claims.

Known prototype limits: local tool copies rather than shared item ownership; no integrated voice (radio call buttons provide a fallback); no host migration or late-join recovery; simple station/control art. Pressure coordination and additional days are deferred. The minimal crew-room UI remains separate from the lobby PR.
