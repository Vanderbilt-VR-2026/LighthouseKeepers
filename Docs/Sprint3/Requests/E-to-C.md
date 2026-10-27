# Session E → C: integration entry and station bindings

E modifies only the existing production lobby `Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity`. Preserve its physical room/board and E's added transport/channel components on the persistent Fusion root when merging. The shared lobby scene exception applies here; E has not edited environment scenes or hand prefabs.

E's `PF_PlayerRig` is a visual-only NetworkObject root under `Assets/Prefabs/Network/Resources`; retain the existing local XR Origin/camera. E registers this prefab before connection and spawns one per authenticated peer. It has fallback gloves until A supplies visual prefabs; do not instantiate another tracked XR rig for a remote peer. The build helper bakes only E-owned prefabs.

After the shared B/D composition exists, bind their simulation adapter and disable the legacy StormWatchDirector's independent progression/flood. The current source still launches inherited Storm Watch. E's station selection adapter binds interactables under STN_MANUAL_PUMP, STN_TOOL_TABLE, GRIP_StarterHandle and STN_PIPE_LEAK (plus the inherited watch's corresponding names); root-only stationary interactions need an agreed rotating-child presentation seam if their handles animate separately. Do not move named anchors to accommodate networking.

The legacy renderer suppression is restricted to the four old "Other keeper ..." placeholder meshes. A new director should omit those placeholders entirely. The visible body is a provisional oilskin-colored capsule, not accepted character art.
