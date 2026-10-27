# Sprint 2 — source and delivery status

**Delivery is incomplete.** Review feedback reports keyboard-only lobby entry, a flat 2D APK launch, no Sprint 2 video, and three ambiguous Sprint 2 branches. [Sprint 3 remediation](../Sprint3/README.md) tracks those failures; they are not marked fixed by this documentation.

## Which branch and artifact?

| Revision | Purpose | APK / video |
| --- | --- | --- |
| [sprint2/final-submission at c299d6c](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/tree/c299d6cca227421bf066dcdd37b84e7a587d4632) | Named final-submission source; includes the isolated environment repair/sleep demo. Use this exact commit to inspect that revision. | No matching published APK or Sprint 2 video found in the checked releases/repository. Publication tracked in #61/#62. |
| [Main-snapshot source 8c15fc6](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/tree/8c15fc6f92341b2095ad4d61d64828b4c99073e5) | Published connected crew-lobby/Storm Watch baseline. | **[Download APK](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/main-quest-8c15fc6/LighthouseKeepers-Main-8c15fc6.apk)** · [SHA-256](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/main-quest-8c15fc6/LighthouseKeepers-Main-8c15fc6.apk.sha256) · [Build record](../../Verification/MainAPK-20261015.md). This is not a build of c299d6c. |
| sprint-2 | Historical main-baseline sprint branch. | Do not choose by branch-name similarity; use the source/hash above. |
| william/sprint-2 | Separate networking work proposed in PR #57. | Open, unmerged proposal; not the canonical reviewed main implementation. |

Comparison on October 27 found the named final-submission branch **six commits ahead** of APK source 8c15fc6, with additional shell/demo assets. The team must freeze and publish the matching final deliverables through [#61](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/61). No branch history is deleted or rewritten by this clarification.

**Sprint 2 video: missing.** [Recording/publishing task #62](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/62). The Sprint 1 demo is not a substitute.

## Start the published main-snapshot experience

Unity **6000.3.23f1**. For the source matching the linked APK, use commit **8c15fc6** and **Lighthouse Keepers → Play game**, or open `Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity`.

This is **two-player Storm Watch**. Follow the [host/guest sequence and full button guide](../LobbyGuide.md): same room → two names visible → both Signal ready → host Begin watch. A single keeper cannot start production gameplay. Keyboard-free Quest entry is an open blocker, not a passed test.

## Inspect the named final-submission demo source

At **c299d6c**, open `Assets/Scenes/Lab/LK_Lobby_Environment.unity`. The branch's [local watch demo guide](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/blob/c299d6cca227421bf066dcdd37b84e7a587d4632/Assets/WatchDemo/README.md) describes its existing host/ready/start entry, repair task, third-floor sleep and repeated days. That isolated prototype is separate from production Storm Watch; its documentation does not establish a controller-accessible solo launch.

The guide's APK target `artifacts/builds/LighthouseKeepers-Environment-BatchA.apk` is a local output path, not a downloadable release. Do not substitute the main-snapshot APK for it.

## What was implemented, and what was proven?

Merged main work includes Fusion crew/gameplay transport, the three-watch cooperative prototype, footsteps/grounding and nighttime presentation; earlier ambience/weather routing is also present. The final-submission branch contains additional isolated shell/watch-demo work. Credit implementation separately from acceptance.

[Connected lobby tests](../../Verification/ConnectedLobby/README.md), [Storm Watch tests](../../Verification/StormWatch/README.md) and [movement/footsteps evidence](../../Verification/MovementFootsteps/README.md) record Editor/transport/build checks. They do not establish a successful two-device headset playthrough. The reviewer-reported failures remain open in #59/#60.

[Back to sprint index](../../../README.md)
