# Reference keeper verification — November 16

The user requested a Blender keeper combining historical workwear proportions with the illustrated coat/cap/scarf cosmetics. The older fisherman remains available; local tracked hands are unchanged. Network stays LK-S5-DREAD1 / LK-Lab-Coop-v33.

| Evidence | Result | Limits |
|---|---|---|
| Existing rig/role baseline | 14/14 | Focused, not full suite |
| Final avatar/IK/role/voice acoustics | 37/37 | No recorded human voice |
| Final local mesh asset check | 6/6 | Retest after waist geometry correction |
| Unity graphics pose review | Pass, zero errors | Monoscopic Editor views |
| Two live Mac clients | Join, both skins, remote crouch/wrist, guest physical pipe, both bed votes → Night2 | Muted; Night1 valve not required |
| Solo Night3 actual-XRI replay | All stations, two steered ships, sleep → Night4; zero errors | Automated, not first-timer timing |
| Quest | Installed; SHA-256 matches | Not launched; stereo, comfort and GPU checks pending |

The Blender and Unity screenshots here were visually inspected. Initial material/shoulder and local waist-cap issues were corrected before these final views. The legs are inferred from head/controller tracking with planted feet, alternating turn/walk steps, crouch knees and supported landings; they are not tracked feet or a physics ragdoll.

The full skin is 22,739 triangles, 51 bones, one material. The local skin is 11,261 triangles; its head/gloves/camera-adjacent collar are excluded. 1024px maps use mipmaps and Android ASTC. See `../../../Design/HarborKeeperAvatar.md` for authoring details.

Reproduce from `~/LighthouseKeepers-M3`: focused runner `artifacts/remaining/m3/tests.py`; graphics `artifacts/s5/keeper/review.py`; two-Mac `crew-driver.py`; solo `solo.py`. Use fresh evidence directories, preserve failed fixture receipts, and mute every Editor. The first co-op fixture tried sleeping before the authored preview ship; the corrected fixture waits for that event instead of changing gameplay.

## Headset checks pending

Look down and crouch without a floating board/extra gloves; check sleeve seams and watch, walk/backstep/turn/climb beside a friend, confirm grab/hand alignment and rejoin, profile at 72Hz with the ship/storm. Human two-way voice also remains pending. Do not auto-launch the headset.

The first final-asset invocation failed before tests started because Unity could not connect to its local licensing/package services. Restarting the stale service allowed the unchanged retry to run: 6/6 passed. This was a tool startup failure, not a game-test failure.

## Delivery

**APK built from clean `7c02e858`**, Unity6000.3.23f1 development IL2CPP ARM64; zero build errors. `LighthouseKeepers-Sprint5-Keeper-7c02e858.apk` (123,874,874 bytes), SHA-256 `909c0a37aabf81baed334ad6ed041114cb7a1dc10dff9c349e2108c82679bc2e`. VR manifest/ARM64/signature/ZIP verified. Installed on the connected Quest 3; installed SHA-256 matches. Not launched.

The legacy asset test expected captain and guest to be different costume assets. That expectation intentionally changed to the newly requested common keeper design; validation for both role slots remains, and role/ownership tests are unchanged. The old assertion failed as expected (3/4); the updated contract passed4/4 in the peer worktree. Both receipts are retained. This is an art-contract update, not a gameplay workaround.
