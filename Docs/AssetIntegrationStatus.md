# September 23 environment and audio integration

Unity remains exactly 6000.3.23f1, URP 17.3.0, Android ARM64/IL2CPP. Branch: aneesh/lighthouse-sprint1-foundation; published baseline: 86e2389. No push, merge or rebase. Spear Throw VR remains untouched.

## Scene changes

| Scene | Changes |
|---|---|
| LK_Bootstrap | One AmbienceMusic component/source below AcousticDirector, streamed crossfaded soundtrack, Ambience Music mixer group/exposed -20 dB volume. No additional listener or rig. |
| LK_Core | Widened adjacent house/lantern door modules, moved arrival furniture, worn office desk, utility cabinet/extinguisher, baked warm fill. |
| LK_Level01_Plumbing | Motor replaces crude sump motor/chamber visual; two vertical pipe runs, tool case and utility fixture; baked cool fill. Repair split pipe/anchors, flood origin and leak untouched. |
| LK_Level02_Generator | Two rusted industrial motors on existing vibration plinth, ventilation fan, three drums, cabinet/extinguisher/utility light mesh; generator hum relocated to assembly; baked warm fill. Intake/starter/breaker/socket positions unchanged. |
| LK_Level03_Communications | Worn radio desk, chair/tool case/fixture; east mess table repositioned and retained for clearance; baked warm fill. Radio, map, sockets unchanged. |
| LK_Level04_Lantern | Small drive motor and overhead fixture; beacon/controls/balcony unchanged. |
| LK_Exterior | Fixed annular balcony rain with shelter rejection, three bounded offshore fog volumes, 64x64 ocean grid and opaque stereo-aware shader. |
| LK_DevGym | No content change. |

All scene edits were serialized by Unity Editor APIs. Retired visual objects are retained with disabled renderers/colliders and removed from static batches, so replacements are reviewable. Static render batches and baked lightmaps were rebuilt. Existing probes and scene coordinates are preserved; lighting bake updates probe data.

## Circulation measurements

Both local tower door openings: sampled usable span **0.89m → 1.29m**. House central walking route passes a 1.2m-wide collision test. Level 3 east worktop now spans x=3.97–4.67, leaving about **1.21m** from the landing guard. Tower radius remains 5m (~9.6m inside diameter); spiral width remains 1.6m, original pitch/headroom preserved. The existing 285 spiral support/head-clearance samples pass. No global/nonuniform architectural scaling.

## Weather, water and audio

Balcony rain: fixed radius 5.45–6.70m annulus, cap220, lifetime0.4s, no particle collision/splashes. Clears indoors; roof/house rejection also applies to exterior window rain. Indoor leaks remain intentional.

Fog: three offshore vendor-prefab derivatives, cap8 particles each, 12s lifetime, local bounded placement, no velocity/noise/collision/trails, URP material; CoastalFog reads existing StormController intensity. No competing weather controller.

Water: vendor URP shader retained as editor comparison. Runtime fallback has two animated vertex waves, 4225 vertices/8192 triangles, opaque shading, stereo macros and storm-driven amplitude; no screen depth/color reads, planar reflections or tessellation. Existing LK_Water material is retained for further fallback/flood use. This is a documented compatibility deviation, not a claim that the vendor shader passed Quest testing.

Music: one source, about110.64s processed loop with5s crossfade, Vorbis quality0.5/Streaming/background load, Ambience Music under Master, exposed AmbienceMusicVolume=-20dB,5s startup fade and thunder ducking. Original environmental groups/snapshots remain.

## Files and review

New runtime components: `Scripts/Runtime/Audio/AmbienceMusic.cs`, `Scripts/Runtime/Environment/CoastalFog.cs`. Updated `WeatherVolume.cs` and editor-only `DesktopPreview.cs`. Game-owned variants, textures, prefabs, audio, shader and ocean mesh are under `Assets/_LighthouseKeepers/ThirdPartyVariants/`. Editor integration/validation tools and focused tests are under the existing Scripts/Editor and Scripts/Tests folders.

See ThirdPartyAssets.md for exact vendor selections/notices; Verification/IntegratedAssetPaths.txt is the dependency inventory. See Verification/AssetIntegrationValidation.md for command results, build path and remaining checks. No downloaded demo scene/controller/script is placed in the game; no registry dependency changed.
