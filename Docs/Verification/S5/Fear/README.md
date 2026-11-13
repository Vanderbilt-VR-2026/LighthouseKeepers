# Fear foundation — step 1.3 / #93

Implementation is in review; device and final live co-op acceptance remain open.

- Full EditMode before the last menu/voice fix: **414/414**. Final rerun pending at this checkpoint.
- Focused PlayMode: **19/23**, exactly the same four failures as the pre-quality baseline. No new failures in this set. The retired full-suite Bootstrap hang remains separately documented; this is not a full PlayMode pass.
- Actual-XRI solo Nights1–3 complete and shared state wakes into Night4. See `solo-loop-evidence.txt`. The aggregate JSON says false because its appended diagnostic selected existing spatial station effects along with the new 2D fear sources. A narrow follow-up checks the three actual fear sources: **passes, zero errors**, with finite bounded mono audio and a saved haptics-toggle roundtrip. Do not rewrite the original failed JSON.
- Inspected Night2 board and F1 blackout captures, plus host/guest avatars. Initial menu screenshot exposed the new toggle outside the backing; reflow is implemented and a bounds test added. Updated rendered menu check is pending.
- Live fast-start/late-join found a real Photon Voice race. SessionInfo.IsValid briefly becomes false during launch while the transport remains connected. The fix uses the live channel's stable room name. In the corrected attempt both Voice clients reached Joined, avatars and mono/AEC were present, and fear snapshots were flowing. **Final radios/valve/shared-sleep/rejoin acceptance did not finish.**
- The Mac slept during two later attempts (one overnight; another two-hour timestamp gap), despite `caffeinate -i`. At9% battery it cannot provide a reliable live room. Interrupted logs are preserved under artifacts/s5/fear. Resume with power connected and lid open; do not tune gameplay around wall-clock test timeouts.
- Both Editors remained muted. No human voice/audibility or Quest performance/comfort claim. No ship, Drowned or hiding feature is implied by synthetic threat inputs.

Protocol **LK-S5-FEAR1 / LK-Lab-Coop-v24**. Existing public quality-preview APK is v22 and is incompatible with this source. Build this commit for both clients.
