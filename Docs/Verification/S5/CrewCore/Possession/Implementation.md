# Night 2: the false call and hand rescue

Source work on sprint-5 / PR #89, November 17. Refs #107, #139, #140.

The false call and the line “You’ll die here like the others.” use the speaker name **Unknown**, including B recall. Other instruction lines retain the real harbor master's label. All dialogue is text only. No synthetic speech was added.

After 60 seconds of Night 2 watch time, both keepers are called to Floor 2. The sequence waits for both human keepers, or for the solo player and the walking helper, to reach the actual flat floor. Two physical stair gates then close. After the threat, the guest in co-op or the helper in solo is possessed. Grip their offered left hand to revive them; release preserves progress. The gates reopen after the rescue and a short relief beat. The watch, flood and ship clocks pause throughout this teaching sequence; the lesson has no rescue deadline. A roster change releases the victim and regathers the pair, rather than trapping a late join or remaining player.

The character wears a knitted beanie. Five additional facial joints roll the eyes upward, lower the jaw and reveal narrow dark red tears. The visible body rises gradually up to 0.32 m, capped by a ceiling query, then lowers as the hand rescue progresses. Tracked head/camera transforms are never animated by this effect. Hand IK keeps the physical offered hand reachable; the accepted leg/height and headless local-body implementation remains in place. Reduced-scare mode keeps the hand-rescue lesson with quiet captions and suppresses the face/lift/wall effect.

The host owns the lesson phase, victim and elapsed phase time inside the existing recovery snapshot. Clients render that snapshot and submit the existing authenticated grip action. New compatibility version: **LK-S5-POSSESSION1 / LK-Lab-Coop-v40**. Both clients need this version.

## Verification

75 focused EditMode checks pass (12 recovery/lesson, 8 avatar asset/pose, 8 humanoid, 23 voice/version, 22 M3 runtime, 2 instruction checks). The offline helper navigates to Floor 2, is possessed, and is revived with release/regrip while the watch/flood stay paused. The live two-Editor run verifies both Unknown subtitle/recall labels, the guest victim, unchanged tracked head height, two blocking gates on each client, partial grip/release/regrip, and shared recovery/unlock. Receipts and inspected screenshots are alongside this document. The first transport attempt disconnected; the final run explicitly keeps both Editors running in the background and passes. Earlier failed attempts remain in local `artifacts/s5/possession/`: a rig capsule incorrectly masked the floor ray, and an initial helper waiting point was too close to the stair gate. Those are not passing evidence.

## Not verified / still separate work

- Quest hand reach, seated comfort, eye readability, wall/gate placement and sustained performance require a person in the headset. Muted Editor checks do not verify human voice.
- This is a scripted rescue lesson, not a finished hunter/catch system. Full catches remain disabled.
- Night 1 still needs the requested safe full-station rehearsal. Night 3/4 bespoke scenes and the revised Night 5 finale remain work on #140. The queued three-timed-pulls engine change is #137.

## APK delivery

Clean source `fcfa8bd4` built successfully: `Builds/LighthouseKeepers-Sprint5-Possession-fcfa8bd4.apk` (129,950,078 bytes). Verified package `com.lighthousekeepers.game`, IL2CPP ARM64, VR manifest, signing and ZIP CRC. SHA-256 `8670c7850997e358ab251336d83096902dfa982e6a6bbd171a8ae781616e4d42`. Not installed or launched; Quest unavailable. See `delivery.json` and `Headset.md`. The earlier preflight attempt encountered the Editor shutdown lock; the clean retry completed after the Editors exited.
