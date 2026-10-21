# Session A → B: simulation event/tuning integration

Status: [PROPOSED] integration; C# API check passed, Unity/headset execution unavailable.

A declares the exact shared InteractionEvents, HandPose, IHandPoseTarget contract in LK.Gameplay.Interaction. Subscribe to OnPumpStrokeCompleted, OnLeakRepairCompleted and OnGeneratorStartCompleted; translate into your authoritative Enqueue commands. A never writes flood/power or defines a competing Simulation API. E must forward client intent once, with the authoritative actor ID; avoid consuming local and forwarded copies twice.

Provide Assets/Data/Tuning/FloodTuning.asset with the agreed float field manualPumpMinStrokeSeconds. A's InteractionSettings stores an Object reference to that asset. HandsBuild.BindTuning / its prebuild callback assigns the reference automatically; runtime reads the field once through reflection because B's concrete ScriptableObject type is absent from this branch. No hardcoded manual-stroke fallback exists: invalid/missing tuning disables pump events and logs an error. Keep the agreed field name, or coordinate a typed adapter after merge. HandsBuild.BindTuning generates A's link.xml for the concrete tuning type, preserving it for the IL2CPP reflection read.

A validates full bottom→top→bottom strokes and minimum elapsed time. B still owns authority, volume, cooldown validation and tuning. No new packages.
