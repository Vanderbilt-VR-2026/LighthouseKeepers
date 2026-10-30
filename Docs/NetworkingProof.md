# First-person networking proof

A five-second, single-view clip shows the other keeper waving and meeting your hand. There are no labels, overlays, diagrams, or lighthouse environment. The camera sits at the local desktop keeper's eyes.

Two separate macOS Unity players connect through the existing Photon `FusionLobbyTransport` and `StormWatchFusionLink`. Each generates only its own scripted hand poses; the partner avatar uses received poses. Poses send at 5 Hz and a shared capture clock at 2 Hz. This is a basic desktop transport proof, not live XR tracking or headset acceptance.

## Reproduce

Save open Unity scenes, then run from the repository root:

```sh
/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath "$PWD" -buildTarget StandaloneOSX \
  -executeMethod LighthouseKeepers.Editor.NetworkingProofBuild.Build \
  -quit -logFile /private/tmp/lk-firstperson-build.log
tools/NetworkingProof/record.sh
```

The isolated `LK_NetworkingProof` scene builds as a non-development desktop player to omit the watermark. The production build scene list is not changed. The script launches the host and client, verifies independent process IDs, received hand movement and observed hand proximity, then encodes exactly five seconds from the host's first-person view. macOS Swift/AVFoundation and Python 3 are used; no Unity package is added.

Raw captures, logs and JSON results remain under ignored `Builds/NetworkingProof/run-*`. Missing capture slots hold the preceding frame without speeding up playback. The encoder decodes representative frames from the actual MP4 for visual inspection. See [the recording and executed evidence](Verification/NetworkingProof/README.md).
