# C4 water and coast

Water remains a presentation consumer of the host's accepted height, House volume, day and storm stage. No snapshot fields or gameplay tuning changed. The ocean uses four Gerstner displacements with a sparse distant grid, dual normal detail, crest foam and reflection probes. Flood uses dual scrolling normals, contact ripples and five noncolliding timber fragments. Night-scaled doorway puddles and wall streaks supplement the existing doorway flow simulation. Rain particle/bed presentation ramps from drizzle on Night 1 to heavy rain on Night 3.

The active jet follows the active bolt and inflow fraction. Existing bounded Unity particles now use small soft translucent droplets rather than opaque rectangular streaks; the scrolling jet is GPU shaded. These are conventional Unity particles, not a compute-based GPU particle system. Rock spray is a bounded 48-particle pool, emitted with spatial wash at wave peaks. The combined rocky inlet is decorative, with no new unsafe walk route.

31 focused tests and the physical Night 1 repair/drop/recovery/sleep replay pass. Reviewed water/jet screenshots and ocean-gallery view; the low exterior capture was obscured by a nearby surface, so it is not evidence of a clear coastal walking route. Raw evidence: `artifacts/s4-master/part-c/c4-*`.

Device pending: total water ≤2 ms, soft transparency/MSAA appearance in both eyes, frame/draw/triangle budgets, physical contact ripple acceptance. No target performance claim is made.
