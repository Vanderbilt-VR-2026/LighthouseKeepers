# Drowned / weather — November 14

Fairness:84 focused checks passed.12 instruction-only task/travel models pass with sabotage enabled; Night1/2/3 outcomes unchanged. Night5 duo137s spare. The prior enabled failure is preserved in artifacts/s5/drowned-storm/fairness-timing.json; it exposed a second bug where the beacon keeper assignment changed during travel. Fixed by preserving that assignment for the passage.

Offshore wait:120s maximum, before danger entry only, only while a two-keeper pipe task is unfinished. The night/flood clocks continue. Host snapshots include the hold; guest cannot change it. Safe tutorial ship unchanged.

No headset performance or comfort result is inferred from these models.

Rendered Night2 passes: actual fuel outage, protected control grace, supported apparition, engine restart through existing state actions, restored power and retreat.95 focused cases resolved passing (initial94/95, corrected stale relief-cancellation test fixture, all7 Drowned tests pass). All12 workload models pass. Rain/puddle/appearance screenshots reviewed; no shader errors. Non-contact appearance only; hunter and escape tools remain unimplemented.

The first live v29 run passed join/avatars/mono voice setup/both handheld radios/shared guest pipe/ship/shared sleep/leave/rejoin and the shared apparition, but caught storm stage3 instead of4 in blackout. Cause: the legacy beacon-danger boolean only becomes true during a ship window. Drowned weather now reads circuit darkness separately.19 focused Drowned/Tension checks pass, including the real Night2 watch with no ship danger window. Focused live appearance rerun follows.

Focused live correction: PASS. A fresh host starts Night2, guest joins, both receive sabotage/manifestation at storm stage4, both receive power restoration and retreat. Both screenshots personally inspected. See `crew-weather-fixed/` and compact `crew-summary.json`. The earlier regression's successful join/avatars/mono voice setup/both handheld radios/shared guest pipe/ship/shared sleep/leave/rejoin remain recorded under `crew-before-weather-fix/`; its overall failure is retained, not rewritten as a full pass. Editors muted: human speech clarity/echo not measured.

The gallery rain and puddle graphics were inspected. Geometry: 1,600 shader-animated streaks plus12 supported puddles,3,488triangles/two renderers; oilskin apparition692triangles/one renderer. No additional light, reflection camera or depth copy. GPU cost/stereo/comfort pending Quest, currently absent from ADB.

One solo replay initially loaded the older passive-beacon fixture from artifacts/s5/ship/Night3.cs. It completed actual-XRI pipe/generator/radio, then correctly wrecked North Star because that obsolete fixture never aims the wheel. This is a test setup error; no gameplay change was made. The current artifacts/s5/ship-playtest/Night3.cs fixture (physical wheel tracking) is used for the final replay. Original result retained under obsolete-solo-fixture/.

Final soloNight3 actual-XRI replay: PASS, zero errors. Grip-only pipe repair/drop recovery, can pour, primer squeeze, choke detents, three starter pulls, beacon ignition/alignment, radio tuning/code/PTT, physical wheel tracking through both ship passages, lens wiping and sleep intoNight4. Reviewed wheel/complete-board/survived pictures in solo-night3/. This is a functional input regression, not new human timing evidence;12 slower read/action/travel models remain in timing.json.

No fresh full-suite claim; inherited legacy PlayMode failures remain documented in Next.md. No additional failures remain from the focused rules/render/transport pass after the explicit fixture corrections above.
