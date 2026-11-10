# Class playtest objective recall

PR #85 / s4/integrated. Network LK-S5-VOICE1 remains unchanged: these are local presentation controls.

1. Placement: 1.5 m away, centred, default 13.5 degrees below gaze, smooth follow. Objective changes reveal for five seconds; objective and live subtitle visibility are separate. Height slider in Watch Menu's settings saves -20 to -5 degrees per device. Focused placement/persistence test passes. Headset comfort/readability pending.

Input audit: the Lab rig uses XRI input actions. B has no active Lab action. The unused InputSystem_Actions template has a generic secondary-button Jump binding, but no scene/prefab references it; authored Jump locomotion is disabled. X/Y and wrist recall remain available.

2. B tap/hold reveals immediately, a tap lasts five seconds, and a double tap within 350 ms replays the last line locally (three-second replay debounce). The local radio history retains three cues. The shared host cue/serial and other keeper's audio are not changed. Three focused state/history/placement tests pass; live rendered checks follow step 6.

3. The left watch face now always carries the current local Objective Director instruction below its time readout. Hands and watch mounting pose are unchanged. Compilation and focused tests pass; tiny physical text still needs a worn-headset readability check.

4. X/Y still reveal the objective and request the existing status update; a fresh wrist glance still reveals it. B uses local state and local playback only. No command schema, game state, snapshot or network version changed. Rendered co-op isolation is checked in step 6.

5. The first actual Night 1 objective transition shows “Press B to see your objective” for five seconds, caption only. It is remembered per device and does not repeat on retry. Four focused tests pass, including saved teaching state. Live narration remains independent of this brief reminder.
