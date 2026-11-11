# B recall teaching / visible wiring correction

The Quest's installed APK was pulled and hashed: it matched f5d2746 exactly. The device capture was black, so it is not evidence of headset appearance. The lead's feedback is treated as a real visual/teaching issue.

## Changes

- The old tutorial marked itself taught after a single five-second flash. Now the new per-player acknowledgement is written only when right-controller B is actually pressed. Old `BTaught` flags do not suppress it. Before acknowledgement, the reminder appears at arrival/objective changes on any night; it does not keep the HUD always on. No repeated preference writes after the first B use.
- Larger **OBJECTIVE · B — RECALL** header whenever the transient tracker is visible. **Press B: show current objective** underneath until taught. The fixed House board says **B: OBJECTIVE** beside the night number, so the instruction remains findable after the HUD disappears. Existing tap/hold/double-tap, X/Y, wrist glance and placement remain.
- Compared with f5d2746, main wiring is about 29% slimmer (.60/.85 radius), with smaller straps and a much darker warm oxide finish. All steady emission removed; restoration pulse retained.
- Texture coordinates now repeat every 35 mm instead of stretching the small atlas patch over a whole run. Separate cloth and aged-metal patches remain in one atlas. Explicit texture gradients keep mip selection continuous at tile boundaries; stencil UVs remain fixed. No new texture sample, renderer, light, collider or synchronized state. Total remains 14,634 triangles / five meshes.

## Checks

Four recall tests pass, including an upgraded install with the old taught flag set, repeated objective changes until actual B acknowledgement, and no reminder on an unchanged objective. Three wiring tests pass after the rebake (host/guest feed state, rejected guest writes, generated geometry/material budget). Graphics Night 1 → Night 2 passes with actual XRI clamp/bolts, both hands, wrench drop/recovery, drain and sleep. No gameplay changes were made. Editors muted throughout.

Inspected screenshots: reminder + fixed board, powered before/after, labelled panel, close junction, blackout/pulse. Generated-route audit passes with no unexpected contacts. A capture-script output-directory error was fixed without game changes. The final shader's tile-gradient correction was re-rendered and inspected after the gameplay replay.

No new live transport session for this local presentation pass; the previous activation checkpoint has the two-Mac shared-task/bed result. Network stays **LK-M3-CAMPAIGN1 / v20**; five-night task preview remains ON. Ship/finale/progression work is still pending.

## Device pending

Human B discoverability, wiring appearance and texture stability in both eyes, sustained GPU cost (≤0.3 ms wiring target), and Mac+Quest human checks. Build/install receipts added after completion. Do not launch the Quest without the lead asking.
