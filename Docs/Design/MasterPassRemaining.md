# Master pass — remaining acceptance after Part C

Implementation checkpoints and exact commands are in `Next.md`. C0–C6 have received their implementation/art pass on `s4/visuals` / draft PR #82, stacked on Part B PR #81. Neither PR is merged by the agent. Actual final test/APK evidence belongs in `Docs/Verification/S4Master/part-c/final/`.

## Device gates (all pending)

1. Profile the active Android URP asset at MSAA4x, render scale1.25, fixed foveation0.5. Budget: GPU≤11ms, mainCPU≤9ms at72Hz, draws~150/eye, ≤300k visible triangles, ≤24 audio voices, water≤2ms. 1.25 scale means 56.25% more pixels than1.0, not a measured GPU cost. Set final scale from these results.
2. Continuous headset wall/door/stair sweep; text shimmer in both eyes; glass/reflections/rain and water transparency. Enable a HUD-only composition layer only if the SDF presentation still shimmers.
3. Physical wrench/grip and bolt movement, hand brightness, pole comfort, stair smoothness, bunk and full exterior walk boundary.
4. Mac+Quest crew: avatars, audible two-way mono voice, task replication, split objective/partner line, remote pole, shared sleep and rejoin.
5. First-time tester Nights1–3. The existing bot timings are scripted/modelled, not a human usability measurement. Confirm one instruction, danger comprehension, status sentence and 30–40% timing headroom.

## Deliberate art limits / follow-on plan

See `M2-Plan.md`: current keeper props are low-poly originals; improve custom bevels/coat/stove and consolidate station draws after profiling. Current contact foam is analytic, not bespoke around every prop. Conventional bounded droplets are used instead of a new compute framework. The rocky inlet is decorative; a longer exterior route must gain supported terrain and guards before opening. Hand-placed salt/soot/splash decals and per-prop water foam remain refinement work. Current AO comes from material masks and the bake, not a new vertex-colour AO asset pass. Do not report those refinements as completed.

## Preservation

Skybox and avatar body belong to Will. All gameplay state remains host-owned. Network `LK-S4-VISUALS-C0` / snapshot v15 requires matching clients. Default snap turn, hands/poses, accepted stations, voice processing, flood rules and day balance are preserved. Night1 is a no-loss tutorial. F4→F3→F2 pole descents use safe existing openings; F2→House remains stairs. Do not invent a continuous shaft through the next stair flight.
