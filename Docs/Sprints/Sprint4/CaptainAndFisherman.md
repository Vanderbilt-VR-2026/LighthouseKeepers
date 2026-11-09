# Captain and fisherman models

The multiplayer view now has two artist-assigned appearance slots. The host-authored roster selects **Captain** for the host and **Fisherman** for guests, including a guest who reconnects with a new actor ID. Role does not depend on the keeper name, roster order, player number or the viewing device being host.

The captain slot uses `Assets/Prefabs/Avatars/PF_BasicCaptain.prefab`. The fisherman slot now uses `PF_ReferenceFisherman.prefab`, a textured head/coat version of the requested yellow-raincoat reference. The earlier `PF_BasicFisherman.prefab` remains available. The in-game prefabs are modular head/coat models, with the existing tracked gloves supplying the hands; they do not add full-body IK. See [basic model source and checks](../../../../Tools/Avatars/README.md) and [reference-inspired rigged fisherman](../../../../Tools/Avatars/ReferenceFisherman.md).

Replace the slots on `CrewAvatarView` in `Assets/Prefabs/Network/Resources/PF_PlayerRig.prefab` with your final artwork when ready. Empty or invalid model slots retain the existing grey fallback. Both participants need a build containing the same assigned artwork. No new avatar SDK, role-selection packets or network behaviour are introduced.

## Tools and places to look

- [Blender](https://www.blender.org/) is the recommended free modelling, UV, texture-painting and export tool. Start from a reusable body and customize clothing instead of modelling anatomy from scratch.
- [MakeHuman](https://www.makehumancommunity.org/) can provide an editable base person. Check the license of any clothing or other assets used with it.
- [Mixamo](https://www.mixamo.com/) provides automatic humanoid rigging and animation. It does not create a raincoat or captain's uniform and does not supply VR hand/head IK.
- [Sketchfab fisherman search](https://sketchfab.com/search?type=models&q=fisherman), [Fab fisherman search](https://www.fab.com/search?q=fisherman), and [Unity Asset Store fisherman search](https://assetstore.unity.com/?q=fisherman&orderBy=1) are starting points for premade art. Search also for `ship captain character`, `yellow raincoat fisherman`, `sou'wester fisherman`, and `low poly sailors`. These are catalogue search links, not endorsements or verified downloads of a particular product.

Choose both models in a consistent art style. Confirm the asset permits use in a game and team repository, has usable meshes/materials/textures rather than only a render, and supports your intended rig. Prefer modest mesh/material counts and inspect URP compatibility. Begin with roughly 5–15k triangles and 1k textures per character as an art target, then measure the actual shared-session Quest workload; these values do not establish performance acceptance.

## Make your own artwork

1. In Blender, establish metre scale and make one reusable body/head base. Duplicate it so the two characters have compatible proportions.
2. Give the captain a navy coat, cream or white peaked cap, and restrained brass/gold details. Give the fisherman a yellow oilskin coat with a hood or sou'wester, darker underlayers and workwear details. These are suggested designs, not current bundled models.
3. Create broad clothing silhouettes first; add collar, seams, pockets, lapels and hat details after the characters read clearly at multiplayer viewing distance.
4. UV unwrap and paint or bake the materials. Use roughness variation for cloth versus oilskin and a subtle normal map for seams instead of adding many small mesh details. In Unity URP/Lit, smoothness is the inverse of roughness; verify the imported texture channel setup.
5. For the modular VR path below, keep the head/hat and coat/body independently movable. Retain the game's tracked gloves initially. A full-body design with articulated arms/legs requires a separate skeletal/IK integration; do not split or distort a downloaded skinned character merely to force it into this adapter.
6. Export FBX plus textures. Apply transforms, verify metre scale and local +Z forward in Unity, and correct artist mesh orientation below the tracking anchors. Export FBX rather than depending on Blender being installed on every teammate's computer.

## Current modular VR prefab contract

`CrewAvatarModel` is an adapter for separate head/body visual branches. It is not a full-body Animator/IK controller. The existing pose stream tracks eyes and wrists, so it cannot directly supply tracked hips, feet or elbows to a complete humanoid. Choose/add that IK and locomotion integration after the actual full-body model is selected if full-body characters are required.

Create a prefab for each role with this structure:

```text
CaptainModel or FishermanModel         (CrewAvatarModel component)
  HeadAnchor                          (origin at the eyes, facing +Z)
    Head mesh, cap or hood meshes
  BodyAnchor                          (upright torso/coat pivot)
    Body/clothing meshes
  LeftWristAnchor                     (optional, independent branch)
    Custom left glove mesh
  RightWristAnchor                    (optional, independent branch)
    Custom right glove mesh
```

1. Use a root at unit scale. Assign HeadAnchor and BodyAnchor to the component. Head and body must be separate branches; a head bone nested in a humanoid skeleton's body is not compatible with this modular adapter.
2. Adjust **Body Offset From Head** to your artwork's pivot, in metres; default is `(0, -0.48, 0)`. Put mesh-origin/import corrections on children below each anchor, because the network view controls the anchors' world position/rotation.
3. Leave the optional wrist fields empty to keep the existing tracked gloves. If supplying hands, use distinct wrist anchors with grip alignment matching the current controller poses. Avoid importing a second pair of baked hands in the body mesh.
4. Do not include cameras, audio listeners, NetworkObjects or NetworkRunners in appearance prefabs; validation rejects them. Runtime instances have colliders disabled and rigidbodies made kinematic/noncolliding. Keep any animation from driving tracking anchors.
5. Use the `CrewAvatarModel` component's context menu **Validate avatar model** to check the anchor/component setup, then save both prefabs. Outside Play Mode, open `PF_PlayerRig.prefab`, select the root's `CrewAvatarView`, and drag them into **Captain Model** and **Fisherman Model**. Preserve the existing network identity/component and prefab GUID. No Fusion behaviour is added, so this does not require a new replicated role field.
6. Enter a shared lobby with matching builds. The guest should see the captain, and the host should see the yellow-raincoat fisherman. Local full avatars remain hidden as before; your first-person hands remain the existing gloves.

The view retains current nameplates, head/wrist pose application, torso yaw smoothing and stale-pose visibility. A delayed roster snapshot retries role lookup without inventing a captain. Invalid configured art logs a warning and falls back instead of altering the XR rig or session.

## Verification and remaining work

- Five selected portable NUnit tests passed for host identity independent of player number/order, rejoining guest identity, misleading display names, missing roster entries and empty IDs. These exercise role selection, not Unity rendering or Fusion traffic.
- C# syntax checks passed on all five changed/new scripts with release and Editor/development symbols; repository hygiene and reference/meta checks passed. Unity API compilation is not established by syntax checking.
- Basic models, the reference-inspired fisherman, default prefab assignment and offline Blender previews are complete. Portable full-body rig exports and the modular in-game version have separate validation/scope in the source guides. Unity import/rendering, shared-session validation, APK building and Quest profiling remain pending. The official Unity Editor download endpoint remains blocked in this machine. Blender preview lighting does not establish Unity/Quest appearance.
- After assigning models, test turning, leaning, crouching, close hand interactions, coat/head intersection, local visibility, both eyes, guest rejoin, swapped hosting roles and target-headset performance. For a full humanoid, additionally verify its Animator/IK/locomotion integration rather than assuming auto-rigging provides it.
