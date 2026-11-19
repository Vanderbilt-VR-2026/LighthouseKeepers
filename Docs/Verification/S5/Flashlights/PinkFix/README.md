# Quest pink-environment correction — #146

The user reported the whole environment rendering pink in APK `335a1ff0`. That build introduced a runtime replacement of 2,211 lightmapped environment materials with `RoomFilamentLit`. Its Metal Editor captures and Vulkan build compilation passed, but neither established correct Android XR runtime rendering. The available device log contained no shader diagnostic and the attempted capture was black, so the exact missing variant/driver cause is not established.

The correction removes the custom shader, its Resources asset and the material-replacement component entirely. Authored URP/Lit environment materials remain untouched. The actual bulb surfaces still pulse in Morse; baked illumination pools stay steady between powered/emergency state changes. This intentionally defers pool flicker until it has Android XR acceptance. Flashlight shape, both-hand grip, ON/OFF, unlimited power, focused beam and host-owned gameplay are unchanged.

Validation: seven focused EditMode checks passed, including removal of the replacement shader from Resources and the Quest quality baseline. The muted Day 2 Play Mode fixture passed 21 checks: no error-magenta pixels in ten captures, all 2,211 lightmapped material entries supported and unchanged across updates, focused beam working, and two realtime lights. House and F2 captures were inspected. These are Editor checks, **not proof of corrected worn Quest rendering**. The replacement must also be absent from the packaged build's used-asset report before delivery.

Reproduce with `python3 artifacts/s5/modes/check.py LanternLightingTests QualityBaselineTests`. Start offline Campaign Day 2 with `python3 artifacts/s5/scare-scenes/start.py`, then Pipeline `run_script` for this folder's `FlashlightVisuals.cs`, entry `FlashlightVisuals.Begin`, with a fresh output folder. Keep the Editor muted.

Build/install uses fresh scripts `artifacts/s5/pink-fix/{build.py,deliver.py}` and a new `build/` backup. Install only; never launch the app. Network remains **LK-S5-FLASHLIGHT1 / LK-Lab-Coop-v51**, because no gameplay or synchronized data changed.

APK **592673ef** built, installed on Quest 3; installed SHA-256 matched, **not launched**. SHA-256 `97b7ea2d2a5c11fc897e75cc648d8024279e58a3bfeae3d5205d928a49a1224e`. [Receipt](delivery.json).

After installation, reopen the game manually: check the House walls/floor and the F2 generator in both eyes, then switch the flashlight on and aim around. Corrected headset rendering and sustained performance remain pending that check.
