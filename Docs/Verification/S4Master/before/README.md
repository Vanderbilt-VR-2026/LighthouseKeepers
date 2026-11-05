# S4 master pass — baseline before gameplay edits

Runtime source: PR #79 merge `5dc5e56`; master branch documentation checkpoints add no gameplay changes. Unity 6000.3.23f1, 5 November 2026. Local raw results/captures: `artifacts/s4-master/before/`.

## Results

- Full EditMode: **262/262 PASS**.
- Full PlayMode: **34 passed / 27 failed, 61 total**. All failures below were present before this pass's gameplay changes. This configuration runs current S4/immersion switches enabled; the older audio baseline disabled some switches, so its 18 failures are not an equivalent comparison. Preserve this exact 34-pass set for subsequent regression comparisons. Do not call the full suite passing.
- Graphics solo Nights 1–2 → staged Night 3 arrival: **PASS**, zero captured errors; actual injected XRI grabs, pull/clutch repair, generation and sleep. A separate paced Day 3 graphics run: **PASS**, board 4/4, **79.96 seconds spare**, sleep into Day 4, zero captured errors. Navigation/reading allowances are explicit waits; not a human first-timer trial and not the new instruction-driven bot.
- Two Mac Editors, live Fusion room UPG4UB: **PASS** join/ready/House, both remote grey avatars visible, mono voice processing initialized, guest three-bolt task on both boards, shared sleep into Day 2, guest return to lobby and rejoin the active Day 2. Voice facts confirm native mono capture/AEC/noise suppression/listener references; no human audible speech/echo test was performed.
- Inspected [Day 3 completed board](day3-complete.png) and [host sees guest](host-sees-guest.png). Additional evidence: [shared Day 2](shared-day2.png), [guest rejoined](guest-rejoined.png), [transport events](coop-events.jsonl), [solo trace](solo-day3.txt).
- Public release APK download checked independently: size and SHA-256 match the exact-merge build. [Download verification](public-download.json).

The release is at https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint4-m1. Docs PR #80 adds the main download row; draft #81 is the master-pass work branch. No merges performed.

## Device budget — NOT VERIFIED

| Metric | Target | Current device measurement |
|---|---:|---|
| Frame rate | 72 fps sustained | NOT VERIFIED |
| GPU | ≤11 ms | NOT VERIFIED |
| CPU main | ≤9 ms | NOT VERIFIED |
| Draw calls | ~150 per eye view | NOT VERIFIED |
| Visible triangles | ≤300k | NOT VERIFIED |
| Audio voices | ≤24 | Existing cap; current workload NOT VERIFIED |
| Water GPU | ≤2 ms | NOT VERIFIED |

No Quest was available for the baseline by user instruction. Do not substitute Editor FPS, voice metadata, rendered screenshots or an APK build for headset acceptance. Device checks stay in `Docs/Design/Next.md`.

## Full PlayMode failures (pre-existing baseline)

- `LighthouseKeepers.Presentation.Environment.Tests.EnvironmentPresentationTests.LabUsesOnePlayerAndLightingUpdatesAllocateNoManagedMemory`: Expected: in range (0.22,0.462)   But was:  1.0f
- `LighthouseKeepers.Presentation.Environment.Tests.ShellTraversalTests.ExistingRigWalksAuthoredHouseToGalleryRoute`: RT-F3-TRANSFER stuck at (1.40, 6.61, -0.96) targeting (1.40, 6.60, 0.00)
- `LighthouseKeepers.Presentation.Environment.Tests.WatchGeneratorTests.RepairFirstWaitsForPowerAndThreeRealPullsRestorePump`: Expected: 1   But was:  0
- `LighthouseKeepers.Presentation.Environment.Tests.WatchInteractionTests.GrabRepairDrainAndSleepAdvanceDayWithoutMovingHead`: Physical wrist twist must advance the active coupling   Expected: 1   But was:  0
- `LighthouseKeepers.Net.Tests.CoopAppearanceTests.BothHouseSpawnsHaveStandingClearanceAndRemoteHandsAreVisualOnlyTanMeshes`: Unhandled log message: '[Error] Coroutine couldn't be started because the the game object 'Grab wrench — grip to hold' is inactive!'. Use UnityEngine.TestTools.LogAssert.Expect
- `LighthouseKeepers.Net.Tests.ImmersionInteractionTests.RadioHintsWaitNinetySecondsStayQuietAtStationsAndPlayOnce`: Hint while already at the generator   Expected: 1   But was:  2
- `LighthouseKeepers.Net.Tests.S4HardeningTests.BeaconDeadlineFailsBothAndRetryRestoresDayFaults`: Expected: 72.0d +/- 0.01d   But was:  96.000001430511475d
- `LighthouseKeepers.Net.Tests.S4HardeningTests.HighWaterRequiresContinuousThreeSecondsThenFailsBothAndRetryClearsLoss`: Expected: True   But was:  False
- `LighthouseKeepers.Net.Tests.WatchMenuFlowTests.FailedRetryButtonKeepsDayAndClearsPartialWork`: Expected: True   But was:  False
- `LighthouseKeepers.Net.Tests.WatchMenuFlowTests.TerminalReplayButtonResetsToDayOneWithoutMovingTrackedTransforms`: Expected: True   But was:  False
- `LighthouseKeepers.Net.Tests.WatchMenuFlowTests.TrackedRaysReachReplayRetryAndReturnWithNearbyWall`: System.NullReferenceException : Object reference not set to an instance of an object
- `LighthouseKeepers.Net.Tests.WatchReplicaTests.SameDayHostRetryResetsGuestRadioAudioAfterPreviousBroadcast`: Expected: True   But was:  False
- `LighthouseKeepers.Tests.BootstrapIntegrationTests.BootstrapLoadsTeamEnvironmentWithOneFloodAuthority`: Unhandled log message: '[Error] Scene 'LK_Bootstrap' couldn't be loaded because it has not been added to the active build profile or shared scene list or the AssetBundle has not been loaded. To add a scene to the active build profile or shared scene list use the menu File->Build Profiles'. Use UnityEngine.TestTools.LogAssert.Expect
- `LighthouseKeepers.Tests.FirstWatchPlayTests.AuthoredSliceRepairsBuysTimeAndRecoversAtLanding`: SetUp :   Expected: True   But was:  False
- `LighthouseKeepers.Tests.FirstWatchPlayTests.InspectorClampDurationControlsFeedbackAndExpiry`: SetUp :   Expected: True   But was:  False
- `LighthouseKeepers.Tests.FirstWatchPlayTests.MissingRequiredStationIsReportedBeforeInteraction`: SetUp :   Expected: True   But was:  False
- `LighthouseKeepers.Tests.FirstWatchPlayTests.SelectedBraceAndWristRotationActuallySealAValve`: SetUp :   Expected: True   But was:  False
- `LighthouseKeepers.Tests.FirstWatchPlayTests.WrongValveSurgeIsBoundedAndRetryWorksAfterDrowning`: SetUp :   Expected: True   But was:  False
- `LighthouseKeepers.Tests.PlayerTraversalTests.FootstepsFollowTravelAndStaySilentAtRestAgainstWallsAndInAir`: SetUp : System.NullReferenceException : Object reference not set to an instance of an object
- `LighthouseKeepers.Tests.PlayerTraversalTests.SnapTurnWithOffsetHeadDoesNotPlayFootsteps`: SetUp : System.NullReferenceException : Object reference not set to an instance of an object
- `LighthouseKeepers.Tests.PlayerTraversalTests.StandingOnEnvironmentIsGroundedAndReleasingStickStops`: SetUp : System.NullReferenceException : Object reference not set to an instance of an object
- `LighthouseKeepers.Tests.PlayerTraversalTests.TowerRouteAlsoWorksAtThirtyFramesPerSecond`: SetUp : System.NullReferenceException : Object reference not set to an instance of an object
- `LighthouseKeepers.Tests.PlayerTraversalTests.WalksAllThreeFlightsUpAndDownThroughLandings`: SetUp : System.NullReferenceException : Object reference not set to an instance of an object
- `LighthouseKeepers.Tests.PlayerTraversalTests.WalksThroughHouseAndBalconyDoorwaysInBothDirections`: SetUp : System.NullReferenceException : Object reference not set to an instance of an object
- `LighthouseKeepers.Tests.StormWatchSceneTests.LobbyApprovalBeginsOnlyWhenPartnerEnvironmentSendsInput`: SetUp :   Expected: True   But was:  False
- `LighthouseKeepers.Tests.StormWatchSceneTests.PhysicalPumpRequiresStrokesAndDisconnectOffersRestart`: SetUp :   Expected: True   But was:  False
- `LighthouseKeepers.Tests.StormWatchSceneTests.SceneHasNightLightingAndRepairActuallyOpensStairGate`: SetUp :   Expected: True   But was:  False
