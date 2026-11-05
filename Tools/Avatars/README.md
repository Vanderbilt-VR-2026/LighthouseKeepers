# Basic captain and fisherman models

These original temporary upper-body models are assigned to `PF_PlayerRig` through the existing `CrewAvatarModel` slots:

- `Assets/Prefabs/Avatars/PF_BasicCaptain.prefab`: navy coat, white peaked cap, brass badge/buttons and epaulettes.
- `Assets/Prefabs/Avatars/PF_BasicFisherman.prefab`: yellow oilskin coat, hood with lining, storm flap and pockets.

Each prefab contains separate head/body anchors, two static mesh renderers and references to shared URP/Lit materials. They have no colliders, cameras, listeners, animator, network identity, articulated arms or legs. The existing multiplayer view controls their head and torso; its existing tracked gloves supply the hands. No full-body rig or IK result is claimed.

The four native Unity mesh assets and eight shared materials are in `Assets/Art/Avatars/BasicKeepers`. Teammates can import the committed assets without Blender or an external model-import package. These assets are generated from original geometry in `build_basic_keepers.py`; no external character artwork or license is required.

## Regenerate or edit

Run from the repository root with Blender 4.3+ available:

```sh
blender --background --factory-startup --python Tools/Avatars/build_basic_keepers.py
```

Optional source scene and offline render:

```sh
blender --background --factory-startup --python Tools/Avatars/build_basic_keepers.py -- --preview
```

The optional command writes `artifacts/Avatars/BasicKeepers.blend` and `BasicKeepers-preview.png`. The `.blend` contains the actual model geometry and a preview camera/light setup. Open it in Blender to inspect/edit; it is a static scene, not a rigged character. The image is a Blender geometry preview, not an in-game screenshot. The preview omits runtime gloves and nameplates.

The generator preserves stable asset GUIDs and does not change the network prefab's appearance slots. Rerunning it overwrites the generated basic model meshes/materials/prefabs, so place final artist models in their own assets and assign them through the existing slots instead of editing generated assets in place.

## Validation

```sh
python3 Tools/Avatars/validate_basic_keepers.py
python3 Docs/validate_repository.py
```

Actual checks performed: Blender generated and rendered both models; the standalone mesh validator checks finite packed vertex data/normals, triangle winding, nondegenerate triangles, index/submesh ranges and bounds. It also resolves the model/script/material references and both default network assignments. The captain is 3,361 triangles; the fisherman is 3,300 triangles. Each currently has nine material slots across its two meshes; draw calls and lighting cost require measurement rather than inference from polygon counts.

`BasicKeeperModelTests` adds three Unity Edit Mode cases for actual mesh import/readability, valid model anchors, material/submesh alignment and default role-model assignment. These have **not run here**. C# syntax, source/meta pairing, GUID uniqueness, repository hygiene and whitespace were checked. Unity import, graphics execution, shared-session behaviour, fresh APK and target-headset performance are still unverified in this environment.

## Try and replace

1. Pull `william/sprint-4`, open Unity 6000.3.23f1, and wait for import/compilation. Run `BasicKeeperModelTests` in the Edit Mode Test Runner.
2. Drag either basic prefab into an empty scene to inspect it, or open it in Prefab Mode. No authoring/build command is needed to assign the models; the network prefab already references them.
3. Start a matching host/guest session. The guest should see the captain, and the host should see the yellow-coated fisherman. Their first-person bodies remain hidden; the existing local hands remain unchanged.
4. Test head turns/tilt, leaning/crouching, stairs, glove alignment, nameplates, session leave/rejoin and both-eye rendering. These are temporary upper-body shapes and may intersect at extreme head/torso angles.
5. Open `Assets/Prefabs/Network/Resources/PF_PlayerRig.prefab` and replace **Captain Model** / **Fisherman Model** with your own compatible prefabs. The [artist model guide](../../Docs/Sprints/Sprint4/CaptainAndFisherman.md) describes anchors and the full-body IK boundary.
