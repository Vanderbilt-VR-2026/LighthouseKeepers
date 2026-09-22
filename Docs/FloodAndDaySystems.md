# Flood and day foundation

FloodController exposes Height, Paused, RiseSpeed, SetHeight, ResetFlood and ResumeFlood. Its profile bounds the water to -0.25 through 10.5m. The default is safely paused and rises at only 0.003m/s on Day1 when resumed. An inspector height edit updates the water surface; reset pauses and lowers it safely.

FloodThreshold components at 0.15, 3.35, 6.55 and 9.75m emit a bool event only when crossing or receding. Warning indicators subscribe, and AcousticDirector chooses the emergency snapshot when water reaches the current room. FloodSurface follows the global height; it has no collision or fluid simulation. It currently demonstrates level coverage rather than realistic sealed-room flow or drowning.

DayDirector.ApplyDay(1..5) applies existing ScriptableObjects. Storm intensity, lightning spacing/proximity and flood rise speed are active. Machinery/radio/practical-light response components consume day data. Leak count activates up to ten authored ceiling drips. Puzzle-time-pressure remains extension data; no finished puzzle timer or survival calendar runs. Days1–5 deliberately increase pressure. Applying a day does not automatically start flooding.

Future authority would own selected day, flood height, paused/rise-rate state, failure decisions, and storm event seed/timestamp. This document identifies state only: no networking interfaces or dependency have been installed. Use existing events when adding future game logic.
