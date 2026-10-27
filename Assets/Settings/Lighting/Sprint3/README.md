# Sprint 3 lighting outputs

STATUS: [PROPOSED]. New lightmaps have not been baked in this sandbox.

Run LighthouseKeepers.Environment.Sprint3.Editor.Sprint3LightingBake.Bake in a graphics-enabled Unity 6000.3.23f1 Editor. It generates LightingPreset_Powered.asset and LightingPreset_Fault.asset, independent map sets, probe coefficients and BakeFingerprint.txt here. LuminanceValidator.Run then measures both using the same original powered reference and writes LuminanceFingerprint.txt.

The scene build gate rejects missing/stale outputs. Existing blue and Half-preview maps are not red-fault bake evidence. Reproduction commands and pending device checks are in Assets/Prefabs/Environment/Sprint3/README.md.
