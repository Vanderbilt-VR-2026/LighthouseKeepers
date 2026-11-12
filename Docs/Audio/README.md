# Harbor master recordings

`recording-script.md` is the supplied 94-line number → game ID → path → caption contract. `voice-lines.md` is the supplied context (trigger updates are appended, not silently substituted for the source script). Originals are read-only; the MP3 keeper voice was supplied after ElevenLabs speech-to-speech conversion. No recording is uploaded for transcription.

Processing uses Python, OpenAI Whisper (small.en and targeted medium.en), NumPy/SciPy and FFmpeg. Model weights download once; transcription itself is offline. Intermediate decoded audio, cuts, and alternate takes stay in gitignored `Recordings/Radio/voice-pass/`; diagnostic transcripts stay in ignored `artifacts/voice-pass/`. Only selected processed WAVs, captions, word timing and reports enter the game/repository.

The numbered slates and “again” retakes are removed before processing. `Tools/VoicePass/cut_overrides.json` records ambiguous ASR slate corrections and last-take selection. `voice-manifest.json` records exact boundaries, expected/recognized words, timing and flags. A flag is not proof of a misread: listen before requesting a re-record. An overlong complete recording is retained and reported, never accelerated or cut mid-sentence.

## Reproduce splitting

From the repository root, using a virtual environment with `openai-whisper`, `torch`, `numpy`, `scipy` and `certifi` installed, plus FFmpeg on PATH:

```sh
python Tools/VoicePass/prepare_sources.py "/path/to/Lighthouse Keepers voice lines aneesh"
python Tools/VoicePass/transcribe_memos.py
python Tools/VoicePass/refine_transcripts.py
python Tools/VoicePass/split_memos.py
python Tools/VoicePass/check_cuts.py
python Tools/VoicePass/refine_clips.py
python Tools/VoicePass/report_split.py
```

The model downloads are not uploads of audio. Both transcription scripts reuse local cached weights. All source hashes, cut boundaries and durations are recorded for review.
