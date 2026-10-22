# Simulation API — seven-watch, three-coupling contract

Status: **[PROPOSED] for ship acceptance**. Managed compilation/tests are separate evidence. No current-source APK, genuine ship Play Mode screenshot or headset execution exists in this checkout. R8 remains unmet. This document supersedes earlier watch-index leak activation and automatic watch-success behavior.

## Assemblies and ownership

- `LK.Core.Simulation` / `LighthouseKeepers.Core.Simulation`: pure C# commands, immutable snapshots, `SimulationHost`, `FloodParameters`, `DayTuning`, authority and geometry.
- `LK.Simulation.Unity` / `LighthouseKeepers.Simulation.Unity`: `SimulationHostBehaviour`, `FloodTuning`, `DayTuningAsset`, optional pump-event bridge.
- `LK.Simulation.Editor`: manifest-based build helper and ship acceptance gate.
- `LK.Simulation.Tests`: EditMode NUnit tests and real Lobby → Play Solo reachability test.

E owns networking in **LK.Net**. Reference `LK.Core.Simulation` before enabling E's `LK_SPRINT3_SIMULATION`. Every facade, fake and backend implementing this interface must implement BOTH shorthand methods:

```csharp
public interface ISimulationHost
{
    SimSnapshot Current { get; }
    void Enqueue(SimCommand cmd);
    event System.Action<SimSnapshot> OnTick;
    void ActivateDayLeak(int leakId);
    void ForceGeneratorFault();
}
```

Both methods ONLY enqueue commands, with the same authorization and next-tick semantics as Enqueue:

```csharp
public void ActivateDayLeak(int leakId) =>
    Enqueue(new SimCommand(SimCommandKind.ActivateDayLeak, targetId: leakId));
public void ForceGeneratorFault() =>
    Enqueue(new SimCommand(SimCommandKind.ForceGeneratorFault));
```

B's pure host, Unity driver and test fake implement both. E must add these methods to `FusionSimulationAdapter` in its own lane; exact patch instructions are in `Requests/B-to-E.md`. `LocalAuthority : IAuthority` is an authority policy, **not** an ISimulationHost implementation. Its `IsAuthoritative` property remains true. E injects its replacement policy; no network namespace enters the core.

## State and execution

```csharp
public interface IAuthority { bool IsAuthoritative { get; } }
public SimulationHost(FloodParameters tuning, ISimulationGeometry geometry,
    IAuthority authority, double initialVolumeM3 = 0,
    System.Func<int, bool> canRepairPipe = null);
public const double TickSeconds = 0.1;
public void Advance(double elapsedSeconds);
```

One authoritative thread drives the accumulator. FIFO commands apply at the next 10 Hz tick, followed by volume integration and OnTick. Enqueue does not immediately modify Current. A nonauthoritative host discards commands and elapsed time; E owns client snapshot replication. Never advance one host from two drivers. Inputs/durations must be finite; negative duration/initial volume are invalid.

Volume is double; world water Y = measured F1 floor + volume / floor area. Only volume's zero floor is applied; no easing or height ceiling. Depth at/above failHeightM for failSustainSeconds fails the run; falling below resets the sustain clock. Failed freezes water/fuel and requires ResetRun. Pausing freezes integration but continues tick publication.

ResetRun returns a dry, active-one-leak arithmetic fixture at DayIndex=0. The ship must BeginDay(1) before readiness. BeginDay(1..7) starts healthy with power/pump enabled, clears all coupling history and the generator fault, and selects that day's count/multiplier. It preserves volume, remaining fuel, pause and any development base-rate override. It rejects unavailable, flooded or fuel-exhausted geometry.

**D owns completion of every begun watch.** B leaves Phase=Running after repairs, safe water, restored power and expiration of the configured day duration. D evaluates its board, clock, cooperation and bed requirements and emits its own progression events. A repair cannot complete a watch. Passive pumping continues below safe height all the way to zero. Only the bare DayIndex=0 arithmetic fixture retains automatic Phase=Succeeded after repair/safe water; it is not a ship progression path.

## Commands

```csharp
public readonly struct SimCommand
{
    public readonly SimCommandKind Kind;
    public readonly int ActorId, TargetId;
    public readonly float Value;
    public SimCommand(SimCommandKind kind, int actorId = 0,
        int targetId = 0, float value = 0);
}
```

| ID | Kind | Target/value and effect | Sender |
| ---: | --- | --- | --- |
| 0 | RepairLeak | **TargetId=coupling ID 0..2**, within current configured count and currently active. Seal only that coupling. D's injected CanRepairPipe(actorId) must authorize it; restore powered automatic pumping | Validated physical interaction |
| 1 | ManualPumpStroke | Remove configured m³ immediately at command processing, limited by one global cooldown; no banked dry strokes | Validated physical interaction |
| 2 | StartGenerator | Require fuel and measured dry plinth. In a begun watch, clear fault seep and restore electric pumping; leave every pipe leak unchanged | D validates prime/choke/fuel first |
| 3 | StopGenerator | Stop generator and electric pump | Validated interaction |
| 4 | SetElectricPump | Value=0/1; starting requires nonzero power ≥ tuning threshold | Validated interaction |
| 5 | BeginDay | **TargetId=day index 1..7**; healthy watch setup described above | Host only |
| 6 | ResetRun | Reset volume, tank, leak fixture, coupling history, pause, override and stroke cooldown | Host only |
| 7 | ForceGeneratorFault | Stop generator/pump; begun watches enable fault seep. D chooses when | Host only |
| 8 | PauseSimulation | Freeze integration without changing Phase | Host only |
| 9 | ResumeSimulation | Clear pause; never revive a terminal phase | Host only |
| 10 | ActivateDayLeak | **TargetId=coupling ID 0..2, NOT day index**; require begun watch and ID < configured leak count. Activate that coupling and trip electric pump. Retried activation does nothing, including after that coupling was repaired, until BeginDay clears history | Host only |
| 100 | DebugSetLeak | Editor/development only; Value=0/1 sets all configured coupling bits off/on | Host only |
| 101 | DebugSetWaterHeight | Editor/development only; Value=world water Y; derive volume | Host only |
| 102 | DebugSetWaterVolumeM3 | Editor/development only; paused Running; finite nonnegative m³ | Host only |
| 103 | DebugSetLeakInflowM3PerSec | Editor/development only; paused Running; finite nonnegative **base per-coupling** m³/s, multiplied by the day's multiplier | Host only |

Original IDs are stable. Gameplay/fault commands require unpaused Running; BeginDay accepts Running/Succeeded; ResetRun and pause controls accept all phases. Every Value must be finite even if unused. Unknown enum values reject. `SimCommandPolicy.IsHostOnly` allows interaction IDs 0..4 only; everything else/unknown is host-only. E authenticates ActorId from transport and enforces station identity, proximity and target ID; backend authority alone does not validate a remote repair target. D's actor-only repair guard is an additional check, not a target-identity validator.

### Coupling integration and repair input

Each active coupling adds `leakInflowM3PerSec * DayTuning.InflowMultiplier`. Three couplings contribute three equal shares; sealing one removes exactly one share. Manual throughput remains 85–95% of **one** coupling on all seven profiles. It cannot offset even one active leak.

Generator-fault seep is residual flow in the same circuit: use total coupling flow while any coupling is open; otherwise use generatorFaultSeepM3PerSec while that fault remains. Restart clears the fault seep only. It does not repair couplings or create a fourth coupling.

**B no longer subscribes to OnLeakRepairCompleted.** That shared event contains only actorId, so using it to repair a specific one of three physical couplings would guess a target. A/D must submit exactly one `Enqueue(new SimCommand(RepairLeak, actorId, leakId))` after validating the actual coupling. D may still observe the event for progression, but completion requires its full objective/clock logic. The optional B bridge forwards only OnPumpStrokeCompleted; raw generator events also remain D's responsibility. Subscribe once per command input endpoint and avoid forwarding both locally and through the network.

## Snapshot

All following members are public readonly fields, except the two computed count properties. Constructor's original first ten arguments are preserved. E must version/extend its original ten-field packet; constructor defaults do not replicate missing fields.

```csharp
public SimSnapshot(int tick, double waterVolumeM3, float waterHeightM,
    float inflowM3PerSec, float outflowM3PerSec, bool leakActive,
    bool generatorRunning, float powerAvailable, bool electricPumpRunning,
    RunPhase phase, string lastRejectReason = "", bool generatorFlooded = false,
    bool generatorGeometryAvailable = true, float fuelRemainingSeconds = 0,
    float secondsAboveFail = 0, float timeToFailSeconds = float.PositiveInfinity,
    bool paused = false, int dayIndex = 0, bool generatorFaultActive = false,
    int activeLeakMask = 0, int activatedLeakMask = 0, int configuredLeakCount = 0);
```

| Member | Type | Meaning |
| --- | --- | --- |
| Tick | int | Monotonic 10 Hz publication index, including resets/pause |
| WaterVolumeM3 | double | Nonnegative authoritative volume |
| WaterHeightM | float | Derived world Y, not depth |
| InflowM3PerSec | float | Actual total inflow, zero paused/terminal |
| OutflowM3PerSec | float | Actual tick removal / .1 s, including discrete manual strokes |
| LeakActive | bool | Any coupling OR generator fault remains active |
| GeneratorRunning | bool | Actual generator running state |
| PowerAvailable | float | Binary 0/1 from running, valid geometry and dry plinth |
| ElectricPumpRunning | bool | Enabled pump; actual flow also requires unpaused integration |
| Phase | RunPhase | Running=0, Failed=1, Succeeded=2; D owns ship watch completion |
| LastRejectReason | string | Latest rejected command; cleared by next accepted command |
| GeneratorFlooded | bool | World water Y ≥ measured plinth top |
| GeneratorGeometryAvailable | bool | Valid exact scene measurement exists |
| FuelRemainingSeconds | float | Remaining runtime in seconds |
| SecondsAboveFail | float | Sustained failure-clock seconds |
| TimeToFailSeconds | float | Estimate at current power with no future manual strokes; positive infinity is valid |
| Paused | bool | Integration suspended independently of Phase |
| DayIndex | int | 0 arithmetic fixture; 1..7 selected watch |
| GeneratorFaultActive | bool | Fault seep still needs generator recovery |
| **BeaconLit** | **bool** | **GeneratorRunning && PowerAvailable > 0.5f**, derived in constructor; C uses for amber lantern |
| **ActiveLeakMask** | **int** | Bits 0..2 identify currently open couplings |
| **ActivatedLeakMask** | **int** | Bits activated at least once this watch; prevents retries reopening a repaired pipe |
| **ConfiguredLeakCount** | **int** | This day's allowed coupling count |
| ActiveLeakCount | int property | Count of open bits |
| RepairedLeakCount | int property | Count of activated but currently sealed bits |

Replicate masks/count and all needed diagnostics, not just LeakActive. Derived BeaconLit/count properties reconstruct from constructor inputs. E validates masks 0..7, configured count 0..3, active subset of activated and bits within configured count. A client's count is not inferred from its local day asset.

## Seven-day tuning — D reads this surface

`Assets/Data/Tuning/FloodTuning.asset` references seven `DayTuningAsset`s, `Day01.asset` through `Day07.asset`, in its public `days` array. Each is under B's ownership and has public fields:

```csharp
int dayIndex;           // 1..7
int leakCount;          // 0..3
float inflowMultiplier; // per coupling
bool generatorFault;   // D's schedule should include a fault; B never fires it itself
float dayLengthSeconds;// D owns the clock
DayTuning ToParameters();
```

`FloodTuning.GetDay(int dayIndex)` returns immutable pure `DayTuning` with `DayIndex`, `LeakCount`, `InflowMultiplier`, `GeneratorFault`, `DayLengthSeconds` properties. `FloodTuning.ToParameters()` validates/copies exactly seven ordered profiles. The pure `FloodParameters` constructor accepts the original eleven doubles followed by optional `IReadOnlyList<DayTuning> days = null`; it defensively copies and exposes `DayCount` and `GetDay(int)`. An omitted catalog permits DayIndex=0 arithmetic only; BeginDay rejects missing seven-day tuning. Null/default/out-of-order/invalid profiles reject. Numbers and provisional pacing are in `Tuning.md`.

D schedules IDs **0..LeakCount-1**, separately from the day index. D reads the same profile to build its board/clock and generator-fault schedule. The core selects count/multiplier at BeginDay; it does not schedule activation or auto-complete on elapsed DayLengthSeconds. D owns any per-day tasks, cooperation policy and progression.

## Scene composition and geometry

`ISimulationGeometry` exposes `double FloorWorldY` and `bool TryGetGeneratorPlinthTopWorldY(out double)`. `FixedSimulationGeometry(double floorWorldY, double? generatorPlinthTopWorldY)` validates finite measured heights; null disables power. Tuning fail/safe heights are depths above F1. C published F1=0, generator `STN_GENERATOR/PLINTH_TOP`=3.750 m, not generator root=3.300 m.

`SimulationHostBehaviour.ConfigureScene(FloodTuning, Transform floor, Transform plinthTop, IAuthority, Func<int,bool> canRepairPipe)` must run while inactive before Start. ConfigureAuthority must precede Start. ConfigureRepairValidation may refresh the callback. `IsInitialized` gates Current/Enqueue (both throw before Start). The driver alone advances using Unity elapsed time; core never references Unity. E enables only the host backend and supplies the facade on clients. Solo supplies one keeper; D handles cooperative prerequisites. B supplies no fake second player. Missing/false/throwing D validation rejects repair before mutation.

No runtime B debug canvas is instantiated in ship players. Editor debug panel requires explicit opt-in. B's link.xml preserves contracts, driver and both tuning types for IL2CPP; each owner preserves its own reflected types.

## Acceptance and reproduction

`NoNetworkingLeakageTest.OnlyLKNetMayReferenceNetworkingNamespaces` scans loaded project runtime assemblies using reflection, including signatures, generic types/constraints, members, locals/catches and resolved IL references. Only exact **LK.Net** may reference networking namespaces. Legacy LK.Networking/LK.StormWatch.Networking are not exempt. Fixture tests cover generic signatures, method bodies, namespace declarations and the exact exemption. B's standalone run loads B assemblies only; a full merged Editor domain must also pass.

The sole scene source remains C's `Assets/Scenes/Sprint3/ShipScenes.json`. B does not write C's manifest, scene files or Build Settings. Manifest/build gate requires lobby index 0, Shell House index 1, retained NetworkLobby and all Shell/Dress House/F1..F4/Exterior pairs, with no legacy world. Real `Round2ShipReachabilityTests.LobbyPlaySoloReachesShipAndCapturesPipe` invokes the actual lobby button, checks active A/D/B bindings and captures `STN_PIPE_LEAK` from an independent observer camera. No rig is moved. It saves `artifacts/round2/B/STN_PIPE_LEAK.png` and source-fingerprinted `reachability.json`; missing/stale proof blocks player builds. No substitute image is evidence.

```sh
UNITY_BIN=/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity
mkdir -p artifacts/round2/B
"$UNITY_BIN" -batchmode -quit -nographics -projectPath . -logFile artifacts/compile.log
"$UNITY_BIN" -batchmode -projectPath . -buildTarget Android -runTests -testPlatform EditMode -testFilter LighthouseKeepers.Tests.Simulation.Round2ShipReachabilityTests.LobbyPlaySoloReachesShipAndCapturesPipe -testResults artifacts/round2/B/ship-playmode.xml -logFile artifacts/round2/B/ship-playmode.log
"$UNITY_BIN" -batchmode -quit -nographics -projectPath . -buildTarget Android -executeMethod LighthouseKeepers.Simulation.Editor.SimulationBuild.BuildAndroid -logFile artifacts/round2/B/build.log
adb install -r artifacts/round2/B/LighthouseKeepers-Ship.apk
```

Run Play Mode with graphics available and Android selected. Native startup is currently blocked by Package Manager socket permissions; the canonical merged ship set is also absent in this checkout. APK and screenshot paths above are expected outputs, not produced artifacts. The in-headset checklist and exact managed test command are in `Tuning.md`.
