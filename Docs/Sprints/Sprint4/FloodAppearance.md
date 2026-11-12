# Interior flood water appearance

The Lab's existing `FloodWater.mat` now uses `LighthouseKeepers/Interior Flood Water` instead of the flat blue unlit material. Its existing GUID is preserved, so the committed Lab scene picks up the change without regeneration.

## Appearance and controls

- Two slowly drifting, crossed layers of the existing `LK_Water_Wear.png` texture produce ripple shading and subtle colour variation. World-space sampling works on the existing flood disc, which has no texture UVs.
- A muted grey-green tint replaces the saturated blue/teal. The existing `FloodSurface` component still darkens `_BaseColor` as water rises.
- URP lighting supplies wet highlights and existing environment-probe reflections. Premultiplied transparency reveals some floor detail when looking down; opacity increases toward grazing angles.
- The material exposes water tint, ripple repeats per metre, ripple shading height, drift speed, smoothness and looking-down opacity. The default ripple height is 0.008 m and affects shading only, not the flood mesh or authoritative water height.
- The shader uses two ripple texture samples per fragment, standard URP lighting and stereo macros. It does not request camera opaque/depth textures, refraction, planar-reflection cameras, additional meshes or per-frame scripts. Transparent overdraw and lighting still need Quest measurement.

`WatchDemoBuilder` retains the authored water material when regenerating the Lab. If the material must be recreated, it uses the new shader and existing ripple texture with the shader's defaults.

This change affects appearance only. Flood rates, pipe/pump rules, failure thresholds, snapshots, the existing circular coverage and the local XR rig are unchanged. It is not room-by-room fluid simulation or depth-dependent water absorption; transparent sorting at intersections remains a visual acceptance check.

## Checks performed

- Standalone DXC 1.8 compilation to Vulkan 1.1 SPIR-V passed for both vertex and fragment entry points in six configurations: mobile baseline, additional lights/cascaded shadows/linear fog, screen-shadow keyword, instanced stereo, vertex lights/exponential fog, and desktop clustered lights. These checks used Unity's upstream `6000.3/staging` URP source headers whose package declares version 17.3.0. Mobile headers emit precision-conversion warnings; the compiler reported no errors. This is not Unity's shader importer or an Android build.
- C# syntax checking passed for the changed scene builder with release and Editor/development symbols. This does not establish Unity API compilation.
- Asset references, preserved material GUID, source/meta pairs, repository hygiene and diff whitespace were checked.
- Unity 6000.3.23f1 is unavailable in this machine: its official download API still returns a network-policy 403. No in-engine preview, Unity import, APK, headset execution or performance result is claimed for this change.

## Preview and acceptance

1. Pull `william/sprint-4` and open Unity 6000.3.23f1. Wait for import; check the Console for shader errors and inspect `Assets/WatchDemo/FloodWater.mat` for a supported shader and assigned ripple map.
2. Choose **Lighthouse Keepers → Play game → PLAY SOLO**. Look down at the first-floor flood and move the view through different angles. Confirm drift, wet highlights and partial transparency, with no pink material or fixed blue fill.
3. In Play Mode, select the active `FloodController` and preview its Inspector height at 0.02, 0.25 and 0.5 m. Inspect wall, stair, prop and floor intersections; do not save Play Mode height changes. Check daylight/normal practical lights and Day 2 blackout lighting.
4. Repair the pipe with pump power available, then verify the surface still drains. Exercise high-water failure/retry and check the material stays assigned after reset. Repeat in a host/guest session to verify matching gameplay water levels; ripple phase is local cosmetic animation.
5. Build a fresh development APK using **Lighthouse Keepers → Sprint 3 → Networking → Build Quest APK**. Verify both eyes, glancing-angle shimmer, transparent objects, and close-to-surface views. Compare measured CPU/GPU timing and sustained workload against the same scene before this material change.
