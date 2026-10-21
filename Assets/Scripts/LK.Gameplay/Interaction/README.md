# Session A — hands and physical verbs

STATUS: [PROPOSED] source implementation; Unity import, shader compile, Play Mode, Android build, transport integration and headset acceptance are unverified. Do not label this as a deliverable APK.

## Source and dependencies

- Two hand prefabs in `Assets/Prefabs/Hands/Resources/Hands` contain an OpenXR-style 26-joint hierarchy: wrist + palm + thumb four + four fingers five joints each. `HandRig` creates the continuous skinned glove from authored mesh/pose payloads. Open/Fist/WrapGrip/Pinch are baked; PumpHandle uses WrapGrip. Grip/trigger run continuously in empty air. Input settles 90% within 75 ms; selection/release uses a 75 ms eased transition. No tracking package or Animator is required.
- Every installed grip has `PhysicalGrip`, an explicit attach, direct-only hover/select filtering, Fresnel rim feedback, positioned grab/release/impact clips, and the agreed haptic pulses. The wrench preserves authored contact or derives handle contact from bounds, adds a trigger sized in metres, uses XRI inherited throw velocity, and persists only the tool across scene unloads. Canceled selection can transfer to a replacement local hand while the physical grip remains held. Actual carry/throw requires headset testing.
- `ConstrainedArc` drives the manual pump (45°) and primer (25°); the rendered hand follows the constrained grip while the tracked interactor stays with the controller. Full bottom→top→bottom cycles count once, interrupted cycles reset, and B's referenced tuning supplies manual timing. Choke uses two detents with hysteresis. `GeneratorRestart` constrains the pull to 0.40 m down, springs home on release, fails audibly without three primer strokes/correct choke, and emits a valid-start event only when physically ready. D/B retain run/power/flood authority.

Bindings use C's exact names. DirectGameplayFilter covers both XRRayInteractor and Starter Assets NearFarInteractor, while preserving teleport targets and the separate UI path. New source does not edit scenes, existing rig prefabs, legacy WatchControl/WatchWrench, simulation, progression or networking. The current base lacks the new pump/generator grip anchors and B tuning. Source therefore cannot establish playable Sprint 3 integration by itself. Requests: `Docs/Sprint3/Requests/A-to-B.md`, `A-to-C.md`, `A-to-D.md`, `A-to-E.md`. Optional `DISCHARGE_ManualPump` places the accepted-stroke water burst at C's outlet; no invisible fallback position is presented as a valid surge.

`InteractionBootstrap` adapts the old active-camera XROrigin/Left Controller/Right Controller hierarchy. E should embed hand prefabs explicitly and assign actor IDs; remote avatars must not run local input. Legacy scene scripts remain unchanged and need replacement by C/D in their lanes to avoid competing progression. D must call each component's reset methods on day/retry/reset. Unity may generate the new shared `Assets/Scripts/LK.Gameplay.meta` on import; this ancestor sidecar is outside A's literal Interaction lane and has deliberately not been authored here.

## Tapan branch review

Fetched `team`; base HEAD is `4391afd` (`sprint3/base`), not main. No branch was merged/deleted. Branch creation failed because the worktree Git index is outside the writable sandbox; edits remain on detached HEAD and are uncommitted/unpushed.

| Reference | Assessment | Specific reason |
| --- | --- | --- |
| station-anchor-binding, original `5b83592`, tip merge `d764199` | Partially reused (approach only) | `ValveHandwheel` correctly disables free tracking and projects wrist rotation onto an axis. A reuses that separation of tracking/constraint, while replacing absolute twist accumulation, adding hand poses/feedback and using signed wrench tightening. It has no integrated pose/haptic path. No source is cherry-picked. |
| generator-starter-lever, `3490087` | Rewritten | `LeverSwitch` clamps wrist rotation to a pivot and signals a toggle threshold. It is not a 0.40 m linear down-pull and has no spring return or primer/choke gating. A uses constrained displacement and explicit readiness. |
| beacon-interaction, original `c1154ca`, tip merge `107802f` | Rewritten physical-verb approach; scene props not imported | The original change is scene-only: lever/wheel props reference LeverSwitch/ValveHandwheel and signal PuzzleSockets. It adds no new verb code. The tip's empty three-dot diff initially hid that original change; it was inspected separately. Beacon geometry/sequencing stays with C/D. |
| radio-interaction, `a0fabda` | Partially reused (event-binding pattern only) | PushToTalkButton uses XRI activated/deactivated correctly. No frequency-dial implementation exists in its C# diff; the scene reuses ValveHandwheel. A follows the same event-lifecycle pattern, and leaves radio code/scenes untouched because radio is outside A1–A5. |

## Actual evidence — October 20, 2026

[VERIFIED] narrower offline checks only:

- Runtime and Editor source compiled against the installed Unity 6000.3.23f1 reference assemblies and existing XRI 3.6.0/InputSystem/CoreUtils DLLs. Two CS0649 warnings are serialized private reference fields assigned in Unity assets; they are not runtime validation.
- Seventy executable behavioral assertions passed against the actual StrokeCycle/GeneratorStartState classes: ten natural cycles, partial travel, rapid cycles, top/bottom dwell, release/regrip, idle-time bypass, invalid inputs, analog hysteresis and XRI press/release frame edges, unprimed/wrong-choke failure, recovery, consumption and reset.
- Both payloads have 26 parent-ordered joints, four normalized poses, valid skin weights/indices, and closed manifold surfaces; prefab references resolve by GUID. Left/right meshes currently have 19,972/20,392 triangles. These counts are not a Quest performance measurement; appearance, pose fit and sustained CPU/GPU time remain device checks.

The verifier is reproducible after a normal Unity import:

```sh
python3 Assets/Scripts/LK.Gameplay/Interaction/Verification/verify_hands.py --assemblies Library/ScriptAssemblies
```

This run instead read the pre-existing sibling project's ScriptAssemblies without changing it. Results are in ignored `artifacts/hands-offline-verification.txt`. It does not import prefabs, execute Unity native calls, compile shaders, run XRI in Play Mode, or build IL2CPP.

Unity compile/build was attempted at baseline, A1+A2, A4 and final review. The compile/A1+A2/A4 attempts aborted before compilation because UPM cannot open `/tmp/Unity-Upm-*.sock` (`listen EPERM: operation not permitted`). An overlapping final attempt also reported an existing Editor lock, then exited; it is not build evidence. Logs: ignored `artifacts/compile.log`, `compile-a1-a2.log`, `build-a1-a2.log`, `compile-a4.log`, `build-a4.log`, `compile-final.log`, `build-final.log`. ADB's daemon cannot open its listener either (`could not install smartsocket listener: Operation not permitted`). No APK was produced or installed; no headset was inspected.

Blender 4.5.9 headless also exits 139 during Metal initialization, before running Python (`MTLBackend::metal_is_supported`, null device). `author_hands.py` remains reproducible Blender source. `portable_hands.py` authored the checked-in continuous skinned meshes and poses entirely on CPU; it is not a rigged primitive glove. `serialize_unity_assets.py` writes deterministic prefab/material/metadata references from those payloads. No third party art or package was added.

## Commands on an unrestricted development machine

Run from the project root, with Unity closed:

```sh
mkdir -p artifacts
"/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity" -batchmode -quit -nographics -projectPath . -logFile artifacts/compile.log
LK_HANDS_APK=artifacts/LighthouseKeepers-Hands.apk "/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity" -batchmode -quit -nographics -projectPath . -executeMethod LighthouseKeepers.Gameplay.Interaction.Editor.HandsBuild.BuildQuest -logFile artifacts/build-hands.log
adb install -r artifacts/LighthouseKeepers-Hands.apk
```

The build helper reads the current build list without authoring scenes. Its callback binds B's exact FloodTuning asset into A's settings and writes link.xml for the concrete type so IL2CPP preserves the reflection-read tuning. Missing tuning logs a warning and leaves manual stroke events disabled; integrate B/C/D/E before accepting the pump build. In Editor use **Lighthouse Keepers → Hands → Bind simulation tuning**, then **Validate authored assets** before Play Mode. New shader/prefab/audio import still needs a normal Editor session.

After reviewing the source, an unrestricted user can create `sprint3/hands` from this unchanged base and make small commits with conventional prefixes. Stage only A's source/art/prefabs/metas, request files and the appended decision lines; exclude Library/Temp/Logs/Build/Builds/artifacts and project files. Push only with `git push team sprint3/hands`. This sandbox could not create a branch or commit, so no commit hash is reported.

## Numbered headset acceptance — all pending

1. Stand in empty space. Squeeze/release each grip: fingers form a fist and reopen smoothly without approaching an object. Use each trigger with grip released: thumb/index move to Pinch. Check both eyes and rapid alternating inputs; no snap or long lag.
2. Reach for the wrench at the table with either hand. Hover shows a surface rim and a short pulse; grip brightens it, wraps the fingers and plays a positioned grab. The handle sits through the fist. Throw it across the room: it leaves with hand momentum, rotates, lands and makes an impact sound. Regrab, carry between floors/scene transitions while holding grip, then release; no camera motion or vanished tool.
3. Grab the manual pump with each hand in turn. Move through ten comfortable full arcs at natural pace: ten top clunks, ten distinct down-stroke thumps/gulps and ten outlet surges. Drift the real hand off the arc: only the glove stays on the constrained handle, which never leaves its pivot. Water changes only through B's authoritative event consumer.
4. Try partial strokes, fast jitter, a stationary grip and release/regrab at the top. None should remove water or produce extra down-stroke thumps. Resume a natural full cycle and verify recovery. Try seated and standing without crouching or lifting the shoulder.
5. Before priming, grab/pull the starter 0.40 m down: hear a failed crank, observe spring return on release and no running generator. Prime only twice and repeat: still fails. Prime a third time with wrong choke: still fails. Snap choke closed and pull again: one valid start event/catch sound. After D resets the day, unprimed failure returns.
6. Use the UI ray for distant boards; it must not grab the wrench or pump. Confirm local actor IDs on both clients, one authoritative event per stroke/start, retry resets and held-tool continuity after a real scene/rig transition. Measure frame time/thermal behavior on Quest 3; no comfort/performance claim comes from offline checks.
