# Input-driven networking proof — October 29, 2026

[Watch the five-second clip](FirstPerson.mp4). One first-person view shows a separately controlled client moving sideways, waving, jumping and leaving; the received avatar then disappears. No labels, overlays, diagrams or watermark.

**PASS:** two independent Unity processes (host 71105, client 71154) exchanged Photon poses with zero runtime errors. All **21 host-received pose sequences and positions exactly matched independently transmitted client poses**. Input commands were delivered only to the client; the host has no access to the command file and no timed remote animation. This uses automated local test input, with normal independent keyboard controls also available.

The actual client leave removed the remote avatar **0.096 seconds** after the command, at host capture time 4.317 seconds. The avatar remains absent for the rest of the clip. See `commands.json`, `host-traffic.jsonl`, `client-traffic.jsonl`, both result JSON files and `verification.json` for the command → transmitted pose → received pose chain and disconnect check.

Unity 6000.3.23f1 built the non-development macOS player successfully (`PROOF_BUILD_SUCCEEDED`, 128,459,529 bytes). Log: `/private/tmp/lk-live-network-build.log`. Raw capture: `Builds/NetworkingProof/run-20261029-210623`. Export: exactly 5.00 seconds, 1280×720, H.264, 15 FPS; decoded MP4 frames were visually checked, including the final empty view after disconnection. The SHA-256 identifies the delivered video.

[Reproduce](../../NetworkingProof.md). Execution used checkout `5217496` plus the isolated proof source. Both PRs carry these fixture-only changes; full target-branch regressions, live headset input, full lighthouse handoff and production-rate throughput were not verified. Production networking is unchanged, and existing XR simulator work is excluded.
