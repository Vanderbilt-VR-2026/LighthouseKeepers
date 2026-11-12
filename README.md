# Lighthouse Keepers

VR lighthouse survival for **Meta Quest 3**.

## Submissions — start here

| Sprint | Open the submission | What is included |
| --- | --- | --- |
| **Sprint 4 — M1** | **[Download Quest APK](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint4-m1/LighthouseKeepers-Sprint4-M1.apk)** | Immersion + audio milestone · [release notes](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint4-m1) |
| **Sprint 3** | **[Sprint 3 submission](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/tree/sprint-3)** | Playable APK + submission video |
| **Sprint 2** | **[Sprint 2 resubmission](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/tree/sprint-2)** | Playable APK resubmission |
| **Sprint 1** | **[Sprint 1](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/tree/sprint-1)** | Original environment + APK |

**Professor / TA:** open **Sprint 3** for the submitted game and video. Open **Sprint 2** for the playable resubmission. Each page has its download at the top and short install instructions. No Unity or GitHub account is needed to download.

## Current development

This development branch contains the current playtest fixes and co-op systems. The sprint branches preserve the submitted versions; their downloads are separate from current development.

[Build in Unity](Docs/DEVELOPMENT.md) · [Play together on two Quests](Docs/TwoQuestSetup.md) · [Team design](Docs/DesignDocument.md)

## Play this development build

Install the same APK on both Quests using SideQuest, or `adb install -r <file>.apk`. Open **Lighthouse Keepers** from Unknown Sources.

1. Press **PLAY**.
2. Choose **HOST / PLAY ALONE**. Press **START** immediately to play alone, or share the large crew code with a friend.
3. Your friend chooses **PLAY → JOIN CREW**, enters the code, then presses **JOIN CREW**. The host presses **START**. There is no READY button.
4. Without internet, the host screen says **No connection — Play offline**. Press **START** to play the same lighthouse alone.

Both players need **NETWORK VERSION LK-S5-PLAYTEST1**. A friend can join an ongoing watch using its code. If they leave, the remaining keeper gets solo puzzle rules. Grip holds tools and squeezes the primer; **B** recalls the objective and last announcement.

## Design — S4-M3

The keeper lobby is the first scene: a physical crew ledger introduces the role and a clear choice between solo and joining a friend. The intended next beat is House arrival, the harbor master on a wall radio, tonight’s jobs on the chalkboard, then the wrench on the bench.

Equipment uses direct grabbing and physical turning/pulling. This milestone replaces detached instructions with mounted plates, chalk, gauges, practical lights and sound, and gives the wrench a forgiving snap onto a bolt. Menu rays remain for deliberate lobby/logbook actions. Current tan hands, controller alignment and finger blending are preserved; held tools retain their established grip poses.

These choices shipped in Sprint 4 Milestone 1. [Design rules](Docs/Design/Immersion.md) · [Professor playtest](Docs/Playtests/2026-11-03-professor.md) · [Radio script](Docs/Design/RadioLines.md).

## Project history

Submission branches **sprint-1**, **sprint-2** and **sprint-3** preserve hand-ins. Active Sprint 4 work uses separate review branches; previous branch histories are saved under archive tags.

[Browse the archive](Docs/Archives/Branches-2026-11-03.md) · [All releases](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases)
