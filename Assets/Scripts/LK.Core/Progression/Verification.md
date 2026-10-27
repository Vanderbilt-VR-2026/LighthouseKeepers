# Session D evidence — 2026-10-20

STATUS: [PROPOSED] — source and offline checks exist; Unity import, Android packaging, integration and headset execution have not passed.

Base: `sprint3/base`, `4391afdbcd10615eb9e68620c6ec453c7561b39f`. All source edits are in D's paths, their associated Unity metadata, `Docs/DECISIONS.md`, or D-to-session request files. No package was added, no teammate branch was merged, and no shared scene/rig was rewritten.

## Executed checks

- Unity's bundled Roslyn compiler compiles the actual progression and board source against Unity 6000.3.23f1's engine and .NET Standard reference assemblies, separately for release and development. The editor build helper also compiles against installed UnityEditor references. This uses no fabricated Unity stubs. Compiler/API validation does not import Unity assets, run the engine, or exercise native APIs.
- The offline test runner loads the authored day assets' scalar/enum data into its rules fixture and passes all **20 scenarios** printed in `artifacts/progression-verification.txt`. It checks solo routes, incident timing/one-shot behavior, optional pumping, fuel/primer/choke failures, cooldown/recovery, one command per immediate valid crank, actual-engine confirmation, ten-second warm-up, shared contributions, two-keeper gating, stale/released/unsafe/same-actor isolation, disconnect pause, early bed/failure precedence, frozen results, same-day retry, board content, invalid data and data-authored later days.
- Binary inspection confirms `JumpToDay` is absent in the release progression assembly and present in development. Script GUID/catalog links are checked as text; this is explicitly not Unity deserialization.
- Unity batch compilation was attempted after meaningful source batches. Startup is blocked before import/compilation by sandbox errors: `attempt to write a readonly database` and `listen EPERM: operation not permitted /tmp/Unity-Upm-<pid>.sock`; the package manager times out. Evidence: `artifacts/compile.log`.
- Android build was attempted using the exact README command. An initial overlap with a still-exiting Unity process hit its project lock. The final **sequential** compile/build retry returned `COMPILE_EXIT=1` and `ANDROID_EXIT=1`, both due to the package-manager socket error. The Android trace is `artifacts/progression-android.log`. No APK has been produced by this session. Do not install an older Storm Watch binary and treat it as evidence for this source.
- The bundled SDK's `adb devices -l` was attempted. Its daemon cannot bind the smartsocket listener (`Operation not permitted`), so device discovery/install is also unavailable here. Evidence: `artifacts/progression-adb-devices.txt`. No headset has been exercised.
- `git fetch team` cannot write this worktree's Git metadata, which points outside the writable sandbox. A temporary bare Git directory at `/private/tmp/lk-session-d.git` preserves conventional commits on `sprint3/progression`, based on the exact local `sprint3/base`. Fetch/push to team also fails because `github.com` cannot resolve. The reviewable source remains in this checkout; `artifacts/sprint3-progression.bundle` preserves the branch for import outside the sandbox.
- A local speech attempt returned a zero-sample audio file. No empty or misleading voice clip was added. Day briefing narration remains unassigned pending recorded assets; the existing Button Pop sample is assigned in the catalog for quiet checkbox feedback.

APK: **not produced**. Intended output: `Builds/LighthouseKeepers-Sprint3-Progression.apk`.

Install **after a successful integrated build**:

```sh
adb install -r Builds/LighthouseKeepers-Sprint3-Progression.apk
```

## Headset acceptance — not executed

1. Start Day 1 alone. Read/listen to its brief while moving immediately. Observe no leak before 15 seconds, then locate the spray by ear. Pick up the wrench, approach the pipe, repair it, watch water fall, and grip near the bed while standing. The board ticks each step with a soft cue and shows time, peak water, pump strokes and generator restarts in results. Pumping is optional.
2. Restart Day 1 and try gripping the bed before repair/drain; nothing advances. Allow flooding to fail: a short stationary fade and **The lighthouse flooded** appear. Choose Retry and confirm Day 1 restarts with no earlier-day penalty and a fresh 15-second orientation interval.
3. In a development build, choose **DEV: Day 2** directly. Confirm its intro voice is skipped, generator/pump begin normal, and the previous unlocked-day save is retained. At 45 seconds, hear the engine fall away/contactor, see RUNNING extinguish/FAULT light, and see the room become dim red over about 1.5 seconds as powered pumping stops.
4. On Day 2, pull the starter before priming. Hear a failed crank and the primer-specific audible cue, with no successful catch or running lamp. Try again within two seconds and confirm the cooldown. Inspect quarter-full fuel, prime three times, close the choke, pull, and hear a distinctly different catch/rising engine note only when the engine actually runs.
5. Restore Day 2 alone: power returns warm, pumping resumes and water drains; the board advances after every step and finally asks for bed. Confirm opening the choke during the first ten running seconds is rejected, then allowed afterward. Repeat while spending time on the manual pump first; it buys time without substituting for restart. Hear the water from F2.
6. Attempt Day 3 with one keeper. Read **Day 3 requires two keepers.** Confirm no leak timer runs and no solo helper appears. With two keepers, hold/correct the F2 isolation valve while the partner repairs on F1; both see safe-band pressure feedback and communicate the damage detail.
7. During Day 3 repair, release the valve or leave the safe band. Repair must stop before changing the leak. The same actor cannot hold and repair, and stale/disconnected input cannot keep the valve authorized. Re-establish a fresh partner hold, finish, drain and rest; no mandatory crouching or forced camera motion occurs.
8. Continue from a successful day, terminate/relaunch, and confirm the host resumes the saved current/next unlocked day. Confirm clients use the host's board/progression, not their own local checkpoint. Test the final-day results and verify no unusable Continue button appears. In a non-development build, verify no DEV selector is present.

## Required merge follow-through

All are requests, not claims of integrated behavior: [A](../../../../../Docs/Sprint3/Requests/D-to-A.md), [B](../../../../../Docs/Sprint3/Requests/D-to-B.md), [C](../../../../../Docs/Sprint3/Requests/D-to-C.md), [E](../../../../../Docs/Sprint3/Requests/D-to-E.md). A supplies physical reports, drifting isolation and starter audio. B supplies authority and incident APIs. C supplies the new scene, board controls, fault lighting/audio, gauges, narration and fade. E supplies authority/roster/validated remote reports/replication. The original Storm Watch design remains unchanged until the new entry is explicitly integrated.

Import the preserved branch outside the sandbox without replacing this checkout's files:

```sh
git bundle verify artifacts/sprint3-progression.bundle
git fetch artifacts/sprint3-progression.bundle sprint3/progression:sprint3/progression
git worktree add ../LighthouseKeepers-Progression sprint3/progression
git push team sprint3/progression
```

The bundle requires the existing `sprint3/base` commit. Git will refuse a conflicting local branch rather than rewrite it. This checkout remains at its original detached HEAD because its metadata directory is not writable; the commits and named branch are preserved in the bundle and temporary Git directory.
