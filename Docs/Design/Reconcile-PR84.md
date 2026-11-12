# Reconcile PR #84 with the Sprint 4 stack

## Scope and provenance — Step 1, no code changes

Fetched team on 2026-11-09. PR #84 is `s5/playtest-feedback`, head `25341be164ce47a35ed59293324558ca792a1e2b`. Its base and merge-base are current main `5dc5e567d8e64f154e2ff294a82886080801cecb` (merged PR #79). Its two commits are `554f523` (playtest response) and `25341be` (durable shell nudges). Our stack is #81 s4/master → #82 s4/visuals → #83 s4/connected, head `1ab1df572596f9e7447f1c4f23204a145323a3f6`. Neither side has been merged to main. No gameplay/source changes were made for this mapping.

Direct file overlap is only nine files; independently named additions also overlap in behavior. A clean text merge would not prove correct integration.

| Feature | Ours (#81–83) | Eli's (#84) | Conflict? | Recommendation / agreed resolution |
| --- | --- | --- | --- | --- |
| Wrench input / teaching | MasterPassTuning gripOnlyWrench; automatic proximity snap and preserved arm-pull progress; solo clamp / two-player valve | Retains PR79 trigger clutch; new bench plate and cue3 teach trigger | Yes, incompatible teaching/input | Keep grip-only + auto-snap. Bench plate `GRIP TO HOLD · BRING TO BOLT`; update cue3 caption and tests. Keep valve puzzle. |
| Tracker / HUD | Objective Director, nearest urgent role, timed reveal, wrist/Y-X recall, completion/wake cues; head trailing | World-fixed placement and explicit re-placement on recall; completion/wake captions | Yes, placement and competing job selectors | Keep world-fixed placement; use our authoritative director and existing recall/completion/wake behavior. One tracker, one instruction. |
| Start grace / failure grace | Night1 no-fail, tutorial water cap; first generator grace120s and existing Day3 power-failure grace | Solo clock/flood wait until first work or45s; crew bypasses | Yes, clock ownership and inconsistent tuning | Add start gate every night for host-owned solo/crew state; retain Night1 no-fail and explicit first-appearance station grace. Centralize grace values in one tuning asset. Existing ours is not a generic all-station grace system yet. |
| Proximity voice | CrewSpatialVoice distance + wall ray + floor attenuation; faint two floors away; live two-Mac routing checked | CrewVoiceSpatializer / CrewVoiceAcoustics floor/door loss, silent2+ floors; Editor tests only | Yes, two audio filters/followers | Keep better-tested single playback route; retain mono native capture/AEC/NS. Port useful settings/tests rather than running both processors. |
| Handheld radios / walkies | F1/F4 handsets, head proximity or trigger, host-approved transmitter IDs, existing grab lease | Two House-bench CrewRadioHandsets; both-held channel uses grab owner without extra snapshot fields | Yes, four props and two audio routes | One pair on the House bench. Reuse existing grab owner; retain tested head/trigger routing and release/mute checks. Delete unused duplicate runtime implementation. |
| Ocean | WorldWaterPresentation + KeeperOcean shader, four waves, detail normals/foam, storm stage | StormSeascape radial10.7k-vertex mesh + StormOceanWaves, shore/horizon damping, glint/whitecaps | Yes, independent renderers/materials | Choose only after matched Quest samples and screenshots. Delete losing renderer/shader path, retain flood/pipe water. |
| Coast | Local rocky decorative inlet, merged simple meshes, wet ground/story dressing | DistantCoast silhouette + faint harbor lights, one draw | Potential duplicate coast silhouettes | Compare on device; keep one coherent coast treatment with selected ocean. Do not open unsafe routes. |
| Sky | Leaves Will's skybox ownership intact; lighting/fog/dawn blend | Rewrites StormSky and applies cloud lightning/ramp | Ownership conflict, not a textual merge conflict | Lead explicitly prefers Eli’s sky (2026-11-09) and lifted the ownership concern. Enable its separately named switch in this build; preserve the original shader asset for rollback. No additional sky design changes. |
| Flicker / coplanar shell | All-scene coplanar scan, scene offsets/material/occlusion fixes | Prefab3–4mm nudges + LighthouseBuilder.CoplanarNudge table reapplied on rebuild | Yes, scene overrides can double-offset or hide prefab changes | Consolidate durable nudges into authoring table. Inspect/reset only overlapping scene overrides; rerun coplanar audit and rebuild invariant tests. |
| Fall guards / collision | WorldSafety, sampled edge audit, ramps/pole safety, decorative collision fixes | Three F3/F4 guards, shallow UCX rock lowering, player ignores wrench | Semantic duplicates + complementary fixes | Keep coverage from both; dedupe guards and confirm route clearance. Explicitly test8cm F1 Support foot and dropped wrench. |
| Lens / lantern glass | Transparent Fresnel rings, glass/brass, reflection probes and powered core | WatchBeacon.lensAlpha, FresnelAmber/glass smoothness changes | Yes, runtime material override versus authored material | Use one lens material path, preserve useful alpha/smoothness tunables and powered behavior; verify stereo sorting and cost. |
| Hint ordering | Objective Director owns step/job decisions, pressure/next-job cues, once-per-night hints | ObjectiveIndex hint order, holding wrench suppresses only until repair; staged Night3 lines | Yes, competing priority sources | Director remains sole instruction authority; port corrected post-repair wrench and staged-night hint logic. |
| Captions | Central strings, PA subtitles, next-step and waking guidance | Done/next/floor, Tonight, grace, put-wrench-down captions; cue3 trigger text | Yes, duplicated or contradictory text | One caption source with concise centralized strings; keep Eli's useful transition/grace captions adapted to grip-only and director order. |
| Storm ramp | Host-owned StormDirector stage (time/danger), nightly visual water/rain scaling | Night-based seascape/cloud intensity ramp | Yes if each writes ocean/sky uniforms | Retain authoritative stage; combine night baseline with the chosen ocean's parameters. Optional sky only reads it. |
| Network version | LK-S4-CONNECTED2 / snapshotv17; valve/radio state | LK-S5-PT1; different grab registry | Incompatible clients | New `LK-S5-INTEGRATED1` for both clients; bump snapshot protocol if fields change. Validate matching registry and host-owned transitions. |
| Pole / connected stations / co-op | Pole, dependency director, pump/beacon/report chain, valve puzzle, shared bed/rejoin | Not implemented | No equivalent to replace | Keep ours; rerun all shared mechanics against Eli's complementary fixes. |

## Every file touched by both sides

- `Assets/Scripts/LK.Net/CrewVoiceSession.cs`
- `Assets/Scripts/LK.Net/Tests/PlayMode/ImmersionInteractionTests.cs`
- `Assets/WatchDemo/Runtime/HarborRadio.cs`
- `Assets/WatchDemo/Runtime/ImmersionWorld.cs`
- `Assets/WatchDemo/Runtime/ObjectiveHud.cs`
- `Assets/WatchDemo/Runtime/WatchDemoController.cs`
- `Assets/WatchDemo/Runtime/WatchHouseFlood.cs`
- `Assets/WatchDemo/Runtime/WatchState.cs`
- `Assets/_LighthouseKeepers/Scripts/Runtime/Core/ImmersionTuning.cs`

## Independent additions that must not run twice

`CrewSpatialVoice` versus `CrewVoiceSpatializer`/`CrewVoiceAcoustics`; `HandheldCrewRadio` versus `CrewRadioHandset`; `WorldWaterPresentation` ocean/coast versus `StormSeascape`/`StormSeascapeGeometry`/`DistantCoast`; our world-safety/edge guards versus `PlaytestWorldFixes`; our runtime glass treatment versus `WatchBeacon` lens material changes. The full changed-file comparison is preserved in `artifacts/reconcile/overlap.json`.

## Verification plan and evidence boundaries

Step2: same package, one installed build at a time; same72Hz workload/viewpoints in House,F1,F2,F4,gallery sea and peak storm. Record GPU/FPS, render settings, head pose, focus, thermal context and captures. Whole-build comparison does not isolate ocean from sky/lighting/resolution; report that limitation and use feature-off comparisons where needed. Do not label CPU+GPU minus GPU as main-thread CPU.

Step3: branch s4/integrated from main, merge our stack, then #84; no main merge. Run both baselines, reconciled suites, solo1–3 and two-Mac valve/radio/sleep/rejoin. Step4: device profiling, collision/wall/edge/text/grace/glass checks plus Mac+Quest voice/task/sleep. Step5: one draft PR supersedes #81–84 without closing any.

Device measurements below are whole-build comparisons; resolution differs. Final integrated performance still requires measurement.

## Live comparison notes

The lead saw our new interior textures and reported the sea clipping in/out, with both placeholder and recorded voices. Eli’s unchanged build then showed the older interior and a preferred ocean/mountain background without the observed sea clipping. The apparent restart was the agent installing the second APK, not a game crash. Initial black adb captures were inconclusive and must not be treated as a startup failure; subsequent logs confirm our Lab rendered. Our first route was interrupted; the lead repeated it later. Both complete test baselines are recorded below.

Lead follow-up: explicitly prefers Eli’s ocean/water and mountain background. Keep that exterior selection, validate cost in the integrated build, and retain our interior/guidance/audio additions. The gallery incorrectly selects muffled rain while the lantern interior selects the rooftop layer correctly. `RainSoundscape.Weights` uses a4.4m tower radius on every floor, misclassifying the exposed F4 gallery as interior; fix its height-specific enclosure boundary and test lantern→gallery in every direction. Lead also dislikes the muffled layer’s quality and still sees mesh clipping in PR84. New status/tutorial lines are absent from PR84 as expected; integration must preserve ours.

Full baseline results: ours EditMode280/280, PlayMode40/65; Eli EditMode284/284, PlayMode39/64.24 failures are common; ours additionally fails RadioHintsWaitNinetySecondsStayQuietAtStationsAndPlayOnce, Eli additionally fails ExistingRigWalksAuthoredHouseToGalleryRoute. Preserve the union of baseline passing tests, stored in artifacts/reconcile/baseline-comparison.json. These are actual full-suite results, not an all-green claim.


## Selected exterior and measured comparison

The lead selected Eli’s ocean, mountain coastline and sky. Integrated uses one `StormSeascape` renderer and one distant-coast mesh; our flood/pipe water stays. `VisualPassSettings.eliStormSky` enables a separate copy of Eli’s shader without overwriting the original sky asset. The new textures, PA announcements, recorded voice lines and additional tutorial/status placeholders remain.

| Stationary captured view | Ours: scale 1.25, MSAA4 | Eli: scale 1.0, MSAA4 |
| --- | --- | --- |
| House | insufficient stationary samples | 9.60 ms median / 10.38 p95, 72 fps |
| F2 normal | insufficient stationary samples | 10.24 / 10.30 ms, 72 fps |
| F2 strongest night storm | insufficient stationary samples | 10.72 / 11.14 ms, 72 fps |
| F4 | 25.97 / 28.16 ms, 35.5 fps | insufficient stationary samples |
| Gallery sea, strongest night storm | insufficient stationary samples | 9.26 / 9.49 ms, 72 fps |

Routes included F1 and gallery on both APKs, but movement/focus left insufficient matched stationary samples in several views. Do not fill those cells using nearby floor samples. Raw logs, poses, screenshots and parser are in `artifacts/reconcile/device-*`. Pacing time from Unity Main Thread includes waits and is not CPU work time. Integrated adds the OpenXR app CPU metric and starts at scale1.0 with4x MSAA, pending its own target-device test. The resolution change means this table does not isolate ocean or sky cost.

## Integrated decisions and regression results

New network version `LK-S5-INTEGRATED1`, snapshot18. Both clients must update. Gate engagement, gate elapsed time, first-station grace deadline, taught-station mask and retry mask are host-owned snapshot fields. Active grace values live in MasterPassTuning: start45s, generic first station60s, generator120s, Day3 solo90s/crew60s. Night1 remains no-fail. Timing/flood balance otherwise stays unchanged.

The gallery audio boundary now follows the smaller F4 enclosure, so the outside walkway uses the exterior rain bed; the enclosed lantern keeps rooftop rain. The interior recording retains its own muffled character with less additional low-pass filtering (6500Hz instead of2800Hz). Lower-floor outside zoning is unchanged.


Integration checks caught and corrected a real interaction between the two grace systems: the tutorial door used the paused simulation clock, so its30s fallback could not open during Eli’s45s start gate. It now counts elapsed simulation plus start-gate time. The additional grace fields exceeded the old2KiB message cap (2137-byte max-notice test envelope). FusionMessageChannel now permits a bounded4KiB envelope; protocol18 still validates host, nonce, sequence, fields and ranges. This avoids silently dropping snapshots and blocking the guest’s ready state. No competing transport was added.

The shell builder now carries the12 reviewed Lab visual-mesh corrections as well as Eli’s10 transform nudges. Original FBX/collision are unchanged. Separation tests check a real gap, rather than requiring the throat surface to be below the House when the reviewed visual-only mesh places it above.

The first focused PlayMode pass verified gallery/lantern/House rain weights, world-fixed tracker recall, exactly two bench radios, one ocean renderer, no active Support foot collision, rock/wrench collision safety, floor-edge coverage and both-hand wrench arm motion. Remaining traversal/narration checks are being rerun after the targeted fixes.

Latest regression results: all305 EditMode tests pass. Of21 focused PlayMode tests,20 now pass after the final3/3 shell/glass rerun; the remaining SameDayHostRetryResetsGuestRadioAudioAfterPreviousBroadcast failure existed in both unchanged baselines. The House-to-gallery route, idle-hint policy and bounded full snapshot tests now pass. Graphics solo/co-op and integrated Quest acceptance remain pending.

The merged PlayMode result set overlays focused reruns on the full run: 47 passing cases,24 failures shared by both original baselines, and **zero missing baseline passes**. This is an aggregate, not a claim that a fresh full suite is all green. `artifacts/reconcile/integrated-rerun/aggregate.json` records the source run for each case.

Graphics Nights1–2 pass through physical clamp, both-hand bolts, dropped-wrench recovery, generator startup and shared presentation transitions. The first replay exposed a dawn wash tied only to the paused simulation clock; it now advances through start grace and the replay passes. The72-view Editor wall sweep completed and its contact sheet was inspected. Views inside/near pillars are occluded; still captures cannot certify continuous-motion Quest flicker or stereo glass sorting.

The first live two-Mac run exposed a second size check inside `LatestSampleBatch`: payloads above2KiB were still silently discarded despite the outer channel fix. A single4KiB constant now governs both. A2300-byte round-trip regression and the actual full snapshot through the batch pass; the live rerun now delivers guest strokes and pauses safely when the other keeper releases the valve. No additional replicated fields or network-version change were needed.
