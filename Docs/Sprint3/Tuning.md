# Session B tuning — seven watches

STATUS: **[PROPOSED]** for ship/headset acceptance. These are deterministic starting numbers, not headset-tuned values. **59 managed NUnit methods pass; one actual ship Play Mode test remains unrun.** Native Unity startup is denied Package Manager/licensing IPC access. No new APK or genuine screenshot exists. Current API: [SimulationAPI.md](SimulationAPI.md).

## FloodTuning.asset

| Field | Value | Units / purpose |
| --- | ---: | --- |
| leakInflowM3PerSec | .180 | Base m³/s **per active coupling** |
| manualPumpStrokeM3 | .200 | m³ per accepted down-stroke |
| manualPumpMinStrokeSeconds | 1.20 | Seconds, global across actors/pumps |
| electricPumpM3PerSec | .520 | m³/s at full power |
| floorAreaM2 | 47.8 | m² |
| failHeightM | .90 | Depth above actual F1 floor |
| safeHeightM | .15 | Depth above actual F1 floor |
| failSustainSeconds | 3 | Continuous seconds ≥ failure depth |
| electricPumpStartPower | .35 | Start threshold |
| initialFuelSeconds | **2100** | Generator runtime; carries across watches |
| generatorFaultSeepM3PerSec | .176 | Residual circuit inflow when faulted and no coupling remains open |

The supplied .200/.55 stroke combination would remove 202% of one leak's inflow. The proposed 1.20 s cooldown yields .16667 m³/s: 92.59% of a baseline coupling and 94.70% of the fault seep. Electric pumping is 3.12× manual throughput. Only volume floors at zero; height has no ceiling/easing/asymptote.

## Day tuning — D reads these exact assets

`FloodTuning.days` references `Assets/Data/Tuning/Day01.asset` through `Day07.asset` in order. All watch durations are provisional **180 seconds**; D owns the clock, incident timing, board tasks and completion. The generatorFault flag asks D to schedule a fault; the simulation never fires it automatically.

| Day | leakCount | inflowMultiplier | generatorFault | dayLengthSeconds | One leak m³/s | Manual / one leak |
| ---: | ---: | ---: | --- | ---: | ---: | ---: |
| 1 | 1 | 1.00 | false | 180 | .1800 | 92.59% |
| 2 | 1 | 1.00 | true | 180 | .1800 | 92.59% |
| 3 | 2 | 1.01 | true | 180 | .1818 | 91.68% |
| 4 | 2 | 1.02 | true | 180 | .1836 | 90.78% |
| 5 | 3 | 1.04 | true | 180 | .1872 | 89.03% |
| 6 | 3 | 1.06 | true | 180 | .1908 | 87.35% |
| 7 | 3 | 1.08 | true | 180 | .1944 | 85.73% |

Leak IDs are **0, 1, 2**, independent of day indices **1..7**. D schedules IDs 0..leakCount-1; RepairLeak.TargetId selects exactly one active coupling. The first repair removes one share, never every leak. Repeated activation cannot reopen that repaired coupling within the same watch. BeginDay clears history. No actor-only repair event mutates B state or completes D's day.

Three open Day 7 couplings total .5832 m³/s, greater than electric pumping alone. Sealing one lowers inflow to .3888 m³/s, allowing powered recovery; sealing every coupling leaves zero pipe inflow. Generator recovery and pipe repairs remain independent. When no coupling is open but the generator fault remains, the same circuit uses its .176 m³/s seep; rates are not added as a fourth pipe.

The tank was raised from 900 to 2100 runtime seconds for seven watches. Seven planned 180 s watches plus seven worst-case safe-height recovery intervals total approximately 1743 seconds, leaving roughly 357 seconds. This does not guarantee unlimited delays or prevent eventual fuel exhaustion. No fuel/geometry guess or automatic refuel was added.

## Executed evidence

- **59 passed, zero failed** managed NUnit methods. Core and Unity adapter compile against installed Unity assemblies with Editor and release defines. Editor builder and tests also compile. AssetDatabase setup is replaced in this runner with reading the same serialized tuning floats and following the FloodTuning day-asset GUID references.
- Four original balance tests pass: maximum legal pumping with one leak for 120 s still rises at <20% idle rate; repaired/powered electric recovery from .90 to .15 m takes **69.0 s**; dry baseline idle failure at **242.0 s**; recoverable volume/power/leak grid yields negative actual net flow for positive-volume states. Dry volume remains zero.
- Additional tests cover all seven profiles' one-leak ratios and 120 s pumping, three independent coupling shares, invalid/inactive targets, duplicate activation after repair, day-history reset, D-owned completion, continued drainage below safe height, strict BeaconLit threshold, catalog validation/defensive copying, fuel budget and a 24-state three-leak/power/depth recovery grid.
- `NoNetworkingLeakageTest` scans loaded assemblies through reflection. B's managed domain passes, including emitted adversarial fixtures; this does not certify all merged project assemblies. Only exact LK.Net is exempt.
- Unity compile, actual ship Play Mode screenshot and Android build attempts are recorded under `artifacts/round2/B/`; startup cannot initialize Package Manager/license IPC in this sandbox. The manifest and merged C/D/E ship composition are also absent here. No Editor/Android/headset success is claimed.

Reproduce the managed run:

```sh
python3 Assets/Tests/EditMode/Simulation/run_standalone.py --unity /Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents --assembly-cache '/Users/aneeshvasamreddy/Desktop/Virtual Reality Design/Lighthouse Keepers/Library/ScriptAssemblies'
```

The [API document](SimulationAPI.md) contains exact native compile, real Lobby → Play Solo screenshot and APK build commands. Expected APK: `artifacts/round2/B/LighthouseKeepers-Ship.apk`; expected screenshot: `artifacts/round2/B/STN_PIPE_LEAK.png`. **Neither has been produced.** Install only after a current-source build:

```sh
adb install -r artifacts/round2/B/LighthouseKeepers-Ship.apk
```

## In-headset checklist — unexecuted

1. From Lobby → Play Solo enter the actual Shell/Dress House; inspect the single KEEPER WATCH board, collect the real wrench and reach F1 STN_PIPE_LEAK standing. Observe water continuously rising with no plateau.
2. Leave one leak alone: it crosses the pipe's .90 m failure line, remains there for three seconds, then fails. Restart from the lobby; no stale volume or phase should remain.
3. Pump at a comfortable legal cadence: water rises more slowly but never stops while one coupling remains open. Faster/spammed/alternating-player strokes must not exceed the global throughput limit.
4. On a three-leak watch, repair one actual coupling at a time. Observe the board advance only that target and inflow/rise slow by one share. A repaired coupling stays sealed; the other two remain independent.
5. Restore generator power using D's required steps, repair all couplings and watch powered drainage continue through .15 m to dry floor. Recovery from .90 to .15 m should take under 90 s. The F4 amber beacon lights with generator power and goes dark on fault/power loss.
6. Repair the last coupling early: the day must wait for D's clock, remaining board tasks and bed interaction. Exercise all seven days and verify board count, multiplier/fault schedule and timer come from that day's tuning.
7. Repeat with two keepers: both views agree on coupling identities, power, water and beacon; clients cannot inject lifecycle/admin/debug commands. Confirm all actions can be performed standing without forced camera movement or comfort vignette.

B will hold these proposed values until lead tuning notes arrive. Source compilation, transport evidence and device acceptance remain separate.
