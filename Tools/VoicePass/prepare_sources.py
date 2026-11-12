"""Decode read-only memo sources and measure silence; never edit source files."""
import sys,json,hashlib,subprocess,wave
from pathlib import Path
import numpy as np
from scipy.signal import butter,sosfilt
from split_memos import ROOT,RAW,OUT
source=Path(sys.argv[1]);RAW.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True);rows=[]
for i in range(1,5):
 row={'memo':i}
 for kind,p in [('original',source/'original'/f'Memo {i}.m4a'),('converted',source/'final'/f'Memo {i} post.mp3')]:
  before=hashlib.sha256(p.read_bytes()).hexdigest();dest=RAW/f'memo{i}-{kind}.wav'
  result=subprocess.run(['ffmpeg','-v','warning','-y','-i',str(p),'-ar','44100','-ac','1','-c:a','pcm_s16le',str(dest)],capture_output=True,text=True,check=True)
  (OUT/f'decode-{i}-{kind}.log').write_text(result.stderr)
  assert before==hashlib.sha256(p.read_bytes()).hexdigest()
  with wave.open(str(dest)) as w:seconds=w.getnframes()/w.getframerate()
  row[kind]={'path':str(p),'decoded':str(dest.relative_to(ROOT)),'seconds':seconds,'sha256':before}
 row['deltaSeconds']=row['converted']['seconds']-row['original']['seconds'];assert abs(row['deltaSeconds'])<=.1,row
 rows.append(row)
 with wave.open(str(RAW/f'memo{i}-original.wav')) as w:sr=w.getframerate();a=np.frombuffer(w.readframes(w.getnframes()),np.int16).astype(float)/32768
 a=sosfilt(butter(2,100,fs=sr,btype='highpass',output='sos'),a);r=np.sqrt(np.mean(a[:len(a)//441*441].reshape(-1,441)**2,axis=1));db=20*np.log10(np.maximum(r,1e-9));groups=[]
 for v in np.where(db>-45)[0]:
  if groups and v-groups[-1][-1]<=25:groups[-1].append(v)
  else:groups.append([v])
 groups=[(g[0]/100,(g[-1]+1)/100) for g in groups if len(g)>=4]
 (OUT/f'memo{i}-speech-islands.json').write_text(json.dumps(groups))
(OUT/'source-manifest.json').write_text(json.dumps(rows,indent=2))
