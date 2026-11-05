# Music

Tracks and stings supplied by the team, AI-generated with Google Gemini/Lyria. Original MP3s are untouched and excluded from the build. Reprocess with `Tools/S4/process_music.py <source-folder>` after beat analysis.

Mode: **CROSSFADE**, two seconds at the outgoing track’s next bar boundary. Beat estimates include half/double-time ambiguity and the independently arranged tracks have no proven common phase; they are not stacked.

| Track | Estimated BPM | Bar seconds | Source start | Whole bars | Loop seconds |
|---|---:|---:|---:|---:|---:|
| calm | 71.78 | 3.344 | 3.669 | 48 | 160.496 |
| crisis | 71.78 | 3.344 | 82.477 | 25 | 83.592 |
| tension | 143.55 | 1.672 | 3.483 | 95 | 158.824 |

Crisis omits the slow build and starts at the detected bar nearest 1:22. Loops use a 150 ms equal-power wrap overlap and retain an exact whole-bar sample period. All are normalized to −18 LUFS before the quieter music bus gain.

| Sting | Three source starts (seconds) | Default | Length |
|---|---|---:|---:|
| fixed | 45.0, 19.5, 13.0 | 1 | 3.5 s |
| survived | 26.5, 13.5, 20.0 | 1 | 3.5 s |
| lost | 27.0, 40.0, 13.5 | 1 | 3.5 s |

Candidates are in gitignored `Recordings/Music/sting-candidates/`. Defaults choose the strongest energy window, with fast attack and a naturalizing tail fade. Listening acceptance is pending. Runtime: any unfinished task → tension; water >70% loss height, <20 s beacon darkness allowance, or <60 s remaining → crisis; all fixed → calm after the current bar. Each task completion plays fixed; shared sleep plays survived; loss plays lost. Guest state comes from the authenticated host snapshot.
