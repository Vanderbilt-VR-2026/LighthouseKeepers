# Campaign and Survival

PLAY offers two modes before the existing host / join and duty choices. The host selects the mode for the whole session. A joining player inherits it, including on late join. The modes share the physical stations, body, hands, walkies, lanterns, objectives, B recall, Owen, and sleep system.

Campaign keeps the five-night horror experience. Survival is the judges' no-horror endurance mode: the storm and flood are the threat. Both remain without spoken narration while recording is pending.

Survival starts each day with generator, pipe, radio and beacon work. Fuel and three timed pulls restore power; two keepers brace and wrench the pipe; the pump drains it. One keeper reads the House telegram while the other enters its code on F4. Hold the microphone trigger to clear the convoy, then aim from outside F4. A ship waits when the beam misses; it cannot end the run. Completing every job and guiding the ship permits shared rest on F3. The next day repeats the loop with more pressure, without a five-day cap. Owen takes the complementary duty from Day 1 when alone.

Day 1 is a practice watch: water rises at 40% of its base rate, caps at 0.22 m and cannot end the run. Lantern oil is unlimited that day. All jobs and the guided passage are still required. From Day 2, only sustained high water ends Survival. No deadline, beacon-dark death, horror capture or solo sea-fall defeat. Falling off the island uses the existing ten-second return instead. Unpowered pumping permits continued seepage even after the pipe is fixed. The wristwatch counts elapsed time, and the danger strip shows water pressure. Retry restarts the current day; returning to the lobby starts a new Day 1 run.

Tuning lives in `Assets/WatchDemo/Resources/SurvivalTuning.asset`. Generated day definitions are transient, host/guest deterministic, and do not modify the authored Campaign assets.

| Setting | Survival baseline |
| --- | --- |
| Solo flood multiplier | 1.00 × (1 + 0.08 × days completed) |
| Two-human flood multiplier | 1.10 × (1 + 0.08 × days completed) |
| Beam tolerance | 4° + 10° / (1 + 0.18 × days completed) |
| Ship approach / guided passage | 45 s / 30 s |
| Start / first station grace | 65 s / 65 s on Days 1–2; 45 s / 45 s afterward |
| Early flood multipliers | Day 1 ×0.4, capped at 0.22 m; Day 2 ×0.9 |
| Repaired seep, pump unpowered | 0.0006 m/s × daily flood multiplier |
| Storm base | Rises every two days, up to Peak |

The original `AudioPass/calm`, `tension`, `crisis` soundtrack and its quiet rests are used in Survival and the lobby. Campaign retains the user-supplied score. Survival instantiates no Drowned, possession, fake calls, fear score or horror story papers, and queues no harbor narration. No global scare preference is changed.

Network: **LK-S5-PRACTICE1 / LK-Lab-Coop-v50**. The mode is a host-owned Photon session property and snapshot field. Old builds cannot join. Both players must update.

Issue #141; PR #89. Original mode evidence: `Docs/Verification/S5/GameModes/README.md`. Latest practice-day checks: `Docs/Verification/S5/HousePractice/README.md`.
