# S4-M1 evidence (2026-11-02)

Runtime/build: **7cc8702**, Unity **6000.3.23f1**. This is an in-progress verification record, not full headset acceptance. Baseline: `coop/basics` 1217866 / runtime 8e3b98a. Main is untouched.

## Regression results

| Run | Before | After | Result |
|---|---|---|---|
| Full EditMode | 218 passed | 219 passed | No lost passes |
| Full PlayMode | 38 passed, 15 failed | 40 passed, 15 failed | No lost passes or new failures |
| Exact graphics solo replay, S4 off | Days 1–3 + bed, zero errors | Same, zero errors | Passed |
| S4 enabled | Not applicable | Final replay passed Days 1–3, loss/retry, four-second nights, zero errors | Passed |
| Mac + Quest | Shared Lab, guest bolts, both bed → Day 2 | S4 lobby avatars and Mac → Quest chime confirmed | Shared S4 day and two-way speech pending |

[Test-by-test comparison](test-comparison.json) lists every pre-existing failure: Bootstrap 1, First Watch 5, old PlayerTraversal 6, Storm Watch 3. The original archived scenes are not part of this Lab build. Failures are reported, not silently removed or repaired out of scope. Full XML/logs are in `artifacts/s4m1/before/` and `artifacts/s4m1/after/`; the exact graphics run is `after/solo/`.

New tests cover bounded smooth lighting, sustained high-water failure/reset, beacon failure/reset, read-only guest snapshots and no Photon Voice in offline solo. They do not prove live network failure/retry.

## Images inspected

Before and after use the same replay and camera position with S4 disabled:

| Before | After |
|---|---|
| ![Before House](before-house.png) | ![After House](after-house.png) |

Final S4 blackout and starter captures are Editor graphics evidence; handheld comfort remains a headset check. The blue starter highlight is the existing interactable feedback. No free-hand or tracked-controller base rotation changed.

| F1 outage | F2 outage | F4 no power |
|---|---|---|
| ![F1](blackout-f1.png) | ![F2](blackout-f2.png) | ![F4](blackout-f4.png) |

| Left hand | Right hand |
|---|---|
| ![Left](starter-left.png) | ![Right](starter-right.png) |
| ![Left side](starter-left-side.png) | ![Right side](starter-right-side.png) |

![Night text](night-survived-editor.png)

![Beacon failure](beacon-failure.png)

![Day 3 complete](solo-day-three-complete.png)

[Final S4 replay result](s4-solo-result.json) and [step evidence](s4-solo-evidence.txt).

These avatar shots are from the **lobby**, not the House. User confirmed hearing the original bell sent through Photon Voice. Quest microphone permission was granted; intelligible return speech has not yet been accepted.

| Quest sees Mac Host | Editor sees Keeper |
|---|---|
| ![Quest avatar](quest-lobby-avatar.png) | ![Editor avatar](editor-lobby-avatar.png) |

## Connection retry

After the NGCAVK voice attempt was interrupted by PLAY SOLO, Quest join attempts to R7RQ4P returned Photon `GameNotFound` (32758), while the host reported an open US room. Exact Quest-entered room text was not captured, so the root cause is not established. A fresh CREW77 room was successfully joined by a second independent Mac Editor using the normal lobby path, then that temporary client left to free the Quest slot. Awaiting the user's new Quest attempt; do not label the issue resolved solely from the Mac test.

## Build and performance limits

[Build identity and SHA-256](build.json). Only one Quest was connected. [250-second solo performance sample](initial-solo-performance.json): median GPU 6.83 ms, p95 7.07 ms, 72 Hz target; approximate render-thread wall time median 4.39 ms, p95 5.12 ms. This sample is from f83507f before the final guide-light visibility/brightness adjustment. Not full CPU cost, not voice overhead, and not a no-hitch claim.

Pending: final Mac+Quest day/night, both live network loss/retry paths, intelligible two-way voice, matched voice-on/muted device timing, day-change hitch and held-prop jitter measurements. A second Quest is not available.

## Repeat

Run Unity Test Runner's complete EditMode and PlayMode suites. For the unchanged baseline, launch the Editor with `LK_S4_DISABLED=1`; for the cloud networking tests set `LK_FUSION_CLOUD_TESTS=1`. Run the same graphics solo replay in each configuration, then use matching S4 builds in region `us` for Mac+Quest. Do not mix the public S3 preview and S4 matchmaking versions.

See [implementation, tuning, original audio sources and teammate install steps](../../OneLighthouse/S4-M1.md).
