# Flashlights and Morse floor bulbs — #146

The two lanterns are now worn metal, right-angle flashlights. Hold GRIP around the ribbed handle; press TRIGGER to switch the focused beam on or off. There is no LOW mode, fuel drain, oil can or oil gauge. This applies to Campaign and Survival on every night. The beam follows the tool, not the head. Existing drops, hooks, ownership, underwater switch-off and held-light repulsion remain.

The grip was checked against both animated hand meshes. An initial attachment passed through the fingers; the final attachment uses the measured finger cavity rather than the generic tool contact. No global hand pose, avatar mesh or other tool attachment was changed.

One supported hanging bulb remains in the House and on each floor. House/F1 have backup power; F2–F4 require the LIGHTS circuit. Slow, softened Morse patterns are HELP ME, HELP ME at a different rate/phase, 666, YOULL DIE HERE and DEATH. The House stays at least 60% bright so instructions remain readable. Upper fixtures were raised above their equipment and the five bulb clearance probes found no collider overlap within 8 cm.

## Delivery

APK source **335a1ff0**, IL2CPP ARM64 / normal package, installed on Quest 3 with matching device SHA-256; **not launched**. SHA-256 `f5a72ba43be465fd3eff8a20d1fd458ee37510f90607da06588e499ce69ff532`. [Delivery receipt](delivery.json).

APK: `Builds/LighthouseKeepers-Sprint5-Flashlights-335a1ff0.apk`.

## Executed verification

| Evidence | Result | Limit |
|---|---|---|
| Focused EditMode | 78 passed, 0 failed | Lantern, mode/snapshot, runtime, voice/catalogue and Quest baseline checks; not the full suite |
| Survival Day 1, actual XRI manual input | 12 checks passed: both-hand pickup, contact, mesh clearance, trigger ON/OFF and release | Editor simulation, not worn controller acceptance |
| Campaign Day 2, rendered Play Mode | 11 checks passed: two tools, five bulbs, beam reach/direction, baked-pool dimming, shader and light count | Controlled fixture, not a full watch or live two-player session |
| Lighting bake | Powered 8.6 s, emergency 8.0 s; 444 probes, two maps per state | Mac Progressive GPU bake timings, **not Quest frame times** |
| Screenshots | Left/right/front/top grip, near/far beam, dim/bright House and F3/F4 mounts inspected | Both-eye readability and moving-hand clipping need the headset |

The beam is a 28° spot with a 16° inner cone, 9 m reach and intensity 36. It uses a feathered 64×64 cookie and no shadow map. Far-target centre luminance rose from 0.00177 to 0.06712; pointing away returned it to 0.00177. Near surfaces are bright and need a headset check for glare. Two active flashlights use two realtime lights; fixed bulbs add none. Baked pools dim through a shared material-per-source URP shader, leaving direct flashlight light unaffected. 2,211 lightmapped renderers used that shader in the captured scene. Sustained Quest cost and light leakage around walls are NOT VERIFIED.

All Editor runs were muted. Network **LK-S5-FLASHLIGHT1**, transport **LK-Lab-Coop-v51**. No snapshot fields added; the lobby version changed because both players need identical unlimited-power and ON/OFF semantics. State replication tests passed; new live voice/co-op acceptance was not performed in this slice.

Raw receipts are in [tests](tests), [grip checks](grips.json), [rendered checks](rendered-checks.txt), [mount clearance](mount-clearance.txt) and [bake report](bake.txt). Capture scripts are preserved in [fixtures](fixtures); they modify only a temporary Play Mode scene.

## Reproduce

Open this worktree with Unity 6000.3.23f1. Set `EditorUtility.audioMasterMute = true` before every test or Play Mode run. From the repo root with the existing Pipeline bridge available:

```sh
python3 artifacts/s5/modes/check.py LanternTests LanternLightingTests SurvivalModeTests M3RuntimeTests VoicePassTests StoryImportTests QualityBaselineTests
```

For grip captures, start offline Survival Day 1 (`artifacts/s5/interaction-fixes/start.py`), then use Pipeline `run_script` with `Docs/Verification/S5/Flashlights/fixtures/FlashlightGripCheck.cs`, entry `FlashlightGripCheck.Run`, and a fresh absolute output folder. Stop Play Mode. For beam captures start Campaign Day 2 (`artifacts/s5/scare-scenes/start.py`) and invoke `FlashlightVisuals.Begin` from the matching fixture with a fresh output folder. Never run the fixtures concurrently. `BulbPhotos.Begin` captures the supported fixtures from their room side.

## Six-step headset check

1. Start Survival Day 1. Confirm the House instructions and F1 bulb are visible, and locate the two flashlights by the House door.
2. Pick up a flashlight in each hand in turn. Look at the palm, thumb and fingers; rotate the wrist and check the handle stays inside the grip without penetrating the hand.
3. Press TRIGGER: one bright beam. Press again: off. Aim across F2 at the generator, then at a nearby wall; report reach and any uncomfortable glare.
4. Drop, regrip and return it to a hook. Leave it on while working: it must not drain or disappear. Submersion still switches it off; retrieve and toggle it on again.
5. Restore LIGHTS power. Check one bulb per floor, each flickering differently, with no floating fixture or blocked walkway. Report any uncomfortable flicker or unreadable instruction.
6. Join a partner using the same build. Each take one flashlight; check remote pickup/drop and both beams. Sustained 72 Hz / GPU ≤11 ms profiling with both beams and peak storm is still pending.

## Source attribution

Flashlight geometry, Morse sampling and the baked-pool modulation are original project code. `RoomFilamentLit.shader` adapts Unity URP 17.3's installed `Shaders/Lit.shader` and includes Unity's existing lighting implementation. Copyright Unity Technologies ApS; [Unity Companion License](Unity-Companion-License.md). No downloaded art, audio or new AI narration was added.

Day 2 nightmare #144 and Day 3 cursed ship #145 remain pending; this delivery does not implement those scenes.
