# Round 2 — E: ship routing and networking

**Ship acceptance: incomplete. No new APK is claimed.** The graphics route below
is verified in an isolated integration snapshot; C/D still need to publish their
pending source/scenes and C must supply valid lighting bakes. The existing dirty
`sprint3/networking` worktree was preserved; implementation lives in the separate
`sprint3/networking-round2` worktree and is published to `team`.

Solo and hosted entry now resolve the same manifest TextAsset at
`Assets/Scenes/Sprint3/ShipScenes.json`: single-load Shell House, then its eleven
additive Dress/Shell scenes. No ship route targets Storm Watch. Both existing
lobbies were saved through Unity APIs with new route/config/component references;
comparison found **zero changes to pre-existing transform/renderer blocks**.
The Resource settings asset references the original JSON, rig, DayCatalog and
FloodTuning; it does not maintain another routing list.

E's facade now compiles unconditionally against B and implements
`ForceGeneratorFault()` and `ActivateDayLeak(int)`. One B SimulationHost advances
on the authority; clients receive immutable snapshots and a read-only D projection.
Remote lifecycle/debug commands are refused; remote interaction commands use
transport-authenticated actor IDs and lease/proximity validation. Water and amber
lantern feedback read snapshots. D's ShipDaySession owns the watch and the single
house board; E adapts its existing callback to C's typed checkbox renderer.

`IPlayerIdentity.ActorId` is assigned to the local rig and A's hands (offline=0).
The 114-byte pose packet carries head, both hands, torso yaw, HandPose and four
analog values. Remote gloves use A's authored skins; E supplies remote arm IK for
A's `Resources/Hands/PF_PlayerBody`. **That body asset is still absent**, so full
body appearance remains incomplete. No primitive substitute is generated.
Round 2 messages use protocol 2; internal Photon room names use `LK_S3R2_` so an
older APK cannot join a matching code and interpret the scene indices differently.
The displayed six-character code is unchanged.

Removed the world DAY/RELAY selectors introduced in 8386dc8. PLAY SOLO stays in
the lobby. The cloned E rig at
`Assets/Prefabs/Network/Resources/PF_ShipLocalPlayer.prefab` disables
`TunnelingVignetteController` / `TunnelingVignette`; the shared source rig was not
rewritten. D's validated bed transition is connected to a short full-screen fade,
House placement and one authority acknowledgement; its headset feel is untested.

## Actual Editor Play Mode evidence

The runner opened the canonical environment lobby, invoked its visible **PlaySolo**
Button.onClick, waited for D activation and the complete world, then captured
`VIEW_BriefingBoard_2p5m` at world `(1.8, 1.6, -7.39)`.
`artifacts/round2/E/playmode.json` records the real callback and all twelve loaded
Shell/Dress paths. The picture visibly shows Day 1, its tasks and the bench wrench.
The long fourth task is clipped; D has a request to shorten it.

![Editor Play Mode: House after Lobby Play Solo, VIEW_BriefingBoard_2p5m](../../artifacts/round2/E/VIEW_BriefingBoard_2p5m.png)

**Evidence scope:** the verification checkout combines E with read-only snapshots
of C/D's uncommitted drafts. Four provisional corrections to C were applied only
there: detach before cross-scene moves, avoid truncating the calibration glyph,
use Unity-null checks for imported lights, and enlarge the heading line box.
These are recorded in `dependency-test-fixes.patch` and requested from C; they are
not smuggled into E's commit. Draft file hashes are in
`dependency-draft-snapshot.json`. This picture does not establish that the
published E branch alone is a complete or shippable game.

| Check | Actual result |
| --- | --- |
| Standalone installed-Unity assembly checks | B core, legacy transport, E runtime and E editor: passed |
| Native Unity networking Edit Mode | **25 passed, 0 failed** |
| Native Unity networking Play Mode | **3 passed, 1 failed**: ship-flow test encounters C's `Ship requires new additive powered/fault bakes.` error |
| Graphics entry/capture | Actual lobby button reached all 12 ship scenes, active Day 1 board and wrench; observer PNG saved and inspected |
| Live two-peer transport / Quest / release stripping / comfort | Not run; no acceptance claim |

The verification clone also logged an Editor SearchDatabase indexing exception.
No runtime error or build gate was suppressed to obtain a passing test result.

## Reproduce after C/D source and scene integration

In the integrated Unity Editor:

1. Run **LK/Ship/Bind lobby and player routes**.
2. Run C's **LK/Ship/Apply Build Scenes** and ship authoring/bake/validation tools.
3. Run **LK/Ship/Capture Session E route**, then inspect the PNG and JSON under
   `artifacts/round2/E/`.
4. Run `LK.Net.EditModeTests` and `LK.Net.PlayModeTests`. The missing-bake failure
   must be resolved before a new APK is accepted.

Exact commands with an already-open Pipeline-enabled Editor:

```sh
~/.unity/bin/unity command menu --path 'LK/Ship/Bind lobby and player routes' --json
~/.unity/bin/unity command menu --path 'LK/Ship/Apply Build Scenes' --json
~/.unity/bin/unity command --detach menu --path 'LK/Ship/Capture Session E route' --json
~/.unity/bin/unity job status JOB_ID --json
```

Owner handoffs: [C](Requests/E-to-C-round2.md), [D](Requests/E-to-D-round2.md),
[A](Requests/E-to-A-round2.md), [F](Requests/E-to-F-round2.md). C owns the pending
lighting/geometry/visual acceptance; A owns body art and physical interaction
acceptance. A complete watch, multiplayer generator contributions, failure/retry,
bed progression and synchronized two-headset play still need integrated execution.
