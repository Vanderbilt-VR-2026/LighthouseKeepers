# November 16 — brief scares and physical cleanup

Latest headset feedback supersedes the long stationary apparition and pole plan. Work stays on sprint-5 / PR89; no main merge. Editor audio stays muted. Install at delivery, do not auto-launch.

## Changes being verified

- Fuel can: dynamic gravity on release, host owns dropped physics, eight-second return; either hand.
- One shared flare pistol beside the wrench on the House bench, one round/night. Reserved second network slot cannot fire.
- Remove the pictured F4 glass/door lamps, housings and cable branches. Exposed F4 runs and the suspended junction are removed; the remaining risers terminate below the lantern deck. Rebuilding the routes cannot restore the glass-mounted fixture.
- Red wheel needle sits in front of the target, extends to the same ring.
- Pole retired; original UV-correct solid floor/collision restored on F2/F3/F4. No pole narration or sign. Archives stay on disk.
- Arm-swing run: joystick movement plus alternating free tracked hands, up to +60%, eased in/out. No bob, FOV change or camera manipulation; no run charge from stationary waving or tool use. Existing pose replication carries movement.
- Host-selected brief encounters: 1.15 s lunge, 1.8 s ceiling glimpse, 4.8 s watcher. Ceiling requires a real underside and clear volume/visibility; falls back to standing watcher if none fits. Short localized head/shoulder distortion; no screen distortion/strobe. Actual face remains at least 1.5 m away. Off still removes appearances/audio.
- Darker room illumination (.60 → .34) and emergency envelope (.115–.1725 → .065–.10). Readable board, controls and stair cues retained. Broad voltage sags even on mains; no false power-state changes.
- Quiet floor-supported wet approach sounds behind/alongside the keeper reuse the existing world audio source. Surface/body detail and head anatomy revised; these remain original low-poly models, not scanned characters.
- Radio duck reduced from −9 to −3 dB; voice chat −6 dB. Pre-scare music dip capped at −6 dB instead of −24. Music is never stopped by narration.

Network **LK-S5-DREAD1 / LK-Lab-Coop-v33**. New `poseKind` is host-owned; new cue range 102–104 and single-gun interaction inventory require both clients to update.

## Additional recording lines

Temporary offline macOS Daniel TTS, one take, processed mono44.1kHz/−18LUFS target. Existing teammate recordings untouched. Replace using the existing radio importer:

| Cue | File | Text | Trigger |
|---|---|---|---|
|102|keeper_run.wav|Swing your arms while you move, keeper. Don't stop here.|After the first encounter has left; at most once/night, normal spacing.|
|103|keeper_flare.wav|One flare on the bench. Save it for a clear shot.|Night2+ opening after15s, if round available; at most once/night.|
|104|keeper_watched.wav|I heard that too. Keep moving, keeper.|Later-night encounter aftermath, normal queue/spacing.|

Generated lines use estimated word-weighted subtitle reveal, not claimed forced alignment. Pole cue86 remains archived but unreachable. Final evidence and device limitations will be recorded in Next.md.

## Editor evidence

Before: 56 focused checks passed (`artifacts/remaining/m3/dread-before.json`). After: 89 focused checks passed (`dread-focused-final.json`). Graphics review4 passes actual left/right fuel-can release and eight-second recovery, closed pole floors, single bench pistol, front wheel pointer and supported ceiling placement. Review7 passes the final fixture/wiring audit with zero unexpected collider contacts. Wiring now has 10,912 triangles. These are geometry counts, not GPU timings.

Solo Night3 actual-XRI replay passes pipe, fuel/primer/choke/starter, radio/beacon, both steered ships, sleep toNight4, zero logged errors (`artifacts/s5/dread/solo-night3/solo/result.json`). Screenshots were opened and inspected. The creature models remain visibly low-poly/stylized; new poses and short appearances need worn-headset acceptance.

The first drop fixture carried residual throw velocity because it released immediately after teleporting the hand. Settling the test hand before release restored the intended stationary-drop check; production physics were not weakened. The short ceiling search originally put the face inside the two-metre exclusion zone; a longer, clearance-tested search fixes that. Wiring revisions that crossed guard geometry were rejected and their exposed F4 runs removed.
