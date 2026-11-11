# Step 3 — grip-only primer

The bulb is larger (10.5 × 14.5 cm), mounted at 1.20 m on the generator front with a retaining collar/backing, PRIMER stencil and existing brass tally. Grip pressure compresses it. 80% counts a squeeze; 25% or release refills/rearms. Sustained holds, noisy partial values and extra squeezes never add counts. Trigger activation is no longer subscribed. Existing EnginePrime host validation/snapshot counts are reused.

Focused tests: GripSqueezeTests 3/3, WatchEngineTests 6/6. Actual XRI graphics tests passed separately for left/right: three grip-only squeezes, long hold no repeat, visible compression, refill, fourth squeeze ignored, trigger never pressed. Inspected both screenshots. The first harness attempt failed because HandRig correctly cleared untracked input; the runner now feeds grip between HandRig and EngineControl updates, without changing game tracking. Host engine fault is initialized as a focused fixture; this is not a completed Day 2 replay or new two-client proof.

Editors muted; no listening/haptic comfort claim. Worn reach/contact, live guest priming and job-end full suites remain pending. See Docs/Design/InteractionInputs.md for the control audit.
