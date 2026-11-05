"""Prepare beat-aligned loops and three short sting alternatives. Sources never changed."""
from pathlib import Path
import json,sys,subprocess,shutil
import numpy as np
from scipy.signal import resample_poly
from process_radio_takes import ff,decode,save,measure,ROOT
OUT=ROOT/'Assets/Audio/Resources/AudioPass';OUT.mkdir(parents=True,exist_ok=True)
def normalize(source,dest):
 m=measure(source);f='loudnorm=I=-18:TP=-1.2:LRA=7:measured_I={input_i}:measured_TP={input_tp}:measured_LRA={input_lra}:measured_thresh={input_thresh}:offset={target_offset}:linear=true'.format(**m)
 ff(['-v','error','-i',str(source),'-af',f,'-ac','1','-ar','44100',str(dest)])
def loop_samples(a,start,seconds,fade=.15):
 n=round(seconds*44100);d=round(fade*44100);s=round(start*44100);x=a[s:s+n+d];assert len(x)==n+d
 # Equal-power overlap, exact whole-bar period; tail reaches the same sample as the start.
 t=np.linspace(0,np.pi/2,d);return np.r_[x[d:n],x[n:n+d]*np.cos(t)+x[:d]*np.sin(t)]
def main(source):
 source=Path(source);analysis=json.loads((ROOT/'artifacts/audio-pass/phase3/music-analysis.json').read_text());report=[]
 for row in analysis:
  name=next(n for n in ['calm','tension','crisis'] if n in row['name']);a=decode(row['source']);bar=240/row['bpm'];start=row['crisisBar'] if name=='crisis' else next(t for t in row['bars'] if t>=2)
  count=int(((len(a)/44100)-start-.15)//bar);seconds=count*bar;tmp=ROOT/f'Recordings/Music/{name}-loop.wav';save(tmp,loop_samples(a,start,seconds));normalize(tmp,OUT/f'{name}.ogg')
  report.append({'track':name,'bpm':row['bpm'],'barSeconds':bar,'sourceStart':start,'bars':count,'loopSeconds':seconds});print(report[-1],flush=True)
 stings=[]
 for name,file in [('fixed','LK sting fixed relief.mp3'),('survived','LK sting survived.mp3'),('lost','Lk sting lost.mp3')]:
  a=decode(source/file);window=round(3.5*44100);blocks=np.array([np.mean(a[i:i+window]**2) for i in range(0,len(a)-window,22050)]);chosen=[]
  for i in np.argsort(blocks)[::-1]:
   start=float(i)*.5
   if all(abs(start-s)>4 for s in chosen):chosen.append(start)
   if len(chosen)==3:break
  for j,start in enumerate(chosen,1):
   x=a[round(start*44100):round(start*44100)+window].copy();x[:2205]*=np.linspace(0,1,2205);x[-22050:]*=np.linspace(1,0,22050);p=ROOT/f'Recordings/Music/sting-candidates/{name}-{j}.wav';save(p,x)
   if j==1:normalize(p,OUT/f'sting_{name}.wav')
  stings.append({'sting':name,'candidateStarts':chosen,'chosen':1,'seconds':3.5,'reason':'Highest-energy non-overlapping moment; 50 ms attack, 500 ms tail fade.'})
 (ROOT/'artifacts/audio-pass/phase3/processed.json').write_text(json.dumps({'mode':'CROSSFADE','loops':report,'stings':stings},indent=2))
 doc='# Music\n\nTracks and stings supplied by the team, AI-generated with Google Gemini/Lyria. Original MP3s are untouched and excluded from the build. Reprocess with `Tools/S4/process_music.py <source-folder>` after beat analysis.\n\nMode: **CROSSFADE**, two seconds at the outgoing track’s next bar boundary. Beat estimates include half/double-time ambiguity and the independently arranged tracks have no proven common phase; they are not stacked.\n\n| Track | Estimated BPM | Bar seconds | Source start | Whole bars | Loop seconds |\n|---|---:|---:|---:|---:|---:|\n'
 for r in report:doc+=f"| {r['track']} | {r['bpm']:.2f} | {r['barSeconds']:.3f} | {r['sourceStart']:.3f} | {r['bars']} | {r['loopSeconds']:.3f} |\n"
 doc+='\nCrisis omits the slow build and starts at the detected bar nearest 1:22. Loops use a 150 ms equal-power wrap overlap and retain an exact whole-bar sample period. All are normalized to −18 LUFS before the quieter music bus gain.\n\n| Sting | Three source starts (seconds) | Default | Length |\n|---|---|---:|---:|\n'
 for r in stings:doc+=f"| {r['sting']} | {', '.join(str(x) for x in r['candidateStarts'])} | 1 | 3.5 s |\n"
 doc+='\nCandidates are in gitignored `Recordings/Music/sting-candidates/`. Defaults choose the strongest energy window, with fast attack and a naturalizing tail fade. Listening acceptance is pending. Runtime: any unfinished task → tension; water >70% loss height, <20 s beacon darkness allowance, or <60 s remaining → crisis; all fixed → calm after the current bar. Each task completion plays fixed; shared sleep plays survived; loss plays lost. Guest state comes from the authenticated host snapshot.\n';(ROOT/'Docs/Design/Music.md').write_text(doc)
if __name__=='__main__':main(sys.argv[1])
