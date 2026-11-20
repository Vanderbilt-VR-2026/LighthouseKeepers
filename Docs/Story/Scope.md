> Latest user instruction: no AI narration while mechanics are developed. `HarborRadioLines.captionsOnly` is ON: selected cues are shown as readable text, without PA speech/static, including legacy fallback voices. Keep files/GUIDs for the final recording pass. Voice availability and trigger rules below still govern which text may appear; no optional ID is enabled merely because playback is silent.

# Felt story / horror loop — November17 scope

This user-approved trim overrides the expansive bible's production list. Keep Rule14, Lucy Gale's letter, Walter's engine log and Tom's unsent letter. No additional readable-note interaction or15-paper expansion. The later co-op focus restores a minimal scripted second keeper to Sprint 5; the elaborate Owen narrative remains stretch. See `Docs/Design/CoopCoreLoop.md`. Until that partner is physically present and functional, companion-dependent lines stay inactive. The target uses the same light/hiding/defense and witnessed-safety rules for human and bot crews; none of the new catches are enabled yet.

Six visual clues (Tier2, after the whole Tier1 loop): wet prints from sabotage to exit; Walter/Tom photo above F3bunk with shoulder patch; matching dripping oilskin on F2hook fromN3; lantern-readable NEVER ALONE IN THE DARK aboveF2door; tank/lens black wet handprints; doll onF3bunk thenHousebench thenstairbottom. Preserve world-space placement and room paths.

## Voice availability

`03-Recording-Sheets.md` is copied verbatim from the user's updated source. Its SHEET0 contains52 IDs (45HM/7ANT). All get one offlineTTS take. Remaining60 stay catalogued but unavailable. `StoryVoiceAvailability.json` records selected IDs and successful real imports. The existing import menu auto-enables only the imported matching ID, after conversion succeeds; it preserves filenames/GUIDs and exact captions. TTS never marks a line as recorded. Generation refuses to overwrite unrecognized audio or a real recording. `Tools/Story/build_catalog.py` no longer overwrites the recording sheet.

Availability is not trigger eligibility. Do not play cues for a mechanic which is not yet implemented. Current legacy performances remain the accurate fallback for existing mechanics, not a second simultaneous voice. ANT's selected files are ready for the Tier2 private adapter; they are never broadcast through PA. Existing priority, staleness, prerequisite, breathing-room and voice-chat rules remain.

## Recording-sheet conflicts — held, not silently rewritten

- `HM_N1_ARRIVAL_SOLO` says “You and Teague got across”; `HM_TASKS_DONE_SOLO` says “Teague will take the first hour.” Placeholders are generated exactly, but no solo trigger uses them while no functional companion exists. Existing accurate solo arrival/sleep lines remain. Future replacement: “You got across. Good.” / “Get some sleep. Bunk's on Three.” Requires an agreed recording/caption change.
- `HM_SHIP_SIGHTED` says south; current ship path is not south. Keep accurate existing ship-sighted cue until path/script agrees.
- The default confession starts at selected `HM_N5_TRUTH_2` after a safe ship; optional TRUTH1 is only a prerequisite if it is enabled. This avoids deadlocking the selected story on an inactive optional line.
- Missing hunt/hiding/cable/radio-wire triggers stay inactive until their systems exist. Selected does not mean all52 play in today's build.

TTS is temporary and intentionally identified in `placeholder-manifest.json`; it is not the final actor performance. All clips mono44.1kHz16bit, under10s, same importer chain; HM−18LUFS, ANT−24LUFS, peaks≤−1.2dBTP. Word reveal uses the existing even-timing fallback.
