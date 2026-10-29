# Sprint 2 release

Base: sprint2/final-submission / archive/sprint2-final-submission, c299d6cca227421bf066dcdd37b84e7a587d4632. Separate sprint2/release worktree.

Authorized identity-only changes: Android package com.lighthousekeepers.sprint2; product name Lighthouse Keepers S2. This permits installation beside Sprint 3. No gameplay, scene, input or day-loop changes. Builder remains LighthouseKeepers.Editor.Environment.EnvironmentBuild.BuildAndroid, using LK_Lobby_Environment then LK_Lab_Environment.

VR startup/package verification and any strictly necessary startup fixes will be recorded below. The original offline lobby uses TAKE YOUR POST → SIGNAL READY → BEGIN WATCH; it does not have Sprint 3’s PLAY SOLO button. The user explicitly chose to preserve these original buttons. Both the lobby and Lab use the local transport and need no Photon connection.
