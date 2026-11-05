# Audio pass — phased verification

Branch: `s4/immersion`, draft PR #79. Do not merge. Raw recordings remain outside the build and source files are never modified.

## Latest follow-up — mono voice and Day 3 balance

Phase 6 was subsequently authorized and is now implemented. [Day 3 balance, timing table, screenshots and current verification](Day3Balance.md); [mono voice processing and remaining headset check](VoiceEcho.md). Current network version: **LK-S4-M3-D3BAL1 / v8**. The entries below are historical checkpoints: statements that Phase 6 was deferred apply to those earlier builds. Their headset acceptance does not certify the new build.

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

Final calibration follow-up: a lobby source inspection found Unity's initial audio reset stopped the calm loop. Lobby music now resumes automatically, legacy placeholder lobby ambience is stopped, and rising-water ambience uses the Ambience bus. Belt-radio source gain changed .18 → .65 to remain audible against the recorded storm; distance rolloff still makes it quieter than the wall receiver. The relevant focused test and quick Night 1 rerun passed with zero errors. The two-minute scripted audition was recorded at `d4eaecc` before this final calibration: −26.60 LUFS integrated, −6.73 dBTP, 9.80 LU loudness range. It contains injected viewpoints/outage/danger/repair and is not a gameplay timing or locomotion test. Per-bus playback LUFS/true-peak measurements and worn-headset DSP/GPU measurements remain unverified.

## Live co-op and headset feedback

Matching `1a4a7c5` Mac + Quest joined room `C5H67U` in region `us`. User confirmed the grey avatar, host chime and intelligible Quest speech at the Mac. All three user-operated pipe bolts reached the host board. Shared sleep then advanced the host into Day 2; the user saw the transition start. The original inability to sleep was the missing Mac bed vote, not a rules change.

The smoke check caught Photon renaming Speaker objects after linking; the scene routing scan had put voice in SFX_World. The Mac was immediately rerouted through VoiceChat. A persistent speech-component identity now survives renaming; the regression test and mixer/ceiling test pass. Codec changed to 48 kHz / 48 kbps / 20 ms to address muffled voice. The user identified repeating tonal rain taps; three impact clips were regenerated as damped broadband noise at lower source gain. Focused FFT checks show no dominant pitched bin (max-bin fractions .004–.009). Final codec/impact acceptance awaits the updated APK.

A two-minute co-op capture is saved locally, along with board/actual-Quest screenshots. Its first section includes the deliberate test chime; routing was corrected live on the Mac during this session, and the later codec update is not represented in that entire capture. Do not call it an unmodified final-build recording.

Device measurements from `1a4a7c5`, mixed live viewpoints/Day 1–2, 583 sampled reports: GPU median 12.09 ms; main-thread marker median 14.10 ms (includes waits); isolated ceiling callback mean about .034 ms. Real-voice cap 24. The MasterDSP marker reports values too small/variable to establish whole-system audio CPU cost. This is not a fixed-view before/after benchmark or proof of sustained 72 fps. Final-build total audio CPU, per-bus playback LUFS/true peaks, and binaural/HRTF remain **NOT VERIFIED**. Standard head-position panning/attenuation is implemented; no spatializer plug-in is installed.

Phase 6 remains deferred. Day assets, day lengths, flood rates, generator rules and beacon loss tuning have not changed in this audio pass.

## Objective, PA and bunk follow-up (before Phase 6)

Focused checks: shared host-chosen objective round-trip plus malformed-index rejection; PA replay command nonce/revision rules; subtitles for all 17 spoken clips; storm-only music rests bypassed in crisis. All passed in the Editor. Quick graphics Night 1 passed with zero runtime errors: physical pipe interactions, drainage and bed transition into Day 2. A separate presentation capture completes the task programmatically to inspect the short completion animation; it is not evidence of a player repair.

The lamps now mount on a timber headboard. Their physical labels say JOBS REMAIN / READY TO REST / RESTING, and a footplate explains warm lamps and waiting for both keepers. The bed, mattress and pillow move together away from the central post. Shared sleep rules are unchanged. Inspected screenshots show the bed clear of the post and the fixtures attached. House PA replay button moved beside the board so it is reachable rather than hidden behind it.

X/Y or left-wrist glance recalls the seven-second tracker; harbor captions use the same style. Six spatial horns replace the belt radio. Replay last message from a physical button; explicit replay is separate from the once-per-night automatic narration rule. The lobby version is LK-S4-M3-PAHUD1 / snapshot v7 because the host-selected objective index is replicated. Both clients need the new APK/source.

The preceding audio-only build 096b833 was installed and launched successfully (SHA-256 04f97bdcb1a6ca36369697b100ba80552b009810da5c0445a8130dc1fd6a2e7f). It does not include this PA/HUD/bunk follow-up. Final follow-up APK and matching-device smoke are recorded below when completed. No Phase 6 work has been performed.

The earlier two-minute co-op capture measured −28.17 LUFS integrated, −3.39 dBTP and 16.30 LU loudness range, with the live routing/codec caveats above. It is a master capture, not per-bus loudness evidence.

Inspected Editor presentation screenshots: [objective and subtitles](media/hud-objective-and-subtitle.png), [completion strike/tick](media/hud-completion-tick.png), [House horn and reachable replay button](media/pa-speaker-house.png), [bunk mounts and clearance](media/bunk-headboard-lamps.png). Final visual-only replay passed, zero errors.

A sleep-page screenshot caught the tracker remaining visible over the logbook. The presentation gate now hides it during sleep, failure and the deliberately opened Watch Menu. This changes only tracker visibility; sleep/menu/failure rules are untouched.

## Installed PA/HUD build

Source `9f407c3`: repository `MultiplayerBuild.BuildQuest` succeeded, adb install −r succeeded, and Android launch succeeded on Quest serial 2G0YC1ZG32048T. APK `Builds/AudioPass-9f407c3/LighthouseKeepers-AudioPass.apk` (104,967,928 bytes), SHA-256 `c6762397b7b8b05ca7c3076bfd4b2a3a8c10d83a1a57308b0323cc5b38198afa`. This includes the sleep/menu/failure tracker gate. Matching final live Mac+Quest acceptance is pending.

Final 9f407c3 focused tests passed again. The final presentation replay asserts that tracker alpha is zero during sleep; transition to Day 2 passed with zero runtime errors. Inspected [clear sleep logbook](media/sleep-logbook-clear.png). Fresh matching-build Mac host VZRQZA is online in region us; headset smoke result is pending user participation.

## Final matching Mac + Quest smoke — PASS

Both 9f407c3 clients joined VZRQZA, region us, and loaded the Lab with two avatars. User confirmed: “In the House; avatar and announcement clear” and “Clear speech; Mac hears me; pipe done.” Host observed guest bolt counts 0 → 1 → 2 → 3, completed board task, and shared objective changing to rest. Mac Host then voted for bed; the existing both-keepers rule remains in place. No final shared-bed claim until the guest sleeps.

Inspected [Editor remote avatar](media/final-coop-avatar-editor.png), [host board after guest repair](media/final-coop-board-complete.png), and [actual Quest pipe view](media/final-quest-pipe.png). Quest logs show mic permission granted, 48 kHz / 48 kbps / 20 ms voice in us, and persistent VoiceChat routing after the Speaker rename. Startup logged “Voice client is busy” and a few frames arriving before voice-info initialization; speakers subsequently initialized and user-confirmed voice passed. No ongoing Photon connection failure was observed.

Final worn-headset acceptance: avatar, announcement clarity, return voice and shared task **PASS**. Exact HUD angular comfort/stereo legibility, each PA replay button/floor reverb, final full audio CPU/per-bus loudness and the new physical bunk labels remain **NOT VERIFIED** by the wearer. No extra position matrix/full suites, and no Phase 6 balance work.

Shared sleep subsequently **passed**: the guest bed vote registered first. The automated Mac movement/sleep call initially queued a vote before its replicated pose reached the bunk, so host proximity validation rejected it. Resending the Mac vote after pose convergence started the existing shared transition. Host and actual Quest log both report Day 2 (Quest guest sequence 1453, day=2). This was test-driver coordination, not a missing bunk collider; no production sleep-rule change was made.

User reported feedback while MIC MUTED. Device telemetry shows MIC MUTED; the Mac has no remote Speaker after the Quest stops recording, confirming outgoing Quest voice is removed. Mac was immediately muted too; its Recorder reports TransmitEnabled=false, RecordingEnabled=false, DebugEchoMode=false. Acoustic-feedback confirmation is pending; do not claim the feedback complaint resolved.
