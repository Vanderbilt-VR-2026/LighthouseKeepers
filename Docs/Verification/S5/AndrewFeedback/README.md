# Andrew / Aneesh playtest corrections — 18 November 2026

Source branch sprint-5, PR #89. No AI speech; Editor muted throughout. Both players require **LK-S5-LANTERNSTART1 / LK-Lab-Coop-v48** because Night 1 now includes lantern state.

## Implemented and inspected

Immediate height-relative, headset-following briefing with Skip. Personal arrival objectives: walkie, lantern, wick, wrench. Walkies no longer exclude the hand hover colliders. Radio power is grip then lift the visible tip; microphone guidance explicitly requires the held mic trigger. Code request changes to code entry after Owen delivers it. Day 1 now includes a House code reader in paired play. House telegram and standing orders moved clear of the lanterns/radio. Owen explains his complementary job from Night 2; wrists hang down. Reviewed front, side and rear images; no neck gap in these poses. Human avatar unchanged.

Beacon wheel uses hand travel around its rim; tolerance narrows from 14° / 11° / 9° / 7.5° / 6° on Nights 1–5. Objectives specify the **outside Floor 4 gallery**. Both snap and smooth providers disable the accidental 180° turnaround. Ordinary faults, ship activity and radio completion no longer trigger the intense score overlay; possession and visible horror still do.

Five dim fixed room landmarks, freshly baked powered/emergency lightmaps, stronger lantern pools with a 32px cubemap cage cookie, denser exterior fog. No added realtime lights or shadow passes; two shadowless portable lights remain the existing quality exception. Fog hides distance but does **not** cull geometry. Removed lamp cable drops and lighting rings; retained machine circuits and risers: 4,734 triangles, five combined meshes, one shared material. GPU cost NOT MEASURED.

Possession rise uses an original synthesized wet-fabric/low strain loop (`Assets/Audio/S5/Resources/S5Audio/possession-rise.wav`, mono 22.05kHz), spatially located at the floating keeper; it stops on recovery. No downloaded sound or voice model was used.

## Verification boundaries

`interactions.json`: 13 actual physics-hover / XR input checks, including walkie grip/release, radio lift and wheel arc. Hardware reader disabled only to inject controller edges; XR selection filters remain active. `tutorial.json`: muted live Night 1 accelerated rule route, pipe → fuel/starter → beacon → radio → guided ship. It injects station actions and is not human travel/feel evidence. `code-fetch.json`: live Owen walked to the relocated telegram and returned its actual code.

`timing-final.json`: rule simulations with assumed travel/read/action times, not measured first-timer performance. All Nights 1–5 pass solo and two workers. The older Demo still fails (solo time limit; paired stalled instruction); it is not accepted. Full suites were not rerun.

| Night | Solo seconds | Duo seconds | Flood rate vs previous |
|---|---:|---:|---|
| 1 | 362.3 | 387.8 | unchanged, no fail |
| 2 | 389.1 | 389.1 | +50% |
| 3 | 479.1 | 479.1 | +50% |
| 4 | 601.8 | 602.8 | +50% |
| 5 | 759.4 | 759.4 | +15% solo; co-op unchanged |

A 50% Night 5 increase killed both careful model runs; even +15% killed the duo. Final co-op peak water is 0.513m against a 0.55m loss threshold, so no further increase ships without a headset playtest.

## Still needs a headset

Readability while turning, hand feel, radio/belt stability, realistic wheel difficulty, lantern darkness/fog/ship visibility, physical possession sound balance, Owen moving/reviving, sustained CPU/GPU, two-headset voice and shared sleep. Editor screenshots are inspected composition evidence, not stereo or performance acceptance.

## Live transport rerun

Two muted Editors passed host/join, enabled Night 1 lantern snapshot, preparation hold, radio clearance, ship pass, leave/rejoin and retry. The first run caught a MaterialPropertyBlock initialized in a MonoBehaviour constructor; moved it to Awake and reran with **zero new runtime exceptions**. These actions were scripted; microphone audio and real controller input across devices remain pending.
