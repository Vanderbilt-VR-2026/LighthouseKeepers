# Pipe lever redesign — November 4

The lead found wrist-roll repair physically awkward even after progress retention. The wrench fought the intended arm movement, floated after release, and the square bolts and central pipe run were unclear. Narration repeated too often.

Before changes: runtime d383fb7, docs HEAD 475533e. Solo graphics Days 1–3 plus real beacon loss/retry and bed into Day 4 passed with zero errors. Evidence: artifacts/immersion/pipe-redesign/before/solo/.

Live Mac+Quest room FY4C9K (us): join, avatar, Mac-to-Quest chime, host physical pipe repair, shared board and shared sleep into Day 2 passed. The lead confirmed the board and Day 2. Guest repair was blocked by the reported usability problem; do not call that passed. Incoming voice packets were present, but the sampled signal was near silence, so intelligible return speech was not verified. The first automated host grab failed because its hand pose had not published; corrected only the untracked test driver and retried successfully.

Implementation plan: upright bolts on an east-wall pipe; projected hand-position ratchet with free return and retained progress; actual released-wrench physics under existing host authority; bounded mist/smoke; once-per-night radio and idle-only hints; a recording import menu. Preserve hand base rotation, all other stations, avatar/body and water visuals. New behavior has tuning switches. Reuse current tool pose replication; extend only the narration cue contract and bump network version for matching builds.

Acceptance: actual XRI grabs with both hands and multiple approach positions/heights; wrist rotation alone cannot repair; return/release cannot unwind; host release falls and replicates; radio stays quiet while working. Repeat the same solo replay and live co-op check, inspect requested captures, measure effect cost on Quest or mark it unverified.

The lead clarified the snap direction: **keep it under my hand—no jump**. The lever therefore follows the actual projected hand direction immediately rather than rotating to a preset away-facing pose. The closest wrench-face normal is retained to avoid an unnecessary upside-down flip.

## Follow-up on installed 61df219

The lead reports the arm motion still fights the hand, markings face the wrong way, and the pipe is visibly offset from the wall. This is a failed headset usability check despite passing injected Editor paths. They explicitly request **grip to hold, trigger to lock, release trigger to readjust, then lock again**, preserving all tightening. Asked which motion to keep, they chose **short arm pull around the bolt**. No wrist-roll mode.

Actual Lab shell raycasts (not the lobby) put the east inner wall at x=3.90 at z=0, x=3.80 at z=±0.88 and x=3.677 at z=±1.30. The previous pipe x=3.05 and guessed plates were too far into the room. Mount the segmented run and brackets from actual shell hits. Turn top markings toward the centre and make the pressure dial upright facing the approach.

Source 61df219 before this correction: focused interaction/release tests 7/7; solo Days 1–3 loss/retry through Day 4 passed. Two-Mac guest pipe, host generator, shared sleep through Day 3 and guest fuel passed; a screenshot serialization error interrupted the driver after guest repair, then the remaining run resumed successfully. Host released wrench at (3.276662,0.004997,-0.799122) was dynamic; guest was kinematic at the identical replicated position. Live Mac+Quest acceptance after the redesign is still outstanding.

The clutch correction uses three 30-degree clicks per bolt, with partial clicks saved across trigger release. The instruction plate reads “GRIP: HOLD / TRIGGER: LOCK / SHORT PULL · RELEASE TRIGGER · RESET.” The active bolt remains under the hand's projected direction; the tracked hand visual is not forced to a socket pose. Network version is LK-S4-M3-PULL2 (snapshot v5), because the host now requires a fresh trigger-clutch command from the wrench lease owner.

8096655 installed successfully: device input logs show all three Day 1 bolts completed and sleep into Day 2. Comfort verdict is still pending. The first PULL2 two-Mac test found guest trigger commands rejected by the older CoopCommandWindow maximum (BeaconAlign). Correct the validator to include WrenchClutch, test every declared action plus replay/stale-day rejection, and use PULL3 matchmaking so the rejected-command build cannot silently host a newer client. Preserve solo behavior.
