# Fisherman full-body posing

`PF_RiggedFisherman.prefab` replaces the static guest head/coat in `PF_PlayerRig`. The captain still uses the basic modular model. This change is cosmetic: it consumes the existing 110-byte head/wrist/grip frame and does not add gameplay authority, collisions, tracking cameras, input bindings or network messages.

## What moves

- Eyes follow the sampled headset pose, including tilt; the body turns toward headset yaw.
- Upper/lower arms solve toward the existing controller wrists. Inspector wrist corrections allow glove alignment adjustments. Arms have bounded stretch; physically unreachable poses stop at the reach limit.
- Grip/trigger values and the existing discrete hand poses curl the generated finger chains.
- Hip height follows crouching, with inferred forward-bending knees. Boots use static walkable surfaces on the same default layers as `GroundedSlopeMovement` (Default and layer 8). Dynamic props, triggers, steep surfaces and the local XR Origin are excluded.
- Feet plant, alternate short lifted steps as the head moves/turns, and reset after a teleport, re-enable or long rendering gap. Small height changes select the upper walkable surface rather than the floor below stairs.

Only the head and controllers are tracked. Hips, elbows, knees and feet are estimates, so this is procedural VR posing, without mocap clips, real foot tracking, cloth or facial animation. Extreme crouches/reaches, stairs, coat intersections and controller palm orientation need device review and potentially more weight painting. The local player's first-person hands keep their existing presentation; the host observes the remote guest's fisherman.

## Quick preview in Unity

1. Open the project with its pinned Unity version and let scripts/assets import.
2. Choose **Lighthouse Keepers → Sprint 4 → Avatars → Preview full-body fisherman**. Save existing scene changes if prompted. This opens a separate **unsaved** scene; it does not alter build scenes.
3. Inspect the model in **Scene** view. Use the **Fisherman poses** window: crouch, grip, reach forward and walk in a circle. The preview runs the actual skin builder/pose solver with synthetic tracking. Expect articulated elbows, curled gloves, bent knees and lifted walking steps.
4. Run **Window → General → Test Runner → EditMode**, including `CrewLimbIKTests`, `CrewHumanoidAvatarTests` and `BasicKeeperModelTests`. The integration tests exercise skin initialization, tracked eyes/wrists, crouching, fingers, stepping, stacked floor geometry, teleport reset and invalid tracking. Close the preview scene without saving when done.

This preview verifies visual/solver behavior only after it is actually run; it does not connect Photon or exercise controllers.

## Shared-session and headset acceptance

1. Build both devices from the same updated branch. An older installed APK will keep the static model. Use the usual game entry, have one player host and the other join the same room, then launch together.
2. The **host** should see the yellow-raincoat guest; the **guest** should still see the basic captain. Check that no second grey gloves/torso appear and no full body obscures the local player's view.
3. Have the guest look around, reach at waist/shoulder height, squeeze/release grip and trigger, crouch, walk forward/back/sideways, rotate, climb the existing slopes/stairs and teleport. Observe wrists, knees, boot contact, hood/neck and coat intersections from the host.
4. Grab/release existing gameplay props, change floors, disconnect/rejoin and swap host roles. The visual body must not move the user's camera, block objects or change interaction authority. Check the Console/device log for initialization/pose errors.
5. Profile the shared gameplay scene on the target Quest: CPU/GPU frame times, GC allocations, draw calls, headset/refresh rate and sustained run duration. The full skin has **26,496 triangles, 20,079 UV/normal/tangent seam vertices, 51 bones and one material/rendering submesh**. Its cost has not been measured on headset.

## Implementation and source

- `CrewHumanoidAvatar` builds one runtime skin per remote fisherman, caches bones/scratch arrays and reuses a bounded raycast buffer. Parsed mesh arrays are released after upload; the instance destroys its owned mesh on teardown. Materials are shared. Skeleton transforms are siblings to avoid inherited shear when a limb stretches; logical parent relationships drive fingers/toes.
- `CrewLimbIK` and `CrewFootstep` are allocation-free geometry/state helpers, also exercised by portable NUnit tests.
- `FishermanRig.bytes` is committed, versioned JSON skin data, not an APK. It includes baked metre-scale positions, normals, UVs/tangents, four weights per vertex and rest joints. Unity needs no Blender installation or additional avatar package to use it.
- The [Blender generator](../../../Tools/Avatars/ReferenceFisherman.md) now exports this data too. To re-export from an existing source file:

```sh
blender --background artifacts/Avatars/ReferenceFisherman/Fisherman_Rigged.blend --threads 4 --python-exit-code 1 --python Tools/Avatars/export_fisherman_skin.py
python3 Tools/Avatars/validate_fisherman_skin.py
blender --background artifacts/Avatars/ReferenceFisherman/Fisherman_Rigged.blend --threads 4 --python-exit-code 1 --python Tools/Avatars/validate_fisherman_skin.py
```

Keep the full-body component only on an appearance prefab with `CrewAvatarModel.FullBody` assigned. Clear modular anchors for that variant. Existing basic/static prefabs remain compatible and can be assigned as alternatives; their assets and GUIDs are preserved. Do not assign an arbitrary FBX directly to this data adapter: a different skeleton needs an exporter/mapping and proportion review.

## Evidence from this cloud environment

- Six production limb/footstep NUnit tests passed under .NET, including folded/collinear targets, reach/stretch limits, pole bending, planted feet, landing and reset.
- The exported skin passed finite geometry, normalized weights, valid joint/index ranges, nondegenerate triangle and tangent-basis checks.
- Rest and posed runtime bind/weight deformation matched Blender's evaluated mesh within **0.000001 m**, including arm, leg, head and finger motion. This checks exported skinning, not Unity rendering or VR behavior.
- C# syntax checks passed with release and Editor/development symbols. Native static model regression checks passed.
- **Unity API compilation, the new Unity integration tests/preview, Photon session execution, APK building and headset acceptance/performance remain unrun:** no Unity Editor is installed in this environment. Portable tests and Blender comparisons do not establish those results.
