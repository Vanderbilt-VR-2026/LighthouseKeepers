# Sprint 3 daily report — 2026-10-21

Recreated `integrate/sprint-3` from `sprint3/base` (`6f1447b`) after
`git fetch team --prune`. Five clean `--no-ff` merges, in order:
simulation `fd61b60`, progression `f07235c`, hands `37cb8b0`, environment
`2d728bd`, networking `9a9722c`. No merge conflicts or hand-edited scene/prefab
merges. Previous integration is preserved at
`backup/integrate-sprint-3-before-networking-20261021`; both pre-existing
untracked files were preserved.

**Installed and launched:** `Builds/LighthouseKeepers-Sprint3-Networking.apk`,
144,664,183 bytes; Unity 6000.3.23f1, Android ARM64/IL2CPP, development build.
Existing `LighthouseKeepers.Net.Editor.MultiplayerBuild.BuildQuest` returned
`result=Succeeded`, `errors=0`; archive CRC passed. SHA-256:
`30358e0a386afb4add3e29241fce90a02d64325750fb6024ed84455752257e83`.
`adb install -r` returned `Success` on Quest 3 `2G0YC1ZG32048T`, package
`com.lighthousekeepers.game`. Cold launch returned `Status: ok`; the process was
present and its captured 500-line PID log tail contained no matching runtime
exceptions. This is launch evidence, not an observed lobby/headset pass.

The helper builds the production network lobby and inherited Storm Watch,
excluding Lab and DevGym; C's new fixtures remain Lab-only. Two earlier APK
attempts were rejected for 11,086 duplicate TypeDB errors. Restart alone did not
fix them: stale `TypeDb-All 2.json` files in Editor/Player BuildPlayerData and
`Player/RuntimeInitializeOnLoads 2.json` caused duplicate registrations. Moved
those three generated files to the evidence directory's `quarantined-build-cache`
and rebuilt successfully. A separate batch startup hit a licensing timeout;
the successful command borrowed the already-running Hub licensing connection.
Unrelated numbered source duplicates were left untouched and excluded from the commit.

Compilation initially failed on missing Mono.Cecil and Fusion.Runtime references
in two test assemblies. Corrected their asmdefs; recompile reports zero errors.
The existing build preprocessor binds FloodTuning and preserves it for IL2CPP;
Fusion's existing helper bakes its network prefabs. No gameplay scene was rewritten.
Offline checks: simulation **22 passed, 0 failed, 1 Unity-only audit not run**;
progression **20 scenarios passed**; hands **70 assertions passed**, plus serialized
hand payload/reference checks. Four hand/network prefabs' external GUID references
resolve. Unity networking Play Mode: **3/3 passed**, including a real offline
Fusion runner spawning the player prefab without an extra camera and same-actor
controller identity. The override test ran its Editor/development assertion;
it does not establish release-player behavior. Unity networking Edit Mode:
**20/20 passed** (crew codes/readiness, actor gate, leases, pose buffering and
the deterministic latency/loss model). No live Photon pair was tested.
Offline checks are not native interaction, network transport or headset acceptance.

Headset checklist, in encounter order. **Expected-to-work** is a source prediction,
not a headset pass. All steps still require a wearer.

1. **Lobby — expected-to-work:** connect/wake Touch controllers, clear any Quest
   controller-required prompt, inspect both hands, move/turn and point at the
   physical crew board. Host/join through CREW OPTIONS and the six-character
   keypad; invalid codes should show an error. With two keepers, ready both and
   BEGIN WATCH. Shared lobby/pose/grab behavior remains unverified on two devices.
2. **Play Solo — known-incomplete:** there is no standalone Play Solo route in
   this production lobby. BEGIN WATCH requires two ready connections. Development
   `DebugSoloOverride` does not bypass the lobby. For one-person rehearsal, host
   in the same-revision Editor, join from Quest, ready both and begin from the host.
3. **House — expected-to-work for the inherited watch:** after two-player start,
   enter the house, inspect the legacy briefing board, grip/release/regrab the
   wrench and check glove poses and ray buttons. **Known-incomplete:** C's new
   wall board/bench, D's daily objective/Continue/Retry presentation and recorded
   briefings are not connected to production loading.
4. **F1 — expected-to-work for the inherited cooperative pipe/pump controls:**
   have one keeper brace while the other repairs, then check feedback and the
   first stair gate; test release/regrip and recovery. **Known-incomplete:** the
   new 15-second leak, safe-water/day completion, C's physical pump fixture and
   A/B/D authoritative stroke-to-water integration are not wired into this entry.
5. **F2 — expected-to-work for the inherited generator watch:** once the route
   opens, follow the existing board's pump/generator recovery controls with the
   partner. **Known-incomplete:** C's diesel/plinth, physical primer/choke/starter
   sequence, fuel reporting, D's 45-second fault/day progression and new
   powered/red-fault lighting acceptance. E's new B-simulation adapter remains
   behind `LK_SPRINT3_SIMULATION`; merging networking does not enable this path.

No current two-headset run, comfort/reach/readability result, ten-minute completion
or measured Quest performance is claimed. The new DayDirector/backend composition
and Day 3 distinct-actor valve gate remain incomplete; inherited Storm Watch
behavior does not verify those Sprint 3 requirements.

Reproduce from the repository root with Unity open:

```sh
~/.unity/bin/unity recompile --json
~/.unity/bin/unity command --detach menu --path 'Lighthouse Keepers/Sprint 3/Networking/Build Quest APK' --json
~/.unity/bin/unity job status JOB_ID --json
cat Builds/LighthouseKeepers-Sprint3-Networking.apk.txt
# Only after result=Succeeded and errors=0:
/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb -s 2G0YC1ZG32048T install -r Builds/LighthouseKeepers-Sprint3-Networking.apk
```

With this project's Editor closed, this was the successful batch command (the
licensing pipe belongs to the current Hub session):

```sh
mkdir -p artifacts/sprint3-networked-integration
/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -nographics -projectPath "$PWD" -buildTarget Android -licensingIpc LicenseClient-w0cUwLXL7i1XUa5XL3n3x -executeMethod LighthouseKeepers.Net.Editor.MultiplayerBuild.BuildQuest -logFile artifacts/sprint3-networked-integration/manual-build.log
```

If duplicate-cache errors return, close Unity, preserve the three generated copies
outside its cache, then rerun the build above:

```sh
python3 - <<'PY'
from pathlib import Path
import shutil
for relative in ('Editor/TypeDb-All 2.json', 'Player/TypeDb-All 2.json', 'Player/RuntimeInitializeOnLoads 2.json'):
    src = Path('Library/BuildPlayerData') / relative
    dst = Path('artifacts/sprint3-networked-integration/quarantined-build-cache') / relative
    if src.exists():
        dst.parent.mkdir(parents=True, exist_ok=True)
        if dst.exists():
            dst = dst.with_name(dst.name + '.retry')
        if dst.exists():
            raise RuntimeError('Preserved copy already exists: ' + str(dst))
        shutil.move(str(src), str(dst))
PY
```

Current invocation evidence is under ignored `artifacts/sprint3-networked-integration/`.
