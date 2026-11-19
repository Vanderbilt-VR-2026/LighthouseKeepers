# Walkie audio and House-to-gallery traversal — November 19

The walkie created its holster-click AudioSource on the same object where the grip component later created another source. Unity rejected the shared fear filter during scene startup. The holster sound now follows the walkie on its own child object, leaving each filtered source independent. Volume, grip, carry and voice logic are unchanged; there is no network version change.

The existing traversal test initially stopped on that audio error. After the audio fix it reached the House door but stopped because the in-world briefing was still open, which intentionally pauses the tutorial gate's timeout. The locomotion-only fixture now uses the briefing's existing Skip action first. With the Editor muted, its simulated XR rig walked from the House through all authored routes to the F4 gallery and radio standing area: [Play Mode result](traversal-final.json). [16 focused EditMode checks passed](editmode.json) for radio/belt, crew voice and audio. The [initial audio failure](traversal-before.json) and [intermediate gate result](traversal-after-audio.json) are retained.

The separate [clearance audit](navigation-clearance.json) still reports **61 one-metre route-edge intersections** with the F2/F3 and F3/F4 stair liners; it did not report a blocked player capsule in that audit. The successful centre-line traversal does not prove the full authored width, every possible player position, or headset comfort. Keep the clearance issue open until the liner, route and a worn walk are reviewed. The traversal uses simulated input, skips the briefing and does not test real controllers, co-op voice, audio quality, or a complete night. No APK was built or installed.

Reproduce in Unity 6000.3.23f1 with the Pipeline bridge open, using a fresh output directory. The fixture mutes the Editor before the test and polls the asynchronous Play Mode run:

```sh
LK_TEST_OUT="$PWD/artifacts/s5/campaign-audit/traversal-repro" python3 Docs/Verification/S5/RadioAudio/fixtures/traversal.py
```

Focused EditMode: `python3 artifacts/s5/modes/check.py RadioBeltTests ToolBeltTests CrewSpatialVoiceTests AudioPassTests`.
