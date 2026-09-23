# Environment architecture

Eight authored scenes use world metres, not runtime procedural generation. Bootstrap owns the only XR Origin, listener, interaction manager, day/flood/storm/audio controllers. Content scenes contain no duplicate player. The loader switches the active scene to Exterior after loading so its sky/fog settings apply.

| Scene | Ownership and contents |
|---|---|
| LK_Bootstrap | Integration: persistent player, managers, additive scene list |
| LK_Exterior | Storm, ocean, rocks, exterior rain/ambience and daylight |
| LK_Core | Structural tower, keeper house, windows, shared stair and floor circulation |
| LK_Level01_Plumbing | Pipes, pump, leaks, flood surface, repair and pressure anchors |
| LK_Level02_Generator | Engine, fuel, starter, intake, breakers |
| LK_Level03_Communications | Bunk/mess, radio, charts, remote bearing/pressure anchors |
| LK_Level04_Lantern | Lens, drive, controls, protected balcony |
| LK_DevGym | Standalone player, doorway/ramp/grab/flood/audio tests |

Floors are at 0, 3.2, 6.4 and 9.6 metres. The spiral has a 1.6 m tread width, 1.8 m centreline radius and one revolution per floor. A 96-segment smooth ramp collider supports visual treads without collider lips. Doors and protected windows connect the staging house and balcony; invisible guard colliders are backed by visible railings. The shoreline is a vista, not a traversable island.

Static render batches combine compatible authored meshes by material per content scene while retaining source colliders. Disabled source renderers remain editable; regenerate batches after structural edits, or temporarily re-enable source renderers and disable the corresponding batch. Do not render both. This trades fine-grained culling for fewer draw submissions in a small environment; device profiling must determine the best balance. Basalt LOD groups remain separate. Light probes are authored; baked lighting/probe data must be checked in the final handoff status before relying on it.

Original wear textures are 128×128. Ocean and sky shaders use URP-compatible stereo macros and inexpensive animated shading. Rain is localized with capped particles. The scene does not use volumetric fog, screen-space post effects, HDRP or networking.

## September 23 asset integration

Selected vendor props are nested in game-owned prefabs under `Assets/_LighthouseKeepers/ThirdPartyVariants/`. See AssetIntegrationStatus.md for per-scene replacements and measured circulation. Retired source geometry stays disabled and excluded from rebuilt static batches. Puzzle transforms/anchors and scene ownership remain unchanged. Baked practical fills illuminate the new props without additional runtime shadow lights. Offshore fog is bounded away from the building; the ocean uses a dedicated opaque Quest fallback.
