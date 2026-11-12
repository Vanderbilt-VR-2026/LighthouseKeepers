# Step 1 — solo pipe, crew valve

Solo no longer needs the valve/clamp. It retains the old post-clamp leak fraction, so this removes an instruction rather than speeding up the flood. No clamp control or P9 instruction is shown. The archived cue/serialized enum remains for asset compatibility. Two keepers still need distinct valve/tool actors; host state validates ownership and heartbeat. One keeper after disconnect can immediately continue repair; joining restores the two-person rule without resetting bolt progress.

The valve is 0.43 m beside the active bolt. It follows a newly active bolt only after release so a held handle never jumps. Its whole presentation (including generated labels/guidance) is hidden for solo. The old singleton clamp method is a no-op for callers preserved across the upcoming M3 merge.

Verification: pressure 4/4 and director 8/8; graphics actual-XRI Night 1 repair/drop/recovery/bed to Night 2 passed. The live one→two→one presentation check exposed a cached destroyed primitive collider; fixed by tracking only the real grip collider, and the recheck passed. Screenshot inspected. State replication tests passed; fresh two-Mac transport and full suites are deliberately reserved for Job 1 step 7. No new replicated fields, no version change yet. Device grip reach remains unverified.
