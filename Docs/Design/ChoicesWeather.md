# Priorities and natural storm motion — November 16

The latest playtest request supersedes the staged “finish one job, unlock another fault” campaign pacing. Night 1 remains the protected pipe tutorial. Nights 2–5 and Demo now author their opening station failures together at clock zero. Night 2 starts with a stopped, nearly empty generator and leaking pipe; Night 3 also needs the beacon and radio. Power dependencies remain physical: you can repair the pipe before the generator, but the pump/radio/beacon need electricity. Neither job completion breaks or unlocks another station. The full board shows the outstanding jobs, and the director recommends a current priority without preventing a different repair. Later timed refuel/climax events remain authored night events, independent of completing an objective.

The night lengths, ship arrival windows, flood rates, first-station grace, Night-1 no-fail and accepted arm-pull repair are unchanged. Night 2 no longer queues the contradictory “Pipe first” announcement. Night 3 opens with the existing recorded “Tonight it's everything” line. No voice recordings/captions were altered. Initial fault receipts still support the existing delayed, view-safe Drowned reveals; no first-minute or Night-1 scare was added.

Both clients require **LK-S5-CHOICES1 / LK-Lab-Coop-v32**. The new flare receipts required a schema version change; this further identity separates the new simultaneous opening rules from the previous staged build. No client owns simulation state.

## Weather and gallery

Rain, sky clouds, ocean waves and puddle rings previously multiplied the absolute elapsed shader clock by current storm intensity. Changing intensity therefore jumped their animation phase, causing the reported fast-forward effect. Their clocks now run at steady rates. Density, wave amplitude, cloud cover and sound can still intensify; rain strength eases over ten seconds, ocean/sky intensity retains its existing twenty-second easing. This changes the existing sky's motion only, not its art.

The two unsupported shell gallery lamp assemblies and their eight cable runs were removed from the active prefab, saved wiring routes and authoring recipes. The wiring was rebaked: 13,262 triangles, five meshes, one shared material. No other tower circuit was removed.

Exterior puddles use a darker water tint and restrained probe reflections. Eight supported patches are added to the F4 gallery; their height compensates for the gallery render surface's existing offset from collision. Current scene: 23 puddles, 3,752 triangles total for rain plus puddles, no additional realtime lights/reflection passes. These counts are **not** a device GPU measurement.

## Evidence

79 focused cases pass after updating only assertions contradicted by the approved simultaneous-fault design and correcting an existing stale duration assertion. They cover either job order, retry, initial faults across all nights, real power/fuel, host snapshots, Drowned receipts, flare authority and music assets. Actual-XRI solo Night 3 passes pipe, physical generator controls, beacon, radio, both steered ships and bed into Night 4, errors 0. Its fast scripted inputs are functional regression evidence, not human first-timer timing approval.

Graphics reviewed: gallery puddles, exit fixtures, sea, exterior puddles and Night-3 ship passage. Muted two-Mac final verification and APK delivery are recorded in Next.md. Headset feel, auditory loop acceptance, worn stereo/weather motion and GPU budgets remain device checks.

A repeat live flare test correctly hit scenery when its preselected firing position put the generator in the way. Both clients agreed on the blocked impact; the fixture now selects a supported, obstruction-free position using the same collision classes. Gameplay obstruction checks were not weakened. Earlier failure receipts are preserved under `artifacts/s5/finish/two-mac-5/`.
