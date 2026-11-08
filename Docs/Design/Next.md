# Part E checkpoint — s4/connected

Stacked draft [PR #83](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/83), based on s4/visuals / PR #82. E1 dependencies, E2 pressure valve + solo clamp, E3 proximity/handheld voice are committed. E4 device plan is [TwoHeadsetSession.md](TwoHeadsetSession.md). Final Part D regression/build results follow when complete.

Network **LK-S4-CONNECTED2 / v17**. Both clients must update. New replicated fields are valve ownership/clamp and handheld transmitter IDs. Solo microphone remains off. Will's skybox/avatar body and local hand rotation are untouched.

## DEVICE CHECKS PENDING — Part E

- Install the final Part E APK on both Quests; no install during this offline turn.
- Execute all ten steps in [TwoHeadsetSession.md](TwoHeadsetSession.md): setup/join, both valve roles, release surge, shared tasks, dependency chain, cross-floor direct/radio speech, mute, shared sleep, disconnect/rejoin.
- Human intelligibility/echo cancellation in both directions, radio grip/head proximity comfort, wall/floor attenuation, actual device audio CPU and voice count.
- All existing visual/comfort/performance checks below remain pending; Mac tests cannot certify them.

---

# Sprint 4 master pass — Part C checkpoint

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
