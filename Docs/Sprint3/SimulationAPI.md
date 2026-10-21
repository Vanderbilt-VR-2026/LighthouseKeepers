# Simulation API — October 21, 2026

Status: **[BUILT-UNTESTED] for managed assemblies only**. The command and reflection tests run outside Unity; Android, scene integration and headset acceptance remain unverified. This is the binding contract for B, C, D and E. It answers the updated `E-to-B.md` in E's networking worktree; that updated request was not yet present on E's fetched remote branch.

## Assemblies and namespaces

| Assembly | Namespace | Purpose |
| --- | --- | --- |
| `LK.Core.Simulation` | `LighthouseKeepers.Core.Simulation` | Pure C# state, commands, parameters, authority and geometry contracts; no Unity/networking references |
| `LK.Simulation.Unity` | `LighthouseKeepers.Simulation.Unity` | Tuning ScriptableObject, scene driver, optional interaction event bridge and development panel |
| `LK.Simulation.Tests` | `LighthouseKeepers.Tests.Simulation` | Editor NUnit tests, including `NoNetworkingLeakageTest` |
| `LK.Simulation.Editor` | `LighthouseKeepers.Simulation.Editor` | Existing-scene Android builder |

E owns all networking implementations in `LK.Net`. No networking package or implementation is introduced by B. Reference `LK.Core.Simulation` from E's asmdef before enabling E's `LK_SPRINT3_SIMULATION` symbol. That symbol is not needed by B's assemblies. E's guarded adapter must implement **all** interface members, including `ForceGeneratorFault()`.

## Authority and host lifecycle

```csharp
public interface ISimulationHost
{
    SimSnapshot Current { get; }
    void Enqueue(SimCommand cmd);
    event System.Action<SimSnapshot> OnTick;
    void ForceGeneratorFault();
}

public interface IAuthority { bool IsAuthoritative { get; } }
public sealed class LocalAuthority : IAuthority
{
    public bool IsAuthoritative => true;
}
```

`ForceGeneratorFault()` is shorthand for enqueuing `ForceGeneratorFault`, not an out-of-band write. A failed or paused run rejects that gameplay fault command until its state permits it.

`SimulationHost` has this constructor and additional public driver method:

```csharp
public SimulationHost(FloodParameters tuning, ISimulationGeometry geometry,
    IAuthority authority, double initialVolumeM3 = 0);
public const double TickSeconds = 0.1;
public void Advance(double elapsedSeconds);
```

`initialVolumeM3` must be finite and nonnegative. `Advance` rejects nonfinite/negative durations. Call it on one authoritative thread; the command queue is single-threaded. Every full 0.1 accumulated seconds processes FIFO commands, integrates if Running/unpaused, then publishes an immutable snapshot and `OnTick`. Enqueue alone does not publish or immediately update `Current`; command latency is at most one simulation tick while the driver advances. Never drive the same host from both E and the Unity component.

An unauthoritative host discards `Enqueue` input and does not advance; it is **not a client replica**. E's facade receives authenticated snapshots and forwards commands to the authoritative backend. The server derives `ActorId` from the transport sender, not client input. `IAuthority` only establishes who advances the host; it does not authenticate each actor or validate physical interactions.

Initial/reset state: volume 0, active leak, generator OFF, pump OFF, full configured fuel, `Running`, unpaused. Snapshot tick is monotonic across ResetRun. `Succeeded`/`Failed` freeze integration but still publish ticks. BeginDay resumes a succeeded day; a failed run requires ResetRun.

## Commands and network authorization

```csharp
public readonly struct SimCommand
{
    public readonly SimCommandKind Kind;
    public readonly int ActorId;
    public readonly int TargetId;
    public readonly float Value;
    public SimCommand(SimCommandKind kind, int actorId = 0,
        int targetId = 0, float value = 0);
}
```

The original IDs 0–7 and development IDs 100–101 are preserved explicitly. Unused Value defaults to zero and must still be finite. Use TargetId zero unless a command uses it. ActorId carries provenance; B does not infer privileges from its numeric value.

| ID | `SimCommandKind` | Value / target / effect | Valid state and sender |
| ---: | --- | --- | --- |
| 0 | `RepairLeak` | Stop the single active leak | Running, unpaused; host-validated repair completion |
| 1 | `ManualPumpStroke` | Remove configured m³/stroke, flooring volume at zero; Value is ignored | Running, unpaused; host-validated down-stroke; one global pump cooldown across actors |
| 2 | `StartGenerator` | Start only with measured dry plinth and fuel remaining | Running, unpaused; host-validated interaction |
| 3 | `StopGenerator` | Stop generator and electric pump | Running, unpaused; host-validated interaction |
| 4 | `SetElectricPump` | Value **0 or 1**; starting requires nonzero power at/above the configured threshold | Running, unpaused; host-validated interaction |
| 5 | `BeginDay` | TargetId **nonnegative day index**; reopen leak, clear failure timer, resume Running; preserve volume, fuel, power/pump, pause and any development rate override | Running or Succeeded; **host only**, scheduled by D |
| 6 | `ResetRun` | Restore initial dry state and published tuning rate; clear pause, rate override and cooldown | Any phase/pause; **host only** |
| 7 | `ForceGeneratorFault` | Stop generator and electric pump; D chooses timing | Running, unpaused; **host only** |
| 8 | **`PauseSimulation`** | Pause integration; no Value argument needed | Any phase; **host only**; does not change Phase |
| 9 | **`ResumeSimulation`** | Resume integration only if Phase is Running | Any phase; **host only**; never revives Failed/Succeeded |
| 100 | `DebugSetLeak` | Value **0 or 1**; diagnostic leak toggle | Any phase/pause; **local host development tooling only** |
| 101 | `DebugSetWaterHeight` | Existing diagnostic command: finite world Y, converted to volume using injected origin/area, flooring below-floor input at dry; reset failure timer | Any phase/pause; **local host development tooling only**; does not revive a terminal run |
| 102 | **`DebugSetWaterVolumeM3`** | Finite Value **≥0 m³**; no ceiling; replace volume, clear failure timer and refresh flooding/power | **Paused Running only**; **local host development initialization only** |
| 103 | **`DebugSetLeakInflowM3PerSec`** | Finite Value **≥0 m³/s**; no ceiling; override this run's active-leak rate without editing the asset | **Paused Running only**; **local host development initialization only** |

IDs 100–103 exist only under `#if DEVELOPMENT_BUILD || UNITY_EDITOR`; they are absent from a release build's enum and handlers. Undefined IDs are rejected by the host. The new initialization commands reject negative input instead of silently coercing it. Zero rate preserves `LeakActive` and cannot by itself satisfy the repaired-leak success condition. BeginDay preserves a diagnostic rate override; ResetRun restores the copied asset value. Absolute world-Y remains derived from volume, including when the older height diagnostic is used. Do not translate received snapshots into any initialization command.

For E's old controls: `Paused=true/false` maps to PauseSimulation/ResumeSimulation. There is no height-per-second `RiseSpeed` state: during development initialization, convert a desired rise speed to m³/s using **speed × the same FloorAreaM2** and enqueue DebugSetLeakInflowM3PerSec. Initialize a desired water height using **max(0, worldY − F1OriginY) × FloorAreaM2** with DebugSetWaterVolumeM3. Host gameplay never uses development initialization to simulate leaks, pumping or replication.

Use the shared policy helpers in E rather than maintaining another list:

```csharp
public static class SimCommandPolicy
{
    public static bool IsHostOnly(SimCommandKind kind);
    public static bool IsDevelopmentOnly(SimCommandKind kind);
}
```

IsHostOnly is false only for IDs 0–4 and true for everything else, including unknown IDs. IsDevelopmentOnly identifies the four debug commands in an Editor/development build and returns false in a release build, where those commands are undefined. E must first reject undefined IDs, then reject every host-only command from a remote client. Debug initialization is restricted to local development/admin entry points, even on a development host. IDs 0–4 still require E's authenticated sender, station lease/proximity/completion validation and B's own invariants; missing interaction validation must reject the command.

Pause continues 10 Hz ticks/events, freezes water, fuel, failure sustain and pump cooldown, and rejects gameplay commands. Generator/pump enabled state and binary power remain visible while actual paused inflow/outflow are zero. BeginDay may be queued while paused; explicit ResumeSimulation is still required. Pause and Resume are idempotent. FIFO ordering applies within a batch: enqueue Pause before diagnostic initialization; enqueue Resume afterward only when ready to integrate again.

## Snapshots and rejection feedback

`RunPhase` IDs: Running=0, Failed=1, Succeeded=2. All snapshot fields are public readonly:

| Field | Type | Meaning |
| --- | --- | --- |
| `Tick` | int | Monotonic publication tick |
| `WaterVolumeM3` | double | Authoritative volume, no upper ceiling |
| `WaterHeightM` | float | **World Y** = injected F1 origin + volume / area |
| `InflowM3PerSec` | float | Current actual leak inflow; zero paused/terminal/sealed |
| `OutflowM3PerSec` | float | Actual removal during this tick / 0.1 s, including accepted manual impulses |
| `LeakActive` | bool | Leak needs repair, independently of a diagnostic rate of zero |
| `GeneratorRunning` | bool | Generator enabled and capable of running |
| `PowerAvailable` | float | Binary 0 or 1; no partial-power state this sprint |
| `ElectricPumpRunning` | bool | Pump enabled; pause suspends its actual flow |
| `Phase` | RunPhase | Running / Failed / Succeeded |
| `LastRejectReason` | string | Latest rejected-command explanation, empty after a subsequent accepted command; not a per-command acknowledgement queue |
| `GeneratorFlooded` | bool | World water Y reaches C's plinth top |
| `GeneratorGeometryAvailable` | bool | Exact valid plinth measurement exists |
| `FuelRemainingSeconds` | float | Remaining generator runtime seconds |
| `SecondsAboveFail` | float | Sustained time at/above the failure depth, frozen by pause |
| `TimeToFailSeconds` | float | ETA assuming no new manual strokes and persistent current power; positive infinity when not projected to fail/paused, zero after failure |
| **`Paused`** | bool | Integration paused independently of Phase |

Public constructor, preserving E's first ten positional arguments:

```csharp
public SimSnapshot(int tick, double waterVolumeM3, float waterHeightM,
    float inflowM3PerSec, float outflowM3PerSec, bool leakActive,
    bool generatorRunning, float powerAvailable, bool electricPumpRunning,
    RunPhase phase, string lastRejectReason = "", bool generatorFlooded = false,
    bool generatorGeometryAvailable = true, float fuelRemainingSeconds = 0,
    float secondsAboveFail = 0, float timeToFailSeconds = float.PositiveInfinity,
    bool paused = false);
```

This is C# source compatibility, **not a network packet specification**. E's old ten-field/40-byte snapshot packet omits diagnostics and Paused. E must version and extend its wire format, validate the added fields (ETA explicitly allows positive infinity), and replicate all fields needed by consumers. Defaults in the short constructor do not establish actual fuel, plinth geometry or rejection state on a client. Only E serializes packets; C/D read the facade's snapshots.

Readable rejections include missing/flooded generator geometry, exhausted fuel, pump without power, invalid binary/finite input, too-fast stroke, paused gameplay, terminal-phase input, and initialization outside paused Running. The generator and electric pump stop when power is lost; restoring the generator does not automatically restart the electric pump.

## Geometry, parameters and Unity composition

```csharp
public interface ISimulationGeometry
{
    double FloorWorldY { get; }
    bool TryGetGeneratorPlinthTopWorldY(out double height);
}
public FixedSimulationGeometry(double floorWorldY,
    double? generatorPlinthTopWorldY);

public FloodParameters(double leakInflowM3PerSec, double manualPumpStrokeM3,
    double manualPumpMinStrokeSeconds, double electricPumpM3PerSec,
    double floorAreaM2, double failHeightM, double safeHeightM,
    double failSustainSeconds, double electricPumpStartPower,
    double initialFuelSeconds);
```

Fixed geometry requires a finite floor and a finite plinth above the floor, or null for unavailable plinth data. Never substitute the STN_GENERATOR root for a plinth-top measurement. Unknown/flooded geometry prevents generator starts. FloodParameters exposes immutable double properties matching those names in PascalCase. Base rates, area, failure height and durations must be positive/finite; safe height must be nonnegative and below fail height, threshold within 0..1, fuel nonnegative/finite. Threshold heights are **depths above F1 origin**, not absolute world Y. See `Tuning.md` for asset numbers and balance evidence.

`FloodTuning.ToParameters()` copies `Assets/Data/Tuning/FloodTuning.asset` once at driver startup. There is no runtime asset mutation or automatic live reload. `SimulationHostBehaviour` implements ISimulationHost and ISimulationGeometry. C assigns its serialized `tuning`, `floorOrigin` and `generatorPlinthTop` references. `useLocalAuthority` is only for an explicit offline rehearsal and defaults false.

E starts the backend component disabled, calls `ConfigureAuthority(IAuthority)` **before Start**, and enables it only after host authority and scene readiness are established. E implements the network authority in LK.Net; B supplies LocalAuthority for offline use. A disabled client component must not construct/advance a SimulationHost. On the host, Unity Start constructs it and Update calls Advance(Time.deltaTime); externally driven pure hosts do not use that MonoBehaviour. Use `IsInitialized` before accessing the driver's Current or enqueuing; those operations throw before initialization. ConfigureAuthority throws after initialization.

Other public driver members: `FloorWorldY`, `TryGetGeneratorPlinthTopWorldY(out double)`, `RebindInteractionEvents()` and the ISimulationHost members. The optional InteractionEventBridge subscribes only to A's exact three static Action<int> events; its public surface is `InteractionEventBridge(ISimulationHost)`, `Bind(Type)`, `BindAvailableAssembly()` and `Dispose()`. Bind a single bridge to the intended facade/authority endpoint, preserve A's event accessors for IL2CPP, and avoid duplicate subscriptions through both the facade and backend. Rebind unsubscribes existing listeners first.

C's water view moves only a dedicated presentation mesh using snapshot height. D schedules BeginDay/ResetRun/faults and waits for **repaired leak AND depth ≤ safeHeightM** for success. Failure requires depth ≥ failHeightM for the full configured three seconds; only the above-threshold fraction of a crossing tick counts. No consumer writes a legacy flood controller or copies snapshot height back into Enqueue.

## Reflection tripwire and verification

Source: `Assets/Tests/EditMode/Simulation/NoNetworkingLeakageTest.cs`, assembly `LK.Simulation.Tests`. `OnlyLKNetMayReferenceNetworkingNamespaces` enumerates **AppDomain.CurrentDomain.GetAssemblies()** and inspects loaded project runtime assemblies: Assembly-CSharp, LK.* and LighthouseKeepers*, excluding Editor/test tooling. Vendor SDK/framework assemblies are dependencies, not game layers. There is exactly one permitted game assembly name: **LK.Net**. LK.Networking and LK.StormWatch.Networking are not exempt merely because their source was moved under the LK.Net directory. E must consolidate them or coordinate reference migration; B does not silently weaken the test.

The scanner checks reflected assembly dependencies, declarations, bases/interfaces, generic constraints/arguments, members, method locals/catches and resolved IL member/type tokens. It detects Fusion, Photon, Unity.Netcode, Unity.Networking, System.Net, LighthouseKeepers.Net and legacy networking namespaces, plus dependencies on networking game assemblies. Unreadable reflection metadata fails the test. Four emitted-fixture tests prove forbidden generic fields, method-body-only calls, networking declarations and the exact LK.Net exception. No Mono.Cecil or Unity compilation-pipeline dependency remains.

**35 managed NUnit methods passed, zero failed/skipped** on this checkout. The loaded-assembly scan in that run included LK.Core.Simulation and LK.Simulation.Unity. This does not prove E's not-yet-merged networking assemblies or the entire Unity Editor domain pass. The original four balance tests remain green: idle failure 242.0 s, electric recovery 69.0 s, manual throughput 92.59% of inflow. Core and adapter also compile with release defines.

```sh
UNITY_BIN=/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity
"$UNITY_BIN" -batchmode -quit -nographics -projectPath . -logFile artifacts/compile.log
"$UNITY_BIN" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -assemblyNames LK.Simulation.Tests -testResults artifacts/simulation-editmode.xml -logFile artifacts/tests.log
"$UNITY_BIN" -batchmode -quit -nographics -projectPath . -buildTarget Android -executeMethod LighthouseKeepers.Simulation.Editor.SimulationBuild.BuildAndroid -logFile artifacts/build.log
adb install -r artifacts/builds/LighthouseKeepers-Simulation.apk
```

Unity startup attempts are blocked by Package Manager socket permission errors in this sandbox, so no new APK or headset evidence exists. Reproduce the managed run with `Assets/Tests/EditMode/Simulation/run_standalone.py` as documented in Tuning.md. The previous headset checklist still applies after scene integration. Additional unexecuted checks:

1. As host, pause while flooding/powered: both peers display Paused and unchanged water/fuel while 10 Hz snapshots continue. Resume restores normal flow. A client pause or initialization request must be rejected by E.
2. In a development host's paused Running state, initialize volume and leak rate. Both views reflect derived world height after replication; Resume uses the requested rate. Reset restores published tuning. Never enqueue initialization when receiving a snapshot.
3. Leave the leak unpumped: visible rise has no plateau and fails three seconds after crossing the pipe; manual strokes slow it without stopping it; after repair the electric pump drains to safe height within 90 seconds.
