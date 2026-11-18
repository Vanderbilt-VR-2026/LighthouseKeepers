# Current checkpoint — unfinished-feature delivery

Branch **sprint-5 / PR #89**. Clean implementation source **4828cbbe**. Network **LK-S5-TRAINING1 / LK-Lab-Coop-v47**; both clients must update. Editors muted. No AI voice. Install only, never auto-launch Quest.

## Implemented and pushed this pass

1. **#137 d4acce3f:** fuel + three timed physical starter strokes; no campaign primer/choke. Misses keep progress; host owns timing. 52 focused checks and actual offline XRI stroke fixture pass.
2. **#112 f9f7a70c:** complementary-duty Owen physically travels/works machinery or radio/beam, yields to tools/commands, awaits explicit ship clearance, retires when another human joins. Actual navigation/station fixture passes in 106.8 s (fixture time, not a human completion measurement); 34 focused checks.
3. **d9c3813d:** right-hip walkie carry through existing validated grab leases. Actual offline grip/carry/drop and live two-Editor movement/disconnect release pass; 34 focused checks.
4. **#139 dab1ba19:** beard mouth clearance and skin-fitted wet eye trails. Same accepted body rig/solver; 16 avatar checks and inspected normal/possessed/restored renders.
5. **#140 4828cbbe:** full Night 1 station rehearsal: pipe, fuel/start, beacon, radio clearance, real aiming, sleep. 49 focused checks and actual accelerated instruction route pass. Final two-Editor clearance/pass/rejoin/retry and guest physical timed starter pass. Evidence: `Docs/Verification/S5/Unfinished/`.

Instruction-only timing simulation now completes Nights 1–5 solo/duo, but NOT the Demo. This is not a first-timer measurement or continuous human campaign test. Final review also fixed helper panel overload on the 2.5-unit grid: nine focused checks plus actual stable shed/reset fixture pass (`Unfinished/Panel/`). No new protocol fields. Final corrected APK packaging is the active step; append the verified delivery receipt before calling it installed.

## Next implementation work

1. **#140/#105/#106: Night 3 bunk → warned hunter → hiding/hold breath → final ship.** Not implemented/enabled. `Docs/Design/Day3-Radio.md` is the design. Existing `HunterState`/`HunterNavigation` are foundations only. Add host-owned scene/hiding state, valid observations, actual escape/navigation tests and reconnect/recovery checks before enabling catches. Protect the second ship during the lesson. Reuse Day 2 presentation; no camera movement or face-contact scares.
2. Demo pacing and missing story beats: current model solo expires; duo becomes incomplete on return-to-bed when fuel exhausts. Do not claim six-minute Demo acceptance. Preserve Nights 1–3 while rebuilding its data/flow.
3. Remaining engine/panel and beacon-bearing information split, later-night scenes/finale, remaining issue checklist. Recordings **#122 LAST**, after guide wording stabilizes. No additional AI voice.

## Reproduce / exact next commands

- Worktree: `cd /Users/aneeshvasamreddy/LighthouseKeepers-M3`; do not change the Desktop checkout.
- Focused checks with the muted, compiled Editor: `python3 artifacts/s5/unfinished/check.py TimedStarterTests CrewCompanionTests CampaignOrderTests RadioBeltTests GrabAuthorityTests HarborKeeperTests CrewHumanoidAvatarTests M3RuntimeTests VoicePassTests`. Per-slice receipts are already saved; rerun only if source changes justify it.
- Actual fixtures: `starter-final.py`, `bot-final.py`, `belt-play.py`, `tutorial-final.py`; two live Editors: `crew-final.py`, `crew-starter-timed.py`, all under `artifacts/s5/unfinished/`. Use fresh result folders. Never refresh/edit Assets while playing.
- Preserve peer overlays before synchronizing; `sync-training.py` has a consumed backup directory, so change it for the next run.
- Build/delivery drivers: `python3 artifacts/s5/unfinished/build-final.py`, then `python3 artifacts/s5/unfinished/deliver-final.py`. Close both Editors first, clean commit, fresh output/backup folder for future builds. Delivery validates APK/package/signature/CRC/hash; it does not launch.

## DEVICE CHECKS PENDING

Follow `Docs/Verification/S5/Unfinished/Headset-Check.md` in order. Full Night 1 teaching, fair timed-starter band (host and guest), both complementary roles, stair routes/tool handoff, right-hip carry/regrab, briefing height, watch/cuff, possession face/revival/follow, Night 3 radio challenge, human co-op voice/shared task/sleep/rejoin/offline, sustained Quest CPU/GPU. None is inferred from muted Editor tests. User is not available now; do not wait or request a session.

Historical details: `Docs/Design/CheckpointHistory/2026-11-17-unfinished.md` (superseded checkpoints, not the active next-work list).
