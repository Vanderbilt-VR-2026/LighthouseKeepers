# Reconciled One Lighthouse candidate — verification

Runtime base: `sprint3/one-lighthouse` at `d2e30665a44b29a4e3bf84a7bfb7b049f93317b4`; later teammate docs through `7182583` and `sprint-3` through `e90e303` are also preserved in PR #65. PR #66 is superseded, with no branch deletion or main merge. This evidence replaces the earlier PR candidate's old-world numeric-keypad/legacy-control checks. The published e88f4e1 APK remains historical and does not contain this Lab integration.

## Changes under verification

- One alphanumeric six-character crew keypad; no duplicate numeric keypad beneath Play Solo.
- Lab crew launch rejects two connected keepers at UI and raw transport boundaries while preserving offline solo, roster/readiness and host-alone diagnostics. Lab shared gameplay is not implemented.
- Controller Watch Menu, real return-to-lobby cleanup, same-day failed retry and Day 1 replay after completion; tracked rig/hand transforms preserved.
- One APK pipeline for current build menus, validated against actual BuildPlayerOptions scenes: Network Lobby then Lab. BuildReport, source/scene/options record and final binary verification remain required.
- Architecture checks recognize the explicitly retained networking adapter assemblies and declared test tooling, while keeping new gameplay/simulation free of network dependencies and the pure core free of Unity dependencies.

## Coordinated results

| Check | Actual result |
| --- | --- |
| Full EditMode | **200/200 passed**, no skips; [sanitized report](EditMode.xml). Runtime source `df66a6f`. |
| Relevant Lab/lobby PlayMode, headless | **23 passed, 0 failed, 2 graphics-only skips**; [sanitized report](LabPlayMode.xml). Source `d6b6e87` adds only teammate shipping docs to the tested runtime. |
| Graphics-enabled crew keypad + Watch Menu | **7/7 passed**, no skips; [sanitized report](TrackedUI.xml). Covers the two headless skips plus five overlapping regression cases. |
| Distinct selected PlayMode cases across both runs | **25/25 passed**. Includes two opt-in real-Photon diagnostics using peers in one Editor; no shared Lab gameplay claim. |
| APK verifier regression tests | **4/4 passed** (`python3 -m unittest discover -s Tools/tests -p 'test_verify_quest_apk.py'`). |
| Fresh Android player | **Succeeded, 0 errors, 7 warnings**, clean source `89199c5a10e5a077758e6c0b2710595afab60826`. [APK / records](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint3-one-lighthouse-candidate-20261027) · [build evidence](Build.md). |

The PlayMode selection covers `LighthouseKeepers.Net.Tests`, `LighthouseKeepers.Presentation.Environment.Tests`, `LighthouseKeepers.Tests.LobbyControllerEntryTests` and `LighthouseKeepers.Tests.FusionLobbyIntegrationTests`. Cloud diagnostics ran with `LK_FUSION_CLOUD_TESTS=1 LK_RUN_ONLINE_TESTS=1`. The graphics run selects `CrewLobbyBoardTests` and `WatchMenuFlowTests` without `-nographics`. All runs use Unity 6000.3.23f1, the installed `unity test` CLI, the explicit worktree path and `-job-worker-count 2`.

This is not a claim that every historical PlayMode fixture passes under the two-scene ship manifest. The teammate's unmodified-base full-suite failure inventory remains in [M7](../../OneLighthouse/M7.md); Bootstrap/First Watch/Storm Watch fixtures need their own historical scene manifests. Earlier M4 results describe their own source and are not substituted for this run.

Unity repeatedly reserializes the water-surge and four radio materials on import/render. The candidate records that canonical Unity/URP serialization (asset version/locking metadata and float formatting; shader, texture and authored appearance preserved) so importing a clean build checkout does not dirty it before provenance is recorded. No rig, hand anchor or station artwork was reauthored.

No target-headset acceptance is claimed from compilation, Editor execution, packaging, installation or a running process. A real video, immersive cold launch/restart, controller reach/readability, physical station use, sound mix and device performance remain separate checks. Co-op acceptance cannot pass while shared Lab state is unfinished.

Raw Unity command lines/runtime output are omitted from published reports; sanitized NUnit results retain test names, results and timings.

Issue labels are updated from the final scope/evidence. Final GitHub Project card-field synchronization could not be completed because the browser/CDP connection timed out and the connector does not expose Project editing; do not infer synchronized Project fields from the issue labels.
