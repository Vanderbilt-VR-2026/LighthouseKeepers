# Shared hand rescue — November 17

Refs #107, #135. Network **LK-S5-RESCUE1 / LK-Lab-Coop-v39**; both clients update.

Host owns two stable duty slots, the 40-second deadline, accumulated three-second grip, and ten-second recovery grace. Fresh nearby hands and direct grip are required. Release pauses progress. Local fade moves the rig to the F1 recovery spot while preserving tracking; no forced headset animation. The offline partner walks to the real hand and uses the same hold rule, restoring its previous order afterward.

Eight recovery model tests and related focused regressions pass. Live muted two-Editor checks: both rescue directions, distant-grip rejection, release/regrip, guest leave while host down, bot takeover, rejoin and matching recovery state. Actual bot F4→F1 travel plus hand hold took **22.36 seconds**, leaving about 17.6 seconds. Screenshots inspected. These are automated Editor fixtures, not worn headset/voice/performance acceptance.

Only the development Watch Menu rescue drill activates this state. Hunter catches remain OFF. The new requested possession face/lift and Night 2 teaching event are separate pending work (#139/#140).

Reproduce: muted Editor bridge, `python3 artifacts/s5/rescue/check.py CrewRecoveryTests`; `solo.py` and `crew-final.py` use the existing lobby and actual navigation/direct interactions. Choose fresh artifact destinations for reruns. Host-departure migration, actual hand reach and seated acceptance remain pending.
