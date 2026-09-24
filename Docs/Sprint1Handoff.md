# Sprint 1 handoff

Current team editor: **6000.3.23f1**. The migration passed static validation, Play Mode, four Edit Mode tests and an Android development build (zero errors, one warning). See [Unity63Migration.md](Unity63Migration.md). Earlier device records concern the 6000.5 build; the migrated APK requires headset acceptance.

This repository contains an authored environment foundation, not a finished survival game. The first Quest test revealed red/pink lighting, non-working controls and block-shaped hands. These were acceptance failures; successful compilation/build alone did not mean the sprint was complete. See Verification/DeviceVerification.md for the replacement build's actual acceptance status.

## Completed and verified by automated/editor checks

- Eight authored scenes, six additive content scenes loaded by Bootstrap, a separate DevGym, one XR Origin and AudioListener in the exploration environment.
- Quest package/platform configuration, independent application identity, original materials/meshes, four connected levels, keeper house, protected balcony, ocean/rocks/storm backdrop.
- Static validation of material/script/prefab references, required objects/layers, 16 puzzle sockets across six station identities, four flood thresholds and five ordered day profiles.
- 285 spiral ramp support/head-clearance samples. This does not replace a full controller-driven route test.
- Play Mode checks: additive startup, enabled move/snap actions, injected-input movement and snap rotation, flood crossing/reset, Day 5/Day 1 application, five rendered views.
- AudioMixer's twelve groups and seven snapshots; separately generated interior/exterior rain, thunder filtering and zone transitions.
- CPU light bake, 10 lightmap atlases, 32 probe positions, conservative mobile renderer and static material batches.
- Local Git checkpoint, visible meta files/text serialization, cache exclusions, team ownership guide and repository hygiene validation.

## Implemented, with remaining headset acceptance

- Left-stick continuous walking, right snap/optional smooth turning, floor tracking, gravity/collision, glove-shaped hands, stabilized torso, grabbing/haptic hooks.
- Motion vignette and head-boundary fading need both-eye review. Smooth turning and seated reach need human testing.
- Full staircase/balcony traversal, environmental audio mix, headset navigation brightness and sustained thermal performance are not certified by desktop tests.
- A short initial Quest view reported compositor 72 Hz, but this is not proof of stable full-route 72 FPS. Use application frame times and a ten-minute device route.

## Placeholders

- Original procedural audio is explicitly temporary. Replace with licensed rain, ocean, structural, machinery and thunder recordings and mix on-device.
- Original stylized geometry, wear textures, props and glove shapes are foundation art. Art approval against the actual reference image is pending because no image file was supplied.
- Water is a rising visual surface and threshold framework, not a sealed-room fluid simulation or drowning system. Ocean and beacon are inexpensive visual approximations.
- Stations contain roles, anchors and events, not complete cooperative puzzles.

## Intentionally deferred

Networking, voice, accounts/lobbies, enemies/combat, inventory, saves, full calendar/survival balancing and final puzzle validation. No networking or voice framework was installed. Day puzzle-time-pressure is extension data only.

## Blocked or unverified

Subjective headset acceptance cannot be inferred from logcat or a rendered desktop image. ADB screenshots on this Quest returned black despite the user seeing the app, so those screenshots are not visual evidence. See the device record for the user's latest result. No final claim of reference-image fidelity or thermal performance is made.

## Five next team tasks

1. Complete the Quest route, controls, grabbing and both-eye comfort acceptance checklist; fix issues before adding gameplay.
2. Refine architecture, lens, props, materials and glove art against the actual concept while profiling Quest GPU cost.
3. Replace procedural audio and tune interior/exterior acoustics on the headset.
4. Have each scene owner prototype one local puzzle role/event without networking or fixed player-count assumptions.
5. Record a ten-minute performance/thermal baseline, review scene/prefab ownership, and merge small tested PRs.

## Reproduction and review

Open Scenes/Bootstrap/LK_Bootstrap under Assets/_LighthouseKeepers. Press Play; WASD/Q/E provides a collision-aware desktop preview. Use Scenes/Development/LK_DevGym for isolated tests. README and XRQuestSetup contain exact Android build/USB steps. Verification contains build, test, scene validation and device records.

The read-only Spear Throw VR project was not changed. Deliberate differences are the game identity/content root, additive architecture, slower 1.6m/s walking, comfort/body presence, 20m shadows, two unshadowed per-pixel practical lights per object, baked navigation fills, flat cold ambient lighting with environment reflections disabled, and no AI Assistant/inference dependency. See ReferenceProjectAudit.

Historical implementation baseline: 8bc7ccc. The foundation was subsequently published to the team feature branch; main remains unchanged. Unrelated duplicate ProjectSettings files were preserved rather than deleted; see the final Git record.

The implementation checkpoint is titled `Build Lighthouse Sprint 1 foundation and repair Quest lighting and controls`. Eight unrelated duplicate ProjectSettings files (PackageManagerSettings 2, ProjectAuditorSettings 2/3/4, ShaderGraphSettings 2/3/4, URPProjectSettings 2) remain untracked and untouched. The working tree is therefore not globally clean. No generated Unity cache/build folders are tracked. Full package additions/removals, including transitive packages, are listed in Verification/PackageChanges.md.

Afterward, 44 malformed untracked lighting copies appeared. They were preserved outside Assets in ignored Builds/RecoveredDuplicateLighting-20260922; canonical assets were unchanged and repository validation passed again. See Verification/DuplicateLightingRecovery.md.

## Asset integration — September 23

Implemented in Unity 6000.3.23f1: selected weathered industrial props, entrance/radio desks, utility lighting meshes, optimized URP variants, restrained offshore fog, Quest ocean fallback, streamed looping licensed ambience and dedicated mixer routing. Circulation/rain repairs remain in place. Detailed source inventory: ThirdPartyAssets.md; exact scene/file changes and verification: AssetIntegrationStatus.md and Verification/AssetIntegrationValidation.md.

**Verified in the validation checkout:** seven-scene Bootstrap load, one XR Origin/listener, sixteen puzzle sockets/six station identities, four flood thresholds, five day profiles, unchanged puzzle references, locomotion/snap input, flood reset/day changes, soundtrack startup/uniqueness, bounded exterior fog, shader asset checks, door/landing clearances and rain shelter behavior. The final test/build record distinguishes completed runs from pending work.

**Requires headset verification:** new content's loudness/loop quality, both-eye water/fog/rain, hand reach, final navigation feel and sustained 72 FPS. The user reported the earlier repaired build looked good; that is historical acceptance, not a test of this content revision.

**Placeholders:** generator assembly is environmental motor machinery, not a complete diesel simulation; procedural environmental audio remains; some furniture and all cooperative puzzles remain foundation content. Vendor water is comparison-only; a project-owned Quest fallback is used because the supplied shader requires screen textures and lacks the required stereo setup.

**Deferred:** networking/voice, complete puzzles and survival progression. No packages for those systems were installed.

**Repository:** no push or commit for this integration. Preserve the existing PackageManagerSettings change and duplicate ProjectSettings files. Review the vendor license/dependency manifest before any later team commit.

Final September23 result: main-project Edit Mode **9/9 PASS**; corrected Play Mode checks **PASS**, including moving flood/lens visuals; Android development APK **Succeeded**, zero errors/one BuildReport warning, 81.76MiB. The warning concerns crash-report debug symbols; unused URP terrain/spatial-mapping shader stripping messages are documented. APK is in `Builds/LighthouseKeepers-Development.apk`. No on-device test of this revision and no push/commit.

Use **Lighthouse Keepers → Integration → Open complete environment** to inspect all content scenes; Play starts at Bootstrap. The main project's original package manifest/lock and tracked asset GUIDs remain unchanged.
