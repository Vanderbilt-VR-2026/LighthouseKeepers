# Sprint 3 — Thursday evening, October 29, 2026

Deadline: Thursday evening in America/Chicago; verify the course's exact cutoff. [Submission issue/agent prompt #61](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/61) · [Milestone](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/milestone/1).

## Reconciled candidate

[Single PR #65](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/65) incorporates `sprint3/one-lighthouse` at `d2e30665a44b29a4e3bf84a7bfb7b049f93317b4` and replaces the earlier PR candidate's old-world controls/numeric keypad with the teammate's Lab world and alphanumeric crew UI. The same existing PR branch is used; no second integration PR is needed.

The playable candidate is **solo One Lighthouse**, with three content days plus a Day 4 placeholder. **Co-op gameplay remains disabled pending host-authoritative Lab state integration (M5).** Host-alone entry is diagnostics, not multiplayer evidence. The older three-watch Storm Watch is not in this candidate's two-scene APK.

| Required item | Status |
| --- | --- |
| Canonical final submission ref | Not frozen; PR #65 remains under review. Preserve teammate branch history. |
| Fresh reconciled APK / checksum / build record | Pending the coordinated test and build run; link the new artifact below before submission. The earlier e88f4e1 APK is a different world/source. |
| Immersive launch, controller entry/menu/stations | Target-headset acceptance pending (#59/#60/#31). |
| Sprint 3 video | Missing (#62); record this exact downloaded artifact. |
| Co-op gameplay | Blocked by unfinished shared Lab state; do not claim two-client acceptance. |
| Solo reviewer route | PLAY SOLO from the lobby; return/retry/replay through Watch Menu. |

## Start and build

Unity **6000.3.23f1**. **Lighthouse Keepers → Play game** opens Network Lobby; choose **PLAY SOLO** to enter `Assets/Scenes/Lab/LK_Lab_Environment.unity`. Follow the House board, F1 pipe, F2 generator, F4 radio and F3 bed. See the [root launch/controls guide](../../../README.md) and [complete lobby guide](../LobbyGuide.md).

All supported Quest build entry points use the [same guarded pipeline](../../Verification/StormWatch/QuestBuildPipeline.md). Actual build scenes are **Network Lobby, then Lab**, including the networking-menu builds that formerly removed `/Lab/`. Source, scene list, options and ordinary zero-error BuildReport accompany the new APK; final archive/manifest/native-library/signature/SHA checks must pass.

[Current verification and known limits](../../Verification/Sprint3/README.md). Editor checks do not establish physical controller usability, immersive startup, audio balance or Quest performance.

## Submission checklist

- [ ] Team reviews the current Lab-based scope and PR #65; keep pending co-op accurately labeled.
- [ ] Publish/link the new exact APK, checksum, source SHA and build/binary records here.
- [ ] Test the downloaded artifact on Quest: immersive cold launch/restart, controller-only entry/menu, repair/generator/radio, failed retry, final replay and return.
- [ ] Record headset/OS, actual result, APK hash and audio/performance observations.
- [ ] Attach accessible Sprint 3 video and label any missing Sprint 2 video separately.
- [ ] Have a fresh reviewer follow only the README and downloaded candidate.
- [ ] Freeze one canonical submission branch/tag after integration and required acceptance; keep feature refs distinct.
- [ ] Update issues/Project according to implementation, review and validation evidence.

The copy-ready prompt in #61 is the handoff entry. It must use this current Lab/two-scene/solo-gated scope rather than the superseded Storm Watch build instructions.
