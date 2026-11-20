# Lighthouse Keepers — approved voice script (Sprint 5)

Source: Aneesh's approved story/voice brief, November 16, 2026. Words below are the recording and caption authority. Implementation status and proposed corrections are separate in `Implementation.md`; an unavailable action must never be taught. Owen never speaks.

Filename = lowercase ID + `.wav`. Mono 44.1 kHz, 16-bit, one take, under 10 s; no spoken IDs/numbers/slates. Preserve existing audio filenames/GUIDs; new IDs are additive. HM = Elias Merrow (existing PA/radio chain). ANT presets M = Margaret, L = Lottie, W = Walter. ANT is not radio-processed.

## Playback contract

1. P1 danger > P2 objective/warning > P3 story > P4 flavour. Higher priority cancels queued lower priority. Voice chat wins.
2. Revalidate each queued trigger immediately before playback; discard repaired faults, gone ships and finished steps.
3. Prerequisites before instructions. Never teach choke before prime.
4. At most two P2 lines back to back, then at least 8 s before another non-P1 line.
5. Help after ~20 s idle near the station (15 s where specified), or two failures; never while doing the action; 60 s cooldown per help line.
6. HM/ANT never overlap. ANT only alone, low light, HM queue empty, >=4 s after HM, >=90 s after ANT, <=6 per night, never N1, Full scares only. No ANT start during nearby voice chat.
7. Teammates talking: HM -6 dB, ANT -10 dB. Music -7 dB during speech; never cut.
8. P3 story only in calm: no hunt, no ship window, no unrepaired fault younger than 10 s.
9. HM captions exactly match speech. ANT captions lowercase, italic, dimmer, no speaker tag, recipient only. Nonverbal caption `[humming]`.
10. ANT source 0.6–1.2 m behind/beside the listener, quiet close whisper, M/W -2 to -4 semitones or L +2, faint wet layer, short room reverb, <=-24 LUFS short-term. HM plain close-miked, clear names/numbers, light radio static.

## Lines

| ID | Speaker | Exact text | Delivery | Trigger | Prerequisite | Priority | Repeat | Cancel/suppress | Mode |
|---|---|---|---|---|---|---|---|---|---|
| HM_N1_ARRIVAL_COOP | HM | Kettle Rock, Saltmere port office. We have your arrival. Keep the passage lit, and report anything the last crew left behind. | warm, businesslike, a little tired | N1 start +3 s | — | P2 | 1/run | — | C |
| HM_N1_ARRIVAL_SOLO | HM | Kettle Rock, Saltmere port office. We have your arrival. Keep the passage lit, and report anything the last crew left behind. | warm, businesslike | N1 start +3 s | — | P2 | 1/run | — | S |
| HM_N1_OWEN_INTRO | HM | Owen Teague lost his hearing in the war. Sharpest eyes on the coast. Point, and he'll follow. | fond, matter-of-fact | after ARRIVAL_SOLO | — | P2 | 1/run | — | S |
| HM_N1_RULE | HM | One rule on the Kettle above all the others. Two keepers. Always together after dark. | firm, slower | after arrival | — | P2 | 1/run | — | B |
| HM_N1_RULE_REASON | HM | The Board will tell you it's the machinery. Needs two pairs of hands. Well. It does. | dry, a beat too long before Well | 20 s after RULE, calm | — | P3 | 1/run | any fault active | B |
| HM_N1_FIRST_JOB | HM | Job board's by the door. Start with what's leaking. There's always something leaking. | light, wry | after RULE | — | P2 | 1/run | — | B |
| HM_N2_OPEN | HM | Saltmere port office. Check the engine before the next convoy. | brisk, a little worried | N2 start | — | P2 | 1/night | — | B |
| HM_N3_OPEN | HM | Night three. I've a broadcast for the ships, and I need your radio for it. | businesslike | N3 start | — | P2 | 1/night | — | B |
| HM_N4_OPEN | HM | Worst of it's coming tonight. Two ships, close together. You'll be busy. | serious, steady | N4 start | — | P2 | 1/night | — | B |
| HM_N5_OPEN | HM | Last night of the storm. The Providence sails at midnight. Two hundred and twelve aboard. | quiet, heavy | N5 start | — | P2 | 1/night | — | B |
| HM_DEMO_OPEN | HM | Kettle Rock, harbor here. Ship due in five minutes, and you've a pipe gone on Floor One. Move. | urgent but controlled | Demo start | — | P2 | 1/run | — | B |
| HM_N1_PIPE_LEAK | HM | Water's coming in on Floor One. Pipe joint by the pump. Take the wrench from the bench. | clear, practical | N1 pipe fault | — | P2 | 1/run | pipe fixed | B |
| HM_HELP_PIPE_BOLTS | HM | Three bolts on that joint. Fit the wrench and turn them right. Righty tighty. | patient | idle 20 s at pipe | holding wrench | P2 | 60 s cd | turning a bolt / fixed | B |
| HM_HELP_PIPE_ONE_LEFT | HM | One bolt left. Keep at it. | encouraging | 2 of 3 bolts done, idle 15 s | — | P4 | 1/night | fixed | B |
| HM_PIPE_FIXED | HM | That'll hold. Good work. | satisfied | pipe fixed | — | P2 | 1/night | — | B |
| HM_HELP_PUMP | HM | The pump clears the water, but only if it's got power. Check the panel on Floor Two. | practical | water rising + pump unpowered 20 s | — | P2 | 60 s cd | pump powered | B |
| HM_HELP_VALVE_COOP | HM | That pressure valve takes two. One of you on each wheel. Turn together, slow. | patient | valve step, idle 20 s | — | P2 | 60 s cd | both turning / done | C |
| HM_HELP_VALVE_SOLO | HM | Pressure valve takes two. Point Teague at the far wheel, then turn together. | patient | valve step, idle 20 s | — | P2 | 60 s cd | Owen on wheel / done | S |
| HM_HELP_GEN_REFUEL | HM | Fill her from the can first. Tilt it right over the cap. | calm instruction | at generator, tank empty, idle 15 s | — | P2 | 60 s cd | pouring / full | B |
| HM_HELP_GEN_PRIME | HM | Now prime it. Pump the primer till it goes stiff. | calm | tank full, not primed, idle 15 s | refuelled | P2 | 60 s cd | primed | B |
| HM_HELP_GEN_CHOKE | HM | Choke closed for a cold start. Then pull the cord. Hard. | calm | primed, choke open, idle 15 s | primed | P2 | 60 s cd | choke closed | B |
| HM_HELP_GEN_PULL_AGAIN | HM | She won't catch first go. Pull again. | encouraging | 1st failed pull | choke closed | P4 | 2/night | running | B |
| HM_GEN_RUNNING | HM | Power is back. Check your partner, then the remaining work. | relief, warm | generator starts | — | P2 | 1/night | — | B |
| HM_HELP_PANEL | HM | Power panel's on the wall, Floor Two. Not enough for everything. Pick what matters. | practical | first power shortage | — | P2 | 1/run | — | B |
| HM_HELP_RADIO_TUNE | HM | Radio's on Floor Four. Turn the dial till my voice comes in clear. | patient | radio task, idle 20 s | — | P2 | 60 s cd | tuned | B |
| HM_HELP_RADIO_CODE_COOP | HM | Tonight's code is on the telegram in the House. One of you reads it, one sends it. | clear | code step, not entered 20 s | tuned | P2 | 60 s cd | code sent | C |
| HM_HELP_RADIO_CODE_SOLO | HM | Code's on the telegram in the House. Send Teague for it, or fetch it yourself. | clear | code step, not entered 20 s | tuned | P2 | 60 s cd | code sent | S |
| HM_RADIO_TRANSMIT_OK | HM | Got you loud and clear, Kettle Rock. I'll pass the word to the ships. | pleased | code transmitted | — | P2 | 1/night | — | B |
| HM_HELP_BEACON_AIM | HM | Beacon wheel's on Floor Four. Turn the beam onto her lights and hold it there. | urgent-calm | ship approaching, beam off-target 15 s | beacon powered | P2 | 60 s cd | on target | B |
| HM_HELP_BEACON_HOLD | HM | Keep it on her. She has to see you the whole way past the rocks. | steady | first time on target in a window | — | P2 | 1/night | — | B |
| HM_BEACON_DRIFTING | HM | You're drifting off her. Bring it back. | sharp, short | off target >3 s in danger window | — | P1 | 30 s cd | on target | B |
| HM_SAB_PIPE | HM | Something's torn that pipe open again. Follow the water, Floor One. | grim | pipe sabotage, N2+ | — | P2 | 1/night | fixed | B |
| HM_N2_FUEL_FAILURE | HM | The engine is losing fuel. Check the tank on Floor Two. | urgent | fuel sabotage | — | P2 | 1/night | running | B |
| HM_SAB_CABLE | HM | You've lost a circuit. There's a cable down on Floor Two. Join it at the junction box. | urgent, clear | cable sabotage | — | P2 | 1/night | fixed | B |
| HM_CABLE_FIXED | HM | That's the circuit back. Lights on Two. | relief | cable fixed | — | P2 | 1/night | — | B |
| HM_SAB_LENS | HM | Your light's fouled. There's muck on the glass. Get a rag on it. | urgent | lens smeared | — | P1 | 1/night | clean | B |
| HM_LENS_CLEAN | HM | That's it. She's shining clean. | relief | lens clean | — | P2 | 1/night | — | B |
| HM_SAB_RADIO_RESTORED | HM | There you are. Wire was cut. That wasn't the storm. | quiet, unsettled | radio wire rejoined; HM silent while cut | — | P2 | 1/night | — | B |
| HM_N2_HANDPRINTS | HM | If there's marks on that tank, they're old. Get her running. | too quick, then flat | player within 1.5 m of tank handprints | — | P3 | 1/run | generator running | B |
| HM_N2_LANTERN_INTRO | HM | Take a lantern from the marked hook. You will need it while the engine is out. | calm, firm | first blackout | — | P2 | 1/run | lantern held | B |
| HM_N2_LANTERN_OIL | HM | Oil's dear out there. Keep the wick low, and hang it up while you work. | practical | first lantern grab | — | P2 | 1/run | — | B |
| HM_LANTERN_LOW_OIL | HM | Your lamp's running low. There's one can of oil on the shelf, Floor Two. | concerned | held lantern <20% oil | can unused | P2 | 1/night | refilled | B |
| HM_WARN_WATER_HALF | HM | Water's halfway to the engine. Get that pump running. | firm | water 50% | — | P2 | 1/night | falling | B |
| HM_WARN_WATER_CRITICAL | HM | Water's at the machinery! Pump, now, or you lose the power! | urgent, raised | water 85% | — | P1 | 2/night | falling | B |
| HM_POWER_SHIP_PRIORITY | HM | Ship's coming. Beacon first. The water can wait a minute. Not much more. | decisive | ship approaching + beacon unpowered | — | P1 | 1/night | beacon powered | B |
| HM_WARN_BEACON_DARK | HM | Your light's dark and there's a ship on the approach! | alarmed | beacon dark + ship approaching | — | P1 | 30 s cd | beacon shining | B |
| HM_N1_SHIP_PREVIEW | HM | See those lights? That's the Mary Ellen, coal for the city. She'll pass well clear tonight. | relaxed, teaching | N1 ship lights visible | — | P3 | 1/run | — | B |
| HM_SHIP_SIGHTED | HM | Lights to the south. There's a ship making for the passage. | alert | ship lights appear, N2+ | — | P2 | 1/ship | — | B |
| HM_SHIP_DANGER | HM | She's in close by the rocks. Get your beam on her. Now. | urgent | danger window opens | — | P1 | 1/ship | on target | B |
| HM_SHIP_PASSED | HM | Ship clear of the rocks. Keep watch for the next signal. | relieved, proud | ship passed | — | P2 | 1/ship | — | B |
| HM_SHIP_WRECK | HM | No. No, she's on the Kettles. | stricken, quiet | wreck | — | P1 | 1/ship | — | B |
| HM_SHIP_WRECK_AFTER | HM | Nothing you can do for her now. Keep the light. There'll be others. | hollow, steady | 6 s after WRECK, non-final ship | — | P2 | 1/night | — | B |
| HM_SEPARATED_COOP | HM | You two are on different floors. Don't stay that way long. | quiet, uneasy | different floors 45 s, N2+, dark | — | P3 | 1/night | reunited | C |
| HM_SEPARATED_SOLO | HM | Teague's lost sight of you. Get back to him. | quiet, firm | Owen lost sight 30 s in darkness | — | P3 | 1/night | regained | S |
| HM_REUNION | HM | That's better. Stay where you can see each other. | relief | reunited after hunt or separation | — | P3 | 1/night | — | B |
| HM_N3_HIDE | HM | Kettle Rock, listen to me. Get somewhere it can't see you. Hide. Now. | urgent, low, every word clear | first hunt telegraph | — | P1 | 1/run | — | B |
| HM_HIDE_COACH | HM | Don't move. Don't breathe. It'll pass. | whisper-close to the mic | first hidden during hunt | — | P1 | 1/night | hunt ends | B |
| HM_N3_AFTER_HUNT | HM | You're still there. Good. Stay together. I mean it this time. | shaken, then hard | first hunt ends | — | P2 | 1/run | — | B |
| HM_BELL | HM | The storm bell, Floor Two. Ring it. It can't abide the bell. | urgent | first chase, bell unused | — | P1 | 1/run | bell rung | B |
| HM_FLARE | HM | Flare pistol's on the bench in the House. One shot. Make it count. | urgent | N4 first chase, flare unused | — | P1 | 1/run | flare fired | B |
| HM_CAUGHT_SOLO | HM | Keeper? Keeper! Get up. You're on Floor One. Get up. | frightened, then commanding | solo caught first time | — | P1 | 1/night | — | S |
| HM_CAUGHT_COOP | HM | Your partner's down on Floor One. Get to them. Take their hand. | urgent | co-op player downed | — | P1 | 2/night | revived | C |
| HM_REVIVED | HM | There. Both of you. Don't let go of each other again. | relief | revive completed | — | P2 | 1/night | — | C |
| HM_N2_WALTER | HM | Walter Brennick kept that engine nineteen years. Never lost a night. Till this summer. | fond, then it catches | N2, GEN_RUNNING +30 s calm | — | P3 | 1/run | — | B |
| HM_N3_RECOGNITION | HM | Same as August. The fuel, then a cable. Same order. | to himself, low | N3 cable fixed +calm | — | P3 | 1/run | — | B |
| HM_N3_RULE_TRUTH | HM | It's not the machinery. It's never been the machinery. Just don't be alone in the dark. | quiet, plain, almost ashamed | N3_AFTER_HUNT +20 s | — | P3 | 1/run | — | B |
| HM_N4_TOM | HM | My son was stationed on the Kettle. Tom. He has been missing for a week. | flat, holding it together | N4 opening calm, ~60 s | — | P3 | 1/run | — | B |
| HM_N4_STAIRS | HM | Tom wrote about something at the bottom of the stairs. I thought he was tired. | quiet | N4 tall figure sighting or radio log read | — | P3 | 1/run | — | B |
| HM_N4_NOT_WALTER | HM | If you see Walter, it's not Walter. Do you understand me? It is not Walter. | intense, slow | after N4_STAIRS +calm | — | P3 | 1/run | — | B |
| HM_N5_TRUTH_1 | HM | There's a thing I should've told you on the first night. | hesitant | N5 first calm after first ship | — | P3 | 1/run | — | B |
| HM_N5_TRUTH_2 | HM | The Lucy Gale. I told Walter to light her through, whatever it took. He went down alone. | confession, steady | 10 s after TRUTH_1 | HM_N5_TRUTH_1 | P3 | 1/run | — | B |
| HM_N5_TRUTH_3 | HM | Tom called me. Said Walter was on the stairs. The Board says no boats in the dark. I didn't send one. | breaking, controlled | next calm >=30 s later | HM_N5_TRUTH_2 | P3 | 1/run | — | B |
| HM_N5_TRUTH_4 | HM | I'll not give that order again. Not for any ship. You keep each other first. | resolved | 8 s after TRUTH_3 | HM_N5_TRUTH_3 | P3 | 1/run | — | B |
| HM_N5_OWEN | HM | Why do you think I sent you a man who can't hear it? | quiet, almost a smile | TRUTH_4 +calm | HM_N5_TRUTH_4 | P3 | 1/run | — | S |
| HM_STORY_TELEGRAM | HM | You read the bottom of that telegram? Good. I meant it. | quiet | 10 s after telegram read, N3 | — | P4 | 1/run | — | B |
| HM_TASKS_DONE_COOP | HM | That's the watch done. Get some sleep, both of you. Bunks are on Three. | warm, tired | all tasks done | — | P2 | 1/night | — | C |
| HM_TASKS_DONE_SOLO | HM | That's the watch done. Get some sleep. Teague will take the first hour. | warm, tired | all tasks done | — | P2 | 1/night | — | S |
| HM_EARLY_SLEEP | HM | Not yet. There's still work on the board. | gentle, firm | tries to sleep early | — | P2 | 30 s cd | — | B |
| HM_MORNING | HM | Morning, Kettle Rock. You made it through. Saltmere thanks you. | relieved, warm | wake, N1–N4 | — | P2 | 1/night | — | B |
| HM_N1_DAWN | HM | Not bad for a first night. Get some breakfast in you. | light | N1 wake after HM_MORNING | — | P4 | 1/run | — | B |
| HM_FAIL_FLOOD | HM | Water's in the machinery. The light's gone. We'll take the watch again. | defeated, kind | flood loss | — | P1 | — | — | B |
| HM_FAIL_WRECK | HM | We lost her. We'll go again from the start of the watch. | heavy | wreck loss | — | P1 | — | — | B |
| HM_FAIL_TAKEN | HM | Kettle Rock? I can't raise either of you. Kettle Rock? | frightened, growing quieter | night lost to catch | — | P1 | — | — | B |
| HM_RETRY | HM | Right. Again. Same storm, same ships. You know it better now. | steady, encouraging | retry starts | — | P2 | — | — | B |
| HM_N5_PROVIDENCE_SIGHTED | HM | That's her. The Providence. Every light she has is lit. She's trusting you. | awed, grave | Providence lights appear | — | P2 | 1/run | — | B |
| HM_N5_PROVIDENCE_PASSED | HM | She's through. Two hundred and twelve. She's through. | overwhelmed, quiet | Providence passes | — | P1 | 1/run | — | B |
| HM_N5_DAWN | HM | First light. I'm coming out myself. Board be damned. | resolved, emotional | N5 dawn | win | P2 | 1/run | — | B |
| HM_N5_CANDLES | HM | Look at the city. A candle in every window. That's for you. | soft | 8 s after N5_DAWN | — | P3 | 1/run | — | B |
| HM_DEMO_END | HM | That's how the night goes on the Kettle. There are five of them. | warm, hint of dread | demo complete | — | P2 | 1/run | — | B |
| ANT_N2_ENGINE | M | It's quieter without the engine. | pleased, almost kind | alone N2 blackout 20 s | — | P3 | 1/run | engine running | B |
| ANT_LAMP_LOW | M | Your little lamp is getting smaller. | patient | alone lantern <30% | — | P3 | 1/night | refilled | B |
| ANT_BOLT | M | That bolt was tight before. | observational | alone at sabotaged pipe | — | P3 | 1/run | fixed | B |
| ANT_TANK | M | It drinks so much, your engine. | slow | alone generator in dark | — | P3 | 1/run | — | B |
| ANT_MARGARET_LOG | M | Eleven weeks. | barely audible | alone reading Margaret log | item 13 read | P3 | 1/run | — | B |
| ANT_LEFT_PARTNER_COOP | W | You left the other one upstairs. | flat, close | co-op alone lower floor dark N3+ | — | P3 | 1/night | reunited | C |
| ANT_LEFT_PARTNER_SOLO | W | You left the quiet one upstairs. | flat, close | solo Owen lost sight 20 s N3+ | — | P3 | 1/night | regained | S |
| ANT_WHERE | W | Where is the other one now? | curious | alone dark N3+ | — | P3 | 1/night | reunited | B |
| ANT_STAIRS_COUNT | W | Forty-one. Forty-two. | counting slowly, unhurried | alone dark stairs | — | P3 | 1/run | — | B |
| ANT_STAIRS_WALTER | W | Walter used to count these stairs. | thoughtful | alone dark stairs N4+ | ANT_STAIRS_COUNT | P3 | 1/run | — | B |
| ANT_HIDE_NEAR | W | Breathing is loud, in a cupboard. | right beside you | hidden, within 2 m, not holding breath | — | P1 | 1/night | breath held | B |
| ANT_HIDE_LEAVE | W | You'll come out. They always come out. | moving away | leaves search | — | P3 | 1/night | — | B |
| ANT_BUNK | W | Tom hid under that bunk. | quiet | searching F3, keeper under bunk | — | P3 | 1/run | — | B |
| ANT_RUN | W | Run, then. | almost amused | chase starts alone | — | P1 | 1/night | — | B |
| ANT_SHIP | M | So many lights on her. So many. | hungry, soft | alone gallery ship window | — | P3 | 1/run | — | B |
| ANT_WRECK | M | Thank you. | sincere, tiny | wreck, keeper alone | — | P3 | 1/run | — | B |
| ANT_LIGHT_OUT | M | There. That's better. | relieved | beacon goes dark, keeper alone | — | P3 | 1/night | — | B |
| ANT_STUNG | M | Margaret kept a lamp too. | hurt, cold | first lantern stun | — | P3 | 1/run | — | B |
| ANT_DOLL | L | She dropped that. Give it back. | small, flat, too calm | alone holding doll | item 11 | P3 | 1/run | — | B |
| ANT_HUM | L | [humming] | Eternal Father, Strong to Save, first line, distant | apparition E13 | — | P4 | 1/night | — | B |
| ANT_CHILD_STAIRS | L | It's dry up there. | small, from below | child stairs N5 | — | P3 | 1/run | — | B |
| ANT_N4_SHIP_A_SHIP | W | A ship's a ship, boy. | Walter cadence, gentle | N4/N5 split dark ship window | — | P3 | 1/run | — | B |
| ANT_N5_TOM | W | He stood in the light until it went out. | quiet, final | N5 alone dark | — | P3 | 1/run | — | B |

## Demo order

HM_DEMO_OPEN → HM_SAB_PIPE → optional HM_HELP_PIPE_BOLTS → HM_PIPE_FIXED → HM_N2_FUEL_FAILURE → HM_N2_LANTERN_INTRO → HM_N2_LANTERN_OIL → optional HM_HELP_GEN_* → HM_GEN_RUNNING → HM_SAB_CABLE → HM_N3_HIDE → HM_HIDE_COACH → ANT_HIDE_LEAVE → HM_CABLE_FIXED → HM_SHIP_SIGHTED → HM_SHIP_DANGER → HM_SHIP_PASSED → HM_DEMO_END. About 18 lines in six minutes, silence between beats.

The supplied count was HM 89 / ANT 23 / total 112. The importer counts the actual rows and reports any discrepancy rather than inventing a missing line. A complete run plays roughly 60–70, not the entire corpus.
