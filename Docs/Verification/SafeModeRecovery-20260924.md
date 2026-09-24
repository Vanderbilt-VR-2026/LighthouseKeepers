# Safe Mode recovery — September 24, 2026

Unity version remains 6000.3.23f1. The editor log contained **26 distinct C# compilation errors**, all caused by four untracked numbered copies of editor scripts defining classes already present in the canonical files. Errors were CS0101 (duplicate class), CS0111 (duplicate member), and CS0579 (duplicate InitializeOnLoad attribute).

Moved these conflicting files together with their `.meta` files out of Assets, preserving their complete contents under `Builds/SafeModeRecovery-20260924/Assets/_LighthouseKeepers/Scripts/Editor/`:

- LighthouseAssetStoreAccess 2.cs
- LighthouseCirculationWeather 2.cs
- LighthousePlayVerification 2.cs
- LighthouseVendorImport 2.cs

`Builds/SafeModeRecovery-20260924/Recovery.json` records original paths and SHA-256 checksums. Nothing was deleted. Canonical scripts, scene references, packages and existing user settings were not replaced. Other numbered scene/art/settings copies were left intact because they do not cause these compilation errors. Their producer has not been determined; synchronization conflict copies are a possible explanation, not a verified diagnosis. A Git ignore rule would not stop Unity compiling a duplicate `.cs` under Assets.

Added the editor-only **Lighthouse Keepers → Play environment** command to open Bootstrap, enter Play Mode and report interactive startup without closing the game. No runtime gameplay changes were needed to resolve this incident.

## Verification

- Unity `-batchmode -buildTarget Android -executeMethod LighthouseKeepers.Editor.LighthouseIntegratedValidation.RunAndPlay`: PASS, exit0; log `/tmp/lk-safe-mode-recovery-play.log`. Compilation succeeded; seven scenes, one rig/listener, existing puzzle/day/flood checks, movement/snap, soundtrack, moving flood surface and rotating lens passed with no captured runtime errors.
- Edit Mode tests: **9/9 passed**, zero failed/skipped. Result: `SafeModeRecovery-EditMode.xml`; log `/tmp/lk-safe-mode-recovery-tests.log`.
- Interactive launch: **PASS**, seven scenes loaded, Bootstrap ready, zero runtime errors during startup; game left running. Result: `InteractiveLaunch.txt`; log `/tmp/lk-safe-mode-recovery-launch.log`.

These commands use `/Users/aneeshvasamreddy/UnityEditors/6000.3.23f1/Unity.app/Contents/MacOS/Unity`, the symlink to the installed 6000.3.23f1 editor, and this project’s absolute path. No APK rebuild was necessary for duplicate editor-only files; no Quest/device performance claim is made. This report and the Play environment command are included in the follow-up repair commit. The conflicting script copies were never tracked, so their local removal does not appear as a Git deletion. A clean clone does not contain those copies.

## Original compiler errors

```text
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseAssetStoreAccess.cs(11,22): error CS0101: The namespace 'LighthouseKeepers.Editor' already contains a definition for 'LighthouseAssetStoreAccess'
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseAssetStoreAccess.cs(14,22): error CS0111: Type 'LighthouseAssetStoreAccess' already defines a member called 'InspectAccount' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseAssetStoreAccess.cs(29,19): error CS0111: Type 'LighthouseAssetStoreAccess' already defines a member called 'Callback' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseAssetStoreAccess.cs(30,22): error CS0111: Type 'LighthouseAssetStoreAccess' already defines a member called 'Success' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseAssetStoreAccess.cs(34,22): error CS0111: Type 'LighthouseAssetStoreAccess' already defines a member called 'Failure' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseAssetStoreAccess.cs(35,15): error CS0111: Type 'LighthouseAssetStoreAccess' already defines a member called 'Timeout' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseAssetStoreAccess.cs(36,15): error CS0111: Type 'LighthouseAssetStoreAccess' already defines a member called 'Finish' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseCirculationWeather.cs(13,22): error CS0101: The namespace 'LighthouseKeepers.Editor' already contains a definition for 'LighthouseCirculationWeather'
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseCirculationWeather.cs(17,22): error CS0111: Type 'LighthouseCirculationWeather' already defines a member called 'Apply' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseCirculationWeather.cs(72,16): error CS0111: Type 'LighthouseCirculationWeather' already defines a member called 'DoorWidth' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseCirculationWeather.cs(79,22): error CS0111: Type 'LighthouseCirculationWeather' already defines a member called 'ValidateLoaded' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseCirculationWeather.cs(91,22): error CS0111: Type 'LighthouseCirculationWeather' already defines a member called 'RebuildBatches' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthousePlayVerification.cs(17,2): error CS0579: Duplicate 'InitializeOnLoad' attribute
Assets/_LighthouseKeepers/Scripts/Editor/LighthousePlayVerification.cs(18,21): error CS0101: The namespace 'LighthouseKeepers.Editor' already contains a definition for 'LighthousePlayVerification'
Assets/_LighthouseKeepers/Scripts/Editor/LighthousePlayVerification.cs(21,9): error CS0111: Type 'LighthousePlayVerification' already defines a member called 'LighthousePlayVerification' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthousePlayVerification.cs(22,21): error CS0111: Type 'LighthousePlayVerification' already defines a member called 'Run' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthousePlayVerification.cs(24,14): error CS0111: Type 'LighthousePlayVerification' already defines a member called 'State' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthousePlayVerification.cs(25,14): error CS0111: Type 'LighthousePlayVerification' already defines a member called 'Log' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthousePlayVerification.cs(29,14): error CS0111: Type 'LighthousePlayVerification' already defines a member called 'Tick' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthousePlayVerification.cs(76,14): error CS0111: Type 'LighthousePlayVerification' already defines a member called 'Finish' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthousePlayVerification.cs(80,14): error CS0111: Type 'LighthousePlayVerification' already defines a member called 'Capture' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseVendorImport.cs(15,22): error CS0111: Type 'LighthouseVendorImport' already defines a member called 'ImportAndInspect' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseVendorImport.cs(20,15): error CS0111: Type 'LighthouseVendorImport' already defines a member called 'Next' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseVendorImport.cs(25,15): error CS0111: Type 'LighthouseVendorImport' already defines a member called 'Completed' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseVendorImport.cs(26,22): error CS0111: Type 'LighthouseVendorImport' already defines a member called 'Inspect' with the same parameter types
Assets/_LighthouseKeepers/Scripts/Editor/LighthouseVendorImport.cs(9,22): error CS0101: The namespace 'LighthouseKeepers.Editor' already contains a definition for 'LighthouseVendorImport'
```
