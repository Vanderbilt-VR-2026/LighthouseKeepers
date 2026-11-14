# Feel checklist — five beats per interaction

Step 1.1 / #91 skeleton. These are acceptance targets, not a claim that every cue is already present. “Existing” identifies a current verb; validate its individual sound/visual/haptic details in phase 9 (#119). New entries stay pending until implemented and exercised. Player-facing words live in KeeperStrings, not this table.

| Interaction | Hear | Find | Understand | Do | Payoff: visual / sound / haptic / world | Status |
|---|---|---|---|---|---|---|
| Wrench pickup / drop | bench clink | bench/current glow | GRIP | hold; release falls | collision clank; recovery after timeout | Existing; device feel pending |
| Pipe bolt pull | leak hiss | active bolt/chalk | auto-snap near head | short arm pull; ratchet return | bolt turns/sinks; spray/gauge reduce; click/haptic | Existing |
| Crew pressure valve | surging pipe | beside active bolt | role objective | one holds GRIP; other repairs | spray changes; release resumes pressure | Existing; N3+ crew policy |
| Fuel can / filler | engine sputter | red can and gauge | fill instruction | grip + tilt over cap | gauge rises; pour sound | Existing |
| Primer bulb | dead engine | current bulb glow | squeeze ×3 | GRIP pressure + release | bulb deflates; tally/click/haptic | Existing |
| Choke | engine state | lever glow | open/close objective | grab and translate to detent | lever snaps; clunk/haptic | Existing |
| Starter cord | cough | T-handle glow | pull ×3 | grip + pull toward keeper | cord extends/recoils; catch/run | Existing |
| Power-panel switches/reset | trip bang | labelled wall cabinet | one circuit objective | physical switch/reset | lamp + meter + circuit/wiring reaction | Existing |
| Beacon ignition/alignment | motor whine | wheel/lamp | light then align | switch; turn wheel | beam + marker; motor/haptic | Existing |
| Beacon lens wipe | dim beam | soot/handprint | wipe lens | rag rubbing | soot clears; beam recovers | Existing soot; sabotage handprints pending #99 |
| Radio power/tune/code/PTT | static | current console control | one step + remote code | toggle; dial; buttons; mic hold | lock tone, code lamps, chatter/reply | Existing; split info #112 pending |
| PA replay / B recall | keeper message | speaker/objective | B taught once | press or double tap | last caption/audio; no menu noise | Existing |
| Handheld radio | distant partner | two bench radios | head/trigger transmit | grip; hold near head or PTT | squelch, voice reaches floors | Existing |
| Watch / objective height | world danger | left wrist face | glance / deliberate settings | wrist turn; height slider | backlight, timer, recalled instruction | Existing |
| Pole slide | shaft squeal | pole/side plate | GRIP TO SLIDE | grip; release at floor | controlled descent; exit; soft haptic | Existing |
| Bunk / shared sleep | all jobs done line | warm bunk lamp | sleep objective | grip; crew votes | night page, storm, dawn bell | Existing |
| Door / wardrobe / locker | creak | handle/slats | grab handle | hand-swing hinge; latch | door motion/click; hiding occlusion | Pending #117/#106 |
| Lantern hold / wick / hook | flame flutter | House hook | current wick setting | grip; wick; hang | warm pool, sway, flame and haptic | Pending #101 |
| Lantern oil refill | sputter | F2 oil can | low-oil cue | tilt over filler | oil rises; pour; flame steadies | Pending #102 |
| Cable reconnect | snap/spark | torn ends/evidence | join cable | grab ends and join | spark/click; circuit returns | Pending #99 |
| Sprint / hide / hold breath | hunt telegraph | escape/hiding route | first-encounter lesson | stick click; crouch/A; hold A | breath/stamina; unseen hiding breaks pursuit | Pending #105/#106 |
| Lantern stun | wet breathing | visible hunter | HIGH beam | aim for two seconds | recoil; retreat; oil cost | Pending #107 |
| Storm bell | hunt | F2 brass bell | one use/night | grip rope and pull | bell rings; entity retreats | Pending #107 |
| Flare box / gun | hunt/ship | F4 box; key hook | unlock / fire | key, grip, fire | flare hiss; retreat; limited ammo | Pending #107 |
| Partner revive | P26 distress | downed partner | reach partner | grip their hand | recover; alarm/breath relax | Pending #107 |
| Ship protection | approaching horn | horizon/beam | keep light shining | restore dependent machines | pass horn/swell or named wreck/flare | Pending #94–97 |
| Night continue / select / Demo | lobby quiet | HOST page | one clear choice | press intentional button | load saved unlocked night | Pending #115/#116 |
| Logbook / retry / credits | page turn | morning/loss page | one reason/outcome | turn/retry/return | saved progress or reset; no forced motion | Existing retry; report/credits pending #111/#114 |

## Shared acceptance gate

Every row needs eye-height screenshots, an interaction test (including release/retry where applicable), and headset fit/latency checks. Target: understand within two seconds; each step is one physical verb with <50 ms grab response. Default final payoff haptic is 0.10 s at 0.6 unless the existing verb needs a gentler ratchet; verify comfort, do not apply blindly. Harbor acknowledgement is optional and rate-limited, not another line on every click. No future-control glow, floating completion panel, hand-rotation change or new camera motion.

Evidence is linked from Next.md per issue. Phase 9 must replace each pending cell with the actual implementation, measured timing and device result; no unchecked target is a pass.
