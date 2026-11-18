# Day 3 radio authentication — November 17

Implemented on sprint-5, PR #89. Protocol **LK-S5-RADIO3 / LK-Lab-Coop-v41**; both clients must update.

- 70 focused EditMode checks pass: nine authentication rules, twelve recovery, twenty-three voice pipeline, twenty-two existing M3 runtime, two command-window and two instruction-table checks. Clean compilation verified first.
- Actual offline Play Mode fixture uses direct XRI hand grips. An unchallenged acceptance is refused; a real code matches the House telegram; wrong decisions can be corrected; the false reply differs; neither acceptance nor rejection changes the beacon automatically.
- Live two-Editor fixture: host/join, distant guest hand rejected, guest challenge and decisions replicated, actual leave/rejoin during a challenge retains the code/stage/duty and the helper takes over while alone. Final rejection matches on both clients.
- The first crew run found a missing command-validator allowance. Fixed in `CoopCommandWindow`, then its exhaustive declared-action test and the live crew fixture passed. An Editor bridge timeout required restarting the peer; it was not a game failure.
- Inspected the radio screenshot, found the first layout obscured broadcast controls, and moved the new buttons into the unused service-meter strip. Final screenshots inspected.

These are focused fixtures that restore earlier tasks and schedule a call. They do **not** prove a complete Night 3, worn readability, human voice, GPU cost, or the bunk/hunter sequence. Editors stayed muted. No new spoken recordings/TTS. No Quest launch.

Reproduce from the source worktree: `python3 artifacts/s5/day3/check.py RadioAuthenticityTests CrewRecoveryTests VoicePassTests M3RuntimeTests CoopCommandWindowTests InstructionsStayShortAndTableHasEveryStep EveryActionHasAnObjectLocationAndOneButton`; then `solo-start.py` + `solo-check.py`, and `crew.py` with a matching peer. Use fresh receipt folders for another run.
