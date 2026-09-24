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

## Team GitHub repository

Repository: https://github.com/Vanderbilt-VR-2026/LighthouseKeepers

The initial foundation is published on `aneesh/lighthouse-sprint1-foundation`. The local `team` remote points to the class repository; `origin` retains Aneesh’s existing personal remote. The branch connects both initial histories without rewriting either. Review against team `main`, which initially used Unity 6000.3.23f1; this foundation requires 6000.3.23f1, matching team `main`. The editor version is now agreed.

For this checkout, subsequent committed work is pushed with `git push team aneesh/lighthouse-sprint1-foundation`. Stage named assets and their metadata together; inspect `git diff --staged` before committing. Do not use `git add .` to scoop up duplicate sync files or unrelated local edits. The newer uncommitted XR/material/settings edits present at publication were deliberately left local because they were not part of the verified build.

Open a pull request to `main`, assign a teammate to review, and describe tests and headset limitations. Do not merge directly into main or force-push shared branches. After the foundation is reviewed and merged, start each task from updated main on a branch such as `aneesh/fix-stair-collision`. Keep subsequent PRs focused by scene or system.

### Current editor version

The subsequent compatibility migration targets **Unity 6000.3.23f1** to match the team. The original 6000.5 build is historical; do not upgrade the migrated branch back to 6000.5. See Unity63Migration.md for validation evidence and the preserved pre-migration checkpoint.
