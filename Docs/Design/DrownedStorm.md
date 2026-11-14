# The Drowned owns the storm — November 14

Latest direction: make the Drowned the central antagonist and let the weather communicate its intentions. Preserve the accepted repair inputs, wheel-guided ships, hands, voice, watch and co-op. No skybox or avatar edits. Editor audio muted; no Quest launch.

## Order and acceptance

1. Resolve the two-keeper timing conflict. A captain can hold outside the danger window for a bounded time while both keepers finish valve+wrench repair. Night/flood clocks continue; once a ship enters the rocks it cannot get this protection. Same host state controls travel and both views. Enable sabotage only after all 12 existing workload models pass.
2. Make the Drowned visible and consequential: one host-owned sabotage/appearance lifecycle, darkness encourages it, repairs/light force retreat, evidence tells what it did. Reuse TensionDirector instead of creating a competing weather scheduler. Never spawn a threat in the tutorial or without warning/recovery. Comfort options remain required before hunts are enabled.
3. Weather communicates the lifecycle: quiet before its arrival, rain/wind/ocean rise during sabotage and pursuit, relief after it retreats. Use the existing replicated storm stage. Remove independent day-only weather floors that prevent relief. Do not modify the skybox asset.
4. Rain should read as heavy falling water, not a few bright dashes. Use bounded near-camera exterior streaks plus a sparse distant shell; shelter masks prevent indoor rain. No depth/opaque copies, new shadowed lights or per-drop physics. Grounded irregular puddle meshes show wet highlights and analytic ripples outside; one shared material, no screen-space reflections. Existing rain audio zones and voice-first mix stay.
5. Focused rules/replication tests,12 task/travel models, inspected graphics views and live two-Mac regression. Quest GPU/CPU costs are pending worn-headset profiling; geometry/voice/particle bounds are estimates, not GPU measurements. Build/install only; leave app closed.

Any unfinished milestone remains explicit in Next.md. Commit/push each completed step. No changes to main.

## Implemented boundary

The Drowned now fronts the fault loop: spatial pre-sound, wet traces, a host-owned warning/manifestation/retreat and storm escalation. Original hooded oilskin mesh,692 triangles, one material, no light/collider. Night1 is protected; no appearance before60s, during an unintroduced control or its grace, or during a recovery window. Night2 allows at most2 sightings; later nights3, with60s spacing. Light restoration/repair ends the appearance; dark sightings last at most12s. Watch Menu → Sound settings → Drowned sightings can hide the form and its breath per player; offscreen fault rules stay the same.

This is a NON-CONTACT SABOTEUR/APPARITION slice. No NavMesh hunt, catch, lantern stun, hiding or extra damage is enabled or claimed. Those remain phases4–5. This makes it the visible cause of the current machines failing without enabling a hunter before escape tools exist.

One network version: **LK-S5-DROWNED2 / LK-Lab-Coop-v29**. New host frames carry offshore hold and Drowned phase/event receipts. Rain/puddles consume the existing storm value; no second weather authority.

Storm: warning dips existing background audio; dark manifestation raises the stage to4; retreat/repair eases it. Time, water and ship danger still provide the safety-critical pressure floor. Removed day-only ocean/rain intensity floors so a repair can actually ease later-night weather. Sky assets untouched.

Rain:1600 wrapped vertex-animated streaks, analytic shelter rejection matching the shell, no per-drop CPU/physics. Replaces the existing rain particles and their per-frame particle scan.12 ground-supported irregular outdoor puddles in the current shell, combined mesh with probe highlights and asynchronous impact rings. No depth/opaque texture, reflection camera, realtime light, new post effect or collider. Rain+puddles3488triangles,2 renderers; Drowned692triangles/1renderer. Shader overdraw and stereo appearance require Quest measurement; no GPU cost claimed.

Original mono44100Hz breath: `Assets/WatchDemo/Resources/S5Audio/drowned_breath.wav`; deterministic filtered noise(seed84)+67/102Hz quiet resonances,3s envelope, no sampled/third-party content. Existing four sabotage sounds and voice-first mixer retained.

Validation:95 focused cases, one initially stale cancellation fixture corrected to call the real receipt cancellation; all7 Drowned cases then pass.12 instruction/travel models pass; rendered Night2 outage/appearance/restoration passes, screenshots personally inspected. Live transport and APK receipts are appended to the verification README when complete.
