# Two keepers, one machine

Two players are the design target. They should need each other's observations and actions, with recovery after a dropped tool, missed message or disconnect. Solo uses explicit mechanical substitutes, never a hidden second player. The same host-owned simulation runs both modes.

## Station designs

| Station | Two simultaneous roles / places | Solo adjustment | Evaluation / status |
| --- | --- | --- | --- |
| Pipe | One keeper holds the spring pressure valve at the north-east F1 wall; the other pulls the wrench around the three bolt heads further along the east wall. | Grip the valve once to fasten its visible clamp, then tighten. | **Implemented this pass.** Separate work positions, same clear physical verbs. Release surges the leak to its unreduced level, pauses tightening and preserves every earned degree. No extra failure timer. |
| Generator | Keeper on F3 reads an oil-pressure/rev gauge; keeper on F2 primes/chokes/pulls and responds to calls. | An F2 mirror gauge replaces the remote instrument. | **Design only.** Good information split, but avoid withholding the initial fuel/primer tutorial or inventing an unexplained starting penalty. Prototype after the pipe is accepted. |
| Breaker / pump | F1 keeper watches the water and calls for pumping while F2 keeper allocates load and resets the breaker. | All load and water information remains available locally/on the board. | Existing shared power grid already supports voluntary coordination; a mandatory hold gate is **not implemented**. Avoid a second arbitrary two-person lock. |
| Beacon | Keeper at the F4 lens turns the wheel; keeper at a separated radio-room bearing repeater calls left/right and confirms alignment. | Mount the target bearing on the lens housing. | **Design only.** The current radio and beacon share F4, so a real information split needs a separated repeater; do not pretend they are on different floors. |
| Radio | One keeper tunes/enters/transmits on F4 while another reads the night's code from the House. | CodeSource stays beside the console. | **Design only.** CodeSource is already separate. Use short colour-and-symbol codes; don't rely on colour alone. |
| Bed / recovery | Each keeper reaches their bunk; both vote before the shared night transition. | One keeper's vote advances. | Existing shared-bed behavior retained. The board shows who is still needed. Disconnect reduces the required count. |

## Borrowed principles, not copied assets

- **Keep Talking and Nobody Explodes:** split useful information between operator and observer, so precise conversation does real work. Borrow for the future generator/code roles. Its official description separates the bomb operator from friends holding the manual. [Official game site](https://keeptalkinggame.com/).
- **Lethal Company:** travel and equipment create choices about sticking together versus splitting up. Radios should earn their place as physical tools, and reunion should feel safer. This is our design interpretation; the developer lists walkie-talkies among the equipment. [Developer's Steam page](https://store.steampowered.com/app/1966720/Lethal_Company/).
- **Phasmophobia:** tension comes from interpreting tools together in an uncertain place. Borrow readable equipment and shared evidence, not its supernatural rules. The developer describes cooperative evidence gathering with investigation equipment. [Developer's Steam page](https://store.steampowered.com/app/739630/Phasmophobia/).
- **We Were Here:** separated viewpoints make describing a location or measurement valuable. Borrow bounded information asymmetry and an in-world radio; avoid long cryptic instructions. The developer explicitly describes separated players communicating by walkie-talkie. [Developer's Steam page](https://store.steampowered.com/app/582500/We_Were_Here/).

## Implemented pipe contract

- Keep the accepted grip-only snap and arm-pull ratchet. No trigger clutch, new wrist motion or lost progress.
- In a two-person crew, only a fresh valve hold by **another actor** permits bolt progress. The host validates hand proximity, actor identity, revision and heartbeat. First holder wins; stale holds expire after the tuning asset's timeout.
- A release changes the gauge, spring handle and spray immediately on the host, then on replicas. Leak pressure while held is reduced to the configured fraction; release never exceeds the original unheld leak.
- The Objective Director assigns Hold the valve / Tighten the bolt (Grab the wrench first if needed). The active holder keeps that role. A known wrench owner takes the bolt role. Urgent generator work still takes precedence during an outage; the other keeper may suppress the leak meanwhile.
- Solo, host-alone and disconnected crews use the visible clamp. Joining a second keeper removes the solo latch; disconnect preserves all completed bolt work and makes the clamp available. Retry/new night clears the hold/clamp.
- New systems have switches in `ConnectedTasksTuning`. Both crew clients need matching settings/builds. Production authority and scene routing are reused, not replaced.

## Voice contract

Near speech is spatial and clear; walls/floors attenuate and low-pass it; two or more floors away is faint. Each keeper can carry one of the F1/F4 radios. The listener must hold a radio to receive the clear cross-floor channel; the speaker holds theirs near their head or holds trigger. Release falls back to proximity. Mute always stops microphone recording/transmission, including handheld use.

Radio colour is playback-only: band-pass, mild crunch, faint crackle and short squelch. The accepted native mono capture and AEC/noise-suppression chain stays intact. Host-approved held-radio state routes one existing voice stream; no duplicate broadcast stream or echo. Human intelligibility, headset reach and acoustic occlusion acceptance remain device checks.
