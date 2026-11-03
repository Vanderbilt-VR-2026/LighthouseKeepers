# Repository guide

[← Project home](../README.md)

## Where to go

- **Play or grade the game:** use [Releases](RELEASES.md). You do not need to pick a Git branch.
- **Stable submitted source:** `main`. Keep Sprint 3's `sprint3-final` tag/release and Sprint 2's `sprint2-final-v2` tag/release unchanged.
- **Current co-op development:** `coop/basics`, reviewed in [draft PR #74](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/74). It is not merged into main.
- **Sprint 2 release maintenance:** `sprint2/release`; original submission source remains at `archive/sprint2-final-submission`.
- **Exact co-op preview source:** tag `coop-preview-20261102` at `8e3b98a`; download instructions can evolve without relabelling the tested binary.
- **Feature work:** short-lived owner/feature branches and a PR. After merging, remove the branch when its owner agrees; preserve releases with tags.

## Cleanup review — November 2, 2026

No branches, tags, releases or PRs were deleted or closed in this review. A branch name alone does not show whether work is disposable. The checks below use exact ancestry, archive tags and open PRs.

### Recommended first batch: seven old branches

These branches were authored/maintained by Aneesh, have no open PR, and their tips are preserved in main or an archive/release tag. **Deletion is pending approval.** Deleting a branch removes its name; the preserved commits/tags and all submission downloads remain.

| Branch | Tip | Why it can be removed |
| --- | --- | --- |
| `aneesh/environment-shell` | `11aac60` | Merged into main; exact archive tag exists. |
| `docs/sprint-submission-downloads` | `1dcc722` | PR #70 merged; submission download links are in main. |
| `integrate/sprint-3` | `682fe40` | Contained in main/Sprint 3 release; exact archive tag exists. |
| `sprint3/one-lighthouse` | `7182583` | Contained in main and the preserved Sprint 3 release. |
| `sprint3/networking` | `bfa1062` | Exact tip retained by archive/sprint3-networking; old experiment, no open PR. |
| `sprint3/simulation` | `92dfdd2` | Exact tip retained by archive/sprint3-simulation; old experiment, no open PR. |
| `sprint-2` | `8c15fc6` | Historical Storm Watch snapshot, not the resubmission; retained by main-quest-8c15fc6 and main. |

### Keep; do not bulk-delete

- `main`, `coop/basics`, `sprint2/release`, `sprint2/final-submission`: active/stable/submission entry points.
- `sprint-3`: Eli's open PR #72 targets this branch. Do not remove a PR's base.
- `Sprint-1`, `parked/round2-main-folder`: tips are not proven contained in main or the submission tags; review/archive deliberately.
- All branches attached to open teammate PRs: keep until their owners decide how to merge or retire them.
- `codex/design-document-main`, `codex/main-quest-apk`, `codex/sprint3-feedback-docs`: merged/contained, but teammate-owned; candidates for owner-approved follow-up cleanup.
- `codex/design-document-sprint3`: PR #69 merged into `sprint-3`; do not confuse that with main ancestry.

### Open PRs at audit time

| PR | Branch | Purpose |
| --- | --- | --- |
| [#74](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/74) | `coop/basics` | Co-op basics: two players, shared tasks, shared bed |
| [#73](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/73) | `codex/readme-intentional-design` | Document intentional design choices in README |
| [#72](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/72) | `codex/first-person-network-proof-sprint3` | Show client input and disconnect over Photon in first person |
| [#71](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/71) | `codex/first-person-network-proof-main` | Show client input and disconnect over Photon in first person |
| [#67](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/67) | `william/sprint-3` | networking changes |
| [#57](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/57) | `william/sprint-2` | Add four-player Fusion multiplayer for William Sprint 2 |
| [#56](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/56) | `tapan/beacon-interaction` | Add beacon lever and bearing alignment wheel interactions |
| [#55](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/55) | `tapan/radio-interaction` | Add push-to-talk and frequency dial interactions to the radio |
| [#54](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/54) | `tapan/generator-starter-lever` | Add basic pull-lever interaction to the generator starter |
| [#48](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/48) | `tapan/station-anchor-binding` | Tapan/station anchor binding |
| [#47](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/47) | `tapan/remove-default-sun` | Remove default directional sun from exterior scene |

The docs-only `docs/start-here` PR updates the player-facing home page without merging the co-op implementation. It should be removed after its own merge, subject to the same branch policy.

## Protected history

Keep **all existing release tags and assets**, including `sprint3-final`, `sprint2-final-v2`, `archive/sprint2-final-submission`, and the older candidate/archive tags. Do not rename or retarget a submission tag. The older `sprint-2` branch is not the playable Sprint 2 resubmission.

The original repository checkout and any uncommitted user work are separate from this documentation cleanup. No local worktree is removed.
