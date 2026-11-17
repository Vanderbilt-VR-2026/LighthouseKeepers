# Active checkpoint — November17 fun horror loop

Branch **sprint-5 / draft PR#89**. Editor muted. Accepted body/hands intact. **Install only; never auto-launch Quest.** Network currently **LK-S5-LANTERN1 / LK-Lab-Coop-v35**.

## Done

- Story scope trimmed: four existing notes only; no15-readable expansion. Six visual clues deferred toTier2. Owen **Post-course / stretch #135**. Solo protection no longer depends on an absent companion.
- User's updated recording sheets copied exactly.52 SHEET0 offlineTTS placeholders (45HM/7ANT), exact captions, mono44.1kHz16bit, under10s. Other60 remain catalogued/off unless explicit real-recording import succeeds. No new fake actor recordings. Unsupported Teague/south/missing mechanic triggers stay held.
-54 focused EditMode checks passed; measured audio and scope evidence `Docs/Verification/S5/StoryTrim/`. No new synced fields; no version bump at this content checkpoint.
- Previous lantern APK33c2bf35 remains installed, not launched. Lantern/body acceptance history below.

## Next — Tier1 in this exact order

1. Hunter: inspect actual floor/stair navigation; single Walter rig, hostFSM, LOS/light observations, telegraph+silence. No catches enabled until escape/hiding/recovery tested. Add `HorrorTuning` as one source for all new horror numbers; then migrate existing lantern/fear/sabotage consumers without duplicated knobs. Refs#105,#113.
2. Sprint/hide/breath (#105,#106); defenses (#107); caught/revive (#107); fear (#93); sabotage/evidence/physical repairs (#98–100); ship-window attack (#95,#99); night data/telemetry (#109,#110); exactsix-minute Demo (#116).
3. After ALL Tier1: Tier2 apparitions,6visual clues,privateANT,scareoptions,morninglogbook. Tier3doors/mirror/save/credits only iftime beforeNov29. Owen outside course scope.
4. At every completed Tier1 checkpoint: build clean source, install ifADB present without launching, inspect evidence, commit/push with issue reference and keep headset-only acceptance open.

Exact commands: `python3 Tools/Story/build_catalog.py --check`; `python3 Tools/Story/build_placeholders.py --verify`; scope test driver `python3 artifacts/s5/loop/test_scope.py`. Unity bridge helpers in `~/LighthouseKeepers-Coop-Basics/artifacts/coop/mac_pair.py`; explicit `EditorUtility.audioMasterMute=true` and72fps before eachPlay. Fresh build backups/output required (previous lantern scripts are reference only).

## DEVICE CHECKS PENDING

- No fresh hunter build yet. Last installed33c2bf35: lantern wick/oil/hooks, seated reach, stereo glass, body/Brecall, twoHIGHlamp GPU≤11ms/CPU≤9ms at72Hz.
- Matchingv35 Mac+Quest: guestfuel/refill/valve/rejoin/sharedsleep and humanvoice. MutedEditor tests do not prove voice clarity.
- NewTTS intelligibility/timing needs listening after the next build; no misleading Owen/direction cues. Fullhunter/hiding/catches/privateANT remain implementation work.

---

# Active checkpoint — November16 Kettle Rock oil lanterns

Branch **sprint-5**, draft **PR #89**, worktree `~/LighthouseKeepers-M3`. Main untouched. Editor muted/capped72fps. Accepted body/hands retained. **Never auto-launch Quest.**

## Done

- Prior story slice: approved canon,112 exact script/caption rows + recording sheets, four physical readables, canonical ship names and recording-aware host PA hooks. Old performances remain until new files arrive.
- Two host-owned oil lanterns from Night2/Demo: grip handle, trigger OFF/LOW/HIGH,90sHIGH/180sLOW, watch/sight oil gauges, shared one-use tilted oil can on F2. Seven supported hooks across House/stations, gravity/20s recovery. Two shadowless lights maximum, no new realtime wall lights.
- HIGH clear held aim2s spends15%oil and repels the current non-contact apparition20s. Full hunter/light-protection/LOW slowing is still pending.
- **LK-S5-LANTERN1 / LK-Lab-Coop-v35** for new shared fuel/wick/frame/action. Both clients update.
- 67focused EditMode +1PlayMode guard,5physical XRI cases, actual soloNight1→Night2 and live two-Mac shared repair/sleep/lanterns pass. Evidence `Docs/Verification/S5/Lantern/`. New recordings, human audio and worn-device performance not claimed.

- Clean **33c2bf35** APK built/verified/installed on Quest3, hash matched; **not launched**. Receipt `Docs/Verification/S5/Lantern/install.json`; headset route `Headset.md`.

## Next / exact commands

1. Delivery complete. Do not rebuild unchanged source. Prior build commands for reference only: `artifacts/s5/lantern/build-fixed.py` and `install-fixed.py` used `artifacts/s5/lantern/rebuild/`. Future changes require NEW backup/output folders, clean source, closed Editors and APK hash verification. Do not reuse that backup or overwrite the installed binary with an older build.
2. Next gameplay slice: physical torn-cable/radio-wire repairs + evidence and prerequisites. Read `Docs/Story/01-Story-Bible.md`, `Implementation.md` and Sprint5-Master relevant phase. Keep accepted coexisting tasks/player choice; never reintroduce solo clamp or trigger wrench.
3. Then live witness/light observations → navigating hunter + reachable hiding → catches only after escape tests. One entity, three opaque wet forms; Lottie apparitions only. Owen must exist/see player before enabling any solo lines about Teague.
4. Remaining15 lore props/read flags, ANT private spatial playback/captions, full narrative encounters/Demo/finale and recordings. Actor files still needed: exact112 takes in `Docs/Story/03-Recording-Sheets.md`. Import menu accepts lowercase IDs, keeps old GUIDs. Incompatible two-wheel/compass instructions stay inactive.

Commands: `python3 Tools/Story/build_catalog.py --check`; focused filters `LanternTests`, `DrownedTests`, `M3RuntimeTests`, `VoicePassTests`, `StoryImportTests`. PlayMode `HandLanternsSurviveTheLightBudgetGuard` asynchronously via bridge. `artifacts/s5/lantern/final-check.py` and `crew.py` need fresh evidence directories. Peer backup under `artifacts/s5/lantern/peer-backup/`.

## DEVICE CHECKS PENDING

- Latest **33c2bf35** lantern APK is installed; do not launch automatically. Worn72Hz GPU<=11ms/CPU<=9ms with two HIGH lamps, ship/entity/peak storm; budget-first per-vertex fallback if needed. No device-equivalent claims from Editor.
- Night2: grip either House lantern, trigger LOW/HIGH/OFF; check stereo glass, warmth and wrist oil bar; short/seated reach and both hands.
- Hang at F1/F2/F4 hooks, release into water/onto floor; test20s/unreachable recovery. Oil can beside F2 engine log: tilt over filler; one refill shared/night. Walk over dropped can/lantern without head pop.
- Matchingv35 Mac+Quest: real voice/radios, guest wick/fuel/refill/drop, Night3 valve, rejoin, shared sleep. Existing two-Mac tests are not a worn headset acceptance.
- Recheck4paper readability/grip, B recall/objective height and accepted body. Arm-swing running remains user-unconfirmed.
- Full hunter/LOS/hiding/Owen/private ANT and new voice listening remain implementation/recording work, not merely device tests.

---

# Active checkpoint — November16 Kettle Rock first story slice

Branch **sprint-5**, draft **PR #89**. Main untouched; Editors muted and capped72fps. User accepted body. Never auto-launch Quest.

## Done

- Canon,112 exact voice rows (HM89/ANT23), recording sheets and stable additive cue catalogue preserved in `Docs/Story/`. Core speech/witness safety rules tested.
- Supported new-recording triggers connected to existing host PA and subtitles, original legacy receipts preserved. Missing performances keep existing audio; no new actor recordings installed. Ordered calm confession/rule/recovery hooks are ready for files. No Owen/two-wheel valve/missing hide instruction falsely enabled.
- Four physical story papers, either-hand pickup, drop/recovery and shared ownership; actual ship names from the bible. New cue/prop identity **LK-S5-STORY1 / LK-Lab-Coop-v34**.
- Focused checks pass; actual-XRI solo Night1 pipe/preview ship/sleep→Night2 passes. Two-Mac avatars/crouch/wrists, guest note pose/release, new-cue exact captions (silent test fixture), shared pipe/sleep→Night2 pass. Eight both-hand paper cases pass. Screenshots personally inspected. `Docs/Verification/S5/Story/`.
- Built clean **3e6867b7**, verified and installed `Builds/LighthouseKeepers-Sprint5-Story-3e6867b7.apk`; installed hash matches. **Not launched.** Receipt and headset checklist in `Docs/Verification/S5/Story/`. PR89 updated; issues100/122 remain open for unfinished scope/recordings.

## Next / exact commands

1. Delivery complete; do not rebuild/reinstall unchanged source. Catalogue check: `python3 Tools/Story/build_catalog.py --check`. For future builds adapt `artifacts/s5/story/build.py` to a NEW backup/output folder, close Editors, build clean source, verify/install via adapted `install.py`; never auto-launch or reuse a backup silently.
2. Next gameplay slice: lantern/oil and physical cable repair. Read `Docs/Story/01-Story-Bible.md` §§3–4 and `Implementation.md` first. Extend existing host WatchState/WatchSnapshot/CoopAction validation and bump protocol for new fields. Keep accepted grip-only pipe and physical generator intact.
3. Connect actual keeper LOS/light observations; then escape-verified hunter+hiding before enabling catches. Next Owen (no voice), remaining props/read flags, private ANT adapter, encounter schedule/finale/Demo. Do not call current apparition a hunter.
4. New recordings go in gitignored `Recordings/Radio/hm_*.wav` / `ant_*.wav`; menu **Lighthouse Keepers → Import Radio Recordings**. Exact112 performances in `Docs/Story/03-Recording-Sheets.md`; human recordings are still needed. Conflicting valve/compass phrases are documented/inactive.

Repro tools: muted bridge `run_tests` individually with `StoryImportTests`, `StoryRulesTests`, `VoicePassTests`, `M3RuntimeTests`, `DreadPlaytestTests`. Solo driver `artifacts/s5/keeper-fix/SoloNight1.cs`, untouched lobby PlayMode; crew driver `python3 artifacts/s5/story/crew.py` (new output directory on rerun), peer backup `artifacts/s5/story/peer-backup/`. Do not treat zero-test/failed-fixture attempts as coverage.

## DEVICE CHECKS PENDING

- Latest APK3e6867b7 installed, not launched. Worn72Hz GPU<=11ms/CPU<=9ms not measured. See `Docs/Verification/S5/Story/Headset.md`.
- Grip/read all four notes with either hand, short/seated reach, stereo readability in blackout, release/gravity/recovery. No rig head pop from paper collision.
- Matchingv34 co-op: partner note ownership/drop, valve, real voice listening/radios, rejoin and shared sleep. Two-Mac Night1 did not exercise the Night3 valve gate.
- Accepted body retained; arm-swing run/comfort remains unconfirmed.
- New HM/ANT listening, exact word alignment, private ANT captions, Owen, hunter/hiding,19-prop/full story and Demo acceptance remain pending implementation/recordings.

---

# Active checkpoint — November 16 keeper headset correction

Branch **sprint-5**, draft **PR #89**, worktree `~/LighthouseKeepers-M3`. Main untouched. Editors muted; **never auto-launch Quest**. Network remains **LK-S5-DREAD1 / LK-Lab-Coop-v33**; no new synced state.

## Done

- Reproduced the headset startup-height failure: guessed floor stays airborne after a transient HMD height. Local skin now takes the locomotion floor, waits for tracked/stable height, scales to standing stature and preserves crouching. Sleeves follow actual glove wrists; original tracked hand rotations/inputs unchanged.
- Identified the flat "board" as the legacy **Stabilized chest proxy**. Disabled in the shared prefab, authoring source and old scene overrides; no new avatar mesh change needed.
- Arm-swing running: late hand binding, empty squeezed grips allowed, normal reversal pauses accepted, direction follows head yaw. Existing +60% easing and no run while holding props/standing retained.
- 27 focused EditMode +3 PlayMode checks pass. Inspected real-lobby graphics at1.35/1.65/1.9m eye height, turns and crouching. Solo Night1 physical pipe/sleep→Night2 passes. Two-Mac avatar/movement/shared pipe/shared sleep→Night2 passes with test Editors capped72fps. First uncapped attempt disconnected; no networking code changed. Human voice listening unverified (muted).
- Evidence: `Docs/Verification/S5/KeeperFix/`. Diagnostic630fe719 was not a fix. Correction **ad642404** built clean and installed/hash verified on Quest3; not auto-launched. APK `Builds/LighthouseKeepers-Sprint5-KeeperFix-ad642404.apk`, SHA-256 `dcf431c8e059faf6bc31656bcdc2164377d0b41b614b9aa3752991603d8b187c`. User subsequently confirmed “body looks good.” Arm-swing running, stereo edge cases and profiling remain unconfirmed.

## Next actions / exact commands

1. Corrected APK is already installed and PR89/issue129 updated. User launch/acceptance is pending. Quest dropped from ADB after installation; installed hash had already been verified. Never auto-launch.
2. Read answers to the two pending questions (body fit, then running in House). If USB returns, inspect `artifacts/s5/keeper-fix/quest-live.log` for `KEEPER_BODY_FIXED` and `ARM_SWING_RUN`; restart capture if its process exited. ADB: `/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb devices`.
3. Record the actual headset outcome in `Docs/Verification/S5/KeeperFix/`; issue129 stays open for device acceptance/profiling. If another correction is needed, rerun only affected focused tests and use a NEW backup directory/output for `artifacts/s5/keeper-fix/build-correction.py`. Reinstall verified APK with `python3 artifacts/s5/keeper-fix/install.py`; never silently reuse the diagnostic build.
4. Actual hunter/navigation/hiding and remaining campaign polish are still unfinished. Do not expand gameplay while this regression is under review.

## DEVICE CHECKS PENDING

- User opens correction, stands naturally in lobby for2s, looks down: no flat proxy board or lifted legs; sleeves attach to original gloves.
- Raise each hand, crouch/stand, turn, recenter, restart. Standing must not look crouched. Check seated start then standing refit.
- PLAY → HOST / PLAY ALONE → START. In the House, empty hands, hold left stick forward and swing arms alternately; squeezing empty grips is okay. Test while facing another direction; speed eases back when swings stop. Run is not active in lobby.
- Original watch/hands/wrench grip intact; two matching clients check remote body, shared task/sleep/rejoin and human voice.
- Worn72Hz GPU≤11ms/CPU main≤9ms and stereo comfort remain unverified.

---

# Active checkpoint — November 16 keeper body

Branch **sprint-5**, draft **PR #89**, worktree `~/LighthouseKeepers-M3`. Main untouched. Editors muted; never auto-launch Quest. Network remains **LK-S5-DREAD1 / LK-Lab-Coop-v33** (avatar adds no replicated fields).

## Completed

- Previous cleanup/horror pass committed and pushed (`745410f8`): the photographed F4 lamp/cables removed at source; brief animated encounters, gravity/recovery and the other accepted corrections. Verified Dread APK and evidence: `Docs/Verification/S5/Dread/`.
- New reference keeper authored in Blender: historical proportions/canvas overalls plus navy coat, cap, scarf, grey hair and belt pouch. Editable source `SourceArt/Avatars/HarborKeeper.blend`; builder `Tools/Avatars/build_harbor_keeper.py`. Original Will fisherman preserved as fallback.
- Both network appearance slots use the new full-body skin. Local tracked tan hands/rotations unchanged; VR has a separate coat/arm/leg skin without duplicate head/gloves. Planted boots, turn steps, crouching knees, slope orientation and landing/teleport reset reuse the established solver.
- Verified: baseline14, final37 focused checks; graphics front/side/back/crouch/walk/landing/local look-down. Final local-body asset recheck6/6. Live two-Mac join/new skins/remote crouch+wrist/guest pipe/shared sleep→Night2 PASS. Night1 does not require the co-op valve; this is not a new valve-gate test.
- Actual-XRI solo Night3 all stations/two steered ships/shared progression toNight4 PASS, zero errors. Editors muted; human voice listening not verified. Screenshots inspected. Receipts: `Docs/Verification/S5/Keeper/`.

## Delivery

**APK built from clean `7c02e858`**, Unity6000.3.23f1 development IL2CPP ARM64; zero build errors. `LighthouseKeepers-Sprint5-Keeper-7c02e858.apk` (123,874,874 bytes), SHA-256 `909c0a37aabf81baed334ad6ed041114cb7a1dc10dff9c349e2108c82679bc2e`. VR manifest/ARM64/signature/ZIP verified. Installed on the connected Quest 3; installed APK SHA-256 matches. Not launched.

Source commit `7c02e858` is pushed. The subsequent test/documentation commit updates the intentionally shared costume assertion and delivery receipts; it does not change player code/assets.

## Next actions

1. Draft PR89 and issue129 updated with the receipts; issue stays open for headset acceptance. Nothing merged to main.
2. Latest keeper APK is installed and awaits user launch. Follow the device list below; never auto-launch.
3. Remaining hunter/navigation/hiding and campaign polish stay pending; apparition work is not a finished hunter. No further gameplay expansion in this avatar pass.

## DEVICE CHECKS PENDING — keeper body

- Latest verified APK installed; user launches. Stereo, feel, performance and human co-op checks remain pending.
- Two matching headsets: friend full body/head/arms/legs; wave/grip each hand, crouch, walk/backstep, turn, stairs and rejoin. Same networkv33.
- Look down: coat/arms/legs, no head or extra gloves; verify waist/sleeve fit in stereo. Confirm local hand alignment, wrench/grab and watch remain correct.
- 72Hz GPU≤11ms/CPU main≤9ms with both skins, plus sustained ship/storm. Mesh counts are not profiling.
- Preserve prior pending darkness, arm-swing run, flare, water/can, F4 floating-fixture sweep, voice and shared-bed human checks.

---

# Active checkpoint — November 16 brief scares / physical cleanup

Worktree `~/LighthouseKeepers-M3`, branch **sprint-5**, draft **PR #89**. Editor muted. Never launch Quest automatically. Main untouched. Network **LK-S5-DREAD1 / LK-Lab-Coop-v33**, both clients update.

- Implemented: pictured F4 fixture and all exposed lantern cable loops removed at source; floors restored after pole removal; single bench flare; fuel-can gravity/8s recovery; front-facing wheel needle; arm-swing run; brief host-selected lunge/watcher/ceiling poses; darker lighting/voltage sags; music retained beneath speech; three sparse placeholder narration cues. Details: [DreadPlaytestPass.md](DreadPlaytestPass.md).
- Verified: 89 focused checks; graphics both-hand can drop/recovery; final wiring audit zero unexpected contacts; solo Night3 stations/two ships/sleep toNight4. Two-Mac guest flare/repulsion and host-owned dropped-can/recovery pass. Evidence: `Docs/Verification/S5/Dread/`. APK built from clean `745410f8`: `Builds/LighthouseKeepers-Sprint5-Dread-745410f8.apk` (125,085,198 bytes), SHA-256 `5c1013e6daaf505678ac571c92757dcc605803f9ddd8bb8812a22c4ba77ff739`. ARM64/VR manifest/signature/ZIP verified. ADB empty: not installed or launched.
- Latest `adb devices` has no headset. Do not wait or launch it. Build a clean-source APK; install if it returns.

## Next: new keeper avatar (user requested AFTER this pass)

References: `/Users/aneeshvasamreddy/Downloads/lighthouse keeper reference.jpeg` and `/Users/aneeshvasamreddy/Downloads/300px-LighthouseKeeper.webp`. Human workwear proportions from photo; stylized navy coat/cap/scarf/leather equipment from illustration. Author in Blender. Reuse Will's existing `CrewHumanoidAvatar`/51-bone rig export, improve inferred planted feet/knees/stairs, retain local tan hands and existing pose transport. No real leg tracking claim. Preserve existing art as fallback; inspect Blender + Unity front/side/back and co-op views. User explicitly authorized body changes with these new references.

Blender: `/Users/aneeshvasamreddy/Applications/Blender.app/Contents/MacOS/Blender`. Existing art builder: `tools/Avatars/build_reference_fisherman.py`, export `export_fisherman_skin.py`; existing procedural footsteps/IK: `Assets/Scripts/LK.Net/CrewHumanoidAvatar.cs`. Build on these rather than replace networking.

## DEVICE CHECKS PENDING — current pass

- Install latest APK without launching; user starts it.
- Worn-headset F4 door/wire sweep; fuel can both hands/drop/8s return; one flare on bench; wheel pointer in front; restored floors/no pole.
- Alternating free arm swings while moving: +60% run, no run from standing waving or holding tools; comfort and collision unchanged.
- Brief encounters/ceiling visibility in stereo, Sightings Off, no face intrusion; darkness/stairs readability, radio/music balance and TTS placeholders.
- Matching v33 Mac+Quest/two-Quest join, human voice, guest interactions/shared sleep/rejoin, sustained72Hz CPU/GPU profiling. Editor evidence cannot certify these.

---

# Current checkpoint — November 16 demo/choices pass

Worktree `~/LighthouseKeepers-M3`, branch **sprint-5**, draft **PR #89**. Editors muted; do not auto-launch Quest. Main untouched. New network identity **LK-S5-CHOICES1 / LK-Lab-Coop-v32**: both players update.

## Completed this pass

- Three original low-poly Drowned forms (hag, soaked doll, gaunt creature), still non-contact apparitions. Height-aware placement, common state/comfort rules. Existing stairwell reveal mechanism retained.
- Original held flare pistol: one round per pistol/night, host-owned trajectory/impact/ammo, guest grab lease, wall obstruction, apparition retreat. Replaces planned multiple active defenses. Fixed pre-compression sample limit that silently dropped larger impact snapshots; compressed wire budget stays 4 KiB.
- Generator cage/tank/engine rebuild preserving all accepted controls and grips: 12,144 triangles, three meshes. Six physical PlayMode checks pass.
- Supplied demo soundtrack source + edited loops **committed to GitHub as requested**. 0:40–1:10 tension, quiet bed, crisis section, ~1:11 short scare hit. Same music decks/ducking; original score remains a switchable fallback. Sources/analysis: `Docs/Design/DemoSoundtrack.md`.
- Night 1 unchanged; Nights 2–5 + Demo open with their required jobs already broken. Choose pipe or generator first; electrical dependencies still matter. No completion-triggered opening faults. Updated contradictory opening announcements without altering recordings.
- Fixed weather fast-forward by decoupling absolute animation clocks from intensity. Darker reflective puddles, eight on the supported gallery deck. Removed unsupported gallery lamps and their cable branches at source; 13,262-triangle wiring rebake.
- 85 final focused tests pass. Actual-XRI solo Night3 completes stations, steered ships and bed intoNight4 (zero errors). Live two-Mac late join/shared form/guest flare/repulsion and subsequent state delivery pass. The first blocked-shot fixture was corrected to choose a supported clear firing position; wall collision preserved. Screenshots inspected. No full-suite or first-timer timing claim.

## Delivery

**Built and installed from clean source `93acb696`. Not launched.** Unity 6000.3.23f1 IL2CPP ARM64; build success, VR manifest/signature/ZIP verified. Installed APK hash matches. APK: `Builds/LighthouseKeepers-Sprint5-Choices-93acb696.apk` (125,711,650 bytes). SHA-256: `0acba1e7867b3e1c25a9f205ca5f781ac5ce3a6002c473713598d75e7adca9c5`. Receipts: `Docs/Verification/S5/Choices/`. Later documentation commits do not change this APK source.

## DEVICE CHECKS PENDING

- Installed build awaits user launch and worn-headset checks; no automatic launch.
- PLAY → HOST / PLAY ALONE → Night2/3. Choose either pipe or generator first; confirm no new job breaks just because you repaired one. Night1 remains tutorial.
- B objective recall, watch/hands, both-hand flare grip/trigger/drop and one-round limit. Shoot an apparition from a clear line; walls should stop the flare.
- Three forms in blackout/stereo, liked F4→F3 reveal, Sightings Off; no face intrusion.
- Generator controls/reach and visual acceptance; current geometry is original stylized low-poly, not a purchased photoreal asset.
- Gallery/outdoor puddles dark and grounded, no dangling exit lamp/wires. Ship view: storm intensifies smoothly without time-speed jumps.
- Listen to the supplied sample's loops/scare transition and radio/voice clarity. Editors were muted, so human listening remains unverified.
- Worn Quest72Hz GPU≤11ms / CPU main≤9ms; matching-build Mac+Quest/two-Quest voice, task, wheel, sleep and rejoin.

## Next after this correction pass

Actual hunter/navigation, hiding/recovery and remaining campaign finale/progression polish are unfinished. Do not describe the apparition layer as a completed hunter. Flare pistol is the only requested active defense; do not add lantern/bell/trap defenses. `timingApproved=false` until human first-timer acceptance.

Reproduce from this worktree: `caffeinate -i python3 artifacts/remaining/m3/tests.py NEW 'TowerWiringTests;QualityBaselineTests;M3RuntimeTests;AudioPassTests;DrownedTests;DrownedHorrorTests;VoicePassTests;FlareTests;LatestSampleBatchTests'`; graphics `artifacts/s5/finish/choices-night3.py`, `weather-review.py`, `crew-driver.py`. Always use fresh output directories and keep both Editors muted. Peer changes were backed up before synchronization. Build from a clean commit using `artifacts/s5/finish/build.py` with a fresh backup directory.

---

# Current checkpoint — November 15 visibility corrections

Worktree `~/LighthouseKeepers-M3`, branch **sprint-5**, draft **PR #89**. Nothing merged to main. Editor muted throughout. Skybox and avatar body unchanged.

## Completed and delivered

1. **Drowned visibility:** host picks a supported, unobstructed nearby position; a spatial warning precedes the quick reveal when the selected keeper looks toward it. Face/head/arm movement is more visible. Night 1 and the first minute remain protected. Still a non-contact apparition; hunting/hiding is unfinished.
2. **Ships and wheel:** approach 45 s, passage 30–35 s, departure 25 s; authored danger-window start times preserved. West-gallery pedestal faces the fairway. Old lever/plate occluders removed; softer beam haze keeps the ship visible. Miss/reacquire and crew preparation rules retained.
3. **Generator:** exposed cylinder heads, alternator, fuel/exhaust plumbing, tank and narrow supported control rail. Accepted physical inputs/grips preserved. 9,480 triangles in three machinery meshes; no added lights/colliders.
4. **Verification:** 53 reveal/voice/quality + 35 ship/data + 6 physical PlayMode cases pass. All 12 workload models pass. Actual-XRI solo Night 3 passes both ships and sleep into Night 4. Live two-Mac shared reveal/retreat and guest physical wheel slip/recovery/pass pass. Screenshots inspected; no fresh full-suite claim.

**Installed APK: source `3bbc4030`, network `LK-S5-SCARE3 / LK-Lab-Coop-v30`. Both clients must update.** Built clean with Unity 6000.3.23f1, IL2CPP ARM64, zero build errors. Installed SHA-256 matches the verified APK. **Not launched**, respecting the standing instruction.

APK: `Builds/LighthouseKeepers-Sprint5-Visibility-3bbc4030.apk` (125,075,834 bytes). SHA-256: `98f9656607eb840cd98d710db1fc2250de5a0a556b9fadd6b3ca1a8f7fa6f6af`.
Evidence and receipts: `Docs/Verification/S5/Visibility/`.

## DEVICE CHECKS PENDING

- Night 2: warning direction, visible reveal, comfort, speech clarity and power-restoration retreat; Sightings Off.
- Night 3: west-gallery wheel/ship visibility and reach, deliberately miss briefly then reacquire; faster passage pacing.
- Generator control reach/readability and physical fuel/primer/choke/cord sequence.
- Worn Quest 72 Hz: GPU ≤11 ms / CPU main ≤9 ms; same-view apparition/beam/generator comparisons. Triangle counts are not GPU evidence.
- Matching-build Mac+Quest/two-Quest voice, valve, guest wheel, shared sleep and rejoin. Editors were muted; audible conversation is not verified this pass.
- Preserve pending offline, wall/edge, stereo text and first-timer workload checks. Demo model is tight (5.9 s spare); no human timing approval.

## Resume

Current requested corrections are delivered; wait for headset feedback. Remaining master brief: repair cable/lens work (#99), concise sabotage explanations (#100), lantern (#101–103), then actual hunter/hiding/escape (#105–108). Keep `timingApproved=false` until human acceptance. No auto-launch on Quest.

Latest reproducible scripts/receipts: `artifacts/s5/playtest-nov15/` (build.py, solo-night3/, two-mac/, ship-beam-soft/); focused runner `artifacts/remaining/m3/tests.py`. Use fresh output folders if rerunning. The delivered APK is tied to 3bbc4030; later documentation commits do not change its source.

---

# Archived checkpoints — superseded by the current checkpoint above

# Sprint 5 checkpoint — November 14

Worktree `~/LighthouseKeepers-M3`, branch **sprint-5**, draft **PR #89**, base main after teammate merge of PR88. Main-folder branch untouched. Editor muted. Never launch Quest without request; installing is allowed.

## Latest device delivery — November 14 evening

Installed `LighthouseKeepers-Sprint5-Horror-029f9ae9.apk` on the connected Quest3. The installed APK SHA-256 matches the verified local build. **Not launched**, respecting the standing no-auto-launch instruction. Worn-headset listening, comfort and GPU checks remain pending. This supersedes the earlier “ADB empty / not installed” build-time notes below. Receipt: `Docs/Verification/S5/DrownedHorror/install.json`.

## Current follow-up — Drowned horror presentation

User requested creative liberty with frightening effects/music/sound. Design: `Docs/Design/DrownedHorror.md`. Implemented and pushed in steps:
- Sound: six original mastered clips, spatial wet approach/breath/reveal/retreat, bowed-metal score reusing the two existing music decks. Voice-first attenuation, no old stings on re-enable/late join, retry receipts clear. Original teammate voice/music untouched.
- Visuals: gaunt crooked oilskin figure, wet probe sheen/slow head lift, low mist/drips. 2,131 body+68 effect triangles; no new lights. Revised the face after screenshot review.
- Checks: baseline51/51; final focused62/62. Graphics Night2 outage/appearance/restore/Off/duck pass. Actual-XRI soloNight3 passes pipe, generator, beacon, radio, two steered ships, lens wipe and sleep intoNight4. Live two-Mac host/late-join/manifestation/storm/sound/score/restore/retreat pass. Standing/seated face-height exclusion fixed and tested. Screenshots inspected. Evidence `Docs/Verification/S5/DrownedHorror/`.

Network stays **LK-S5-DROWNED2 / LK-Lab-Coop-v29**. No new synced fields. **Delivered APK built from clean029f9ae9**, VR manifest/signature/ZIP verified, zero build errors. Not installed: ADB empty; not launched. Editors muted throughout. Resume master-brief #99/#100 → lantern → hunter/hiding; this is still the non-contact apparition slice. Never launch Quest without request.

APK built from clean **029f9ae9**, IL2CPP ARM64, zero build errors: `Builds/LighthouseKeepers-Sprint5-Horror-029f9ae9.apk` (125,069,482 bytes). SHA-256 `ed2175ecfd0b62c15eef1eefbb9ee92825a7d2e77ef6660f5e888659dafbaafc`. VR manifest, signature and ZIP checks pass. **Not installed: no Quest is visible to ADB. Not launched.** Network remains LK-S5-DROWNED2 / v29.

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-M3
# Use a new output name for any rerun.
caffeinate -i python3 artifacts/remaining/m3/tests.py horror-NEW 'DrownedHorrorTests;DrownedTests;TensionDirectorTests;AudioPassTests;KeeperFearTests;VoicePassTests;QualityBaselineTests'
caffeinate -i python3 artifacts/s5/drowned-horror/night3.py
caffeinate -i python3 artifacts/s5/drowned-horror/crew-appearance-driver.py
# Both graphics scripts must use fresh result directories on a rerun.
```

## Latest requested playtest corrections

- `57e0a6d6`: full steamer hull remains visible from horizon to shore; removed the grey proxy/floating lights. Same11,760-triangle/two-material hull at distance; no realtime ship lights. Reviewed normal-LOD gallery images.
- `5fb7fe4c`: the F4 wheel now aims the actual beam at the moving ship. Host requires powered/lit/clean lens, within8°, held steady0.6s. Off-beam pauses safe travel;30s grace then named wreck. Reacquire to continue. Night1 remains a safe preview. Task/director and arrival times reconciled;72 focused tests,12 workload models, actual-XRI soloNight3 plus live guest steering pause/recovery/pass. See `Docs/Playtests/2026-11-13-ship.md`.
- `bd3e8bca`: original rounded diesel castings/tank, ribbed alternator, flywheel, exhaust and fuel plumbing.10,072triangles/three combined meshes, no added lights/colliders. Accepted physical controls/grips unchanged; actual-XRI Night2 full start passes. Reviewed front/oblique/both-grip pictures.
- `f9aa684a`: watch resin case/bezel/LCD/four side buttons; **dial/text rotated180°**, hand and mount unchanged. Focused watch geometry/face/backlight test passes; three accessory views inspected. Headset wrist feel pending.

Evidence: `Docs/Verification/S5/Ship/playtest/`, `Docs/Verification/S5/Props/`.

## November 14 — Drowned / weather pass

- Fairness fix3562928f: sabotage enabled; bounded120s offshore crew wait; stable wheel keeper assignment.12 models pass (Night5 duo137s spare).
- Visible non-contact Drowned: host warning/manifestation/retreat, light/repair repel it, no first-minute/Night1/new-control-grace scares, bounded sightings and local opt-out. Storm follows the phase; independent day-only rain/ocean intensity removed. No hunter/catch/stun claimed.
- Heavy exterior rain replaces particle scan with1600 GPU-animated streaks.12 supported outdoor puddles with ripples.692-triangle oilskin form;3488 rain/puddle triangles;3 renderers total, no new lights/depth/reflection pass. Graphics inspected; shader self-sorting, overly bright puddles and first-control-grace race corrected.
- Focused cases pass across the documented initial run and targeted fixture/regression reruns;19 final Drowned/Tension checks include real blackout before a ship window. All12 solo/duo workload models pass. Graphics Night2 outage→Drowned→power restore→retreat passes. Live co-op storm4 appearance and restoration/storm1 now pass after the window-gating fix. Evidence: `Docs/Verification/S5/DrownedStorm/`.

Network **LK-S5-DROWNED2 / LK-Lab-Coop-v29**. Both clients update. Delivered source **47f9ee0c**. Final actual-XRI soloNight3 passes through both steered ships and bed intoNight4, zero errors. Live shared Drowned/storm/retreat passes. APK built and verified; **not installed because ADB sees no Quest; not launched**. The older passive-beacon fixture's expected wreck remains preserved as a setup error; no gameplay was relaxed.

APK: `Builds/LighthouseKeepers-Sprint5-Drowned-47f9ee0c.apk`, 124,605,442bytes. SHA-256`0a1da78a7a4d9cab29186d02e124e43d706fe8b798fb1517048bad87b4bbcf6c`. Build/VR manifest/signature receipts: `Docs/Verification/S5/DrownedStorm/apk.*.json`.

After this pass: finish phase3.2 cable reconnect/lens smears (#99), short explanations (#100), phase4 lantern (#101–103), then actual NavMesh hunter/hiding/escape phases (#105–108). Latest user direction makes the Drowned central; do not confuse the shipped saboteur/appearance slice with a finished hunter. `timingApproved=false` until human workload/headroom acceptance.

## Reproduce current checks

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-M3
# Choose new output names/directories; preserve previous receipts.
caffeinate -i python3 artifacts/remaining/m3/tests.py drowned-next 'DrownedTests;TensionDirectorTests'
# Full existing co-op transport harness, then focused Drowned/storm correction:
caffeinate -i python3 artifacts/s5/drowned-storm/crew-driver.py
caffeinate -i python3 artifacts/s5/drowned-storm/crew-appearance-driver.py
# Current physical-input solo replay includes wheel steering (not older ship/Night3.cs):
caffeinate -i python3 artifacts/s5/drowned-storm/night3-wheel.py
# All Editors mute before play; never refresh source while playing.
# Build a fresh output path from a clean commit:
LK_APK_OUTPUT="$PWD/Builds/LighthouseKeepers-Sprint5-Drowned-NEW.apk" caffeinate -i /Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity -batchmode --burst-disable-compilation -quit -projectPath "$PWD" -buildTarget Android -executeMethod LighthouseKeepers.Net.Editor.MultiplayerBuild.BuildQuest -logFile "$PWD/artifacts/s5/drowned-storm/build-NEW.log"
# Install only; no launch command:
/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb install -r Builds/LighthouseKeepers-Sprint5-Horror-029f9ae9.apk
```

## DEVICE CHECKS PENDING

Horror presentation: listen in Night2 after the protected generator step. Verify wet approach direction, reveal/breath/score intensity, speech priority, smooth score return, Sightings Off, stereo mist/face fade and no close-face pop. Profile same view with appearance on/off; 2,199 body/effect triangles is not GPU evidence. Editor audio was muted throughout. Final horror APK is ready from029f9ae9; package receipt above. ADB empty, no install or launch.

Latest APK is **029f9ae9 / LK-S5-DROWNED2 / v29**, `Builds/LighthouseKeepers-Sprint5-Horror-029f9ae9.apk`. Install when ADB returns; **do not launch without a new request**. Previously installed8927fb64/v27 lacks this Drowned/rain pass and cannot join v29. Both players need the same current build.

- Start with **PLAY → HOST / PLAY ALONE → DEV: SELECT NIGHT3 → START** to test active ship steering. Night1 ship is intentionally a safe preview.
- Check hull consistent at horizon/approach/near; F4 wheel follows bearing, beam holds the ship, deliberately miss then recover. Confirm named wreck/retry and both-player outcomes.
- Inspect new generator from the front/side; refill, three primer squeezes, close choke, three cord pulls, reopen. Confirm hand reach unchanged.
- Check left-watch dial now faces the wearer, LCD readable, backlight and objective/B recall still work.
- Human Mac+Quest/current-build two-Quest voice, valve, guest wheel and shared sleep/rejoin; both needv29 for the new build. Muted Editors prove configuration/transport, not audible conversation.
- Worn Quest72Hz: GPU≤11ms/CPU main≤9ms; full-hull distance change/beam/generator peak-storm view. No Editor FPS substitutes. Quest3 quality baseline remains MSAA4/scale1/fixedFFR0.5.
- New v29: Night2 outage → warning/sighting after first-control grace → storm rises; restore the generator → figure retreats/weather eases. Confirm warning audibility, no face pops, both-eye fade and Sightings Off.
- Inspect12 outdoor puddles and heavier rain at ground/gallery. Rain stays outside roofs; no floating puddles. Profile rain/Drowned on/off at the same view.
- Night5 human pacing/headroom still pending. Hunter/lantern/hiding are not enabled.
- Preserve existing pending offline Wi-Fi-off, wall/edge sweeps, held-prop comfort and rendered text checks. Will's skybox/avatar not edited.

## Earlier completed foundations

Quality1.0 and foundation1.1–1.3 implemented; headset acceptance remains open. Last phase2 full EditMode438/438; focused legacy PlayMode19/23 with the same four inherited failures. Full legacy PlayMode still stalls in retired Bootstrap tests, so no full-suite pass is claimed. Latest checks above are focused on the changed behavior. Previous source805e779 foundation and005e641 ship build receipts remain archived. Grading remote has main+sprint1–5; PR134 docs-only publication awaits teammate review.

---

## Historical checkpoints (new brief above takes precedence)

# Full remaining plan — clean checkpoint after Step 2a (November 11)

Branch `sprint-5`, draft PR #89. Nothing merged to main. Editor muted throughout; all test Editors closed. Quest was installed but **not launched**.

## Done

- **1a–1c:** audited existing accepted fixes; repaired the tally-counter obstruction of the starter. Full EditMode 372/372. Full PlayMode 52/83 initially passed; six corrected cases passed focused rerun, leaving 25 inherited failures (listed in evidence). Graphics Nights 1–3 and live two-Mac join/avatars/valve/both radio routes/mute/sleep/leave/rejoin passed. PR #85 ready for review. APK d5051f7 installed, installed hash matches, app closed. See Docs/Verification/RemainingPlan/Step1/.
- **Merged PR #85 into M3:** preserved current PLAY/host-alone/offline lobby, no-clamp solo, physical generator, sea recovery, watch and latest wiring. Campaign valve roles follow authored night AND current roster; join/leave/retry covered. Merge 0b6de74.
- **2a runtime reconciliation:** campaign Night 2 now consumes fuel to E with sputter and mains loss, then asks for refill; data-authored FuelExhaustion replaces the old silent timer reset. Solo clamp metadata removed. 69 focused cases pass. All 12 instruction-only task models pass (five nights + Demo, solo + duo). Actual-XRI graphics Nights 1–3 pass through sleep into Night 4, zero logged errors; inspected generator and Night 3 board. Docs/Verification/M3/scheduling-current/ and Docs/Playtests/telemetry-m3.md.

M3 network **LK-M3-CAMPAIGN2 / LK-Lab-Coop-v22** (campaign + sea-recovery fields). PR #85 installed on Quest is **LK-S5-PLAYTEST1 / v21**. Both clients must use the same build. Campaign remains the previously enabled task preview; timingApproved remains false. Final ship-inclusive 25–35% / 15–25% headroom is NOT yet approved. No night duration/flood retuning this pass.

## Next — Step 2b, then 2c, then horror

1. Implement named ship pass/wreck loop from NightScenario.ships (P21–P24 = cue IDs 96–99). Host owns passage outcomes/dark-window grace; guests consume snapshots. Add snapshot fields/version bump. Keep legacy catalogs outside campaign untouched. Ships must gate sleep; safe Night 1 preview; sighting → rocks → away, spatial foghorn, named loss, pass relief/sting, HUD/board state. Retime provisional authored windows against the current timing trace — existing windows are NOT approved for shipping. Reuse Eli's StormSeascape/ocean, no sky or avatar edits.
2. Re-run instruction-only models including ship waiting/pressure; graphics pass/wreck + screenshots; live campaign co-op. Then physical doors/mirror (2c).
3. Step 3 horror slices 1–3; Step 4 finale/progression; Step 5 horror 4–6; Step 6 split-information/final verification. **None implemented this pass.** No new M3 APK has been delivered since the merge; build/install when the next completed part is ready.

## DEVICE CHECKS PENDING

- Real Quest Wi-Fi OFF HOST / PLAY ALONE → START.
- Wiring GPU ≤0.3 ms and full scene GPU/CPU at 72 Hz; Mac timing cannot establish Quest cost.
- Missing lobby hands / board-like own-body report; physical grip/choke/pour/starter/watch comfort; B recall/text readability.
- Human voice intelligibility, Mac+Quest/2-Quest current-build transport and radio falloff.
- Ships/doors/mirror/horror budgets and comfort only after those systems exist.

Install allowed; **do not launch the Quest** without a new explicit request. Root integrated worktree still has unrelated user Wood.mat, Quest prefab ray-width edits and pipe-lever evidence; preserve them. M3-generated Wood change was backed up and restored after graphics capture.

## Exact commands

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-M3
python3 artifacts/remaining/m3/tests.py scheduling-final
LK_M3_TIMING_OUTPUT="$PWD/artifacts/remaining/m3/timing.json" /Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity -batchmode --burst-disable-compilation -quit -projectPath "$PWD" -buildTarget StandaloneOSX -executeMethod LighthouseKeepers.Editor.M3CampaignBot.RunBatch -logFile "$PWD/artifacts/remaining/m3/timing.log"
# Current mechanics replay: artifacts/remaining/m3/Nights123.cs, entry ImmersionSoloReplay.Run(output).
# replay.py launches a muted batch graphics Editor; inspect/use the corrected Nights123.cs,
# whose pending-stage waits account for authored relief periods.
# No Quest launch command here.
```

## Twelve-step headset checklist

Use the installed PR #85 build for 1–8; 9–12 need later M3 systems and are NOT ready today.

1. PLAY → HOST / PLAY ALONE → START; confirm hands and House.
2. With Wi-Fi off, confirm the same offline route.
3. Tap/hold B for objective + last line; double-tap to replay; adjust objective height.
4. Grip wrench, repair without a solo clamp, drop/recover it, then sleep.
5. Night 2: watch fuel reach E, hear sputter, see mains/pump/beacon cut out.
6. Pour fuel; squeeze primer three times; pull choke up; pull cord three times; push choke down.
7. Find separate POWER PANEL, trace subtle cables, check left-wrist time/objective.
8. Two matching builds: join/start, avatars, voice/radios, shared valve repair and sleep; leave/rejoin.
9. PENDING ship: gallery sighting → beam-lit passage and horn; test named wreck/retry.
10. PENDING doors/mirror: hand-swing doors and inspect Will's avatar from three angles.
11. PENDING horror: apparition, telegraphed hunt, sprint/hide; Scare Off removes it.
12. PENDING Demo: six-minute loop through ship moment, then continue/night-select persistence.

---

# Generator, watch and wiring delivered — November 11

Branch `s4/integrated`, PR #85. Latest scope: [GeneratorWatchWiring.md](GeneratorWatchWiring.md). Work in numbered order, focused tests + one muted graphics solo Night 2 replay per item, commit/push each. No full suites or horror work. Preserve unrelated Wood.mat, Quest prefab ray widths and pipe-lever evidence.

1. DONE: actual Night 2 fuel exhaustion, three-second sputter, empty gauge, mains outage and cause-first narration. Existing network fields only; `LK-S5-PLAYTEST1 / v21` unchanged. MasterDirector 12/12, VoicePass 23/23 pass. Final graphics Night 2 replay passed with cue 4 before refuelling, zero logged errors. Evidence: Docs/Verification/GeneratorWatchWiring/step1/.
2. DONE: mounted numbered generator control layout, fuel can on floor. Focused scene test and solo Night 2 pass; evidence step2/.
3. DONE: physical choke detents; existing pour, starter and grip-pressure primer preserved. Both-hand focused test + graphics Night 2 pass. Evidence step3/.
4. DONE: small dorsal left-wrist digital watch, corrected accessory orientation and gaze backlight. Focused recall/watch checks and Night 2 replay pass; inspected images step4/.
5. DONE: restored/rebaked M3 wiring; no duplicate feed. 13,894 triangles, five meshes, one material, zero unexpected clearance contacts. Four focused cases and Night 2 pass. Wiring was developed only on sprint-5, not dropped by a merge. Evidence step5/.
6. DONE: source **c7b9a99** built successfully. Installed on the connected Quest; installed APK SHA-256 matches. App deliberately left closed.

APK: `/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/Builds/LighthouseKeepers-GeneratorWatch-c7b9a99.apk` (134,388,246 bytes). SHA-256 `757b9d404bb060355e7e9d06fcbd9092eb367bf56c108479859d027ef90dd0c2`. [Delivery/evidence](../Verification/GeneratorWatchWiring/README.md). PR #85 updated; no merge. All five implementation items are done; the listed device checks and separate missing-lobby-hands report remain. Do not resume M3/horror without the next request.

Reinstall without launching:
```sh
'/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb' install -r '/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/Builds/LighthouseKeepers-GeneratorWatch-c7b9a99.apk'
```

## DEVICE CHECKS PENDING (current pass)

- Missing lobby hands / board below body: not reproduced or resolved. Preserve tracked hand rotations and Will's avatar; compare worn headset after START. First-frame logs alone do not establish the cause.
- Night 2 fuel warning, actual blackout, speech timing and reachable controls.
- Both-hand physical choke/pour/starter feel; watch fit/readability/orientation.
- Wiring visibility/clearance and GPU cost <=0.3 ms; complete scene target 72 Hz.
- Two-headset fuel state/presentation, voice/tasks/sleep regression (current pass has replica-focused evidence only).

## Exact current check commands

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
python3 artifacts/playtest-fixes/lobby-state-tests.py MasterDirectorTests generator-step1.json
python3 artifacts/playtest-fixes/lobby-state-tests.py VoicePassTests generator-step1-voice.json
# Stop Play Mode before starting a fresh replay; helpers explicitly mute the Editor.
python3 artifacts/generator-pass/solo.py step1-final
```

---

# Job 1 delivered — STOP before horror (November 11)

On `s4/integrated` / PR #85. Steps 5–6 and the simplified lobby are committed and pushed. Shipping code: **dff6e4b**. Generator vs POWER PANEL are distinct; island rails/sea recovery and one PLAY flow are implemented. No main merge and no M3/horror work in this turn.

**Verification:** 21 focused cases pass, one actual-XRI graphics solo Night 1–2 replay passes, and one live two-Mac smoke passes (host alone, late join, avatars, guest sea recovery, shared valve task, shared sleep, leave/rejoin). Post-smoke defensive recovery and keypad-clearance changes passed focused tests; the entire smoke was not repeated. No full suites, per the lean request. Both test Editors were muted and are now closed. Evidence: [delivery report](../Verification/PlaytestFixes/final/README.md).

**Delivered:** `/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/Builds/LighthouseKeepers-Playtest-dff6e4b.apk`; 133,792,150 bytes; SHA-256 `28b7df5f9f990c0f0e00b01e89c605968206a5dcd6721b407913819f29f4981d`. Installed and launched on the connected Quest as explicitly requested this turn; process alive after 15 seconds. No human headset acceptance implied. Build audio unchanged.

**Network:** LK-S5-PLAYTEST1 / LK-Lab-Coop-v21. Both players must update. Sea-recovery snapshot added; no READY requirement. One player gets solo puzzles; joins/leaves adapt roles immediately. Current night time is preserved on roster changes; the next night uses the new roster's budget.

## Next job (not started)

Merge verified Job 1 into `sprint-5`; re-bake its existing TowerWiring authoring for the moved power panel (do not duplicate the feed). Finish the M3 ship prerequisite, then the queued horror job. Full scope: [PlaytestAndHorrorQueue.md](PlaytestAndHorrorQueue.md). Stop here until asked to continue.

## DEVICE CHECKS PENDING

- ADB screenshot was black; confirm the rendered lobby while wearing the Quest. Startup activity/OpenXR and live process were confirmed.
- Human reach/readability: generator vs separate POWER PANEL, grip-only primer, B recall/objective height.
- Actual Quest Wi-Fi OFF: PLAY → HOST / PLAY ALONE → START on the offline message. Editor forced-unreachable path passed; device acceptance pending.
- Worn island edge traversal; sea fall comfort and ten-second return with a partner; dropped tools and tracked hands.
- Two-Quest real voice listening, shared tasks/sleep/rejoin; worn GPU/CPU and frame budgets.
- Locate the reported separate F3 yellow remnant; the all-floor audit did not reproduce it.

## Exact delivery commands

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
# Reinstall the matching build without erasing app data:
'/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb' install -r 'Builds/LighthouseKeepers-Playtest-dff6e4b.apk'
# Launch only when requested (authorized for this completed delivery):
'/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb' shell am start -n 'com.lighthousekeepers.game/com.unity3d.player.UnityPlayerGameActivity'
```

---

## Earlier checkpoints (historical; latest scope above takes precedence)

# November 11 playtest fixes — ACTIVE CHECKPOINT

Work on `s4/integrated` / PR #85, then merge the verified Job 1 into `sprint-5` / PR #89. Do not merge main. Preserve unrelated Wood.mat, TMP fallback atlas, Quest ray-width prefab edits and pipe-lever evidence. Editors must stay muted; do not launch the Quest without a request.

## Active checkpoint — Steps 5–6 and lobby implemented; final checks/build next

Steps 1, 3 and 4 are implemented and checked; step 2 was audited but the reported separate yellow remnant was not reproduced. Job 1 is NOT finished. Full job-end suites, solo 1–3/two-Mac smoke, APK and M3 merge have NOT run for this checkpoint. Job 2 has not started. The full remaining scope (including the new lobby and horror additions) is preserved in [PlaytestAndHorrorQueue.md](PlaytestAndHorrorQueue.md). Quest absent from ADB on November 11; no app launched.

## Current work

Step 1: solo valve clamp removed from rules, objectives and narration; solo retains the prior post-clamp inflow rate. Two-player hold/release and distinct worker validation remain; disconnect allows immediate solo repair. Valve sits beside the active bolt and moves only when unheld; hidden/non-interactable for one player. Focused pressure 4/4 and director 8/8 pass. Graphics actual-XRI Night 1 → Night 2 passed; live one→two→one valve visibility/placement passed (0.43 m from active bolt). Step 1 complete; evidence in Docs/Verification/PlaytestFixes/step1/. No new replicated fields; network stays LK-S5-VOICE1 / v19 for now. Lobby changes will require a new matchmaking version.

## Next in order

2. Pole audit complete (PlayMode 1/1; inspected F3 screenshot). No obsolete shaft or separate yellow remnant reproduced. Location clarification/device confirmation pending; working pole and safety hatch preserved. Evidence: Docs/Verification/PlaytestFixes/step2/.
3. DONE: grip-only pressure primer at chest height, larger bulb, release/refill, tally and feedback. Focused 3/3 + engine 6/6; left/right actual-XRI graphics pass, screenshots inspected. See InteractionInputs.md and Docs/Verification/PlaytestFixes/step3/.
4. Implemented: short action/object + specific place/floor; nearby controller GRIP/TRIGGER cue. Director tests 9/9 and near/far graphics check pass; final rendered icon verified and screenshot inspected. Director tests now 10/10, including actual rendered mesh. See Docs/Verification/PlaytestFixes/step4/.
5. DONE: separate yellow generator and dark-green wall POWER PANEL, over 4 m apart, one ceiling feed. Focused graphics test 1/1 and inspected screenshot: Docs/Verification/PlaytestFixes/step5/. Previous survey: Current panel is WatchPowerPanel.Create at STN_GENERATOR_PANEL; layout JSON anchor (3.031089,3.3,1.75), yaw 240. Generator mesh is reused by Editor/OneLighthouseM2.cs from the fixture prefab (Diesel crankcase, Alternator cover, Radiator grille). Inspect both world bounds before moving; runtime is generated, so fix the source/anchors and test rebuild. Integrated has ConnectedMachinePresentation legacy riser; M3 replaces it with baked TowerWiring resources, so re-bake that authoring after merge rather than duplicating conduit.
6. DONE: perimeter rails + 160-direction sweep; solo sea loss/retry + host-owned 10 s per-keeper crew recovery. Focused rules 4/4, graphics edge/sea retry 1/1; Docs/Verification/PlaytestFixes/step6/. Network LK-S5-PLAYTEST1 / v21; live transport check queued.
Lobby implemented: one PLAY flow, host-alone/START without READY, existing offline loader and keypad, dynamic puzzle roles; docs updated. Focused 13/13 passed. Final graphics Night 1–2 / live two-Mac / build pending.
7. Latest lean scope: focused tests only, one graphics solo Night 1–2 and one two-Mac smoke; no full suites. Build/install AND launch if Quest visible (explicitly requested this turn); update PR #85. Merge into M3 only after this job is verified.
Job 2: finish M3 ship first; write TheDrowned.md, then implement the latest horror brief in numbered checkpoints. Do not claim ships/horror/finale/progression done; M3 currently has scheduling and wiring plus B recall.

## DEVICE CHECKS PENDING

- Current job has no APK yet. Verify no-clamp solo repair, two-player valve reach/role split and disconnect.
- Grip squeeze/release feel, primer visibility; readable objects/locations/button cues.
- F3 pole remnant removed without losing floor exits/fall protection.
- Updated lobby with Quest Wi-Fi off and two matching builds online; rejoin/leave.
- Island edge walk, sea recovery; human voice; worn GPU/CPU/readability.

## Exact commands / evidence

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
python3 artifacts/playtest-fixes/tests.py PressureValveTests pressure.json
python3 artifacts/playtest-fixes/tests.py MasterDirectorTests director.json
python3 artifacts/playtest-fixes/solo.py
```
Additional focused commands: `python3 artifacts/playtest-fixes/pole-test.py OnlyCurrentDescentShaftAndThreeLandingsExist pole-test.json`; `python3 artifacts/playtest-fixes/primer-tests.py GripSqueezeTests grip-cycle.json`; `python3 artifacts/playtest-fixes/primer-live.py`; `python3 artifacts/playtest-fixes/objective-tests.py MasterDirectorTests director-locations.json`; `python3 artifacts/playtest-fixes/hud-live.py`. These use saved scene/input fixtures and must not be called human/device evidence. The primer runner injects grip after HandRig updates; old EngineLive/Days12 trigger-based primer harnesses must be updated before step 7 (do not change gameplay back for the harness).

Results/runners: `artifacts/playtest-fixes/`. Do not overwrite completed evidence; choose fresh output paths for reruns. Full suites only at job end. Preserve the earlier baseline below; its solo-clamp requirement is superseded by this playtest.

---

# Class playtest recall checkpoint — 2026-11-10

Job 1 on `s4/integrated` / PR #85 is implemented, committed and verified; APK built from **573cf72**. The Quest disappeared from `adb devices` before installation. **Do not launch the Quest app unless the lead asks.** All Editor runs must remain muted with `EditorUtility.audioMasterMute = true`; `EditorPlaytestAudio` handles Editor initialization/Play Mode transitions, and `artifacts/coop/mac_pair.py` also mutes before runs. No player audio settings changed.

Done: below-eye 1.5 m lazy-follow objective, five-second presentation, saved -20 to -5 degree height, instant B tap/hold, double-tap local replay, three-line history, wrist objective, existing X/Y/wrist recall, one-time Night 1 B reminder. Network remains **LK-S5-VOICE1 / snapshot v19**; no synchronized fields changed.

Verification: EditMode 355 passing cases after the single FMOD scene-audit rerun; PlayMode 49 passing cases with the same 24 inherited failures and 3 skips after replacing the obsolete world-fixed assertion with the requested lazy-follow test. Original full results and focused reruns stay separate. Physical Night 1 and live two-Mac join/avatar/local replay/shared pipe/shared sleep pass. Mono/AEC setup passes, but no human listening claim while Editors are muted. See `Docs/Verification/ObjectiveRecall/README.md`.

## DEVICE CHECKS PENDING

- Connect the Quest over USB, install the exact APK below; **do not launch without a new request**.
- When the lead is ready: press B near the board, pipe and generator; verify same-frame full objective and last line below gaze, hold, tap expiry, double-tap audio replay.
- Turn the head quickly and slowly: lazy follow must be comfortable and leave the centre clear. Adjust Objective height in Watch Menu settings and restart to check persistence.
- Read the current job on the left watch, check X/Y and wrist glance, and the one-time Night 1 B reminder.
- Human two-way voice and Mac+Quest task/sleep smoke; on-device worn-headset CPU/GPU/readability/comfort remain unverified.

## Exact continuation

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
"/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb" devices -l
"/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb" install -r Builds/LighthouseKeepers-Recall-573cf72.apk
# STOP here: do not run am start or monkey without the lead asking.
```

For M3, merge this branch into `/Users/aneeshvasamreddy/LighthouseKeepers-M3` / `sprint-5`, preserving all night/string assets. Continue A2/A3 runtime schedules, then A4 measurement, ships, doors/mirror, and remaining parts in `M3-FiveNights.md`. The A1 campaign is still disabled. No five-night timing, ship payoff, physical-door or mirror acceptance is claimed. Keep PR #89 draft and never merge main.

Raw runners/results: `artifacts/class-recall/`; `tests.py` (EditMode), `play-focused.py`, `play.py` (full PlayMode), `solo.py` (Night 1), `crew.py` (two-Mac smoke), `build.py` (exact-source Android build). Before repeating a run choose a fresh output directory; inspect the script's existing destination. Both Editors were stopped after the smoke test. Preserve the unrelated Quest prefab ray-width edit and `Docs/Verification/Immersion/pipe-lever/`.

---

# Integrated checkpoint — s4/integrated

This branch reconciles #81–#84. Do not merge or close those PRs. Network **LK-S5-INTEGRATED1 / snapshot 18**; both clients need the same APK. Selected exterior: Eli’s ocean, mountain coast and opt-in sky, per the lead. Our newer interior, narration/PA, pole, connected power and valve puzzle remain. See [Reconcile-PR84.md](Reconcile-PR84.md).

Post-cleanup focused7/7, graphics solo Nights1–3 and live two-Mac valve/radio/sleep/rejoin PASS (artifacts/reconcile/cleanup-graphics and cleanup-crew). Earlier full-suite evidence: 305/305 EditMode; full PlayMode plus focused reruns preserve every pass from both baselines (47 pass, 24 inherited common failures in the aggregate). Graphics Nights1–3 and live two-Mac valve/radios/sleep/rejoin pass. Cleanup APK built successfully fromd3d6f6f (white board/black text, House clutter removed, fixtures clear of rails,2×MSAA). Latest hash/path: Docs/Verification/Integrated/build-result.json. The lead requested installation; adb installedd3d6f6f successfully on Quest2G0YC1ZG32048T and relaunched it. Receipt: Docs/Verification/Integrated/install-d3d6f6f.txt. Human visual checks and2×MSAA profiling remain pending. Draft PR85 supersedes81–84 without closing them.

## DEVICE CHECKS PENDING — integrated

- Installed `Builds/LighthouseKeepers-Integrated.apk` fromd3d6f6f. Confirm `LK-S5-INTEGRATED1` in lobby during the human check.
- Profile House, F1, F2, F4, gallery sea and strongest storm at 72 Hz: GPU ≤11 ms, CPU work ≤9 ms, draws/triangles/voices. Installed cleanup build: scale1.0, MSAA2, fixed foveation0.5. Device samples exceeded11ms, so the authorized2× fallback needs a matching-view check. Keep pacing wait time separate from CPU work.
- Continuous wall/doorway sweep; no head pop at bench, centre aisle, F1, dropped wrench or disabled Support foot. F3/F4 edges blocked.
- Solo valve clamp → grip-only wrench snap → visible clockwise turn; drop/recovery; tracker fixed until recall; grace understandable; glass stereo sorting/text clarity.
- F4 enclosed rooftop rain → exposed gallery louder exterior rain; F1 outside unchanged. Recorded/new placeholder announcements audible.
- Mac+Quest: join, avatars, both voice directions with headphones, cross-floor falloff, both handheld radio modes, both valve roles, shared task/sleep, disconnect/rejoin. Follow [TwoHeadsetSession.md](TwoHeadsetSession.md).

## Reproduce current checks

Run from `/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics`. Existing raw scripts/results are in `artifacts/reconcile/`: `integrated_graphics.py`, `integrated_crew.py`, `build_integrated.py`. Use new output directories for new runs. Finish Play Mode before rebuilding. Preserve the unrelated user ray-width change saved in `preserved-user-rig.patch` and restore it after exact-source builds.

---

# Part E checkpoint — s4/connected

Stacked draft [PR #83](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/83), based on s4/visuals / PR #82. E1 dependencies, E2 pressure valve + solo clamp, E3 proximity/handheld voice are committed. E4 device plan is [TwoHeadsetSession.md](TwoHeadsetSession.md). Part D: graphics solo Nights 1–3 and two-Mac valve/radio/sleep/rejoin checks pass. 280 EditMode passes; full PlayMode plus corrected clamp-prerequisite rerun preserves all 40 prior passes, with the same 25 inherited failures. [Final evidence](../Verification/S4Connected/final/README.md).

APK built successfully from `ce3b8f7`: `Builds/LighthouseKeepers-S4-PartE.apk`. SHA-256 `dca2a05ece210708281331ff327d630f694c0eda2f0dc0b559d32e1bb79cb4d3`. Later delivery commits only add documentation/evidence.

Network **LK-S4-CONNECTED2 / v17**. Both clients must update. New replicated fields are valve ownership/clamp and handheld transmitter IDs. Solo microphone remains off. Will's skybox/avatar body and local hand rotation are untouched.

## DEVICE CHECKS PENDING — Part E

- Install `Builds/LighthouseKeepers-S4-PartE.apk` on both Quests; exact source/hash in `Docs/Verification/S4Connected/final/build-result.json`. No install during this offline turn.
- Execute all ten steps in [TwoHeadsetSession.md](TwoHeadsetSession.md): setup/join, both valve roles, release surge, shared tasks, dependency chain, cross-floor direct/radio speech, mute, shared sleep, disconnect/rejoin.
- Human intelligibility/echo cancellation in both directions, radio grip/head proximity comfort, wall/floor attenuation, actual device audio CPU and voice count.
- All existing visual/comfort/performance checks below remain pending; Mac tests cannot certify them.

## Part E local commands

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
python3 artifacts/s4-connected/after/sync_tests.py
python3 artifacts/s4-connected/after/exact_full_run.py
python3 artifacts/s4-connected/after/solo.py
python3 artifacts/s4-connected/after/crew-fixed.py
python3 artifacts/s4-connected/build.py
```

Use fresh output folders, run the crew script only with both Editors ready, and never refresh scripts during Play Mode. Build wrapper preserves the unrelated Quest player prefab ray-width edit. Device session remains the next action; stop after this chunk.

---

# Previous Part C checkpoint

Worktree: `/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics`. Branch **s4/visuals**, draft [PR #82](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/82), stacked on `s4/master` / draft [PR #81](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/81). Never merge main. Will's skybox and avatar body are unchanged.

Network version **LK-S4-VISUALS-C0**, snapshot v15. Both crew clients must update. The bump covers the composite spoken-status cue vocabulary (IDs 44–75); C0–C5 rendering/audio additions consume existing authoritative state and add no replicated fields.

## Completed this chunk

- Part B follow-up (`37a0447`): one-sentence spoken time/water/beacon status, central legacy menu/readout strings, sampled walkable-boundary audit and five F3/F4 guard repairs. Actual House→gallery route and two pole descents pass. Evidence in `Docs/Verification/S4Master/part-c/b-leftovers.md`.
- C0 (`132bfef`): Android's active QuestPipeline uses MSAA4x, render scale1.25 and fixed foveation0.5. TMP SDF HUD/subtitles; fixed mipmapped world writing. All 36 authored scenes scanned; detected coplanar pairs 1509→0 without moving colliders. Realtime light inventory retained.
- C1 (`62a6d85`): opaque double-sided shell walls, .04 m near clip, wall LOD fade/culling fixes, occlusion off. 72 rendered views across House/floors/stairs/gallery reviewed; not a headset continuous-motion certification.
- C2 (`77909e5`): CC0 limewashed stone, weathered wood, rusty metal/brass, wet stone; metre UVs, packed masks, ASTC and mipmaps. Sources/derivations in `AssetSources.md`.
- C3 (`fef69ae`): actual CPU light bake, 2 lightmaps, 99 probes and 5 room reflections; one enabled realtime beacon point light, no realtime shadows. Practical housings, window haze, interior/exterior fog and dawn tint. Existing blackout/restore still drives presentation.
- C4 (`3366396`): dual-normal flood, contact ripples/debris, four-wave ocean and rocky inlet, small soft droplets/scrolling active jet, nightly rain/seep/runoff and wave impacts. Host flood rules and damage are untouched.
- C5 (`c0e67dc`): transparent lantern/Fresnel glass, amber core, window streaks, station wear and nonblocking keeper props. Original sound-gap cues. Local hand illumination now uses baked probes plus a small tan visibility floor; pose/rotation/grip unchanged. Clean Night 1 replay passes.
- C6: `M2-Plan.md` records remaining per-floor art, safe exterior-route work, model-source options and budget allocations. `WorldAudioGaps.md` records additions/reuse/licences.

## Evidence and limits

Per-step evidence is in `Docs/Verification/S4Master/part-c/`; raw runs in `artifacts/s4-master/part-c/`. C4/C5 each pass 31 focused checks and physical Night 1 repair/drop/recovery/sleep. Final evidence is saved in `part-c/final/`: 272/272 EditMode pass; full PlayMode 39/65, then an ordered test-cleanup rerun restores the avatar pass (40 passing cases, the same 25 inherited failures). Solo Nights 1–3, two-Mac guest repair/shared sleep/rejoin, and the 72-view wall sweep pass. See the regression notes for the full-run versus rerun distinction.

Known visual limits: first-pass low-poly props; analytic flood boundary foam does not trace every arbitrary prop; conventional bounded Unity droplets rather than compute-particle simulation; no new walkable coastline without support/guards. Render scale1.25 is provisional, not performance-approved. Composition-layer HUD fallback remains conditional on observed headset shimmer. See `MasterPassRemaining.md` for acceptance work.

## Built APK

`Builds/LighthouseKeepers-S4-PartC.apk`, runtime source **a8688d4**, 125,947,726 bytes. SHA-256: `543d5acc763f2c474c88eebf0c837389f92efacd184aafed06da06918bb2c2d2`.
Later commits only change test teardown/evidence; the runtime source is unchanged. Android IL2CPP build and both Vulkan/GLES shader compilation succeeded. Installation was skipped as requested. [Final report](../Verification/S4Master/part-c/final/README.md).

## Exact local commands

Use fresh output directories on reruns; wrappers intentionally preserve old evidence.

```sh
cd /Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics
python3 artifacts/s4-master/part-c/sync_tests.py
python3 artifacts/s4-master/part-c/exact_full_run.py
python3 artifacts/s4-master/part-c/solo_after.py
python3 artifacts/s4-master/part-c/launch_coop.py
python3 artifacts/s4-master/part-c/build.py
```

Unity6000.3.23f1. Run batch graphics with `--burst-disable-compilation` for this Mac's test environment; no `-nographics` for captures. Never edit/refresh scripts while playing. Build method `LighthouseKeepers.Net.Editor.MultiplayerBuild.BuildQuest`, env `LK_APK_OUTPUT`. Close the primary Editor before building. Preserve the unrelated `LK_QuestPlayer.prefab` ray-width delta and `Docs/Verification/Immersion/pipe-lever/`; the build wrapper saves/restores that delta. Secondary worktree has source overlaid for tests, with its previous patch saved; do not reset it.

## DEVICE CHECKS PENDING — NOT VERIFIED

The current instruction says build but skip installs. No device results are inferred from Editor tests.

- Install `Builds/LighthouseKeepers-S4-PartC.apk`; source/hash in `Docs/Verification/S4Master/part-c/final/build-result.json` (built successfully).
- Profile House, F1–F4, outside and peak storm: sustained72fps; GPU≤11ms, CPU main≤9ms; ~150draws/eye, ≤300k visible triangles, audio≤24 voices; water≤2ms. Capture the same workload with visual switches off/on to isolate cost.
- Compare scale1.0/1.25/1.4 at MSAA4x; select within budget. Fall back to2x only if needed. Verify fixed foveation and both-eye SDF/physical text shimmer.
- Check glass lens/rain/foam transparency in both eyes, ocean silhouette against Will's skybox, practical powered/blackout readability, tan-hand brightness, strong/weak pipe spray.
- Grip-only wrench, clockwise turn/sink, both hands, water drop/recovery; no pose regression.
- First-time Nights1–3: current-control guidance, one-sentence status, PA audible on stairs, wake/slate guidance, reasonable Night3 timing, cause-specific fail/retry.
- Pole reach/comfort/release/landing, smooth stairs and bunk clearance; continuous doorway/wall sweep and ledge safety on every walk route.
- Mac+Quest join, avatars, two-way audible voice/echo processing, guest task, split objective, remote pole movement, shared sleep/rejoin. Two-Mac transport tests do not certify human speech or headset tracking.

Then stop and await the user's next chunk. No main merge is authorized.

Final screenshot review also caught main-page Close reappearing over Sound settings on live state refresh. WatchMenu now keeps its main buttons hidden until Back. The new regression and rendered follow-up pass; no synced fields changed. The `859f8b0` APK predates this menu-only correction; next APK must include it.
