# Duplicate lighting recovery — September 22, 2026

After the successful build and commits, 44 untracked files appeared beside the authored lighting assets: 22 duplicate lightmap/lighting/reflection files and 22 metadata copies with mismatched names. Their origin was not established. Unity Editor was closed (only Hub/licensing processes were running).

All 44 copies were moved without deletion into the Git-ignored `Builds/RecoveredDuplicateLighting-20260922/`, with original filenames and SHA-256 hashes recorded in its manifest.json. They are retained locally for review. Canonical tracked assets and GUIDs were left untouched. Keeping the malformed copies inside Assets would cause orphan metadata and duplicate-GUID import problems.

Additional duplicate ProjectSettings files remain untracked and untouched; inspect `git status --short` for the current list. If new duplicates continue appearing, investigate the external file synchronization process before opening Unity. No cause has been assumed or system sync settings changed.
