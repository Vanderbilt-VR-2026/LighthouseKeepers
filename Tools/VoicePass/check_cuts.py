import os,json,time
from pathlib import Path
import certifi
os.environ['SSL_CERT_FILE']=certifi.where()
import whisper,torch
torch.set_num_threads(6);model=whisper.load_model('small.en',device='cpu')
out=Path('artifacts/voice-pass/clip-transcripts-final');out.mkdir(exist_ok=True)
for row in json.loads(Path('artifacts/voice-pass/cuts.json').read_text()):
 n=row['number'];p=out/f'{n:03d}.json'
 if p.exists():continue
 r=model.transcribe(f'Recordings/Radio/voice-pass/cuts/{n:03d}-original.wav',language='en',word_timestamps=True,fp16=False,condition_on_previous_text=False,temperature=0,verbose=None)
 p.write_text(json.dumps(r,indent=2));print(n,r['text'],flush=True)
