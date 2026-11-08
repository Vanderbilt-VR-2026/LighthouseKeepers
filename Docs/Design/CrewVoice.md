# Crew voice and handheld radios

Part E, behind ConnectedTasksTuning proximityVoice and handheldRadios switches. Solo keeps voice off. No avatar/body or microphone capture changes.

Open microphone remains spatial at the remote head. Speech has full gain within 3 m, falls toward 8% at 18 m, and multiplies by 40% for one floor or 7% for two floors. A wall ray reduces it another 45% and low-passes at 1800 Hz. These are tunable approximations, not an acoustic simulation.

Two physical radios dock on F1 and F4. Each keeper takes one. Hold a radio to your head or hold its trigger to transmit; hold your own radio to receive across floors. Host validates the existing first-grab ownership and expires transmit intent after 0.55 s without a heartbeat. Muting the microphone also stops radio transmission. Release returns to proximity voice, and the radio falls; an abandoned radio returns to its dock after 20 s.

The same Photon mono stream is routed once, either from the avatar head or the local receiver. Radio playback uses 350–3500 Hz filtering, gentle crunch, faint noise and an original synthesized squelch. There is no second microphone stream or speaker loopback. Existing native mono capture, echo cancellation and noise suppression remain enabled. Output processing cannot provide a guarantee against two open speakers feeding microphones in the same physical room; use headphones during Mac testing.

Network version LK-S4-CONNECTED2, protocol v17. Both clients must update. Host-owned radio sender IDs and valve state are the new fields. Playback occlusion is local and never writes game state.

Focused gain/expiry tests and actual XRI grip/head/trigger/release checks pass. Captures inspected in Docs/Verification/S4Connected/e3. Final two-Mac transport checks are recorded separately. Human listening, cross-floor intelligibility, physical grip comfort and Quest audio CPU cost remain device checks.
