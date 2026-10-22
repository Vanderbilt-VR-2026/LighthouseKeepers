# B → C: scene measurements and presentation


## Latest Round 2 — amber beacon and three independent coupling views

`SimSnapshot.BeaconLit` is now a public readonly bool, derived exactly as `GeneratorRunning && PowerAvailable > .5f`; use it for the warm amber/gold F4 lantern. No separate beacon state is written outside simulation. E must replicate constructor inputs. Never use cyan/blue power feedback for the lantern.

Independent physical couplings use IDs **0..2**. Snapshot.ActiveLeakMask bits identify the open couplings; ConfiguredLeakCount gives the day's 0..3 capacity. ActiveLeakCount/RepairedLeakCount are computed read-only properties. Present leak effects per bit; water remains snapshot-derived world Y. A/D/E enqueue targeted repair commands. Seven DayTuningAsset profiles under B's Data/Tuning lane are referenced by FloodTuning; D owns tasks and the sole House board.

B does not create/edit your ShipScenes.json or scene assets under R1. R8 remains pending your canonical manifest, actual anchors and live composition from the lobby; expected B screenshot is artifacts/round2/B/STN_PIPE_LEAK.png, not yet produced. Full signatures: ../SimulationAPI.md.

Status: [PROPOSED] pending C scene integration. B has not modified scenes or prefabs.

## Round 2 — actual ship route required

B now blocks all player builds unless C's `Assets/Scenes/Sprint3/ShipScenes.json` lists the environment lobby at index 0, real Shell House at index 1, existing NetworkLobby as a retained dependency, and all twelve real Shell/Dress House/F1/F2/F3/F4/Exterior scenes, with no legacy world scenes. Use the existing flat `scenes` array schema; do not leave the earlier one-Lab preview manifest. B does not write your manifest or Build Settings under R1. The build gate verifies that enabled settings exactly match the manifest and rejects any unlisted scene passed by an old builder.

The typed `SimulationHostBehaviour.ConfigureScene(profile, floor, plinthTop, authority, canRepairPipe)` composition entry is ready. Bind your actual F1 floor datum and published `STN_GENERATOR/PLINTH_TOP` at Y=3.750, not the generator root. E must create/configure/enable one host only after the shared ship scenes are ready; D supplies the repair validator. Source presence alone is not an installation. Connect water/discharge/lighting/beacon presentation to this same host/facade; beacon color remains your warm amber/gold.

Remove any SimulationDebugPanel component from shipping scenes. B never creates its canvas in a player and Editor preview defaults off; the build scene callback rejects serialized diagnostic components. Do not add any replacement task UI outside the one house KEEPER WATCH board.

R8 is automated by `Round2ShipReachabilityTests.LobbyPlaySoloReachesShipAndCapturesPipe`: it clicks the real lobby Play Solo action and captures `artifacts/round2/B/STN_PIPE_LEAK.png` only after all ship scenes and an initialized Day 1 host exist. Missing/stale Play Mode evidence blocks builds. In this checkout the ship manifest and complete loader integration are absent, and Unity IPC prevents test execution; no screenshot is claimed. See SimulationAPI.md for commands. Earlier notes below are historical where superseded.

1. Publish an exact transform at the **generator plinth top** and the F1 finished-floor origin. `STN_GENERATOR`'s root Y is not a plinth measurement. Assign these to `SimulationHostBehaviour.generatorPlinthTop` and `floorOrigin`; no height is guessed. Missing plinth data deliberately prevents generator power. Reference `Assets/Data/Tuning/FloodTuning.asset` in the driver's `tuning` field.
2. Add exactly one scene-owned `SimulationHostBehaviour`. Enable `useLocalAuthority` only in an offline rehearsal; E injects network authority before `Start` in production. Subscribe water presentation to `OnTick`, using `snapshot.WaterHeightM` (world Y); depth is volume / floor area plus the injected F1 origin. Never integrate/clamp water in presentation. No automatic singleton or scene bootstrap was added by B.
3. Retire the legacy water writers together with D/E. `Assets/_LighthouseKeepers/Scripts/Runtime/Flood/FloodController.cs` caps height in `Update` and `SetHeight`; `StormWatchRules` clamps `State.water`, and `StormWatchDirector` writes that value into the flood controller. These files are outside B's lane and remain untouched. Do not leave two water authorities active.
4. Attach `SimulationDebugPanel` to an explicit standing-height anchor facing the player and reference the driver. Its generated canvas is 1.2 × 1.4 m; centre around standing eye/chest height with all buttons available to the existing XR UI ray. It needs the scene's existing EventSystem/XR UI input. B never moves the rig or a station. Readability at two metres and standing reach remain headset checks.
5. During merge, retain/generated Unity folder `.meta` files for new shared ancestors (`LK.Core`, `Assets/Tests`, etc.). B supplied asset/script metadata inside its owned subtrees, including the fixed FloodTuning script GUID; it did not write shared ancestor metadata outside its lane.

See `Docs/Sprint3/Tuning.md` for numbers and the device checklist. New code is not yet wired into the existing scenes.
