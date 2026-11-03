# S4-M2 verification

Latest APK runtime: **eab3980** (Unity 6000.3.23f1), built from a clean checkout, installed and launched on the connected Quest. Work branch `s4/m2`, stacked on `s4/m1` / PR #76; draft PR #77. Main and the Sprint 2/Sprint 3 releases are untouched.

APK: `Builds/LighthouseKeepers-S4M2.apk`. SHA-256: `4720719a49a4fa765f78bcda284313b40aebff6d47053ed1ec76fa0a0f533583`. [Build record](starter-fix/apk-build.json), [signature/CRC/VR manifest checks](starter-fix/apk-verification.json), [installation](starter-fix/installed.json). Launch logs show OpenXR reaching FOCUSED with no detected Unity null/missing-reference or shader exceptions. This proves launch, not headset acceptance of the correction.

## WORKS

- Before edits, accepted S4-M1 solo Days 1–3 and the live Mac+Quest baseline passed: guest three-bolt repair, shared bed/night into Day 2 and host generator restoration. The user confirmed the shared night. [Before solo](before-solo.json), [before co-op](before-coop.json).
- On the final runtime, both the unchanged M2-disabled solo replay and the M2-enabled replay pass with zero errors: Days 1–3, physical pipe/starter/radio inputs, actual beacon-dark deadline and retry, and beds into the existing Day 4. [Original result](starter-fix/original-solo.json), [M2 steps](starter-fix/m2-solo-steps.txt). The original-view screenshot comparisons differ only by isolated rendering rounding. [Comparison](starter-fix/image-comparison.json).
- Full final suites: **233/233 EditMode; 39/56 PlayMode**. Every baseline pass is retained. The 17 remaining failures were already failures before M2; the existing intermittent avatar teardown case passed this run. The new starter visibility/pull regression passes. [Final EditMode](starter-fix/editmode.json), [final PlayMode and exact failures](starter-fix/playmode.json), [before suites](before-tests.json). No unrelated failing tests were changed.
- Grid, generator and beacon were implemented and tested in that order, with separate graphics and two-independent-Mac guest-action checks before proceeding. [Grid co-op](grid/coop-result.json), [generator co-op](generator/coop.json), [beacon co-op](after/beacon-coop.json). The final starter correction also passes guest primer/choke/pull/open-choke and leased-can pouring into the host simulation. [Latest generator co-op](starter-fix/coop.json).
- The user confirmed Day 1's House, hands, wrench and pipe worked on candidate 6a7ebcf. They then found the Day 2 starter was not visible. The new panel occluded it in 12/27 sampled standing views. The correction moves the existing interaction forward, exposes a contrasting grip and recoil cord, and labels it. Visibility is now 27/27; full pull and spring-back are tested. Base hand rotation, per-hand anchors and 0.30 m travel remain unchanged. [Correction and before/after evidence](starter-fix/README.md).

## NOT VERIFIED

Final eab3980 headset acceptance, the after-build Mac+Quest shared day, and two physical Quests remain unverified. The user became unavailable before co-op; room MGZZT9 was closed and no successful after-build Mac+Quest join is claimed. First-timer 45–75 s generator timing, new-control comfort/readability/haptics and sustained Quest frame cost need a wearer check. Previously deferred wall transparency and wrench-grip polish remain deferred.

## Implemented behavior and reuse

One host-owned grid drives pump, beacon, radio and lights. Day 1 can supply every circuit; later days fit two large loads plus lights. Overload latches the breaker: shed load, then RESET. Engine stop/restart cannot clear a trip. Guest actions use the existing authenticated crew-command checks; fuel can and rag use the existing first-grab ownership lease, with pour position/tilt and rubbing measured on the host.

The generator retains the accepted starter gesture and adds fuel, three primer squeezes, choke closure, three pulls and reopening within five seconds. Wrong order coughs; five closed-choke attempts flood it for eight seconds. Fuel runs down at authored day rates, including when a guest continues alone after disconnect. The beacon requires power, ignition and alignment; its lens physically rotates, its beam visibly sweeps, and soot dims it. Only rubbing removes soot; stationary cloth/jitter does not.

Reuse: Eli Gripenstraw's flood/technical work, William's Fusion crew transport and replication, Tapan's interaction foundations, and the accepted S4-M1 hands, starter attachment, sound assets, voice, bed and Watch Menu. Original scene and hand assets are unchanged. New day assets are outside the original auto-scanned Days folder, preserving the original catalog and authoring behavior.

## Screenshots inspected

[Breaker overload](grid/overload.png) · [board power line](after/day-three-board.png) · [fuel pour](starter-fix/guest-fuel-pour.png) · [aligned beacon](after/beacon-aligned.png) · [outside sweep](after/beacon-outside-sweep.png) · [guest soot wipe](after/guest-wipe.png) · [latest generator running](starter-fix/guest-starter.png) · [starter at standing reach](starter-fix/after-standing.png) · [starter at close reach](starter-fix/after-close.png).

The loss screen [capture](after/beacon-loss-board.png) uses an advanced host clock for framing; both full solo replays separately waited the actual deadline. One initial grid host capture was occluded by an avatar, so the unobstructed guest capture is used. An initial final-pair setup hit an Editor assembly reload; restarting Play Mode after compilation passed without a gameplay workaround. These discarded attempts are retained in local artifacts.

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

## Reproduce

Unity 6000.3.23f1. Run the full EditMode and PlayMode suites with S4 enabled and `LK_S4M2_DISABLED=1` for the unchanged regression comparison; the new visibility test explicitly enables M2 then restores that setting. Graphics drivers are saved locally under `artifacts/s4m2/`: `starter_replays.py`, `M2Replay.cs`, `StarterView.cs`, `starter_pair.py`. Complete logs/XML/captures are under `artifacts/s4m2/before`, `after` and `starter-fix`. Build with `LighthouseKeepers.Net.Editor.MultiplayerBuild.BuildQuest`.

For the next wearer check: PLAY SOLO Day 1 first; then Day 2, grip the primer bulb and squeeze trigger three times, close the choke, grip the labelled centre STARTER and pull toward you three times, then reopen the choke within five seconds. Test the breaker. For co-op use a fresh crew code and the same APK on both clients. [One-page two-Quest setup](../../TwoQuestSetup.md).
