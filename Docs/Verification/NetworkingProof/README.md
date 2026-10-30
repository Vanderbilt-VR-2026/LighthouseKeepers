# Five-second first-person proof — October 29, 2026

[Watch the clip](FirstPerson.mp4): exactly **5.00 seconds**, 1280×720, H.264, 15 FPS, one first-person view. No labels, overlays, diagrams, watermark, or lighthouse background. The partner waves and meets the local keeper's hand.

**PASS:** two independent macOS Unity processes connected through the existing Photon Fusion transport. Host PID 68941 and client PID 68982 each received 35 remote pose updates, observed hand proximity during the high-five, and reported zero runtime errors. Local poses are scripted; the partner is drawn exclusively from received poses. See the result JSON files and `verification.json`.

The non-development player built successfully in Unity 6000.3.23f1 (`PROOF_BUILD_SUCCEEDED`, 128,457,477 bytes). Local log: `/private/tmp/lk-firstperson-build.log`. Capture: `Builds/NetworkingProof/run-20261029-193942`. The MP4 contains 75 frames sampled without speedup, and decoded frames at 1, 3 and 4 seconds were visually checked. `Video.sha256` identifies the delivered clip.

Reproduce with [the demo guide](../../NetworkingProof.md). This isolated scene sends poses at 5 Hz and the shared capture clock at 2 Hz; production networking is not changed. This is scripted desktop transport evidence, not live XR tracking, full lighthouse handoff, production-rate throughput, or headset acceptance. An earlier higher-rate long-form attempt did not pass; this low-rate proof does not establish that the production traffic issue is resolved.

The recording was built and run in the working checkout at `5217496` plus the isolated proof files. The PRs add these files to the latest fetched `main` and `sprint-3` heads; full target-branch regression suites were not rerun. Existing XR simulator work is excluded.
