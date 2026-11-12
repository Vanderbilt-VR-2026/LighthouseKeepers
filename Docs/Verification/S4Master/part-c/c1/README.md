# C1 wall visibility

Authored wall materials were opaque but backface culled (Cull=2). Bounds audit with a 1 mm tolerance found all inspected wall vertices contained; no stale bounds repair was needed. Wall surfaces now render both sides, retain opaque depth writing, and final wall LODs do not cull out. Runtime camera near clip 0.04 m and occlusion disabled remove close-view/cached-occlusion dependence. Skybox and avatar body unchanged.

72 graphics captures cover House, F1–F4, three stair flights and gallery at 45-degree increments. Contact sheets visually reviewed: wall enclosure remains present; no missing room sectors observed. This is discrete Editor coverage, not a guarantee about sub-frame headset motion. Existing reflective lighting artifacts remain for C3 bake, and the opaque lantern rings are addressed in C5. Device 360 sweep pending.
