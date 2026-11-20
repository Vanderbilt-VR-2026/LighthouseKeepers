# Ceiling crawl under the headlamp

The existing staged Day 2 crawl fixture now uses the keeper's actual automatic headlamp instead of a removed handheld flashlight. In the muted Editor, all 11 checks passed: the scene began, the light was active, a real wall and ceiling were found, the body moved and respected its safety bounds, obstacles shortened the crawl, the no-ceiling fallback worked, and the Quest-pipeline shader compiled. The fixture turns a simulated Editor head for these photographs; the game does not rotate a player's camera.

I inspected the [wall](wall.png), [ceiling start](ceiling-start.png) and [ceiling end](ceiling-end.png) images. The figure is visible in the beam but remains an obviously stylized, pale prototype; these still images do not establish convincing animation, scare timing or headset comfort. The nightmare stays **disabled** pending a complete unstaged night and worn acceptance. This was not a new APK or a device test.

Reproduce with the muted Unity Editor using `artifacts/s5/headlamp-nightmare/start.py`, then invoke the parent folder's `fixtures/CeilingCrawlProbe.cs` via Pipeline with a fresh absolute output directory. The fixture temporarily enables the nightmare in memory and restores the setting on teardown.
