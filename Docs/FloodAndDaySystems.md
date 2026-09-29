# Flood and day foundation

FloodController exposes Height, Paused, RiseSpeed, SetHeight, ResetFlood and ResumeFlood. Its profile bounds the water to -0.25 through 10.5m. The default is safely paused and rises at only 0.003m/s on Day1 when resumed. An inspector height edit updates the water surface; reset pauses and lowers it safely.

FloodThreshold components at 0.15, 3.35, 6.55 and 9.75m emit a bool event only when crossing or receding. Warning indicators subscribe, and AcousticDirector chooses the emergency snapshot when water reaches the current room. FloodSurface follows the global height; it has no collision or fluid simulation. It currently demonstrates level coverage rather than realistic sealed-room flow or drowning.

DayDirector.ApplyDay(1..5) applies existing ScriptableObjects. Storm intensity, lightning spacing/proximity and flood rise speed are active. Machinery/radio/practical-light response components consume day data. Leak count activates up to ten authored ceiling drips. Puzzle-time-pressure remains extension data; no finished puzzle timer or survival calendar runs. Days1–5 deliberately increase pressure. Applying a day does not automatically start flooding.

The rise rate accelerates with height: base rate × (1 + surge-per-metre × height gained). `TimeToHeight` integrates that acceleration for a stationary target. It returns infinity when paused, when the base rate is zero, or when the target is above maximum water height. Already reached targets return zero. At maximum height or zero rise rate, Update stops publishing unchanged height events.

`FloodDangerMonitor` exposes safe/watch/warning/critical/submerged stages and time to head submersion. It is currently a component API, not attached to an authored environment scene or connected to a player-facing warning display. `FloodSurface` darkens toward its abyss color as global height increases.

Future authority would own selected day, flood height, paused/rise-rate state, failure decisions, and storm event seed/timestamp. The lobby has an `INetworkTransport` interface with a local simulation, but no real network transport or synchronized gameplay is installed. Use existing events when adding future game logic. See LobbyAndFloodIteration.md for the current preview and Quest checklist.
