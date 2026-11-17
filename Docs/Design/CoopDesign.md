# Two keepers, one machine

The current design is [CoopCoreLoop.md](CoopCoreLoop.md), updated November 17 after the user's co-op-first focus change. It supersedes this document's old solo-clamp design and the previous plan to defer every companion.

Two humans are the target; one human gets a simple scripted second keeper. **Lightkeeper** leads ships/beacon/radio. **Machinist** leads engine/pumps/repairs. Duties select responsibilities and useful information, never exclusive tool permissions. Solo uses the same paired interactions with a visible bot, not an invisible clamp or duplicated local clues.

## What actually exists

- Pipe: a fresh valve hold by another authenticated actor allows wrench progress. Release increases pressure, pauses tightening and retains work. Grip-only auto-snap and arm pulls remain.
- Power: generator → panel → pump/beacon/radio dependency exists. Circuit allocation can be coordinated voluntarily.
- Shared tasks, sleep, proximity voice, handheld radios, leases and rejoin already have earlier transport/playtest evidence.
- Generator/code/bearing split variants are authored in night data, but are not a complete runtime information split. The radio HUD currently leaks code answers; fix it as part of the paired puzzle.
- Roles, companion and hand-revive are **not implemented**. Navigation/hunter foundations are not a substitute for those features.

## Next playable target

One short crew scenario: choose duties → solve a valve/repair and an information relay → restore power under a warned threat → save a ship → rescue a partner. Test both duties, then the same route with the scripted partner, before expanding the horror schedule. Implementation order, failure/recovery behavior, offline rules and acceptance checks are in [CoopCoreLoop.md](CoopCoreLoop.md).

Keep spatial voice and the existing mono/AEC path. Both humans must actually exchange information across rooms. Automated transport tests do not certify human intelligibility or headset comfort.
