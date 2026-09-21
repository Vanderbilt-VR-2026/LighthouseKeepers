# Sprint 1 implementation plan

## Audit (completed before modifications)

- Project root confirmed: Lighthouse Keepers; Unity 6000.5.10f1.
- Unity editor was closed. Existing Logs/Editor.log contains zero C# compiler-error entries; a fresh headless compile remains required.
- URP 17.5.0, Input System 1.20.0, Test Framework 1.7.0 and UGUI 2.5.0 are installed. UGUI includes TextMeshPro.
- Reference located within the parent directory at ../First VR Build/Spear Throw VR. Inspected files only; never opened or modified that project.
- Existing Git changes are recorded in InitialGitStatus.txt and InitialUserChanges.patch. Preserve the deleted HubForceResolve files, user's cloud identity, and modified template settings/assets.
- The initial manifest contains com.unity.ai.assistant despite the brief prohibiting it. Remove that direct dependency; let Unity resolve the lock file.
- No concept-image attachment or matching file is available. Use the supplied written direction provisionally; do not claim image matching.
- The latest brief ends midway through player presence; remaining requirements and final verification specification have been requested.

## Implementation sequence

1. Mirror compatible reference package versions; configure Android/OpenXR using Editor APIs and new game-owned settings assets. Preserve template scenes and user assets.
2. Establish a small runtime assembly, editor tooling, test assembly, clean folders and team documentation.
3. Generate reusable architectural/prop prefabs and material assets; construct Bootstrap plus eight additive content scenes with explicit scene ownership.
4. Build human-scale keeper house and four-level route, protected stairs/balcony, exterior silhouette and storm views.
5. Configure one XR player, controller hands, torso, locomotion/turning comfort, and basic interaction examples.
6. Add data-driven environmental audio, storm, beacon, flood test controls, day-difficulty data and editor-only puzzle sockets. No completed cooperative puzzles or networking.
7. Validate scene topology, unique managers/listeners, collisions, scale, serialized references, additive loading and system tests. Render review images and inspect them.
8. Build Android APK headlessly. Record actual verification outputs and measured performance; distinguish desktop evidence from Quest evidence. Complete any additional checks in the remaining brief.

The environment is Sprint 1 only. No enemies, combat, inventory system, voice chat, online services or speculative networking abstractions.
