# Lighthouse Keepers

A cooperative VR lighthouse survival prototype built with **Unity 6000.3.23f1** for Meta Quest. Production **Storm Watch requires exactly two keepers**. The earlier First Watch is a separate local, one-floor rehearsal.

## Choose a sprint

| Sprint | Start here | APK and video status |
| --- | --- | --- |
| Sprint 1 | [Sprint 1 README](Docs/Sprints/Sprint1/README.md) | Environment APK and 48-second demo linked there. |
| Sprint 2 | [Sprint 2 README](Docs/Sprints/Sprint2/README.md) | Main-snapshot APK available; it does **not** match the later final-submission branch. Sprint 2 video missing. |
| Sprint 3 | [Sprint 3 README and release checklist](Docs/Sprints/Sprint3/README.md) | Due **Thursday evening, October 29, 2026 (America/Chicago)**. [Fresh technical candidate](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint3-tech-candidate-20261027) available; final submission ref, headset acceptance and video pending. |

APKs are downloaded from GitHub Releases; each sprint README links its available artifacts and source. Their absence from a branch's file tree is intentional. Missing or mismatched deliverables are explicitly identified.

## Current review blockers

The reviewer reported that room-code text entry required a keyboard and that the supplied APK opened as a flat 2D app. These are open release blockers: [controller-only entry #59](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/59) and [immersive APK launch #60](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/60). The candidate source adds a controller-operated room keypad and Quest packaging gates; the older published APK predates them. Editor tests and Android packaging do not establish headset acceptance.

[Track Sprint 3](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/milestone/1) · [GitHub Project](https://github.com/orgs/Vanderbilt-VR-2026/projects/15) · [Exact issue acceptance criteria](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues?q=is%3Aissue+label%3Asprint%3A3)

## Start the current main experience

1. Open the repository root in **Unity 6000.3.23f1** and let packages/compilation finish.
2. Choose **Lighthouse Keepers → Play game**. Entry: `Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity`.
3. Use **two compatible clients** with internet access. The host opens **Crew options**, chooses a room name, then selects **Host crew**. For controller-only entry in this source, both clients use the digits, **BACK** and **CLEAR** buttons under **Crew options** to choose the same numeric room code; the guest selects **Join crew**. Keeper names can stay at their defaults.
4. Confirm that both keeper names appear. Each keeper selects **Signal ready**. The **host** selects **Begin watch** when both are ready.
5. Both clients enter Storm Watch on the existing connection. Day 1 starts after both environments send gameplay input. Use **Return to lobby** on a floor board to leave.

Read the [complete lobby button guide and troubleshooting](Docs/Sprints/LobbyGuide.md) before testing. The older published build has the #59 keyboard-entry blocker. The keypad in this source still requires target-headset acceptance.

**Reviewing alone:** in Unity, choose **Lighthouse Keepers → First Watch → Play plumbing escape** for the local rehearsal, or **Development → Play environment** for an environment preview. These are different experiences; the current production APK does not advertise a selectable solo mode. The [one-person review task #63](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/63) tracks that gap. A simulated guest does not validate multiplayer.

## Build and install

Install Android Build Support, SDK/NDK Tools and OpenJDK for the matching Unity version. Use **Lighthouse Keepers → Storm Watch → Build Quest APK** for the main crew-lobby/game build. Output is a fresh source-stamped APK under `Builds/Sprint3-<commit8>-<UTC>/`. See the [repeatable build and final APK checks](Docs/Verification/StormWatch/QuestBuildPipeline.md). The historical Sprint 1 environment builder makes a different experience.

Download the [fresh Sprint 3 candidate APK](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-tech-candidate-20261027/LighthouseKeepers-Sprint3-e88f4e1c-20261027T202736Z.apk) and [SHA256SUMS](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-tech-candidate-20261027/SHA256SUMS). It was built from source `e88f4e1c99cee75588758196fa149a199bcd1559`; [build and binary-verification records](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint3-tech-candidate-20261027) accompany it. This is a Development build for review, with headset acceptance still pending.

Connect Quest with USB debugging authorized, then:

```sh
adb install -r LighthouseKeepers-Sprint3-e88f4e1c-20261027T202736Z.apk
adb shell am start -n com.lighthousekeepers.game/com.unity3d.player.UnityPlayerGameActivity
```

Check immersive stereo, tracked head/controllers and keyboard-free entry on the **downloaded artifact**, including cold launch and restart. A flat window is a failed launch check; capture the APK hash and diagnostics for #60. Packaging checks passed; no Quest was connected for device acceptance. The [Sprint 2 README](Docs/Sprints/Sprint2/README.md) preserves the older main-snapshot APK and its source mismatch.

## Development and evidence

Quest: left stick walks, right stick snap-turns, grip selects nearby tools/controls. Editor desktop preview: WASD walks and Q/E turns; keyboard rehearsal controls are documented in [Storm Watch](Docs/StormWatch.md). Desktop preview does not prove controller usability.

[Storm Watch design](Docs/StormWatch.md) · [Connected lobby verification](Docs/Verification/ConnectedLobby/README.md) · [Storm Watch verification](Docs/Verification/StormWatch/README.md) · [Networking](Docs/FusionNetworking.md) · [Team workflow](Docs/TeamWorkflow.md) · [Asset provenance](Docs/ThirdPartyAssets.md)

Implementation, compilation, Editor execution, real transport testing and headset acceptance are separate. Keep scene/asset `.meta` files, use feature PRs, and exclude Unity caches and build outputs from Git. Earlier setup prose is preserved in [the October 16 README](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/blob/833aa0f94065ee7740001300f2663d1e1879d962/README.md); use this index for current delivery status.
