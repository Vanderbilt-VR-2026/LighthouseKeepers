# Keeper body from the November 16 references

The historical photograph supplies human proportions, canvas bib overalls, rolled cuffs and a working cap. The illustrated keeper supplies a dark sea coat, scarf, grey facial hair and belt kit. The result is an original Blender model with a quiet, worn workwear palette. The two supplied pictures are references, not textures pasted on the model. The anatomical head reuses the existing MakeHuman CC0 subset; provenance remains in `Tools/Avatars/Sources/`.

The previous fisherman assets remain available. Both host and guest appearance slots now use `PF_HarborKeeper`. Both roles intentionally share this costume; names and host/guest identity remain separate. It shares the existing 51-bone, tracked-head-and-wrist adapter, pose interpolation, finger curls and network identity. Local tracked hand geometry, hand rotations, grips and interactions are unchanged. No replicated field was added: network remains **LK-S5-DREAD1 / LK-Lab-Coop-v33**.

## Legs and first-person embodiment

The head and hands are tracked; hips, knees and feet are inferred. The feet stay planted during small head turns, step round larger turns, alternate while walking, lift over supported stairs, and adapt to ground normals. Hips lower and knees bend during crouching. Teleports reset the stance. This implements the grounded full-body feel requested by reference to Blade & Sorcery; it does not use that game's code, promise identical behavior or claim actual foot tracking.

The new prefab enables planted foot orientation and caps leg stretch at 1.06. The original prefab retains its previous solver settings. Boots have no collision, so they cannot push the camera or change locomotion. There is no physics ragdoll, cloth simulation, full-body tracker support or jump animation in this pass.

In VR, a separate cosmetic skin shows the local coat, arms and legs. Its head, gloves and camera-adjacent collar geometry are excluded at export so the camera cannot sit inside a head and the existing tan hands remain the only local hands. The local skin is not another network avatar. It has no camera, listener, collider, interaction component or gameplay authority. It follows the same inferred solver; stereo waist placement and comfort still needs a headset check.

## Art and cost

- Editable source: `SourceArt/Avatars/HarborKeeper.blend` (packed maps), outside Assets so Unity never needs a Blender installation.
- Repeatable authoring: `Tools/Avatars/build_harbor_keeper.py`, reusing the established rig exporter and material baker.
- Full body: 22,739 triangles, 18,049 exported seam vertices, 51 bones, one material.
- 1024px albedo/normal/smoothness maps, mipmaps; Android ASTC6x6 colour/data and ASTC4x4 normals. No additional lights, physics or network messages.
- The first-person body has 11,261 triangles; this adds a skinned draw and must be profiled on Quest. Triangle counts are not GPU measurements.

```sh
cd ~/LighthouseKeepers-M3
/Users/aneeshvasamreddy/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --threads 4 --python-exit-code 1 --python Tools/Avatars/build_harbor_keeper.py
caffeinate -i python3 artifacts/remaining/m3/tests.py keeper-repeat 'HarborKeeperTests;CrewHumanoidAvatarTests;CrewLimbIKTests;CrewAvatarRoleTests;CrewSpatialVoiceTests;CrewVoiceAcousticsTests'
```

Acceptance: inspect front/side/back and crouch, both players see the new keeper, tracked hands remain responsive, feet plant while turning and on stairs, no own head/extra gloves, no new session or task failures. Evidence and unverified items are recorded separately in `Docs/Verification/S5/Keeper/`.
