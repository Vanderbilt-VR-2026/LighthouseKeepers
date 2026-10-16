# Fusion networking verification — October 13, 2026

Unity 6000.3.23f1; installed Fusion Runtime 2.1.3. All runs used the current checkout with the authored crew lobby and unchanged Photon settings.

- **Edit Mode: 51/51 passed**, final run. Includes original local lobby tests, Foundation/environment tests, nine protocol/roster tests, and three asynchronous session-manager tests, Photon-first scene ordering and explicitly configured local practice. See `EditMode.xml`.
- **Play Mode: 10/10 passed**, full suite with `LK_FUSION_CLOUD_TESTS=1`. See `PlayMode.xml`.
- Default runtime lobby: an unconfigured LobbyManager resolves to FusionLobbyTransport without adding a local simulation component.
- Real Fusion Single session: authored lobby resolves the real adapter, hosting creates a host row, ready enables launch, Fusion loads Bootstrap and all six content scenes, exactly one active NetworkRunner remains, the session survives board unload, and leaving returns to a disconnected networking lobby.
- Real Photon Cloud peers: shared host/guest names and ready state; late join receives the current roster and ready states; guest cannot kick the host; host kick removes the late joiner on all peers; host departure disconnects the guest and clears its roster. The test uses isolated physics scenes and a traffic-only scene manager, separately from the production scene-manager test.
- Existing Bootstrap authority/rig/audio checks and all six movement/footstep tests pass, including ascending/descending all three stair flights and the 30 FPS traversal path.

The NUnit XML files preserve result metadata and assertions; runtime `<output>` logs were removed to avoid publishing Photon connection diagnostics. Raw local logs are `/private/tmp/lk-issue33-default-editmode.log` and `/private/tmp/lk-issue33-default-playmode.log`.

Runs were headless on the Mac. No new Quest APK was built, and no headset, two-process scene launch, XR keyboard entry, graphical rendering, comfort, or sustained device/network performance was verified. Avatar poses, interactions, and gameplay state remain local. See `Docs/FusionNetworking.md` for independent-client and headset checks.

The main Play command and first build scene now open Photon by default. The standard development APK scene list includes the crew lobby before Bootstrap; recovery setup preserves that order. Local practice and direct environment entry are explicitly labeled Development tools. The historical Sprint 1 environment APK command remains a diagnostic path. No APK was built during this update.
