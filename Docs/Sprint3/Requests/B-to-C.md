# B → C: scene measurements and presentation

Status: [PROPOSED] pending C scene integration. B has not modified scenes or prefabs.

1. Publish an exact transform at the **generator plinth top** and the F1 finished-floor origin. `STN_GENERATOR`'s root Y is not a plinth measurement. Assign these to `SimulationHostBehaviour.generatorPlinthTop` and `floorOrigin`; no height is guessed. Missing plinth data deliberately prevents generator power. Reference `Assets/Data/Tuning/FloodTuning.asset` in the driver's `tuning` field.
2. Add exactly one scene-owned `SimulationHostBehaviour`. Enable `useLocalAuthority` only in an offline rehearsal; E injects network authority before `Start` in production. Subscribe water presentation to `OnTick`, using `snapshot.WaterHeightM` (world Y); depth is volume / floor area plus the injected F1 origin. Never integrate/clamp water in presentation. No automatic singleton or scene bootstrap was added by B.
3. Retire the legacy water writers together with D/E. `Assets/_LighthouseKeepers/Scripts/Runtime/Flood/FloodController.cs` caps height in `Update` and `SetHeight`; `StormWatchRules` clamps `State.water`, and `StormWatchDirector` writes that value into the flood controller. These files are outside B's lane and remain untouched. Do not leave two water authorities active.
4. Attach `SimulationDebugPanel` to an explicit standing-height anchor facing the player and reference the driver. Its generated canvas is 1.2 × 1.4 m; centre around standing eye/chest height with all buttons available to the existing XR UI ray. It needs the scene's existing EventSystem/XR UI input. B never moves the rig or a station. Readability at two metres and standing reach remain headset checks.
5. During merge, retain/generated Unity folder `.meta` files for new shared ancestors (`LK.Core`, `Assets/Tests`, etc.). B supplied asset/script metadata inside its owned subtrees, including the fixed FloodTuning script GUID; it did not write shared ancestor metadata outside its lane.

See `Docs/Sprint3/Tuning.md` for numbers and the device checklist. New code is not yet wired into the existing scenes.
