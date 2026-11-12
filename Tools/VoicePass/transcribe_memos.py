import os,json,time
from pathlib import Path
import certifi
os.environ['SSL_CERT_FILE']=certifi.where()
import whisper,torch
os.environ['TOKENIZERS_PARALLELISM']='false';torch.set_num_threads(6)
out=Path('artifacts/voice-pass');model=whisper.load_model('small.en',device='cpu')
for i in range(1,5):
 p=out/f'memo{i}-transcript.json'
 if p.exists():continue
 print('TRANSCRIBING',i,flush=True);start=time.time()
 r=model.transcribe(str(Path('Recordings/Radio/voice-pass')/f'memo{i}-original.wav'),language='en',word_timestamps=True,fp16=False,condition_on_previous_text=False,verbose=False)
 p.write_text(json.dumps(r,indent=2));print('DONE',i,time.time()-start,flush=True)
