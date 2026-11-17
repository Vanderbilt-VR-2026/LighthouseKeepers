# Caption-only narration

User requested no AI speech until the final recording pass. All harbor narration is temporarily text-only, including legacy fallbacks, so no placeholder voice can leak through. Audio assets/GUIDs are retained. `HarborRadioLines.captionsOnly` is the single switch for the later recording pass.

Full words appear immediately; long lines paginate across two lines, with 2.6 words/second, 4–18 seconds reading time, then hold/fade. B recall and replay work. Essential text remains visible even if the old speech-subtitle preference was off. The host still chooses cue/age; priority, stale-queue checks and rate limits remain. Caption-only playback never starts PA speech/static or audio ducking; voice chat, rain, machines and music stay intact.

30 focused EditMode tests passed. Actual muted Play Mode observed arrival and grace captions, replay, and zero playing narration sources over 24 seconds. This is not a human headset readability test; no new co-op claim yet. No replicated fields changed for this step.
