# Reconciled One Lighthouse candidate — verification

Base: `sprint3/one-lighthouse` at `d2e30665a44b29a4e3bf84a7bfb7b049f93317b4`; integrated in the existing PR #65. This evidence replaces the earlier PR candidate's old-world numeric-keypad/legacy-control checks. The published e88f4e1 APK remains historical and does not contain this Lab integration.

## Changes under verification

- One alphanumeric six-character crew keypad; no duplicate numeric keypad beneath Play Solo.
- Lab crew launch rejects two connected keepers at UI and raw transport boundaries while preserving offline solo, roster/readiness and host-alone diagnostics. Lab shared gameplay is not implemented.
- Controller Watch Menu, real return-to-lobby cleanup, same-day failed retry and Day 1 replay after completion; tracked rig/hand transforms preserved.
- One APK pipeline for current build menus, validated against actual BuildPlayerOptions scenes: Network Lobby then Lab. BuildReport, source/scene/options record and final binary verification remain required.
- Architecture checks recognize the explicitly retained networking adapter assemblies and declared test tooling, while keeping new gameplay/simulation free of network dependencies and the pure core free of Unity dependencies.

## Coordinated results

The integrated source's full Edit Mode, relevant Play Mode, rendered UI, Python verifier and fresh APK results will be recorded here after execution. Earlier teammate M4 records in Docs/OneLighthouse/M4.md describe their own source and are not substituted for this run.

No target-headset acceptance is claimed from compilation, Editor execution, packaging, installation or a running process. A real video, immersive cold launch/restart, controller reach/readability, physical station use, sound mix and device performance remain separate checks. Co-op acceptance cannot pass while shared Lab state is unfinished.

Raw Unity command lines/runtime output are omitted from published reports; sanitized NUnit results retain test names, results and timings.
