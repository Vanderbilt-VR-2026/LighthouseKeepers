# Storm Watch

Two keepers survive three continuous watches and reach the lantern alive together. The pacing target is roughly ten minutes for the whole run, not ten minutes per day. That target remains subject to a two-person headset playtest; no timer forces an automatic win. Days 4–5 and the pressure-regulation minigame are deferred.

## Start a crew

Use **Lighthouse Keepers → Play game** (the Storm Watch play shortcut opens the same lobby). One keeper chooses **Host crew**, the other joins that room through **Crew options**. Both signal ready; only then can the host choose **Begin watch**. The room holds exactly two keepers.

Fusion loads `LK_StormWatch` for the crew using the existing lobby runner. That entry loads the six environment scenes and the existing XR rig. There is no second room selection, Host/Join action, runner, or Begin button inside gameplay. The host starts Day 1 after both environments send valid gameplay input, so the loading keeper does not consume air or watch time. The host owns flood, progression, supplies and rescue state; the client sends interaction/pose input on a separate gameplay channel of that same connection.

**Return to lobby** is available on each floor's instruction board and on the result board. Leaving shuts down the crew session. A partner disconnect ends that run and returns the remaining keeper to a fresh lobby, with an explanation; create/join a new crew and ready up again. Retry after a win/loss keeps the current crew, resets the run and starts once both are ready in the environment. Host migration and late-join recovery remain unsupported.

The existing nighttime environment, player rig and controller bindings are reused. The exterior daylight object is removed; interior lighting, the restored beacon and brief lightning flashes remain. The ordinary environment preview remains under **Development → Play environment**; the older First Watch entry is a local rehearsal.

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
- World-space boards show instructions, water, power, progress and the recovery choices. Crew room selection and ready state live on the lobby watch board.

## Rescue and failure

Submersion consumes six seconds of air by default. A downed keeper has a short rescue window. The other keeper grabs the yellow belt lifeline, aims at their teammate within eight metres and releases. The float attaches with forgiving aim; the stranded keeper grabs the presented catch handle, and the rescuer grips and pulls their handle. The rope and countdown show progress. Rescue fades the view before moving the recovered rig beside the rescuer; no sustained forced-camera motion is used. Reach, catch affordance and blink comfort require headset testing.

If both keepers are down, or a rescue window closes, the run ends. Restart resets shared progression, supplies, flood and player positions. A disconnect pauses progression immediately while the lobby transport ends the session and returns both sides to the lobby. Reconnect together in a fresh room to retry.

## Editor rehearsal

WASD / Q–E use the existing desktop preview. Near the relevant station: B braces, R repairs the pipe, P pumps, G services the generator, T holds the radio, and 1/2/3 turn SUN/WAVE/STAR. Enter continues at refuges; on a terminal result it retries. The initial watch starts from the ready lobby. L casts toward the view, K catches and J pulls. These keys are available only in Editor/development players; physical controls remain the Quest path. Mouse pointers use the same boards. Two participants are still required; there is no invisible solo helper.

## Authoring and verification

The Inspector's `WatchTuning` controls action durations, flood rates, blackout timing, support grace, air/rescue/pull timers and reach. Default work durations are 14 seconds per pipe stage, 18 per generator step and 8 per beacon switch at full interaction effort. Human movement, regripping, communication and refuges account for the remaining pacing; measure the total during the first two-person playtest.

**Build Quest APK** writes `Builds/LighthouseKeepers-StormWatch.apk`; **Build desktop companion** writes a development Mac player when that Unity platform module is installed. Both build helpers use the normal crew lobby first, followed by Storm Watch, the development Bootstrap, and the six content scenes. The standard development APK uses that same scene list.

Acceptance checks cover distinct-player dependencies, physical proximity, blackout/manual pumping, release and support recovery, preserved completed steps, single shared supply choices, radio-only information, rescue eligibility/catching/pulling, both-player victory, retry and disconnect pauses. Reports are recorded under `Docs/Verification/StormWatch`. Distinguish rules tests, scene/interaction tests, real transport checks and headset usability/performance; passing one is not evidence for the others.
