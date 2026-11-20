DAY 2 NIGHTMARE + DAY 3 CURSED SHIP — two scripted horror set pieces, plus fixes to the run controls and
the flare gun. Read this all first. Make the exact implementation decisions yourself, but meet every
rule and acceptance check here. Rule zero: don't break anything that works. Keep the VR rules: never move
or rotate the player's camera, no scares inside the face (stop at least 0.5 m away), no strobing (flashes
below 3 Hz), and start each scare when the player is looking toward it. Host owns all scare state; both
players see the same thing at the same moment. Bump the network version and report it.

====================================================================================================
PART 0 — CONTROL + WEAPON FIXES (do these first; everything below depends on them)
====================================================================================================
A. RUN = HOLD A
- Remove arm-swing running completely (it doesn't work reliably).
- Hold A to run. Stamina: about 4–5 s of running, refills in about 3 s when released. The keeper's
  breathing gets harder as stamina drains. Below 15%, a short gasp sound and a light haptic pulse.
- A is now only RUN. Move hold-breath to X. Update every prompt, tutorial, watch help page and icon.

B. FLARE GUN MUST VISIBLY AND RELIABLY HIT (I shot it and couldn't see the shot hit anything)
- Visible projectile: a bright red magnesium flare round with a smoke/spark trail, fast enough to feel
  like a gun but slow enough to follow with your eyes (decide the speed; roughly 40–60 m/s) with a slight
  drop. It must be clearly visible at night in the storm from both players' views.
- Reliable hits: swept/continuous collision (no tunnelling through thin colliders or fast targets). The
  projectile ignores the shooter's own hands, body and the gun. The target's hit collider is somewhat
  larger than its body so close shots never miss by a pixel.
- Clear feedback: muzzle flash + loud crack + strong haptic kick in the firing hand. On a HIT: a burst of
  red fire on the target, a scream, and the target burns and collapses (see Part 2). On a MISS: the flare
  visibly lands and keeps burning where it struck (wall, floor, sea), so the player can see exactly what
  happened.
- The flare can briefly light its surroundings (one temporary realtime light while in flight, no shadows;
  log it as a documented exception to the Quest light budget in QuestQuality.md).
- Networking: the host decides hits; both players see the same flight path and impact.
- Tests: PlayMode tests for hits at 2, 5, 10 and 20 m against a still and a moving target, shots from
  either hand, a shot from the client, and a guaranteed miss. Screenshot sequence of a shot in flight and
  of a hit; look at them yourself.

====================================================================================================
PART 1 — END OF DAY 2: THE NIGHTMARE "CAN YOU COME WITH ME?" (teaches revive)
====================================================================================================
1. WAKE-UP: players go to sleep after Day 2. Instead of morning, they wake in the middle of the night:
   lights off, storm loud, radio silent, the watch says 3:00 AM. At the top of the stairs stands a small
   child in soaked 1940s clothes, dripping. She says quietly: "Can you come with me? I'm scared." She
   walks slowly down the stairs, stopping to look back. HUD: "Follow her."
2. THE FOLLOW (about 30–45 s of slow dread): she stays just at the edge of the lantern light, a few steps
   ahead, and waits if they stop. Her footsteps make no sound; each floor's lamps go out as she passes;
   small wet handprints on the railing. She stops in the dark generator room, facing the corner.
3. THE TURN: when the keepers get close, she whispers "You're not him." Her head turns too far. She climbs
   the wall onto the ceiling and crawls toward them upside down, joints bending wrong, knuckles cracking.
   Lights burst. She drops behind them. The stairs down to Floor 1 are the way out.
4. THE CHASE (difficult): HUD in red: "RUN — hold A". Goal for both: get OUTSIDE through the Floor 1
   door. Stepping into the storm breaks the nightmare.
   - She's slower than a running keeper and faster than a walking one, and she cuts corners by crawling
     on walls and ceilings. Running out of stamina is dangerous.
   - Doors slam shut on the way and must be grabbed open; lights die floor by floor; the exit door is
     stiff and takes a real pull.
   - She targets whoever is behind. A background time limit (about 45–60 s) makes her faster the longer
     it lasts.
5. CAUGHT: her face lunges in close (stops short of the face), dark red screen, muffled underwater sound.
   That keeper is down. Their partner sees the body lift off the floor and float, mouth stretched too
   wide, eyes bleeding. Caught player's HUD: "Your partner must escape and come back for you."
   - One caught → the survivor's HUD: "GET OUTSIDE". If they make it in time, the nightmare ends.
   - Both caught, or the survivor doesn't get out in time → night failed, retry from the wake-up.
   - Solo: get out in time or retry from the wake-up.
6. BACK INSIDE (the revive): outside, everything snaps back to normal. HUD: "Go back in. Find your
   partner." Inside it's normal too (lights on, generator humming, no child, no water), except the
   partner floating, possessed, where they were caught. Grab their hand within about 30–40 s (HUD:
   "Grab their hand") → the body drops, the partner gasps awake. Out of time → night failed. The real
   harbor master afterwards, with static: "Keepers? You've been silent for ten minutes. What happened
   up there?"
This is where revive is taught; every later catch uses the same revive.

====================================================================================================
PART 2 — DAY 3: "DON'T TRUST THE RADIO" + THE CURSED SHIP
====================================================================================================
DAY 3 MECHANIC — FAKE RADIO CALLS: the real harbor master (Navy port office) now checks every order with
the night's challenge word and password, and his voice always has radio static. The fake is perfectly
clean, slightly too close, and gives plausible wartime orders ("U-boat off the point, kill your light").
The radio gets a CHALLENGE control: real = correct password; fake = wrong answer, the question repeated
back, or breathing. Obeying a fake has a real cost. Keep this light: one or two fake calls in the night.

THE CURSED SHIP (one of tonight's convoy ships; scripted, not the players' fault)
1. THE FIGURE: as this ship comes into view and a keeper looks at it, a tall, thin figure is standing on
   deck, perfectly still, staring straight at the lighthouse with glowing blood-red eyes. Its head slowly
   follows whichever keeper is closest to the beacon. The eyes must be clearly readable at that
   distance (small emissive points with a soft glow; no bloom post-processing).
   RANDOM SPAWN: pick a random spot each time the night is played, from at least 6 points on the ship
   (e.g. bow, bridge wing, stern rail, lifeboat, mast ladder, funnel). Never the same spot twice in a row.
   Each must be visible from the beacon.
2. BLOOD RAIN: while players stare at it, the rain turns dark red for several seconds: red streaks on the
   windows, red splashes on the gallery, red-tinted lightning (a single flash, no strobe). Then it fades
   back to normal rain.
3. THE WRECK: the ship veers onto the rocks no matter what the players do. A grinding crash, then a huge
   explosion (fireball, burning debris, fire on the water; the boom arrives a moment after the flash
   because of the distance). The ship breaks and sinks, burning. The harbor master makes it clear this
   wasn't the keepers' fault. It doesn't count as a ship they lost.
4. THE TELEPORT: the figure is gone from the burning ship. 2 s of near-silence. Then it's standing right
   beside a keeper at the beacon (about 1.5–2.5 m away, beside or behind them, never blocking the
   stairs, never inside geometry). Red eyes, close. Then it lunges. HUD in red: "RUN — hold A", then
   "GET THE FLARE GUN". The harbor master: "The flare gun! It's the only thing that'll stop it!"
5. THE CHASE — RELENTLESS: it always knows where the keepers are. Hiding doesn't work (it rips doors
   open), the lantern doesn't stop it, and it never gives up or searches. It's about as fast as a running
   keeper, so stamina decides everything. It can blink a short distance when out of sight, always with a
   warning sound first and never landing in front of a keeper's escape path. Keepers must get to the
   flare gun (decide the best place for this chase: the existing House bench for a desperate run down
   the tower, or somewhere closer; make it hard but possible). Doors slam, lights fail.
6. GRABBED = DOWN: if it grabs a keeper, that keeper is down/possessed (same as the nightmare).
   - Co-op: the other keeper must still get the flare and stop it, then revive their partner in time. If
     both are grabbed → night failed, retry the whole night.
   - Solo: grabbed → night failed, retry the whole night.
7. THE FLARE — ONE SHOT: one flare round. HIT → it takes the flare in the chest, burns red, screams, and
   collapses into seawater and ash; the storm drops a stage; the downed partner can be revived. MISS →
   the flare burns where it landed, the figure stops, looks at the keeper, and walks forward slowly
   → night failed, retry the whole night (make the "you missed" moment clear).
8. AFTER: the night continues to its normal end (remaining tasks + ships). At bedtime the fake voice,
   clean and calm: "Goodnight, boys. Tomorrow you meet the water." (sets up Day 4).

====================================================================================================
BUILD + TEST
====================================================================================================
- Build both set pieces with one reusable scare kit: door lock/slam, lights out/burst, blood
  rain/bleeding walls, fake voice, possession (float + bleeding eyes), lunge jump scare, chase AI,
  teleport/blink, HUD alerts. Put every number (speeds, timers, stamina, spawn points, distances) in
  the HorrorTuning asset.
- Dev menu buttons: "Start nightmare", "Start cursed ship", "Start relentless chase", "Give flare gun".
- Tests: both players escape; one caught + survivor escapes + revive; both caught = fail; survivor too
  slow = fail; cursed-ship figure appears at different spots across 10 runs; flare hit stops the chase;
  flare miss fails the night; grabbed in solo fails the night; chase never blocks the only path; no
  pathing through walls. Screenshots of every key moment (child on the stairs, child on the ceiling, the
  figure on the deck, blood rain, the explosion, the figure beside the beacon, the flare hit); look at
  them yourself.
- Audio: TTS placeholders for any new lines (child, fake harbor master, real harbor master); list them in
  the voice-line catalogue so I can record them later.
- Install on the Quest (don't launch). Report: what works, network version, and a 6-step headset test
  that uses the dev buttons. Commit + push per step, open/close GitHub issues with evidence.
