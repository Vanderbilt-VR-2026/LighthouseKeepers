# November 9 voice-build headset feedback

The lead confirmed the imported voice is clear and each clip contains one take. They then reported excessive/late narration, tracker text disappearing behind walls at F1/generator, and a nonworking, odd-looking pole.

- Narration: current-context checks, stale queue expiry, two-line burst limit with 20-second recovery, fewer redundant lines. See triggers.md.
- Tracker: all tracker TMP text now uses the same SDF overlay material as subtitles, drawing above both world geometry and its dark backing. Its world-fixed placement and wrist/button recall remain unchanged.
- Pole: reproduced failure at F3. The merged `Playtest edge guard F3 320° stairwell edge` blocked alignment with the shaft. During a deliberate grip, only the local player's collision with nearby pole-entrance fall guards is ignored; other collision protection stays enabled. The pairs restore on landing/destruction. Both authored drops remain in existing stair voids, with gravity/ordinary movement restored at the bottom. The interactable also registers only its intended grab trigger, avoiding temporary primitive colliders that are destroyed during construction. The model now has two mounting collars, support brackets and a single hinged entry bar, replacing duplicated posts and the detached bar.

Verification evidence will be recorded in verification.md. Editor traversal is not headset comfort acceptance.

Focused verification: normal XRI hover/grip (no forced selection), both hands on both poles, release/pause/regrip, landing and guard restoration passed. Context-expiry/two-line burst-limit and existing 90-second idle-hint tests passed.
