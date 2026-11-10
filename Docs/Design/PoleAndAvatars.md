# Continuous pole and William's avatars/water

The lead confirmed build 6626b39's pole works and tracker text remains readable, then requested a reachable shaft with a side plate, circular floor openings and a full F4-to-ground descent. They also requested integration of William Qian's PR #86.

## Integration

PR #86, `william/sprint-4` at `f9b8fd5`, is merged into `s4/integrated`, not main. Captain appearance goes to the host, rigged fisherman to the guest. Existing replicated head/controller/finger poses drive the appearance; no new network fields. Local tracked hands and their rotation remain unchanged. Mono voice/AEC and gameplay authority are unchanged. Matchmaking remains LK-S5-VOICE1 / snapshot v19; both testers should use this new APK to see the same world.

Will's Interior Flood Water material replaces the earlier runtime StormWater override. Existing tower/House flood heights and rules remain authoritative. The doorway sheet, debris, leak and runoff remain; the surface uses Will's lit ripples. No sky or ocean changes.

## Pole

One continuous brass shaft sits on the east side at (1.7, 1.1) in plan. Real 32-sided circular apertures pass through F2/F3/F4, with matching non-convex floor collision and metal liners. F1 remains solid. Guarded entries are at 3.3, 6.6 and 9.9 m; descent ends at ground level beside the House route. The top mounting reaches the lantern ceiling. It is a downward shortcut, not an upward elevator.

The plate sits beside the west gate, outside the reach corridor. Grip initiates a controlled slide; release pauses and regrip continues. Only the local keeper temporarily ignores the pole enclosure collision. Other players retain fall protection. Movement-off configurations retain the structural guards and disable the grab interaction.

F3's surveyed walking route now passes to the west of the shaft. The wall bunk moves 0.3 m toward its wall and 0.3 m along it, clearing the F3 enclosure without blocking the approach from the stairs; its pose is shared by builder/runtime and saved in the scene. F4's beacon control plate moves along the north wall to (1.0, 9.9, 2.4), leaving room to operate it without standing in the pole enclosure. Corresponding standing-space records retain their radius and are surveyed at the active generator, bunk and beacon locations; old station identifiers remain intact.

`Lighthouse Keepers/Rebuild continuous pole openings` cuts the original UV-corrected meshes, preserves interpolated normals/tangents/UV/lightmap UVs and submeshes, saves the scene and shell-prefab references, and is repeatable. `LighthouseBuilder` reuses these aperture meshes on shell rebuild. The layout/manifest hash changes describe route/anchor changes; original FBX geometry is untouched.

## Verification

Results and screenshots: [verification](../Verification/PoleAndAvatars/README.md). Device comfort and performance are separate from Editor results.
