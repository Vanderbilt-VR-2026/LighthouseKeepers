# C to A — geometry and ship cleanliness

STN_MANUAL_PUMP on F1: root (0.450,0,1.250), yaw90; PIVOT_PumpHandle local (0,1.050,0.380), +X axis; GRIP_PumpHandle is a pivot child at (0,0,0.250). Use -22.5..+22.5 degrees around local X. Radius0.250 and arc45 degrees give **0.191m vertical hand travel**, not the conflicting 0.35m target. Please bind these anchors rather than moving the station. GeometryIntersectionValidator checks the 1m clearance volume and sampled hand sweep once Unity can run.

PumpDischargeEmitter on STN_MANUAL_PUMP exposes EmitStroke() and Surge; call only after an accepted down-stroke. No simulation or interaction event is authored by C. VFX_PumpDischarge is the visible floor outlet.

The scene wrench retains its existing XRGrabInteractable. C replaced its mesh and physical box (0.12,0.035,0.35m), added a separate generous **trigger** box (0.24,0.16,0.46m), and placed it at the near edge of the0.90m workbench. A owns final selection, collision list and hand behavior.

**Cleanliness blocker:** Assets/_LighthouseKeepers/Prefabs/Player/LK_QuestPlayer.prefab contains built-in Capsule/Sphere/Cube renderer meshes (19 serialized references). Ship validation checks inactive renderers too; hiding them is insufficient. Please replace hand-owned meshes with your authored assets. C has not edited the shared player/hand prefabs.
