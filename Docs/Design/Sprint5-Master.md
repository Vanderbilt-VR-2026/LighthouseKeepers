# LIGHTHOUSE KEEPERS — SPRINT 5 MASTER BRIEF (final sprint)

You are the game director, gameplay engineer, technical artist and sound designer finishing Lighthouse
Keepers: a co-op (1–2 players, solo always supported) VR storm-horror survival game for Meta Quest 3
(Unity 6000.3.23f1, URP, OpenXR, XRI 3.x, Photon Fusion + Photon Voice). This brief is the single
source of truth for Sprint 5. Read it fully before starting and re-read the relevant section before each
phase. Where it conflicts with older docs, this brief wins. Where it is silent, follow the PILLARS.

---------------------------------------------------------------------------------------------------
## 0. HOW TO WORK (non-negotiable)

- AUTONOMOUS MODE: implement all phases back to back. Usage is not a concern. Don't pause for headset
  checks — log every headset-only check in `Docs/Design/Next.md` → "DEVICE CHECKS PENDING". Stop only to
  ask ONE clear question when something truly needs a decision or an asset only I can get (e.g. Mixamo
  downloads); keep working on other steps meanwhile.
- BRANCH: `sprint-5` (draft PR #89; renamed during grading cleanup). If Sprint 4 PR #88 has been merged into main, merge main into it first.
- COMMIT + PUSH after every numbered step, with a clear message. Keep `Next.md` current (done / next /
  exact commands) so "continue" always resumes cleanly.
- RULE ZERO: nothing that works may break — lobby (PLAY → HOST / PLAY ALONE / JOIN CREW, offline
  fallback), tan hands, harbor master voice + word-by-word subtitles, B recall + objective height,
  pipe repair, physical generator, power panel, watch, wiring, co-op (join, avatars, proximity voice,
  radios, valve, shared sleep, rejoin), five-night scheduling. Baseline before Phase 1
  (`artifacts/s5/before/`), re-verify at the end of every phase. Fix regressions before moving on.
- NETWORK: the host owns all game state (faults, entity, ship, lantern oil, fear inputs). Sync through
  the existing system. Any new synced field → bump the lobby NETWORK VERSION and report it.
- OWNERSHIP: William Qian owns the SKYBOX and the AVATAR BODY — don't edit them; blend with them.
- EDITOR: keep `EditorUtility.audioMasterMute = true` during every Play Mode/test run. Install builds on
  the Quest at the end of each phase if `adb devices` shows it; never launch unless asked.
- EVIDENCE: for every visual/feel change, take Play Mode screenshots from the player's eye height and
  LOOK at them yourself; fix what looks wrong before reporting.
- LICENSING: record every external asset (name, URL, author, licence) in `Docs/Design/AssetSources.md`.
  Allowed: CC0, CC-BY (with attribution in credits), Unity Asset Store free (Standard EULA), Mixamo
  (Adobe), Sonniss GDC bundles (royalty-free), Pixabay content licence, ElevenLabs output from my paid
  plan. NOT allowed: anything "non-commercial" (CC-BY-NC), BBC Sound Effects, ripped game/film audio.
  This game may win a cash prize, so licences matter.

---------------------------------------------------------------------------------------------------
## 1. PILLARS (every decision must serve these)

1. **ALWAYS KNOW WHAT TO DO.** One instruction at a time. Objective = object + place + action
   ("Wipe the lens — lantern room, Floor 4"). The harbor master says WHAT happened and WHY.
2. **ALWAYS FEEL HUNTED.** The storm and the Drowned are the antagonists; the lighthouse is the battlefield.
   It's a horror game you play by keeping a lighthouse alive — not a repair simulator.
3. **THE WORLD TALKS FIRST.** Sound → light → physical evidence → haptics → voice → HUD → whiteboard.
   No floating panels. Only the objective line, subtitles and the wrist watch are UI.
4. **EVERY ACTION FEELS PHYSICAL.** Grab, pull, twist, squeeze, tilt. Every interaction has ≥4
   feedback channels (sound + visual + haptic + world reaction).
5. **TENSE BUT FAIR.** Telegraph every threat. Always an escape. Lose for being slow or careless —
   never for being confused.
6. **CLAUSTROPHOBIC, NOT NAUSEATING.** Darkness, closeness, sound and heartbeat create pressure. No
   vignette, no forced camera motion, no head bob, no strobe faster than 3 Hz.
7. **QUEST-FIRST.** 72 fps locked (13.9 ms frame budget). GPU ≤ 11 ms, CPU main ≤ 9 ms with the ship +
   the Drowned + storm peak in view. Measure on device. Follow the professor's Quest 3 quality baseline
   in §14 — it is a grading requirement.

---------------------------------------------------------------------------------------------------
## 2. THE STORY + CORE DESIGN (write `Docs/Design/Sprint5.md` from this first)

Something comes out of the sea in the storm: **THE DROWNED**. It wants the light out so ships wreck and
the sea takes them — and the keepers. It is the **reason things break**.

**Sabotage (the cause of every fault):**
| Sabotage | Pre-sound (2–5 s before, spatial at the location) | Break | Evidence left | Fix |
|---|---|---|---|---|
| Tears a pipe joint | wet heavy steps below, metal groan | burst + hiss, spray | wet footprints to the sea door, claw scrapes on the pipe | wrench the bolts |
| Drains / fouls fuel | slow wet dragging, a gurgle | generator coughs, dies → blackout | oily handprints on the tank, fuel puddle | refuel + restart |
| Yanks a cable | something heavy on the floor above, a snap + spark | circuit dies (one system) | torn cable end swinging, sparks, scrapes on the wall | reconnect the cable ends at the junction box (grab both ends, join; spark + click) |
| Smears the beacon lens | whispers at the top of the tower | beam dims to ~30% | black wet handprints on the glass | wipe with the rag (rubbing) |
| Rips the radio wire | static cuts out, a wet thud | radio dead | dripping wet trail, torn wire | reconnect |

Evidence decals fade 20 s after the fix. Footprints always point to WHERE IT WENT (a door, the stairs,
the sea), so players can track it — and fear it.

**Light vs dark (core rule):**
- Darkness makes it bolder: it only HUNTS when a floor is dark (blackout, broken lamp circuit).
- Light pushes it back: lit rooms, the shining beacon, and the LANTERN beam.
- Every LOST SHIP makes it stronger next night (faster, more sabotage).

**The night loop:** calm → it sabotages something (you hear it) → you go fix it, often in the dark,
knowing it might still be there → light returns, it retreats → a ship needs your beacon → it attacks the
light → climax → relief → dawn.

---------------------------------------------------------------------------------------------------
## 3. THE TENSION DIRECTOR (one brain for storm, horror, music, fear)

Unify the existing StormDirector, music system, fault scheduling and the entity into one host-owned
`TensionDirector` with a single tension value 0–1 and a rhythm: CALM → DREAD → SABOTAGE → HUNT → RELIEF.
It reads the night data and decides WHEN things happen; systems decide HOW.

Hard rules:
- Never two new problems within 15 s (except the Night 5 climax).
- A 10–20 s breather after every fix (storm drops one stage, music resolves, no new sabotage).
- Never a chase during a first-time tutorial step; never a jump scare in the first 60 s of a night.
- Always an escape: sprint + a hiding spot reachable within ~10 s from every station (automated check).
- Before every big scare: 2–3 s of near-silence (music and rain dip) — silence makes the scare land.

Outputs it drives together: storm stage (rain/wind/lightning rate/wave height/lamp sway), music layer,
entity state, interior fog density, lightning timing, harbor master priority, and the FEAR value (§6).

---------------------------------------------------------------------------------------------------
## 4. THE FIVE NIGHTS (data assets; tune from measured times)

| Night | Name | New idea | Obstacles | Ships | Drowned |
|---|---|---|---|---|---|
| 1 | The Leak | learn the lighthouse | pipe torn (footprints to the sea door); no fail timer | 1 preview, passes on its own | none (one glimpse on the rocks at dawn) |
| 2 | Blackout | generator + LANTERN | fuel drained → blackout; lantern taught in the dark | 1 real | apparitions only (2–3) |
| 3 | The Warning | radio + beacon + hiding | torn cable + radio warning; first HUNT in the blackout ("Hide, keeper!") | 2 | 1 hunt |
| 4 | Everything Breaks | juggling + fighting back | several sabotages; lens smeared during a ship window; breaker overload choice; flare gun appears | 2 close together | 1–2 chases |
| 5 | The Long Night | everything | storm peak; every system attacked; final large ship = climax | 3 incl. final | chases + climax |

Timing targets (first-timer bot): 25–35% spare time on Nights 1–3, 15–25% on Nights 4–5, Demo Night
reaches the ship moment in ~5 min. Write the table to `Docs/Playtests/telemetry-s5.md`.

Dawn (Night 5 win): storm breaks, grey-gold light floods the windows (lighting/fog tint — not the skybox),
the sea calms, gulls, the Drowned sinks back into the water (seen from the gallery), rescue boat
approaches the dock, P25, short skippable credits with team names + asset credits.

---------------------------------------------------------------------------------------------------
## 5. THE LANTERN (new mechanic — precious, rarely used)

Period-correct hurricane oil lantern (tin + glass globe + wire bail). One per keeper, hanging on a hook
by the House door. Introduced on Night 2 in the first blackout ("Take the lantern, keeper. Oil's dear —
use it when you must.").

- HOLD: grip on the bail handle. It hangs naturally (pendulum swing, slight lag). Light is ON while
  held and the wick is up.
- WICK: twist the knob (thumbstick up/down while holding, or trigger to toggle) → OFF / LOW / HIGH.
  LOW = 2.5 m warm pool, slow oil use; HIGH = 4 m, faster use and used for the stun.
- OIL IS SCARCE: ~90 s of HIGH (≈180 s of LOW) per full tank. The globe's oil level is visible; the
  flame sputters and dims under 20%. ONE small oil can per night (F2 shelf) refills one lantern once —
  tilt it over the filler (same verb as the generator fuel). Make players CHOOSE when to use it.
- HANG IT: hooks at every station (pipe, generator, power panel, radio, beacon). Hanging it there frees
  both hands to work in a blackout — oil still burns. This is the main use.
- STUN: hold HIGH, aim the beam at the Drowned for 2 s → it recoils and retreats ~20 s; costs 15% oil.
- LIGHT RULE: the Drowned won't step into a lantern's HIGH pool for long; LOW pools only slow it.
- TECH: one realtime point light per lantern, NO shadows, range = pool radius, warm (#FFB066),
  subtle flicker (noise on intensity ±8%). Max 2 realtime lights total (one per keeper). Glass globe
  emissive + flame card. Oil level synced (host-owned).
  QUALITY NOTE (§14): the professor's baseline bakes all additional lights. The lantern is the ONE
  documented exception: in the Quest URP Asset set Additional Lights = Per Pixel, Per Object Limit = 2,
  Cast Shadows OFF. Every other light stays Baked/Mixed. If GPU time goes over budget, switch the lantern
  to Per Vertex before cutting anything else. Log this exception in QuestQuality.md.
- WATCH shows oil % as a small bar when holding the lantern.

---------------------------------------------------------------------------------------------------
## 6. CLAUSTROPHOBIA + "YOUR HEART STARTS RACING"

A hidden FEAR value 0–1 (per player, computed on the host from: Drowned distance + line of sight,
darkness at the player, water level near them, active ship danger, being alone). It drives:

1. **Heartbeat** — the player's own heartbeat (2D, low, felt more than heard): 62 bpm at fear 0 →
   140 bpm at fear 1, volume rising with fear. Synced CONTROLLER HAPTICS: a soft double pulse
   (lub-dub) on both controllers per beat, amplitude 0.03 → 0.18, duration 0.04 s.
2. **Breathing** — the keeper's breath (close, 2D): calm → shaky → panicked at high fear; holds breath
   (silence) while hiding when it's close.
3. **Sound closing in** — at fear > 0.6: world sounds slightly low-passed (cutoff 22 kHz → 6 kHz), the
   Drowned's sounds stay crisp (it's the focus); music drops to its HUNTED layer.
4. **Light closing in** — in blackouts interior fog density rises (visibility ~6–8 m), lamps die floor by
   floor, practical light pools shrink; lightning briefly reveals the room (and sometimes a figure).
5. **Space closing in** — during hunts, a door slams shut somewhere nearby (physics, sound); hiding
   spots are tight (you see slats/dark wood inches from your face); low ceilings on the stair landings
   creak and drop dust.
6. **Silence before the scare** (Tension Director rule).
No vignette, no camera shake, no FOV changes, no screen effects that move the camera.
Watch Menu "Heartbeat haptics: On/Off".

---------------------------------------------------------------------------------------------------
## 7. THE DROWNED (horror implementation)

**Forms:** (a) the drowned keeper — tall, soaked oilskin, face hidden, the main hunter; (b) the pale
child — apparitions (stairwell, windows, the bunk), hunter only in Night 5 if time allows.

**Assets (where to get them):**
- Characters: Quaternius (quaternius.com, CC0 — rigged characters with animations), Kenney (CC0),
  Poly Pizza (check each asset's licence). Best animations: Mixamo (mixamo.com, needs my Adobe login —
  if needed, give me an exact list: character + animations "Zombie Walk", "Creeping", "Running",
  "Searching", "Grab"/"Lunge", "Idle Breathing", FBX for Unity, 30 fps, without skin for extra clips —
  and I'll download them to `~/Downloads/drowned-mixamo/`).
- Make them creepy in Unity: dark wet shader (low albedo, high smoothness patches, slight emissive eye
  glints optional), dripping water particles (≤40), trailing mist (≤60), elongated proportions via bone
  scale, tattered oilskin cloth (simple cloth or baked animation). ≤15k tris, 1 material, 1 skinned mesh.

**Behaviour (host-authoritative, NavMesh across all floors + stairs; never through walls; automated
route test so it never gets stuck):**
DORMANT → SABOTAGE (walks to a target, performs the sabotage animation, leaves evidence, exits) →
APPARITION (scripted one-shot scare at a spot) → STALK (wanders toward sounds/dark floors) → CHASE (sees
a player in darkness within 8 m) → SEARCH (last known position, checks hiding spots slowly) → RETREAT
(light, bell, flare, dawn).
- Telegraph every chase: wet breathing grows, nearby lights flicker, music shifts, 2 s before it moves.
- Speed: walk 1.2 m/s, chase 3.4 m/s (player sprint 4.0 m/s), so escape is always possible.
- It targets the player who is ALONE IN THE DARK; the partner can save them (lantern, bell).

**Player tools:**
- SPRINT: click the left stick → +60% speed for 4 s, 6 s cooldown; the keeper's breathing shows
  stamina; comfort-safe.
- HIDE: House wardrobe (hand-opened doors, closes with a latch), under the F3 bunk (crouch or press A to
  "lie down" for seated players), F2 oilskin locker. Hidden + not seen entering = it searches and leaves.
  Peek through slats. Breathing gets louder when it's close; hold-breath option (hold A) silences it.
- STUN: lantern HIGH beam 2 s (§5).
- BELL: brass storm bell on F2 — ring it (grab rope, pull) → it flees the tower for ~45 s. Once per night.
- FLARE GUN (rare, Nights 4–5): locked box on F4 (key on the power panel hook), one flare per night →
  drives it off for the encounter; also lights the sea briefly.
- CAUGHT: screen fades to black (no camera motion), muffled underwater sound; solo → wake on F1 in the
  water, −45 s, dropped items, 2nd catch = night lost ("Taken by the sea"); co-op → downed on F1, partner
  has 40 s to grab your hand to revive (P26), else night lost.

**Scares (apparitions, Night 2+, darkness only):** pale child at the bottom of the dark stairwell (gone
when the lantern reaches it), a face at the F3 window in a lightning flash, wet footsteps on the floor
above when nobody's there, the bunk sheet pulled slowly, a figure on the far rocks only in lightning,
something tapping the glass at F4. Each fires once per night max. Stingers: short, under the loudness
ceiling, preceded by silence.

**Outside ambience (you shouldn't want to be out there):** heavy breathing behind you when outside alone,
shark-fin shapes circling in the swell below the rocks, whispers carried in the wind, waves crashing
over the path, the figure on the rocks.

**Comfort:** Watch Menu "Scare level: Off / Low (apparitions only) / Full"; lobby note "Contains jump
scares and loud sounds". Off removes the Drowned entirely (sabotage still happens "off-screen").

---------------------------------------------------------------------------------------------------
## 8. THE SHIP (the payoff moment)

- Low-poly 1900s coastal steamer, named on the hull (e.g. *Mary Ellen*, *Northern Star*, *Halcyon*,
  *Saint Brendan*, final ship *Providence*). ~8–12k tris, 1–2 materials (Poly Haven / CC0 or built).
  Emissive portholes + mast lights (no realtime lights), slow-turning searchlight (additive fresnel cone),
  funnel smoke (≤150 GPU particles), bow spray at high storm. LOD to a light cluster at distance.
- Path: horizon → close past the rocks below the lighthouse → away. Lights appear 60–90 s before its
  DANGER WINDOW (45–70 s, tuned). Foghorn gets louder and more precisely placed as it approaches.
- The beacon must SHINE through the window (powered + lit + aligned + clean). The Drowned attacks the
  light during windows (lens smear / cable yank) — the climax of each night.
- PASS: horn of thanks, music swells and resolves, storm eases a stage, P23, chalkboard "SHIPS ✓".
  WRECK: grinding crash far below, a red flare rises, P24, the night is lost; logbook names the ship.
- Beacon-dark loss ONLY applies while a ship is in or approaching its window.
- From the F4 gallery the ship's lights slide past right below — make it huge: storm swell, lightning
  on cue, searchlight sweeps across the tower.

---------------------------------------------------------------------------------------------------
## 9. THE 30-SECOND LOOP (perfect this — it's what players feel every minute)

Every station follows: **HEAR it → FIND it → UNDERSTAND it → DO it → FEEL the payoff**.
1. HEAR (0–3 s): spatial pre-sound + break sound at the location; harbor master says what + where.
2. FIND (3–15 s): the objective names place + object; light/sound lead there (flickering bulb above
   the fault, hiss/spark audible through floors); evidence trail.
3. UNDERSTAND (≤2 s at the station): only the next control glows; the brass plate/stencil shows the
   step; the controller-button icon appears near the objective.
4. DO (3–10 s): one physical verb per step with resistance and clear progress (bolt turns, needle
   moves, cord extends). Grab response < 50 ms. Hand pose fits every grabbable (no clipping).
5. PAYOFF (0–2 s): the problem visibly stops (spray dies, engine roars, beam brightens), a satisfying
   sound (clunk/roar/hum), haptic thump (0.10 s @ 0.6), lights/wires react, harbor master "Good." /
   "That'll do it.", storm/music breathe out.
Write `Docs/Design/FeelChecklist.md`: a table of EVERY interaction × the 5 beats, each with its sound,
visual, haptic, world reaction, voice. Fill gaps until every cell is done.

---------------------------------------------------------------------------------------------------
## 10. CO-OP: SPLIT INFORMATION (2 players is the default)

| Night | One keeper reads… | The other does… | Solo |
|---|---|---|---|
| 2 | fuel/primer gauge on the POWER PANEL (F2 wall) | works the generator | gauge on the generator |
| 3 | broadcast code on a telegram slip (House desk) | enters it on the F4 radio | slip pinned by the radio |
| 4 | ship bearing on the radio console dial | aligns the beacon lens on the gallery | bearing plate by the wheel |
| 5 | all of the above + the valve | | |
The objective shows each player their half ("Read the code to your partner" / "Ask your partner for the
code"). Proximity voice + the handheld radios carry it. Host-owned, synced.

---------------------------------------------------------------------------------------------------
## 11. ART DIRECTION (consistent look)

- Mood: a weathered 1900s lighthouse in a killing storm. Warm pools of lamplight, deep blue-black
  shadows, cold storm light at windows, salt, rust, damp.
- Palette: lamp warm #E8A85C, lantern #FFB066, shadow #0B1018, storm teal-grey #3C5560, emergency red
  #8A1F1F (≤15% intensity, slow pulse), lightning #D8E6FF, dawn #E9D7B0, wet sheen highlights.
- Materials: CC0 photoscanned PBR from Poly Haven (polyhaven.com) and ambientCG (ambientcg.com): painted
  plaster/brick with damp, worn wood planks, rusty painted iron, brass, wet stone, oilskin fabric, glass.
  1K textures (2K only for the main tower wall), ASTC 6x6 colour / 4x4–5x5 normal, mipmaps, packed
  metallic/AO/smoothness masks, URP Lit/Simple Lit, no parallax, few shared materials, GPU instancing.
- Props (CC0): Poly Haven models, Quaternius, Kenney; anything custom via Blender (Blender MCP) —
  low-poly with good normals. Needed: hurricane lanterns, oil can, wardrobe, oilskin lockers, bunk with
  sheets, brass storm bell + rope, flare gun + locked box, telegram slips, rag, cable ends + junction
  boxes, crates, rope coils, coat hooks, the previous keeper's logbook, a cracked mirror.
- Decals: footprints (wet), handprints (black wet), claw scrapes, salt stains, water lines, soot — one
  atlas, mesh decals.
- Lighting: baked (Mixed/Baked only except the two lantern lights), light probes on every floor and
  stair, baked reflection probes per room, powered + blackout states, lightning via emissive/ambient/
  probe boost. URP fog: exponential, interior density 0.02 powered → 0.06 blackout; exterior 0.035.
- Post: classic URP post-processing OFF. Tonemapping + colour grading only through Unity 6.3's
  "On Tile Post Processing" renderer feature (never its vignette). No bloom/DOF/SSAO/SSR.
- MSAA, render scale, foveation, shadows, textures: exactly as §14.

---------------------------------------------------------------------------------------------------
## 12. SOUND DESIGN (it's half the horror)

Write `Docs/Design/SoundMap.md`: EVERY event → sound(s), source, 2D/3D, bus, volume, priority.

**Sources:** Sonniss GDC Game Audio Bundles (sonniss.com/gameaudiogdc — royalty-free, commercial OK),
Freesound.org (CC0 or CC-BY only, attribute CC-BY), Pixabay sound effects, Kenney audio (CC0),
ElevenLabs Sound Effects from my account (give me a list of exact prompts + filenames and I'll
generate them), or synthesize. No NC, no BBC.

**Needed (at minimum):** storm beds (inside/outside/roof — already have), wind gusts, thunder (near/far),
rain impacts on glass/metal, waves on rocks; Drowned: wet heavy footsteps (wood/metal), dragging,
breathing (near/far), whispers, child humming, a wet slap, a low growl/gurgle, sabotage sounds (metal
tear, cable snap + spark, fuel gurgle, glass smear); jump-scare stingers (3–4 variants, short); player:
heartbeat (variable tempo), breathing (calm/shaky/panic), hold-breath, sprint footsteps; lantern: lift,
wick turn, flame flutter, sputter, oil pour; hiding: wardrobe creak/latch, locker clang, bunk creak;
bell ring; flare gun shot + flare hiss; doors: creak, latch, slam; ship: distant + near foghorn, engine
chug, hull creak, horn of thanks, wreck crash, distress flare; dawn: calm surf, gulls, rescue-boat motor;
logbook page turn; UI: none (diegetic only).

**Mix:** bus priority VoiceChat > Radio > Danger/Entity > SFX_Critical > SFX_World/Ambience > Music.
Duck music −7 dB and ambience −3 dB under voice/radio. Entity sounds always cut through at high fear.
Master limiter −1 dBTP. Keep dynamics (quiet moments quiet). Measure loudness of a 2-minute Night 3 capture.

**New harbor master lines:** write a script table (ID, filename, text, context, delivery) in
`Docs/Audio/voice-lines-s5.md` like `voice-lines.md` for: each sabotage explanation ("Something's torn
the pipe open — follow the water!", "It's been at the fuel!", "A cable's down on Floor 2!", "The light's
fouled — there's muck on the glass!", "Radio's cut!"), lantern intro + low-oil warning, first hunt
("Hide, keeper! Somewhere it can't see you!"), hiding coaching ("Don't move. Don't breathe."), the bell
("Ring the storm bell!"), flare gun ("The flare box, keeper — top floor!"), being caught, revive, Night 1
dawn glimpse, dawn rescue variant. Use TTS placeholders until I record them with the same pipeline.

---------------------------------------------------------------------------------------------------
## 13. PHASES (do in order; commit + push per step)

**PHASE 1 — FOUNDATION:** 1.0 [#90](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/90) QUEST QUALITY BASELINE FIRST (§14): audit every setting, apply the
baseline via an editor menu, add QualityBaselineTests, measure on device, write QuestQuality.md — so
everything after is built and measured against the right settings. 1.1 [#91](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/91) Sprint5.md + FeelChecklist.md + SoundMap.md skeletons. 1.2 [#92](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/92) TensionDirector
(unify storm/music/faults/fear). 1.3 [#93](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/93) Fear system + heartbeat audio/haptics + breathing (§6).
**PHASE 2 — THE SHIP (§8).** 2.1 [#94](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/94) ship asset + path + lights. 2.2 [#95](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/95) danger window + pass/wreck + voice.
2.3 [#96](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/96) beacon-dark-only-during-windows rule. 2.4 [#97](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/97) make the gallery moment huge.
**PHASE 3 — SABOTAGE (§2).** 3.1 [#98](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/98) sabotage events + pre-sounds + evidence decals for every fault.
3.2 [#99](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/99) new fixes: cable reconnect, lens wipe with handprints, fuel drained. 3.3 [#100](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/100) harbor master explanations.
**PHASE 4 — LANTERN (§5).** 4.1 [#101](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/101) lantern + wick + oil + hooks. 4.2 [#102](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/102) oil can + watch bar. 4.3 [#103](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/103) Night 2 teach.
**PHASE 5 — THE DROWNED (§7).** 5.1 [#104](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/104) assets + apparitions + outside ambience. 5.2 [#105](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/105) NavMesh + stalk/chase/
search/retreat + sprint. 5.3 [#106](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/106) hiding spots. 5.4 [#107](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/107) stun/bell/flare + caught/revive. 5.5 [#108](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/108) Scare level option.
**PHASE 6 — THE FIVE NIGHTS (§4).** 6.1 [#109](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/109) night data + Tension Director schedules. 6.2 [#110](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/110) timing pass with bots.
6.3 [#111](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/111) Night 5 climax + dawn rescue + credits.
**PHASE 7 — CO-OP SPLIT INFO (§10).** 7.1 [#112](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/112) gauge / telegram / bearing. 7.2 [#113](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/113) entity targets the lone keeper.
**PHASE 8 — PROGRESSION + DETAILS.** 8.1 [#114](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/114) morning logbook report card (ships by name, peak water, close
calls, encounters survived, keeper's grade). 8.2 [#115](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/115) continue from Night N + night select. 8.3 [#116](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/116) DEMO NIGHT
(6 min: pipe sabotage → blackout + lantern + restart → one apparition → one short chase ending in a hiding
spot → one ship passing). 8.4 [#117](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/117) hand-operated doors (hinge, latch, synced). 8.5 [#118](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/118) one cracked House mirror
(Quest-cheap planar/RT within 2 m) showing Will's avatar.
**PHASE 9 — 30-SECOND LOOP + AMBIENCE POLISH (§9, §11, §12).** 9.1 [#119](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/119) fill every FeelChecklist cell.
9.2 [#120](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/120) art pass on new props/decals. 9.3 [#121](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/121) full sound pass + mix + loudness check. 9.4 [#122](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/122) new voice-line script.
9.5 [#123](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/123) fix every NOT VERIFIED/known issue in Next.md; z-fighting/clipping sweep; text readability.
9.6 [#127](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/127) QUALITY RE-AUDIT (§14): re-run the baseline audit (new props/materials/lights may have drifted —
mipmaps, ASTC, aniso, static flags, probes, light modes), re-bake lighting, re-measure with OVR Metrics
Tool at storm peak, use the §14.9 diagnosis table on anything over budget.
**PHASE 10 — VERIFY + SHIP.** Tests: no new failures; first-timer bot wins Nights 1–5 solo + Demo;
two-player bot wins Nights 3 and 5; every sabotage + fix; ship pass + wreck; hunt/hide/stun/bell/flare/
caught/revive; lantern oil; Scare Off; save/continue; Wi-Fi-off offline; QualityBaselineTests pass.
Device: OVR Metrics Tool + Profiler attached to a development build — fps, CPU/GPU ms with ship +
Drowned + storm peak in view; 72 fps; Render Graph Viewer shows no unexpected intermediate textures.
Final QuestQuality.md table filled in (every §14 setting, its value, where it lives, measured cost). Screenshots you've looked at for every key moment. Build, install,
push, update PR #89 (plain-English description + network version). FINAL REPORT: WORKS / NOT VERIFIED per
phase, timing table, network version, assets I still need to provide, voice lines to record, and a
20-step headset playtest checklist (Night 1 start → first sabotage → Night 2 blackout + lantern → first
hunt + hiding → a ship passing from the gallery → a wreck → Demo Night).

---------------------------------------------------------------------------------------------------
## 14. QUEST 3 QUALITY SETTINGS (professor's field manual, Unity 6.3 LTS + URP + OpenXR — REQUIRED)

Source: the course deck "Quality settings for Meta Quest 3" (Unity 6000.3.23f1, URP, OpenXR + XRI,
standalone). It's graded, so follow it exactly. Where this brief needs an exception (the lantern
lights), document it. Record every setting + value + where it lives + measured cost in
`Docs/Design/QuestQuality.md`.

**14.0 The budget.** Every setting spends the same frame: 13.9 ms at 72 Hz (11.1 ms @ 90, 8.3 ms @ 120).
We ship at 72 Hz. Two eyes, one mobile tile-based GPU. MSAA is cheap on it. Anything that forces an
intermediate full-screen texture is expensive: HDR, classic post, Depth/Opaque Texture copies.

**14.1 Find the URP Asset the Quest build REALLY uses (do this first).**
Edit > Project Settings > Quality → the Android/Meta column: the green tick is the default level.
Select that level and read its Render Pipeline Asset. Edit THAT asset, not a look-alike (the VR
template's Quest default is Low → "Performance URP Config"; ours may differ). Remove or point to the
same asset any other levels a Quest could select, so there's no ambiguity. Log the asset name.

**14.2 URP Asset (Inspector)** — the Quest one from 14.1:
| Setting | Value |
|---|---|
| Rendering > Depth Texture | OFF (unless a shader truly needs it — list it if so) |
| Rendering > Opaque Texture | OFF |
| Quality > HDR | OFF (the Quest display is 8-bit) |
| Quality > Anti Aliasing (MSAA) | 4x (drop to 2x only if measured GPU-bound) |
| Quality > Render Scale | 1.0 baseline (= 1680×1760 per eye). The panel is 2064×2208, so ~1.23 matches it at +51% pixels. Raise toward 1.2 ONLY if the OVR Metrics GPU time at storm peak leaves ≥ 2 ms of headroom. Never go below 1.0 (small text breaks, and store review can fail). Set it once; never change it at runtime. |
| Lighting > Main Light | per-pixel. Shadows: 1 cascade, Max Distance ≤ 15 m (shorten distance before raising resolution), Shadow Resolution 1024; soft shadows only if measured affordable. If the scene has no realtime main-light shadows, turn them off. |
| Lighting > Additional Lights | baseline Disabled (bake them). OUR EXCEPTION: Per Pixel, Per Object Limit 2, no shadows — only for the two lanterns (§5). |

**14.3 Universal Renderer Data** (URP Asset > Rendering > Renderer List):
- Rendering Path: Forward (never Deferred: G-buffer loads are slow on tile GPUs).
- Depth Priming: Disabled (the pre-pass is paid twice in stereo).
- Post-processing (integrated): OFF.
- Renderer Features: "On Tile Post Processing" (new in 6.3) for tonemapping + colour grading only. No
  vignette (comfort pillar). No SSAO. Remove any other features we don't need (e.g. old droplet/lens
  features if superseded; if kept, prove they add no intermediate texture in the Render Graph Viewer).

**14.4 OpenXR** (Project Settings > XR Plug-in Management > OpenXR — the ANDROID/META tab, not desktop):
- Render Mode: Multi-view (both eyes in one set of draw calls).
- Latency Optimization: Prioritize Input Polling.
- Depth Submission Mode: None (we don't use depth occlusion).
- Offscreen Rendering Only: ON (per Meta's recommendation in the deck).
- Meta Quest Support (gear): check Symmetric Projection and the other Quest options; enable what
  measurably helps and log it.
- Features: tick "Foveated Rendering" (it ships unticked). If the package shows "Foveated Rendering API",
  choose SRP Foveation (OpenXR 1.17.1 may not show it).

**14.5 Foveated rendering** — the feature tick does nothing alone. At startup, get the
XRDisplaySubsystem (SubsystemManager.GetSubsystems) and set `displays[0].foveatedRenderingLevel` (0 = off,
1 = strongest; Quest maps it to discrete levels). Start at a medium level (~0.5–0.75), check text and the
edges of the lens. You can't preview it in the Editor — log it as a device check. Reuse our existing
FFR code if it already does this; make sure it actually runs in the build.

**14.6 Resolution control.** Use the URP Render Scale (set once) only. Do NOT use
XRSettings.renderViewportScale or the OpenXR "Automatic viewport dynamic resolution" feature: viewport
scaling can't be combined with fixed foveation when post is on, and we want both FFR and on-tile grading.

**14.7 Lighting** (Window > Rendering > Lighting) — "many lights for the cost of none":
1. Mark all scenery Static (Contribute GI): walls, floors, stairs, fixed props.
2. Lights → Baked (or Mixed where a moving object needs a realtime shadow). Wall lamps, the beacon room
   and the powered/blackout states are baked variants (as in §11).
3. Light Probe Groups on every floor, stair landing and outdoor path; moving things (hands, avatars, the
   Drowned, the ship near the rocks, lanterns' meshes) sample probes.
4. Generate Lighting (Progressive GPU). Re-bake after moving static objects or lights. Raise lightmap
   resolution only where stepped shadow edges show; keep the atlas count and size low; log the bake
   stats (lightmap count, MB, time).

**14.8 Textures** (Texture Import Settings > Advanced):
- Generate Mipmaps ON for every 3D texture (no mipmaps = shimmer/crawl with every head movement). UI and
  subtitle font atlases follow their own rules.
- Android override: ASTC (6x6 colour, 4x4–5x5 normal, as §11).
- Aniso Level 4–8 on floors, stairs, decks, walls seen at grazing angles; 1–2 elsewhere. Quality >
  Anisotropic Textures = Per Texture.
- Shimmer is a texture problem, not an MSAA one — fix it with mips/aniso, not more MSAA.

**14.9 Player:** Project Settings > Player > Other Settings (Android) > Graphics APIs = Vulkan only
(remove OpenGLES3); keep IL2CPP ARM64.

**14.10 "Usually off" check** (all must be OFF): HDR, classic post-processing, Depth Texture, Opaque
Texture, SSAO, Depth Priming, Deferred path.

**14.11 Measure on the headset, never in the Editor.** Tools: OVR Metrics Tool (on-device overlay: fps,
CPU/GPU), Unity Profiler attached to a development build (CPU-bound or GPU-bound?), Frame Debugger
(draw calls and passes), Render Graph Viewer (catch unexpected intermediate textures). THE LOOP: change
one setting → measure → keep or revert. Write every result to QuestQuality.md.

**14.12 Diagnosis — symptom → check first:**
| You see | Check first |
|---|---|
| Jagged edges on geometry | MSAA |
| Everything soft, small text unreadable | Render scale; foveation level |
| Floors and textures shimmer | Mipmaps + anisotropic filtering |
| Blocky or swimming shadows | Shadow distance, then resolution |
| Flat, lifeless lighting | Bake lightmaps; add light probes |
| Dropped frames, GPU-bound | Render scale, MSAA, shadows, post-processing (then the lantern lights → per-vertex) |
| Dropped frames, CPU-bound | Draw calls + batching (SRP Batcher, static batching, GPU instancing); confirm Multi-view |

**14.13 Make it stick (automation):**
- Editor menu "Lighthouse Keepers/Apply Quest Quality Baseline" sets everything above (14.1–14.9) in one
  click and logs what changed.
- EditMode `QualityBaselineTests` fail if any of these drift: Quest quality level → expected URP Asset,
  HDR off, MSAA 4x (or 2x if documented), render scale within the documented value, Depth/Opaque Texture
  off, Forward path, Depth Priming disabled, integrated post off, no SSAO feature, Additional Lights
  per-pixel limit ≤ 2 with no shadows, main-light shadow distance ≤ 15 m and 1 cascade, OpenXR Android
  Multi-view + Foveated Rendering feature on, Vulkan only, and every texture under Assets/Art and the new
  Sprint 5 asset folders has mipmaps + ASTC on Android.
- QuestQuality.md: before/after table (setting · old · new · where · measured ms) + bake stats + the
  final OVR Metrics numbers at storm peak.
