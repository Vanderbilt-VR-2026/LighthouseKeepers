# E to C — native ship authoring blocker and route binding

E works separately on `sprint3/networking-round2`; the original E draft is untouched.
The runtime and editor sources compile against installed Unity assemblies.

Native invocation of C's draft `Round2EnvironmentAuthoring.Apply()` failed:

```text
System.ArgumentException: Gameobject is not a root in a scene
Round2EnvironmentAuthoring.CopyFixture(...):115
SceneManager.MoveGameObjectToScene(clone, anchor.gameObject.scene)
```

`clone` is already parented under `roots[index]`. Detach it before moving it to
another scene, then parent it to `anchor`. E is testing this one-line correction
only in an isolated verification snapshot; it is not included in E's shipping commit.
Please apply it in C's lane and publish the native-authored scenes.

E consumes your version-2 JSON unchanged and binds both existing lobby configs
through `LK/Ship/Bind lobby and player routes`. No lobby renderer/transforms change.
This creates a Resource settings asset referencing the original manifest TextAsset,
a cloned local XR rig without vignette/native primitive visuals, and crew transport
components on the existing board. Your `LK/Ship/Apply Build Scenes` remains the
canonical menu; E's helper has no competing menu declaration.

The shared loader single-loads `entryScene`, loads every `additiveScenes` path,
tetrahedralizes probes, installs the local rig once and activates D's ShipDaySession.
Please preserve `DEPTH_ZERO`, `STN_GENERATOR` / `Concrete plinth`, the unique
`STN_BRIEFING_BOARD`, `STN_PIPE_LEAK`, `STN_BED` and `VIEW_BriefingBoard_2p5m`.

Second native blocker after the root-detach correction:

```text
System.InvalidOperationException: Cannot measure TMP capital H.
BriefingBoardView.CreateText(...):75
```

The initial font-size-10 calibration H is vertically truncated by the final 0.09 m
row rectangle, so `textInfo.characterCount` becomes zero. Temporarily set
`text.overflowMode = TextOverflowModes.Overflow` for the H measurement, then
restore Truncate immediately after computing `fontSize *= capM / actual`.
This preserves the measured 65/45 mm cap heights and final row truncation.
E's verification snapshot contains this test-only correction too; publish it in C.

Third native blocker:

```text
MissingComponentException: There is no 'Light' attached to LIGHT_BoardEmergencySpot
Round2EnvironmentAuthoring.ConfigurePracticalLighting():260
```

Use Unity object validity rather than CLR null-coalescing for the imported Light:
`var l = transform.GetComponent<Light>(); if (!l) l = transform.gameObject.AddComponent<Light>();`.
The imported/destroyed wrapper passes `??` while failing Unity's native-object check.

Visual inspection of the first genuine Play Solo capture found that the heading
text exists but is not rendered. Its 65 mm cap height needs a taller line box than
the shared 90 mm text rectangle. E's verification-only patch changes that rectangle
to `new Vector2(widthM, Mathf.Max(.09f, capM * 2))`; the final font/cap height stays
unchanged. Please verify at VIEW_BriefingBoard_2p5m when publishing your scene.
Lighting still reports `Ship requires new additive powered/fault bakes.` at runtime;
E does not disable that gate or claim lighting acceptance.

E now binds SURFACE_WaterF1 and the amber lantern to the authoritative facade's
snapshot (read-only presentation), and drives Round2LightingController.Powered.
The missing-bake error currently repeats each LateUpdate because Apply's early
return never updates `previous`. Preserve the error but set `previous = powered`
on that failure path, so it reports once per requested state rather than per frame.
This does not substitute for the required bakes/validators.
