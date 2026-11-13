# Fear foundation — step 1.3 / #93

Implementation and automated regression pass; headset acceptance remains open.

- Final full EditMode, including menu bounds and voice compile: **416/416** (`editmode-final.json`).
- Focused PlayMode: **19/23**, exactly the same four failures as the pre-quality baseline. No new failures in this set. The retired full-suite Bootstrap hang remains separately documented; this is not a full PlayMode pass.
- Actual-XRI solo Nights1–3 complete and shared state wakes into Night4. See `solo-loop-evidence.txt`. The aggregate JSON says false because its appended diagnostic selected existing spatial station effects along with the new 2D fear sources. A narrow follow-up checks the three actual fear sources: **passes, zero errors**, with finite bounded mono audio and a saved haptics-toggle roundtrip. Do not rewrite the original failed JSON.
- Inspected Night2 board and F1 blackout captures, plus host/guest avatars. Initial menu screenshot exposed the new toggle outside the backing; reflow is implemented and a bounds test added. Updated menu image was captured and inspected: toggle and Back fit inside the page. Its final probe timed out later when the Mac slept again; the raw timeout is retained, not relabelled as a full-run pass.
- Live fast-start/late-join found a real Photon Voice race. SessionInfo.IsValid briefly becomes false during launch while the transport remains connected. The fix uses the live channel's stable room name. In the corrected attempt both Voice clients reached Joined, avatars and mono/AEC were present, and fear snapshots were flowing. The later uninterrupted final run passes radios/shared sleep/rejoin; see `crew.json` and `crew-events.jsonl`.
- The Mac slept during two later attempts (one overnight; another two-hour timestamp gap), despite `caffeinate -i`. At9% battery it cannot provide a reliable live room. Interrupted logs are preserved under artifacts/s5/fear. Resume with power connected and lid open; do not tune gameplay around wall-clock test timeouts.
- Both Editors remained muted. No human voice/audibility or Quest performance/comfort claim. No ship, Drowned or hiding feature is implied by synthetic threat inputs.

Protocol **LK-S5-FEAR1 / LK-Lab-Coop-v24**. Existing public quality-preview APK is v22 and is incompatible with this source. Build this commit for both clients.

## Android build

Exact source `859f8b0`, normal package `com.lighthousekeepers.game`, IL2CPP ARM64. `Builds/LighthouseKeepers-Sprint5-Foundation-859f8b0.apk`. SHA-256 `1ace925be0aa22b44756e5512cc625956a061cf44e98818c445b6f5d7c09184c`. Build succeeded; see build metadata/verification. No Quest detected, so no install or launch. Both players need v24. This is a foundation development build, not a final Sprint5 release.

## Uninterrupted transport result (November 13)

`crew.json` and `valve.json` both pass. The normal lobby starts a host alone, a guest joins the running watch, both avatars render, Voice rooms join with mono/AEC/NS, cross-floor proximity and both handheld routes work, mute stops transmission, the guest repairs the pipe, both sleep into Night2, and leave/rejoin preserves the watch. The separate Night3 run uses the host XRI valve and guest XRI wrench and checks the host-authored per-keeper fear on different floors. The menu toggle and its bounds are verified; inspected images are included. No human audibility or device timing claim.

An earlier uninterrupted attempt exposed a test-placement bug: the capsule remained enabled during a scripted teleport after sea recovery. Camera/head stayed at the House while the injected hand moved to the pipe. `test-placement-before.txt` retains that failure. The driver now disables the capsule only for the test reposition, synchronizes physics, restores it, and asserts the actual head position; `guest-physical-repair.txt` passes without gameplay changes.

Final screenshot review also caught main-page Close reappearing over Sound settings on live state refresh. WatchMenu now keeps its main buttons hidden until Back. The new regression and rendered follow-up pass; no synced fields changed. The `859f8b0` APK predates this menu-only correction; next APK must include it.
