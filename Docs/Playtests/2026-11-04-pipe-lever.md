# Pipe lever redesign — November 4

The lead found wrist-roll repair physically awkward even after progress retention. The wrench fought the intended arm movement, floated after release, and the square bolts and central pipe run were unclear. Narration repeated too often.

Before changes: runtime d383fb7, docs HEAD 475533e. Solo graphics Days 1–3 plus real beacon loss/retry and bed into Day 4 passed with zero errors. Evidence: artifacts/immersion/pipe-redesign/before/solo/.

Live Mac+Quest room FY4C9K (us): join, avatar, Mac-to-Quest chime, host physical pipe repair, shared board and shared sleep into Day 2 passed. The lead confirmed the board and Day 2. Guest repair was blocked by the reported usability problem; do not call that passed. Incoming voice packets were present, but the sampled signal was near silence, so intelligible return speech was not verified. The first automated host grab failed because its hand pose had not published; corrected only the untracked test driver and retried successfully.

Implementation plan: upright bolts on an east-wall pipe; projected hand-position ratchet with free return and retained progress; actual released-wrench physics under existing host authority; bounded mist/smoke; once-per-night radio and idle-only hints; a recording import menu. Preserve hand base rotation, all other stations, avatar/body and water visuals. New behavior has tuning switches. Reuse current tool pose replication; extend only the narration cue contract and bump network version for matching builds.

Acceptance: actual XRI grabs with both hands and multiple approach positions/heights; wrist rotation alone cannot repair; return/release cannot unwind; host release falls and replicates; radio stays quiet while working. Repeat the same solo replay and live co-op check, inspect requested captures, measure effect cost on Quest or mark it unverified.

The lead clarified the snap direction: **keep it under my hand—no jump**. The lever therefore follows the actual projected hand direction immediately rather than rotating to a preset away-facing pose. The closest wrench-face normal is retained to avoid an unnecessary upside-down flip.
