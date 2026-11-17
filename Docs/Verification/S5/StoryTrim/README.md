# Story trim checkpoint — November17

54 focused EditMode tests pass (4scope,5import,22speech/witness,23existingvoice). Editor muted. Script catalogue check passes; supplied recording sheet copied byte-for-byte.52 generated offlineTTS placeholders are mono44.1kHz16bit, individually under10s, exact captions; measured loudness/truepeak in `audio.json`. Runtime blocks optional unrecorded story lines; the real importer enables matching IDs only after successful conversion. ANT never enters PA transport. Stable cue integers and existing GUIDs retained; no replicated-state change, network remains LK-S5-LANTERN1/v35.

Four existing notes retained. No 15-readable expansion. Owen moved to Post-course issue #135. Six visual clues are queued for Tier2 after the playable loop. The exact two Teague solo lines and south compass line are content-ready but trigger-held, documented in `Docs/Story/Scope.md`; no phantom companion/direction instruction shipped.

This checkpoint does not claim live hunter, hiding, private ANT or all 52 triggers complete. Clean **a270bea1** content/foundation APK installed and hash-verified; no Quest launch. Hunter integration remains next. See install.json and Headset.md.

Reproduce: `python3 Tools/Story/build_catalog.py --check`; `python3 Tools/Story/build_placeholders.py --verify`; bridge filters StoryScopeTests, StoryImportTests, StoryRulesTests, VoicePassTests. Explicit conversion reuses the existing import menu. Audio source originals untouched.

Audio follow-up: the first peak-limited normalization left one short line below the target. An oversampled limiter and measured gain correction fixed it; all 52 output measurements now pass (see audio.json). No source recordings altered.
