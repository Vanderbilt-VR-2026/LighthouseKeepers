# Fresh two-Mac regression — November 12

Source runtime: quality commit 6c13885, with editor-only menu/test and documentation follow-ups. Both clients LK-M3-CAMPAIGN2 / v22. Main-host/guest Editors were muted for the entire test; microphones/mono/AEC configuration and transport were inspected, not human intelligibility.

Passed: online host starts alone without READY; guest late joins; both remote avatars visible (screenshots inspected); native mono input, AEC and noise suppression active; guest sea recovery owned by host; guest pipe repairs shared; cross-floor proximity attenuation; host trigger radio and guest near-head radio; mute stops radio transmission; shared bed into Night 2; guest leaves and rules adapt; rejoin into the same night.

Separate Night 3 run: two-keeper valve policy active, host holds the real XRI valve, guest uses real XRI wrench to repair all bolts, both boards receive completion and valve release replicates. Both board screenshots inspected. No gameplay changes were made to get these passes.

The first scripted pipe attempt instantly teleported the return stroke and stalled over transport. A gradual 0.4-second return, keeping the same 20-degree pull, passed. A subsequent valve run was interrupted by Mac sleep/network loss and is excluded; the fresh run passed. Raw failed/interrupted runs remain under artifacts/s5/quality/ (two-mac-initial and two-mac-valve-interrupted).

Evidence: result.json/events.jsonl for the Night 1–2 crew run; valve-result.json/valve-events.jsonl for Night 3. This is live two-process transport evidence, not a two-headset comfort or performance result.
