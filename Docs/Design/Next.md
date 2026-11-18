# Current checkpoint — Andrew / Aneesh playtest corrections

Branch **sprint-5 / PR #89**. Installed APK: **6f05ad39**, `Builds/LighthouseKeepers-Sprint5-AndrewFixes-6f05ad39.apk`. Quest package/hash verified; **not launched**. Editor muted; no AI voice; install without launch. Do not modify the Desktop checkout.

## Current implementation

Immediate headset-following arrival briefing with Skip; actual direct-hover walkie fix; both lanterns enabled from Night 1 at the House entrance; personal walkie → lantern → wick → wrench objective sequence; radio lever grip/lift at the visible tip; distinct code-request/entry/transmit captions; hand-around-rim beacon input; disable accidental 180° stick turn; Owen relaxed wrist orientation; House orders moved off the radio/board; intense score accent limited to possession and visible horror.

Network **LK-S5-LANTERNSTART1 / LK-Lab-Coop-v48**. Night 1 now has an enabled lantern snapshot (old peers reject that state), so both players need the new build. No extra replicated fields.

Focused checks and a real physics-hover/input fixture are recorded in `Docs/Verification/S5/AndrewFeedback`. Input fixtures disable the hardware HandRig reader while injecting button edges; they do NOT bypass XR hover/select filters. Worn usability and GPU cost remain pending.

## Next, in order

1. Core pass implemented and verified: nightly beam tolerance, modeled flood retune, paired N1 code, Owen role/pose, possession rise sound, freshly baked dim fixtures, lantern cookie and exterior fog, lamp cable drops removed. Evidence in AndrewFeedback/README.md. Final build/transport delivery follows.
2. Northern Star watcher/clown: visible gaze encounter, disappears after five seconds observed, respects scare setting; no camera motion or near-face spawn.
3. Muted live Night 1 and two-Editor transport regression PASSED (including N1 lantern, radio clearance, ship pass, rejoin, retry; zero new runtime exceptions on corrected run). Current-source ARM64 build and Quest install PASSED; not launched. Full headset audio/feel/GPU still pending.
4. Then resume older missing Night 3 warned hunter/hiding lesson and Demo pacing. Neither is accepted or enabled by these playtest fixes. Recordings #122 LAST.

## Exact commands

`cd /Users/aneeshvasamreddy/LighthouseKeepers-M3`

Focused: `python3 artifacts/s5/andrew-feedback/check.py ArrivalBriefingTests RadioStateTests RadioBeltTests BeaconStationTests PlayerSoundtrackTests RadioRelayTests CampaignOrderTests CrewCompanionTests VoicePassTests`

Live scene: `python3 artifacts/s5/andrew-feedback/open-play.py`; run `artifacts/s5/andrew-feedback/InteractionPlay.cs` through the existing Pipeline `run_script` bridge, entry `InteractionPlay.Run`, fresh result path; stop Play Mode before editing Assets. Never start a scene while tests are still running.

Build drivers from `artifacts/s5/unfinished/build-final.py` and `deliver-final.py` need fresh output/backup folder, product suffix and network constants before reuse. Close Editors, clean source, validate ARM64/package/signature/hash. Preserve then restore only documented builder/importer churn.

## DEVICE CHECKS PENDING

Arrival immediate/follows/seated height/Skip; B recall; right/left snap symmetry; actual walkie hover/grip/release/belt; lantern on House hooks, wick/oil/hanging; radio lift/dial/code/hold-trigger sequence; solo both roles and human code relay; beacon outside F4, circular turning/regrip, nightly difficulty; darkness/readability, fog/ship visibility and sustained Quest CPU/GPU; possession audio/eyes/revive; Owen hands/neck/stairs; voice, shared sleep and rejoin. Do not treat an Editor fixture as headset acceptance.

## Delivery / continuation commands for this pass

Current installed build source **6f05ad39** (delivery complete). Build: `python3 artifacts/s5/andrew-feedback/build.py` (one-use backup directory; use a fresh output folder for a second build). Install/validate without launching: `python3 artifacts/s5/andrew-feedback/deliver.py`. Normal package `com.lighthousekeepers.game`; connected Quest serial `2G0YC1ZG32048T`.

Two-Editor script: `python3 artifacts/s5/andrew-feedback/crew.py`; it now explicitly skips each personal briefing after spawn. The peer was copied from the source with its previous overlay backed up to `artifacts/s5/andrew-feedback/peer-backup`. Both Editors muted. Do not copy the peer overlay back to source.

User test plan: `Docs/Playtests/2026-11-18-headset-checklist.md`. Next implementation work is the requested Northern Star watcher, then the older warned Night 3 bunk/hiding lesson and failing Demo pace. More tool use should build on the accepted wrench/lantern/radio interactions; preserve the music, physical repair and heartbeat. Final voice recordings remain last.
