# AI Game Development Lexicon reference

Converted from the user-supplied `AI Game Development Lexicon.docx` on October 13, 2026. Source SHA-256: `fd86dc54025acf5de848e548bea907f7e14a3493d9dd615bd1501348dc39114b`.

The following is reference material. Example prompts are illustrative, not standalone repo tasks. Operational guidance is in [README.md](README.md).

AI Game Development Lexicon

A working vocabulary for VR designers directing AI coding agents

Three primary competencies:

Design literacy: Articulating what an experience should do, how it should feel, and how people should interact with it.

Technical literacy: Understanding Unity's objects, systems, architecture, and performance constraints well enough to direct development.

Agentic literacy: Specifying, delegating, inspecting, debugging, and iterating on work performed by AI agents.

The goal is not simply to know definitions. It is to use vocabulary to transform design intentions into precise, testable instructions.

Part I. The Core Lexicon

1. Game Design & Experience Design

These terms should be part of every designer’s active vocabulary. They describe what the experience is supposed to accomplish, independently of the technology implementing it.

| Term | Definition |
| --- | --- |
| Mechanic | A rule or interaction through which a player affects the game world. |
| Core Loop | The recurring cycle of player actions, feedback, and rewards that structures gameplay. |
| Affordance | A quality of an object that makes a particular interaction possible or apparent. |
| Signifier | A visual, auditory, or other cue communicating how something should be used. |
| Feedback | Information communicating the consequences of an action. |
| Game State | The current condition of the game's systems, objects, and variables. |
| State Transition | The change from one state to another in response to an event or condition. |
| Player Agency | The player's capacity to make meaningful choices and influence outcomes. |
| Emergent Behavior | Complex or unexpected outcomes arising from interactions between simpler systems. |
| Progression | The sequence through which challenges, abilities, or experiences develop. |
| Onboarding | How players learn the rules and interactions of an experience. |
| Difficulty Curve | The progression of challenge throughout an experience. |
| Juice / Game Feel | Sensory feedback and responsive details that make interactions satisfying. |
| Diegetic Interface | Interface elements that exist within the fictional world rather than as external overlays. |
| Grayboxing / Blockout | Using simple geometry to establish spatial layout and interactions before producing detailed art. |
| Vertical Slice | A small but representative portion of a game demonstrating its intended quality and major systems. |

Weak instruction:

Make the object more interesting when I touch it.

Stronger instruction:

Improve the object's interaction affordances and feedback. When the player's hand approaches, provide a subtle visual signifier. When the object is grabbed, trigger a haptic response and change its material emission. When released, provide appropriate audio feedback.

2. Spatial Design & VR Interaction

These concepts are particularly important because a working piece of code does not necessarily produce a comfortable or intuitive VR experience.

| Term | Definition |
| --- | --- |
| 6DoF | Six degrees of freedom: three positional and three rotational dimensions. |
| Pose | An object's tracked position and orientation. |
| World Space | Positions and rotations defined relative to the global scene coordinate system. |
| Local Space | Positions and rotations defined relative to an object's parent transform. |
| XR Origin / Camera Rig | The system organizing the user's tracked head, hands, and playspace in a Unity scene. |
| Interactor | The component or system initiating interactions with objects. |
| Interactable | An object configured to respond to an interactor. |
| Direct Interaction | Interacting with nearby objects through virtual physical proximity. |
| Ray Interaction | Interacting at a distance using a projected ray or pointer. |
| Hover | An interaction state in which an object is being targeted but not necessarily selected. |
| Select | An interaction state representing the selection or holding of an interactable. |
| Activate | An action performed on an interactable, distinct from merely selecting it. |
| Attach Point | The position and orientation used to connect a grabbed object to an interactor. |
| Snap Zone / Socket | A defined location or volume where an object can be placed or attached. |
| Locomotion | The system through which a player moves through the virtual environment. |
| Teleportation | Moving between positions without continuous virtual translation. |
| Snap Turning | Rotating the player's view in discrete increments. |
| Smooth Turning | Rotating the view continuously. |
| Haptics | Tactile feedback, commonly through controller vibration. |
| Spatial Audio | Sound positioned and processed to communicate locations in 3D space. |
| Occlusion | One object blocking another from view; also relevant to spatial audio. |
| Embodiment | The sense that a virtual body or representation corresponds to one's physical self. |
| Presence | The subjective feeling of actually being within a virtual environment. |
| VR Comfort | Design considerations that reduce discomfort, disorientation, or simulator sickness. |
| Passthrough | Displaying the physical environment through headset cameras within an XR experience. |

Unity's XR Interaction Toolkit formally distinguishes interactors, interactables, and interaction states such as hover, select, and activate. These terms are especially useful because they bridge experience design and implementation. Unity Documentation

Weak instruction:

Make the player able to pick up and rotate the cube.

There are several different implementations that could satisfy this request.

Does the cube follow the controller's orientation? Does the player rotate it with the thumbstick? Is it constrained to one axis? Is its motion physically simulated? What happens when it collides with another object?

Stronger instruction:

The cube should be a direct-grab interactable. While selected, it follows the controller's position and orientation. It should have collision detection and should not penetrate static environment geometry. When released, it should retain an appropriate throwing velocity.

3. Unity Architecture & Scene Organization

A mental model of what Unity contains and how its parts fit together.

| Term | Definition |
| --- | --- |
| Scene | A collection of objects and systems representing part of a Unity experience. |
| GameObject | The fundamental object container in Unity. |
| Component | A modular unit of functionality attached to a GameObject. |
| Transform | A component defining position, rotation, and scale. |
| Hierarchy | The parent-child organization of GameObjects in a scene. |
| Parenting | Establishing a hierarchical relationship between objects. |
| Prefab | A reusable GameObject configuration saved as a project asset. |
| Instantiation | Creating an object instance, often during runtime. |
| MonoBehaviour | A common base class for Unity scripts that attach to GameObjects. |
| Inspector | The Unity interface for examining and editing object properties. |
| Serialized Field | A value Unity preserves and may expose for configuration in the Inspector. |
| Asset | A project resource such as a mesh, material, texture, script, or audio file. |
| Mesh | Geometry defining a 3D object's shape. |
| Material | A collection of settings controlling how a surface is rendered. |
| Shader | A program controlling aspects of how geometry and surfaces are rendered. |
| Collider | A component defining an object's shape for collision detection. |
| Rigidbody | A component enabling an object to participate in physics simulation. |
| Trigger | A collider configuration that detects overlaps without producing ordinary physical collision responses. |
| Layer | A classification used by systems such as rendering, physics, and raycasting. |
| Event | A notification that something has occurred, allowing other systems to respond. |
| Input Action | An abstracted player input such as Grab, Move, or Activate. |
| URP | Unity's Universal Render Pipeline. |
| OpenXR | A cross-platform API standard for XR applications and hardware. |
| Package / Dependency | External or modular functionality installed into a Unity project. |

Weak instruction:

Make me a control panel with buttons.

Stronger instruction:

Create a reusable control panel Prefab with three button GameObjects. Each button should have its own collider and interaction component. Expose the button actions as configurable events in the Inspector. Organize the hierarchy so that the panel can be moved and scaled without breaking its interactions.

An AI Designer should be able to describe an object hierarchy verbally, inspect what an agent created, and recognize unnecessary complexity.

4. Game Logic & Systems Thinking

| Term | Definition |
| --- | --- |
| State Machine | A system defining possible states and the rules governing transitions between them. |
| Event-Driven Logic | Behavior triggered by events rather than continuous checking. |
| Polling | Repeatedly checking whether a condition is true. |
| Conditional Logic | Behavior depending on whether specified conditions are met. |
| Threshold | A numerical boundary at which behavior changes. |
| Constraint | A rule limiting an object's movement or behavior. |
| Interpolation | Calculating intermediate values between two values or states. |
| Easing | Controlling the acceleration or deceleration of a transition. |
| Damping | Reducing movement or oscillation over time. |
| Cooldown | A required delay before an action can occur again. |
| Debouncing | Preventing repeated triggering from a single noisy or prolonged input. |
| Event Listener | A component that responds when an event occurs. |
| Modularity | Organizing functionality into independent, reusable parts. |
| Encapsulation | Keeping a system's internal implementation separate from its public interface. |
| Single Source of Truth | Maintaining one authoritative location for a piece of state or information. |
| Determinism | Producing predictable results from the same inputs and initial conditions. |

Weak instruction:

Make a lever that opens a door.

Stronger instruction:

| State | Trigger | Result |
| --- | --- | --- |
| Idle | Player approaches | Lever becomes highlighted |
| Hovered | Player selects lever | Lever becomes movable |
| Selected | Player rotates lever past threshold | Door opening begins |
| Activated | Door reaches open position | Sound and visual confirmation |
| Resetting | Player returns lever to rest | Door closes |

Part II. Performance Literacy

5. Rendering & Optimization

As AI dramatically reduces the effort required to generate scenes, effects, shaders, and objects, it also makes it easier to create experiences that perform poorly.

Computational resources are design materials.

| Term | Definition |
| --- | --- |
| Frame Rate (FPS) | The number of frames rendered per second. |
| Frame Time | The time required to produce a frame, measured in milliseconds. |
| Frame Budget | The maximum time available to produce a frame at a target refresh rate. |
| CPU Bound | Performance limited primarily by CPU processing. |
| GPU Bound | Performance limited primarily by graphics processing. |
| Draw Call | A command submitting geometry or related rendering work to the graphics pipeline. |
| Batching | Combining compatible rendering work to reduce submission overhead. |
| GPU Instancing | Rendering multiple copies of compatible geometry efficiently using shared resources. |
| Polygon / Triangle Count | A measure of geometric complexity. |
| Overdraw | Rendering multiple surfaces or fragments over the same screen pixels. |
| Fill Rate | The rate at which the GPU can process and write rendered pixels. |
| Shader Complexity | The computational cost of rendering a material or effect. |
| Real-Time Lighting | Lighting calculated dynamically as the experience runs. |
| Baked Lighting | Lighting information calculated in advance and stored for runtime use. |
| Occlusion Culling | Avoiding rendering objects hidden behind other geometry. |
| Frustum Culling | Avoiding rendering objects outside the camera's view. |
| Level of Detail (LOD) | Switching between different representations of objects based on viewing conditions. |
| Texture Resolution | The dimensions and associated memory requirements of an image texture. |
| Mipmaps | Precomputed lower-resolution texture levels used to improve sampling and performance. |
| Object Pooling | Reusing objects instead of repeatedly creating and destroying them. |
| Garbage Collection (GC) | Automatic management and reclamation of certain allocated memory. |
| Allocation | Reserving memory for data or objects. |
| Profiling | Measuring where processing time and resources are consumed. |
| Thermal Throttling | Hardware reducing performance because of heat or power constraints. |
| Foveated Rendering | Allocating different levels of rendering detail across the visual field. |

| Refresh rate | Frame time budget |
| --- | --- |
| 72 Hz | 13.89 ms |
| 90 Hz | 11.11 ms |
| 120 Hz | 8.33 ms |

Weak instruction:

Optimize my game.

Strong instruction:

Profile the current scene on the target headset. Report CPU frame time, GPU frame time, draw calls, and garbage collection allocations. Identify the three largest measurable bottlenecks. Recommend changes ranked by expected performance improvement and impact on visual quality. Implement only the approved changes, then profile again under the same conditions.

Design decisions versus optimization decisions

How do we make this effect faster?

and

Is this effect worth its computational cost?

Suppose an AI creates an environment with 20 dynamic lights.

One approach is to ask the agent to optimize those lights.

A better designer might determine that only three lights are essential to the experience.

This is one of the most important implications of AI-assisted game development:

The best optimization may be a design decision, not a programming decision.

Part III. Agentic Development Literacy

6. AI Coding Agents, MCP & CLI

| Term | Definition |
| --- | --- |
| Coding Agent | An AI system capable of planning and performing software development tasks using tools. |
| Agentic Workflow | A process in which an AI executes multiple steps toward a goal rather than producing a single response. |
| MCP (Model Context Protocol) | A protocol that connects AI applications with tools and external sources of context. |
| MCP Server | A service exposing capabilities or data that an AI client can access. |
| Tool Call | A specific action an AI agent requests through an available tool. |
| CLI (Command-Line Interface) | A text-based interface for executing commands against software and operating systems. |
| Repository | A collection of project files typically managed with version control. |
| Context File | A document describing project rules, architecture, constraints, or instructions for an AI agent. |
| Scope | The boundaries of what an agent is permitted or expected to modify. |
| Dependency | Software or functionality required by another system. |
| API | An interface through which one software component communicates with another. |
| Refactoring | Restructuring existing code without intentionally changing its externally observable behavior. |
| Code Review | Inspecting changes for correctness, maintainability, and unintended consequences. |
| Diff | A representation of the changes between versions of files. |
| Commit | A recorded snapshot of changes in a version-control system. |
| Branch | A separate line of development in version control. |
| Rollback | Returning a project or system to an earlier state. |
| Regression | A problem introduced into functionality that previously worked. |
| Hallucinated API | A function, class, or capability an AI invents or incorrectly assumes exists. |
| Acceptance Criteria | Explicit conditions that must be satisfied for a task to be considered complete. |
| Verification | Gathering evidence that an implementation satisfies its stated requirements. |

MCP versus CLI

MCP: Allows an AI agent to use connected tools, potentially including scene inspection, object manipulation, or runtime observation.

CLI: Allows an agent or developer to execute command-line operations, such as building a project, running tests, or deploying to a headset.

Unity Editor: Contains the project's scene and asset structure, with its own editing and runtime facilities.

Headset: Runs the deployed experience, where actual interaction, comfort, and hardware performance must ultimately be evaluated.

Part IV. A Vocabulary for Testing and Debugging

7. Verification & Quality Assurance

| Term | Definition |
| --- | --- |
| Compilation | Converting source code into a form that can be executed. |
| Play Mode | Running a simulation of the Unity experience within the Editor. |
| Build | Producing an executable version of the application for a target platform. |
| Deployment | Installing or transferring the application onto its target device. |
| Runtime Error | A problem occurring while the application is executing. |
| Exception | An error condition reported by software during execution. |
| Stack Trace | A record of the sequence of calls associated with an error. |
| Null Reference | An attempt to access an object or reference that does not exist. |
| Test Case | A specific scenario used to check functionality. |
| Edge Case | An unusual or extreme condition that may reveal problems. |
| Reproduction Steps | A repeatable sequence of actions that triggers a bug. |
| Unit Test | A test of an individual component or behavior. |
| Integration Test | A test checking whether multiple components work together. |
| Regression Test | A test confirming that previously working functionality remains intact. |
| Benchmark | A standardized workload used to measure and compare performance. |
| Performance Baseline | Measurements recorded before optimization or other changes. |

| Level | Evidence |
| --- | --- |
| 1 | Agent reports implementation complete |
| 2 | Project compiles without errors |
| 3 | Feature works in Unity Play Mode |
| 4 | Feature works on the target VR headset |
| 5 | Feature meets usability and performance requirements |
| 6 | Feature survives integration with the rest of the project |

A feature isn't finished just because it passes Level 2.

Part V. Teaching Students to Speak to Coding Agents

8. The Anatomy of a Strong Agent Instruction

| Element | Question |
| --- | --- |
| Intent | What experience are we trying to create? |
| Context | What project, system, and existing objects are involved? |
| Behavior | What should happen, and under which conditions? |
| Architecture | How should the result fit into the existing Unity project? |
| Constraints | What must the implementation avoid or preserve? |
| Performance | What hardware and resource limits must it respect? |
| Verification | How will we determine that the result is correct? |

Weak instruction:

Create a VR puzzle where I pick up objects and put them in the correct places.

Stronger instruction:

Intent: Create a tactile VR puzzle in which the player discovers which of three artifacts belongs in each socket.

Context: This is an existing Unity URP project using OpenXR. Inspect the installed XR interaction packages and the current scene before selecting an implementation.

Behavior: Each artifact must support direct grabbing. When an artifact enters the correct socket's detection volume, provide subtle visual feedback. If released within 8 cm of the socket's center and 15 degrees of its target orientation, smoothly interpolate it into place over 0.25 seconds. Incorrect artifacts must remain movable.

Architecture: Use the project's existing interaction framework. Make socket configuration reusable. Expose the snap distance and angle thresholds in the Inspector. Avoid unnecessary new dependencies.

Constraints: Do not change the XR Origin or existing controller bindings. Do not move the player's camera. Preserve existing prefab references.

Performance: Design for standalone VR. Avoid per-frame object creation and unnecessary allocations. Prefer lightweight feedback effects.

Verification: Confirm compilation, inspect scene references, test interaction in Play Mode, and build to the target headset. Report what was tested, measured, and not verified.

Part VIII. A Shared Agent Vocabulary Protocol

Some common instructions to use with AI agents: 

These could become part of each project's AGENTS.md or equivalent agent context file.

| Instruction | Purpose |
| --- | --- |
| Inspect before editing | Have the agent examine existing scene structure and dependencies. |
| Plan before implementation | Require a short implementation plan for nontrivial changes. |
| Preserve existing behavior | Minimize unintended regressions. |
| Prefer existing systems | Avoid redundant frameworks and incompatible packages. |
| Expose design parameters | Make important values configurable in the Inspector. |
| Use version-compatible APIs | Check actual installed package versions rather than assuming APIs exist. |
| Minimize the change surface | Avoid unnecessary changes outside the requested feature. |
| Verify references | Check that required components and object references are correctly assigned. |
| Test before reporting completion | Require compilation and available runtime tests. |
| Report evidence and limitations | Distinguish verified functionality from assumptions. |
| Profile before optimizing | Identify measured bottlenecks rather than guessing. |
| Preserve rollback capability | Maintain version-control checkpoints before substantial changes. |

## Links in the source

- https://docs.unity3d.com/ja/current/Manual/com.unity.xr.interaction.toolkit.html?utm_source=chatgpt.com
