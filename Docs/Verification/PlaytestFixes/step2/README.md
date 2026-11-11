# Step 2 — pole-remnant audit (device report not reproduced)

The saved Lab scene/prefabs contain no separate old pole/slide geometry. The live scene has exactly one continuous shaft, three landing assemblies and one F3 hatch/fall-support collider. The obsolete `Brass slide shaft` name is absent on every floor. Source is `WatchDemo/Runtime/PoleSlide.cs`; no competing pole builder was found. Its dark brass floor liners are non-emissive; the regression asserts this and the assembly counts.

Inspected `f3-before.png`: current shaft, grey flush grate and side-mounted plate beside the bunk. I did not reproduce a separate yellow remnant here and have not deleted the working pole or safety hatch speculatively. A location clarification is pending; exact removal remains a DEVICE CHECK, not a claimed fix. No production pole changes in this commit. Focused PlayMode audit 1/1 passes with Editor audio muted.
