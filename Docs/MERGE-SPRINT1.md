# Sprint 1 integration and repair

[VERIFIED] This is the historical initial merge record. Current contributor integration, test counts and APK status are in [Sprint1Publication.md](Sprint1Publication.md). Reproduction commands use the renamed Sprint 1 tooling; original logs retain their historical identifiers.

## Baseline and ownership

[VERIFIED] Integration starts from team/main 41c9819, including Eli's c2b9760 through PR #20. The clean checkout is LighthouseKeepers-Sprint1, branch integrate/sprint1-team-work. The separate local Phase 1 checkout has not been merged or copied into this branch.

[VERIFIED] William's 2887c83 was merged with a merge commit, preserving his authorship and VS Code configuration.

[VERIFIED] Retained William's recovery scene as Assets/_Recovery/LK_Recovery.unity, preserving its GUID. Keep the established LK_Bootstrap first in the build: it already owns XR and additive loading. The project lead approved this reconciliation and requested its explanation on GitHub. Recovery is not loaded alongside the playable environment, avoiding duplicate cameras, listeners and global volumes.

## Flood and lobby reconciliation

[PROPOSED] Evolve Eli's flood implementation from the current main baseline. Do not import the local Phase 1 scenes, controls, prototypes or assembly tree. Preserve Eli's danger stages, countdown, depth darkening and development diagnostics while evolving the simulation toward volume-based inflow/outflow. His FloodController is the starting authority; no second controller will be installed beside it.

[VERIFIED] Preserved the six lobby source files and eight tests, isolated under LK.Lobby and left unwired. No networking transport is selected or added. The lobby's screen-space UI is a prototype, not accepted Quest UI.

[PROPOSED] Port the intent of Eli's four flood tests when volume authority is implemented. Retain their names and credit his source contribution. No tests are silently retired.

## Validation and device gate

[VERIFIED] Initial baseline Edit Mode: 21 tests, 14 passed / 7 failed. Failures were runtime lifecycle assumptions in Edit Mode lobby and danger-monitor tests. An attempted SendMessage setup caused Unity ShouldRunBehaviour assertions; it was replaced with explicit reflection invocation of the existing initialization callbacks. No assertion was removed or weakened.

[VERIFIED] Corrected Edit Mode: 21/21 passed, including all eight lobby tests and four flood tests. Play Mode: 1/1 passed, loading all six content scenes and asserting one XR Origin, one listener, one flood authority, 16 sockets and four thresholds. Recovery remains excluded from the playable scene set.

[VERIFIED] Flood-authority search: `rg -n 'SetHeight\(|height\s*\+=' Assets/_LighthouseKeepers/Scripts/Runtime` finds height storage/advancement only in Flood/FloodController.cs, plus DevelopmentDiagnostics calling its public SetHeight API. FloodSurface moves presentation geometry, not authoritative state. Volume conversion is still PROPOSED, not implemented.

[VERIFIED] Explicit-Android diagnostic build succeeded with 0 errors and 1 warning (diagnostics enabled without full crash-report debug symbols). APK: Builds/LighthouseKeepers-Sprint1-Environment.apk, 85,546,562 bytes; SHA256 1fb994d9ed5ae72dd3365a1e678d6a16c9a3a3f9f313f65661f0eae32b555910.

[UNRESOLVED] Device acceptance is pending. Quest is disconnected by the project lead's confirmation. Automated results cannot establish headset comfort, navigation, grabbing, sound balance or performance.

[VERIFIED] Artifacts (ignored by Git): artifacts/tests/editmode-baseline.xml, editmode-integration.xml, playmode-integration.xml; corresponding logs under artifacts/logs/. Recovery rename completed via Sprint1Integration.PreserveRecovery; GUID 8790e0acdc4c38148b27e8188cfc3b3a is unchanged.

[PROPOSED] Device checklist once connected: launch the integration APK; look for missing/pink surfaces; walk from the entrance to each floor and balcony; identify each blocked stair approach and unreadable area; check both controllers track; listen for duplicated audio; record observed defects without treating this baseline as the upcoming hands/cleanup implementation. Capture to artifacts/captures/<date>-integration.mp4. Obtain project-lead acceptance before Priority 1.

[PROPOSED] Minimum circulation fixes may precede the integration device gate, as approved by the project lead. Gameplay priority 1 remains gated on priority 0 device acceptance.

[VERIFIED] Project lead clarified this work is still Sprint 1. Current branch, checkout, build tooling and APK names now consistently identify Sprint 1.

## Reproduction commands

[VERIFIED] Commands run from the integration checkout, using Unity 6000.3.23f1 at the path below. XML and logs stay in ignored artifacts; they are not committed.

```bash
UNITY="/Users/aneeshvasamreddy/UnityEditors/6000.3.23f1/Unity.app/Contents/MacOS/Unity"
"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults artifacts/tests/editmode-integration.xml -logFile artifacts/logs/editmode-integration.log
"$UNITY" -batchmode -nographics -quit -projectPath . -executeMethod LighthouseKeepers.Editor.Sprint1Integration.PreserveRecovery -logFile artifacts/logs/recovery.log
"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform PlayMode -testResults artifacts/tests/playmode-integration.xml -logFile artifacts/logs/playmode-integration.log
"$UNITY" -batchmode -quit -buildTarget Android -projectPath . -executeMethod LighthouseKeepers.Editor.Sprint1Integration.BuildDiagnosticAndroid -logFile artifacts/logs/android-integration-android-target.log
```

[VERIFIED] The first diagnostic build omitted `-buildTarget Android`. OpenXR MetaQuestFeature validation accessed the selected editor target and logged a NullReferenceException. Unity reported Succeeded with Errors=1; that is not accepted as a clean result. The explicit-Android rerun is required. No package upgrade or vendor-code edit was used.

[UNRESOLVED] This is an integration diagnostic APK, not a shipping-quality acceptance build. Primitive cleanup, navigation and luminance validators, volume conversion, rigged hands, and later mechanic gates have not been implemented in this integration commit. No sustained Quest performance or visual/interaction claims are made.

[VERIFIED] Build-generated URP prefilter cache changes and a Standalone batching entry were reviewed, backed up under artifacts/verification/build-generated, and removed from the source diff. Authored project settings remain at the team main baseline. No teammate edits were discarded.
