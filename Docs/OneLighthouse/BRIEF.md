# Lighthouse Keepers — Single-Engineer Brief (authoritative)

You are the ONLY engineer on Lighthouse Keepers, a co-op VR survival game for Meta Quest 3.
Unity 6000.3.23f1 · URP · OpenXR · XR Interaction Toolkit 3.x · Photon Fusion · Android IL2CPP ARM64.
Repo: this folder. Branch: `sprint3/one-lighthouse` (created from `integrate/sprint-3` @ 682fe40).
Remote: `team` = github.com/Vanderbilt-VR-2026/LighthouseKeepers (authoritative). Never push to `origin`.
You run with no sandbox: full git, network, Unity CLI, adb. Unity Hub is signed in. The lead (Aneesh)
owns environment/level design and tests every APK in a Quest 3.

Re-read this file and `Docs/OneLighthouse/LOG.md` at the start of EVERY turn.

---

## 0. What went wrong before (do not repeat)

Six parallel Codex agents each built a piece in their own worktree. The result:
- Lobby → Play Solo loaded `LK_StormWatch` (old Sprint 1/2 scene) via
  `FusionLobbyTransport.GameplaySceneName = CrewSessionPolicy.StormWatchSceneName` (FusionLobbyState.cs:13).
  The real Sprint 3 work was never attached to what loads.
- Commit 8386dc8 added per-floor "SOLO DAY 1 / DAY 2 / DAY 3 / SOLO RELAY / RETURN TO LOBBY" world
  panels. The lead does not want them.
- The wrench and pipe-repair animation disappeared from what loads.
- Hands were wrong: the lead wants the Sprint 3 hands (tan skin), which he says work well.
- The TunnelingVignette ("goggles" effect) is still in the game. It is created in
  `Assets/_LighthouseKeepers/Scripts/Editor/LighthouseScenes.cs:59-60` (TunnelingVignette prefab under the
  camera, wired into `PlayerComfort`), `ComfortSettings.vignetteEnabled = true` by default
  (`Runtime/Player/ComfortSettings.cs`), and referenced by `HeadBoundaryComfort` (LighthouseCollisionFix.cs).
  Some scripts disable it at runtime, others re-add it. Remove it at the source.
- The blue/cyan beacon and a glowing yellow ceiling cube look wrong.
- macOS duplicate files ("Name 2.meta", "Name 3.png") kept appearing and corrupting builds
  (IL2CPP "same key: mscorlib"). Never commit them; delete them from Library/Bee if a build hits it.

**Rule zero: nothing counts unless it is reachable from Lobby → Play (Solo or Co-op) in the built APK.**

---

## 1. Which lighthouse — corrected by the lead, M1.5

**The base world is `Assets/Scenes/Lab/LK_Lab_Environment.unity` from
`archive/sprint2-final-submission`.** This is the lead's concept-art lighthouse from
`aneesh/environment-shell` (Batch A): House spawn, KEEPER WATCH board, desk wrench, F1 pipe with
automatic tightening, Eli's FloodController/FloodSurface, F3 bed and next day, original rain/storm
sky/ocean, and side stairs. Its game is `Assets/WatchDemo`; read its README. Preserve that loop.
`WatchDemoBuilder.Build` authors the demo and `EnvironmentBuild.BuildAndroid` was its ship builder.

**Original Sprint 2 lobby:** `Assets/Scenes/Lab/LK_Lobby_Environment.unity`.
**Retained current lobby UI:** `Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity`.
This is the latest M1 Fusion lobby with PLAY SOLO and host-alone start; keep its look and route both
modes into the Lab world through one constant. The old environment lobby remains on disk for reference.

`LK_Bootstrap` and `LK_StormWatch` are the old Sprint 1 central-spiral world, not the hand-in game.
Exclude Bootstrap, StormWatch, Core, Exterior and Level01–04 from the build. Delete nothing from disk.
The ship manifest is NetworkLobby + Lab world; add another scene only if a real dependency requires it.

Since the Sprint 2 tag, WatchDemo, Lobby_Environment, Assets/Data/Layout and the LK.Editor /
LK.Presentation Environment scripts were unchanged. Session C changed the Lab scene (+20,855/-2,840).
Before restoring it with `git checkout archive/sprint2-final-submission --
Assets/Scenes/Lab/LK_Lab_Environment.unity`, inspect `archive/sprint3-environment` and log useful
fixtures for later deliberate reuse. Do not re-add those fixtures in M1.5.

**Side stairs already exist.** The stairs milestone is dropped unless the lead asks for it.

---

## 2. Reuse the six-session work. Where it is.

All of it is preserved. Read with `git show <ref>:<path>` / `git diff`. **Port pieces deliberately;
never merge a ref wholesale.**

| Ref | Contains | Use |
|---|---|---|
| current branch (= integrate/sprint-3) | Merged A/B/C/D/E round 1: hands (Assets/Scripts/LK.Gameplay/Interaction, Assets/Prefabs/Hands, Assets/Art/Hands), simulation (LK.Core/Simulation), progression + board (LK.Core/Progression, LK.Presentation/Board), environment fixtures/validators (Assets/Prefabs/Environment/Sprint3), Fusion networking (LK.Net) | Already here. The Sprint 3 **hands are the ones the lead likes**. Make them the hands in the Lab rig, with verified Android materials and grip orientation. |
| `archive/sprint3-simulation` (= branch sprint3/simulation, 92dfdd2) | 3 coupling leaks, 7 watch profiles, drainage-after-success fix, simulation API v2 | Consider for M2 |
| `archive/sprint3-networking` (= branch sprint3/networking, bfa1062) | Round 2 rig binding, ship-manifest routing | Port rig/ownership pieces; ignore its Shell/Dress routing |
| `parked/round2-main-folder` (033ddec) | ShipLensDroplets shader + URP feature, ShipStormPresentation, ShipBeaconPresentation, build-scene guard tests | Port for M4 |
| `archive/sprint2-final-submission` | Assets/WatchDemo (WatchWrenchHandPose, WrenchHaptics, WrenchMetal), LK_GrabWrench prefab, Sprint 2 pipe repair | Authoritative M1.5 world and existing loop; preserve wrench and animation |
| `archive/aneesh-environment-shell` | Sprint 2 side-stair shell | Existing world source; no stairs milestone |
| tapan/* branches (open PRs #48, #54, #55, #56) | Valve, starter lever, radio, beacon interactions | Read; reuse correct bindings |
| william/sprint-2 (PR #57) | Original Fusion 4-player work | Reference for M5 |

Where Sprint 3 code expects anchors (STN_TOOL_TABLE, STN_PIPE_LEAK, STN_MANUAL_PUMP/PIVOT_PumpHandle/
GRIP_PumpHandle, STN_GENERATOR/GRIP_*, STN_BRIEFING_BOARD, STN_BED…), **create those named anchors
inside the Lab scene through an editor script when a later milestone needs them** so the existing bindings work. Don't rewrite
the bindings.

---

## 3. The game (lead's concept art; non-negotiable)

- **HOUSE (Tools & Briefing):** tool bench with wrench and tools on the wall; ONE whiteboard
  "KEEPER WATCH — DAY N" on the wall listing that day's tasks as checkboxes. This is the ONLY task board
  in the game.
- **F1 PLUMBING:** pipes, red valve wheels, three leaking couplings with spray, hand pump, visible water.
- **F2 POWER & RECOVERY:** yellow diesel generator (primer, choke, starter), control panel with lamps,
  manual bilge pump.
- **F3 RADIO & FORECAST:** radio desk, storm chart, bed (sleep ends the day).
- **F4 BEACON:** warm AMBER/gold lantern with rotating beam (never blue/cyan), outdoor gallery with railing.
- **EXTERIOR:** night, heavy rain, wind, crashing sea, gentle lightning. Rain droplets on the camera lens
  only when the head is outdoors.
- **Loop:** FAULT → DIAGNOSE → REPAIR / RESTORE POWER → WATER FALLS + BEACON ON → NEXT WATCH.
  Power runs pump and beacon. Repairs reduce inflow. Manual pumping buys time.
- **Days:** data-driven; M2 ships Day 1 and a combined Day 2, with Day 3 copying Day 2. More content later.

---

## 4. Player features required

**Hands:** the Sprint 3 hands. Tan skin tone (not green glove, not grey). Upright, correct wrist
orientation on Quest 3 Touch Plus. Fingers blend from grip/trigger even in empty air (60–90 ms). Exactly
one hand visual per side; disable any legacy controller/glove model at runtime and in prefabs.

**No vignette, anywhere, any mode:** remove the TunnelingVignette from rig creation (LighthouseScenes.cs),
set `vignetteEnabled=false` and remove the option from ComfortSettings, delete the instances from every
scene/prefab via an editor script, and add an EditMode test that fails if any TunnelingVignetteController
exists in any build scene or player prefab. Also check URP Volume profiles for a Vignette override and
remove it.

**Digital wristwatch (Casio-style, no brand name):**
- A small rectangular resin digital watch: black case, grey LCD with dark segment digits, four side
  buttons, a strap that fits the hand's wrist. No logo, no "CASIO", no brand text anywhere. Low-poly
  (Blender headless if needed, <2k tris, one material + an LCD render texture or TextMeshPro on a quad).
- **Equip on either hand:** starts on the House tool bench (or the player can start wearing it on the
  left wrist, with the lead able to toggle the default in settings). Grab it and bring it near the
  opposite wrist: it snaps on with a click and a haptic pulse. Grab it off to move it to the other wrist
  or drop it. Remember the choice per player for the session.
- **Display:** a 7-segment style countdown `MM:SS` = time left in the day, plus `D3` (day number), plus
  a tiny progress indicator `2/5` (tasks done/total). Readable when raised to the face (~30 cm). Backlight
  turns on when the wrist is turned toward the head (dot product > 0.7), off otherwise. The last 60 s
  blink, with a short beep every 10 s. Optionally one press of the face (index finger poke) cycles
  TIME LEFT → TASKS → WATER LEVEL.
- **Co-op:** each player has their own watch, and players can see the other keeper's watch on their wrist
  (replicated wrist choice; the display shows shared day state).

**Modes:**
- **Solo** (lobby PLAY SOLO): offline, one player, co-op-only steps auto-pass, full game playable.
- **Co-op** (Host/Join, 2 players): normal rules with two-person gates.
- **Co-op with only one player (debug):** the lead must be able to Host, start alone and play everything.
  Two-person gates will block real progression; that's expected. But every interaction, the watch, the
  board, water, power, beacon and stairs must work so he can debug. Show a small "WAITING FOR SECOND
  KEEPER" line on the whiteboard for blocked steps. In development builds, a lobby toggle "Debug: allow
  solo co-op gates" lets him bypass the gates (compiled out of release).
- Both modes load the **same** Lab shell world.

**Comfort:** no forced camera motion, no shake, no vignette, nothing requires crouching. Snap turn on the
right stick, smooth locomotion on the left (as today).

---

## 5. Milestones (one per turn; wait for the lead's "next")

**M1 — Historical checkpoint, superseded by M1.5.** The original brief incorrectly selected Bootstrap.
The lead's headset result: wrong central-stair world, white horizontal hands, no flood, blue-block wrench.
Keep the successful no-vignette/no-panels/host-alone work. Editor success did not establish headset acceptance.

**M1.5 — Restore the real Sprint 2 game; correct hands on Android.**
1. Correct section 1, inspect and log session C's later reuse candidates, then restore the Lab scene
   exactly from `archive/sprint2-final-submission` before applying the limited changes below.
2. Use NetworkLobby's latest Fusion/PLAY SOLO/host-alone UI; route both modes through ONE constant
   into Lab. Guard Build Settings: NetworkLobby + Lab (+ a demonstrated dependency only). Delete no scenes.
3. Reapply M1 wins to this rig: remove TunnelingVignette itself (Sprint 2 only disabled it in preview),
   no floating selector/debug panels or placeholder cubes, host-alone start, Sprint 3 hands. Preserve
   the functional KEEPER WATCH board, real wrench, pipe, flood, bed, weather and side stairs.
4. Verify in graphics Play Mode, solo from lobby: House spawn → read board → grab the real wrench →
   carry to F1 pipe → automatic repair animation → water falls → F3 bed sleep → Day 2. Screenshot
   every step, plus flood at t=0, t=60 seconds unrepaired and after repair. Inspect the images yourself.
5. Diagnose WHITE hands on Android: log actual renderer/material/shader names in a development build;
   inspect stripping, Resources material, runtime replacement and duplicate visuals. Fix with URP
   Lit/Simple Lit, tan approximately #C68E6A, explicit shader inclusion and a regression test.
6. Rotate the hand VISUAL, never the tracked transform: Quest 3 Touch Plus grip posture, palm inward,
   knuckles up, fingers forward/slightly down. Measure against the grip pose on both hands; starting
   calibration roughly X -60° to -90° with left/right mirroring. Preserve tracking and finger blending.
7. If the restored lantern is cyan, make its light/material warm amber. Full look work remains M4.
8. Clean compile, EditMode and relevant PlayMode tests, then build the correctly routed development
   APK (`Builds/LighthouseKeepers-M1.5.apk`), install on the USB Quest, report and STOP.
   Physical checks: House/board, real wrench/pipe repair, water rises/falls, bed → Day 2, tan angled
   hands, no vignette. Never label installation or Editor rendering as physical headset acceptance.

**M1.5 headset corrections after bb2f997:** The lead accepts the base hand rotation; freeze it.
Before later milestones, mount the House board flush to the wall (centre 1.5 m; normal within 2°;
back gap under 0.03 m), remove the scale-reference cube/marker at their source, texture the workbench,
and rest a single-mesh 25 cm steel wrench on it with an eight-second return to the same pose.
Fit mirrored handle attachments inside the existing Sprint 3 WrapGrip without changing base hand
rotation; preserve automatic tightening/haptics. Remove grain, REC, battery and exposure text, keeping
only a small day/countdown/task readout. Review first-person/side grip screenshots, run regression
checks, measure Quest GPU before/after, rebuild/install/push and stop. See `M1.5-house-fixes.md`.

**M2 — Gameplay skeleton + generator foundation (lead revision).** M1.5 build 39eb8eb is
accepted: the House → wrench → repair → drain → bed loop works. Freeze hand rotation. Log slightly
transparent walls and imperfect wrench grip in BACKLOG.md; do not polish them in M2.
1. ScriptableObject Day01/02/03 assets in Assets/Data/Days define station/task text, duration and
   starting faults. D1 pipe only/power on. D2 generator + pipe, free order, power off; both required
   before sleep. D3 copies D2. Live board checkmarks use these assets. Adding Day04 requires a new
   asset, not code; document in exactly ten lines in DAYS.md. Development-only Day 2 lobby shortcut.
2. STN_GENERATOR on F2 against the wall, standing reach, clear stairs/routes. Reuse the best existing
   diesel mesh after surveying Sprint 2 Lab, Bootstrap Level02 and session C. GRIP_StarterHandle:
   direct grab, ~0.3 m pull toward player, spring return; three good pulls start it. Each pull gets
   crank audio, haptic and engine-only shake. Running loop, green lamp and brighter F2 lights on start.
   Reuse Sprint 3 verbs; inspect Tapan's starter branch and Storm Watch. Primer/choke are future stubs.
3. One day-owned PowerState. Power off dims F1/F2 to playable emergency light and disables the
   electric pump, increasing water rise. Power on restores lights/pumping. Preserve Day 1 behavior.
4. Preserve hands, wrench/repair, flood, bed, clean camera, solo and host-alone. No hand rotation changes.
5. Verify Day 1 and Day 2 in graphics Play Mode from lobby: off → three pulls → on → repair → drain
   → both board checks → sleep → Day 3. Inspect screenshots of F2 stairs/generator, held handle,
   off/on lighting and Day 2 board. Compile/test/build Builds/LighthouseKeepers-M2.apk, install USB,
   push team, report physical checks and stop. Wristwatch and seven-day content are deferred.

**M3 — Hands/body polish.** Refine embodiment and interaction poses on the real world, upper-body
arms/torso, reach, ownership-ready hand/watch attachment. M1.5 fixes color/orientation first.

**M4 — Look.** Amber beacon with power, warm tungsten interior, fault-only emergency lighting, heavy
rain/wind/sea/soft lightning, wet windows and outdoor-only lens droplets. Reuse parked ShipLensDroplets
and presentation pieces deliberately. Target 72 fps on Quest 3; report measured device frame time.

**M5 — Co-op.** Two players, replicated hands/body/watch, wrench ownership, shared board/water/day
state and appropriate two-person gates. Document Editor-host + headset-client evidence separately from
physical two-player acceptance. Host alone remains usable for debugging.

**M6 — Ship to main.** PR `sprint3/one-lighthouse` → `main`, tag `sprint3-final`, APK from that tag,
GitHub Release and README download/sideload/PLAY SOLO instructions. Reinstall Release download and smoke test.

---

## 6. Working rules

1. One milestone per turn. When finished, stop and wait. The lead's headset notes always come first.
2. Small commits with conventional prefixes. `git push team sprint3/one-lighthouse` at the end of every
   turn; print the SHA. Never commit "* 2.*"/"* 3.*" duplicates, Library/, Temp/, Builds/, artifacts/.
3. Change scenes through editor scripts or menus (repeatable), never hand-edited .unity YAML.
4. Before you finish each milestone: clean Unity batch compile, EditMode + relevant PlayMode tests, a
   graphics Play Mode run from the lobby with screenshots you have LOOKED AT yourself
   (artifacts/one/M<n>/*.png, fix what looks wrong before reporting), the Android APK
   (`Builds/LighthouseKeepers-M<n>.apk`), `adb install -r` if `adb devices` lists the Quest.
5. Report format (short):
   STATUS (VERIFIED in editor / BUILT / INSTALLED) · WHAT CHANGED (≤6 bullets) · REUSED FROM (refs) ·
   SCREENSHOTS (paths) · APK + install result · HEADSET CHECKS (≤6 numbered physical checks) ·
   KNOWN ISSUES · PUSHED SHA.
6. Append one line per decision to `Docs/OneLighthouse/LOG.md`.
7. If something is ambiguous, pick the option closest to the concept art and section 3, log it, and continue.
