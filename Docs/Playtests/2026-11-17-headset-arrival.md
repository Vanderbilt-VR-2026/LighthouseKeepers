# Headset feedback — November 17, build 052b6813

## Keep

Player arms/feet/eyes/body are accepted. New soundtrack, spatial horror sounds, stair scares, heartbeat haptics and the existing four letters/logs work. Preserve their feel. No AI narration; recordings last.

## Required corrections (user feedback, implementation queue)

1. Briefing spawns at the ceiling on Quest. Wait for tracked height/origin to settle; place in front of actual eyes and recover on tracking/recenter. Story: last keepers vanished **one week** ago, fate unknown; replacement crew guides vital WWII American supply ships and investigates. Keep evil ambiguous. Make F3 bedroom/rest explicit on duties page.
2. Watch clips the left sleeve. Clear the watch without changing accepted arms/feet. Harbor narration/status spam is unwelcome: remove routine time/water/light composites and unsolicited flare hints; HUD teaches steps, HM reserved for useful events. Rewrite remaining old prose naturally.
3. Owen's always-visible black slate and arms held out look wrong. Remove generic slate/status display; use name tag, relaxed hands, useful task animation. Bot must resume after F2 possession, automatically support the complementary role, follow between tasks and fetch/report split clues. Never leave him permanently waiting on a transient route failure or sealed stair.
4. World: backward “Third watch. The light answered back” note; floating internal F3/F4 stair-drop guards (right of F3 ascending stair, left of F4 radio) should go; user explicitly accepts stepping down onto stairs. Preserve outside/island perimeter guards. Remove floating red outlet/slab beside beacon. Correct radio brackets piercing F4 glass as seen from balcony. Put lanterns where instruction says (F2/bedroom/House) with clear labels. Explain intentional wet evidence; no abrupt visible note spawning on bed.
5. Possession is effective but needs hand motion while floating, mouth visible through beard, visible restrained dark eye tears. Preserve reachable revive hand and stationary tracked camera.
6. Ordered campaign: generator/power first when failed, then leak/pump, radio clearance, actively aim/hold ship, rest. No distant ship/emergency objective skipping prerequisites. Radio broadcast should authorize ships to approach at the player's pace. Night1 ship must require teaching aim, not automatic perfect passage. Link Night2 summons to engine failure so it has context. Simpler generator requested again: pending #137 fuel + three timed physical pulls (retire primer/choke after replacement works).
7. Walkies spasm in grip. Fix physical/network pose ownership, offer belt carry, explain radio code acquisition. Solo bot fetches code automatically and gives a readable code reply when the player presses the walkie's trigger; no bot voice needed. Co-op: actual partner uses carried walkie and reads House telegram. Bot should visibly use radio.

## Execution / acceptance

Work in separately committed slices: arrival/caption clarity; companion/navigation + stable walkie and clue delivery; campaign order/clearance + Night1 aim teaching; world/watch/possession placement; generator simplification. Focused tests each; muted actual solo + live pair for replicated behavior; network bump when fields/actions/protocol change. Final clean-source APK, install without launch, honest headset checklist. No claim that new hunter/hiding is delivered.

Device checks: real startup height/seated/recenter, watch cuff, walkie grab/holster, companion stair-follow after rescue, boat/tutorial clarity, F3/F4 interior openings/glass/outlet, possession face/hand motion, complete paired watch. Prior evidence fixtures passed but this actual run exposed timing/navigation/clarity gaps; keep that distinction.
