# S4-M1 evidence (2026-11-02)

Rejoin runtime/build: **8098ea4**; live headset evidence below: **7cc8702**, Unity **6000.3.23f1**. This is an in-progress verification record, not full headset acceptance. Baseline: `coop/basics` 1217866 / runtime 8e3b98a. Main is untouched.

## Regression results

| Run | Before | After | Result |
|---|---|---|---|
| Full EditMode | 218 passed | 220 passed | No lost passes |
| Full PlayMode | 38 passed, 15 failed | 40 passed, 15 failed | No lost passes or new failures |
| Exact graphics solo replay, S4 off | Days 1–3 + bed, zero errors | Same, zero errors | Passed |
| S4 enabled | Not applicable | Final replay passed Days 1–3, loss/retry, four-second nights, zero errors | Passed |
| Mac + Quest | Shared Lab, guest bolts, both bed → Day 2 | Guest pipe, shared night → Day 2, host generator; voice both directions confirmed | Passed live |

[Test-by-test comparison](test-comparison.json) lists every pre-existing failure: Bootstrap 1, First Watch 5, old PlayerTraversal 6, Storm Watch 3. The original archived scenes are not part of this Lab build. Failures are reported, not silently removed or repaired out of scope. Full XML/logs are in `artifacts/s4m1/before/` and `artifacts/s4m1/rejoin-regression/`; the exact graphics run is `rejoin-regression/solo/`.

New tests cover vacant-slot rejoin admission, bounded smooth lighting, sustained high-water failure/reset, beacon failure/reset, read-only guest snapshots and no Photon Voice in offline solo. They do not prove live network failure/retry.

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

These avatar shots are from the **lobby**, not the House. User confirmed hearing the original bell sent through Photon Voice. Quest microphone permission was granted. Return speech was subsequently confirmed in the CREW77 House session below.

| Quest sees Mac Host | Editor sees Keeper |
|---|---|
| ![Quest avatar](quest-lobby-avatar.png) | ![Editor avatar](editor-lobby-avatar.png) |

## Connection retry

After the NGCAVK voice attempt was interrupted by PLAY SOLO, Quest join attempts to R7RQ4P returned Photon `GameNotFound` (32758), while the host reported an open US room. Exact Quest-entered room text was not captured, so the root cause is not established. A fresh CREW77 room was successfully joined by a second independent Mac Editor using the normal lobby path, then that temporary client left to free the Quest slot. The Quest subsequently joined CREW77 and completed a shared day. No source change was required; the exact old-room error cause remains unproven.

## Build and performance limits

[Build identity and SHA-256](build.json). Only one Quest was connected. [250-second solo performance sample](initial-solo-performance.json): median GPU 6.83 ms, p95 7.07 ms, 72 Hz target; approximate render-thread wall time median 4.39 ms, p95 5.12 ms. This sample is from f83507f before the final guide-light visibility/brightness adjustment. Not full CPU cost, not voice overhead, and not a no-hitch claim.

Live shared day/night, guest pipe, host generator, high-water failure/retry and voice in both directions are now confirmed below. Remaining: a complete matched voice-on/muted timing window, hitch/jitter acceptance and a second Quest. Beacon network propagation now passed with two Editors as recorded below; its Quest capture remains missing. The first beacon capture was missed when the guest immediately retried; later recapture occurred after the guest left and is not two-client evidence.

## Repeat

Run Unity Test Runner's complete EditMode and PlayMode suites. For the unchanged baseline, launch the Editor with `LK_S4_DISABLED=1`; for the cloud networking tests set `LK_FUSION_CLOUD_TESTS=1`. Run the same graphics solo replay in each configuration, then use matching S4 builds in region `us` for Mac+Quest. Do not mix the public S3 preview and S4 matchmaking versions.

See [implementation, tuning, original audio sources and teammate install steps](../../OneLighthouse/S4-M1.md).


## Final Mac + Quest session: CREW77

User confirmed “Day 2 works; Mac hears me.” Quest heard the Mac’s generated chime; the friend at the Mac heard the Quest speech. Guest repaired the three Day 1 bolts, host board ticked, water drained; both voted in bed, saw the night text, and woke in Day 2. Host pulled the starter three times; Quest logs confirmed power on / one task completed.

| Quest sees Mac Host in House | Editor sees Keeper on F3 |
|---|---|
| ![Mac Host](crew77-house-quest.png) | ![Keeper](crew77-keeper-editor.png) |

| Editor shared night | Quest shared night |
|---|---|
| ![Editor night](shared-night-editor.png) | ![Quest night](shared-night-quest.png) |

| Host receives guest task | Host starts generator |
|---|---|
| ![Guest repair](guest-pipe-complete-editor.png) | ![Generator](host-generator-power-on.png) |

| High-water loss: Editor | High-water loss: Quest |
|---|---|
| ![Flood Editor](high-water-loss-editor.png) | ![Flood Quest](high-water-loss-quest.png) |

The flood setup injected .56 m on the host. The normal three-second sustained-height rule fired, both displayed the loss, and the normal retry reset Day 2 and both failure latches. [Host loss snapshot](high-water-loss-host.json), [retry snapshot](high-water-retry-host.json), [shared night](shared-night.json), [Day 2](both-day-two.json). Screenshots inspected.


### Rejoin issue found during mute test

Quest successfully changed to MIC MUTED, then the user accidentally selected Return to lobby. Rejoining the same room returned GameClosed (32764) because the existing transport closes a running watch and refuses all late joins. Host continued safely with one keeper and a disconnect notice. User requested this be fixed: a vacant slot must accept the same code and load the host’s current Lab state. The muted performance sample was interrupted, so no voice overhead delta is claimed.


## Rejoin fix: 8098ea4

The two-Editor test in room LYHYAW passed **two** cycles of guest Return to lobby → join the same code → current Lab watch. Host controller identity and day revision stayed unchanged; Day 2, running generator, clock, water and tasks synchronized. Each returning guest was read-only, with fresh actor/command state, two avatars and the disconnect notice cleared. Capacity remains two. The `crewRejoin` switch only permits this after the shared Lab has loaded; initial launch remains locked during scene loading.

After the second rejoin, the guest repaired the pipe through real XRI input; both voted in bed and woke on Day 3. Both network clients received high-water and beacon loss, then normal retry cleared each loss and restored Day 3 faults. High water used a .56 m injected setup and the real three-second threshold. The beacon network test advanced the host clock to its deadline; the earlier solo graphics replay exercised the real wait. This separates network propagation evidence from timing evidence.

[Rejoin results](rejoin/result.json) · [Post-rejoin tasks, bed and failures](rejoin/extended-result.json)

| Host preserved Day 2 | Returning guest received Day 2 |
|---|---|
| ![Host](rejoin/rejoined-2-host.png) | ![Guest](rejoin/rejoined-2-guest.png) |

| Returning guest repairs: host | Returning guest repairs: guest |
|---|---|
| ![Host task](rejoin/returning-guest-repair-host.png) | ![Guest task](rejoin/returning-guest-repair-guest.png) |

| Shared night after rejoin: host | Shared night after rejoin: guest |
|---|---|
| ![Host night](rejoin/rejoin-shared-night-host.png) | ![Guest night](rejoin/rejoin-shared-night-guest.png) |

The failure snapshots and screenshots are saved in `rejoin/`. At the bed, the pre-existing world-space bed prompt can overlap the Watch Menu in these views; the failure HUD remains readable. This is a presentation limitation, not a claimed clean screenshot of the entire menu.

### Live performance limits

[Stable-view microphone-on sample](crew-performance.json): F3 view on Quest, 31 VrApi samples, GPU median **10.21 ms**, p95 **10.57 ms**; approximate render-thread wall time median **7.92 ms**, p95 **8.95 ms**. Refresh target 72 Hz; sampled FPS 72–73. Two stale frames occurred. The muted window was interrupted by Return to lobby, so **voice overhead is not isolated**. The broader shared-night/traversal window includes 27.78 ms frames and screenshot capture; a zero-hitch claim is not supported. These are measurements, not inferred from Editor FPS.

Final 8098ea4 regression comparison: **220 EditMode passes; 40 PlayMode passes and the same 15 existing failures**. No lost passes and no new failures. [Exact solo replay result](rejoin-solo-result.json); the after-House image above is from this final unchanged replay.

## Quest rejoin accepted: CREW88, 8098ea4

The user returned to the lobby, entered the same code and joined directly into the running House without another READY round: “Yes, I rejoined the House.” Host controller instance stayed **63556**, day revision stayed **1**, elapsed time advanced from **7.70 to 66.53 seconds**, and water continued rising. A fresh guest actor joined, both avatars were visible, and the disconnect notice cleared. Quest logs show the guest received the running host snapshot rather than restarting the day. Voice reconnected (`MIC ON · Joined`). These captures were inspected.

[Headset rejoin result and limits](quest-rejoin/result.json) · [Quest logs](quest-rejoin/quest-rejoin-log.txt)

| Quest sees Mac Host after rejoin | Mac sees returning Keeper |
|---|---|
| ![Quest](quest-rejoin/quest-rejoined.png) | ![Editor](quest-rejoin/quest-rejoined-editor.png) |
