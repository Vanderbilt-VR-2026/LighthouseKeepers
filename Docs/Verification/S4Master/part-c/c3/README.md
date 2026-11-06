# C3 practical lighting

Two 1024 lightmaps, 99 light-probe positions and five 128 room reflection probes baked. Warm fixtures use baked shadows; four cool area sources provide window spill. The moving beacon point light is the sole permitted realtime source; every other runtime Light is disabled after the central controllers update its cue colour/intensity. Existing fixture emissive lenses retain power/step feedback; generated guide lights receive wall-mounted housings. No skybox or avatar edits.

Central emergency material modulation preserves the powered bake. Powered/blackout House and F2 views reviewed; text, controls and floor remain visible in the captured dark state. Fog changes smoothly between cold exterior and thin warm interior; fixed window haze cards fade near the viewer; dawn tint is presentation only. Existing bounded GPU mist and ceiling dust retained. No bloom, DOF, SSAO or SSR added.

Night 1 physical repair→sleep passes; 31 focused rules checks pass. Runtime light inventory saved. Device contrast, sustained GPU≤11 ms and CPU≤9 ms pending. Bake commands: VisualLightingAuthoring.Bake (Editor); do not rebuild the shell to bake.
