# E1 — connected machine

The accepted electric pump already requires its powered circuit to drain repaired flooding, and the beacon already requires ignition/alignment plus power. Those rules are retained. The radio can prepare its dial/code, but a report cannot transmit while the actual beacon is dark. Turning the beacon off pauses transmission without deleting the tuned frequency or accepted code; restoring it permits sending. The Objective Director guides beacon recovery before a radio report.

Physical feedback: a wall-mounted riser carries a bottom-to-top amber sequence on generator restoration; a pipe pressure needle reacts to pump power; a beacon-proof lamp and mechanical flag on the radio console wake when the beacon is shining and the radio circuit is live. No realtime lights are added.

All consumers read the existing host state. The dependency/presentation switch is ConnectedTasksTuning.dependencies. No snapshot fields change in E1. Subsequent pipe/handheld work will bump the network version.
