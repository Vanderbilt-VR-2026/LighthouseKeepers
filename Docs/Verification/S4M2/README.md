# S4-M2 verification

Base: `s4/m1` 529fbe6 (runtime 8098ea4). Work branch `s4/m2`. Unity 6000.3.23f1. No main merge.

Before any gameplay edits, full suites ran with S4 enabled: 220 EditMode passes; 37 PlayMode passes / 18 failures. The 15 archived-scene failures remain. Three additional S4-enabled baseline failures: EnvironmentPresentationTests expects old lighting; WatchInteractionTests expects the shorter old sleep; CoopAppearanceTests receives a coroutine-on-disabled-wrench teardown error. These were observed on the unchanged base, not introduced by M2. The previous S4-disabled suite had 40 passes / 15 failures. [Exact baseline list](before-tests.json).

The unchanged `artifacts/s4m1/S4Replay.cs` passed solo Days 1–3 with zero errors, including physical pipe/starter/radio input, actual beacon deadline/retry and beds through the existing Day 4. [Result](before-solo.json). House and final board screenshots inspected.

Live Mac + Quest, CREW88: guest repaired three bolts (45.28 s); host board updated and flood drained; both normal bed votes produced NIGHT 1 SURVIVED then Day 2; three host XRI starter pulls restored power. User confirmed the shared night and Day 2. [Result](before-coop.json). Both night screenshots and the host board/generator screenshots inspected.

Full logs, XML and captures: `artifacts/s4m2/before/`. Graphics Editor import initially timed out; recovery resumed only the graphics replay, without changing gameplay or rerunning completed suites.

Implementation sequence: grid → test → generator → test → beacon → test. All three pieces passed focused solo and two-client tests before proceeding to the next.

## Piece 1: power grid

Three EditMode rule tests passed: overload/recovery, generous Day 1 capacity, and read-only replica/invalid-state handling. Graphics PlayMode passed real XRI switch and reset grabs, actual starter pulls, all four consumers, radio refusal during trip, blackout/beacon presentation, safe recovery, reallocation and replica application. [Steps](grid/solo-steps.txt). New day assets live in `Assets/Data/S4M2Days`, outside the original auto-scanned Days folder, preserving the old catalog and its rebuild behavior. Grid and catalog are opt-in through `S4M2Tuning`; Editor off-switch is `LK_S4M2_DISABLED=1`. Two independent Mac clients passed guest overload, unsafe-reset rejection, guest offload/reset, and host reallocation. Both snapshots agreed. Guest panel captures inspected; one host close-up was occluded by the nearby guest avatar, so it is not visual acceptance evidence. [Co-op result](grid/coop-result.json).

## Piece 2: generator sequence

Four EditMode rule tests passed (wrong order, stall/flood recovery, fuel exhaustion/refill, and read-only/Day 1 behavior). Graphics solo passed actual XRI grabs, trigger squeezes, starter travel and carried-can tilt/proximity; also verified stalled/flooded recovery. Lamp and pour captures inspected. Legacy decorative gauge/cap are hidden only while the new controls are enabled to avoid contradictory readings. The accepted starter attachment is unchanged. Two independent Mac clients passed guest primer/choke/starter/open-choke commands and guest leased-can pouring measured on the host. [Co-op result](generator/coop.json). The guest fuel-pour capture was inspected.

New catalog fuel: Day 1 100% and no drain; Day 2 80% at 0.15%/s; Days 3/4 4% at 0.25%/s. Pour 6%/s, minimum start 12%, warning below 20%. Primer 3; starter 3; flood on pull 5; choke open within 5 s; flood wait 8 s. Beacon allowance in the new catalog is 120/150 s before the existing 0.8 difficulty multiplier (96/120 s effective), allowing the longer generator sequence. First-timer completion time is not yet headset verified.

## Piece 3: beacon station

Three beacon rule tests passed. Graphics solo used XRI ignition/wheel and a carried rag: no-power ignition rejected; power alone did not light an unready beacon; alignment ticked its task; motion cleaned while a stationary rag did not. Actual dark-beacon state caused failure and normal Retry restored the authored day. Alignment/cleaning captures inspected. Outside beam opacity was increased after the first capture was too faint; the two-client test passed guest ignition, alignment, leased rag and host-measured cleaning. [Co-op result](beacon/coop.json). The final outside capture shows the sweeping beam clearly; it was inspected after the opacity adjustment.

Days 1/2 begin lit/aligned/clean. Days 3/4 begin unlit, alignment 0.18 (target 0.5 ±0.025), 30% soot, accumulating 0.1%/s while shining. Cleaning removes 40% per metre of rubbing; fully dirty beam retains 25% brightness. Dark timer requires power, ignition and alignment.

Starter cooldown is 0.25 s, matching the existing network minimum and accepting the previously tested 0.5 s full-stroke cadence. Primer cooldown remains 0.35 s. Full regression compares S4 enabled / M2 disabled to the before run; a separate M2-enabled replay tests the intentionally revised sequence and day data.

## Regression status

The full suites on 3ff9d46, S4 enabled and M2 disabled, finished with **230/230 EditMode** and **38/55 PlayMode**. Every before-pass still passed. The remaining 17 failures match before-failures; the previously failing avatar teardown case passed this run. [Exact after list](after-tests.json). No unrelated failing tests were changed. The final review suite passes **233/233 EditMode**. The fuel-exhaustion test now vents the choke after dry pulls before testing a fueled start, preserving the newly tested five-pull flooding rule. Final graphics/Quest results follow below.

Review commit 6a7ebcf adds actual lens rotation when aligning, restores current-day fuel/soot rates for a guest who continues alone after disconnect, counts unprimed closed-choke pulls toward flooding, avoids resetting a held new prop during the shared night, filters sub-stroke cloth jitter, and labels engine-off power on the board.

## Tuning

| Setting | Accepted S4-M1 | S4-M2 enabled |
| --- | --- | --- |
| Day duration | 315 s effective | 315 s, unchanged |
| Available output | All loads together | Day 1: 3.25; later: 2.5 |
| Circuit demand | One on/off state | Pump / beacon / radio: 1 each; lights: 0.25 |
| Overload | None | Latched trip; shed load, then grip RESET |
| Beacon outage deadline | Day 2: 72 s; Day 3: 60 s | Day 2: 96 s; Days 3/4: 120 s |
| Start | Three pulls | Fuel ≥12%, three primer squeezes, choke closed, three pulls, choke open within 5 s |
| Flooded engine | None | Five closed-choke attempts; 8 s recovery |
| Fuel | No consumption | Day 1: 100%, no drain; Day 2: 80%, 0.15%/s; Days 3/4: 4%, 0.25%/s |
| Pour / warning | None | 6%/s; warning below 20% |
| Beacon readiness | Power only | Power + ignition + alignment |
| Alignment | Automatic | Target 0.5 ±0.025 (±6.75°); Days 3/4 start at 0.18 |
| Soot | None | Days 3/4 start at 30%; +0.1%/s while shining; dirty beam retains 25% brightness |
| Cleaning | None | 40% removed per metre; minimum accumulated rubbing stroke 1.2 cm |

Shared controls/tunables: `Assets/WatchDemo/Resources/S4M2Tuning.asset`. Day-specific settings: `Assets/Data/S4M2Days/Day01–04.asset`. Disable `enabledForWatch` to return to the accepted catalog and mechanics; Editor regression uses `LK_S4M2_DISABLED=1`. The existing S4 tuning still supplies the 0.75 day-duration and 0.8 beacon-deadline multipliers.


## Final build and replay evidence

Runtime APK source: `6a7ebcf15505e33897c3e27432a052c54c8c72c6`, clean build checkout, Unity 6000.3.23f1; build succeeded with zero errors. Package `com.lighthousekeepers.game`, Lab + NetworkLobby, Android signature/CRC/VR manifest checks passed. SHA-256: `a02abd2a183e8361e3cecff053d0d04b111e6890892fe0b8995624e4fd778dab`. [Build record](after/apk-build.json), [APK checks](after/apk-verification.json). Installed on the one connected Quest; launch logs show OpenXR reaching FOCUSED. This is launch evidence, not a completed headset playthrough.

With M2 switched off, the unchanged graphics replay passed Days 1–3, the real beacon-dark loss/retry and beds through Day 4, with zero errors. [Steps](after/original-solo-steps.txt). Day 3 complete board and night frame were pixel-identical to before. The House/Day 1/Day 2 board comparisons differed only in isolated one-level color rounding; the Day 4 pulsing-light frame averaged 0.50/255 channel difference (maximum 4), consistent with capture timing. [Pixel comparison](after/image-comparison.json).

Inspected final captures: [aligned wheel](after/beacon-aligned.png), [outside beam](after/beacon-outside-sweep.png), [fuel pour and generator lamps](after/fuel-pour-day-3.png). The M2-enabled full replay also passed with zero errors: real starter/primer/choke/fuel inputs, Day 3 actual 120 s dark-beacon loss and retry, ignition/alignment, radio/pump reallocation, radio transmission, three-bolt repairs and beds through Day 4. [Steps](after/m2-solo-steps.txt), [final 4/4 board and power line](after/day-three-board.png). The final two-Mac rerun passed guest ignition, actual lens rotation/alignment and host-measured rag cleaning; a stationary cloth did not clean. [Result](after/beacon-coop.json), [steps](after/beacon-coop-steps.txt), [rag capture](after/guest-wipe.png). Quest Day 1 confirmation and the after-build Mac+Quest day remain pending; do not read the earlier baseline acceptance as acceptance of this APK.


The first final two-Mac attempt was discarded: a late Editor assembly reload after the test-fixture edit reset lobby objects during Play Mode. Restarting both Editors’ Play Mode after compilation produced the passing run above; no gameplay workaround was added. The final fuel fixture changes only test setup, so the installed runtime remains exactly 6a7ebcf.

[Beacon loss screen](after/beacon-loss-board.png) was captured after advancing the host clock for framing, then normal Retry succeeded. The full solo replay separately waited the actual 120-second deadline. Its original failure capture was still aimed at the generator, so that frame is not used as visual proof.

WORKS (Editor evidence): original Days 1–3, M2-enabled Days 1–3 and beds, breaker overload/recovery, generator mistakes/refuel/start, beacon alignment/soot, and guest actions for all three new systems. APK builds, installs and starts OpenXR on the connected Quest.

NOT VERIFIED: final APK’s user-played Quest Days 1–3 and Mac+Quest shared day; two physical Quests; 45–75-second first-timer generator target; new grip comfort, haptics, text readability and sustained headset frame cost. The headset screenshot was black while awaiting the wearer; it is retained in local artifacts and is not claimed as visual acceptance. The final full PlayMode repeat on the exact APK runtime finished: **37 passed / 18 failed**, with **zero regressions** and the exact same failure set as before. The avatar teardown case that passed the intermediate run failed again as it did in the baseline; it remains an existing intermittent test failure. [Final full-suite result](after/playmode-final.json).
