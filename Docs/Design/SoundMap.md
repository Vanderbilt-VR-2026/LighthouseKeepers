# Sprint 5 sound map

Step 1.1 / #91 skeleton. Existing bus/source values are distinguished from proposed authoring ranges. Ranges below are linear AudioSource gain, not LUFS measurements. Unity priority 0 is highest. No external sound has been downloaded for this skeleton. Add exact author/URL/licence to AssetSources.md before importing anything.

| Event | Sound | Source / attribution | Space | Bus | Gain / mix policy | Priority | Status |
|---|---|---|---|---|---|---|---|
| Arrival / current job / status / warning / win / loss | Recorded harbor master + existing radio chain | Radio/Resources/MasterRadio; voice-lines.md | 3D PA horns | Radio | existing source .95; bus +1.7 dB | 10 | Existing; new sabotage/hunt script #122 |
| Partner nearby / across floors | Live mono voice; wall/floor falloff | Photon Voice + CrewVoiceSession | 3D head | VoiceChat | existing gain/AGC, priority over PA | 0 | Existing; human clarity pending |
| Handheld PTT / near-head talk | Open/close squelch + voice | Existing crew radio filter | 3D receiver | VoiceChat | reuse current speech gain | 0 | Existing |
| Rain exterior / interior / roof | Three supplied looping rain beds | YouTube Audio Library, terms pending; AudioMix.md | 2D zones | Ambience | existing .65/.45/.55 gains; bus −6.5 dB | 140 | Existing; licence entry needs exact confirmation |
| Window / railing / puddle / drip impacts | Damped noise transients | Original Tools/S4/process_rain.py | 3D surfaces | Ambience | existing .08–.16 | 140 | Existing |
| Wind / thunder / lightning / waves | Gusts, near/far thunder, surf | Existing generated storm clips | 3D events / 2D bed | Ambience | reuse current mix; critical thunder below voice | 140 | Existing; phase 9 audit |
| Footsteps / stairs | Wood/metal cadence | Original existing S4 synthesis | 3D feet | SFX_World | bus −6 dB; audit source gain | 90 | Existing |
| Wrench pickup / collision / splash | Metal clank, water splash | Existing S4 clips | 3D contact | SFX_World | bus −6 dB | 90 | Existing |
| Bolt seat / ratchet / seal | Clunk, tick, final clunk | Existing pipe sound code | 3D bolt | SFX_World | quieter reset; bus −6 dB | 90 | Existing |
| Pipe leak / flood / doorway seep | Pressure hiss, rising water, trickle | Existing S4/VisualAudio synthesis | 3D pipe/door; flood bed | SFX_Critical / Ambience | leak-scaled; seep existing .10 ceiling | 40 / 140 | Existing |
| Valve held / released | Pressure fall / surge | Existing leak sound state | 3D pipe | SFX_Critical | pressure-scaled, no new duplicate loop | 40 | Existing |
| Fuel warning / exhaustion / pour | Cough/sputter; pour/gurgle | Existing generator sound set | 3D engine/filler | SFX_Critical / SFX_World | bus −3/−6 dB | 40 / 90 | Existing |
| Primer / choke / starter / catch / run | Squeeze click, detent, crank/cough/catch, engine loop | Existing generator clips | 3D controls/engine | SFX_Critical / SFX_World | reuse current levels; no extra voice per squeeze | 40 / 90 | Existing |
| Breaker switch / trip / reset / return | Clunk, bang, mains hum | Existing S4Audio/breaker and wiring | 3D panel/boxes | SFX_Critical | bus −3 dB; return hum low | 40 | Existing |
| Beacon ignition / alignment / dark / soot | Motor, lock cue, power-down | Existing beacon synthesis | 3D lantern room | SFX_Critical | bus −3 dB | 40 | Existing |
| Radio power / tune / code / mic / reply | Static → tone; button click/buzzer; mic; reply | Existing station radio set | 3D console | SFX_Critical | bus −3 dB; PA owns spoken reply | 40 | Existing |
| Pole grab / slide / exit | Metal squeal/click | Existing pole + WorldAudioGaps | 3D pole | SFX_World | existing low source; bus −6 dB | 90 | Existing |
| Bunk vote / fade / dawn / chalk | Creak, storm, bell, chalk | ImmersionAudio/creak; VisualAudio/chalk; existing stings | 3D bunk/board; 2D transition | SFX_World / Music | existing creak .12, chalk .10; duck voice | 90 / 200 | Existing |
| Music calm / tension / crisis / rest | Bar-aligned crossfades, quiet rests | Team Gemini/Lyria; Music.md | 2D | Music | existing .23; bus −2 dB before duck | 200 | Existing |
| Task fixed / night survived / lost | Short selected stings | Team Gemini/Lyria; Music.md | 2D | SFX_Critical | existing .45; bus −3 dB | 40 | Existing |
| Pre-scare silence / dread / hunt / relief | 2–3 s dip; hunted layer; resolve | TensionDirector; new original synthesis planned | 2D + localized telegraph | Music / Ambience / Danger | planned: under voice; never master-mute | 20–200 | Pending #92/#121 |
| Fear heartbeat / calm-shaky-panic breath / hold breath | 62–140 bpm lub-dub; close breath | Original runtime synthesis | 2D local keeper | SFX_Critical; undertone Music | heart .045–.16, breath .07–.20; optional double haptic | 40 / 200 | Implemented #93; device audition and future hiding integration pending |
| Wet steps / dragging / breathing / whispers / hum / growl | Drowned presence cues | Original synthesis or licensed sources, not selected | 3D entity | Danger (planned) | planned ≤critical SFX; no clipping | 20 | Pending #104/#105 |
| Sabotage pipe / fuel / cable / glass / radio | Metal tear, gurgle, snap/spark, wet smear, static cut | New original synthesis planned | 3D affected station | Danger (planned) | planned .15–.45 source, voice ducks | 20 | Pending #98–100 |
| Lantern lift / wick / flame / sputter / oil / hang | Metal bail, wick click, flame, pour, hook | New original synthesis planned | 3D lantern/filler | SFX_World | planned .08–.25 source; bus −6 dB | 90 | Pending #101–103 |
| Door / wardrobe / locker / bunk hide | Hinge creak, latch, metal clang, cloth | Reuse existing creak/clunk first; synthesis if needed | 3D hinge/contact | SFX_World | planned .08–.25 source | 90 | Pending #106/#117 |
| Sprint / hiding / peek | Faster footsteps, breath, cloth | Existing steps + new breath | 3D feet / 2D breath | SFX_World / Danger | planned never masks nearby hunter telegraph | 90 / 20 | Pending #105/#106 |
| Lantern stun / bell / flare / catch / revive | Recoil, brass ring, shot/hiss, underwater, recovery | New original synthesis planned | 3D source; 2D caught transition | Danger / SFX_Critical | planned .15–.5 source; limiter ceiling | 20 / 40 | Pending #107 |
| Outside alone / fins / rocks figure | Breath behind, whispers, closer wave | New original synthesis planned | 3D outside only | Danger / Ambience | planned .05–.2 source; no surprise over ceiling | 20 / 140 | Pending #104 |
| Ship sighted / approach / near hull | Distant→near horn, chug, hull creak | New original synthesis planned | 3D ship | Danger / SFX_Critical | planned .25–.6 with distance rolloff | 20 / 40 | Pending #94/#95 |
| Ship pass / wreck / flare | Thanks horn/resolve; grinding crash/whoosh | New original synthesis + existing sting | 3D ship/rocks; 2D resolve | SFX_Critical / Music | planned .2–.6 source; no deafening crash | 40 / 200 | Pending #95/#97 |
| Dawn / rescue boat / credits / page turn | Calm surf, gulls, motor, page | New original synthesis planned; existing calm music | 3D boat / 2D bed/page | Ambience / SFX_World / Music | planned .05–.3 source; voice leads | 90 / 140 / 200 | Pending #111/#114 |

## Routing contract

Keep current VoiceChat > Radio > critical machines > world/ambience > music. Add a dedicated Danger/Entity bus above critical machines when fear/entity sounds are implemented; it must stay crisp while only world sounds are low-passed by fear. Do not run voice chat through that fear filter. The current mix has six buses; Danger is a planned addition, not an existing asset.

Preserve the current voice-first ducking and saved player sliders. Existing voice ducks music/ambience by 7/3 dB; PA uses its accepted 9/5 dB follow-up with 50 ms attack / 600 ms release. Radio speech is 250–4500 Hz with quiet static and short reverb. Mono input + WebRTC echo cancellation/noise suppression must remain enabled. Re-tune only with captures, not assumed loudness. Source details and historical measurements: [AudioMix.md](AudioMix.md), [Music.md](Music.md).

Every new clip needs its filename, source/licence, decoded LUFS and true peak; every new loop needs a seam check. Long beds stream compressed, short cues preload. Pool one-shots and cap real voices at 24. Keep the −1 dBTP master ceiling; no scare exceeds it. Measure a two-minute Night 3 per-bus playback capture and Quest DSP cost in phase 9; source-file LUFS is not that result. Silence and breathing must not mask the one urgent instruction.

Editor audio remains muted during all automation. Auditory quality, comfort, true spatial localization and current-build human voice intelligibility remain device/listening checks even when event timing tests pass.

## Ship phase2 implementation

`Sprint5Ships/foghorn`: spatial SFX_Critical, .35 source volume, linear25–320m, priority45, one call per24 simulation seconds. `thanks` and `wreck` reuse that source, replacing rather than layering it. `engine`: spatial SFX_World, .16,12–110m, priority115, loop. All synthesized locally; originals and script in AssetSources. Ship pass plays the existing short survived sting on the adaptive-music sting source; it does not signal sleeping. The host TensionDirector supplies crisis during passage and relief after pass to both players. Quest voice-count/CPU and human audibility remain device checks.
