# Provided soundtrack verification — November 17, 2026

The supplied Lighthouse Keepers track is the continuous music bed; the earlier sample contributes exactly 01:11–01:16 as a five-second accent. Processing, levels, event list and reproduction are in [PlayerSoundtrack.md](../../../Design/PlayerSoundtrack.md).

## Passed

- 18 focused checks: PlayerSoundtrackTests (5), AudioPassTests (7), DreadPlaytestTests (6). Includes streaming/stereo import, exact accent duration, no overlap/restart, cooldown, rejection of stale events and existing speech ducking.
- Actual muted Editor Play Mode: lobby plays the new bed quietly; offline Night 2 plays it continuously. Injected crisis during an accent does not restart either clip. The accent ends while the bed advances. An injected Night 2 possession threat produces a new accent after cooldown.
- Source copies checked by SHA-256; post-encode loudness and true peaks measured with ffmpeg. Main 396.593 s / -20.00 LUFS-I / -3.80 dBTP; accent 5.000 s / -18.02 LUFS-I / -2.35 dBTP. These are clip measurements, not live headset mix measurements.
- No replicated-state or network-protocol change in this music slice. Existing two music sources are reused.

## Not verified here

Editors stayed muted. Worn listening balance, perceived seam smoothness, sustained Quest CPU/GPU and a full human co-op run require the headset. The Play Mode fixture injects events; it is not a completed Night 2 run. Earlier arrival/authentication pair evidence is in ../Arrival/ and ../Day3/Authentication/.

The importer is Tools/Audio/import_player_soundtrack.py. Test bridge reproduction: run PlayerSoundtrackTests, AudioPassTests and DreadPlaytestTests in EditMode after a completed refresh, then adapt this folder's play.py and ScoreProbe.cs to an artifacts output directory. Keep EditorUtility.audioMasterMute=true.
