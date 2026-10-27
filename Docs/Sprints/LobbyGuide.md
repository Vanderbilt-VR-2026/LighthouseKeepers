# Candidate lobby controls and recovery

This guide describes the reconciled One Lighthouse candidate in PR #65. Entry: `LK_NetworkLobby`; **PLAY SOLO** opens the Lab world offline. Current Lab co-op launch is deliberately blocked until shared simulation is implemented.

| Control | Action / condition |
| --- | --- |
| PLAY SOLO | Closes an existing crew connection, then opens the local Lab. No second player or keyboard required. |
| CREW OPTIONS | Opens name/room controls; keeper names are optional defaults. |
| HOST CREW | Creates a Photon diagnostics room with a fresh six-character code. Share the displayed code. |
| Room field / controller keypad | Select the field to open the 32-symbol keypad. Enter the host's six-character code; BACKSPACE edits and CLOSE KEYPAD returns to the board. Ambiguous 0/1/I/O are excluded. |
| JOIN CREW | Joins the entered code. Check the roster before readying. |
| SIGNAL READY / STAND DOWN | Changes only your own ready state while connected. |
| BEGIN WATCH | A ready host can enter alone for debugging. With two keepers, the button explains the co-op restriction and remains disabled; the transport also rejects a bypass call. |
| LOWER / RAISE BOARD | Adjusts the board/frame together for seated or standing use; does not move the tracked camera. |
| LEAVE WATCH / CANCEL CONNECTION | Leaves the diagnostics room or cancels connection. |
| RECONNECT | Retries the remembered room after a connection failure. It does not supply missing shared gameplay. |

## Return and replay

In the Lab, use the left controller menu button (Escape in Editor) for **Watch Menu**. **Return to lobby** closes any surviving crew session before showing a fresh lobby. Failure offers same-day retry; final completion offers a replay from Day 1. Watch progress and tracked camera transforms are separate: the menu never teleports the rig.

## Common failures

- Invalid room: enter all six symbols from the host's board using the alphanumeric keypad, not the older numeric-room instructions.
- Join failure: check code, network, Photon settings and host region; retry/reconnect or use Play Solo offline.
- Two keepers ready but launch blocked: this is intentional. The Lab's day/water/station state is not replicated yet; it must not run two unrelated solo simulations under a co-op label.
- Solo entry missing from an APK: reinstall the exact current candidate and record its SHA. Both build scenes are required; the new build guards reject lobby-only APK creation.
- Flat 2D launch or missing controller tracking: record the exact APK/source, headset/OS and startup evidence in #60. Packaging checks alone do not establish immersive headset acceptance.

The earlier Storm Watch host/join → both ready → shared three-watch run is a separate, historical implementation. It is not what this candidate loads.
