# M3 instruction-only timing checkpoint

Measured execution of the real WatchState/ObjectiveDirector rules with an **assumed** first-timer action budget: four seconds to read each instruction, twelve seconds per floor, six seconds per ratchet stroke/reset, twelve seconds for a fuel pour, eighteen seconds tuning and fourteen seconds aligning. Two workers advance concurrently against one host simulation; this is not live networking, physical navigation, human timing or a ship encounter. Return-to-bed travel uses the slower keeper. The start gate means wall time and watch-clock time differ.

`LighthouseKeepers.Editor.M3CampaignBot.Run(path)` runs all five nights plus Demo, solo and duo. It performs only the current instruction. It does not bypass repair, primer, starter, code or transmission completion. Water uses the shipped rates/modifier/valve reduction; grace and retry use actual rules.

| Night | Mode | Modeled time including bed walk | Watch budget | Spare on watch | Result |
|---|---|---:|---:|---:|---|
| night-1 | solo | 148.1 s | 315 s | 206.1 s (65.4%) | Tasks pass |
| night-1 | duo | 148.1 s | 315 s | 206.1 s (65.4%) | Tasks pass |
| night-2 | solo | 226.9 s | 315 s | 127.3 s (40.4%) | Tasks pass |
| night-2 | duo | 226.9 s | 315 s | 127.3 s (40.4%) | Tasks pass |
| night-3 | solo | 420.0 s | 515 s | 134.2 s (26.1%) | Tasks pass |
| night-3 | duo | 408.2 s | 510 s | 135.9 s (26.6%) | Tasks pass |
| night-4 | solo | 461.1 s | 540 s | 118.1 s (21.9%) | Tasks pass |
| night-4 | duo | 449.3 s | 510 s | 94.8 s (18.6%) | Tasks pass |
| night-5 | solo | 623.0 s | 760 s | 176.2 s (23.2%) | Tasks pass |
| night-5 | duo | 612.1 s | 730 s | 152.0 s (20.8%) | Tasks pass |
| demo | solo | 329.2 s | 360 s | 70.0 s (19.4%) | Tasks pass |
| demo | duo | 329.2 s | 360 s | 70.0 s (19.4%) | Tasks pass |

## Changes driven by this model

Only M3 assets change. The accepted three-night catalog and its timers are unchanged.

| M3 profile | Initial solo / crew | Current solo / crew |
|---|---|---|
| Night 3 | 450 / 330 s | 515 / 510 s |
| Night 4 | 450 / 450 s | 540 / 510 s |
| Night 5 | 450 / 450 s | 760 / 730 s |

Nights 4–5 scheduled low-fuel warning leaves 18% (previous prototype 10%); the refill task ends at 65%. Fuel consumption is .0015/s (prototype .0025/s) to fit the longer nights without an unannounced extra restart every four minutes. There is still fuel exhaustion if ignored. The low-fuel event waits for a settled engine rather than interrupting its starting sequence.

The model exposed campaign guidance defects: open the choke before resuming the pipe; do not assign the wrench owner to hold the valve; allow the beacon to be made safe before committing both keepers to the pipe; finish the refill immediately at its threshold before running consumption subtracts fuel again. Focused tests cover the host state. Valve requirements now consume the authored station variant: campaign Nights 1–2/Demo use the clamp; Nights 3–5 use two keepers. Legacy co-op is unchanged.

## Not yet a campaign approval

Every `timingApproved` flag remains false, and M3Campaign is OFF. The model excludes ship windows, final rescue, progression and split-information conversation. Nights 1–2 intentionally retain their inherited budgets until passage pacing is measured. Their large task-only margins are not a claim of meeting final ship-night headroom. After Part B, rerun with ship waiting/repair pressure included, then use graphics and two-client verification. No human-first-timer or headset acceptance is claimed.

Raw per-action trace: `Docs/Verification/M3/scheduling/task-timing.json`. Source runner is committed in `Assets/WatchDemo/Editor/M3CampaignBot.cs`.
