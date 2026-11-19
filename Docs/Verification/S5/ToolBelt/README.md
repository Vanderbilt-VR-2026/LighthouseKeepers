# Dedicated tool belt and Day 1 guidance — #147

In both modes, keep the walkie on the right hip, flashlight at the left front and wrench at the left side. Release within 18 cm of a clip; grip to retrieve. Each type has a separate slot, click feedback and pickup hint. A second flashlight cannot occupy its slot. The host validates carrier, slot and occupancy; carried tools do not count as active hand-held station work. Existing tool hand poses and lighting are preserved.

Day 1 station writing follows **your** objective. Pipe: only active bolt or your pressure-valve instruction. Generator: fuel, timed starter or partner assist. Radio: power, tuning, all four colour buttons together, then microphone. Beacon: ignition then aiming wheel. The power panel shows the current circuit; inactive flashlight-hook instructions also hide. Actual controls pulse; partner controls and finished-control glow do not distract. Later nights restore the existing presentation. World-writing mipmaps obey the same visibility filter; the change does not turn off controls/colliders or alter gameplay state.

Network: **LK-S5-TOOLBELT1 / LK-Lab-Coop-v52**. Both players must update. No new packet fields; carry semantics now include the two extra tool types.

## Verification

- 12 final focused EditMode tests pass (5 StationGuidance, 4 ToolBelt, 3 RadioBelt). Earlier voice/network-version checks: 23 VoicePass + 5 StoryImport pass.
- Director regression: 10/12 pass. Two pre-existing assertions fail: HelpPartner has no fixed location; HoldValve is seven words versus the older six-word limit. The director code, tests and every existing string value match pre-task `8b15f0f3`; see `director-baseline-proof.json`. No full-suite claim.
- Muted Editor, actual XRI input: **19/19** checks pass, including three tools clipped simultaneously, second flashlight rejection, regrip and ordinary drop.
- Rendered Day 1 exercise: **34/34** checks pass across 15 local instructions, with a different partner instruction, plus restoration on Night 2. This fixture isolates presentation; it is not a full-night win.
- Live Photon session in two muted Editors: host/join Survival Day 1; guest selects, clips, regrips and drops flashlight, wrench and walkie. Both peers agree on owner/pose; host does not treat carried tools as working hands. The radio fixture was corrected to find tools globally: XRI unparents grabbed tools, so child traversal could inspect the other radio. Production radio occupancy now handles that same unparenting.
- Inspected front/side belt captures and generator/radio/beacon captures. Belt photos use a test-only inspection light and do not establish body clearance. Station captures position the existing flashlight for inspection; no project lighting changes.

## Not verified

Worn reach/body clearance/comfort, both-eye text readability, new full-night runs, disconnect/rejoin with this belt, voice/sleep regression and target GPU cost. #147 remains open with needs-headset-check. Build and install receipt follows; never auto-launch the Quest.

## Headset check

1. Start either mode, Day 1. Pick up the walkie, flashlight and wrench.
2. Release walkie at right hip, flashlight at left front, wrench at left side. They should click into separate clips.
3. Turn, crouch and walk with all three equipped; grip each back. Report the tool, hand and position if it clips or grabs the wrong object.
4. Follow Day 1 objectives: only the current station instruction/control should be shown/glowing. Fuel should not advertise the starter or assist; radio power should not advertise code buttons yet.
5. In co-op, compare your assigned roles and watch the other keeper clip/regrip tools. On Day 2, normal station labels return.

Raw runners are copied beside this report; commands use the existing Pipeline bridge. Do not run Editor tests and Play Mode fixtures concurrently. Build/delivery: `python3 artifacts/s5/tool-belt/build.py`, then `python3 artifacts/s5/tool-belt/deliver.py` (fresh backup directory per build).
