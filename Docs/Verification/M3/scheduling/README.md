# M3 scheduling checkpoint — b3bb144

Campaign OFF. Normal PLAY SOLO/crew still uses the accepted watch catalog. The new five-night/demo schedules run in focused simulation tests; ships/finale/progression are not playable yet.

- Unity compilation and Android build pass. Exact APK/source/hash: build.json.
- Full EditMode: 371/371 pass.
- Full PlayMode: 49 pass, the same 24 inherited failures, 3 skipped; zero new failures or lost passing cases. See comparison.json and full result files.
- Graphics legacy Nights 1–3 pass with actual XRI interactions and sleep. Initial Night 3 attempt hit the test harness's 60-second cold-load deadline while tests/build ran; rerun with a 180-second harness load allowance passed without a gameplay edit. Both raw attempts remain in artifacts/m3/a2.
- Live two-Mac Fusion smoke passes join, Will's avatars, mono/AEC/NS configuration, private B recall/replay, height slider, host-held valve + guest pipe repair, shared board and both sleeping into Night 2. Editors muted; no new human listening acceptance.
- Twelve instruction-only task simulations pass (five nights + Demo, solo/duo). Their assumptions/limits and provisional timing table are in Docs/Playtests/telemetry-m3.md. They do not yet test ship encounters or split-information conversation.

Inspected captures: night-one-board.png, generator-restored.png, night-three-board.png, crew-avatar.png, crew-board.png. No new sky/avatar edits.

Network LK-M3-NIGHTS1 / v20. The new schedule fields are host-owned; guests apply snapshots and cannot advance events. Both clients need matching versions.

ADB sees no Quest. No install or launch. When it returns, use PR85 recall APK 573cf72 until the five-night feature is ready, without launching unless asked. Ship/door/mirror/wiring visuals, headset comfort/readability and worn-device GPU/CPU remain pending.
