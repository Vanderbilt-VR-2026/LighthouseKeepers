# Interaction input audit — November 11

Current integrated build, not unimplemented M3 doors/mirror/horror tools. Hand base rotations and finger blending are unchanged. Direct physical controls use grip selection; lobby/menu ray clicks use trigger without first gripping anything.

| Object | Input | Outcome |
|---|---|---|
| Wrench / active bolt | Hold GRIP, bring jaw to bolt, short arm pulls | Auto-snap; no trigger lock. Push resets, progress stays. Release drops. |
| Pressure valve (two keepers only) | Hold GRIP | Partner tightens; release removes pressure reduction. Solo has no valve step. |
| Primer bulb | Squeeze GRIP to ≥80%; ease/release to ≤25% | One count, visible deflation, click and haptic; sustained hold never repeats. Release refills; extra squeezes after primed are silent. |
| Choke | Hold GRIP, pull UP / push DOWN | 16 cm physical stroke; release holds detent; grab alone does nothing. No trigger. |
| Starter | Hold GRIP and pull arm outward; return | Existing travel/pose and ratchet retained. |
| Fuel can | Hold GRIP, tilt spout above filler | Pour; no second button. |
| Power panel circuit / RESET | GRIP lever | Flip/reset; no trigger. |
| Radio MAIN and code buttons | GRIP/select the control | Flip/depress once; no trigger. |
| Radio tuning knob | Hold GRIP and rotate | Tune; no trigger. |
| Broadcast microphone | Hold GRIP + hold TRIGGER | Essential push-to-talk, explicitly retained. |
| Beacon ignition | GRIP switch | Toggle. |
| Beacon alignment wheel | Hold GRIP and rotate | Align. |
| Lens rag | Hold GRIP and rub lens | Clean; no trigger. |
| Handheld crew radio | Hold GRIP near head; optional TRIGGER elsewhere | Near-head transmission is one gesture; optional explicit push-to-talk retained. |
| PA replay button | GRIP/select button | Replay/status, no combination. |
| Bed | GRIP near bunk (trigger alternative also accepted) | Vote to sleep when jobs complete; no simultaneous buttons. |
| Pole | Hold GRIP; release to pause/exit | Existing descent. |
| Objective recall | B tap/hold, double-tap replay; X/Y/wrist retained | No grip required. |
| Watch Menu / lobby | Point and TRIGGER | UI selection, no grip required. |

Implementation uses existing EnginePrime host command; no extra synchronized field. Old archived trigger-clutch code remains behind the disabled legacy mode, but the accepted grip-only setting and visible fallback plates no longer instruct two-button wrench use. The former primer `activated` handler has been removed entirely. Both hands passed actual-XRI grip-only counts, held-no-repeat, release/refill and extra-squeeze tests; haptic strength/reach/finger contact still need a worn Quest.
