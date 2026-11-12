# PR #85 remaining-plan verification — Step 1b

- Full EditMode: **372 passed, zero failed**.
- Full PlayMode: **52 passed, 31 failed**. Six failures newly exposed against the ObjectiveRecall baseline were investigated and rerun: **6/6 now pass**. The other **25 inherited failures remain**; see full-suite.json and comparison.json for every name/message. This is not an all-green full suite. Aggregate coverage after focused reruns is 58 passing cases with no previously passing case still failing.
- Five fixture expectations were obsolete after accepted changes: hidden Day 2 shortcut now lives on HOST / PLAY ALONE, solo no longer clamps a valve, return targets PLAY rather than PLAY SOLO, and the host no longer needs READY. HUD follow must begin after initial objective placement settles.
- The corrected starter fixture exposed an actual counter obstruction from a close standing angle. Both counter bodies were recessed 4 cm; the same 27-view visibility and physical pull/recoil check passes. No grip transforms, engine rules, hand rotations, avatar or network fields changed.
- Actual-XRI graphics solo Nights 1–3 pass: no-clamp pipe, wrench drop/recovery, physical fuel/primer/choke/starter, staged beacon/radio, board checkmarks and bed transitions. The original harness stopped after Night 3 completion on an obsolete old-pipe timing bound. Final bed was then verified in the same session (solo-bed.json), and the runner was corrected. This is functional regression, not a human first-timer timing claim. No gameplay was changed to satisfy timing.
- Live two-Mac Fusion room 4CFCZC: host START alone; guest late join; both avatars; mono/AEC/NS; guest sea recovery; shared valve/pipe; cross-floor proximity attenuation; host trigger and guest near-head radio; mute cancels transmission; shared sleep; leave adapts to solo; rejoin retains Night 2. Both Editors muted. Batch Editor processes allowed testing to continue while macOS was locked. Human speech intelligibility was not tested.
- Inspected the House/board, generator and both avatar captures. The first board frame was before deferred writing rasterization completed; subsequent board capture is readable.

## Device checks pending

Wiring ≤0.3 ms GPU cannot be established from Mac Editor timing. Geometry: 13,894 triangles, five meshes, one material. Real Quest Wi-Fi OFF, worn control/watch/readability, missing lobby hands, and human two-way listening remain pending. No Quest launch was authorized or performed. No full-suite rerun beyond the affected cases was needed after the small counter adjustment.

## Reproduce

```sh
python3 artifacts/reconcile/integrated_suites.py remaining-step1
python3 artifacts/remaining/step1/batch-rerun.py
python3 artifacts/remaining/step1/solo-guest.py
python3 artifacts/remaining/step1/crew.py
```

These local runners preserve the original raw results. The semicolon filter sent through Pipeline once matched zero tests and is excluded; command-line Unity filtering produced the recorded six-case result. A stalled GUI rerun was cancelled before using the batch runner.

## Step 1c delivery

Source **d5051f7**, APK `LighthouseKeepers-PR85-d5051f7.apk` (134,389,082 bytes). SHA-256 `74709cc32f6843aab60553c55ed52ee61be311ed31135fe0a90edce7f25da800`. Installed on Quest; installed APK hash matches. App left closed.
