# Reference-inspired fisherman

This model reconstructs the approved yellow-raincoat fisherman reference as editable 3D geometry: raised hood, long oilskin coat, storm yokes, patch pockets, front snaps, fitted beard, brown gloves, dark trousers and rubber boots. It is a reference-inspired approximation, not a pixel-identical reproduction of the concept's facial sculpt, fabric folds or lighting.

## Two deliverables

- **Portable full-body model:** `artifacts/Avatars/ReferenceFisherman/Fisherman_Rigged.blend`, `.fbx` and `.glb`, with a 51-bone skeleton, skin weights and embedded/packed textures. The full mesh is 26,496 triangles, 13,653 Blender vertices and approximately 1.826 m tall. Hands include finger bones. There are no animation clips or facial expressions. The game now has a procedural VR adapter described below.
- **Static alternative:** `Assets/Prefabs/Avatars/PF_ReferenceFisherman.prefab`, derived head/coat meshes with baked UVs/normals/smoothness and one shared URP material. This remains available as an alternative; the guest slot now uses the full-body variant. It uses the existing tracking/gloves and does not attach the full-body skeleton to the XR rig. Head plus coat total 15,335 triangles in two mesh renderers/material slots. Actual Quest cost remains unmeasured.

The original basic fisherman remains available. The network prefab's identity, pose protocol, ownership and gameplay rules are unchanged. Replace the guest appearance slot as documented in [the artist guide](../../Docs/Sprints/Sprint4/CaptainAndFisherman.md). The new `PF_RiggedFisherman.prefab` uses the complete skin with head/wrist-driven arms, finger curls, crouching and inferred footsteps. See [implementation, preview and acceptance steps](../../Docs/Sprints/Sprint4/FishermanFullBody.md). Unity/device acceptance is still pending.

## Rebuild the art

Blender 4.3+ is required to regenerate, but not to import the committed native Unity appearance assets.

```sh
blender --background --factory-startup --threads 4 --python-exit-code 1 --python Tools/Avatars/build_reference_fisherman.py -- --preview
```

This generates the versioned `FishermanRig.bytes` full skin, native head/coat assets, material and 2048px albedo, tangent-normal, roughness and packed Unity smoothness textures. It writes portable full-body files and actual Blender renders (`Fisherman-preview.png`, front/side/back images, and `Fisherman-turnaround.png`) under the ignored artifact folder. Images are renders of the mesh, not new concept illustrations or Unity screenshots.

Use an explicit output copy for artist edits: rerunning the generator replaces its generated art files. The source `.blend` contains packed textures, a single full-body rig/mesh and studio lights/camera. Derived head/body objects are hidden and are separate static export sources. The source script and committed native assets allow regeneration if ignored artifacts are not present in a fresh checkout.

## Mesh provenance

The anatomical head is adapted from MakeHuman's **CC0-1.0** base mesh. Only the head/upper-neck subset is bundled in `Sources/MakeHumanHead.obj`; MakeHuman program code is not used. The original OBJ explicitly declares its September 2020 CC0 release. The upstream commit, complete source hash, extraction rule and copyright credits are recorded in `Sources/MakeHumanHead.source.json`; the full asset license is `Sources/MakeHuman-CC0.txt`.

Upstream: [MakeHuman base mesh at the recorded revision](https://github.com/makehumancommunity/makehuman/blob/a8bc2d54ff0ac92e78ff71431b1023eda42bf482/makehuman/data/3dobjs/base.obj), [asset license](https://github.com/makehumancommunity/makehuman/blob/a8bc2d54ff0ac92e78ff71431b1023eda42bf482/LICENSE.ASSETS.md). Clothing, hood, gloves, boots, procedural surface details, skeleton and generated material maps are created by this repository's Blender script.

## Validation performed

```sh
python3 Tools/Avatars/validate_reference_fisherman.py
python3 Tools/Avatars/validate_basic_keepers.py
blender --background --factory-startup --threads 4 --python-exit-code 1 --python Tools/Avatars/validate_fisherman_rig.py
```

- Blender built, baked, exported and rendered the model. Every full-body vertex has normalized weights to actual bones; the initial arm pose check moves more than 1,000 vertices.
- FBX **and** GLB were imported into fresh Blender scenes. Both preserve a 51-bone skinned mesh at the expected metre scale; arm, leg and head pose checks each deform more than 100 vertices. Reports are written beside the exports.
- The GLB has embedded images, 51 skin joints and normalized exported skin weights. Its UV-seam-expanded vertex count is 20,014.
- Native mesh checks cover packed positions/UVs/normals/tangents, nondegenerate triangles, index ranges and material/texture/prefab references. Source/meta pairing, GUID uniqueness, YAML parsing, Python/C# syntax and repository hygiene were checked. The basic model generator was exercised after adding textured export support; the previously authored basic asset bytes are retained, and their mesh checks pass.
- The Unity Edit Mode model-import tests now include this prefab and texture channels. These tests, actual Unity import/rendering, humanoid Avatar mapping, live networking, a fresh APK and target-headset execution/performance remain **unrun** here. Blender round trips do not establish Unity humanoid compatibility or VR tracking behaviour.

## Use the full-body files in Unity

1. Import `Fisherman_Rigged.fbx` into your chosen art folder. Set **Rig → Animation Type → Humanoid**, **Avatar Definition → Create From This Model**, then **Apply → Configure** and inspect the mapping/T-pose. This step has not been validated automatically in Unity.
2. Assign the committed `FishermanSurface.mat` to the imported mesh, or configure an equivalent URP/Lit material using albedo, a Normal Map import for `Fisherman_Normal.png`, and `Fisherman_Smoothness.png` as Metallic/Smoothness (RGB is nonmetallic; alpha is smoothness). The standalone roughness map is for tools which expect roughness instead.
3. Preview arm/leg/head deformation and inspect gloves, shoulder joins, coat/leg intersection and neck/hood clearance. This basic rig will need further weight painting for extreme poses or production animation; cloth simulation and facial animation are not included.
4. The bundled model already has a dedicated procedural adapter through `PF_RiggedFisherman` and `CrewAvatarModel.FullBody`; its runtime data is exported directly from the source rig. The FBX route above is for artist editing/alternative humanoid animation, not a required installation step. Different downloaded skeletons still need mapping/integration. Unity/device validation is required for either route.
