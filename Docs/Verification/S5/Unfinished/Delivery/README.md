# Verified Quest installation

Clean source **b32eae50**, branch sprint-5, PR #89. IL2CPP ARM64, normal package `com.lighthousekeepers.game`. APK package/VR category/signature/ZIP integrity verified. Installed on the connected Quest 3; installed base APK SHA-256 matches the local file. **Not launched.**

APK: `/Users/aneeshvasamreddy/LighthouseKeepers-M3/Builds/LighthouseKeepers-Sprint5-CrewTraining-b32eae50.apk`

SHA-256: `6a8fc3b422c30d3ea548578902657f829a8d0e3e25e6be702fed947ee194277c`

Network **LK-S5-TRAINING1 / LK-Lab-Coop-v47**. Both clients must update.

The earlier fbd7082d APK was built but not installed; b32eae50 includes the helper breaker-budget correction. Documented builder settings were preserved under `artifacts/s5/unfinished/build-corrected/generated-settings/` and restored after the build. No project audio settings were changed. Editors stayed muted during verification.

[Complete headset route](../Headset-Check.md). Worn performance, comfort, readability, human voice and full campaign remain NOT VERIFIED. New Night 3 hunter/hiding and complete Demo remain implementation work, not device-only checks.

Reproduce: use `artifacts/s5/unfinished/build-final.py` and `deliver-final.py` with a fresh backup/output directory for another source build. The current paths have been consumed. Never auto-launch.
