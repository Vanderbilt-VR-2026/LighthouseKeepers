# Audio pass — phased verification

Branch: `s4/immersion`, draft PR #79. Do not merge. Raw recordings remain outside the build and source files are never modified.

## Before changes — 4 November 2026

Source: `2c6ca0cb71c8763253a72ba73ce78f389ab5a296`; matchmaking `LK-S4-M3-PULL3`.
Local evidence: `artifacts/audio-pass/before/`.

- Graphics solo replay: **PASS**, Days 1–3, actual XRI input injection, pipe repair/drain, generator, beacon, radio, real beacon-dark loss/retry, shared sleep code in solo, and next-day reset. Zero captured errors. This is automation, not human task timing or headset sound acceptance.
- Full EditMode: **239/239**. Full PlayMode: **43/61**. All 18 failure names and logs are in `before/full-suite-summary.json` and `artifacts/immersion/pipe-redesign/tests/full/`. The Photon roster integration test returned `GameNotFound (32758)` this run; it passed the immediately preceding run. The other 17 failures are existing environment/legacy rehearsal/traversal/Storm Watch failures. Do not describe the full suite as passing.
- Live Mac host + Quest guest, room `5HW6W2`, region `us`: user confirmed joining the House, seeing Mac Host, hearing the transmitted chime, and intelligible Quest speech at the Mac. Automated incoming audio sampling alone was near-silent and did not establish intelligibility; the latter is the user's listening confirmation.
- The first live attempt flooded with no completed bolts while coordinating the test. Preserved loss evidence; no timer, flood, or gameplay values changed to bypass it.
- On retry, all three user-operated Quest bolts reached the host's board. Mac voted for bed through its existing interaction path; the user gripped the Quest bed and confirmed the night page and Day 2. The user's subsequent generator start also reached the host (power on, task checked).
- Inspected the solo approach/label image, host completed board, actual stereo Quest capture and failed-watch logbook. The Quest capture at repair completion shows the player already climbing stairs; it is not a screenshot of the board.

## Phase 1 — toward-keeper tightening

Implementation under `ImmersionTuning.pullTowardTightens`. Grip and trigger clutch remain unchanged. Locked angular travel toward the keeper earns progress; away travel freewheels. Progress is retained on release. Visual bolt rotation is clockwise viewed above. Host uses existing fresh keeper pose and wrench lease for guest classification; no new snapshot fields.
Matchmaking version: `LK-S4-M3-PULL4` (both clients must match); snapshot layout remains `LK-Lab-Coop-v5`.

Focused rule tests: **9/9**. Focused Play Mode tests: **8/8**, including both hands from three positions, release/readjustment, fuel handling, narration constraints, wall mounting, wrench gravity/recovery, and network release ordering.
Graphics solo Days 1–3: **PASS**, zero logged errors, including loss/retry, drainage and next-day transition (`artifacts/audio-pass/phase1/solo/`). Inspected seated/mid-pull/turned-bolt screenshots.

The first co-op matrix stopped on a test setup error: after dropping the wrench on F1, the next automated approach started in the House outside the permitted 3 m keeper/hand reach. No gameplay fix was made. The corrected harness explicitly returns the tool to the House bench before each position, approaches it beside the bench, then moves to F1 before locking/repairing. Only the five remaining positions were rerun: **5/5 PASS**, room `J5BRJF`, plus the previously passed left-hand position 0. Every completed guest bolt reached the host. Evidence: `artifacts/audio-pass/phase1/remaining-positions/`.

Phase 1 software checks passed. Changed-build Quest acceptance is reserved for the final combined build. The accepted Quest APK remains `2c6ca0c` / `PULL3` until then. No snapshot fields changed.

The user reduced subsequent verification to focused phase tests plus one quick solo Night 1 check per phase, and one final Mac+Quest smoke check. Stop before Phase 6; Day 3 balance is deferred.

## Phase 2 — recorded radio

17 selected single takes replace TTS with existing GUIDs preserved. Focused clip test passed: 17 mono 44.1 kHz clips, each 1–10 seconds. Offline measurements: two-pass normalization about −18 LUFS; every true peak ≤ −1 dBTP. All three alternatives per line remain gitignored. Chosen takes, reasons, lengths, loudness and switching instructions are in `Docs/Design/RadioLines.md` and `Tools/S4/radio_takes.json`. The importer rejects ≥10-second multi-take inputs. Quick solo Night 1 passed, zero captured errors (`artifacts/audio-pass/phase2/solo/`).

## Phase 3 — music and short stings

Prepared three whole-bar streaming Vorbis loops and three 3.5-second stings. Crisis starts at 82.477 s; alternate sting candidates remain gitignored. Beat grid and chosen cuts: `Docs/Design/Music.md`. Focused tests cover clip presence/streaming, danger selection, short stings and invalid replicated music state. Quick Night 1 passed with zero captured errors. The host chooses mode/transition epoch; guests consume it from snapshot v6. Lobby network version is now `LK-S4-M3-AUDIO1`; earlier APKs cannot join this version. No Day 3 gameplay or difficulty values changed.

## Phase 4 — layered rain

Focused tests passed for exterior/interior/roof streaming clips, four impact types, House/F4/outside zone classification and partial doorway blending. Quick solo Night 1 passed, zero captured errors. Three raw rain files remain untouched; normalized and crossfaded Vorbis copies are shipped. Original synthesized impacts/gusts, source credits, gains, filters and timing are in `Docs/Design/AudioMix.md`. The <400 ms impacts use RMS/peak levelling because integrated LUFS is undefined at that duration. No flood visuals or avatar code changed. Headset listening is pending the final build.

## Phase 5 — voice-first mix

Focused mixer/ceiling checks passed; quiet samples pass unchanged. The first Night 1 check caught Unity's prohibition on a scripted filter sharing a GameObject with both an AudioListener and AudioSource. The fix separates the listener onto a zero-offset child of the same tracked head, without moving the camera/source/hands. The corrected quick Night 1 passed with zero errors. Four-slider UI was exercised in Play Mode: Music changed to .43 and restored to the original 1.0; returning to the logbook worked. Six buses, priorities, attack/release, AGC, saved sliders and the 24-real-voice cap are documented in `Docs/Design/AudioMix.md`. A final focused check passed after adding developer audio telemetry.

Final APK, device performance, listening acceptance and one Mac+Quest smoke check are pending. Phase 6 has not started; all day assets and difficulty tuning are unchanged. No additional position matrix or full-suite reruns were performed after the user's reduced-verification instruction.
