# Scheduling after the integrated merge

- 69 focused EditMode cases pass. This includes the accepted three-night Day 3/director/voice rules, campaign schedules, invalid replicas, current-roster valve behavior, and real Night 2 fuel exhaustion.
- All 12 instruction-only models pass: Nights 1–5 plus Demo, solo and two workers. Travel/action assumptions remain explicit; not live co-op or human-first-timer timing. Full traces are in timing.json and the table is in Docs/Playtests/telemetry-m3.md.
- Night 2 now has a data-authored FuelExhaustion event. Fuel consumption drives the gauge and sputter; the event observes the stopped engine instead of switching off a still-fuelled engine. Refuelling restores the normal consumption rate. The same empty-tank state and grace are replicated.
- Solo clamp metadata removed. Both keeper count and authored valve variant determine co-op roles, including retry and roster changes.
- Existing Night 1–3 catalogs remain unchanged. No duration/flood retuning in this pass; final ship-inclusive pacing is pending Part B. Preview campaign remains enabled as previously requested; timingApproved remains false.
- Actual-XRI graphics replay passes Nights 1–3 through the bed transition into Night 4, zero logged errors. Fuel/primer/choke/starter, all bolts/drop/drain, circuit changes, beacon and radio verified. Harness waits for authored breathing pauses instead of old Day3Stage ordinals. Inspected the generator and completed Night 3 board captures. Its 235-second spare is a fast functional-input result, not first-timer pacing.
- No M3 headset execution/performance claim. Human co-op acceptance and the full campaign transport regression remain pending; the current pass has replica rules tests plus PR #85’s live two-Mac baseline.
