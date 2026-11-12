import os,json,re,difflib,time,sys
from pathlib import Path
import certifi
os.environ['SSL_CERT_FILE']=certifi.where()
import whisper,torch
sys.path.insert(0,'Tools/VoicePass')
from split_memos import mappings

def tokens(t):
 t=t.lower().replace('’',"'")
 for a,b in [('keep her','keeper'),('water is',"water's"),('generator is',"generator's"),('fuel is',"fuel's"),('light is',"light's"),('it is',"it's"),('tonight is',"tonight's"),('she is',"she's"),('that will',"that'll"),('you will',"you'll"),('we are',"we're"),('you are',"you're"),('30','thirty'),('10','ten')]:t=t.replace(a,b)
 return re.findall('[a-z]+',t.replace("'",''))
def score(a,b):return difflib.SequenceMatcher(None,tokens(a),tokens(b),autojunk=False).ratio()
if __name__=='__main__':
 torch.set_num_threads(6);model=whisper.load_model('medium.en',device='cpu')
 for row in mappings():
  n=row['number'];p=Path(f'artifacts/voice-pass/clip-transcripts-final/{n:03d}.json')
  while not p.exists():time.sleep(1)
  r=json.loads(p.read_text());s=score(row['text'],r['text'])
  if s>=.999:continue
  out=Path(f'artifacts/voice-pass/clip-transcripts/{n:03d}-medium.json');out.parent.mkdir(exist_ok=True)
  if out.exists():continue
  r=model.transcribe(f'Recordings/Radio/voice-pass/cuts/{n:03d}-original.wav',language='en',word_timestamps=True,fp16=False,condition_on_previous_text=False,temperature=0,verbose=None)
  out.write_text(json.dumps(r,indent=2));print(n,round(s,3),round(score(row['text'],r['text']),3),r['text'],flush=True)
