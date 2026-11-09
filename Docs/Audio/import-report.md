# Voice import — Step 2

94 converted single takes imported; original files unchanged. Existing audio GUIDs retained; 26 P-line assets added. Caption wording matches the supplied script, including cue 20 “That’ll do it.” and contractions. Unrecorded cue IDs remain unchanged.

Mono 44.1 kHz / 16-bit WAV; high-pass 80 Hz, gentle denoise, de-esser, +2.5 dB presence at 2.8 kHz, 2:1 compression and two-pass normalization. Peak-constrained short clips receive gentle final peak limiting to match loudness. Measured range −18.34 to −17.74 LUFS; highest true peak −1.35 dBTP. See processing-report.json for all 94 measurements.

Cuts already retain about 80 ms edge room tone, so processing does not trim their timing again. Timestamp alignment maps script words to original ASR; unmatched words interpolate. Future one-click re-record imports cover every mapped filename and invalidate that clip’s old timestamps so subtitles use the even-reveal fallback.

Unity EditMode: 2/2 focused tests pass (all 94 clips, captions, format, aligned word reveal). Script 1 is a documented 11.79-second single take, retained complete at natural speed. The 22 wording/duration flags in split-report.md remain listening checks, not claims of human-verified errors.
