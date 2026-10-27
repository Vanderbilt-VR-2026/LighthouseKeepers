# Environment shell pipeline

[BUILT-UNVERIFIED] Batch A authors original greybox geometry only. No production scenes, player prefab, simulation, or packages are replaced. Headset acceptance is separate from exporter tests.

Run from the project root:

```sh
blender --background --factory-startup --python-exit-code 1 --python tools/blender/build_all.py -- --out Assets/Art/Generated --manifest artifacts/art-manifest.json --greybox
```

The standard invocation without `--python-exit-code 1` also works, but Blender can return exit zero after an uncaught Python error; CI must include this flag. `--only F4` regenerates one family during iteration; run the complete export for a clean build. All mesh objects have named collision proxies; render-only rails/treads/fixtures declare `collisionEnabled:false`, because continuous safety barriers and smooth stair ramps supply their collision. Import does not add thousands of tiny hand-catching colliders.

[VERIFIED] Coordinates are explicitly converted from Unity XYZ to Blender (-X,-Z,Y), then FBX uses -Z forward/Y up, unit scale1, FBX_SCALE_NONE, bake_space_transform. Unity's importer produced the correct one-metre cube, 1.70 m marker, and +X orientation only after this handedness conversion. Test the imported bounds; do not infer orientation from root Euler angles alone.

Unity menu: **Lighthouse / Rebuild Shell Scenes**. Batch entry: `LighthouseKeepers.Editor.Environment.LighthouseBuilder.BuildAndValidate`. The builder protects unsaved scenes, keeps all legacy scenes untouched, and never replaces Dress scenes. Mesh/prefab GUIDs survive regeneration. The build stamp makes repeated unchanged-input runs byte-identical against the committed generated assets; independent clean-room Unity local-file-ID generation is not claimed deterministic.

Validation entries:

- `EnvironmentValidation.RunGeometry`: imported collider navigation, curved landings, work discs, touch heights, model scale.
- `ShellBuildCache.VerifyTwice`: unchanged-input byte identity.
- `EnvironmentLightingBake.BakeAndValidate`: powered/emergency Lab bakes and linear render measurements (graphics device required).
- `LuminanceValidator.CalibratePoweredReference`: explicit first calibration only; refuses to overwrite an existing reference.
- `EnvironmentBuild.BuildAndroid`: isolated Lab APK, explicit scene list; never changes EditorBuildSettings.

All entry names above use namespace `LighthouseKeepers.Editor.Environment`. Reports, screenshots, test XML, APKs and headset recordings belong under ignored `artifacts/`. The tracked `ArtManifest.json` is a copy of the exporter manifest so a fresh clone can import without ignored files.

[PROPOSED] Surface textures, detailed fixtures, station machinery, ocean/storm dressing and final production lighting integration belong to later art batches. The two Lab presets own the Lab's complete lightmap set; Nirvaan must coordinate that global lightmap ownership before production adoption.

[VERIFIED] `verify_floor_topology.py` imports the exported F1–F4 FBXs and checks each slab is one connected, closed manifold after welding coincident vertices. It applies the FBX object transform before reporting volume. Run with `blender --background --factory-startup --python-exit-code 1 --python tools/blender/verify_floor_topology.py`. This guards against reintroducing the internal wedge faces that caused radial lighting seams.
