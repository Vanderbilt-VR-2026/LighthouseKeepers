# E to F — Round 2 handoff

E worked in a separate `sprint3/networking-round2` worktree and preserved the
original E worktree's substantial uncommitted draft. The published networking ref
includes the integrated Round 1 baseline and B's published 8020a51 dependency.
No C/D draft files or test-only C corrections are included in E's commits.

Merge the published E networking tip. Before another APK:

1. Land C/D's Round2 source and C's four native authoring corrections described in
   E-to-C-round2.md; A must supply PF_PlayerBody and actual physical bindings.
2. Run `LK/Ship/Bind lobby and player routes`, C's Apply Build Scenes, authoring,
   bakes and validators. E references the original manifest TextAsset at runtime.
3. Both lobby configs must target Shell_House; build order is the exact manifest
   (14 scenes, environment lobby first, House second, NetworkLobby last).
4. Run E's capture menu and tests. Native Edit Mode passed 25/25. Native Play Mode
   passed 3/4, with the ship test correctly failing on the missing lighting bakes.
5. Inspect the actual House PNG and loaded-scene proof in artifacts/round2/E.
   The isolated capture uses C/D snapshots plus documented test-only C corrections;
   it is useful integration evidence, not permission to ship unfinished assets.

No Quest APK/install or live pair is claimed in this E turn. Round2EReport.md embeds
the named-anchor screenshot and contains exact reproduction commands and limits.
Do not substitute the earlier installed Storm Watch APK.
