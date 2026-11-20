# Finish pass — November 16

Authorized goal: a dependable complete game, with Night 2 as the first quality gate.
Preserve accepted controls, co-op ownership, the ship wheel, hands and existing audio.

## Order and acceptance

1. Baseline and interaction audit: save failures before edits; replay the actual physical solo loop. Resolve confusing next actions without changing accepted hand alignment. Night 2 must explain the blackout, reveal the culprit, allow recovery, guide the ship and offer sleep.
2. Flare and encounter (updated user direction): original flare pistol is the only active defense. Host owns ammo, obstruction and repulsion; the authenticated held prop owns input. Finish the requested sea-hag/doll/gaunt presentation, then a telegraphed hunter, reachable hiding and recovery. Lanterns are lighting only; omit bell/trap/stun alternatives.
3. Ship moment: preserve full-distance vessel and west-gallery wheel; validate beam contact, loss/recovery, pass payoff and both-player outcomes.
4. Campaign: finish clear mornings, save/continue, final ship/rescue and credits; preserve existing Night 1–3 rules. Human timings remain distinct from automated workload models. Demo needs more headroom.
5. Art/feel: reachable mounted controls, coherent near-field models, clipping/scale sweep. No skybox/avatar changes.
6. Audio: sparse useful speech, recognizable machine/entity cues, silence and recovery; actual headset listening remains required.
7. Co-op/recovery: two-Mac functional checks now, two-Quest acceptance when available; joins, props, voice, shared sleep, retry/rejoin/offline.
8. Release gate: full tests with baseline comparison, actual Quest worst-case profiling, new-player demo without coaching, one matching APK. Never claim device performance from Editor FPS.

## Baseline

Clean source 363c07e8. Focused baseline: 142/146 pass. Four existing failures: two M3RuntimeTests retain old duration/outage assumptions; two MasterDirectorTests retain old passive-beacon instruction/button assumptions. Receipts in artifacts/s5/finish/before/. These are recorded before any gameplay edits. Actual-XRI solo Night 3 baseline passed both ships and sleep into Night 4, zero errors. Quest not visible to ADB. Editor mute hook verified.

## Delivery rules

Commit and push each completed step with issue references. Update Next.md at each checkpoint. Install if connected, never launch without a new explicit request. New replicated fields require a new network version. Keep the draft PR open; do not merge main. Worn-device reach, comfort, listening and GPU/CPU remain DEVICE CHECKS PENDING until observed.

## Accepted steering — November 16

The flare gun is the **only active defense**. Lanterns, if added later, provide work light only; omit lantern stun, bell and traps. Grip the pistol handle with either hand; trigger fires a scarce round; an unobstructed flare drives the Drowned away. Use original Quest geometry: the supplied RuneFox237 Jinxxy package explicitly excludes standalone and commercial game distribution. Preserve its broad flare-pistol reference, not its package/code. Generator reference: VOLMATE portable generator / GEEKS VR horror generator. Rebuild the housing around accepted physical control locations: rounded tank, tubular cage, recessed supported panel, believable engine/alternator and worn finish.

Three original Drowned forms use the user’s sea-hag, doll and gaunt-creature references for silhouette only. One deterministic form per shared encounter, no local random selection and no change to avatar/sky.
