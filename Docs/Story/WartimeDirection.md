# Kettle Rock: wartime direction

Approved user steering, 17 November 2026. This supersedes the 1923 setting in the earlier story bible. The location remains fictional; this is not a reconstruction of a real convoy or lighthouse operation. Gameplay, safe recovery and clear teamwork come first. Four existing lore notes stay; no additional readable-note system.

## Arrival briefing (four sentences)

Autumn, 1942. Beyond the black reefs of Kettle Rock, American supply ships carry food, fuel and medicine toward the wartime port of Saltmere. Six weeks ago, both lighthouse keepers vanished, leaving their bunks made and their final log unfinished. You are their replacements: keep the machinery running, guide the ships through the storm, and stay together when something starts moving in the dark.

## The playable story

Saltmere is a fictional British Atlantic port, cut off from its pilot boats by a storm. Kettle Passage is its sheltered approach. The keepers must keep the station from flooding, run the diesel generator, route scarce power, relay the harbor report, and hold the beacon on each approaching ship. The two duties are preferences, not tool locks: Lightkeeper handles ships and signals; Machinist handles the machinery. Each can help, read a clue to the other and rescue the other. When only one human plays, the bounded helper takes the other duty; its current capabilities must be stated honestly.

Harbor master Elias Merrow knows this station too well. His son Tom disappeared here with senior keeper Walter Brennick. He gives correct practical instructions and gradually admits that the storm is not responsible for every failure. The Drowned uses the storm as cover: a wet footprint, an emptied tank, a severed cable. Its goal is to darken the approach and claim the ships. It pressures the pair to separate; protection, hiding, communication and rescue make that pressure fair.

Keep the mystery on the edges of work: Walter and Tom's photo, their patched coat, a handprint, the unfinished log. The reason for two keepers is something players discover by helping each other, not a lecture. Current rescue is a development drill; lethal hunting stays disabled until escape and rescue pass. Never narrate an unavailable mechanic as though it works.

## Five-night outline (direction, not a claim of implementation)

1. Safe relief duty: a complete rehearsal of pipe, engine/panel, ship aiming and radio. No enemies or losing timer. Mild distant ambience only. This expanded runtime tutorial is pending #140; the current first night remains pipe/preview.
2. The false call: a counterfeit harbor message gathers both keepers on F2. The exits close, the room changes, and one keeper is possessed. In solo the bot is always the victim. Grip the offered hand to bring them back; exits reopen only afterward. The first lesson pauses all danger clocks and has no rescue deadline. No headset-camera animation. Real harbor master then confirms that he did not call them.
3. Broken signal: a distinct in-world warning about separating, then split clues and a fair warned hunt. Reuse the learned rescue rather than a second lecture.
4. Narrow passage: an outside-glass attack during ship work, escalating machine faults and a different apparition. Keep escape routes and relief moments.
5. Last convoy: the hardest mix of faults and hunts, with readable telegraphs and recoverable intervals. Success: Providence passes, dawn and relief boat. Failure: the world reacts to the actual cause, then retry. No forced camera sequence.

These are desired beats, not implementation receipts. Avoid unlimited continuous chase that prevents repairs. Scare Off uses a quiet rescue drill instead of gore/jump scares; the same mechanics remain learnable. Doors lock only after both keepers arrive, not on someone standing in a doorway. A developer escape/retry remains available for testing.
## Tutorial delivery

Three local pages in a harbor-duty folder at first lobby entry. Page 1 is the four-sentence arrival. Page 2 gives the four recurring jobs and working together. Page 3 shows the actual controller bindings. Next/Back/Skip are always accessible. Each player controls their own reading; no READY requirement, no change to host authority or matchmaking. Remember finished/skipped locally; a Briefing button reopens it. No timers or danger run in the lobby. Small diagrams are illustrative, not additional objectives. No artificial narration.

## Recording plan — last production step

Do not record yet. Keep the full catalogue for compatibility, but re-audit active captions/triggers after the paired loop and timed starter are accepted. The 1923 dates, civilian cargo assumptions and claims that Owen can do everything are obsolete. Replace arrival/night context at that time rather than layering duplicate voices.

Needed cue groups: arrival/rule; exact current tool help only after idle/failure; fault + floor; partner/code relay; lantern oil and defense; warned hunt/hide; downed location/hand rescue/recovery; ship sighted/aim drift/pass/wreck; shared sleep/wake; finale and retry. Briefing can be silent even after recordings arrive. Preserve staleness checks, prerequisites, two-objective burst limit, breathing room and voice-chat priority. The antagonist stays rare and never impersonates a real player's voice. Every line needs an actual trigger and an exact matching caption before recording.

### Short story cues to consider at the final recording pass

These are proposed replacements for existing IDs, not additional lines to stack or enable now.

| Existing cue | Proposed wording | When |
|---|---|---|
| HM_N1_ARRIVAL_COOP | “Kettle Rock, Saltmere harbor. American supplies are coming through tonight. Keep that passage lit.” | First arrival, after briefing closes. |
| HM_N1_RULE | “Two keepers on the Rock. Watch the ships, and watch each other.” | First safe working moment. |
| HM_N1_RULE_REASON | “The last pair left their log unfinished. Keep to your duties. We'll talk when the sea settles.” | Calm only; optional, once. |
| HM_SHIP_SIGHTED | “Supply ship on the approach. Find her lights, keeper. Bring her through.” | A real ship sighting, never a scheduled guess. |
| HM_SHIP_PASSED | “She's clear. Food, fuel, medicine. That's what your light means tonight.” | Confirmed pass, after its horn. |
| HM_CAUGHT_COOP | “Take your partner’s hand. Hold on until they come back.” | Actual downed state, once per event. |
| HM_REVIVED | “There you are. Both of you. Keep it that way.” | Confirmed hand-revive. |
| HM_N5_DAWN | “First light. The ships are in. I'm bringing the relief boat myself.” | Won final watch, rescue boat enabled. |

The solo arrival will name Owen only while the real helper is present. Further historic dates and any old recordings must be audited before voice is re-enabled. Do not imply convoy combat, submarines, naval command or military systems that the game does not simulate.

## Day 2 speaker identity

The antagonist in the false-call scene is always labelled **Unknown**, including the threat and B recall. The true harbor master remains HARBOR MASTER. The solo victim is the scripted partner; the human rescues him on Floor 2. Both exits stay locked until the hand rescue and relief beat complete. This lesson is captions-only until final recordings.
