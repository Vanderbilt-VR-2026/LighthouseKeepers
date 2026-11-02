# Co-op basics: protected solo baseline

Work branch: `coop/basics`. No main push or merge. Original `coop/two-headsets`
worktree and its uncommitted documentation/artifacts were left untouched.

## Base discovery (2026-11-01)

Fetched `team --prune` and inspected all Eli-authored refs and open PRs.
The newest Eli Gripenstraw commit is `4d1cff7e10a35b22c54382a34f70be5d07aef731`,
2026-11-01 16:51:23 -0600, `codex/readme-intentional-design`, open PR #73.
It is not merged. Its parent is main `c2dfbde`; its only change is README prose.
The branch starts here as requested; the runtime is identical to current main.

- Explains the first view and next player action.
- Describes tension and recovery moments.
- Explains direct/ray controls and their bindings.
- Documents feedback and hand appearance.
- Changes no networking, group codes, lobby, Watch Menu, or gameplay.

Latest Eli networking-specific work: `48cc609aa82c792815bf6946c859ac48d9b51c27`,
2026-10-29 21:23:23 -0500, `codex/first-person-network-proof-main`, open PR #71.
The parallel Sprint 3 proof is `1f30b27`, PR #72. These are isolated desktop
Photon proof scenes, not shared Lab task implementations. Their useful method is
independent client input, receive-side pose evidence, and actual disconnect removal.

Will's open PR #67, `william/sprint-3` at `a8fde69`, supplies network practice,
prop layout/generation checks, exclusive leases, release momentum and recovery.
Read `Docs/Sprints/Sprint3/WilliamNetworking.md` and the PR body before reuse.
Existing `coop/two-headsets` at `1856e84` adds two Lab spawns, remote tan hands and
host snapshots, but disables guest station interaction. Those commits are reused;
shared guest task commands and shared bed readiness are new work on this branch.
The newer Eli README and submission links were retained when resolving the reuse
cherry-pick's README conflict.

## Untouched baseline evidence

Unity 6000.3.23f1, source `4d1cff7`, clean tracked state before building.
Files: `artifacts/coop/baseline/` (also linked from the original workspace).

- Ship pipeline `StormWatchAuthoring.BuildQuest`: PASS, zero build errors.
  `Baseline.apk`, build manifest and APK/native VR verification are saved there.
- Full EditMode: **200 passed / 0 failed / 0 skipped**.
- Full PlayMode, graphics enabled and `LK_FUSION_CLOUD_TESTS=1`:
  **25 passed / 15 failed / 0 skipped**.
- Passing suites include current Lab lobby/solo, actual wrench repair/drain/bed,
  generator pulls/pump, shell stairs, Watch Menu rays/retry/replay/return, crew
  keypad, real Photon roster/ready/late-join/kick/disconnect and old launch guard.
- Pre-existing failures: BootstrapIntegrationTests (1; excluded Bootstrap scene),
  FirstWatchPlayTests (5; scene setup timeout), PlayerTraversalTests (6; old scene
  setup null references), StormWatchSceneTests (3; scene setup timeout).
  Exact names/messages and pass sets: `test-results.json`, XML and logs.
  No legacy test, old scene, or build list was changed to hide these failures.

Graphics replay: `artifacts/coop/SoloReplay.cs` is compiled in memory by Pipeline,
not imported into the game. It uses the real PLAY SOLO button, XR selections,
controller wrist rotations, starter strokes, radio controls/PTT, and bed calls.
A recorded test-only seed fixes pipe order. It repositions the Editor rig between
stations; it is not headset usability or stair locomotion evidence.

All loop assertions completed: Day 1 three bolts/drain/bed; Day 2 three generator
pulls and pipe/bed; Day 3 actual 75-second beacon loss/retry, generator,
POWER/TUNE/wrong-and-correct CODE/released-and-held PTT, pipe, board 3/3, and bed
into the existing Day 4 placeholder. Screenshots were opened and inspected.
The harness records one unrelated UnityEditor.Search.SearchDatabase indexing
exception, so its raw `result.json` is false despite all gameplay assertions
completing. This exception is preserved verbatim in `solo/loop-evidence.txt`.
Do not describe this baseline as an entirely green full suite.

The only tracked write produced by the baseline Editor was a dynamic TMP fallback
font glyph cache. Its diff was inspected and reverted after closing the Editor.
No gameplay source, scene, hand asset or day asset was edited before this evidence.
