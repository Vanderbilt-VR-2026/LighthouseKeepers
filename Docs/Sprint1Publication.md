# Sprint 1 — team environment publication (September 30)

[VERIFIED] Deadline: October 1. Current branch: `integrate/sprint1-team-work`. Team repository: https://github.com/Vanderbilt-VR-2026/LighthouseKeepers. Review: PR #23. Local checkout: `/Users/aneeshvasamreddy/LighthouseKeepers-Sprint1`. Required Unity: **6000.3.23f1**; Android ARM64/IL2CPP, URP and XR package versions unchanged.

## What is included

[VERIFIED] The project lead approved the latest editor environment appearance. Original and new teammate commits are preserved with merge commits, without squash or history rewriting.

[BUILT-UNVERIFIED] Environment revisions include cleared second-floor dressing, improved access to retained third-floor furniture, solid floor slabs and stair arrivals, seated railings, grounded compact player-facing pipework, solid valve rings, and mounted ceiling lights. The stair texture is preserved. Further second- and third-floor furnishing is deferred at the lead's request. This is an environment foundation; it does not claim completed pipe/generator gameplay, animated grabbing or multiplayer.

| Contributor | Included commit | Reconciliation |
|---|---|---|
| Tapan Sidhwani | `8555bad` | Architecture document, procedural wave-crash source, read-only flood-danger audio and opt-in wiring tool retained. Unity-generated metadata added for the two new scripts. No global asset regeneration or automatic audio wiring. |
| Eli Gripenstraw | `c2b9760`, then `6f20882`, `b8b466a`, `4292781` | Existing flood authority retained, unreachable-target ETA corrected, spatial local lobby preview and all tests retained. Two test initialization conflicts resolved to Eli's shared `TestLifecycle` helper. LK.Lobby gains the screen's required TMP/XRI references. |
| Will Qian / `canwer1` | `2887c83` | VS Code files and recovery scene preserved from prior merge. `LK_Recovery` retains its original GUID and stays separate to avoid a duplicate camera. |
| Aneesh | `d2d7277` and publication follow-up | Scene repairs, baked lighting, geometry tests and Sprint 1 naming. Affected scene ownership areas are explicitly listed in the environment commit body. |

[VERIFIED] Local experimental `feat/phase-1-water-loop` is deliberately not merged. There is one existing height-based flood authority, not competing height/volume controllers. Tapan's new audio helper remains opt-in; Eli's lobby remains a separate local preview. These boundaries preserve the environment the lead just approved.

## Opening the work

[VERIFIED] Check out `integrate/sprint1-team-work`, open the project with Unity 6000.3.23f1, and choose **Lighthouse Keepers → Play environment**. Or open `Assets/_LighthouseKeepers/Scenes/Bootstrap/LK_Bootstrap.unity` and press Play. Click Game; WASD walks, Q/E turns. Six content scenes load additively with one rig and listener.

[VERIFIED] Android build helper: `LighthouseKeepers.Editor.Sprint1Integration.BuildDiagnosticAndroid`. Output: `Builds/LighthouseKeepers-Sprint1-Environment.apk`. Builds and local artifacts are ignored by Git; classmates build locally with Android SDK/NDK/OpenJDK modules. Do not run full scene generators over the authored environment.

## Validation and limits

[UNRESOLVED] Combined-branch test/build results will be appended after execution. The prior environment-only revision passed 27 Edit Mode and 1 Play Mode tests and built for Android with zero errors. Latest Quest inspection, stereo comfort, audio balance and sustained 72 FPS remain pending. The user's positive editor review is not device acceptance.

## Branch policy

[VERIFIED] `aneesh/lighthouse-sprint1-foundation` is an ancestor of team `main`; its remote branch is redundant. Keep current integration and all teammate branches/PRs. Preserve unmerged Phase 1 experiments and backup worktrees. Publication does not merge PR #23 into main; the team retains review control.

[VERIFIED] Combined branch: **31/31 Edit Mode**, **1/1 Play Mode**, and full asset/configuration validation passed. Asset checks cover missing scripts/materials/prefabs, six station identities, 16 sockets, four thresholds, five profiles, one rig/listener and 285 stair support/head-clearance samples. Curated reports: `Docs/Verification/Sprint1Publication/`. No Unity/URP/XR/package or Android architecture changes.
