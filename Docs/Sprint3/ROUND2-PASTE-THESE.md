# Sprint 3 — Round 2: "One Lighthouse"

## What went wrong (diagnosis from the repo, 21 Oct)

1. **The build loads the wrong world.** `CrewLobbyTransport` loads `FusionLobbyTransport.GameplaySceneName`, which is still the Sprint 1/2 scene `LK_StormWatch.unity`. Your Sprint 2 environment (`Assets/Scenes/Shell/*` and `Assets/Scenes/Dress/*`: House, F1–F4, Exterior) **isn't in EditorBuildSettings at all.** That's why you saw a mix: StormWatch's old geometry and blue beacon, with a few of your textures and stairs.
2. **The Sprint 3 work never reached the scene you played.** The reviewer's own report says: *"The existing networking builder uses the production lobby and inherited Storm Watch… C's fixtures and D's new days are not this entry."* So the new hands, wrench, pipe repair and day board were merged in code but never attached to anything you could load.
3. **The "Play Solo" commit made things worse.** To make solo work, Codex added big DAY 1/2/3/RELAY selector panels to every floor of StormWatch. Those are the panels you don't want.
4. **Open review blockers:** B is missing `ActivateDayLeak(int)`, which D calls. E fails to compile against `ForceGeneratorFault()`. Hand actor IDs aren't assigned. A completion event skips D's gate.
5. **Junk files are back:** about 20 untracked `* 2.meta`, `* 2.prefab` and `* 2.json` files. Don't commit them.

**The fix in one sentence:** one ship scene set (your Shell and Dress House, F1–F4 and Exterior), the lobby unchanged, everything else wired into it, and StormWatch retired from the build.

---

## Order of operations

**Tab 7 (plain terminal), first:**
```bash
cd "/Users/aneeshvasamreddy/Desktop/Virtual Reality Design/Lighthouse Keepers"
git branch backup/integrate-before-round2 integrate/sprint-3
git push team backup/integrate-before-round2
mkdir -p ~/LK-junk && find Assets -name "* [0-9].*" -exec mv -n {} ~/LK-junk/ \;
git status -s | head
```
Then merge `integrate/sprint-3` into each of the five sprint3 branches, so every session starts from what you just saw:
```bash
for b in simulation progression hands environment networking; do git checkout sprint3/$b && git merge --no-edit integrate/sprint-3 && git push team sprint3/$b; done
git checkout integrate/sprint-3
```

**Then, in the tab that's already running each session** (a fresh session loses its context and costs more), paste the **ROUND 2 CONTRACT**, then that session's block, in the order B → C → A → D → E → F.

Profiles stay the same: Sol high for A–D, Astra for E, Terra for F. **This round also puts C on Astra.** C has the hardest job (rebuilding a whole scene set), and a wrong scene costs you a day.

---

## ROUND 2 CONTRACT (paste into all six first)

```
ROUND 2 — the lead wore the APK and it was the wrong game. Read this before anything.

ROOT CAUSE: the lobby loads LK_StormWatch (Sprint 1/2). The lead's real environment —
Assets/Scenes/Shell/LK_Shell_{House,F1,F2,F3,F4,Exterior} + Assets/Scenes/Dress/LK_Dress_* —
is not in Build Settings. Sprint 3 hands/wrench/pipe/day-board were merged as code but never
attached to the scene that loads. Per-floor DAY1/DAY2/DAY3/RELAY panels were added to StormWatch.

THE VISION (lead's concept art, non-negotiable):
- HOUSE: tool bench with wrench + tools on wall, and ONE whiteboard "KEEPER WATCH" on the wall
  listing that day's tasks as a checklist. Tasks differ per day. This is the ONLY task UI.
- F1 PLUMBING: pipes with red valve wheels, a leaking coupling, the hand pump, water on the floor.
- F2 POWER & RECOVERY: yellow diesel generator, control panel, manual bilge pump.
- F3 RADIO & FORECAST: radio desk, storm chart, bed.
- F4 BEACON: warm AMBER/gold lantern (never blue/cyan), gallery railing outside.
- EXTERIOR: night, HEAVY rain, wind, crashing sea. Rain droplets on the camera lens when outside.
- Loop: FAULT -> DIAGNOSE -> REPAIR/RESTORE POWER -> WATER FALLS + BEACON ON -> NEXT WATCH.
  Power runs pump and beacon. Repairs reduce inflow. Manual pumping buys time.
- Keep: the lobby (unchanged), the stair and wall textures, the spiral stairs.
- Remove: floating DAY/RELAY selector panels, debug text, orange boxes, the yellow ceiling cube,
  primitive placeholder props, the comfort vignette ("goggles" effect).

SHIP SCENE SET (single source of truth, write it to Assets/Scenes/Sprint3/ShipScenes.json):
  0 LK_Lobby_Environment (+ existing NetworkLobby)  -> 1 LK_Shell_House (+Dress_House)
  additive: Shell/Dress F1..F4 + Exterior. LK_StormWatch, Lab, DevGym, Level01-04 leave the build.
Solo and multiplayer load the SAME scenes. Solo = one player, co-op gates auto-satisfied.

RULES: Round-1 contract rules R1-R7 still apply (lanes, DECISIONS.md, VERIFIED/BUILT-UNTESTED,
push to `team`, never commit "* 2.*" files, never hand-merge .unity/.prefab).
NEW R8: nothing counts unless it is reachable from Lobby -> Play Solo in the ship scene set.
Every report includes a screenshot from an Editor Play Mode run of the ship scenes, taken at the
named anchor, saved to artifacts/round2/<letter>/.
```

---

## B — Simulation (tab 2)

```
ROUND 2 — SESSION B.
1. Add ISimulationHost.ActivateDayLeak(int leakId) and ForceGeneratorFault() (D and E call them;
   E currently fails CS0535). Update LocalAuthority and every implementer.
2. Remove any direct subscriber that completes a day on OnLeakRepairCompleted — D owns completion.
3. Support up to 3 independent leaks (F1 has 3 couplings per the board "PIPE 0/3").
   Each repaired leak reduces inflow by its share. Invariant test stays: manual pumping alone
   = 85-95% of ONE active leak's inflow.
4. Expose SimSnapshot.BeaconLit = GeneratorRunning && PowerAvailable>0.5 for C's lantern.
5. Add tuning for 7 days (DayTuning per day: leak count, inflow multiplier, generator fault y/n,
   day length seconds). D reads it.
Report per contract. Then: idle, take tuning notes from the lead.
```

## C — Environment (tab 3) — relaunch this one with `--profile escalate`

```
ROUND 2 — SESSION C. You own the biggest fix.
1. Build Settings = the ship scene set ONLY (contract). Write an editor menu
   "LK/Ship/Apply Build Scenes" that sets it from ShipScenes.json, plus an EditMode test that
   fails if LK_StormWatch or any Lab/DevGym/Level scene is in the build.
2. Migrate into Shell/Dress scenes, by named anchor (contract §2.4), everything the game needs:
   STN_TOOL_TABLE + wrench mesh (House), STN_BRIEFING_BOARD whiteboard (House wall, eye height
   1.5m, readable from 2.5m), STN_PIPE_LEAK x3 couplings + red valve wheels + STN_MANUAL_PUMP
   with PIVOT/GRIP (F1), STN_GENERATOR with GRIP_* + panel + bilge pump (F2), radio desk, chart,
   STN_BED (F3), lantern + gallery (F4). Reuse Sprint 2 WatchDemo/LK_GrabWrench meshes and the
   pipe-repair art from LK_StormWatch / Level01 — copy the assets, do not reference StormWatch.
   Model anything missing in Blender headless: low-poly, Quest-3 budget (<= 150k tris per floor,
   <= 2 materials per prop, 1024 textures, baked lighting).
3. Lighting: warm. Beacon lantern AMBER (#FFB347-ish), emissive + rotating beam cone, lit only
   when SimSnapshot.BeaconLit. Interiors warm tungsten wall lamps like the concept. Delete the
   yellow ceiling cube and every unlit primitive. Remove all per-floor DAY/RELAY panels.
4. Exterior storm: night skybox, heavy rain particle system (GPU, Quest-safe, ~3-5k particles
   around the player only), wind audio, wave crashes, lightning flash every 20-40s (no strobe,
   comfort-safe). Screen-space lens droplets ONLY when the head is outdoors (trigger volume),
   fading 1.5s on entering; implement as a URP renderer feature, budget < 1ms on Quest 3.
   Rain hitting windows visible from inside.
5. Remove the comfort vignette everywhere (find it in the rig/URP volume; log where it was).
6. Run GeometryIntersection/Luminance/ShipSceneCleanliness validators on the ship set.
Screenshots of House, F1, F2, F3, F4, Exterior from player eye height. Report.
```

## A — Hands & repair (tab 1)

```
ROUND 2 — SESSION A. The lead's headset: hands still wrong, wrench and pipe repair gone.
1. Hands: verify on the rig that actually loads (ship scenes via Play Solo): correct wrist
   orientation, fingers curl from grip/trigger in empty air, glove material (not bare green).
   Write a PlayMode test that loads Lobby -> Play Solo and asserts PF_Hand_L/R are the active
   visuals and the old controller/glove meshes are disabled.
2. Restore the Sprint 2 pipe repair, upgraded: grab wrench at STN_TOOL_TABLE (WrapGrip pose,
   haptics), carry it up to F1, touch a coupling and TWIST (wrist rotation ~180° cumulative)
   with progress feedback (creaking audio, spray shrinking, haptic ticks), raise
   OnLeakRepairCompleted(actorId) per coupling. Port WatchDemo/WatchWrenchHandPose + WrenchHaptics.
3. Pump stroke and generator primer/choke/starter: confirm they bind to C's new anchors.
4. Full body (lead wants Blade & Sorcery feel): upper-body IK — arms from shoulders to hands
   (two-bone IK), torso following head yaw with lag, oilskin jacket. Hands never pass through
   world colliders (physics hands that stop on surfaces, visual offset to controller).
5. Wristwatch on left wrist: analogue face showing time left in the day (reads D's director);
   glance at it -> subtle tick audio.
Assign ActorId from E's player identity (request file if blocked). Report with screenshots.
```

## D — Days & the whiteboard (tab 4)

```
ROUND 2 — SESSION D.
1. The ONLY task UI is the House whiteboard (STN_BRIEFING_BOARD): handwritten-style
   "KEEPER WATCH — DAY N" + 3-6 checkbox tasks that tick live as completed + time left.
   Delete DAY1/2/3/RELAY selector panels and every other floating board.
   Optional small chalk slate per floor showing only that floor's current task (no buttons).
2. Seven days, each different, following FAULT->DIAGNOSE->REPAIR->WATER FALLS+BEACON->NEXT WATCH:
   D1 one leak, learn wrench + pump. D2 generator dies, restart (primer/choke/starter) in blackout.
   D3 two leaks + radio relay of a forecast. D4 leaks + generator fault together.
   D5 beacon misalignment on F4 during storm peak. D6 three leaks, falling power, triage.
   D7 everything, longest storm, ends at dawn. Days are data assets; tune via B's DayTuning.
3. Day end: all tasks done -> go to STN_BED on F3 -> fade -> next day starts in House.
   Fail: water over failHeight -> readable message -> retry day.
4. Solo: co-op gates auto-satisfied when one player. Day select lives in the LOBBY only
   (a small "Start at day" option) — not in the world.
5. Expose time-left for A's wristwatch.
Fix the ActivateDayLeak dependency against B's new API. Report.
```

## E — Networking (tab 5)

```
ROUND 2 — SESSION E. The lobby stays visually as-is.
1. Point GameplaySceneName / scene loading (solo AND hosted) at the ship scene set from
   ShipScenes.json, not LK_StormWatch. Additive load House + F1-F4 + Exterior.
2. Fix CS0535 (implement ForceGeneratorFault / ActivateDayLeak in the networked authority).
3. Assign ActorId per player to A's hands. Replicate the full-body pose (head, hands, torso yaw)
   so the other keeper reads as a person with arms.
4. Remove the solo-practice panels/Relay buttons you added in 8386dc8 from world scenes;
   keep the lobby's PLAY SOLO button.
5. Remove the comfort vignette if it lives on the rig you own.
PlayMode test: Lobby -> Play Solo -> House scene loaded, whiteboard shows Day 1, wrench present.
Report.
```

## F — Reviewer / integrator (tab 6)

```
ROUND 2 — SESSION F. Merge order B -> C -> A -> D -> E into integrate/sprint-3 as each
reports. Before building: run "LK/Ship/Apply Build Scenes", run the build-scene guard test,
refuse to build if LK_StormWatch is in the build or any "* 2.*" file is staged.
Build ONE APK, install with adb, then capture Editor Play Mode screenshots from Lobby -> Play
Solo of: House whiteboard, wrench on bench, F1 couplings, F2 generator, F4 amber beacon,
exterior rain. Put them in Docs/Sprint3/Round2Report.md with a 15-step headset checklist:
lobby -> solo -> grab wrench -> repair 1 pipe -> pump 5 strokes -> watch tasks tick on board ->
climb all floors -> go outside see rain + lens drops -> beacon amber -> bed -> Day 2 starts.
Flag anything still showing a selector panel, blue light, vignette or primitive cube.
```

---

## When you put the headset on
Run F's 15 steps. Report problems to tabs using the same table as before. Three checks decide whether this round worked: **is it your lighthouse, is the wrench on the bench, and is there exactly one whiteboard.**
