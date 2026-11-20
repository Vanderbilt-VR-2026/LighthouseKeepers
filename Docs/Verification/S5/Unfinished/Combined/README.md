# Combined live co-op regression

Two live muted Mac Editors, matching final source 4828cbbe / LK-S5-TRAINING1. Host/join, Night 1 preparation hold, radio clearance, guided ship pass, guest leave/rejoin and retry replicated successfully. A separate guest test used the actual XRI starter grip, 30 cm physical strokes and recoil; the host reached three successful pulls and settled running. The guest generator screenshot was inspected.

Fixture corrections: the preparation helper originally provided too few bounded fuel increments (not a gameplay bug); the starter fixture initially placed the head too far from the handle, then pulled on the descending/out-of-band half of the gauge. Corrected reachable pose, lease wait and time-based ascending strokes pass. No gameplay validation was weakened for these fixtures. Test fixtures inject station inputs and transport; they do not prove human usability or audible voice. Both Editors remained muted.

Reproduce from stopped, compiled Editors: synchronize peer with `artifacts/s5/unfinished/sync-training.py` using a fresh backup folder; run `crew-final.py` (corrected preparation already in ClearanceProbe.cs) and `crew-starter-timed.py`, each with a fresh output directory. The resume fixture captures the successful clearance run after the bounded fuel correction. Shared human sleep/voice and worn timing remain device checks.
