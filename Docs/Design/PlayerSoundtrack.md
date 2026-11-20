# Main soundtrack and five-second danger accent — November 17, 2026

The user-provided `Lighthouse keepers soundtrack.mp3` is now the main bed throughout the watch and at lower volume in the lobby. It continues through music modes, apparitions, possession and night transitions. It keeps the composition's quiet passages; the old timed storm-only rests do not mute this score. Earlier scores remain a fallback behind `AudioPassSettings.providedSoundtrack`.

The other provided demo sample contributes **only 01:11–01:16** as the new five-second musical accent. It overlays the bed for a safely visible Drowned reveal, the Night 2 Unknown threat, power failure, a non-preview ship entering danger, entry into crisis, or a loss. Events use existing host state and encounter receipts. A local cooldown of eight seconds, no queue, no overlapping/restarting accents and a 1.25-second freshness limit prevent stacked or late stingers. No new replicated fields or gameplay changes.

Two existing music decks are reused: one streaming stereo bed, one compressed stereo accent. No extra music voices or realtime lights/cameras. The old spatial demo music hit is removed when this score is selected; physical breathing/footsteps/reveal sounds remain.

| Clip | Source | Runtime length | LUFS-I after Vorbis | True peak |
|---|---|---:|---:|---:|
| main-loop | supplied track, 0.189–400.782 s, four-second equal-power circular overlap | 396.593 s | -20.00 | -3.80 dBTP |
| intense-hit | earlier sample, exactly 71.000–76.000 s, 10 ms attack / 350 ms tail | 5.000 s | -18.02 | -2.35 dBTP |

No speed or pitch changes, no claimed beat alignment between unrelated tracks. The bed's circular overlap joins adjacent source samples at the buffer boundary; processing precedes normalization. Raw sources remain untouched outside Assets. Originals are archived in `Docs/Audio/Sources/`; imported derivatives are in `Assets/Audio/Resources/PlayerScore/`. Credits: user-provided demo soundtrack and previously supplied demo sample; no new authorship/licence claims.

Engine levels: main .32; accent .55; lobby .025; existing Music bus -2 dB plus per-player slider. During an accent the bed gently moves to 85% gain (75 ms attack / 600 ms release) rather than stopping. Voice/radio retains the existing -7 dB music duck and master ceiling. Post-encode source measurements above are not measured headset output-bus levels. Worn balance, seam audibility and danger timing still require listening acceptance.

Rebuild with `artifacts/s5/finish/music/env/bin/python Tools/Audio/import_player_soundtrack.py` (or a Python with numpy and ffmpeg). The importer writes `artifacts/s5/soundtrack/analysis.json` with input/output SHA-256 and measurements.
