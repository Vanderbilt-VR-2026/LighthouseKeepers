# Sprint 2 integration

## Baseline and ownership

[VERIFIED] Integration starts from team/main 41c9819, including Eli's c2b9760 through PR #20. The clean checkout is LighthouseKeepers-Sprint2, branch integrate/sprint-2. The separate local Phase 1 checkout has not been merged or copied into this branch.

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

[UNRESOLVED] Android build and device acceptance are pending. Quest is disconnected by the project lead's confirmation. Automated results cannot establish headset comfort, navigation, grabbing, sound balance or performance.

[VERIFIED] Artifacts (ignored by Git): artifacts/tests/editmode-baseline.xml, editmode-integration.xml, playmode-integration.xml; corresponding logs under artifacts/logs/. Recovery rename completed via Sprint2Integration.PreserveRecovery; GUID 8790e0acdc4c38148b27e8188cfc3b3a is unchanged.

[PROPOSED] Device checklist once connected: launch the integration APK; look for missing/pink surfaces; walk from the entrance to each floor and balcony; identify each blocked stair approach and unreadable area; check both controllers track; listen for duplicated audio; record observed defects without treating this baseline as the upcoming hands/cleanup implementation. Capture to artifacts/captures/<date>-integration.mp4. Obtain project-lead acceptance before Priority 1.

[PROPOSED] Minimum circulation fixes may precede the integration device gate, as approved by the project lead. Gameplay priority 1 remains gated on priority 0 device acceptance.
