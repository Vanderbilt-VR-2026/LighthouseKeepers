# Quest 3 quality baseline — Sprint 5

Authority: Sprint5-Master.md §14, Unity 6000.3.23f1. Step 1.0 is tracked by #90; the final-content re-audit is #127. Network remains LK-M3-CAMPAIGN2 / snapshot 22: these are local rendering/import settings, with no new replicated fields.

The menu is **Lighthouse Keepers → Apply Quest Quality Baseline**. It audits the active Android pipeline, applies the baseline including Lab static/light/probe settings, and writes before/after inventories to `artifacts/s5/quality/`. The separate **Quality → Rebake Quest Lighting** menu rebuilds powered/emergency variants using the existing lighting controller. A build guard rejects settings drift rather than silently changing a release during packaging.

## Required settings and measured cost

**No new Quest measurements yet.** ADB returned no devices on November 12. “Pending” means unmeasured, not zero cost. Editor frame rates, APK success and imported settings are not Quest GPU evidence. The prior 2× MSAA fallback is superseded by this required 4× baseline; only a new matching-view GPU measurement can authorize returning to 2×.

| § | Setting | Before | Required/applied target | Location | Measured cost |
|---|---|---|---|---|---|
| 14.0 | Refresh / budgets | Quest runtime default; telemetry available | 72 Hz; 13.9 ms frame, GPU ≤11 ms, CPU main ≤9 ms | runtime display bootstrap + OVR Metrics | Pending; targetFrameRate alone does not prove XR refresh |
| 14.1 | Android quality | Quest Development, index 0 | Same; other Android-selectable levels must use same pipeline | ProjectSettings/QualitySettings.asset | Pending |
| 14.1 | Actual pipeline | QuestPipeline | Same, GUID 8211712c856274fca823aa424774416a | Assets/_LighthouseKeepers/Settings/RenderPipeline/QuestPipeline.asset | Pending |
| 14.2 | Depth / opaque copies | Off / off | Off / off | QuestPipeline | Pending |
| 14.2 | HDR | Off | Off | QuestPipeline, cameras | Pending |
| 14.2 | MSAA | 2× measured earlier fallback | 4× required new baseline | QuestPipeline | Pending matching-view A/B |
| 14.2 | Render scale | 1.0 | 1.0, authored once; never below 1.0 | QuestPipeline | Pending; raise only with ≥2 ms peak-storm GPU headroom |
| 14.2 | Main light | Per pixel, shadows supported, 20 m | Per pixel; shadows OFF, 15 m ceiling, one cascade, 1024 | QuestPipeline | Pending |
| 14.2 | Soft realtime shadows | Off | Off | QuestPipeline | Pending |
| 14.2 | Additional lights | Per vertex, limit 2, no shadows | Per pixel, reserved for two future oil lanterns only | QuestPipeline | Pending; vertex fallback first if lantern cost exceeds budget |
| 14.3 | Renderer path | Forward | Forward | QuestRenderer.asset | Pending |
| 14.3 | Depth priming | Disabled | Disabled | QuestRenderer.asset | Pending |
| 14.3 | Integrated post | PostProcessData assigned | Null / OFF | QuestRenderer.asset | Pending |
| 14.3 | Renderer features | None | One On Tile Post Processing feature | QuestRenderer.asset | Pending Android Render Graph check |
| 14.3 | Grade | No dedicated on-tile profile | Neutral tonemap, neutral colour adjustment; no vignette/grain/bloom/DOF/SSAO/SSR | Assets/Settings/QuestOnTileVolume.asset | Pending |
| 14.3 | Render Graph | Enabled (modern settings); obsolete field says 0 | Enabled; compatibility mode off | URP global RenderGraphSettings | Pending |
| 14.4 | Stereo | Multi-view | Multi-view | Android OpenXR settings, m_renderMode=1 | Pending |
| 14.4 | Latency | Prioritize Input Polling | Same | Android OpenXR settings | Pending |
| 14.4 | Depth submission | None | None | Android OpenXR settings | Pending |
| 14.4 | Offscreen-only Vulkan | On | On | OpenXR Editor Settings | Pending |
| 14.4 | Meta support / symmetric projection | Support on / symmetric off | Retain; enable symmetry or region culling only after measured A/B benefit | Android Meta Quest Support | Pending, no unmeasured “optimization” claimed |
| 14.4 | Foveated feature / API | On / SRP | On / SRP; eye tracking off for fixed foveation | Android OpenXR feature/settings | Pending |
| 14.5 | Runtime FFR | 0.5, applied in Lab Start | 0.5; startup retry also covers lobby and delayed XR initialization | VisualPassSettings + QuestDisplayBootstrap | Pending lens-edge/text check |
| 14.6 | Dynamic viewport scaling | Feature off; no runtime viewport setter found | Off; URP renderScale is sole resolution setting | OpenXR + source scan test | Pending |
| 14.7 | Scenery GI | Existing authored static flags | Fixed scenery contributes GI; movable props excluded | Lab scene + quality bake menu | Pending |
| 14.7 | Light modes | Baked practicals plus realtime beacon | Fixed lights baked; powered/emergency variants | Lab scene + LightingPreset assets | Pending |
| 14.7 | Probes | Room grids / five reflection probes | Retain room/reflection probes; supplement stairs and outside approach | Lab scene | Pending moving-hand/ship coverage |
| 14.7 | Lightmapper | Progressive CPU, 10 texels/m, 1024 atlas | Request Progressive GPU; document any platform fallback | VisualPass.lighting | Bake evidence below |
| 14.8 | World texture mipmaps | Mixed; raw inventory retained | On for world texture imports; UI/font exceptions | Assets/Art, Assets/WatchDemo, future Assets/Sprint5 | Pending shimmer sweep |
| 14.8 | Android texture format | Mixed | ASTC 6×6 colour, 4×4 normals | TextureImporter Android overrides | Pending memory/GPU measurement |
| 14.8 | Anisotropic filtering | Mixed | Per Texture; ≥4 on grazing floor/wall/wood/stone, 2 elsewhere | Quest quality + TextureImporters | Pending |
| 14.9 | Graphics API | Vulkan + OpenGLES3 | Vulkan only | Android Player settings | Pending device launch |
| 14.9 | Backend / ABI | IL2CPP ARM64 | Unchanged | Android Player settings | APK verification |
| 14.10 | Usually-off checklist | Classic renderer post data assigned; rest off | HDR, classic post, depth/opaque copies, SSAO, depth priming, Deferred all off | QualityBaselineTests | 396/396 EditMode pass |
| 14.11–12 | Measurement/diagnosis | Earlier integrated-build samples, not this source | Same-view per-change OVR Metrics/Profiler/Frame Debugger/Render Graph | Device checklist | Pending |
| 14.13 | Automation | Legacy 2× asset test only | Apply menu, QualityBaselineTests, Android build guard | Assets/WatchDemo/Editor + Environment/Tests | 396/396 EditMode pass |

## Implementation details that matter

Unity 6.3's installed OnTilePostProcessFeature checks the **camera** post-processing flag. That flag must be ON while **QuestRenderer.postProcessData is null** so classic post stays absent. The feature uses a memoryless intermediate attachment on Android XR; it falls back to off-tile texture reads on desktop. Therefore desktop screenshots cannot establish its Quest cost or Render Graph pass behavior.

The vapour shader had an unconditional camera-depth lookup even though the pipeline depth copy was off. The baseline removes that hidden dependency in favor of radial/near-camera fading. Smoke remains a small translucent effect; it does not justify a full-screen depth copy.

The oil lanterns do not exist yet. Their two no-shadow per-pixel lights are an explicit future exception, not permission for new realtime wall lights. Existing baked-room probes and emissive fixtures remain the lighting foundation. Skybox and avatar-body assets are not edited.

## Bake and validation evidence

Actual **Progressive GPU** bake on Apple M2 Pro (OpenCL device named in log). Powered: **2 maps, 4.93 MiB, 17.4 s**. Emergency: **2 maps, 4.93 MiB, 10.7 s**. **444 probes**, **2,227 static renderers**; existing reflection probes retained. Maps are asset-file sizes, not GPU resident memory. Evidence: `Docs/Verification/S5/Quality/bake.txt`. No CPU fallback used.

Full EditMode after application: **396/396 pass**, including QualityBaselineTests. Before graphics Nights 1–3 pass through bed into Night 4, zero logged errors. After graphics Nights 1–3 also pass through bed into Night 4 with zero logged errors. Inspected matching F2 emergency views and the turned pipe bolt: layout/interaction preserved; new bake has slightly different bounce. These desktop views do not exercise Android-only on-tile/FFR. The clean-source development APK built successfully from 6c13885 (IL2CPP ARM64), and APK manifest/CRC/signature checks pass. See `Docs/Verification/S5/Quality/`. Nine texture imports were updated; four are avatar texture compression/aniso metadata only—the source pixels, body mesh, rig and materials remain unchanged. All 20 audited world texture imports have mipmaps and Android ASTC.

Pre-change full EditMode: 393/394. The valve retry test manually set pressure to crew mode without setting the watch's keeper count; retry correctly returned to one-player rules. Full PlayMode stalled in obsolete Bootstrap integration (log retained, run stopped). The current graphics replay is separately recorded; its old Day3Stage assertion and asynchronous lobby-start race are test setup issues, not reasons to rewrite gameplay.

## DEVICE CHECKS PENDING

Install only when ADB returns; do not launch without the lead's request. With permission to run: wear Quest, set 72 Hz, attach OVR Metrics + Profiler, capture 25–60 s each at House, F1, F2, F4, gallery sea and peak storm. Record GPU/CPU, sustained app FPS, draws, triangles and audio voices. Inspect both-eye text, foveation edges, baked powered/blackout states, reflection glass and smoke. Check Frame Debugger/Render Graph for unexpected depth/opaque/classic-post passes and off-tile fallbacks. Later repeat with ship + Drowned + two lanterns at peak storm before step 9.6 can close.

## Preview APK

[Download the Sprint 5 development preview](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint5-preview-20261112), source `6c138852227c2c44204bfd0514592a2d6ec36eb4`. SHA-256: `20c12e9c6c63efe820a11ff31966a9204ec310af2246b666e2f86ca16c2e692b`. It is a task/quality preview, not the completed ship/horror campaign. No Quest is connected, so installation and on-device measurements remain pending.

One-click scene preparation was rerun successfully with no serialized scene drift; expanded QualityBaselineTests **3/3 pass**, including baked fixed fixtures and route-probe coverage. This editor-only follow-up does not change the preview APK runtime content.
