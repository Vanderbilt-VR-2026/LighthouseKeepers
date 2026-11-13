# The keeper's own heartbeat

Sprint 5 step 1.3 / issue #93. The host observes both keepers through the existing pose stream. Darkness, flood depth on the keeper's floor and distance from their partner feed a separate fear value for each actor. Going upstairs does not inherit the lower floor's water threat; sitting does not invent extra water depth. The director reads the maximum host fear when choosing overall tension and the music target; local feedback still uses only the correct actor. Values rise smoothly over about 1.2 seconds and recover over about 6 seconds. Guests apply the host's values and cannot author fear.

The same model accepts ship danger and a host threat interface (distance, visibility, hiding and breath hold). **Ships, the Drowned and hiding have not been implemented yet:** those inputs are neutral in normal play. Their model tests use explicit synthetic evidence, not a claim that encounters exist.

| Feedback | At fear 0 | At fear 1 |
| --- | --- | --- |
| Heart rate | 62 bpm | 140 bpm |
| Haptic first pulse | 0.03 for 0.04 s | 0.18 for 0.04 s |
| Haptic second pulse | 72% strength, 130 ms later | Same |
| Heartbeat source gain | 0.045 | 0.16 |
| Breath source gain / pitch | 0.07 / 1.0 | 0.20 / 2.2 |
| World filter | Exact bypass through fear 0.6 | Gentle cascaded low-pass toward 6 kHz |
| Hunted undertone | Silent | Original low drone at 0.16 source gain; Music bus/ducking |

Heartbeat and breath are original deterministic mono synthesis, 2D and local to the keeper. The two heartbeat pulses use the same source; the additional breath and hunted sources total at most three additional voices. The existing 24-real-voice ceiling remains. The DSP filter has fixed channel buffers and no audio-thread allocations. Actual Quest CPU/audio cost is pending; code structure is not a timing measurement.

The world filter applies only to eligible SFX_World and Ambience sources. The legacy camera UI emitter and any object sharing multiple audio endpoints are deliberately excluded: Unity forbids custom source DSP on those objects. It preserves the existing spatial/room filters and does not touch VoiceChat, Radio or SFX_Critical (the future Drowned's route). The existing master ceiling remains. Blackout fog uses tension/local fear; no camera transform, FOV, vignette or avatar change.

Watch Menu → Sound → **Heartbeat haptics: On / Off** is saved per device and defaults on. Haptics pause during menus, sleep and loss. Breath hold only silences breathing when the host confirms hiding; the actual hiding/input integration belongs to the later Drowned phase. The old loss-only heartbeat is suppressed when this system is active.

`Sprint5Tuning.fearFeedback` switches the system off independently. New snapshot: **LK-S5-FEAR1 / LK-Lab-Coop-v24**. Both players must use the same build. Headset pulse comfort, subjective breath quality, fear-filter intelligibility and performance are NOT VERIFIED while the Quest is absent.

Regression caught during development: the first PlayMode run attached custom DSP to the camera's combined AudioSource/AudioListener and Unity rejected it. `CanAttach` now excludes shared endpoints; a dedicated regression covers both a camera listener and multiple sources. The initial failed run remains in artifacts/s5/fear; later results must pass before this step is delivered.

The graphics regression completed Nights 1–3 and woke on Night 4. Its appended probe initially selected every clip named “Original …”, including spatial station effects; the narrower probe checks only the three named fear emitters and passes. This was a diagnostic selection error, not a request to turn station sounds into 2D audio. Screenshot review also caught the new toggle below the logbook backing; the Sound page is now reflowed inside its existing 800-unit bounds.

Live co-op caught a separate launch race: Fusion's `SessionInfo.IsValid` briefly turns false while closing the lobby for scene loading, even though the transport remains connected. Photon Voice then tried `OpLeaveRoom` on MasterServer and stranded itself. CrewVoiceSession now derives its leader/room identity from the live channel's stable session name; initial connect still waits for valid session metadata. No package-cache edits or new network fields for this fix. A fresh fast-start/late-join test must prove both Voice clients Joined and nonempty remote speakers, not just microphone configuration.
