# Record the harbor master

1. Read a line from [the script](../../Docs/Design/RadioLines.md).
2. Save the recording here with its exact name, such as `keeper_arrival.m4a`.
3. In Unity, choose **Lighthouse Keepers → Import Radio Recordings**.

Use .m4a, .wav or .mp3. Only one file per line name. Record plain speech: the game adds radio effects. Raw recordings stay on your computer; only the processed WAVs in Assets/Audio/Radio are committed.

The importer needs FFmpeg. On this Mac: `brew install ffmpeg`. On another computer, put FFmpeg on PATH or set the `LK_FFMPEG` environment variable to its executable path. Existing voice clips keep their Unity references.
