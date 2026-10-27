# Round 2B — Unblock and Land It

## Why every tab stalled (same cause for all six)

All six reports fail for the same reason, and it isn't the code: **the Codex sandbox.**

- **Unity won't start.** C reports "Unity startup IPC is blocked", B a licensing/IPC failure, and A and D socket `EPERM` errors. Unity needs the Hub's licensing sockets, which `workspace-write` blocks.
- **Git commits fail** (`index.lock` denied, C and A). A worktree's git data lives in the main repo's `.git/`, which is outside the session's sandbox.
- **Pushes fail** (D: "GitHub DNS unavailable"). The sandbox has no network.
- **adb fails** ("Operation not permitted").
- **E and F did nothing.** They only received the contract and asked for their lane. Their blocks below are complete.

**The fix:** reopen each session with the sandbox off. This keeps each session's context.

There's also a second problem. Six Unity instances importing six separate worktrees at once will bring the Mac to a crawl and fight over the license. So this round **only C and F run Unity.** A, B, D and E write code, run their non-Unity tests, commit and push, and F compiles and builds everything together.

---

## STEP 0 — Terminal (tab 7)

1. Open **Unity Hub** and confirm you're signed in with an active license. Close any open Unity **Editor** windows.
2. Run:
```bash
cd "/Users/aneeshvasamreddy/Desktop/Virtual Reality Design/Lighthouse Keepers"
git worktree prune
git status -s
```
3. In **each Codex tab (1–6)**: type `/quit`, then reopen the same conversation with the sandbox off:

| Tab | Command | When the picker appears, choose |
|---|---|---|
| 1 A | `codex resume --yolo -m gpt-5.6-sol -c model_reasoning_effort=high` | the hands conversation |
| 2 B | `codex resume --yolo -m gpt-5.6-sol -c model_reasoning_effort=high` | simulation |
| 3 C | `codex resume --yolo -m gpt-6-astra -c model_reasoning_effort=high` | environment |
| 4 D | `codex resume --yolo -m gpt-5.6-sol -c model_reasoning_effort=high` | progression |
| 5 E | `codex resume --yolo -m gpt-6-astra -c model_reasoning_effort=high` | networking |
| 6 F | `codex resume --yolo -m gpt-6-astra -c model_reasoning_effort=high` | reviewer |

`--yolo` is Codex's flag for no sandbox and no approval prompts. It's safe enough here because everything is in git and you made a backup branch. **If the picker is confusing or a session won't resume**, just run `codex --yolo` (adding the same `-m` and `-c` flags from the table) **inside that branch's worktree folder** and paste the Round 2 contract again, then that tab's Round 2B block. The worktree paths are:
```
A  ~/.codex/worktrees/212b/Lighthouse Keepers      (sprint3/hands)
B  ~/.codex/worktrees/4221/Lighthouse Keepers      (sprint3/simulation)
C  ~/.codex/worktrees/c243/Lighthouse Keepers      (sprint3/environment)
D  ~/.codex/worktrees/ae03/Lighthouse Keepers      (sprint3/progression)
E  ~/.codex/worktrees/5b03/Lighthouse Keepers      (sprint3/networking)
F  the main folder (integrate/sprint-3)
```

4. Paste the blocks below in this order: **B, D, A, E** (code only, quick), then **C** (Unity), then **F** (integrates and builds) last.

---

## PREAMBLE — put this at the top of EVERY block you paste (already included below)

```
ROUND 2B. Your sandbox is now OFF (no sandbox, network on, full git). The failures in your last
report — Unity IPC/licensing EPERM, index.lock denied, GitHub DNS, adb EPERM — were the sandbox.
They are gone. Do not report them again; if one recurs, run `codex --version`, `whoami`,
`ls -la $(git rev-parse --git-common-dir)` and print the result once, then continue.

UNITY POLICY THIS ROUND: only sessions C and F launch Unity. A, B, D, E must NOT start Unity
(six imports at once will thrash the machine and fight over the licence). You compile-check with
your non-Unity tests, commit, push, and let F compile the integrated project. F will paste you
any Unity compile errors in your lane — fix them fast.

Still binding: Round 2 contract (ship scene set = Shell/Dress House,F1-F4,Exterior; lobby
unchanged; House whiteboard is the only task UI; no StormWatch/Lab/DevGym/Level scenes in build;
amber beacon; heavy rain + lens droplets outdoors; no vignette; lanes R1; never commit "* 2.*").

Every request file you wrote last round (Docs/Sprint3/Requests/*-round2.md) addressed to YOU:
read it now and resolve it. Requests FROM you: the owner is being told to resolve them this round.

END OF TURN, mandatory: `git add` only your lane, commit, `git push team <your-branch>`, print the
pushed SHA. A report without a pushed SHA is a failed turn.
```

---

## B — Simulation (tab 2)

```
[paste PREAMBLE above first]

SESSION B — ROUND 2B. Your last turn: 52 managed tests pass, pushed 8020a51. Good. Now:

1. Resolve every request addressed to B: Docs/Sprint3/Requests/*-to-B*.md (A-to-B-round2, C-to-B,
   any from D). For each: implement, or write one line in the file saying why not. Specifically
   confirm these exist and are on ISimulationHost + LocalAuthority + every implementer:
   ActivateDayLeak(int), ForceGeneratorFault(), multi-leak (3 couplings, per-leak repair),
   SimSnapshot.BeaconLit, per-day DayTuning for 7 days.
2. Publish a frozen API summary at top of Docs/Sprint3/SimulationAPI.md titled "B API v2 (frozen)"
   with exact signatures. D, E, A code against this; do not change it again without a request.
3. Make sure B's debug panel cannot appear in a player build (compiled out, not just disabled).
4. Do NOT launch Unity. Run your 52 managed tests, commit, push to team/sprint3/simulation.
5. Then stay alive: F will send compile errors and the lead will send tuning notes.
Report: pushed SHA, the frozen API block, which requests you closed.
```

## D — Days & whiteboard (tab 4)

```
[paste PREAMBLE above first]

SESSION D — ROUND 2B. Your last turn: head d515061, push failed on DNS (sandbox). Now:

1. Push first: `git push team sprint3/progression`. Print SHA.
2. Merge team/sprint3/simulation (B's API v2) into your branch; recompile your offline scenarios
   against it. Remove any shim you wrote for missing ActivateDayLeak/ForceGeneratorFault.
3. Resolve requests addressed to D (Docs/Sprint3/Requests/*-to-D*.md).
4. The vision, confirm each is implemented in data/code (C attaches it to scenes):
   - ONE board, STN_BRIEFING_BOARD in House: "KEEPER WATCH — DAY N", 3-6 tasks with live ticks,
     time left. No buttons on it. No other boards anywhere. Per-floor slates optional, text only.
   - 7 days as data assets: D1 one leak + learn wrench/pump; D2 blackout generator restart;
     D3 two leaks + radio relay; D4 leaks + generator fault; D5 beacon misalignment at storm peak;
     D6 three leaks + falling power triage; D7 everything, ends at dawn.
   - Day end: tasks done -> STN_BED (F3) -> fade -> next day in House. Fail: readable message, retry.
   - Day select ONLY in the lobby ("Start at day 1-7"), coordinate with E who owns the lobby UI —
     you provide the API `IDayDirector.StartAtDay(int)`, E adds the control.
   - Expose `float SecondsLeftInDay` for A's wristwatch.
   - Solo: co-op gates auto-pass with one player; multiplayer keeps them.
5. Do NOT launch Unity. Run 25 scenarios, commit, push. Report SHA.
```

## A — Hands, body, wrench, watch (tab 1)

```
[paste PREAMBLE above first]

SESSION A — ROUND 2B. Your last turn was PROPOSED with nothing committed (index.lock — sandbox).

1. Commit and push everything from last turn now: `git push team sprint3/hands`. Print SHA.
2. Merge team/sprint3/simulation and team/sprint3/progression into your branch (API v2,
   SecondsLeftInDay). Fix your code against them.
3. Your bindings target authored anchors by NAME; C is authoring them this round with exactly the
   contract names. Keep binding by name + scene-loaded callback; fail loudly (one log line naming
   the missing anchor) instead of silently.
4. Deliver, in code + prefabs in your lane (no scene edits):
   - Hands: upright, correct wrist orientation on Quest 3 Touch Plus, tan/leather glove material,
     fingers blend from grip/trigger in empty air (60-90 ms). Disable any old controller/glove
     visual at runtime so only PF_Hand_L/R render.
   - Wrench: WrapGrip pose, attach transform on handle, big grab collider, throwable, haptics.
     Twist-to-repair on a coupling: ~180° cumulative wrist roll, creak audio, shrinking spray,
     haptic ticks, raises OnLeakRepairCompleted(actorId) per coupling. Port the Sprint 2 feel from
     Assets/WatchDemo (WatchWrenchHandPose, WrenchHaptics) on sprint2/final-submission.
   - Pump stroke + generator primer/choke/starter verbs bound to C's anchors.
   - Upper body: two-bone arm IK shoulder->hand, torso yaw follows head with lag, oilskin jacket
     prefab PF_PlayerBody (E parents it under the rig root). Physics-limited hands that stop on
     walls/tables with visual offset.
   - Wristwatch on left wrist: analogue dial = D's SecondsLeftInDay, ticks quietly when looked at.
5. ActorId: read it from E's `IPlayerIdentity` (E is told to publish it); default 0 offline.
6. Do NOT launch Unity. Run offline tests, commit, push. Report SHA + list of prefabs F must see.
```

## E — Networking & lobby (tab 5)

```
[paste PREAMBLE above first]

SESSION E — ROUND 2B. You are SESSION E — NETWORKING. Branch sprint3/networking. You own
Assets/Scripts/LK.Net/**, Assets/Prefabs/Network/**, PF_PlayerRig root, the lobby scene.
Last turn you only read the brief. This turn you implement.

1. Merge team/sprint3/simulation, team/sprint3/progression, team/sprint3/hands into your branch.
2. Routing: FusionLobbyTransport.GameplaySceneName / CrewLobbyTransport must load the ship scene
   set from Assets/Scenes/Sprint3/ShipScenes.json (C writes it this round; until then hardcode the
   same list behind one constant): single-load LK_Shell_House, then additive Dress_House,
   Shell+Dress F1-F4, Exterior. Solo and hosted use the SAME path. Nothing routes to LK_StormWatch.
3. Remove from world scenes/code everything 8386dc8 added: per-floor DAY1/DAY2/DAY3/RELAY panels,
   SoloWatchLink selectors. Keep the lobby's PLAY SOLO. Add a small "Start at day 1-7" stepper in
   the lobby calling D's IDayDirector.StartAtDay(int). Lobby visuals otherwise unchanged.
4. Fix CS0535: networked authority implements B's API v2 fully.
5. Publish `IPlayerIdentity { int ActorId }`; assign per player; offline = 0. A reads it.
6. Parent A's PF_PlayerBody under PF_PlayerRig; replicate head, both hands, torso yaw + HandPose
   enum + grip/trigger floats. Interpolate remote.
7. Remove the comfort vignette if it is on the rig/locomotion provider (log where it was).
8. Do NOT launch Unity. Commit, push, report SHA and the exact scene list your loader uses.
```

## C — Environment (tab 3) — runs Unity

```
[paste PREAMBLE above first]

SESSION C — ROUND 2B. You MAY run Unity (batchmode and graphics). Unity Hub is signed in.
Last turn: HEAD 2d33be8, Round 2 work uncommitted. Your source audit flagged old lobby routes,
old build settings, pending scene attachments.

1. Commit + push your uncommitted Round 2 work immediately. Print SHA.
2. Merge team/sprint3/{simulation,progression,hands,networking} into your branch.
3. Open your worktree in Unity (batchmode first: -quit -logFile artifacts/round2/C/import.log).
   Fix any compile error in your lane; write any in other lanes to F (Docs/Sprint3/Requests/C-to-F-2b.md).
4. Run your tools for real, in this order, committing after each:
   a) "LK/Ship/Apply Build Scenes" -> Build Settings = Lobby + Shell/Dress House,F1-F4,Exterior.
      Manifest must NOT target Lab (F saw it still does). Fix that.
   b) Attach to scenes by contract name: STN_TOOL_TABLE + wrench (House), STN_BRIEFING_BOARD
      (House wall, 1.5 m, readable at 2.5 m), 3 coupling leaks + red valves + STN_MANUAL_PUMP with
      PIVOT/GRIP (F1), STN_GENERATOR + GRIP_* + panel + bilge pump (F2), radio desk, storm chart,
      STN_BED (F3), lantern + gallery (F4). Use the real art from Sprint 2 (WatchDemo, Level01,
      LK_GrabWrench) copied into your lane, or Blender low-poly to concept-art style. NO primitives.
   c) Lighting: warm tungsten interiors, amber beacon lit by SimSnapshot.BeaconLit, delete the yellow
      ceiling cube and every floating panel/primitive. Keep wall/stair textures and stairs.
   d) Exterior: heavy rain, wind, waves, gentle lightning; lens droplets only outdoors (<1 ms).
   e) Remove vignette from URP volumes/renderer if it lives there.
   f) Validators: geometry intersection, luminance, cleanliness — all ship scenes, all pass.
5. Run your Lobby -> Play Solo capture tool in graphics mode. Save PNGs to artifacts/round2/C/:
   house-board, house-bench-wrench, f1-couplings, f1-pump, f2-generator, f3-radio-bed,
   f4-amber-beacon, exterior-rain. LOOK at each one yourself and fix what's wrong before reporting.
6. Commit, push team/sprint3/environment, report SHA + the 8 screenshot paths.
```

## F — Reviewer / integrator (tab 6) — runs Unity, builds, installs

```
[paste PREAMBLE above first]

SESSION F — ROUND 2B. You are SESSION F — INTEGRATOR. You work in the main folder on
integrate/sprint-3. You may run Unity, build, and use adb. You may edit ONLY: merge resolutions,
Docs/Sprint3/**, ProjectSettings/EditorBuildSettings.asset via C's menu. Everything else: send the
exact error to the owning tab's request file and wait for their push.

Loop until accepted (do not stop after one pass):
1. `git fetch team --prune`. Merge in order B -> D -> A -> E -> C (C last: it owns scenes and
   must land on top of the others' code). Never hand-merge .unity/.prefab: take C's side.
2. Refuse to proceed if any "* 2.*" or other macOS duplicate is staged.
3. Unity batchmode compile. Errors -> append to Docs/Sprint3/Requests/F-to-<owner>-2b.md with the
   exact message and file:line, then re-fetch every ~10 min until fixed.
4. Run "LK/Ship/Apply Build Scenes" and the guard test. Build Settings must be exactly the ship set.
5. Graphics Play Mode: Lobby -> Play Solo. Capture the same 8 screenshots as C into
   artifacts/round2/F/. Reject the build (and file requests) if you see: StormWatch geometry, a
   DAY/RELAY panel, more than one task board, blue/cyan beacon, yellow cube, primitive props,
   vignette, missing wrench, missing couplings, no rain outside.
6. Build the Android APK (IL2CPP ARM64, development). If IL2CPP hits "same key: mscorlib",
   clear Library/Bee/artifacts/Android and retry once.
7. `adb devices`; if the Quest is listed, `adb install -r <apk>`. Print result.
8. Write Docs/Sprint3/Round2Report.md: SHAs merged, the 8 screenshots embedded, and a 15-step
   headset checklist: lobby -> Play Solo -> House, one board shows Day 1 -> hands upright, fingers
   curl -> grab wrench from bench -> carry to F1 -> twist-repair a coupling (board ticks) -> pump 10
   strokes -> water falls -> F2 generator restarts -> climb to F4, beacon amber -> step onto gallery,
   heavy rain + lens drops -> check wristwatch -> F3 bed -> Day 2 starts with different tasks.
9. Push integrate/sprint-3. Report SHA, APK path, install result.
```

---

## After F reports

Put the headset on and run the 15 steps. Write down whatever's wrong in plain English and paste each note into the tab that owns it (hands → A, water → B, anything visual → C, days and the board → D, lobby and multiplayer → E).
