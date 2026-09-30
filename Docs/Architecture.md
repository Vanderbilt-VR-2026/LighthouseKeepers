# Lighthouse Keepers — Architecture

This describes how the runtime code, scenes and data assets actually talk to each other. For team workflow see [TeamWorkflow.md](TeamWorkflow.md); for sprint status see [Sprint1Handoff.md](Sprint1Handoff.md). This doc only covers what exists in code today.

## 1. Scene loading (the spine)

There is one entry point: `Scenes/Bootstrap/LK_Bootstrap.unity`. Everything else loads additively from it.

```
LK_Bootstrap (contains XR Origin, persistent systems)
  └─ EnvironmentBootstrap.Start()
       loads additively, in order:
       LK_Exterior → LK_Core → LK_Level01_Plumbing
       → LK_Level02_Generator → LK_Level03_Communications → LK_Level04_Lantern
       then: SetActiveScene(LK_Exterior), restores spawn position, re-enables movement
```

`EnvironmentBootstrap` (`Scripts/Runtime/Core/EnvironmentBootstrap.cs`) runs at `DefaultExecutionOrder(-500)` so it disables the player's locomotion/character controller in `Awake()` *before* any scene loads, then re-enables it once all six scenes are in and physics transforms are synced. This is the fix for the player falling/sliding through unloaded geometry during the additive load. It logs `LK_READY` when done — that's the signal the rig is safe to move.

`LK_DevGym` is a separate, non-additive scene for isolated component testing — it does not go through Bootstrap.

There is no scene-transition system beyond this one load; the six content scenes are all present simultaneously once Bootstrap finishes. "Level01/02/03/04" are spatial zones of one continuous space, not sequential loaded levels.

## 2. The Day → Storm/Flood → Environment/Audio reaction chain

This is the core simulation loop and the main thing to understand before touching sound or UX.

```
DayDirector (Progression)
  ScriptableObject DayProfile[1..5] (Day1..Day5.asset)
  ApplyDay(n) →
      FloodController.RiseSpeed = profile.FloodRiseRate
      StormController.Apply(profile)   → intensity, windIntensity, lightningInterval, thunder proximity
      onProfileApplied UnityEvent<DayProfile>  (anything can subscribe)
```

`DayDirector` is the single source of truth for "how dangerous is today." It doesn't touch audio or visuals directly except through the two systems above and the UnityEvent. Everything downstream reacts to *their* state, not to DayDirector directly (with one exception: `DayEnvironmentResponse` also reads `DayDirector.Current` each frame for flicker/leak/radio behavior).

```
FloodController (Flood)
  height, riseSpeed, surgePerMetre (rise accelerates as water gets higher)
  HeightChanged event (C# Action) + onHeightChanged (UnityEvent<float>)
    ├─ FloodSurface     — moves the water mesh, darkens its color toward "abyss" as it rises
    ├─ FloodThreshold×4 — per-object bool flags ("is this now submerged") for local reactions
    └─ FloodDangerMonitor — compares head height (Camera.main) to water height →
         FloodDangerStage enum: Safe → Watch → Warning → Critical → Submerged
         StageChanged event + TimeToSubmersion (seconds, from FloodController.TimeToHeight)
```

`FloodDangerMonitor.Stage` is the closest thing this project has to a single "tension level" signal. It's currently only consumed by the DevGym debug overlay (`DevelopmentDiagnostics`) — nothing audio-related listens to it yet. **This is the natural hook point for intensity-scaled sound.**

```
StormController (Environment)
  intensity, windIntensity, lightningInterval, thunder proximity — set by DayDirector.Apply()
  Update(): schedules random lightning strikes, sets a global shader float _LKStormWetness
  Strike(): flash sequence on stormLight, then plays a thunder clip after a
            distance-based delay (speed of sound), volume falls off with distance
    ├─ WeatherVolume (rain particles)   — emission rate scales with storm.Intensity
    ├─ CoastalFog (fog particles)       — emission scales with storm.Intensity
    └─ AcousticDirector.thunderFilter   — low-pass cutoff opens up when player is exterior
```

`StormController.Intensity` (0.1–1) is the other tension signal, already tied to weather VFX. Nothing in audio currently reads `storm.Intensity` continuously except the thunder low-pass cutoff (which reacts to interior/exterior, not intensity).

## 3. Audio system

```
AudioMixer: LK_Atmosphere.mixer
  Groups: Master, Player, Ambience Music, Weather Exterior, Weather Interior, Ocean,
          Thunder, Electrical, Machinery, Structure, Radio, Water, UI
  Snapshots (7): Interior Normal, Lantern Balcony, Flood Emergency,
                 Lower Mechanical, Generator Running, Power Failure, Exterior Storm
```

```
AcousticDirector (Audio) — runs every frame, is the snapshot state machine
  Reads: Camera.main position, FloodController.Height, all AudioZone volumes in the scene
  Picks a snapshot by simple rule cascade:
    player is on exterior balcony            → "Lantern Balcony"
    flood height is within 1m of player       → "Flood Emergency"   (overrides everything)
    player.y < 3.2                            → "Lower Mechanical"
    player.y < 6.4                            → "Generator Running"
    else                                      → "Interior Normal"
    (any AudioZone trigger volume overrides by highest Priority)
  Transition(): crosstransitions the mixer snapshot over 1.5s, and lerps the
  thunder low-pass filter cutoff toward "bright" outdoors / "muffled" indoors.
```

```
AudioZone (Audio) — a BoxCollider volume placed in a scene with a Snapshot name +
  Priority + Exterior flag. AcousticDirector polls these (re-queried every 2s via
  FindObjectsByType, not events) rather than zones pushing state.
```

```
AmbienceMusic (Audio) — singleton, plays one looping AudioSource forever from Start().
  Only reactive behavior: ducks to 35% volume while `thunder` AudioSource is playing,
  otherwise fades back to full. Sets mixer float "AmbienceMusicVolume" for external control.
  This is the ONE existing "music bed" — currently a flat loop, no intensity scaling at all.
```

```
RandomSoundEmitter (Audio) — generic one-shot player, fires a random clip from an
  array at random intervals (spacing/pitch/volume all randomized). Used for ambient
  detail sounds (creaks, drips, etc.) wherever it's placed. Not intensity-aware.
```

There is a static, always-on ocean-surf `AudioSource` ("Surf below cliff", `Placeholder_Ocean.wav`, routed to the `Ocean` mixer group) authored into `LK_Exterior`, but it's a fixed ambient loop — nothing scales it.

**`FloodTensionAudio` (Audio, new)** — the tension layer. Reads `FloodDangerMonitor.Stage` only (not storm intensity — deliberately excluded per current scope), normalizes it to 0–1 and smooths it over a few seconds. Drives two things:
- Calls the existing `AmbienceMusic.SetVolume(dB)` to swell the already-playing licensed horror-ambience bed (`LK_Pressure_SeamlessLoop.wav`) louder as danger rises, rather than layering in a second track.
- Plays one-shot wave-crash clips (new placeholder `Placeholder_WaveCrash.wav`, generated by `LighthouseAssets.Audio("WaveCrash")`) on a timer whose spacing shrinks and volume/pitch rises with intensity.

`FloodDangerMonitor` itself was previously dead code — defined but never instantiated in any scene. It's wired into the "Lighthouse persistent systems" object (alongside `FloodTensionAudio`) by the idempotent editor step `Lighthouse Keepers → Integration → Add flood tension audio` (`Scripts/Editor/LighthouseFloodTensionAudio.cs`) — run once in the Unity Editor, not at runtime.

## 4. Player / comfort

```
XR Origin (rig)
  ├─ PlayerComfort — reads ComfortSettings.asset (ScriptableObject), drives:
  │    move speed, snap vs smooth turn provider (XRI Starter Assets), and a
  │    tunneling vignette whose strength scales with the player's own movement/turn speed
  ├─ HeadBoundaryComfort — separate vignette trigger when the head physically
  │    clips into geometry (room-scale leaning), independent of PlayerComfort's vignette
  ├─ BodyPresence — positions/rotates a torso mesh under the head each frame (yaw-smoothed)
  ├─ DesktopPreview — editor-only keyboard fallback (WASD/QE) that feeds the *same*
  │    XRI move/turn providers PlayerComfort configures, so desktop and Quest share
  │    one collision/locomotion path
  └─ RecenterPlayer — manual recenter via XRInputSubsystem
```

`ComfortSettings` is a shared ScriptableObject asset — the README's warning about "changes affect the team, use a branch" refers to this file.

## 5. Puzzle framework (scaffolding only)

`PuzzleSocket` (Scripts/Runtime/Puzzles) is a placeholder marker component: a station ID, a role string, an anchor Transform, a `futureIntent` text note, and one `UnityEvent SignalInteraction()`. 16 of these exist across 6 station identities. None currently drive gameplay logic — they're anchors for the puzzle systems that don't exist yet (per Sprint1Handoff, "roles/anchors/events, not complete puzzles").

## 6. Lobby / networking seam (unused scaffolding)

`Scripts/Runtime/Lobby/` defines `LobbyManager`, `LobbyConfig`, `LobbyPlayer`, `INetworkTransport` and a `LocalLobbyTransport` (loopback, single-machine stand-in). `LobbyManager` never touches networking directly — it only calls through the `INetworkTransport` interface, so a real transport can be dropped in later without touching lobby/UI code. **No actual network transport is implemented**; this is deliberately deferred (see Sprint1Handoff). Not wired into Bootstrap's scene flow at all right now.

## 7. Diagnostics / dev tooling

- `DeviceDiagnostics` — logs raw controller input + FPS every 5s, debug builds only.
- `DevelopmentDiagnostics` — an `OnGUI` overlay (flood height slider/pause, danger stage readout, day-preview buttons). This is the fastest way to preview flood/day state changes without playing through the game.
- `Scripts/Editor/*.cs` (~28 files) — these are **one-time authoring/migration tools**, not runtime code. They live under `Lighthouse Keepers/` menu items (e.g. `01 Configure Quest and Import Starter Assets`, `03 Validate Sprint Foundation`, `Integration/Open complete environment`). They were used to build and validate the foundation; they don't run in the player build. Don't confuse them with the Runtime/ scripts.
- `Scripts/Tests/` — `FoundationTests`, `IntegratedAssetTests`, `LobbyTests`, `WeatherCoverageTests`, an EditMode test assembly checked by menu item `03 Validate Sprint Foundation`.

## 8. Folder map

```
Assets/_LighthouseKeepers/
  Scenes/         Bootstrap, Development(DevGym), Environment(Core/Exterior), Levels(01-04)
  Scripts/
    Runtime/       Core, Player, Environment, Flood, Progression, Audio, Puzzles, Lobby
    Editor/        one-time authoring/validation tools (menu: "Lighthouse Keepers/…")
    Tests/         EditMode tests
  ScriptableObjects/  Day1-5.asset, FloodProfile.asset, ComfortSettings.asset
  Audio/          Ambience, Foley, Machinery, Mixer(LK_Atmosphere), Ocean, Radio, Structure, Thunder
  Prefabs/        Architecture, Environment, Interaction, Lighting, Player, PuzzleSockets, Systems, VFX
  Art/            Lighting, Materials, Models, Textures, VFX
  ThirdPartyVariants/  vendor-derived, Quest-safe variants (see ThirdPartyAssets.md)
  Settings/       Input, RenderPipeline(URP), XR
```

## 9. Tension audio (implemented) and future UX hooks

Flood-danger-driven audio scaling is implemented via `FloodTensionAudio` (section 3). `StormController.Intensity` remains unused as a sound driver — out of scope for now by design, not an oversight.

`FloodTensionAudio.Intensity` is a public 0–1 float — the same smoothed signal driving music/crashes. Any future UX (a danger vignette, a HUD pulse, controller haptics) should read this property rather than re-deriving its own mapping from `FloodDangerMonitor.Stage`, so all tension-reactive feedback stays in sync.

Known separate issue, not yet investigated: reported movement/interaction bugginess on the Quest headset specifically around stair traversal and puzzle-socket interaction. Not part of this audio pass.
