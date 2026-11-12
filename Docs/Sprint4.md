# Sprint 4 — playable co-op lighthouse

[Download the game](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint4-final) · [Review #88](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/pull/88) · [Milestone](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/milestone/2). Video: coming soon.

## What we built

Two keepers can share the same lighthouse, see each other, talk nearby or use handheld radios across floors, share tasks, sleep together and rejoin. One keeper can play alone, including through the offline fallback. Three nights combine wrench repair, the physical generator, a separate power panel, the beacon and a radio broadcast.

The harbor master uses recorded speech with timed captions. B recalls the current objective and last line; the left wristwatch shows time and the objective. The world has practical lighting, restored power wiring, storm/rain zones, water and Eli’s exterior, with Will’s keeper bodies.

## What playtests changed

- One PLAY button leads to hosting alone/with a friend or joining; removed the READY step.
- Wrench uses grip and auto-snap; short arm pulls tighten bolts. No solo valve clamp. Released tools fall and recover if lost.
- The generator really runs out of fuel, sputters and cuts power. Refilling, grip-squeezing the primer, moving the choke and pulling the starter are physical actions.
- The yellow generator and dark-green POWER PANEL are clearly separated; guidance names the actual object and place.
- Objectives are transient and recallable; narration is paced, subtitles readable, the House board white with dark writing.
- Pole access, island boundaries, sea recovery and clutter received fixes. Dropping into the sea loses a solo night; a crew member returns after ten seconds while the partner continues.

## Known issues and evidence

Full EditMode: 372 pass. Full PlayMode initially 52 pass/31 fail; six corrected cases pass rerun, leaving **25 inherited failures**. The failure list is retained; this is not an all-green suite.

Actual-XRI graphics Nights 1–3 and live two-Mac host/join/avatar/valve/radios/mute/shared-sleep/leave/rejoin passed. Both Editors were muted; human speech intelligibility was not re-accepted in that run. [Results and screenshots](Verification/RemainingPlan/Step1/README.md).

Still needs worn-headset verification: reported missing lobby hands/own-body obstruction, actual Wi-Fi-off start on this exact release, physical reach/watch readability, current-build human voice listening and Quest GPU/CPU. Wiring geometry is 13,894 triangles/five meshes/one material; its ≤0.3 ms target is unmeasured. Do not infer device performance from Editor tests. These checks carry into Sprint 5.

**Compatibility:** both players need LK-S5-PLAYTEST1 / snapshot v21. The release is a standalone Android ARM64 IL2CPP APK, package com.lighthousekeepers.game. Sprint 5’s different network version cannot join this build.

Credits: Eli Gripenstraw’s playtest/exterior/shell work; William Qian’s networking, avatar and water; Tapan’s reused interactions; and the team’s art, systems and supplied voice recordings. Sprint 5 ships/horror/lantern mechanics are not claimed in this release.
