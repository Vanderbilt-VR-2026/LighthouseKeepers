"""Offline numbered-memo splitter. Sources are read-only; intermediates stay in Recordings/Radio.
Run with artifacts/audio-pass/tools-env/bin/python after local Whisper word transcription.
Explicit cut overrides belong in Tools/VoicePass/cut_overrides.json, never in source recordings.
"""
import json,re,difflib,wave
from pathlib import Path
import numpy as np
from scipy.signal import butter,sosfilt
ROOT=Path(__file__).resolve().parents[2]
RAW=ROOT/'Recordings/Radio/voice-pass'; OUT=ROOT/'artifacts/voice-pass'
SMALL=['zero','one','two','three','four','five','six','seven','eight','nine','ten','eleven','twelve','thirteen','fourteen','fifteen','sixteen','seventeen','eighteen','nineteen']
TENS=['','','twenty','thirty','forty','fifty','sixty','seventy','eighty','ninety']
def norm(t):return re.findall(r"[a-z0-9]+",t.lower().replace('’',"'").replace("'s",' is').replace("'re",' are').replace("'ll",' will').replace("n't",' not'))
def number(s):
 s=re.sub(r'[^a-z0-9 -]','',s.lower()).strip()
 if s.isdigit():return int(s)
 v=s.replace('-',' ').split()
 if len(v)==1 and v[0] in SMALL:return SMALL.index(v[0])
 if v and v[0] in TENS:
  n=TENS.index(v[0])*10
  if len(v)==1:return n
  if len(v)==2 and v[1] in SMALL[:10]:return n+SMALL.index(v[1])
 return None
def mappings():
 rows=[]
 for line in (ROOT/'Docs/Audio/recording-script.md').read_text().splitlines():
  if not re.match(r'^\| \d+ \|',line):continue
  c=[s.strip().strip('`') for s in line.split('|')[1:-1]]
  rows.append(dict(number=int(c[0]),memo=int(c[1].split()[-1]),gameId=c[2],id=75+int(c[2][1:]) if c[2].startswith('P') else int(c[2]),file=c[3],text=c[4]))
 assert len(rows)==94;return rows
def words(memo):return [w for s in json.loads((OUT/f'memo{memo}-transcript.json').read_text())['segments'] for w in s['words']]
def markers(ws,first,last):
 found=[];expected=first;i=0
 while i<len(ws):
  n=number(ws[i]['word']);width=1
  if n and n%10==0 and i+1<len(ws):
   combined=number(ws[i]['word']+' '+ws[i+1]['word'])
   if combined==expected:n=combined;width=2
  if n==expected:
   found.append(dict(number=n,begin=ws[i]['start'],end=ws[i+width-1]['end'],wi=i,after=i+width));expected+=1;i+=width
  else:i+=1
 assert expected==last+1,('Missing marker',expected,found)
 return found
def build_cuts():
 overrides=json.loads((ROOT/'Tools/VoicePass/cut_overrides.json').read_text());rows=mappings();allcuts=[]
 for memo in range(1,5):
  ws=words(memo);candidates=[]
  pools=[ws]
  if memo==2:
   pools=[ [w for s in json.loads(p.read_text())['segments'] for w in s['words']] for p in sorted((OUT/'refined').glob('memo2-*.json'))]+[ws]
  for pool in pools:
   for wi,w in enumerate(pool):
    for count in [1,2]:
     if wi+count>len(pool):continue
     n=number(' '.join(t['word'] for t in pool[wi:wi+count]))
     if n is not None:candidates.append((n,w['start'],pool[wi+count-1]['end']))
  islands=json.loads((OUT/f'memo{memo}-speech-islands.json').read_text());subset=[r for r in rows if r['memo']==memo];anchors=[];prior=0
  for row in subset:
   n=row['number'];override=overrides.get(str(n),{})
   if override:begin,end=override['markerStart'],override['markerEnd']
   else:
    options=[c for c in candidates if c[0]==n and c[2]>prior+.6]
    assert options,('missing number',n)
    _,begin,end=min(options,key=lambda c:c[2])
   # ASR often assigns the preceding silence to a spoken number. Snap to its isolated voice island.
   nearest=min(islands,key=lambda g:abs(g[1]-end)+max(0,g[1]-g[0]-.95)*3)
   assert abs(nearest[1]-end)<.8,(n,begin,end,nearest)
   begin,end=nearest;anchors.append((row,begin,end,override));prior=end
  for i,(row,begin,end,override) in enumerate(anchors):
   limit=anchors[i+1][1]-.15 if i+1<len(anchors) else 1e9
   speech=[g for g in islands if g[0]>=end+.15 and g[1]<=limit]
   assert speech,('no speech',row['number'])
   start=max(end+.08,speech[0][0]-.08);stop=speech[-1][1]+.08
   onset=[w['start'] for w in ws if end+.1<w['start']<start and start-w['start']<.5 and number(w['word']) is None]
   if onset:start=max(end+.08,min(onset)-.08)
   result=dict(row,start=round(start,3),end=round(stop,3),attempt=override.get('attempt',1),selectionReason=override.get('reason','Single attempt after numbered slate'),duration=round(stop-start,3))
   for kind in ['original','converted']:
    with wave.open(str(RAW/f'memo{memo}-{kind}.wav')) as source:
     sr=source.getframerate();source.setpos(round(start*sr));data=source.readframes(round((stop-start)*sr))
    target=RAW/'cuts'/f"{row['number']:03d}-{kind}.wav";target.parent.mkdir(exist_ok=True)
    with wave.open(str(target),'wb') as dest:dest.setnchannels(1);dest.setsampwidth(2);dest.setframerate(sr);dest.writeframes(data)
   allcuts.append(result)
 (OUT/'cuts.json').write_text(json.dumps(allcuts,indent=2));return allcuts
if __name__=='__main__':
 for c in build_cuts():print(c['number'],c['gameId'],c['start'],c['end'],c['duration'])
