# Sprint 3 — Session B tuning and handoff

Status: **[PROPOSED] for device acceptance**. Source and managed tests exist; no APK was produced and no headset test ran. These numbers are proposed from deterministic tests, not device-tuned values. The current scenes still use legacy water writers until C/D/E integrate the replacement.

For the October 21 command additions, loaded-assembly reflection tripwire, current verification scope and exact binding signatures, see **[SimulationAPI.md](SimulationAPI.md)**. The original-run evidence below predates that API update.

| Asset field | Value | Units / purpose |
| --- | ---: | --- |
| `leakInflowM3PerSec` | 0.180 | m³/s while active |
| `manualPumpStrokeM3` | 0.200 | m³ per accepted completed down-stroke |
| `manualPumpMinStrokeSeconds` | **1.20** | seconds between accepted strokes, shared by actors |
| `electricPumpM3PerSec` | 0.520 | m³/s at full power |
| `floorAreaM2` | 47.8 | m² |
| `failHeightM` | 0.90 | metres above the injected F1 origin |
| `safeHeightM` | 0.15 | metres above the injected F1 origin |
| `failSustainSeconds` | 3 | continuous seconds at/above the failure line |
| `electricPumpStartPower` | 0.35 | minimum available power to start |
| `initialFuelSeconds` | 900 | seconds of generator runtime in a full tank |

The source of tuning is `Assets/Data/Tuning/FloodTuning.asset`. Immutable core parameters are copied from it when the driver starts; restart the driver/Play Mode after editing tuning. ResetRun resets state with that same parameter copy.

The supplied 0.200 m³ / 0.55 s combination would pump 202% of leak inflow, violating the design. Retaining stroke volume and changing the minimum to 1.20 s gives **0.1667 m³/s**, **92.59%** of inflow. Maximum sustained manual pumping still leaves **+0.01333 m³/s**. Electric throughput is **3.12×** manual throughput. Unpumped rise is **0.003766 m/s**, with no upper clamp, target easing or height-dependent inflow. Volume alone floors at zero.

Managed execution measured idle failure at **242.0 s (4:02)** from dry, and electric recovery from the failure line to safe height after repair at **69.0 s**. The recoverability test sweeps 24 volume/power/leak combinations, including a flooded generator; repairing then manually pumping produces negative actual flow for positive-volume states. At the dry boundary, repair succeeds at zero volume: negative flow there would contradict the mandatory volume floor. A negative rate alone does not guarantee escape from an already imminent failure.

The snapshot's `WaterHeightM` is world Y: injected F1 origin + volume / floor area. Failure/safe thresholds are depths relative to that origin. Generator flooding compares that world Y against C's measured plinth top. Missing plinth geometry disables power and produces a readable start rejection; there is no assumed generator-root offset. Fuel burns only while the generator runs. Faults/flooding/fuel exhaustion stop the generator and pump; restoration requires explicit generator and pump commands.

Commands apply FIFO at the next 10 Hz tick, with accepted manual strokes subtracting volume at that boundary. Outflow is actual removal during the tick, including the manual impulse. ETA assumes no future strokes and persistent current power; it is a projection, not a promise about future input/fuel. Terminal phases freeze integration, but ticks/events continue. BeginDay reopens the leak and preserves volume/power/fuel; ResetRun starts dry with generator/pump off. No component is automatically added to scenes.

## Evidence and limits

- Tests were written before core implementation. The four required balance tests load the asset through AssetDatabase in EditMode. The optional standalone runner reads those same serialized float values and invokes the same NUnit test methods without invoking Unity native APIs; it does not run their AssetDatabase setup. Results are in `artifacts/standalone/results.txt`.
- **22 NUnit test methods passed**, with zero failures, in the standalone managed run; the global networking pipeline test was not run. Core, Unity adapter, development panel, Editor builder and test sources compile with Unity's bundled C# compiler and installed libraries. Core and adapter also compile with release defines. These checks do not prove Unity asset import, scene wiring, Android/IL2CPP compilation or UI behavior.
- Every meaningful source group received the requested Unity batch compile attempt. Unity startup fails before import/compilation because its Package Manager cannot open `/tmp/Unity-Upm-*.sock` (`EPERM`); licensing/player IPC also reports sandbox errors. The Android build attempt hits the same failure. Logs: `artifacts/compile.log`, `artifacts/tests.log`, `artifacts/build.log`.
- `adb devices -l` cannot start its listener (`Operation not permitted`). No device connection, install or headset execution is claimed.
- The global networking IL tripwire requires Unity's compilation pipeline and was not executed. Existing base Fusion code violates its intended boundary; it has no legacy exemptions. The new pure core's independent no-Unity/no-networking assembly-reference test passes.
- The initial checkout HEAD matches `sprint3/base` commit `4391afd`. Creating `sprint3/simulation` in the shared git metadata fails because it is outside writable roots (`simulation.lock: Operation not permitted`). Publication therefore uses a separate writable git directory with read-only access to the base objects; the original checkout's detached HEAD remains unchanged. Confirm publication from the `team` remote branch, not the original checkout's branch state.
- Existing tracked duplicate-style names were observed, including `Assets/free horror ambience 2.meta` and sample sprite names ending ` 4.png`. None were added or changed by B; the lead should inspect them under the shared duplicate-file rule.

## Commands to run outside the restricted sandbox

Run from this worktree after scene integration. The builder uses the currently enabled scene list without reauthoring or saving other sessions' scenes.

```sh
UNITY_BIN=/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity
mkdir -p artifacts
"$UNITY_BIN" -batchmode -quit -nographics -projectPath . -logFile artifacts/compile.log
"$UNITY_BIN" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -assemblyNames LK.Simulation.Tests -testResults artifacts/simulation-editmode.xml -logFile artifacts/tests.log
"$UNITY_BIN" -batchmode -quit -nographics -projectPath . -buildTarget Android -executeMethod LighthouseKeepers.Simulation.Editor.SimulationBuild.BuildAndroid -logFile artifacts/build.log
adb install -r artifacts/builds/LighthouseKeepers-Simulation.apk
```

The expected APK path is `artifacts/builds/LighthouseKeepers-Simulation.apk`; **that file does not currently exist**. Building the base scenes before integration merely packages source and does not demonstrate this simulation.

Reproduce managed verification on this machine:

```sh
python3 Assets/Tests/EditMode/Simulation/run_standalone.py --unity /Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents --assembly-cache '/Users/aneeshvasamreddy/Desktop/Virtual Reality Design/Lighthouse Keepers/Library/ScriptAssemblies'
```

## Numbered headset acceptance checklist — not yet executed

1. In an integrated development APK, stand two metres from the panel. Read height, volume, flow, net, ETA, power, leak and phase; operate all buttons with the existing XR ray while standing, with no rig/camera motion.
2. Reset; leave the active leak unpumped and generator off. Observe continuous visible rise throughout, without a plateau. Water crosses the 0.90 m pipe centreline; `Running` becomes `Failed` after three sustained seconds above it, about 4:02 after reset.
3. Reset and pump completed strokes at or faster than the allowed cadence for two minutes. The panel confirms accepted-stroke removal; water still rises, at less than 20% of the unpumped rate. Rapid duplicate strokes/alternating actors must not bypass the cooldown.
4. From a running state near the pipe, repair the leak, start a dry/fuelled generator and activate the electric pump. Spray stops; water visibly falls to 0.15 m or lower within 90 seconds (core prediction: 69 seconds from 0.90 m). The phase becomes `Succeeded` only once the height is safe.
5. With the pump draining, press generator fault. Power drops to zero and the pump stops at the next tick. Restart the generator, then explicitly restart the electric pump; drainage resumes. Raise water to C's measured plinth top using the panel; starting the generator is rejected with a readable flooding reason.
6. Reset and set water just below the failure line. Let it cross, then lower it before three sustained seconds. Confirm the timer resets; the next crossing receives a fresh full three seconds. After failure, ResetRun restores dry water, active leak, generator/pump off and full fuel.

Integration requests: `Requests/B-to-A.md`, `Requests/B-to-C.md`, `Requests/B-to-D.md`, `Requests/B-to-E.md`. No scene, rig, legacy runtime script, package or other session's asset was edited.
