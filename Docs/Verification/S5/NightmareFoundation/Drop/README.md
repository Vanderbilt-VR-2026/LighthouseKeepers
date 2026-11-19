# Day 2: safe drop behind the keepers (#144)

The previous 3.2 m search could miss a safe closer position and leave the child in the corner. The search now tries an inner 1.8 m ring after the outer ring, preferring directly behind before trying the sides. Both keepers' real NavMesh routes must stay at least 1.5 m from the candidate, both heads must remain clear, the floor/capsule must be unobstructed, and the candidate must route back to the keeper. If those conditions cannot be met, it still uses the safe corner fallback. This is host-authored placement within the existing synchronized scene frame; no new network field or protocol version was introduced.

## Evidence

- A geometry audit using the head poses and gaze actually observed during a two-Editor run found why the earlier search missed: the nearer stair-side floor was available while the outer ring was not (`results/drop-audit-real-1.txt`). In the real scene `DropPlacementProbe` checks the selected position, both routes, both viewer clearances and rejection of impossible route clearance: **7/7 passed** (`results/drop-placement-4`).
- Two muted Editors joined via Photon, took the explicitly staged failure/retry checkpoint, then followed the child through the ordinary wake/follow/turn/warning/escape/return without staging later beats. Virtual heads, sticks, run and hand poses used the actual XRI movement, capsules, stamina and door grips. The host placed the child behind the keepers at `(-2.20, 3.56, 2.07)` while the keepers were near `(-0.41, 3.31, 1.84)`; both escaped through the doors and returned in about 12 seconds without a catch (`results/drop-final-natural-1`). This is one simulated pose pattern, not human or headset acceptance.
- Both solo roles escaped from a staged generator-room start using the same real movement and grip/pull paths, each in about 11 seconds (`results/drop-solo-lightkeeper` and `results/drop-solo-machinist`). The pose and turn beat were staged; this is not a full night.
- **50 focused EditMode checks passed**: NightmareIntegration 5, ScareSequence 22, CrewRecovery 13 and SurvivalMode 10 (`results/editmode`).

## Remaining

Worn drop and lunge comfort in both eyes, varied real-player positions, controller reach, whole Night 2 without staged tasks, two-headset co-op and sustained Quest GPU/CPU are not verified. The bedtime scene remains disabled pending completion of its remaining presentation and acceptance work. See [current checkpoint](../../../../Design/Next.md) and [earlier presentation evidence](../Continuation/README.md).

## Reproduce

Use Unity 6000.3.23f1 with `EditorUtility.audioMasterMute=true` before every Play/test. Start Campaign Day 2 with `python3 artifacts/s5/scare-scenes/start.py`; call `DropPlacementProbe.Run` through Pipeline with a fresh absolute output path. With a backed-up matching peer worktree, run `fixtures/natural-setup.py`, the earlier `Coop/fixtures/retry.py`, then `fixtures/natural-debug.py`, each with its own fresh `LK_TEST_OUT`. The setup and retry deliberately stage entry; all other limitations are above.
