# Sprint 1 Quest 3 build — October 1, 2026

Source: PR #23, integration commit `9826c393d09cd17690621cccda0092495f89047b`, containing main `5b4d256`. Sprint-1 had the same project tree as main at verification. The Bootstrap conflict preserves both the walkthrough repair marker and the networking roots. The concurrently published team merge produced the same tree as the local rebase and was retained.

- Unity 6000.3.23f1; Android ARM64/IL2CPP development APK; OpenXR with Meta Quest 3 support.
- Bootstrap plus six environment scenes; development and recovery scenes excluded.
- Edit Mode: 31/31 passed. Play Mode bootstrap integration: 1/1 passed.
- Final Android build: Succeeded, 0 errors, 1 warning (Diagnostics Data requests crash-report debug symbols).
- APK signature: verified using APK Signature Scheme v2. Package `com.lighthousekeepers.game`, version 0.1.0 (code 1), minimum API 32, target API 36, only `arm64-v8a` native libraries.
- APK size: 90,789,089 bytes. SHA-256: `ecd3292c727cb02ab675ce33686240e41cc500532fa740a4b5d5681b79241ade`.

The initial build reported a shader compiler timeout despite a successful build result and was discarded. Reimporting URP Lit and rebuilding with `UNITY_SHADER_COMPILER_TASK_TIMEOUT_MINUTES=60` completed with zero errors. The build helper now rejects any nonzero error count as well as a failed result. No authored scene, package, or Android configuration was changed for this retry.

[Download APK](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/download/sprint1-quest3-20261001/LighthouseKeepers-Sprint1-Environment.apk) · [Release and checksum](https://github.com/Vanderbilt-VR-2026/LighthouseKeepers/releases/tag/sprint1-quest3-20261001)

No Quest installation was performed. On-headset walkthrough, stereo comfort, audio balance, networking behavior, and sustained performance remain unverified. Tests establish scene structure and existing behavior, not device acceptance.
