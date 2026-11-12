# Next world-art pass

C0–C5 establish the materials, baked lighting, stable shell, cheap water and basic keeper props. Further dressing should preserve the current interaction anchors and clear standing zones. Will owns skybox and avatar body.

| Area | Present | Next dressing | Working target: draws / visible triangles |
| --- | --- | --- | --- |
| House | Limewashed masonry, timber bench, chalkboard/PA, logbook, coat, photo, crate/rope, kettle | Replace primitive coat/stove with one authored low-poly set; weather doorway and desk edges | 100 / 160k |
| F1 | Wall pipe/hex bolts, water marks, murky flood/ripples, seep/runoff | Hand-placed salt streaks and wet splash decals matched to pipe spray landing; contact foam masks around every prop | 120 / 180k |
| F2 | Diesel generator, worn enamel/iron/brass, breaker, practical lights | Tool drawer, fuel stains, spare hose; consolidate station draw calls with atlas after interaction review | 130 / 200k |
| F3 | Safe bunk route, practical lamps, job slate, pole landing | Bedding mesh, tucked blanket, chart/logbook shelf; no props in the transfer corridor | 110 / 170k |
| F4 | Transparent Fresnel rings, amber core, brass, radio, reflective rain panes, gallery | Better bevelled lens mesh and brass supports; detailed but low-poly gallery joints | 140 / 240k |
| Coast | Combined rocky ring + open eastern inlet, wet threshold, storm-wave mesh | Author cliffs/shore silhouette and a supported, guarded exterior path before making more coast walkable | 150 / 300k |

These are planning allocations, NOT measured budgets. Final hard limits: GPU ≤11 ms, CPU main ≤9 ms at 72 Hz, ~150 draws per eye, ≤300k visible triangles, audio ≤24 voices, all water ≤2 ms. The new ocean is 32,768 triangles; rock pieces are combined into one renderer. Profile practical full-room views rather than summing isolated mesh counts. If over budget: lower render scale first within 1.0–1.4, reduce transparent cards and rock mesh density, then combine decorative station geometry. Preserve readable lighting and interaction response.

Sources: continue [Poly Haven CC0 models](https://polyhaven.com/models) and the materials recorded in AssetSources.md. Consider Unity Asset Store or Sketchfab only after recording the specific item's licence and attribution; none has been imported by this pass. Custom Blender meshes are appropriate for coat/stove, keeper props and bevelled lens; export fixed metre scale, low-poly meshes, shared atlas, convex interaction colliders separate from render geometry. No new asset/plugin purchase is assumed.

Remaining visual acceptance: continuous on-device wall sweep; transparent lens/rain in both eyes; storm/ocean silhouette against Will's skybox; water transparency and foam quality; powered/blackout readability; 72 Hz profiling and draw/triangle/audio counts. C5 props are a coherent first dressing pass, not final bespoke art. Current flood boundary foam is analytic; it does not yet trace every arbitrary prop. Current droplets use bounded Unity particles, not a compute-particle framework. Do not claim those refinements complete.
