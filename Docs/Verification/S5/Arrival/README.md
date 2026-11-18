# In-world arrival briefing — November 17, 2026

The three-page duty folder opens in the House after START, never in the lobby. Story and responsibilities have newly generated wartime illustrations; controls retain a native diagram. All text is live SDF text. Skip/Finish remembers the choice locally; Watch Menu reopens it. Initial arrival holds host-owned watch/flood/start-grace timers until both current humans finish or skip. A disconnect cannot strand the remaining reader; an already-started watch never pauses for a late join or optional reread.

Network: **LK-S5-ARRIVAL1 / LK-Lab-Coop-v42**, both clients required. One replicated briefing hold and one authenticated completion command were added. Personal page/seen state stays local.

## Executed

- 62 distinct focused EditMode checks passed: arrival 8, existing briefing 2, guidance 8, authentication 9, rescue 12, voice 23. The `BriefingTests` filter also matches the seven arrival tests that existed at its run; do not double-count those. The final arrival run requires eight cases to reject stale assemblies.
- Actual offline Night 1 arrival: lobby clear, folder in House, 90-second simulated reading consumes neither watch clock nor start grace; all three pages, Finish, reread without pausing, Skip. No text overflow. Three final screenshots visually inspected.
- Actual two-Editor Night 3 fixture: real host/join; independently reading pages; host Skip waits for guest; both acknowledgements release clocks; guest direct-grip challenge produces identical replies; far hand rejected; leave/rejoin retains code/decision/duty without re-pausing; wrong decision recoverable; real accepted and fake rejected on both clients.
- Editors muted. No project audio settings changed. Initial screenshot exposed paper drawing above SDF text; corrected material render order, recaptured and inspected. First pair attempt hit Fusion Editor recompile-hook/native Mono failure during an Editor reload. Fresh pair passed. A test-helper name shadow interrupted the later receipt after rejoin; only the remaining assertions were resumed, not gameplay changed. Recovery produced no scene file diffs. Automatically serialized wood color alias and XRI ray width were archived and restored separately.

## Limits

This is an arrival/radio fixture, not a complete three-night playthrough, human voice test, worn readability or Quest performance claim. Day 3's bunk scene/hiding chase and Night 1's full-station tutorial are still unfinished. No AI narration. See the delivery checklist for actual playtest scope.

## Reproduction

Run `python3 artifacts/s5/arrival/check.py ArrivalBriefingTests BriefingTests PlaytestGuidanceTests RadioAuthenticityTests CrewRecoveryTests VoicePassTests`, then `solo.py`, then `crew.py` with the second project synced and both Editors compiled/muted before Play. Use fresh receipt directories. Preserve peer overlays. Generated-art prompts and paths: `Docs/Design/BriefingArt.md`.
