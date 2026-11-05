# Part B evidence

## B1 — single instruction foundation

The host director assigns available jobs using keeper and station positions, splitting jobs where possible. Existing authenticated snapshots carry two actor/step pairs. Guests consume assignments; they do not author them. Network version is **LK-S4-MASTER-B1** / snapshot v10; both players must update.

Grip now auto-seats the wrench when its jaw reaches the active bolt. No trigger is required. The existing arm-pull/righty-tighty rules, free return stroke, retained progress, hand alignment, release physics and recovery remain. The host resolves a guest's head position from its live grab lease, independently of trigger messages.

Current instructions use `Assets/Data/Strings/Resources/KeeperStrings.json`. The board lists jobs; station paragraphs become stencils. The active control gets a warm pulse, completed controls a tilted cap, and idle cues. Guided generators ignore extra primers and remove the hidden choke deadline. Existing disabled-switch mechanics remain available.

Verification: 27 focused checks passed (including four new director/sequence/string tests). Graphics Night 1 passed repair with both hands, held-water/drop recovery, drainage and sleep into Day 2, zero errors. Captures inspected locally in `artifacts/s4-master/part-b/b1-solo/`. The harness still injects obsolete trigger events; they no longer gate snapping. A dedicated no-trigger check and final two-Mac/full-suite regression are required at the Part B boundary. A subsequent visual review replaced free-standing tool cue caps with the existing tool/flange surfaces.

Not yet claimed: headset usability/performance, complete player-string migration, measured 50% text reduction, final nearest-job co-op integration, or B2–B6 completion. Tutorial speech/path cues are completed in B4, not this foundation checkpoint.

## B2 — pressure and status foundation

Host pressure tracks water, beacon darkness and time; snapshots carry peak danger for the near-miss logbook. A wristwatch shows remaining minutes/seconds; a slim HUD strip is visible only above half danger. Physical F1 water marks are ray-mounted to a wall. A ship horn approaches during beacon darkness, with last-two-minute tolls. PA/X/Y status requests use the existing authenticated host command. Danger messages take priority and normal speech has an eight-second quiet gap. Twenty-five single-take TTS placeholders and captions cover warnings, status and subsequent onboarding acknowledgements; original teammate takes are preserved.

29 focused checks PASS, including single-take duration/caption coverage, host pressure replication and invalid peak rejection. Graphics pressure/status check PASS, zero errors; inspected `b2-graphics/hud-water-danger.png`. Network version now **LK-S4-MASTER-B2** / v11. Device readability/audio balance and the F4 ember treatment remain unverified; the status response currently speaks the most urgent channel rather than a composite three-channel sentence.

## B3 — shared storm clock

One host-owned stage (1–4) selects the worse of remaining time and danger, with a 20-second one-stage respite after task completion. Rain density/beds, gust volume, weather-event cadence, music, gentle lamp motion and ocean swell consume it. The original ocean shader gains a small bounded wave-height input; the skybox and avatar code remain untouched. Thunder stays delayed behind broad lightning envelopes. Network **LK-S4-MASTER-B3** / v12.

30 focused checks PASS. Graphics pressure/storm integration PASS, zero errors. The existing storm ocean is deliberately still a coarse cheap surface; C4 owns its visual replacement. Ceiling dust/window rattle detail and all device performance remain unverified. The HUD font was reduced slightly after screenshot inspection showed the grip instruction clipping at its previous size.

B4: focused checks 31 PASS; actual grip-only XRI Nights 1–2 replay PASS, zero errors (`b4-days12-final`). Instruction-only rules bot wins all three nights; timing assumptions and limits are in telemetry-firsttimer.md. Night 1 cannot fail; Night 2 outage at 75 s gets 120 s learning grace. Solo Night 3 budget 435 s gives modeled 32.1% task headroom. Protocol LK-S4-MASTER-B4 / snapshot v13.

B5: actual continuous movement now 1.92 m/s (+20%); snap remains default and optional smooth turn persists. F3 bed moved out of the authored route; F3→gallery walking replay passes. Two practical stair-opening pole drops (F4→F3 and F3→F2) pass actual XRI grab, release pause, regrip, landing and bottom rejection. The F2→House route remains stairs: full-body clearance scans found the next flight blocks a safe pole shaft there. No floor or stair collision was removed to force a drop. Device comfort/remote slide appearance remain pending.

B6: House floor-level rock collision disabled; throat floor visual separated 12 mm from House slab; exact duplicate shell renderers disabled; overlapping gallery perimeter collision and a single combined rail mesh added. Actual F3→gallery walking passes at 1.92 m/s. Focused 31/31 pass; rules bot includes return-to-bed and sleep on all three nights. Final solo Night 3 budget 450 s gives modeled 104.8 s spare / 30.4% headroom including bed approach. Guest X/Y status now uses an authenticated host command; network LK-S4-MASTER-B6 / snapshot v14. Full-suite comparison, live two-Mac replay and APK follow this checkpoint. Gallery capsule sweep regression added; exhaustive arbitrary-edge walking and all-scene coplanar scans are not claimed.

## Final readability and regression corrections

Current protocol is **LK-S4-MASTER-B6 / v14**. Earlier sections record incremental checkpoints, not the final build. The full initial final-suite run passed 270 EditMode tests. It exposed two obsolete PlayMode expectations: requiring trigger-lock and assuming Night 2 immediately starts powerless. Updated tests retain physical interaction assertions, now require automatic grip snap/free-spin return and advance to the scheduled outage. Four focused PlayMode checks pass, including the previously failing F3 route and new gallery safety check. Full comparison and two-Mac results follow below.

The wrench keeps its steel base material after pickup; only its arrival cue pulses emission. Bunk slate mounts relative to the headboard, waterline marks have physical horizontal stripes, extra guided primer squeezes are silent. Watch Menu/audio-setting captions now use the strings table. Controlled Day 2 fixed-text comparison is 244→121 words (50.4%); this does not claim every legacy dynamic string is migrated. Graphics Nights 1–3 pass; a subsequent Night 1 readability replay passes with zero errors. House arrival and grip-only bolt screenshots were inspected.
