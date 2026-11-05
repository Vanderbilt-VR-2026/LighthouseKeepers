from pathlib import Path
import sys,json
import numpy as np
from scipy.signal import butter,sosfilt
from process_radio_takes import ROOT,decode,save,measure
from process_music import OUT,normalize,loop_samples
source=Path(sys.argv[1]);report=[]
for name,file in [('exterior','Rain Heavy Loud.mp3'),('interior','Rain Heavy Quiet Interior.mp3'),('roof','Rain On Rooftop.mp3')]:
 p=source/'Rain'/file;a=decode(p);seconds=len(a)/44100-1.5;tmp=ROOT/f'Recordings/Rain/{name}.wav';dest=OUT/f'rain_{name}.ogg';
 if not dest.exists():save(tmp,loop_samples(a,0,seconds,1));normalize(tmp,dest)
 m=measure(dest);report.append({'file':file,'seconds':seconds,'lufs':m['input_i'],'truePeak':m['input_tp']});print(report[-1],flush=True)
rng=np.random.default_rng(417)
for name,seconds in [('window',.12),('metal',.32),('puddle',.24),('drip',.19),('wind',17)]:
 t=np.arange(round(seconds*44100))/44100;n=rng.normal(size=len(t));low=sosfilt(butter(2,900,fs=44100,output='sos'),n)
 if name=='wind':x=sosfilt(butter(2,[90,1600],btype='bandpass',fs=44100,output='sos'),n);x=loop_samples(x,0,16,.15)
 elif name=='puddle':x=(low*2+np.sin(2*np.pi*(500*t-650*t*t))*.2)*np.exp(-t*25)
 else:
  # Broadband surface transients, not pitched mallet notes.
  band=sosfilt(butter(2,[900 if name=='metal' else 1400,6500],btype='bandpass',fs=44100,output='sos'),n)
  decay={'window':75,'metal':50,'drip':55}[name]
  x=(band+.15*low)*np.exp(-t*decay)

 x[:220]*=np.linspace(0,1,220);p=ROOT/f'Recordings/Rain/{name}-original.wav';save(p,x/max(1,float(np.max(np.abs(x))))*.8);normalize(p,OUT/f'{name}.ogg' if name=='wind' else OUT/f'impact_{name}.wav')
(ROOT/'artifacts/audio-pass/phase4/processed.json').write_text(json.dumps(report,indent=2))
