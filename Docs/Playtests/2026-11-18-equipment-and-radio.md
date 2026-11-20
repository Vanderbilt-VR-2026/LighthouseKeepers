# Equipment and paired-station playtest — November 18

User feedback, issue #142: pipe valve hard to locate and teleports after release; lantern clips into hand, returns home after being set down, and stays lit underwater; needs belt carry for walkie; radio destination/action instructions unclear; code appears in fragments and blocks progress; microphone grip/trigger unclear; wants frequency changes each watch; both roles see the code; remove cloth/lens wipe; Owen nose artifact; F3 bedroom partition lacks collision; keep timed generator pulls but add one partner hold action. Apply shared fixes to Campaign and Survival. Final voice recordings remain last; no AI voice.

## Design decisions

- One fixed upstream pressure-release handle serves all three pipe bolts. No control is moved between bolts. The release handle opens a bypass, with gauge feedback; this is a simplified game interaction. Real isolation/depressurisation reference: [Spirax Sarco](https://content.spiraxsarco.com/-/media/spiraxsarco/international/documents/en/im/t3-im-p625-02-en.ashx?rev=fad5890a2f9f4d9aa1a79d91c2d1eb98).
- Generator partner holds a compression-release assist lever while the other keeper makes the existing timed pulls. A simplified cooperative version of a real starting aid, not a claim that a real portable generator needs two operators. [YANMAR YDG operator manual](https://www.yanmar.com/eu/industrial/wp-content/uploads/sites/11/2021/06/YANMAR-Operation-Manual-YDG-Series.pdf).
- Full ordered radio code stays together. Machinist reads the House telegram; Lightkeeper requests it by walkie. Daily frequency is derived from the replicated watch, stable on retry/rejoin.

## Evidence so far

Radio/recall changes compile; 20 focused EditMode checks pass (RadioRelay 5, CampaignOrder 5, SurvivalMode 7, RadioBelt 3). Worn readability and physical reach remain pending. The final delivery below supersedes the previous installed build.

Props: removed the 20-second automatic return for lantern/walkie (out-of-world recovery remains); host extinguishes submerged flame without deleting oil; two hand-specific bail poses, static pressure bypass and visible belt holster. 15 focused rule tests pass (Lantern 8, PressureValve 4, RadioBelt 3). Physical headset acceptance pending.

Generator: red START ASSIST lever on the engine. Partner holds while operator makes three timed pulls; refilling stays a simple tilt. Solo Owen assists the human or makes the pulls for the Machinist duty. Host checks proximity and distinct actors; release/timeout/leave reset hold. Network LK-S5-COOPFIX1 / LK-Lab-Coop-v50. 39 focused tests pass, including required hold, same-actor rejection, replica, timeout, retry and lone-human fallback. Live co-op/headset pending.

World pass: removed the physical rag and all live soot/wipe requirements from Campaign and Survival, with regression checks across five days in both modes. All 76 authored stair backing panels now block bodies; rebuilt navigation passes 20/20 routes and swept collision checks. Fixed the editor baker to detach registered native navigation before replacing its asset (a prior attempt crashed the Editor). Powered and emergency lighting re-baked with lower room/window fill; directional lantern cookie retains near-hand illumination. No added realtime lights.

Owen: baked beard colour into skin instead of a duplicate shell, removed projecting whisker tubes, seated brows against the face, and recessed the mouth interior. Three close-up views inspected. First-person body vertex positions, triangle count and skeleton are unchanged; headset face acceptance and fine seam shading still need checking. New generator assist label moved clear of fuel signage.

Verification: 40 focused EditMode checks passed on the combined world/radio/starter/companion/survival/lantern/avatar change. Actual XR hover + manual grip/trigger readers passed 16 Play Mode checks: walkie grab/belt/retrieval, lantern bail attachment/set-down beyond 20 seconds, fixed valve through repair/release, radio lever lift, daily tune, full code and mic transmission. Live submerged-lantern check extinguished the flame while preserving oil. Owen navigated to F2 and held assist, then walked to the House telegram and reported all four code symbols together. These are scripted Editor checks, not worn Quest acceptance or a full human playthrough.

Evidence: `artifacts/s5/interaction-fixes/{tests,physical/result.json,bot/result.json,navigation/routes.txt,live/}`. Lighting output: `artifacts/s5/quality/`; powered/emergency completion recorded in the Editor log.

The first two-Editor assist test found the command whitelist still ending at BriefingComplete, silently rejecting EngineAssist. Extended the whitelist; the existing all-actions/replay test now passes (2 command-window, 4 timed-starter, 3 equipment-world checks). The repeat live transport passed before build.

## Final delivery

Two muted live Editors passed the repeat: actual guest assist grip/release accepted and replicated, fixed valve grip accepted, role-specific clue, lantern/radio sync, leave/rejoin, ship passage, shared sleep into Day 2 and retry. Station preparation and ship advancement were scripted. No audible voice or worn full-night acceptance is claimed.

APK source **ad33062a** built successfully as IL2CPP ARM64 and installed on Quest 3 without launch. Installed SHA-256 matches `d667f80dca4b6d9ce2bdc5982ad594cc902fd8d1fbcf04905931e0a2e103b1f0`. Network **LK-S5-COOPFIX1 / LK-Lab-Coop-v50**. [Durable evidence, screenshots and eight-step headset checklist](../Verification/S5/EquipmentRelay/README.md). #142 stays open for headset acceptance; final voice recording remains #122.

During packaging, macOS reported a Unity Bug Reporter using 95.31 GB. Stopped both stale reporter processes, preserved their logs locally and left other user applications alone. The Editors had already been closed; the APK build finished normally. No permanent crash-reporter memory fix is claimed.
