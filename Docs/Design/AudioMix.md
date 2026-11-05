# Audio mix

## Sources

The team supplied **Rain Heavy Loud**, **Rain Heavy Quiet Interior**, and **Rain On Rooftop** from the YouTube Audio Library sound-effects tab. Their source screenshot is saved locally in `artifacts/audio-pass/rain-source-screenshot.png`. The team reports free use; the screenshot does not display licence text. They are not labelled Creative Commons or CC0 here. Exact off-platform licence terms remain unverified. [YouTube's Audio Library help](https://support.google.com/youtube/answer/3376882?hl=en) distinguishes standard Audio Library terms from Creative Commons entries.

Voice: original teammate recordings, one selected take per line. Music and stings: team-supplied Google Gemini/Lyria generation, documented in [Music.md](Music.md). Window taps, metal patter, puddle splashes, drips and gust wind are original procedural synthesis (`Tools/S4/process_rain.py`); no third-party samples. Earlier S4 original synthesized station sounds remain in use.

## Rain layers

The three long beds are mono, Vorbis streaming, normalized to a common −18 LUFS reference before mixing, with one-second overlap loop seams. Decoded measurements: exterior −18.10 LUFS / −0.40 dBTP; interior −18.18 / −0.66; roof −18.23 / −0.28. Codec overshoot is below full scale; source gains and the master ceiling govern final output. Originals remain untouched outside Assets.

Head position samples authored House, tower, roof and open-doorway trigger regions; transitions take about 1.5 seconds. The current doors are open routes, with partial exterior leakage at the doorway. There is no new door mechanic. House/tower interior bed gain .45, outside .65, roof .55. Bed filters: interior 2.8 kHz, roof 9 kHz, exterior 16 kHz. Rain volume is independent of flood rendering.

Four pooled spatial impact emitters: outdoors 2–5 events/second, indoors 1–2.5; window taps at nearby wall openings, F4 metal patter, exterior puddles and interior ceiling drips. Pitch .85–1.15, gain .12–.23, linear rolloff 1–8 m. Short impacts use RMS/peak levelling because integrated LUFS is undefined for <400 ms sounds. Gusts every 15–40 seconds, 5–9-second smooth envelopes, muffled indoors (1.2 kHz vs 7 kHz open air). Thunder follows the comfort-safe lightning envelope with a 1.2-second propagation delay, approximately 400 m. The old static rain beds and duplicate shell thunder are stopped while the new soundscape is enabled; rain particles and flood visuals are retained.

## Controls and verification

`AudioPassSettings` exposes separate music and layered-rain switches. Software checks are recorded in `Docs/Verification/AudioPass/README.md`. Headset mix acceptance, final co-op smoke test, master capture and device CPU cost are pending the final build. Audio sources' loudness measurements are not measurements of the complete headset mix.
