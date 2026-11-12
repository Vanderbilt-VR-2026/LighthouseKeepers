# Step 1a — integrated audit (November 11)

Source: 5cbdb23, branch s4/integrated. This audit precedes the full regression in Step 1b; it does not replace that run.

| Requirement | Existing implementation | Result / boundary |
|---|---|---|
| Generator versus POWER PANEL | GeneratorIdentity, WatchPowerPanel; separate yellow skid and green wall cabinet, over 4 m apart | Source and prior scene evidence verified; matching object-specific objective strings retained. |
| Island edges / sea consequence | IslandSafety perimeter rails, SeaRecoveryState | 80 rail segments; host observes keeper positions. Solo loss says Swept out to sea; crew recovery returns the fallen player after 10 s. Full regression queued. |
| Fuel | WatchEngineControls / FuelCan tilt and spout proximity | Physical pour; no click bypass. |
| Primer | GripSqueezeCycle, 80% squeeze / 25% release | One count per full squeeze, no trigger. |
| Choke | EngineControl hand displacement | 16 cm up/down detent; grabbing alone does nothing. |
| Starter | Existing starter arm travel | 30 cm pull/recoil, no trigger. |
| Lobby | CrewLobbyBoard / CrewLobbyTransport | PLAY → HOST / PLAY ALONE or JOIN CREW; START without READY; late join/rejoin and count-based rules retained. |
| Offline fallback | SoloLobbyFlowTests | Actual Lab load with no NetworkRunner tested with forced-unreachable transport. Real Quest Wi-Fi OFF remains DEVICE PENDING; no app launch authorized. |

No additional gameplay change was necessary in 1a. Preserve the accepted generator/hand poses and existing co-op protocol LK-S5-PLAYTEST1 / v21. The separate missing-lobby-hands report remains unconfirmed on a worn headset.

There is no device-equivalent GPU timing from the Mac Editor. Wiring has 13,894 triangles, five meshes and one material; ≤0.3 ms is a device measurement target, not a result. No target GPU claim will be inferred from Editor FPS.
