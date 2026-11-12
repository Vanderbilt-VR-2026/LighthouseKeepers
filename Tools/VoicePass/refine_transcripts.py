import os,json,time,sys
from pathlib import Path
import certifi
os.environ['SSL_CERT_FILE']=certifi.where()
import whisper,torch
import numpy as np
from scipy.io import wavfile
torch.set_num_threads(6);model=whisper.load_model('small.en',device='cpu')
out=Path('artifacts/voice-pass/refined');out.mkdir(exist_ok=True)
# Independent overlapping windows stop repetitive status reports skipping a 30-second block.
a=whisper.load_audio('Recordings/Radio/voice-pass/memo2-original.wav')
for n,start in enumerate(range(0,222,12)):
 p=out/f'memo2-{start:03d}.json'
 if p.exists():continue
 r=model.transcribe(a[start*16000:min(len(a),(start+15)*16000)],language='en',word_timestamps=True,fp16=False,condition_on_previous_text=False,temperature=0,verbose=None)
 for s in r['segments']:
  s['start']+=start;s['end']+=start
  for w in s['words']:w['start']+=start;w['end']+=start
 p.write_text(json.dumps(r,indent=2));print(start,r['text'],flush=True)
