# C0 render quality

Android Quality index 0 (Quest Development) and Graphics fallback both reference QuestPipeline, GUID 8211712c856274fca823aa424774416a. The unused Mobile_RPAsset is unchanged. Active MSAA remains 4; render scale 1.00→1.25 (56.25% more raster pixels, **not a GPU timing measurement**). Android OpenXR fixed foveation enabled; runtime requests medium (0.5), no eye tracking. Device cost and final scale pending.

Objective/subtitle text now uses TMP SDF. Fixed equipment writing is rasterized on caption changes to mipmapped textures; a Metal render-target vertical flip was caught in screenshots and corrected. Composition-layer escalation remains device-pending if SDF shimmers. No screen grain.

All 36 authored scenes scanned: 1,509 coplanar pairs → zero at 1.5 mm plane / 0.002 m² overlap tolerance. Shared visual mesh copies separate surfaces; collider meshes, transforms, object references and LOD membership stay intact. Two-sided/opposed faces are excluded intentionally. This audit checks authored active renderers, not generated runtime props. All-scene regression added.

162 authored lights inventoried; Lab had 33, including nine realtime. Authored realtime lights converted to Baked. Runtime one-light limit is staged behind the successful C3 bake marker so this checkpoint does not darken the game before baking. C3 restores the moving beacon lamp as the one realtime source. GPU/CPU, aliasing and readability in both eyes: DEVICE PENDING.
