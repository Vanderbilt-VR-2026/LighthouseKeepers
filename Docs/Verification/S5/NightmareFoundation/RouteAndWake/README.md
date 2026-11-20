# Day 2 nightmare — route, wake and flashlight checkpoint (#144)

The feature remains **OFF**. Source network `LK-S5-NIGHTMARE1 / LK-Lab-Coop-v53`. This is an implementation checkpoint, not completion or a headset test.

- `results/generator-1` and `generator-3` record genuine failed route attempts: the child caught the keeper at the first door. The second attempt already used 2.15 m/s pursuit and a safe-side drop; this exposed the need for a reaction window as well as door clearance.
- `results/generator-4` (Machinist) and `results/generator-lightkeeper` (Lightkeeper) passed from the generator room to outside, using the actual XRI continuous-move provider, character capsule, manual stick, the production stamina/button-sampling path, and actual XRI grip/pull at both doors. Each took about 11 seconds. Both runs were **solo Editor automation**, not a human or two-player run. The fixture staged the scene at the turn and placed the rig at the generator once; it then drove the real route without teleporting. The A button was injected into the same sampling method used by XR polling; this does not establish a physical-controller binding. It held run until exhausted and released to recharge.
- The child now drops 3.2 m toward a safe side, with at least 2 m clearance from each available escape route; if no safe position exists, it stays at the corner. A 2.5 s reaction window precedes pursuit. Walking/chase/late chase defaults are 1.92 / 2.15 / 2.30 m/s; sprint reached 3.07 m/s. These are initial settings, not a difficulty acceptance.
- `results/wake-dry-1` has 13 passing checks: completed watch via station/ship rules, actual bed entry, gaze-gated line, dry return and restoration of the unmodified water simulation. `results/spawn-checks.txt` checks both separated retry positions, capsule clearance, routes and three eye heights. The fixture uses test-positioned rigs and staged later beats.
- `results/follow-1` covers the actual authored child NavMesh path from the F3 landing to F2, with a test-positioned following rig. The path and scene checks passed; the new follow speed is 0.3 m/s.
- The Drowned shader previously ignored additional lights, including the hand flashlight. `screens/bunk-flashlight-off.png` and `bunk-flashlight-on.png` were **looked at**: the child and nearby rail become visible under the actual flashlight from the bunk. `results/lighting-3` used the Quest URP asset in the Editor and reports no Editor shader error. This is not Android compilation or device performance evidence; the sculpt still needs presentation polish. An earlier capture at `(2,8.25,.5)` was behind the F3 stair liner. Its collider correctly blocked the gaze test; retry points were moved to the clear side.
- The 92 focused EditMode test receipts are in `tests/` (including prior campaigns, recovery, Survival, voice and House flood). No full-suite claim.

## Still required

Test a complete two-person gaze/follow/escape with actual pursuit and a caught player, release/regrip rescue, late join and disconnect, and retry. Finish/review the shared catch-lunge and wall-to-ceiling-to-drop animation, light bursts, wet print placement and audio balance. Run a full Day 2 from its start and verify normal Day 3 transition. Only then decide whether to enable #144, build and install without launching. Days 3–5 set pieces remain unimplemented.

## Reproduce

With Unity 6000.3.23f1 open and `EditorUtility.audioMasterMute = true`, run `python3 artifacts/s5/scare-scenes/start.py` and wait for Campaign Day 2. Use Pipeline `run_script` with one fixture at a time and a fresh absolute output path: `GeneratorStartProbe.Begin`, `WakeDryProbe.Begin`, `SpawnProbe.Run` or `FlashlightChildProbe.Begin`. The first three scene fixtures use test setup as described above. Stop Play before starting another.

Quest worn comfort, full co-op, on-device shader rendering and sustained 72 Hz/GPU budget are **NOT VERIFIED**. The existing tool-belt APK remains installed and was not launched.
