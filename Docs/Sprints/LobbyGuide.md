# Crew lobby guide

Applies to the **current main** Photon lobby and its two-player Storm Watch flow. Sprint 2's isolated environment demo has a separate entry described in its [README](Sprint2/README.md). This candidate adds a numeric room keypad using existing ray buttons; headset acceptance remains open in [#59](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/59); the UI itself still needs the guidance improvements in [#6](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/issues/6).

## Host and guest sequence

1. Both clients use the same compatible build and internet connection; use the same Photon region.
2. Host: open **Crew options**, use digits plus **BACK/CLEAR** to set a numeric room code (or retain the shared default room), and leave the optional keeper name at its default, then select **Host crew** on the main board. Record the room name exactly.
3. Guest: open **Crew options**, use the keypad to enter that exact room code, including leading zeroes, then select **Join crew** inside the options panel.
4. Wait until **both names** appear on both boards. Connecting is not the same as being in the crew.
5. Each keeper selects **Signal ready**. Both rows should read **READY**.
6. The **host** selects **Begin watch**. The guest waits for the host. Both environments load before Day 1 begins.

The production game cannot launch with only one keeper. To review alone, use the explicitly labeled Unity rehearsal/preview routes in the [root README](../../README.md); an APK solo entry is not currently promised.

## What every button does

| Control | Function / when to use it |
| --- | --- |
| Crew options | Opens/closes keeper-name and room-name setup. It does not connect by itself. |
| Host crew | Creates the crew session using the room name; only one keeper should host. |
| Digits / BACK / CLEAR | Controller-operated room entry. The first digit replaces a nonnumeric default; leading zeroes are retained. Host/Join confirms the displayed code. |
| Join crew | Connects the second keeper to that existing room; it is inside Crew options. |
| Signal ready | Marks your keeper ready after joining the crew. Both must do this. |
| Stand down | The same ready button after readiness; cancels your ready state. |
| Begin watch | Host-only launch, enabled once two keepers are present and both ready. |
| Lower board / Raise board | Adjusts the board for seated/standing reach; it does not move the viewpoint. |
| Cancel connection | Cancels a connection attempt while it is in progress. |
| Leave watch | Leaves the lobby crew. If the host leaves, that crew session ends. |
| Return to lobby | On the gameplay floor/result boards, ends the current session and returns to a fresh lobby. Reconnect and ready again for a new crew. |
| Take your post / Simulate a guest | Development-only local lobby preview controls. The guest is simulated and does not establish device networking. |

During gameplay, a terminal retry keeps the crew and resets the run. A partner disconnect ends the session and returns the remaining keeper to a fresh lobby with an explanation; host migration and late-join gameplay recovery are unsupported.

## If you cannot proceed

| Symptom | Check / expected response |
| --- | --- |
| Room text needs a computer keyboard | The old APK has blocker #59. In this source, use the numeric keypad and default name; record any controller failure, since Editor typing does not prove Quest usability. |
| APK opens as a flat 2D window | Known blocker #60. Record APK name/hash, headset/OS and launch steps. Immersive startup has not passed. |
| Begin watch disabled | Confirm host role, two names, both READY, and that connection is no longer busy. |
| Guest cannot start | Expected: the host starts the watch. |
| Room not found / join fails | Check spelling, internet, compatible builds and Photon region; cancel/leave and retry. |
| Reviewing without a second person | Use the documented local Unity rehearsal or arrange a second compatible client; see #63. |
| Different scene than expected | Check the sprint's branch/ref, entry scene and APK source. The environment demo and production Storm Watch are distinct. |

For acceptance, a fresh reviewer must follow these steps without developer coaching on the exact submitted artifact. Record the result in #31 and the corresponding sprint README.
