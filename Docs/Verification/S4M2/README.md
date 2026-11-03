# S4-M2 verification

Base: `s4/m1` 529fbe6 (runtime 8098ea4). Work branch `s4/m2`. Unity 6000.3.23f1. No main merge.

Before any gameplay edits, full suites ran with S4 enabled: 220 EditMode passes; 37 PlayMode passes / 18 failures. The 15 archived-scene failures remain. Three additional S4-enabled baseline failures: EnvironmentPresentationTests expects old lighting; WatchInteractionTests expects the shorter old sleep; CoopAppearanceTests receives a coroutine-on-disabled-wrench teardown error. These were observed on the unchanged base, not introduced by M2. The previous S4-disabled suite had 40 passes / 15 failures. [Exact baseline list](before-tests.json).

The unchanged `artifacts/s4m1/S4Replay.cs` passed solo Days 1–3 with zero errors, including physical pipe/starter/radio input, actual beacon deadline/retry and beds through the existing Day 4. [Result](before-solo.json). House and final board screenshots inspected.

Live Mac + Quest, CREW88: guest repaired three bolts (45.28 s); host board updated and flood drained; both normal bed votes produced NIGHT 1 SURVIVED then Day 2; three host XRI starter pulls restored power. User confirmed the shared night and Day 2. [Result](before-coop.json). Both night screenshots and the host board/generator screenshots inspected.

Full logs, XML and captures: `artifacts/s4m2/before/`. Graphics Editor import initially timed out; recovery resumed only the graphics replay, without changing gameplay or rerunning completed suites.

Implementation sequence: grid → test → generator → test → beacon → test. After checks pending.

## Piece 1: power grid

Three EditMode rule tests passed: overload/recovery, generous Day 1 capacity, and read-only replica/invalid-state handling. Graphics PlayMode passed real XRI switch and reset grabs, actual starter pulls, all four consumers, radio refusal during trip, blackout/beacon presentation, safe recovery, reallocation and replica application. [Steps](grid/solo-steps.txt). New day assets live in `Assets/Data/S4M2Days`, outside the original auto-scanned Days folder, preserving the old catalog and its rebuild behavior. Grid and catalog are opt-in through `S4M2Tuning`; Editor off-switch is `LK_S4M2_DISABLED=1`. Two independent Mac clients passed guest overload, unsafe-reset rejection, guest offload/reset, and host reallocation. Both snapshots agreed. Guest panel captures inspected; one host close-up was occluded by the nearby guest avatar, so it is not visual acceptance evidence. [Co-op result](grid/coop-result.json).
