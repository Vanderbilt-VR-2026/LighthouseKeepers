# Session C — Sprint 3 environment

STATUS: [PROPOSED]. Authored source assets and scene changes; no Unity compilation, new lighting bake, APK or headset acceptance. The installed Unity6000.3.23f1 cannot start Package Manager's local socket in this sandbox (`listen EPERM`). ADB cannot start its5037 listener. Git cannot write the shared worktree metadata, so branch creation, commits and push were unavailable. The starting detached HEAD is sprint3/base4391afd.

The owned Lab now contains the new wall board/bench, forged wrench art, F1 blue hand pump, F2 diesel generator/plinth/panel/stores and a centrally placed F3 bed. Legacy gameplay and frozen station IDs are preserved. These are environment-assessment changes: the existing Storm Watch production loader uses other scenes, and no production loading/transport adoption is claimed.

`Docs/Sprint3/Heights.md` publishes **generator plinth top world Y=3.750m** (F2=3.300 +0.450). `Docs/Sprint3/Requests/C-to-A.md`, `C-to-B.md`, `C-to-D.md`, `C-to-E.md` and `C-to-F.md` contain the handoffs and integration limitations. `Docs/INVENTORY.md` was absent at session start. The default-sun removal is already in the base; no cherry-pick is needed.

The frozen board anchor is0.20m inboard of the actual wall. The new fixture is offset relative to that anchor to put its back flush on Z=-4.850. Its surface spans1.20×0.80m, Y1.000..1.800, albedo0.85 with0.15× self-emission. `BriefingBoardView.SetLines(string, IReadOnlyList<(string,bool)>)` renders five fixed-height TMP rows and mesh checkbox glyphs. Heading/body cap heights are measured from the actual H glyph at preparation time (65/45mm); text never auto-shrinks. D owns the progression adapter. The legacy TextMesh stays valid but hidden.

The bench top is1.40×0.70×0.06m at surfaceY0.900, centre(1.800,0,-5.750),0.900m from the wall. This new bench occupies the older WP-HOUSE-B marker; the standing approach is from its south side, around(1.800,0,-6.650). The original metadata remains frozen. The wrench has separate resting and generous trigger boxes, rather than resting on an oversized collision volume. A owns final grab binding.

F1 pump root(0.450,0,1.250), yaw90. PIVOT_PumpHandle is local(0,1.050,0.380), +X axis. GRIP_PumpHandle is its child at(0,0,0.250). Range -22.5..+22.5 degrees produces0.191m vertical travel; radius0.250/arc45 degrees cannot produce0.35m. A owns the motion/stroke acceptance and calls PumpDischargeEmitter.EmitStroke(). The emitter never sends simulation commands.

Two **new independent bakes are required**. `Sprint3LightingBake.Bake` creates LightingPreset_Powered and LightingPreset_Fault under Assets/Settings/Lighting/Sprint3, including lightmaps, renderer atlas bindings and probes. The existing BeaconAndMainsLightingGroup.Powered remains the sole power presentation boolean. Fault is red#8B2B22; the house oil lamp uses an independent material so mains blackout preserves it. Recovery fixtures cover the manual pump, starter cluster, breaker and beacon. Atlas coordinates must agree across both states for static batching; otherwise the bake fails. Existing blue/Half maps are stale and are deliberately rejected by the source fingerprint gate.

`LuminanceValidator` reuses the existing linear ARGBFloat sampler:1.6m eye height,0.5m room grid plus stairs/passage/gallery routes, Rec.709 luminance, fixed exposure1 and the same original powered reference0.0214306675 for both states. It retains original0.12/0.04 floor/reference thresholds; the Half-preview branch is unavailable for the new preset names. No automatic recalibration or threshold relaxation is provided. Floor measurements do not prove text readability or comfort.

`GeometryIntersectionValidator` uses renderer bounds against actual wall/floor colliders (AABB broad phase followed by convex bounds-box/mesh penetration), fails beyond0.010m and reports world bounds, separation direction and depth. Building wall/floor/roof/stair construction joints are explicit structural exceptions; furniture is never exempted by ancestry. It also checks the pump sweep and plinth/board datums. The retained shaft liners now stop0.010m below the next slab. The assessment builder additionally runs the existing frozen route-width/headroom validator. Unity physics execution is pending; no intersection-free result is asserted.

`ShipSceneCleanlinessValidator` rejects forbidden names and Unity's built-in meshes even on inactive renderers. ShipScenes.json plus the ShipScene marker scopes actual environment scenes. IProcessSceneWithReport fails the build on a tagged scene with invalid geometry/cleanliness or stale bake/measurement stamps. **The inherited player prefab still contains19 built-in renderer references. A/E must replace those assets; hiding them cannot pass.** No bypass switch is provided. Production scenes must be adopted and tagged by their owners before this gate covers them.

The offline audit passed39488 serialization/native-mesh checks across94 mesh assets. It checks native vertex/index payloads, bounds, local references, hierarchy, anchor uniqueness, frozen station positions, own GUIDs and direct scene primitive references. Two baseline URP shader/version GUIDs require normal package restoration. This is not a Unity import/compile/physics/lighting/device result. Evidence logs are under ignored artifacts/sprint3-environment; compile attempts are artifacts/compile*.log. No APK exists at the target path.

Run these from the repository root in an unrestricted terminal. Resolve the A/E player mesh requests first. Do not rerun apply_to_scene.py on the already-authored scene; it intentionally refuses duplicate application. The Python authoring scripts are reproducible source tools, not a substitute for the following Unity passes.

```sh
UNITY_EDITOR='/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity'
mkdir -p artifacts/sprint3-environment artifacts/builds
"$UNITY_EDITOR" -batchmode -quit -nographics -projectPath "$PWD" -logFile artifacts/compile.log
"$UNITY_EDITOR" -batchmode -quit -projectPath "$PWD" -executeMethod LighthouseKeepers.Environment.Sprint3.Editor.Sprint3LightingBake.Bake -logFile artifacts/sprint3-environment/bake.log
"$UNITY_EDITOR" -batchmode -quit -nographics -projectPath "$PWD" -executeMethod LighthouseKeepers.Environment.Sprint3.Editor.GeometryIntersectionValidator.Run -logFile artifacts/sprint3-environment/geometry.log
"$UNITY_EDITOR" -batchmode -quit -nographics -projectPath "$PWD" -executeMethod LighthouseKeepers.Environment.Sprint3.Editor.ShipSceneCleanlinessValidator.Run -logFile artifacts/sprint3-environment/cleanliness.log
"$UNITY_EDITOR" -batchmode -quit -projectPath "$PWD" -executeMethod LighthouseKeepers.Environment.Sprint3.Editor.LuminanceValidator.Run -logFile artifacts/sprint3-environment/luminance.log
"$UNITY_EDITOR" -batchmode -quit -projectPath "$PWD" -executeMethod LighthouseKeepers.Environment.Sprint3.Editor.Sprint3EnvironmentAuthoring.BuildAndroid -logFile artifacts/sprint3-environment/android.log
adb install -r artifacts/builds/LighthouseKeepers-Sprint3-Environment.apk
```

Each Unity command must return0; inspect its log before proceeding. Graphics-enabled baking/measurement commands intentionally omit -nographics. The last command is the exact future install command, not a claim that an APK was produced or installed. No new packages were added. User-only Git recovery: create sprint3/environment from sprint3/base, make small reviewed commits restricted to the changed owned paths, then `git push team sprint3/environment`. Never stage artifacts or generated project caches.

Pending in-headset checklist:

1. Enter the house, switch Powered=false through D's adapter, stand2.5m from the board and read the heading and all five rows without leaning; complete a row and see its checkbox tick.
2. Stand at the near edge of the bench, pick up the visible wrench on the first reach and set it back; it should rest on the timber rather than hover or fall through.
3. Walk from the house through all floors and the gallery. Inspect bed/frame/blanket, table feet, generator plinth and walls; observe no clipping or obstructed stair approaches.
4. Grip the manual pump and sweep the full45-degree range without touching walls/props; accepted down-strokes should produce an outlet surge after A binds the emitter.
5. Trigger power failure through D: confirm dim red illumination on every floor/landing, visible floor/stair edges, an independently burning oil lamp, and legible pump/starter/breaker/beacon recovery controls. Restore power and observe warm mains illumination return.

Device comfort, actual black-level/readability, network integration and sustained Quest performance remain unmeasured.
