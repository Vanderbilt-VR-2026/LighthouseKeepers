# Five-person workflow

| Owner | Main scene / responsibility |
|---|---|
| Environment artist | Exterior, shared Core, lighting/materials |
| Flood developer | Level01 and flood components |
| Generator developer | Level02, generator props/anchors |
| Communications artist | Level03/04 and radio/lantern anchors |
| Integrator | Bootstrap, XR, audio, tests, build validation |

Use a feature branch and small pull request. Announce scene/prefab ownership before editing. Avoid simultaneous edits to Core, Bootstrap, shared materials or the player prefab. Additive scenes reduce conflicts but do not eliminate shared-asset conflicts. Keep the exact Unity version and package lockfile consistent.

Text serialization and visible metadata are enabled. Move assets within Unity and commit asset/meta pairs. Do not regenerate GUIDs. Never commit Library, Temp, Logs, Obj, Build, Builds or UserSettings. .gitignore already covers these. Run the validation menu and relevant tests before a PR; attach an editor preview and any device evidence. No remote was added or pushed by this implementation.

Binary files are marked binary in .gitattributes. Git LFS is not installed on this machine. The current small originals are ordinary Git files; before introducing large FBX/PSD/WAV source files, agree on LFS installation across all five machines, then add filter=lfs rules in a dedicated PR. Do not introduce filters that silently leave classmates with pointer files they cannot resolve.

Static batches retain source renderers disabled. Editing source geometry requires updating or disabling its batch. Generate missing scenes only for recovery; do not casually regenerate team-authored scenes.
