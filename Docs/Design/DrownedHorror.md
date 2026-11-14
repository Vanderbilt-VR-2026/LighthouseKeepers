# The Drowned: sound and presence

November 14. Latest direction: take creative liberty with frightening effects, music and sound. This pass develops the existing apparition; it does not silently enable a hunter before escape tools exist.

## The encounter

1. **Something stops.** The existing host warning lowers storm/music for about 2.5 seconds. Wet boot weight, cloth dragging and a strained inhale come from the affected station, not from inside the player's head.
2. **Something is there.** A crooked keeper resolves out of low ground mist. The coat hangs unevenly; pale fingers extend below the sleeves; the head slowly lifts inside a collapsed hood. Water runs off it. It does not teleport into reach or move the camera.
3. **The building holds its breath.** An original bowed-metal/bass score replaces the normal music using the same two decks. A short low impact introduces it; irregular breath and wet throat resonance locate the body. No tinkling bells, shouted words or repetitive scream loop.
4. **The light wins.** Existing repairs/light force retreat. The form breaks into drifting mist and a water-drain exhale; the regular score returns with a soft crossfade. There is a real recovery moment.

## Rules and implementation boundaries

- Consume the existing host-owned Drowned phase, fault, time and watch revision. No new gameplay/snapshot fields, no new fault or encounter scheduler. Network remains LK-S5-DROWNED2/v29.
- Preserve Night 1, the first minute, first-control grace, sighting limits, and the 1.5 m face-pop exclusion.
- One event source and one spatial breathing source; reuse the adaptive music decks. No overlapping event stack. Late joins seek the loop and skip old stings; retry clears receipts. The local Sightings Off setting removes all this presentation, including the special score.
- Voice chat and harbor narration remain on their own higher-priority buses. Entity effects stay on Critical SFX; the score obeys the Music slider and existing speech ducking. The existing output ceiling remains unchanged.
- No strobe, camera movement, vignette, full-screen effect, new realtime light, reflection camera or depth copy. Route/station lights remain readable. Do not change Will's sky or avatar.
- One body mesh/material, one small mist/drip mesh/material. Target under 3,000 body triangles and under 250 effect triangles. All motion is shader deformation, not per-frame mesh rebuilding or simulated cloth.
- Original deterministic synthesized audio only. Preserve the teammate's recordings/music. Record duration, loudness, peak and loop discontinuity; export an offline encounter preview. Editor stays muted: numeric analysis and a preview are not human listening acceptance.

## Work and acceptance

1. Record the baseline, create the sound palette and deterministic cue envelopes, wire music/ducking without changing gameplay. Test duplicate receipts, late join, retry and Off.
2. Sculpt the keeper, wet surface, subtle head lift, mist and drips. Review front/side/scene screenshots; enforce geometry and renderer budgets.
3. Focused checks, solo Night 3, live two-Mac shared appearance/retreat, APK build and packaging verification. Install only if ADB returns; never launch without a request. Quest GPU, stereo, startling intensity and audible mix remain device checks.

## Visual review

The final keeper is 2,131 triangles, one opaque/coverage material with probe-based wet sheen. A separate 68-triangle mesh supplies low mist, coat runoff and cold breath. The head rises 7.5 cm slowly; no rig/player camera motion. Screenshot review rejected the round-mouth mask and replaced it with a gaunt face mostly recessed in the hood. No colliders or new lights. Graphics Night2 verifies Off, cue routing, voice attenuation and power-driven retreat. Device cost and listening remain pending.
