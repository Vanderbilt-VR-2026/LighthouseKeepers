# Input-driven first-person networking proof

The five-second clip shows one player's view of a separate Photon client moving, waving, jumping, then leaving. The remote avatar disappears when that client leaves. There are no labels, overlays, or lighthouse environment.

Each instance supports independent **WASD** movement, **Space** jump, **E** wave, and **Escape** leave. The host has no automatic remote-avatar animation or shared choreography clock: partner presentation comes only from `StormWatchFusionLink.RemoteInput`. Poses send at 5 Hz on the existing Fusion connection.

## Reproduce

Save open Unity scenes, then run from the repository root:

```sh
/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath "$PWD" -buildTarget StandaloneOSX \
  -executeMethod LighthouseKeepers.Editor.NetworkingProofBuild.Build \
  -quit -logFile /private/tmp/lk-live-network-build.log
tools/NetworkingProof/record.sh
```

The script starts two independent players. For reproducibility, `drive.py` delivers live input commands through an explicitly selected file read **only by the client**. The host cannot read this file; it records the resulting received movement and the client's actual leave. These automated test inputs are not a human-input claim. Omit `-proof-control` to control either instance with its keyboard. Omit `-proof-quit` for an ongoing interactive session.

`verify.py` matches every host-received pose sequence and position against the client's transmit trace, checks lateral movement and jumping, and checks that leaving removes the partner within the recorded five-second interval. The encoder then creates one first-person H.264 clip, without playback speedup. macOS Swift/AVFoundation and Python 3 are used; no Unity package is added.

The isolated non-development scene and build helper preserve the production scene list. Raw captures/logs stay in ignored `Builds/NetworkingProof/run-*`. See [the video and executed evidence](Verification/NetworkingProof/README.md). This is desktop transport evidence; live XR tracking, full lighthouse handoff and headset acceptance remain separate.
