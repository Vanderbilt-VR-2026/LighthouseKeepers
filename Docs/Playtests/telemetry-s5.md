# Sprint5 timing — ship phase checkpoint

Instruction-only simulation against the actual host rules. Assumptions: four seconds reading, twelve seconds per floor, six seconds per ratchet stroke/reset, twelve seconds pouring, eighteen seconds tuning, fourteen seconds aligning. Two workers share one host model, not two network clients. Gallery travel and waiting for all authored ships now count. This is not measured human timing, physical navigation or a final campaign approval.

The original placeholder schedule wrecked ships before the beacon could reasonably be restored on Nights3–5. Those raw failing traces remain in `Docs/Verification/S5/Ship/gallery/timing-initial.json`. Ship arrivals/approaches were retimed. The real-XRI Night3 replay then exposed a fuel integration regression: a quick restart exhausted the tank just after the final ship, reopening the generator job during the walk to bed. Only campaign Night3 consumption changed from0.0025 to0.0015 tank/second (40% less); a0.72 refill now lasts480s instead of288s. A regression covers the earliest restart through the final passage and30s to reach the bunk. Legacy Sprint4 data, night duration and flood rates are unchanged. Final targets must be checked again after sabotage, lantern, hunts and split-information are implemented (phase6). All `timingApproved` flags remain false.

| Night | Mode | Modeled total incl. gallery/bed | Watch budget | Spare | Result |
|---|---|---:|---:|---:|---|
| night-1 | solo | 260.1s | 315s | 89.0s (28.3%) | Pass |
| night-1 | duo | 260.1s | 315s | 89.0s (28.3%) | Pass |
| night-2 | solo | 272.1s | 315s | 77.0s (24.4%) | Pass |
| night-2 | duo | 272.1s | 315s | 77.0s (24.4%) | Pass |
| night-3 | solo | 425.1s | 515s | 124.0s (24.1%) | Pass |
| night-3 | duo | 425.1s | 510s | 119.0s (23.3%) | Pass |
| night-4 | solo | 480.0s | 540s | 94.1s (17.4%) | Pass |
| night-4 | duo | 440.1s | 510s | 104.0s (20.4%) | Pass |
| night-5 | solo | 670.1s | 760s | 124.0s (16.3%) | Pass |
| night-5 | duo | 670.1s | 730s | 94.0s (12.9%) | Pass |
| demo | solo | 355.1s | 360s | 39.0s (10.8%) | Pass |
| demo | duo | 355.1s | 360s | 39.0s (10.8%) | Pass |

Targets not yet all met: Nights1–3 should retain25–35%; Nights4–5 15–25%. In this intermediate model Night2 and Night3 are just below target; Night5 duo is below target. Demo ship passage is around five minutes of modeled watch time. The phase6 pass must include conversation/hunts rather than tuning against missing work.

Reproduce: `LK_SHIP_TIMING=1 LK_M3_TIMING_OUTPUT=/absolute/output.json Unity -batchmode --burst-disable-compilation -quit -projectPath "$PWD" -buildTarget StandaloneOSX -executeMethod LighthouseKeepers.Editor.M3CampaignBot.RunBatch`. Unity remains muted.
