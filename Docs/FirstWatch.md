# First Watch — plumbing escape

The first playable slice of the survival ascent: contain one ground-floor leak, climb to the first landing, and earn twelve seconds of recovery. The long-term goal is to survive together and reach the lantern. This slice ends at the first landing.

## Play

Unity **6000.3.23f1**: **Lighthouse Keepers → First Watch → Play plumbing escape**.

The dedicated `Assets/_LighthouseKeepers/Scenes/Development/LK_FirstWatch.unity` entry scene reuses the six existing environment scenes and the shared Quest player. The ordinary environment entry, content scenes, player prefab and enabled build list remain unchanged. Disconnected Photon sample controls are inactive in the First Watch entry.

1. Select **Begin watch** on the physical briefing board. The flood begins at 0.10 m.
2. Find the marked seal controls beside the ground-floor plumbing manifold. Read the three-letter order on the board above them.
3. Hold the brass brace with one hand. Grip a valve with the other and twist your wrist; release and regrip as needed. Each valve needs 270 degrees of accumulated turning. Follow the posted order; a green hub means that seal is complete.
4. For one-handed practice, select **Latch clamp** near the station. It holds the brace for twenty seconds and can be relatched. Partial valve progress survives releasing the brace.
5. After all three seals, the pipe spray stops, the water drops by 0.65 m (bounded by the existing minimum), and it rises much more slowly. Climb one floor and step fully onto the first landing.
6. Stay on that floor for twelve seconds to finish the slice. Leaving the landing resumes the escape phase. The result board offers a fresh attempt.

Quest: existing left-stick movement, right-stick turn, grip to hold/twist, tracked pointer and trigger to select board controls. Reach, haptics, hand tracking alignment, comfort and frame rate still require device testing.

Editor: WASD moves, Q/E turns, Enter begins. Near the controls, hold **B** to brace and hold **1 / 2 / 3** to turn A / B / C. **C** latches the clamp. **R** retries after success or failure. These shortcuts call the same repair rules and proximity checks as the physical controls. Mouse and Quest use the same world-space boards.

## Pressure and recovery

- Repair flood rate: 0.018 m/s, with the existing height acceleration. At a stationary 1.65 m head height, this is roughly 78 seconds from the starting water height to submersion, before mistakes.
- Wrong valve: +0.08 m of water, at most once per 1.5 seconds. Completed seals cannot produce additional rewards or penalties.
- A submerged head has six seconds of air. Getting above water replenishes this grace at twice real time. Failure pauses the flood; retry returns the player to the entry and resets the attempt.
- Successful repair: −0.65 m water, then a 0.003 m/s base rise rate.
- First landing recovery: water pauses, Day 1 weather returns, and twelve safe seconds completes the slice. Recovery requires feet at the first-floor elevation, inside the tower, with the head above water.
- The valve order changes on retry. No assets or scene changes are saved during play.

## Scope

This is a **local solo rehearsal of a cooperative-shaped task**. It supports two hands or the timed clamp, not two synchronized headsets. `FirstWatchRules` owns progression, while `FirstWatchDirector` owns the local flood and scene response. Real cooperative play needs one network authority, replicated state/inputs, tracked remote players and shared completion/failure rules; the existing Photon prototype does not supply these.

The current goal is to evaluate the pressure → repair → escape → recovery rhythm before expanding to generator, radio and lantern floors. It is possible to explore other floors; reaching them does not bypass the repair objective or complete this slice.

## Build and verification

**Lighthouse Keepers → First Watch → Build Quest APK** writes `Builds/LighthouseKeepers-FirstWatch.apk` using the isolated entry and six content scenes. It checks Unity's build result and error count, and writes `Builds/FirstWatch-Build.txt`. It does not install or launch on a headset.

Tests cover brace/order requirements, retained turn progress, bounded mistake penalties, repair rewards, premature ascent, safe recovery, drowning, retry, and the authored scene's single rig. Verification on October 8: **43/43 Edit Mode tests**, **9/9 full Play Mode tests**, followed by **3/3 focused Play Mode tests** after final feedback and interaction changes. The focused run includes real XRI selection with simulated hand transforms and wrist rotation, pipe-spray shutdown/reset, and drowning/retry. These are Editor checks, not headset acceptance. Reports and rendered previews are in [Verification/FirstWatch](Verification/FirstWatch/README.md).
