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

## Design contract and acceptance criteria

This slice applies the repo's [agent guidance](../.AGENTS/README.md). Intent: create pressure that players can relieve through a tactile repair, then reward a safe ascent with breathing room. Context: the isolated First Watch entry loads the six existing environment scenes and reuses the existing XR rig, OpenXR/XRI inputs and flood system. Constraints: preserve shared locomotion and controller bindings; keep this local rehearsal separate from future multiplayer authority.

| State | Trigger | Observable result |
| --- | --- | --- |
| Briefing | Select Begin watch | Water rises and the seal order is shown. |
| Repair | Nearby hand selects brace; other hand selects and twists the expected wheel | Accumulated turn progress increases; releasing preserves it. At the threshold the seal becomes green with audio and haptics. |
| Repair | Turn a wrong, unsealed wheel while braced | Water rises once per cooldown; the board explains the mistake. |
| Repair | All three seals complete | Spray stops, water drops, rise slows, and boards direct the player upstairs. |
| Escape | Player reaches the first landing with head above water | Flood pauses and the recovery countdown begins. |
| Recovery | Leave the landing or submerge | Return to Escape; the recovery countdown resets. |
| Recovery | Remain safe for the configured recovery time | Complete; a restart action appears. |
| Repair / Escape | Exhaust submerged-head grace time | Failed; flood pauses and restart appears. |
| Complete / Failed | Explicitly select Restart | Return the rig to the entry, restore leak/flood and clear progress with a new order. |

Interaction contract: the valve remains fixed in world space; wrist orientation projected onto its local forward-axis plane contributes absolute angular travel. Either turn direction counts. Grip selection alone does not turn it; distant ray selection cannot contribute. The default hand reach is 0.65 m, with a 2.3 m station proximity gate. Release and regrip are supported. The brass brace and labelled wheels are signifiers; green hubs and board progress confirm sealing. The interface does not claim a separate hover animation or physically simulated free-grab behavior.

Design tuning is on `FirstWatchDirector`: seal degrees, air grace, recovery time, clamp duration, mistake cooldown and water rise, flood speeds/drop/start height, station reach and landing bounds. Hand reach is on each `FirstWatchValve`. Defaults retain the original pacing. Rule thresholds are copied into one rules instance at Awake; change those before entering Play Mode. UI reads its rule instance and the director's clamp setting. `Lighthouse Keepers → First Watch → Validate entry references` checks the saved entry without replacing the open scene; play and build also run this preflight.

Acceptance checks: reject repair from outside the station; require brace/clamp; retain partial turns; limit repeated wrong turns; stop and restore the spray; prevent premature ascent from winning; require safe recovery; permit failure and a clean retry. Test nondefault thresholds to confirm Inspector tuning affects the rules rather than only the displayed text. Check saved interaction references before building.

Performance and evidence: boards refresh at 10 Hz, tracked wrist input remains per-frame, and the shuffled order string is built once per retry. No target-headset performance improvement is claimed. The 72 Hz target has a 13.89 ms frame budget; measure CPU/GPU frame time, draw calls and GC allocations on the same representative repair/ascent workload when a Quest is available. Record headset model, refresh rate and run duration, including a sustained run for thermal behavior. Comfort, reach, haptics and binocular text readability require that device session.

Tooling dependency: `com.unity.pipeline` **0.8.0-exp.1** supplies the Unity CLI Editor bridge. This experimental package is pinned in the manifest and lockfile; it does not add a gameplay framework. Player/runtime Pipeline access remains disabled. Existing XRI **3.6.0**, OpenXR **1.18.0**, Input System **1.20.0** and URP **17.3.0** provide the gameplay APIs. No new external art or audio licenses are introduced; the slice uses authored primitive controls and existing project assets.
