# Remaining master-pass acceptance checklist

This is a continuation checklist, **not a completion report**. The user's 5 November master-pass request is authoritative. B1 is detailed in `B1-ImplementationPlan.md`. Finish/test/commit/push each numbered step before starting the next; no merges. Preserve offline solo, shared host-owned state, rejoin, accepted hands/tool gestures and audio. Keep new features switchable; new snapshot fields require a lobby protocol bump. Repeat the before baseline at part boundaries; preserve every previously passing test.

## B2 — pressure and status

- Wristwatch: large readable time-to-dawn digits; glancing also recalls objective. Last two minutes: crisis score, distant bell/foghorn each minute.
- F1 painted waterline marks KNEE / GENERATOR LINE; increasing water sound; harbor warnings at 50/75/90% of loss height.
- Dark beacon: darker exterior without changing skybox, approaching horn closer each 10 s, dying ember in lamp glass; voice at 30 s and 10 s remaining.
- Slim icon + fill danger strip only in danger (water/lamp/clock), under the objective.
- X/Y or PA button requests a one-sentence status; automatic about every two minutes if no other speech. Danger preempts lower priority. At least eight seconds between lines unless critical.
- TTS placeholders via existing single-take import/filter pipeline, documented under RadioLines “Status + danger”: “Three hours to dawn”, “Two hours”, “One hour”, “Almost dawn, hold on”; “Water's at your knees, keeper.”, “It's reaching the generator!”, “You're going under, keeper!”; “Your light's out — there are ships out there!”, “Thirty seconds, keeper!”, “Ten seconds!”; acknowledgements “Good.”, “That'll hold.”, “There she goes.”, “Light's burning.”
- Loss: sound specific to water or ship impact, dying lights/storm, fade, one-line logbook reason and retry/lobby choices. Near miss: if danger exceeded 80% during a won night, logbook says “Close call, keeper.” Host owns that history.

## B3 — storm clock

One host-owned StormDirector, one replicated stage. Time/danger choose the worse of Heavy Rain, Gale, Tempest, Peak. Peak in last two minutes/near-loss. Fixing the most urgent fault calms one stage. Rain, gusts, window rattle, thunder, wave height, lamp sway, ceiling dust and music read it. Tempest lightning intervals 15–30 s; comfort-safe brightness/frequency; no skybox changes.

## B4 — onboarding

- Night 1: arrival PA, only wrench guidance glow, HUD “Grab the wrench”; tower door opens after pickup or 30 s. No loss until player reaches the pipe; early danger gives a spoken soft warning.
- Wrench → hiss/pipe → one bolt at a time → drain + acknowledgement → “Get some sleep” → stair lights/F3 bunk lamp → sleep. Never show completion panels.
- Night 2 generator outage after 60–90 s; B1 single-step tutorial. Night 3 introduces radio/beacon, retains staged balance and re-verifies 60–90 s spare solo.
- Voice teaches each verb once; repeat encounters use glow/HUD, voice only after 60 s idle.
- Instruction-only first-timer bot follows the current director/control, wins Nights 1–3 solo. Record actual per-step times including travel in `Docs/Playtests/telemetry-firsttimer.md`; distinguish automation from humans.

## B5 — movement

- Walk speed +20%, smooth stairs, unchanged snap turn; optional smooth turn in Watch Menu.
- Brass fireman's pole with railed/gated floor openings, practical F4→F3→F2→House descent. Engraved “GRIP TO SLIDE”. Either hand, downward only, approximately 2 m/s with eased start/stop, release/bottom stops, no rotation/spikes. Metal squeal + haptic. Existing keeper pose replication must show the slide to the partner.
- Audit/shorten unnecessary routes while preserving clearance and falls protection.

## B6 — fixes and Part B gate

Remove blocking collider/rock under House desk; small decorative props don't collide with player. Diagnose/remove doorway duplicate geometry; scan coplanar overlaps across scenes. Bolts visibly clockwise/sinking; labels remain fixed children. Automated edge walk for floors/stairs/landings/gallery/outside; close ledge holes with colliders/railings and add regressions. Remove remaining completion panels. Baseline replay/full pass-set comparison/two-Mac check, screenshot inspection, APK. Quest-specific acceptance remains pending until available.

## Part C — branch s4/visuals from completed master, separate draft PR

First write 15-line `ArtDirection.md`: mysterious 1900s storm lighthouse, salt/damp whitewashed masonry, worn dark wood, rusty iron/brass, warm practical pools against cold storm windows, haze/half-seen objects. Use concept art; leave Will's skybox/avatar alone. Measure device GPU before/after each C-step when available; otherwise mark NOT VERIFIED, continue Editor builds.

- **C1 walls:** diagnose normals/single-sidedness, occlusion, LOD/bounds, clipping (near 0.03–0.05 m), z-fighting. Scripted 360° sweeps on every floor/stair, zero pops, regression test.
- **C2 materials:** CC0 Poly Haven/ambientCG masonry, wood, rusty paint, brass, wet stone; source/licence links in AssetSources.md. 1K, 2K main walls; ASTC colour 6×6, normals 4×4–5×5; mipmaps, packed metallic/AO/smoothness. URP Lit/Simple Lit, instancing/shared materials, real-world UVs, damp/dirt variation. No parallax. All shell/House walls/floors/stairs/rails/doors.
- **C3 lighting:** baked practical warm light/cold window fill; probes throughout stairs/floors, reflection probe per room; at most one realtime light. Powered and blackout both work, warm restore climbs floors. Lightning via emissive/ambient/probes, never skybox. Exponential cold exterior/thin warm interior/F1 denser fog. Cheap fading god-ray cards/mist, ≤300 dust particles per room, rotating additive fresnel beacon cone, window rain and wet/salt/soot/waterline decals, corner AO. At most tonemap + LUT; no bloom/DOF/SSAO/SSR. MSAA4x + fixed foveation. Dawn window/fog tint shifts toward pale grey-gold, no skybox edit.
- **C4 water ≤2 ms GPU target:** active-bolt scrolling stretched translucent jet, fresnel/no refraction, ≤200 droplets, splash decals/mist/wet darkening; pressure follows every stroke, final drips. Flood optimized plane, two scrolling normals, murky tint, contact foam/ripples, sparse bobbing debris, player-touch ripples and wrench splash, reflection probe; same authoritative height/rules. Ocean 3–4 Gerstner waves tied to storm, dark/whitecaps, wave-peak rock/base spray + sound, distant flattened LOD; match skybox/fog. No screen-space reflection/refraction or tessellation.
- **C5 stations/props:** chipped yellow/oil generator, rusty pipes/red wheels/flanges, brass/bakelite/valve-glow radio, enamel breaker, brass/glass Fresnel. Non-blocking story props: open previous-keeper logbook, oilskin coat, survived-night tally scratches, photograph, dropped lantern, crates/rope/kettle/stove. Keep paths clear.
- **C6 plan:** remaining per-floor set dressing, model sources (Poly Haven/Asset Store/Sketchfab/Blender), exterior rocks/cliffs/island and per-floor budget table in `M2-Plan.md`.

## D — final gate

Full suites without losing baseline passes; instruction bot Nights 1–3; edge walk/wall sweep/version tests. Mac-only co-op now; later Mac+Quest join/avatars/audible voice/shared task/split objective/partner line/pole/shared sleep. Build latest APK; install if ADB connected. Profile each floor/outside/peak storm: 72 fps, GPU≤11 ms, main CPU≤9 ms, ~150 draws/eye, ≤300k visible triangles, ≤24 audio voices, water≤2 ms. Never report device budgets met from Editor results.

Inspect and save screenshots: House arrival (only wrench cue), each generator step/current control, objective+danger, water marks, strong/weak F1 jet/flood, pole-top plate, each floor powered/blackout, beacon/ocean calm/peak, god-ray window, cause-specific lose page. Update both draft PRs; report WORKS / NOT VERIFIED, budgets, network version, recording line list and ten-step first-time-player headset checklist. Then stop.
