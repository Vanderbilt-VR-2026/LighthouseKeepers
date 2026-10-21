# Sprint 3 consolidated report — 2026-10-21

Fetched `team --prune`. All five latest tips dry-run cleanly from `sprint3/base`
in **B → D → A → C → E** order. All five are already in `integrate/sprint-3`
in that order, with `--no-ff` merge commits; no additional merges were needed.
See [MergeReview.md](MergeReview.md) for exact tips and review findings.
No scene/prefab was hand-merged; pre-existing untracked files were preserved.

**Built:** `Builds/LighthouseKeepers-Sprint3-Networking.apk`, Android ARM64/IL2CPP
development; **Succeeded, 0 errors**, 185,407,384 APK bytes. Archive CRC passed;
all native libraries are arm64-v8a. SHA-256:
`beba32ea7aa21b6fbf2a28d78a888b8c319f3e2ba27f79403cbe087f04cfc59c`.
**Installed:** `adb install -r` returned `Success` on Quest 3 `2G0YC1ZG32048T`
(package `com.lighthousekeepers.game`). Launch request returned `Status: ok` but
opened Quest’s `LaunchCheckControllerRequiredDialogActivity`; no game process
was running after the request. Wake/connect Touch controllers and clear the prompt.
No observed lobby or in-headset gameplay pass is claimed.
The existing networking builder uses the production lobby and inherited Storm
Watch; it excludes Lab/DevGym. C's fixtures and D's new days are not this entry.

The first current attempt compiled but failed IL2CPP with
`System.ArgumentException: An item with the same key has already been added. Key: mscorlib`.
Its inputs included `mscorlib.dll`, `mscorlib 2.dll`, `mscorlib 3.dll` and other
numbered copies. Preserved 340 generated DLL/PDB copies from
`Library/Bee/artifacts/Android/ManagedStripped` under ignored
`artifacts/sprint3-review/quarantined-build-cache`, then retried. No source was
removed. Current attempt evidence: `artifacts/sprint3-review/`.

Review blocks: D requires B's missing `ActivateDayLeak(int)`; E's disabled adapter
fails a review-only symbol-enabled compile with **CS0535**, missing
`ISimulationHost.ForceGeneratorFault()`. A's hand actor IDs are not assigned from
E, B's direct completion-event subscriber bypasses D's gate if composed unchanged,
and remote hand visuals remain placeholders. These prevent claiming integrated
Sprint 3 simulation/progression even when the production APK builds.

## Consolidated headset checklist — lobby to finish

**Expected** is a source prediction for this APK, not an observed headset pass.
**Incomplete** means the named Sprint 3 integration is unavailable. Test both
headsets with the same APK; use Editor host + Quest guest for one-person rehearsal.

1. **Lobby entry — expected.** Wake Touch controllers, clear any controller-required
   prompt, inspect hands, walk/turn and use the physical board's UI ray. Check
   open/fist/pinch response and both-eye text. Comfort/reach remain unverified.
2. **Crew setup — expected.** Host, enter the six-character code on the guest keypad,
   test an invalid code, and compare names/readiness. One ready keeper cannot start;
   two ready keepers allow host BEGIN WATCH. **Incomplete:** a Play Solo lobby route
   and integrated voice. Use party/colocated voice for cooperative testing.
3. **House arrival — expected.** Both load together, see one remote keeper, and keep
   independent tracked cameras. Check head/hand motion and nameplate fading.
   **Incomplete:** remote articulated gloves/hand-pose binding; placeholders remain.
4. **House board and workbench — expected legacy behavior.** Read instructions,
   directly grab the wrench with either hand, inspect rim/hold feedback, carry,
   throw, regrab and test simultaneous grabs. Only one keeper should own the tool;
   holding beyond eight seconds should not return it to the bench. Distant UI rays
   should not grab tools. **Incomplete:** C's new board/bench and D's daily
   checkboxes, narration, Continue/Retry board adapter.
5. **F1 pipe and first route — expected legacy watch.** One keeper braces, the other
   repairs; check progress, spray/drainage, release/regrip and the first stair gate
   on both peers. **Incomplete:** D's 15-second leak, B-authoritative water/day
   completion, hand actor identity composition and pre-repair Day 3 validation.
   Legacy success does not establish removal of the old water plateau.
6. **F1 manual pump — expected legacy control.** Grip and stroke; holding still
   should not pump. Check repeated strokes and recovery after release. **Incomplete:**
   C's new constrained fixture/outlet burst in production and the A/B/D authoritative
   stroke path. In the future Lab integration, test ten full cycles versus partial
   travel/jitter; the authored arc has about 0.191 m vertical travel.
7. **F2 generator and first refuge — expected inherited sequence.** Follow the
   existing board, divide pump/generator work, restore power, reunite and choose a
   shared repair-help/rescue-protection reward. Test release, absent support and
   recovery without losing completed steps. **Incomplete:** C's diesel/plinth and
   fresh powered/red-fault bakes; A's three primes/choke/0.40 m starter sequence,
   fuel reporting and D's 45-second Day 2 fault are not the production loop.
8. **F3 communications and second refuge — expected inherited sequence.** Confirm
   route unlocks, readable instructions and a shared refuge decision. **Incomplete:**
   the new bed-rest/results/day-save flow. Intended bed behavior is repair plus safe
   water before rest; early grips must not bank completion. It is not wired here.
9. **Final watch across F1/F2/F3 — expected legacy cooperative faults.** Check valid
   contributors, setbacks and recovery on both peers. **Incomplete:** D's new Day 3
   isolation-valve sequence, strict different-actor authorization, fresh pressure
   heartbeat and B snapshot replication. Development DebugSoloOverride defaults on
   for E's optional gate; a development run cannot prove strict two-actor acceptance.
10. **Radio to F4 lantern — expected legacy finale.** One keeper holds/transmits
    SUN/WAVE/STAR; the other operates beacon switches without the lantern revealing
    the code. Test a wrong switch, retained completed work, beacon light and victory
    only with both alive at the top. **Incomplete:** D's day catalog has a rest
    ending; it does not supply a new climb-to-top finale.
11. **Flood/rescue when encountered, then finish/retry — expected legacy behavior.**
    Test air, downed state, lifeline aim/release, catch, pull, expiry and both-downed
    loss. Check stationary fade/recovery comfort, result return to lobby and fresh
    retry. Separately disconnect guest then host; reconnect host first with the
    remembered code, then guest, and require fresh readiness. **Incomplete:** live
    two-device validation, host migration and restoration of an interrupted day.
12. **Across the whole walk — unverified acceptance.** Record Quest model/refresh,
    CPU/GPU frame times, draw calls, GC, thermal behavior, network RTT/loss, readable
    boards/reach and full run duration. No ten-minute completion, comfort,
    performance or live two-headset synchronization pass is claimed.

## Reproduce

With Unity closed, from the repository root (licensing pipe belongs to this Hub session):

```sh
mkdir -p artifacts/sprint3-review
/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -nographics -projectPath "$PWD" -buildTarget Android -licensingIpc LicenseClient-w0cUwLXL7i1XUa5XL3n3x -executeMethod LighthouseKeepers.Net.Editor.MultiplayerBuild.BuildQuest -logFile artifacts/sprint3-review/manual-build.log
cat Builds/LighthouseKeepers-Sprint3-Networking.apk.txt
# Only after result=Succeeded and errors=0:
/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb -s 2G0YC1ZG32048T install -r Builds/LighthouseKeepers-Sprint3-Networking.apk
```

With Unity open, use **Lighthouse Keepers → Sprint 3 → Networking → Build Quest APK**.
If the same duplicate-assembly error returns, close Unity and preserve only the
generated numbered DLL/PDB copies before rebuilding:

```sh
python3 - <<'PY'
from pathlib import Path
import re, shutil, time
src = Path('Library/Bee/artifacts/Android/ManagedStripped')
dst = Path('artifacts/sprint3-review') / ('cache-recovery-' + str(time.time_ns()))
for file in src.iterdir():
    if re.search(r' [2-9][0-9]*\.(dll|pdb|mdb)$', file.name):
        dst.mkdir(parents=True, exist_ok=True)
        shutil.move(str(file), str(dst / file.name))
PY
```

Prior same-source evidence recorded in the preceding report: B 22 passed/1 Unity-only
check not run; D 20 scenarios; A 70 assertions; networking Edit Mode 20/20 and Play
Mode 3/3. These were not rerun for this unchanged source. The Play Mode runner used
an offline Fusion session, not a live Photon pair or headset acceptance.
