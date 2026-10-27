# One Lighthouse, One Session

## The approach and why

**One Codex session, one branch, one milestone at a time, and an APK you wear after each milestone.**

Six sessions failed for structural reasons, not because the models were weak:
- **Nobody owned the whole game.** Each session made its own piece "work" in its own copy of the project. The pieces only met at the merge, and nobody checked which scene actually loads. That's how you ended up in StormWatch.
- **Context loss.** Each session saw about a sixth of the game and none of what you saw in the headset.
- **Overhead.** Most of every report was about blockers, requests to other sessions and merge order, not your game.

One session on the frontier model, with a tight loop, will get more done:
- **It sees everything.** There's one scene, one branch and one build, so nothing like the StormWatch mix-up can happen.
- **You're the reviewer.** Each milestone ends with an APK and five things to check. You test it and paste back what's wrong.
- **Changes are small.** If something breaks, it's obvious what caused it.

**The environment:** `LK_Bootstrap` loads `LK_Exterior` + `LK_Core` (the tower) + `LK_Level01–04`. The stairs come from `LighthouseArchitecture.cs`, which builds a **full 360° spiral around the centre, from radius 1.0 to 2.6 m.** That's why they take up the middle of every floor. The fix is to change the generator, not to drag things around by hand: a **quarter-turn flight of stairs against the outer wall** on each floor, with the floor opening moved to match. That leaves the centre of every floor free for level design. Since the stairs are generated, the change is repeatable and you can tweak it.

---

## STEP 1 — Terminal (once)

Close Codex tabs 1–5: type `/quit` in each, then close the tab. In tab 6 type `/quit` too. Then, in one terminal:

```bash
cd "/Users/aneeshvasamreddy/Desktop/Virtual Reality Design/Lighthouse Keepers"

# put back the 6 real files the earlier cleanup moved
git restore -- "Assets/Samples" "Assets/XR/Settings 1.meta" "Assets/free horror ambience 2.meta"

# move the remaining macOS duplicate junk out of the project
mkdir -p ~/LK-junk/3
mv -n "Assets/Art/Hands/Resources 2.meta" "Assets/Prefabs/Environment/Sprint3/README 2.md" "Assets/Prefabs/Environment/Sprint3/README 2.md.meta" ~/LK-junk/3/

# save the half-finished Round 2 work that landed in this folder (nothing is lost)
git switch -c parked/round2-main-folder
git add -A && git commit -m "wip: park round 2 changes left in main folder"
git push team parked/round2-main-folder

# start clean from the last build you played
git switch integrate/sprint-3
git switch -c sprint3/one-lighthouse
git push -u team sprint3/one-lighthouse
git status -s
```
`git status -s` should print nothing. The old worktrees and branches stay where they are untouched, so you can come back to anything later.

Open **Unity Hub** (signed in) and **close the Unity Editor**.

## STEP 2 — Start the one session

```bash
cd "/Users/aneeshvasamreddy/Desktop/Virtual Reality Design/Lighthouse Keepers"
codex --yolo -m gpt-6-astra -c model_reasoning_effort=high
```

Paste **M0** first, then **M1**. After each milestone, install the APK, run its checks, and paste your notes back. **Don't paste the next milestone until the current one passes in the headset.**

---

## M0 — Brief (paste first, once)

```
You are the single engineer on Lighthouse Keepers, a co-op VR survival game for Meta Quest 3
(Unity 6000.3.23f1, URP, OpenXR, XRI 3.x, Photon Fusion). Main folder, branch
sprint3/one-lighthouse. You have no sandbox, full git, network, Unity and adb.

HISTORY YOU MUST KNOW: we tried six parallel agents. It failed: the lobby's Play Solo loaded
LK_StormWatch (an old scene) instead of the real game, per-floor DAY/RELAY selector panels were
added, the wrench and pipe repair disappeared from what loads, and hands looked wrong. You are now
the only agent. Old branches sprint3/{hands,simulation,environment,progression,networking} and
parked/round2-main-folder contain code you MAY read and port, but never merge wholesale.

THE GAME (lead's concept): storm-bound lighthouse. HOUSE (tools on bench, wrench, ONE whiteboard
"KEEPER WATCH" listing that day's tasks) -> F1 PLUMBING (pipes, red valve wheels, leaking
couplings, hand pump, water) -> F2 POWER (yellow diesel generator, panel, bilge pump) -> F3 RADIO &
FORECAST (radio desk, storm chart, bed) -> F4 BEACON (warm amber lantern, gallery). Loop: FAULT ->
DIAGNOSE -> REPAIR/RESTORE POWER -> WATER FALLS + BEACON ON -> NEXT WATCH. Power runs pump and
beacon; repairs reduce inflow; manual pumping buys time. Solo and 2-player co-op.

THE ENVIRONMENT IS LK_Bootstrap (loads LK_Exterior + LK_Core + LK_Level01-04). Keep it, its
textures and stair look. The lobby (LK_NetworkLobby) stays as is.

WORKING RULES:
1. One milestone per turn. Do not start the next until I paste it.
2. Small commits, conventional prefixes, push to `team` after every milestone. Never commit
   macOS duplicates ("* 2.*", "* 3.*"). Never hand-edit .unity YAML; change scenes through editor
   scripts/menus so they're repeatable.
3. Every milestone ends with: Unity batch compile clean, relevant tests run, a Play Mode
   screenshot set you have LOOKED AT yourself (artifacts/one/<milestone>/*.png), an Android APK
   (IL2CPP ARM64 development), `adb install -r` if the Quest is connected, and a short report:
   STATUS, WHAT CHANGED (<=5 bullets), SCREENSHOTS, APK, HEADSET CHECKS (5 max), KNOWN ISSUES.
4. Comfort: no forced camera motion, no vignette, no shake, nothing that requires crouching.
5. When I send headset notes, fix them before anything else.
6. Keep Docs/OneLighthouse/LOG.md: one line per decision. Re-read it at the start of each turn
   so context survives.

Acknowledge in <=5 lines, then wait for M1.
```

## M1 — Right world, no clutter

```
M1 — make Lobby -> Play Solo (and Host) load LK_Bootstrap, the real environment.
- Find every route to LK_StormWatch (CrewSessionPolicy.StormWatchSceneName, CrewLobbyTransport,
  FusionLobbyTransport.GameplaySceneName, SoloWatchLink/SoloWatchSession). Point solo and hosted
  play at LK_Bootstrap through one constant.
- Build Settings: index 0 LK_NetworkLobby, then LK_Bootstrap, LK_Exterior, LK_Core,
  LK_Level01-04. Remove StormWatch, FirstWatch, DevGym, LobbyPreview, Lab, Shell/Dress, SampleScene.
  Add an editor menu "Lighthouse Keepers/Apply Ship Build Scenes" and an EditMode test that fails
  if anything else is in the build.
- Remove the per-floor SOLO DAY 1/2/3/RELAY selector panels and any floating debug text/boards
  from what loads. Keep the lobby's PLAY SOLO button.
- Remove the comfort vignette wherever it lives (rig, locomotion, URP volume). Log where.
- Delete the glowing yellow ceiling cube(s) if they exist in these scenes.
Screenshots: lobby, spawn point, each floor from eye height. Build, install, report.
```

## M2 — Stairs to the side

```
M2 — move the stairs out of the middle of every floor to free space for level design.
Today LighthouseArchitecture.cs generates a full 360° spiral (Annulus radius 1.0-2.6) around the
centre, plus a central column/core.
- Change the generator: on each floor, a quarter-to-half-turn stair flight hugging the OUTER WALL
  (inner radius ~ wall radius - 1.1 m, outer radius just inside the wall), rising one storey,
  with a matching opening in the floor above and a railing on its open side. Alternate the side
  floor-to-floor (or stack in the same sector — choose whichever gives the most usable floor and
  log why). Remove the central column unless it's structural for the F4 lantern; if kept, shrink it.
- Keep the current wood tread and iron materials and the railing style.
- Comfortable: rise <= 0.19 m, tread >= 0.28 m at walking line, headroom >= 2.1 m, continuous
  smooth collision ramp under treads (current approach), no snagging at landings.
- Regenerate through the existing menu so it's repeatable. Re-run WalkthroughGeometryTests and
  fix them for the new layout; add a test asserting the floor centre (radius < 2.2 m) is clear on
  F1-F4.
- Any level object (generator, pipes, radio, etc.) that now overlaps the stairs: move it, log it.
Screenshots per floor: one looking at the stairs, one showing the open centre. Top-down
orthographic capture of each floor too. Build, install, report.
```

## M3 — Hands, wrench and pipe repair

```
M3 — hands and the core repair loop.
- Hands: upright, correct wrist orientation on Quest 3 Touch Plus, tan leather glove, fingers
  blend from grip/trigger in empty air (60-90 ms). Only one hand visual per side. Port the best of
  sprint3/hands (read, don't merge).
- HOUSE: tool bench with wrench (+ a few tools on the wall). Wrench: WrapGrip pose, attach on
  handle, generous grab collider, throwable, haptics. Port Sprint 2 feel from
  sprint2/final-submission Assets/WatchDemo (WatchWrenchHandPose, WrenchHaptics, LK_GrabWrench).
- F1: three leaking pipe couplings with visible spray + red valve wheels. Repair = hold wrench on
  coupling and twist ~180° cumulative: creak audio, shrinking spray, haptic ticks. Each repair cuts
  inflow. Hand pump with a constrained handle: each full down-stroke removes water, thump + surge.
- Water rises/falls visibly and the existing flood sim drives it (use the best of
  sprint3/simulation if Bootstrap's is weaker; log which).
Headset checks: fingers curl; pick up/throw wrench; carry to F1; fix all 3 couplings; pump 10x.
```

## M4 — The whiteboard and the days

```
M4 — one task board, real days.
- HOUSE wall: ONE whiteboard "KEEPER WATCH — DAY N", handwritten style, 3-6 checkbox tasks that
  tick live, and time left. Readable from 2.5 m. No buttons. No other boards in the game.
- 7 days as data assets: D1 one leak, learn wrench + pump. D2 blackout, restart generator
  (primer/choke/starter on F2). D3 two leaks + radio relay on F3. D4 leaks + generator fault.
  D5 beacon misalignment on F4 at storm peak. D6 three leaks, falling power, triage. D7 everything,
  ends at dawn.
- Day end: tasks done -> sleep in F3 bed -> fade -> next day starts in the House. Fail: water too
  high -> clear message -> retry the day.
- Lobby: small "Start at day 1-7" option for testing. Solo auto-passes co-op-only steps.
Headset checks: Day 1 board, ticking tasks, bed -> Day 2 has different tasks, Day 2 generator.
```

## M5 — The look

```
M5 — atmosphere.
- Beacon: warm amber/gold lantern + rotating beam, lit only when power is on. No blue/cyan.
- Interiors: warm tungsten wall lamps like the concept; readable but moody. Emergency red only
  during faults.
- Exterior: night, heavy rain (GPU particles around the player, Quest-safe), wind, crashing waves,
  occasional soft lightning. Rain streaks on windows seen from inside.
- Lens droplets on the camera only when the head is outside (trigger volume), fade in/out 1.5 s,
  URP renderer feature under 1 ms on Quest 3.
- Hold 72 fps on Quest 3: report frame time from a profiled development build.
Headset checks: beacon amber, step onto F4 gallery -> rain + droplets, back inside -> they clear.
```

## M6 — Body, watch and co-op pass

```
M6 — presence and multiplayer.
- Upper body: two-bone arm IK shoulder->hand, torso yaw follows head with lag, oilskin jacket;
  hands stop on walls/tables (physics hands, visual offset). Blade & Sorcery feel as the target.
- Left-wrist watch showing time left in the day.
- Co-op: two players in LK_Bootstrap via the existing lobby/Fusion; each sees the other's body and
  hands; wrench grab ownership transfers cleanly; board state syncs. Document how to test with
  Editor-as-host + headset-as-client.
Headset checks: look at your arms; check watch; (with a friend or Editor) see the other keeper.
```

---

## Sending notes back

Paste them in plain English, e.g. *"M2: the F2 stairs clip the generator, and the railing is too low."* The session fixes the notes, rebuilds, and reports again. You only paste the next milestone once the current one passes in the headset.
