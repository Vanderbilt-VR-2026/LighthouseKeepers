# Equipment and paired-station playtest — November 18

User feedback, issue #142: pipe valve hard to locate and teleports after release; lantern clips into hand, returns home after being set down, and stays lit underwater; needs belt carry for walkie; radio destination/action instructions unclear; code appears in fragments and blocks progress; microphone grip/trigger unclear; wants frequency changes each watch; both roles see the code; remove cloth/lens wipe; Owen nose artifact; F3 bedroom partition lacks collision; keep timed generator pulls but add one partner hold action. Apply shared fixes to Campaign and Survival. Final voice recordings remain last; no AI voice.

## Design decisions

- One fixed upstream pressure-release handle serves all three pipe bolts. No control is moved between bolts. The release handle opens a bypass, with gauge feedback; this is a simplified game interaction. Real isolation/depressurisation reference: [Spirax Sarco](https://content.spiraxsarco.com/-/media/spiraxsarco/international/documents/en/im/t3-im-p625-02-en.ashx?rev=fad5890a2f9f4d9aa1a79d91c2d1eb98).
- Generator partner holds a compression-release assist lever while the other keeper makes the existing timed pulls. A simplified cooperative version of a real starting aid, not a claim that a real portable generator needs two operators. [YANMAR YDG operator manual](https://www.yanmar.com/eu/industrial/wp-content/uploads/sites/11/2021/06/YANMAR-Operation-Manual-YDG-Series.pdf).
- Full ordered radio code stays together. Machinist reads the House telegram; Lightkeeper requests it by walkie. Daily frequency is derived from the replicated watch, stable on retry/rejoin.

## Evidence so far

Radio/recall changes compile; 20 focused EditMode checks pass (RadioRelay 5, CampaignOrder 5, SurvivalMode 7, RadioBelt 3). Worn readability and physical reach remain pending. No new APK delivered yet.
