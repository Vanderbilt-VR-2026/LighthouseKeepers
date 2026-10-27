# Sprint 3 — Thursday evening, October 29, 2026

Deadline is **Thursday evening in America/Chicago**; the exact course cutoff was not supplied. [Milestone](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/milestone/1) · [Project](https://github.com/orgs/Vanderbilt-VR-2026/projects/15) · [Sprint issues](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues?q=is%3Aissue+label%3Asprint%3A3)

## Submission status

| Required item | Current status |
| --- | --- |
| Authoritative submission branch/tag/commit | **Not frozen.** main is the reviewed baseline; sprint3/* branches are work in progress, not interchangeable submissions. Freeze one ref in #61 after integration and acceptance. |
| Exact Sprint 3 Quest APK + SHA-256 | [Fresh technical candidate APK](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-tech-candidate-20261027/LighthouseKeepers-Sprint3-e88f4e1c-20261027T202736Z.apk), [checksums](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-tech-candidate-20261027/SHA256SUMS); packaging passed, device acceptance pending. |
| Sprint 3 demonstration video | **Missing** — #62. |
| Immersive Quest cold launch | **Packaging correction implemented; device acceptance pending** — #60. |
| Controller-only lobby entry | **Candidate keypad implemented; device acceptance pending** — #59. |
| Two independent clients complete a run | **Acceptance pending** — #5/#31. |
| One-person review route | Unity rehearsal documented; any APK review option remains a deliverable in #63. |

The candidate is available for acceptance; it is not the frozen submission. No previous APK, Editor test pass or screenshot fills missing device acceptance results. Each item must link to the actual artifact/evidence before submission.

## Download and run the candidate

[APK](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-tech-candidate-20261027/LighthouseKeepers-Sprint3-e88f4e1c-20261027T202736Z.apk) · [SHA256SUMS](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-tech-candidate-20261027/SHA256SUMS) · [Unity build record](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-tech-candidate-20261027/LighthouseKeepers-Sprint3-e88f4e1c-20261027T202736Z.build.json) · [Binary verification](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint3-tech-candidate-20261027/LighthouseKeepers-Sprint3-e88f4e1c-20261027T202736Z.verification.json) · [Release](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint3-tech-candidate-20261027)

Exact build source: [`e88f4e1c99cee75588758196fa149a199bcd1559`](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/commit/e88f4e1c99cee75588758196fa149a199bcd1559), branch `codex/sprint3-technical-fixes`. Later README-only commits do not change this binary. Unity **6000.3.23f1**, Android ARM64/IL2CPP, Development build; **Succeeded, 0 errors, 4 warnings**. APK size: **90,254,809 bytes**.

SHA-256: `c74e0542be57e0a1a65f0571144ecdf7fd3bd2cd8b933f43580724edd5aadecf`.

Install the downloaded APK using the [root build/install commands](../../../README.md#build-and-install). Production requires **two compatible clients**: open **Crew options**, enter the same numeric code with the controller keypad, host on one client and join on the other, confirm both keepers, both **Signal ready**, then the host selects **Begin watch**. Entry is `LK_NetworkLobby.unity`; Editor menu **Lighthouse Keepers → Play game**. Read the [full button sequence and troubleshooting](../LobbyGuide.md). One-person Unity rehearsal is explained in the root README and is a different experience.

Actual binary manifest, native OpenXR/IL2CPP libraries, Android signature, ZIP CRC and source/checksum checks passed. **No headset acceptance or Sprint 3 video is supplied by this release.** Run the outstanding checks below against this exact artifact before freezing `sprint-3` or another agreed canonical ref.

## Priority and acceptance

| Priority | Issue | Completion evidence |
| --- | --- | --- |
| P0 | [#59 Controller-only crew entry](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/59) | Quest controllers can create/join, edit/confirm room selection, ready and launch without a physical keyboard. |
| P0 | [#60 Immersive APK launch](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/60) | The downloaded release APK cold-launches in immersive stereo with head/controllers tracked, including restart. |
| P1 | [#6 Clear lobby sequence](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/6) | New users understand every control and disabled state without coaching. |
| P1 | [#61 One submission and sprint READMEs](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/61) | One exact ref, matching APK/checksum, video and start instructions per sprint; matching links visible from the submitted branch. |
| P1 | [#62 Sprint videos](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/62) | Accessible video links identify source/APK and demonstrate actual changes; headset fixes shown on headset. |
| P1 | [#63 One-person review path](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/63) | The advertised preview is reachable by one reviewer and clearly distinct from the two-player game. |
| P1 | [#31 Fresh-install playthrough](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/31), [#5](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/5), [#33](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/33), [#19](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/19) | Exact submitted artifact, two independent clients, shared progression/flood, recovery/retry, duration and observed issues recorded. |

Full criteria live in the issues. Two-player Storm Watch rules remain the target; four-player expansion, a shell migration and unrelated art polish are not required simply to fix the review blockers.

## Candidate build tools

[Technical candidate Editor evidence](../../Verification/Sprint3/README.md) records the completed checks and their limits.

[Repeatable Quest APK pipeline](../../Verification/StormWatch/QuestBuildPipeline.md) supplies configuration guards, fresh output/provenance and final binary verification. These are reviewable candidate changes, not completed release acceptance.

## Work order

1. **Tuesday:** reproduce the reported headset failures, identify exact failing APK/source, implement the smallest controller-entry and launch fixes, and clarify lobby feedback.
2. **Wednesday:** integrate reviewed changes; build one candidate; install the downloaded candidate on Quest; run the controller-only and two-client checks. Correct failures before recording final evidence.
3. **Thursday before submission:** freeze the passing source, publish APK/checksum, record/link videos, complete the sprint README, have someone follow it from a fresh start, and update every Project card to its evidenced status.

These are planned steps, not claims that work has already passed. Owners should claim the issue/project assignment as work starts; unassigned work is visible.

## Final handoff checklist

- [ ] Fill the authoritative submission ref and exact source commit above.
- [ ] Link the corresponding release APK, SHA-256 and build report here.
- [ ] Link the Sprint 3 headset demonstration here and the Sprint 2 video in its README.
- [ ] Record headset model/OS, client arrangement and passing/failing steps with evidence.
- [ ] Confirm immersive cold launch and restart of the downloaded APK.
- [ ] Confirm controller-only host/join, both ready, host launch and clear error/retry.
- [ ] Verify the documented one-person route and the actual two-player run.
- [ ] Record shared water/gates, distinct-player work, rescue, both-player victory/failure, retry and leave/rejoin; measure duration.
- [ ] Recheck all links from the submitted branch with reviewer-level access.
- [ ] Synchronize Project status with implementation/review/validation evidence; keep unverified work open.

Use the [lobby guide](../LobbyGuide.md) and [main launch/build guide](../../../README.md) as the current baseline. Update them if accepted Sprint 3 behavior changes.
