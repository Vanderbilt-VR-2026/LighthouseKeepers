# Sprint 3 day progression — Session D

This is the opt-in Sprint 3 sequence requested for solo Days 1/2 and a two-keeper pressure-repair Day 3. It does not replace the existing Storm Watch or First Watch scenes. Those scenes still have their previous behavior. No shared scene, rig, simulation, interaction or lighting file was changed by D.

## Integrate once A/B/C/E are merged

1. C/E adds **one** `LighthouseKeepers.Core.Progression.DayDirector` to the Sprint 3 gameplay entry, with no concurrent legacy flood/progression director. Assign `Assets/Data/Days/DayCatalog.asset`, B's simulation host, and `Assets/Data/Tuning/FloodTuning.asset`. Optional `AssignDependencies(...)` supports runtime setup.
2. E calls `ConfigureAuthority(isAuthority, keeperCount, localActorId)`. Only the host calls `BeginSavedDay()`. The default is inactive; clients never assume authority or load a local checkpoint. E must validate remote actors, positions and pressure reports before forwarding them. D implements no transport.
3. Add `DayBoardPresenter` at the briefing board/radio and assign the D director. C supplies `SetLines(string[])` on the renderer under `STN_BRIEFING_BOARD`, with `SetActions(string[], Action<int>)` or equivalent existing buttons wired to Continue/Retry. Preserve those methods for IL2CPP. The presenter never creates visible geometry or moves an anchor. It supplies the existing Button Pop sound quietly using a separate positioned source from narration. Day assets still need recorded `briefingVoice` clips.
4. A/E sends the authority the physical reports listed in [D-to-A](../../../../../Docs/Sprint3/Requests/D-to-A.md) and [D-to-E](../../../../../Docs/Sprint3/Requests/D-to-E.md). Choose **one** route for each discrete input: A's shared event or a direct report, never both. Proximity discovery reads the local head at the named pipe anchor; E forwards remote head positions. A reports grip near the named bed; D validates reach without moving the rig.
5. B supplies the APIs requested in [D-to-B](../../../../../Docs/Sprint3/Requests/D-to-B.md). B consumes pump/repair events, while D alone enqueues validated StartGenerator. A and B must call `CanRepairPipe(actorId)` **before** accepting Day 3 repair effort/state; an event received after B already repaired a leak cannot enforce cooperation. A/E refreshes the different-actor isolation hold at >=10 Hz, clears it on release/unsafe pressure, and validates the actual drifting valve.
6. C supplies the fault/recovery presentation, spatial leak audio, physical gauges and comfort fade requested in [D-to-C](../../../../../Docs/Sprint3/Requests/D-to-C.md). A supplies distinct failed-crank/catch sounds and audible reasons using D's `RestartFeedback`. A requested start is not an engine catch; use actual simulation state for that sound.

The base has no `Assets/Scripts/LK.Core` parent metadata. D supplies metadata for its owned Progression tree only; B/integration should supply the shared `LK.Core.meta` without creating competing copies.

## Runtime boundaries

`DayFlow` is a pure authority-side state machine implementing the exact shared `IDayDirector` interface. Its immutable objective view is also usable by E's client projection. Its predicates only inspect facts and a read projection of the shared snapshot. It never computes or assigns water, power or flood failure duration. The simulation's `Phase == Failed` wins over success; B owns the three-second threshold sustain.

`SharedSimulationPort` caches B's exact `ISimulationHost.Current` fields and `Enqueue` method by reflection because B's types do not exist on `sprint3/base`. It does not redeclare any shared A/B type. Reads are capped at 10 Hz in the controller; no dynamic generic callbacks or expression compilation are used. The port requires a public `(SimCommandKind, int, int, float)` constructor, `ForceGeneratorFault()`, `ActivateDayLeak(int)` and `safeHeightM` on B's tuning asset. Missing dependencies leave the session unavailable and log the exact integration error; there is no fallback flood model. `link.xml` covers conventional A/B assembly names; adjust it to the final names when integrating.

Before timing a new day, D enqueues ResetRun + BeginDay and waits for a changed Running tick. Day 1 starts its leak at 15 seconds. Day 2 requests its atomic generator fault/seep at 45 seconds. Incident feedback is raised once; C should use the applied simulation snapshot for synchronized lights/audio, since command application may lag one tick. On generator recovery, B must resume powered pumping and resolve the fault seep. After a repair, water must reach B's `safeHeightM` before bed use is accepted. Early grips are not banked.

Restart fuel/primer/choke contributions belong to the crew and can come from either keeper. Failed cranks change no completed objectives and enforce a two-second cooldown. Unacknowledged starts can be retried after that interval. Engine restart statistics count actual off-to-on snapshot transitions. The choke can open after ten seconds of actual running. Fuel starts at the asset's 0.25 value when A initializes the physical gauge; D requires a reported gauge examination/refill and blocks an empty tank.

Day 3 with one keeper sends no reset/begin/incident commands and displays **Day 3 requires two keepers.** During a crew loss, the flow pauses and invalidates the isolation hold; E owns disconnect/session termination. The repairer cannot authorize their own hold. A heartbeat older than the asset's 0.3-second freshness limit is rejected. A owns the drift, pressure calculation/feedback and preservation of unfinished physical repair effort.

## Authoring and checkpoints

Duplicate a `DayDefinition` asset, set a unique positive day number, author ordered objective predicates and timed incidents, and add it to the catalog. There is no switch on the day number. The development selector reads the catalog, including later authored days. Enum fields are serialized values; append future predicates/kinds rather than reordering existing values. A new kind of physical verb or simulation incident still needs its owning session to implement it.

Results freeze time taken, peak water height, pump strokes and observed generator restarts. Continue is offered only when another catalog day exists; a final result has no dead Continue button. Retry resets the same day's physical setup without removing any unlocked day. `PlayerPrefs` key `LighthouseKeepers.Sprint3.CurrentDay.v1` is saved at normal day start, failure and success (the next available day). It stores day progress, not an in-flight simulation or inventory snapshot. Development jumps and their continuation/retry never overwrite that checkpoint.

`JumpToDay(int)` and local preview context menus exist only under `DEVELOPMENT_BUILD || UNITY_EDITOR`. Once E has configured authority, the C board exposes DEV day buttons, including direct Day 2 entry with no briefing audio. ResetRun + BeginDay provides the known baseline; the 45-second calm interval is retained for timing tests. Day 3 still requires two keepers. D does not grant a client authority to request jumps.

## Reproduce verification and build

From the repository root:

```sh
python3 Assets/Scripts/LK.Core/Progression/Tests~/verify.py

/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -nographics -projectPath . -logFile artifacts/compile.log

/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -nographics -projectPath . -buildTarget Android \
  -executeMethod LighthouseKeepers.Core.Progression.Editor.Sprint3ProgressionBuild.BuildQuest \
  -logFile artifacts/progression-android.log

adb install -r Builds/LighthouseKeepers-Sprint3-Progression.apk
```

Run Unity commands sequentially after the preceding process exits. The build helper validates that enabled scenes depend on the new DayDirector and catalog, and refuses to publish an unchanged legacy scene as this feature. It uses the existing build list and explicit LZ4, without changing project settings. On the base branch, C/E integration is still required before that preflight can pass.

See [Verification.md](Verification.md) for actual executed evidence and the numbered headset checklist. Offline C# compilation is not Unity import or an Android build.
