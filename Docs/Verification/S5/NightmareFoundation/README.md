# Nightmare foundation — #144 (not released)

67 focused EditMode checks passed with the Editor muted. The ten scene checks used a test-controlled camera/rig and direct door-state calls; they are not hand-input, multiplayer or headset acceptance. Child path used the actual authored stair NavMesh and collision sweeps. Scene clock advanced while watch time stayed fixed. Snapshot, solo escape, retry and co-op rescue rules have unit coverage.

The feature stays OFF in HorrorTuning until physical door/revive, host/guest, visual and checkpoint checks finish. No APK is associated with this intermediate commit. Reviewed wake, turn, ceiling and outside images in `artifacts/s5/final-scaring/play-1/`; the ceiling capture missed the child. The stale normal-task HUD location shown there has been corrected. Further visual review required.

Day 3 #145 and Day 4–5 #148/#149 are not implemented. Source/steps are in `Docs/Design/Next.md`.
