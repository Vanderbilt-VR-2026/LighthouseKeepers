# Verification checklist

Automated outputs are in Verification/. Keep their exact run context; desktop renders do not prove headset comfort or mobile frame rate.

On Quest 3:
- Confirm Lighthouse Keepers launches under com.lighthousekeepers.game and both tracked hands respond.
- Walk using left stick, snap-turn with right; test walls, the house doorway, all three spiral ascents/descents and balcony railings.
- Check standing/seated reach and floor height. Physically leaning through a wall still needs a comfort/safety review.
- Grab/release the wrench, oil can and radio with each hand; feel haptics where supported.
- Verify both-eye vignette during movement and smooth rotation; it must fade completely at rest. Toggle intensity/off in ComfortSettings and rebuild if needed.
- Listen in house, each floor and balcony: interior rain must differ in timbre, and thunder must be muffled inside. Assess placeholder levels before replacing them.
- Preview flood manually and reset; apply Days1 and5 and compare lightning, sound and rise rate.
- Profile CPU/GPU frame times on device for at least 10 minutes, including lantern vista and flood. 72Hz requires frame time below13.89ms with thermal headroom. Desktop FPS is not accepted evidence.
- Check no pink/missing materials, stereo artifacts, head clipping, snag points or visible debug labels.

The source image was unavailable. Review silhouette, colour and prop quality against the actual concept before accepting art direction as matched. Do not mark subjective checks complete based only on logcat.

## September 23 asset integration checks

Automated evidence: `Verification/IntegratedAssets.txt`, `PlayMode.txt`, `DesktopWeatherPlayMode.txt`, `CirculationPlayMode.txt`, `EditMode.xml`, `AndroidBuild.txt`, and `AssetIntegrationValidation.md`. The current integration's APK must be tested separately from earlier headset fixes.

1. Open `Assets/_LighthouseKeepers/Scenes/Bootstrap/LK_Bootstrap.unity`, enter Play, and wait for six additive content scenes. Use WASD to walk and Q/E to snap-turn. Check the worn entrance desk, motor assembly, drums, utility cabinets, radio desk and overhead fixtures.
2. Walk the house centerline and both widened tower/lantern doors. Sampled usable door span is 1.29m (previously 0.89m). East Level 3 landing clearance is approximately 1.21m. Check controller-hand reach, not just head clearance.
3. Walk around the full lantern balcony: doorway, opposite side, edges and both sides of the ring. Roof overhang shelter is intentional. Balcony rain must activate outside and clear inside; window rain remains outside. Check Days 1 and 5, both eyes, boundary popping and transparent overdraw.
4. Look at the sea from the house windows, each tower window, lantern and balcony. The Quest fallback is dark, opaque and animated. The supplied water material is an editor comparison only. Look for distant grid/horizon artifacts and shoreline gaps.
5. Offshore fog must stay outside, low and restrained. Three emitters are at least 20m from the tower; no emitter belongs inside a room. Check whether sprites look like smoke sheets or create uncomfortable close billboards.
6. Listen for one persistent ambience bed beneath weather/machinery. Exact source: `Assets/free horror ambience 2/ha-pressure-nofx.wav`; edited loop: `ThirdPartyVariants/Audio/LK_Pressure_SeamlessLoop.wav`. Audition for voices, stingers, musical peaks and the loop join (approximately every 110.64 seconds). Selection was based on filename, duration/level analysis and a controlled crossfade; human listening has not been replaced by those checks.
7. Tune `AmbienceMusicVolume` in `LK_Atmosphere` (initial -20 dB), hear the startup fade, thunder ducking and existing interior low-pass transitions. Confirm radio information remains intelligible. Current weather/machinery/thunder recordings are still procedural placeholders.
8. Build/install the new APK using README instructions. Verify both tracked hands, left movement, right snap turning, optional smooth turn, vignette, grabbing, stairs, flood reset and puzzle reach. Prior user acceptance of the repaired build does not validate this new content.
9. Profile at least ten minutes on Quest 3, especially the balcony, fog/ocean vista and rising flood. Record CPU/GPU frame times and thermals. 72 FPS, final loudness, visual quality and stereoscopic comfort are **not yet verified**.
