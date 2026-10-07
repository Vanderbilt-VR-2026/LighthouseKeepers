# Sound-first architecture

`Audio/Mixer/LK_Atmosphere.mixer` contains Master, Weather Exterior, Weather Interior, Thunder, Ocean, Structure, Machinery, Electrical, Water, Radio, Player and UI. Snapshots: Exterior Storm, Interior Normal, Lower Mechanical, Generator Running, Power Failure, Flood Emergency, Lantern Balcony.

AcousticDirector selects room/exterior treatment and smoothly transitions over 1.5 seconds. Authored AudioZone bounds can override floor-based fallback classification. Thunder is globally scheduled, pitch/volume varied, and low-pass filtered indoors (about 1.6kHz) versus outdoors (18kHz). Interior rain is a separately synthesized enclosed timbre, not simply exterior rain turned down. Room reverb zones, spatial attenuation, generator hum, radio interference, water drops, and separately scheduled structural creaks establish floor identity. Flood height can select Flood Emergency. DayEnvironmentResponse modulates machinery/radio and practical-light instability.

StormController schedules flashes and delayed thunder using approximate distance / 343m/s; deterministic seed and inspector preview support development. Every clip named Placeholder_* is an original synthesized placeholder. Replace clips in emitters and thunder arrays with licensed recordings, preserving output groups and attenuation. Procedural thunder is not final cinematic audio; multiple distinct recorded thunder takes are a recommended next task.

Mixer authoring uses an editor-only reflection adapter because Unity has no public mixer creation API. The resulting .mixer is an ordinary editable asset; reflection is not used at runtime. Power Failure and Exterior Storm snapshots are ready for future triggers; no full power game is implemented.

## Licensed ambience bed — September 23

Bootstrap contains one `AmbienceMusic` component and looping AudioSource below the existing AcousticDirector. It routes to the new **Ambience Music** group under Master, with exposed `AmbienceMusicVolume` initially -20 dB. Startup source gain fades over five seconds; an existing thunder-source reference temporarily ducks the bed. Existing twelve environmental groups and seven snapshots retain their routing.

Source: `Assets/free horror ambience 2/ha-pressure-nofx.wav`. Playback derivative: `Assets/_LighthouseKeepers/ThirdPartyVariants/Audio/LK_Pressure_SeamlessLoop.wav`, approximately110.64 seconds with a five-second crossfade. Import uses streamed Vorbis quality0.5 and background loading. This is licensed ambience, while machinery/weather/thunder remain procedural placeholders. Audition content, loop transition and mix in the headset before treating it as final.

## Keeper footsteps

`PlayerFootsteps` on the shared player prefab (and the unpacked DevGym rig) measures completed horizontal rig travel in LateUpdate. A .65 m stride triggers one of four original short boot impacts through the existing Player mixer group, with small pitch/gain variations and no consecutive sample repeats. Playback stays centered for the local keeper. Clips live in `Assets/_LighthouseKeepers/Audio/Footsteps`; their README records provenance and format.

Actual displacement controls cadence, so holding the stick against a wall does not produce footsteps. The component compensates for rotation around an offset headset and excludes headset bob/lean. Idle motion, airborne frames, pauses, long frame gaps and teleport-sized displacements reset the partial stride. It does not change tracking, movement, collision, or networking.

Ground detection includes Default (DevGym) and Environment (tower geometry), ignores triggers, and excludes Water, UI, PlayerBody and Interactable. The previous Starter Assets mask omitted Environment and could classify a standing keeper as airborne. The prefab, DevGym and fallback rig authoring now use the corrected mask.

`GroundedSlopeMovement` adjusts queued walking motion to follow nearby walkable slopes through XRI’s collision pipeline. This prevents repeated loss of grounding and silent footsteps on the descending spiral. The ground probe honors the controller’s slope limit; unsupported falls retain the gravity provider’s behavior.
