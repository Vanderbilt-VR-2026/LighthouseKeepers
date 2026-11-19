# Day 2 nightmare checkpoint — #144, feature OFF

This is a prerequisite for Day 3 #145, Day 4 #148 and Day 5 #149. Those three scenes are not implemented. Source network: **LK-S5-NIGHTMARE1 / LK-Lab-Coop-v53**. No new APK has been built or installed for this checkpoint.

## Implemented

Host-owned scene phase, independent clock, crew/caught/escaped masks, bounded door progress and validated snapshots. Bedtime intercept, gaze-gated child follow through the authored stairs, wall/ceiling presentation, warning/quiet/escape phases, outside then return, caught-in-place recovery, wake-checkpoint retry and replay skip. Normal night and flood timers pause; completed repairs survive retry. The watch reads 03:00 AM, normal station labels/glows are suppressed, storm sound remains, and the engine/radio quiet appropriately. Six handprints are projected onto actual rail geometry. Three scene-only TTS clips are catalogued; routine narration stays caption-only.

## Evidence

- **85 distinct focused EditMode tests passed**, across the nine receipts in `tests/`. Includes ordinary campaign ordering, Survival, recovery, voice and command validation. No full-suite claim. One mistyped test filter returned zero tests; the correct `StationGuidanceTests` subsequently passed all five.
- `scene/checks.txt`: **26 checks passed** in muted Play Mode, including all 14 NavMesh/capsule-swept escape segments, F3→F2 movement, independent scene clock, warning/quiet, door bounds and outside→return→relief. A test-controlled rig and direct state door calls were used; this is not a locomotion/physical-door playthrough.
- `lifecycle/checks.txt`: **11 checks passed**. Station/ship rules seeded a completed Day 2; the actual bed action entered Wake, catch/retry preserved work, replay skip advanced through ordinary sleep into Day 3. These checks use staged catch and test-controlled rig movement. Initial fixture setup repaired the pipe before its scheduled activation and failed; the corrected fixture follows generator→pipe→radio→ship.
- `transport/`: **actual Photon host/guest on two muted Editors**, actual guest XRDirectInteractor grip/pull opened the host-owned door, and actual host hand grip rescued the caught guest. The victim stayed at the catch location. Scene phase, catch and outside transition were staged by the host fixture. This is transport plus input evidence, not two-headset acceptance. An intermediate fixture assumed the guest's duty slot; the final fixture derives it from the live role assignment.
- Inspected `screens/wake.png`, `outside-return.png`, and `ceiling-review.png`. The last image is a presentation review after correcting an upside-down standing pose to a ceiling-braced pose. Further animation/flashlight polish is still required. Earlier cropped/missed captures are not acceptance evidence.

## Remaining before enabling and delivering #144

Keep `HorrorTuning.nightmareEnabled = false`. Finish an actual movement-input escape through both doors with A stamina, for both roles; verify two-person gaze/wait, late join/disconnect, actual pursuit catch and release/regrip revival. Position the starting child at the authored stair landing and tune the follow duration with that placement. Review the complete wall→ceiling→safe drop/catch-lunge sequence under the real flashlight, rail-print placement, darkness/restoration and the three audio cues. Check return-phase water presentation. Confirm no headset camera movement, no face intersection, no route blocked by the child/doors. Then enable the asset, rerun impacted checks, build and install without launch.

Default measured walking speed was 1.92 m/s; the default sprint setting computes to 3.072 m/s. Late nightmare pursuit was reduced from 3.3 to 2.95 m/s so it remains below that default sprint. This is not an escape-time measurement; slower comfort settings and stamina still need the locomotion test.

## Reproduce

Open the sprint-5 worktree with Unity 6000.3.23f1; set `EditorUtility.audioMasterMute = true` before each run. EditMode only: `python3 artifacts/s5/modes/check.py NightmareIntegrationTests ScareSequenceTests CampaignOrderTests CrewRecoveryTests SurvivalModeTests VoicePassTests StoryImportTests CoopCommandWindowTests StationGuidanceTests`.

Play: `python3 artifacts/s5/scare-scenes/start.py`, wait for completion, then Pipeline `run_script` on one file in `fixtures/` with a fresh output directory argument and entry `NightmareProbe.Begin`, `NightmareLifecycle.Begin`, or `TurnProbe.Begin`. Each enables the flag only in memory. Stop Play before another fixture/test. Live transport fixture: `python3 artifacts/s5/final-scaring/crew-3.py`, after a fresh backed-up peer sync.

## DEVICE CHECKS PENDING

New scene is disabled and not ready for headset acceptance. All worn comfort, visibility, catch distance, door reach, sound balance and sustained Quest GPU/CPU are NOT VERIFIED. Quest was connected; its existing tool-belt build was left installed and was not launched. The requested Day 4–5 eight-step headset test belongs with their finished build.
