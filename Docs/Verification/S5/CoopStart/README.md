# Co-op starter assignment — November 19

The timed generator needs one keeper on the starter and another on START ASSIST. During a ship or pipe assignment, the director could ask one keeper to start it while sending the other to hold the pipe valve, or ask both to pull the starter. Neither crew could start the engine by following both instructions. The host now assigns complementary jobs whenever its current work requires a paired start; the previous job resumes after the engine starts. The solo companion path is unchanged. Network protocol **LK-S5-COOPSTART1 / LK-Lab-Coop-v54**; all players must use the same build.

The watch menu now also tolerates a refresh before its nightmare-skip button has been constructed. An existing menu regression test exposed the null reference while running the broader focused suite.

## Evidence and limits

- [116 focused EditMode checks passed](tests.json) with the Editor muted, including role reversal, host/guest snapshot parity and resuming work after the engine starts. The four new Night 4/5 role cases failed before the fix because the partner was assigned the valve.
- [Two live muted Editors](two-peer-events.jsonl) joined through Photon on Night 4, saw complementary instructions on host and guest, and saw the same running engine after three pulls. Fuel and pulls were staged on the host; this was **not** an XRI controller test, voice check, complete night, or headset test. [Receipt](two-peer-result.json).
- The instruction-only campaign model was updated to hold the assist, use each day's actual radio frequency, and let both workers advance while the starter waits. [Before](model-before.json) and [after](model-after.json) include step transitions and the explicit modeled results. With authored ships enabled, Night 4 co-op and the two-player demo now complete in this model. Night 5 co-op still floods and the solo demo still times out under its assumed travel/action speeds. These are unresolved model results, not proof of human playability or a reason to retune from guessed times. The model does not exercise actual Owen navigation, physical hands, the possession lesson, bedtime nightmare, shared sleep, real headset performance, or the still-unimplemented Night 3–5 scare scenes.
- The nightmare feature flag remains **off** pending its full-night and headset checks. No APK was built or installed for this change.

## Reproduce

Unity 6000.3.23f1; `Assets/WatchDemo/Editor/EditorPlaytestAudio.cs` mutes the Editor at startup and on Play transitions. For the focused suite, use the class filters in [tests.json](tests.json). Run the rules model from the worktree (a nonzero exit is expected while any simulated run fails):

```sh
LK_M3_TIMING_OUTPUT="$PWD/artifacts/s5/campaign-audit/repro.json" LK_SHIP_TIMING=1 /Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity -batchmode --burst-disable-compilation -quit -projectPath "$PWD" -executeMethod LighthouseKeepers.Editor.M3CampaignBot.RunBatch -logFile "$PWD/artifacts/s5/campaign-audit/repro.log"
```

For the live transport fixture, back up and sync the peer worktree first, open both Editors muted, then use a fresh output directory:

```sh
LK_TEST_OUT="$PWD/artifacts/s5/campaign-audit/two-peer-repro" python3 Docs/Verification/S5/CoopStart/fixtures/two-peer.py
```

The fixture stops Play Mode on both Editors. It does not launch the Quest.
