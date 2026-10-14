# Storm Watch

Two keepers survive three continuous watches and reach the lantern alive together. The pacing target is roughly ten minutes for the whole run, not ten minutes per day. That target remains subject to a two-person headset playtest; no timer forces an automatic win. Days 4–5 and the pressure-regulation minigame are deferred.

## Start a crew

Use **Lighthouse Keepers → Storm Watch → Play two-player watch**. Both players load `LK_StormWatch`, choose the same two-digit crew room, and select Host / Join. The host owns gameplay state; the client sends interaction/pose input and receives the shared flood, objectives, supplies and rescue state. Exactly two connected keepers are required. The Photon app configuration already in the project is used. Internet access is required.

This isolated entry includes its own minimal crew join controls and does not replace the separate networking-lobby work. Shared environment scenes, the existing player rig and its controller bindings are reused. The exterior daylight object has been removed: this is a nighttime experience with interior lighting, the restored beacon and transient lightning flashes. The ordinary development gym keeps its preview lighting.

## The continuous loop

| Watch | Cooperative work | Reward / route |
| --- | --- | --- |
| Day 1 — First Watch | One keeper holds the pipe coupling; the other picks up the wrench and twists it against the marked coupling. Lock three seal stages. The generator and powered pump work normally. | Spray stops, water visibly falls, and the first stair gate opens. |
| First refuge | Regroup on the generator floor. Choose three faster repair steps OR ten extra seconds on future rescue windows. | Open the next watch together. Water begins approaching after a short drainage interval. |
| Day 2 — Power Failure | A new leak starts; the generator fails after a warning interval. One keeper strokes the manual pump below while the other turns the generator control upstairs. Restore three generator steps, then reunite to seal the pipe. | Power returns, water falls, and the route to the radio floor opens. |
| Second refuge | Choose repair help or rescue protection and regroup. The approaching flood discourages waiting indefinitely. | Start the storm. |
| Day 3 — The Storm | Plumbing and power fail together. Pump, restore the generator, and seal the pipe; then one keeper reads/transmits the radio code while the other turns the matching beacon switches above. | The beacon lights. The radio keeper must also climb to the top; both must be alive for victory. |

Completed repair steps survive setbacks. Releasing a control retains unfinished progress; operating the generator after pump support has been absent for the grace interval resets only its unfinished step and causes one water surge per cooldown. Re-establish pump support to recover stability. Wrong beacon switches likewise reset only the unfinished beacon step. Day 1 does not introduce pressure regulation.

The radio keeper holds the radio while relaying SUN / WAVE / STAR in the posted order. The lantern board does not reveal the code. Voice can use colocated speech or a Quest party; in-game call buttons also relay each symbol, so voice software is not required. Integrated voice transport is not added.

## Physical interactions

- Grip the brass coupling to brace it. The other keeper must do the repair; one player cannot provide both jobs in the rules.
- Grip the wrench, bring it within 0.6 m of the marked pipe coupling, and turn your wrist. Release/regrip freely. A dropped wrench returns to its workbench after eight seconds. Each client has a local tool; its job contribution is authoritative on the host.
- Grip and stroke the manual pump handle. Holding it still does not pump.
- Grip and twist generator / beacon controls. Hover brightens the grip; holding turns it green. Step confirmation uses audio, visible progress and a brief haptic pulse.
- World-space boards show instructions, water, power, progress and the recovery choices. Room selection uses +/- buttons, including on Quest.

## Rescue and failure

Submersion consumes six seconds of air by default. A downed keeper has a short rescue window. The other keeper grabs the yellow belt lifeline, aims at their teammate within eight metres and releases. The float attaches with forgiving aim; the stranded keeper grabs the presented catch handle, and the rescuer grips and pulls their handle. The rope and countdown show progress. Rescue fades the view before moving the recovered rig beside the rescuer; no sustained forced-camera motion is used. Reach, catch affordance and blink comfort require headset testing.

If both keepers are down, or a rescue window closes, the run ends. Restart resets shared progression, supplies, flood and player positions. A disconnect pauses progression and presents Leave Crew in front of the keeper. Leaving resets the local watch and returns the rig to the entry behind a blink; reconnect together in a fresh session to retry. Host migration and late-join recovery are not supported.

## Editor rehearsal

WASD / Q–E use the existing desktop preview. Near the relevant station: B braces, R repairs the pipe, P pumps, G services the generator, T holds the radio, and 1/2/3 turn SUN/WAVE/STAR. Enter begins or continues; on a terminal result it retries. L casts toward the view, K catches and J pulls. These keys are available only in Editor/development players; physical controls remain the Quest path. Mouse pointers use the same boards. Two participants are still required; there is no invisible solo helper.

## Authoring and verification

The Inspector's `WatchTuning` controls action durations, flood rates, blackout timing, support grace, air/rescue/pull timers and reach. Default work durations are 14 seconds per pipe stage, 18 per generator step and 8 per beacon switch at full interaction effort. Human movement, regripping, communication and refuges account for the remaining pacing; measure the total during the first two-person playtest.

**Build Quest APK** writes `Builds/LighthouseKeepers-StormWatch.apk`; **Build desktop companion** writes a development Mac player when that Unity platform module is installed. Build helpers use the isolated entry plus the six content scenes and do not alter the normal build list.

Acceptance checks cover distinct-player dependencies, physical proximity, blackout/manual pumping, release and support recovery, preserved completed steps, single shared supply choices, radio-only information, rescue eligibility/catching/pulling, both-player victory, retry and disconnect pauses. Reports are recorded under `Docs/Verification/StormWatch`. Distinguish rules tests, scene/interaction tests, real transport checks and headset usability/performance; passing one is not evidence for the others.
