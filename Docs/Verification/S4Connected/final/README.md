# Part E + Part D — connected tasks

Draft PR #83 stacks on s4/visuals / PR #82. Branch s4/connected. Each E step was committed and pushed separately; main was not changed. Will's skybox/avatar body and the existing hand rotation were not edited.

## WORKS — Editor evidence

- The existing powered pump/flood rule is retained. The radio report now requires an actually shining beacon; tuning and entered code survive a dark beacon. The Objective Director restores that dependency first. Physical conduit sleeves light upward when power returns; the pump gauge reacts to pump power; the console has a beacon-proof lamp and moving mechanical flag.
- The pipe is a two-person job: one keeper grips the separate pressure valve while the other tightens. Release restores unreduced spray/inflow and pauses repair without losing earned degrees. Host authority validates actor, proximity, ownership, revision and expiring holds. The accepted grip-only arm-pull ratchet is retained.
- Solo and host-alone use a one-grip clamp. A second keeper joining clears the solo latch; a disconnect exposes it again. Per-player guidance assigns valve and wrench roles. No extra loss timer was added.
- Two handheld radios live on F1/F4. Actual XRI checks cover grabbing, trigger transmission intent, head proximity, release and falling. Two-Mac live transport covers cross-floor routing in both directions, trigger release and microphone mute. Speech playback comes from the other head nearby or the held receiver during radio use, with wall/floor attenuation and radio colour. Native mono capture, AEC and noise suppression remain active.
- Fresh graphics solo Nights 1–3 pass through sleep. Night 3's paced scenario finished the jobs at 291.1 s and retained 138.8 s after its return-to-bed allowance. It is a scripted timing scenario with real XRI input and explicit navigation/read waits, not a new human first-timer measurement.
- Two independent Mac Editors over live Fusion pass join/ready, both avatars, host valve + guest three-bolt repair, release/pause/resume, voice routing/mute, shared sleep into Night 2, guest return/rejoin, host-alone clamp, then guest valve + host three-bolt repair. Both boards agree. Room TWR3TS was test-only and is closed.

## Regression accounting

Before: fresh graphics solo Nights 1–3 and two-Mac join/avatar/voice-configuration/guest-task/shared-bed/rejoin passed at 164633f. Full-suite reference is Part C: 272 EditMode passes and 40 effective PlayMode passes, with 25 inherited failures.

After: **280/280 EditMode passed**. The complete PlayMode run produced **39/65 passes**: the same 25 inherited failures plus the wrench-motion test missing the new solo-clamp prerequisite. Its setup now sets that prerequisite; gameplay was not bypassed or changed. The focused rerun passes all of that test's existing hand/stance assertions. Combined coverage is **40 passing cases, the same 25 inherited failures, no new unresolved failures**. This is a full run plus one targeted rerun, not a claim that the full run itself returned 40/65. [Counts and exact failure names](regression-comparison.json) · [full suite](full-suite-summary.json) · [targeted result](wrench-targeted.xml).

The first radio automation attempt grabbed in the same instant it teleported a synthetic hand; the host rejected the stale pose. Preparing the hand pose before gripping fixed the harness. Production ownership checks were retained. Failed raw evidence is preserved under artifacts/s4-connected/after/two-mac; the successful complete rerun is two-mac-fixed.

## Network / voice evidence

Both clients need **LK-S4-CONNECTED2 / LK-Lab-Coop-v17**. Valve state and handheld transmitter IDs are new host-owned fields. No avatar replication or voice capture format change.

At the tested F1/F4 separation, direct voice targeted gain 0.0222 with an approximately 1800 Hz low-pass; held-radio reception targeted gain 0.9 with a 3500 Hz upper cutoff. Trigger transmission and head-proximity transmission worked in opposite directions. Microphone mute cleared host-approved transmission and removed the remote stream. Both clients reported mono 48 kHz remote transport and live AEC/NS processors. These are routing/configuration facts, **not a human judgement that speech sounds good**. [Voice records](voice-routing.json).

## Tuning and limits

| Setting | Value |
| --- | --- |
| Leak while valve held/clamped | 65% of normal; release returns to 100% |
| Missing hold heartbeat expiry | 0.55 s |
| Conduit restore travel | 2.4 s, bottom to top |
| Full nearby voice gain | Within 3 m |
| Distance falloff | To 8% at 18 m, before wall/floor attenuation |
| Wall gain / cutoff | 55% / 1800 Hz |
| One-floor / two-floor gain | 40% / 7%, multiplied by distance/wall gain |
| Radio activation near head | Within 0.38 m, or held trigger |
| Radio band | 350–3500 Hz, mild crunch/static and original squelch |

Values and switches live in ConnectedTasksTuning. Listener holds a radio to receive the clear cross-floor channel. No second voice stream is created.

## NOT VERIFIED

Quest install, tracking/reach/grip comfort, both-eye text readability, actual cross-floor listening and echo cancellation, Mac+Quest/two-Quest acceptance and sustained performance are pending. The headsets were offline, as instructed. Approximate ray/floor attenuation is not a physical acoustic simulation. Props are deliberately simple; the new generator/beacon/radio two-person puzzle designs are documented but **only the pipe puzzle is implemented**.

The inherited captures show some HUD subtitle/partner-text crowding and dark remote hands under this lighting, both present before Part E. Those were not altered in this pass; avatar body remains William's area. Existing Part C visual limitations and legacy test failures remain documented.

| Device budget | Target | Result |
| --- | --- | --- |
| Frame rate / GPU | 72 fps / ≤11 ms | Device pending |
| CPU main | ≤9 ms | Device pending |
| Draw calls / visible geometry | ~150 per eye / ≤300k triangles | Device pending |
| Water GPU | ≤2 ms | Device pending |
| Audio voices / added voice CPU | ≤24 / measure direct vs radio | Device pending |
| Android render settings | MSAA4x, scale1.25, fixed foveation | Retained; budget acceptance pending |

## APK and device session

`Builds/LighthouseKeepers-S4-PartE.apk` — Android IL2CPP build succeeded from `ce3b8f7a3a5355f49c5f6aa5c50adeb74813cf6f`, 125,968,326 bytes. SHA-256: `dca2a05ece210708281331ff327d630f694c0eda2f0dc0b559d32e1bb79cb4d3`. [Machine-readable build result](build-result.json). Install skipped. Use [TwoHeadsetSession.md](../../../Design/TwoHeadsetSession.md) for the ten-step acceptance session; it covers setup/join, both pipe roles, release surge, dependencies, speech/radios/mute, sleep and rejoin. All device checks remain in [Next.md](../../../Design/Next.md).

## Inspected captures

[Before avatar](screenshots/avatar-before.png) · [after avatar](screenshots/avatar-after.png) · [held valve](screenshots/valve-held.png) · [shared board after role swap](screenshots/shared-board-reversed-roles.png) · [radio held on F4](screenshots/radio-in-hand.png) · [conduit off](screenshots/conduit-off.png) · [conduit on](screenshots/conduit-on.png) · [beacon proof lamp](screenshots/radio-proof-lit.png) · [Night 3 survived](screenshots/night-three-survived.png). The screenshots were opened and visually inspected; they do not certify headset comfort or performance.
