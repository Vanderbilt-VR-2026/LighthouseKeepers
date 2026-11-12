# C5 stations, glass, story and sound

Station materials reuse the CC0 worn-metal palette: generator yellow enamel, iron, brass, radio cabinet and fittings. The physical controls, grab points, shapes and networking are unchanged. The beacon's existing Fresnel rings now use transparent reflection-probe glass with a separate amber core. Window panes carry subtle scrolling rain streaks. No skybox or avatar-body assets were edited.

The House now contains an open keeper logbook, oilskin coat/hook, old photograph, tally scratches, crate/rope, dropped lantern and kettle/stove. All are nonblocking decoration; the runtime audit checks the prop roots have no enabled solid colliders. These are low-poly first-pass models, not final bespoke assets.

The lighting review found that local hand meshes opted out of probes, which made them dark after replacing realtime fixtures. Local hands now use the new baked probes plus a small tan visibility floor (8% emission). This changes illumination only: no material base colour, hand orientation, finger pose or grip transforms changed. The reviewed close-up shows tan hands again. Late-created Editor hands are included by the existing five-second fixture scan; Quest hands exist at scene start.

Sound audit and source details: `Docs/Design/WorldAudioGaps.md`. Four reusable cue sources and one quiet seep loop; existing footsteps/station sounds remain. New original cues cover door movement, chalk, bunk settling, pole engagement and water entering the doorway.

The first final Night 1 recording was contaminated by four errors from my diagnostic query asking the pointer-highlight shader for an unsupported emission property. Its actual repair/sleep sequence finished. The clean rerun does not issue that query; see `night1.json`. This was an inspection-script error, not a gameplay workaround.

Reviewed powered/blackout rooms, glass on/off, board/bench and tan hand with turned bolt. GPU/CPU, shader appearance and hand brightness in the headset remain DEVICE PENDING.
