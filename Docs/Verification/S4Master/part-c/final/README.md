# Part C delivery — Editor verified, device pending

Part B follow-ups and the C0–C6 implementation pass are pushed. Draft [PR #82](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/82) stacks on [PR #81](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/81). Neither was merged. Will's skybox and avatar body were not edited.

## WORKS — observed evidence

- Composite spoken time/water/beacon status; migrated legacy strings; authored route/pole tests and sampled boundary guards. The boundary audit repaired five gaps; it is not proof against every possible human movement.
- Android's active QuestPipeline: MSAA 4×, render scale 1.25, fixed foveation 0.5. TMP SDF objectives/subtitles and fixed mipmapped world writing. Scan of 36 authored scenes reduced detected coplanar overlaps from 1,509 to zero. Wall fixes passed a reviewed 72-view Editor sweep.
- Shared CC0 stone/wood/iron/brass/wet-stone materials, metre-scale UVs, ASTC imports and packed maps. Actual CPU lighting bake: two lightmaps, 99 probes, five saved room reflections. One enabled realtime beacon light; no realtime shadows. Reflection textures survive scene reload and Play Mode.
- Practical lamp housings, window haze/rain, fog and dawn tint; transparent Fresnel lantern rings with powered amber core; spinning visible beacon beam. Blackout/restoration still follows the existing power state.
- Flood normals/ripples/debris, bolt-driven leak jet, four-wave ocean and rocky inlet, increasingly strong nightly rain/runoff/seep effects. Existing authoritative flood damage and task rules remain unchanged.
- Nonblocking keeper props and station materials. Spatial door, chalk, bunk, pole and water-entry cues fill the sound gaps; existing machine audio and voice processing remain intact.
- Graphics solo Nights 1–3 complete through sleep. The paced Day 3 XRI run completed tasks at 291.6 s and retained 138.4 s after a 20 s return-to-bed allowance. The separate instruction-only timing model finishes at 345.2 s with 104.8 s spare. These are scripted/modelled times, not a human first-timer playtest.
- Two independent Mac Editors over live Fusion: join/ready, both remote avatars visible, guest's three bolts update both boards, shared sleep into Night 2, return/rejoin. Mono voice transport and AEC/noise-suppression configuration checked; human listening is pending. See [co-op notes](co-op-notes.md).

## Regression accounting

EditMode **272/272 passed**. The complete PlayMode run passed **39/65**, with 25 baseline failures and one test-isolation failure. A failing legacy wrist-roll test left its synthetic wrench grab selected during scene unload. Test teardown now releases that grab. An ordered rerun passes both affected avatar tests; the old wrist-roll assertion still fails as it did before. Combined coverage is **40 passing cases and the same 25 inherited failures**, with no previously passing case left failing. This is a full run plus targeted rerun, not a claim that one full run produced 40/65. [Exact accounting and failure list](regression-notes.md) · [comparison](regression-comparison.json) · [full suite summary](full-suite-summary.json).

The co-op harness also needed a complete synthetic grip input and time for its hand pose to reach the host. The successful rerun used production interaction/authority unchanged. Failed evidence was retained rather than overwritten.

## APK and matching builds

`Builds/LighthouseKeepers-S4-PartC.apk` — runtime source `a8688d4726d0e9a05d407204d8b8e0bd67cfb6d3`, 125,947,726 bytes. Later commits are test/evidence only. Unity 6000.3.23f1 Android IL2CPP build succeeded, including Vulkan/GLES shaders. Installation skipped as requested.

SHA-256: `543d5acc763f2c474c88eebf0c837389f92efacd184aafed06da06918bb2c2d2`.

Both players need **NETWORK VERSION LK-S4-VISUALS-C0 / LK-Lab-Coop-v15**. The version bump covers new composite status cue IDs; the visual pass adds no new replicated fields.

## NOT VERIFIED — device and art limits

| Budget | Target | Current evidence |
| --- | --- | --- |
| Refresh / GPU | 72 Hz / ≤11 ms | Device pending, every floor/outside/peak storm |
| CPU main | ≤9 ms | Device pending |
| Draws | ~150 per eye | Device pending |
| Visible triangles | ≤300k | Device pending; ocean mesh itself is 32,768 triangles |
| Water GPU | ≤2 ms | Device pending, feature-off/on comparison required |
| Audio voices | ≤24 | Existing limiter retained; simultaneous device workload pending |
| Render scale | 1.0–1.4 within GPU budget | 1.25 provisional; +56.25% pixels vs 1.0, not a measured GPU cost |
| MSAA / realtime lighting | 4× / at most one light | Configuration and Editor assertions pass; Android runtime verification pending |

Headset readability/shimmer, both-eye transparency, bolt feel, pole comfort, stair/ledge safety, sustained performance and Mac+Quest voice/task/sleep remain pending. The SDF HUD composition-layer fallback is conditional on observed headset shimmer.

This is a first visual pass: low-poly original props, visibly regular ocean detail, analytic flood foam rather than foam tracing every prop, bounded conventional Unity droplets rather than a new compute-particle system. Extra salt/soot/splash decals, per-corner vertex AO and bespoke prop modelling remain in [M2-Plan](../../../../Design/M2-Plan.md). The coastline is decorative and does not open an unsupported walking route. No claim of finished art or locked 72 fps is made.

## Inspected screenshots and next check

[House arrival](screenshots/house-arrival.jpg) · [glass lit](screenshots/glass-on.jpg) · [glass dark](screenshots/glass-off.jpg) · [generator](screenshots/generator-step.jpg) · [pipe jet](screenshots/water-jet.jpg) · [tan hand and bolt](screenshots/tan-hand-turn.jpg) · [coast](screenshots/coastline.jpg) · [wall sweep](screenshots/wall-sweep.jpg) · [Night 3 survived](screenshots/day3-survived.jpg).

[Host sees guest](screenshots/02-avatar-host.jpg) · [guest sees host](screenshots/02-avatar-guest.jpg) · [shared repair host](screenshots/03-guest-task-shared-host.jpg) · [shared repair guest](screenshots/03-guest-task-shared-guest.jpg) · [shared Night 2](screenshots/04-shared-day-two-guest.jpg) · [rejoined Night 2](screenshots/05-rejoined-day-two-guest.jpg).

[Ten-step first-time headset checklist](headset-checklist.md) · [checkpoint and exact commands](../../../../Design/Next.md) · [status/danger recording lines](../../../../Design/RadioLines.md) · [asset licences](../../../../Design/AssetSources.md) · [sound-gap audit](../../../../Design/WorldAudioGaps.md).
