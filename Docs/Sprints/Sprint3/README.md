# Sprint 3 — Thursday evening, October 29, 2026

Deadline: Thursday evening in America/Chicago; verify the course's exact cutoff. [Submission issue/agent prompt #61](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/61) · [Milestone](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/milestone/1).

## Reconciled candidate

[Single PR #65](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/65) incorporates `sprint3/one-lighthouse` through `7182583294c1fcf0962e755099887890f3bb5d94` (runtime base `d2e3066`), plus `sprint-3` documentation through `e90e303` and replaces the earlier PR candidate's old-world controls/numeric keypad with the teammate's Lab world and alphanumeric crew UI. The same existing PR branch is used; no second integration PR is needed.

The playable candidate is **solo One Lighthouse**, with three content days plus a Day 4 placeholder. **Co-op gameplay remains disabled pending host-authoritative Lab state integration (M5).** Host-alone entry is diagnostics, not multiplayer evidence. The older three-watch Storm Watch is not in this candidate's two-scene APK.

| Required item | Status |
| --- | --- |
| Canonical final submission ref | Not frozen; PR #65 remains under review. Preserve teammate branch history. |
| Fresh reconciled APK / checksum / build record | [Download APK](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-one-lighthouse-candidate-20261027/LighthouseKeepers-Sprint3-89199c5a-20261027T222901Z.apk) · [checksum/build records](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint3-one-lighthouse-candidate-20261027). Succeeded, zero errors, seven warnings; source `89199c5a10e5a077758e6c0b2710595afab60826`. The earlier e88f4e1 APK is historical. |
| Immersive launch, controller entry/menu/stations | Target-headset acceptance pending (#59/#60/#31). |
| Sprint 3 video | Missing (#62); record this exact downloaded artifact. |
| Co-op gameplay | Blocked by unfinished shared Lab state; do not claim two-client acceptance. |
| Solo reviewer route | PLAY SOLO from the lobby; return/retry/replay through Watch Menu. |

## Exact candidate artifact

[Download LighthouseKeepers-Sprint3-89199c5a-20261027T222901Z.apk](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-one-lighthouse-candidate-20261027/LighthouseKeepers-Sprint3-89199c5a-20261027T222901Z.apk) · [SHA256SUMS.txt](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-one-lighthouse-candidate-20261027/SHA256SUMS.txt) · [Unity build record](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-one-lighthouse-candidate-20261027/LighthouseKeepers-Sprint3-89199c5a-20261027T222901Z.build.json) · [binary verification](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-one-lighthouse-candidate-20261027/LighthouseKeepers-Sprint3-89199c5a-20261027T222901Z.verification.json).

Binary source: `89199c5a10e5a077758e6c0b2710595afab60826`. APK size: **83,275,214 bytes**. SHA-256:

```text
b6701f239acfe88c7e5be7e6d4c98ecd211f33cafa99824a025d9123ef5eba4c
```

Unity6000.3.23f1, Development + CleanBuildCache, ARM64/IL2CPP/OpenXR, version0.1.0(1). Ordinary BuildReport: **Succeeded, 0 errors, 7 warnings**. Source status was empty; both recorded build options and the binary's scene table contain only Network Lobby and Lab. GitHub's uploaded APK digest matches the locally verified binary. Subsequent changes to artifact links/verification docs do not change this binary's source SHA. [Detailed build evidence](../../Verification/Sprint3/Build.md).

## Start and build

Unity **6000.3.23f1**. **Lighthouse Keepers → Play game** opens Network Lobby; choose **PLAY SOLO** to enter `Assets/Scenes/Lab/LK_Lab_Environment.unity`. Follow the House board, F1 pipe, F2 generator, F4 radio and F3 bed. See the [root launch/controls guide](../../../README.md) and [complete lobby guide](../LobbyGuide.md).

All supported Quest build entry points use the [same guarded pipeline](../../Verification/StormWatch/QuestBuildPipeline.md). Actual build scenes are **Network Lobby, then Lab**, including the networking-menu builds that formerly removed `/Lab/`. Source, scene list, options and ordinary zero-error BuildReport accompany the new APK; final archive/manifest/native-library/signature/SHA checks must pass.

[Current verification and known limits](../../Verification/Sprint3/README.md). Editor checks do not establish physical controller usability, immersive startup, audio balance or Quest performance.

## Submission checklist

- [ ] Team reviews the current Lab-based scope and PR #65; keep pending co-op accurately labeled.
- [x] Publish/link the new exact APK, checksum, source SHA and build/binary records here.
- [ ] Test the downloaded artifact on Quest: immersive cold launch/restart, controller-only entry/menu, repair/generator/radio, failed retry, final replay and return.
- [ ] Record headset/OS, actual result, APK hash and audio/performance observations.
- [ ] Attach accessible Sprint 3 video and label any missing Sprint 2 video separately.
- [ ] Have a fresh reviewer follow only the README and downloaded candidate.
- [ ] Freeze one canonical submission branch/tag after integration and required acceptance; keep feature refs distinct.
- [ ] Update issues/Project according to implementation, review and validation evidence.

The copy-ready prompt in #61 is the handoff entry. It must use this current Lab/two-scene/solo-gated scope rather than the superseded Storm Watch build instructions.
