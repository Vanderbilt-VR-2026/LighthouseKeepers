# Flare pistol — one active defense

The latest user direction supersedes lantern stun, bell and trap defenses. Two original signal pistols sit in the House beside the bench from Night 2 onward. Grip holds the fitted handle; trigger fires one round. Each pistol has one round per night, replenished on retry/new night. Misses spend the round. Releasing drops the prop; it returns after 20 seconds or if below the floor. Night 1 remains unchanged.

A ballistic flare hitting the manifested Drowned sends it into the existing retreat state. Walls stop shots. It adds no forced camera motion, realtime light, deathmatch damage or new failure path. This works against the current apparition; a full hunter is still to come. An empty trigger clicks with a short cooldown. Sound is an original synthesized 1.6-second pop/hiss, routed to SFX_Critical.

The host owns ammo, muzzle pose, collision and repulsion. A guest must have the fresh existing grab lease and a nearby tracked hand. Requests cannot provide arbitrary projectile origins. Both clients reproduce the same timestamped flight/impact. Replay/old revision/unowned requests cannot spend ammo. Network version is LK-S5-FLARE1 / LK-Lab-Coop-v31 because the snapshot adds flare receipts. Both clients must update.

The reference [RuneFox237 FlareGun](https://jinxxy.com/RuneFox237/FlareGun) is a PC VRChat package with standalone and usage restrictions. No package, mesh, texture, code or audio was imported. Our ~29 cm pistol is original geometry: a hollow flared barrel, break-action receiver, trigger guard, pivot, hammer and fitted wood stocks. Uses the accepted tan-hand pose without changing the base hand rotation. Real XRI grabs were inspected for left/right hands; headset grip comfort remains unverified.

Switch: Sprint5Tuning.flareDefense. The deterministic pair is registered before the existing prop registry seals; Night 1 hides/disables them rather than changing the prop IDs between nights.

Verification: 13 transport/flare/authority cases pass. Live two-Mac host + late-joining guest passed authenticated held-gun shot, host collision/repulsion on both clients, unowned/repeated-shot rejection. A discovered pre-compression 4 KiB ceiling was separating host/guest after impact; uncompressed samples now allow 8 KiB while the compressed wire stays bounded at 4 KiB. Graphics/XRI both-hand grasp/fire/drop inspected. Editors muted; audible human voice and Quest checks remain pending. Evidence: `Docs/Verification/S5/Flare/`.
