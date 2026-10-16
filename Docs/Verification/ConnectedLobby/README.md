# Connected crew lobby verification — October 15, 2026

Unity **6000.3.23f1**, macOS. The normal entry is now `LK_NetworkLobby` → `LK_StormWatch` → six content scenes, with one persistent Fusion crew runner per client.

## Executed checks

- **Edit Mode: 75/75 passed.** Includes production two-keeper configuration, lobby/Storm Watch build order, lobby request/snapshot ordering, shared watch rules and prior regressions. See `EditMode.xml`.
- **Full Play Mode regression: 20/20 passed, no skips.** Includes the two online Photon tests, both environment/lobby scene transitions, earlier First Watch interactions, traversal, footsteps, physical pumping and the cooperative watch scene. See `PlayMode.xml`.
- **Final focused Play Mode after the scene-arrival and XR-manager refinements: 8/8 passed, no skips.** Includes two real Photon peers sharing the roster, all-ready launch, retained commands, authoritative water/progression snapshots, stable runner identities, and closing the host session if its partner leaves after launch approval. The Single-mode lifecycle test verifies gameplay binding when scene arrival precedes the separate reliable lobby start snapshot; one rig/runner and one scene-owned XR interaction manager shared by all active interactors/interactables; no second Host/Join UI; return to a fresh lobby; a new runner on rejoin; and the partner-loss explanation. A director fixture verifies automatic Day 1 start only after the partner environment sends input, pause during missing input and subsequent recovery. See `FocusedPlayMode.xml`.
- **Rendered UI inspection:** `Entry.png` and `Generator.png` show the updated instructions and return-to-lobby controls. TMP overflow assertions passed. These are Editor verification-camera captures from an accelerated offline repair fixture, without a connected crew; they demonstrate board layout, not a full multiplayer run. The UI was unchanged by the final transport refinements.
- Merge conflicts resolved, asset/meta pairs preserved, and diff whitespace checks passed with Unity's inherited trailing spaces ignored. Existing simulator edits were excluded from the integration commit.

The full Play Mode pass preceded the final scene-arrival and XR-manager refinements; the final focused Play Mode report verifies those changes. The Edit Mode report covers the compiled runtime and rules. XML reports retain counts, assertions and failure diagnostics while omitting runtime log output.

## Test boundaries

The cloud tests use two real Photon runners and a traffic-only scene manager in one Editor. They verify lobby approval and gameplay traffic on the same connections. The Single-mode test uses the real production scene manager but loads the gameplay scene directly because production correctly rejects a solo launch. These complementary tests do **not** establish two independent clients completing the scene transition together.

No new APK was built for this integration. The October 13 Storm Watch APK predates this connected flow. Quest/Quest or Quest/Editor acceptance remains: shared scene launch, physical grip/reach, first-watch timing, synchronized flood and gates, radio/beacon, rescue, terminal retry, leave/rejoin, both-eye readability and sustained device performance.

## Reproduce

From the repository root, use the installed Unity 6000.3.23f1 executable:

```sh
Unity -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform EditMode -testResults /tmp/connected-edit.xml
LK_FUSION_CLOUD_TESTS=1 Unity -batchmode -projectPath "$PWD" -runTests -testPlatform PlayMode -testResults /tmp/connected-play.xml
LK_FUSION_CLOUD_TESTS=1 Unity -batchmode -projectPath "$PWD" -runTests -testPlatform PlayMode -testFilter 'FusionLobbyIntegrationTests|StormWatchSceneTests' -testResults /tmp/connected-focused.xml
```

The cloud flag opts into temporary rooms using the already configured Photon app. Leave it unset for offline tests. Build the current Quest player with **Lighthouse Keepers → Storm Watch → Build Quest APK**; both that helper and the standard development build begin at the crew lobby.
