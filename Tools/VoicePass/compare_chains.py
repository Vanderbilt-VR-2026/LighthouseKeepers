"""Offline proxy A/B (not a substitute for spatial headset listening)."""
import os,json,subprocess,sys,re,difflib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(Path(__file__).parent));from import_clips import ff
OUT=ROOT/'artifacts/voice-pass/ab';OUT.mkdir(parents=True,exist_ok=True)
import torch,whisper
torch.set_num_threads(6);model=whisper.load_model('small.en');entries=json.loads((ROOT/'Docs/Audio/voice-manifest.json').read_text())['entries'];rows=[]
for number in [1,5,18,30,68]:
 e=next(x for x in entries if x['number']==number);row={'number':number,'text':e['text']}
 for name,chain in [('old','highpass=f=350,lowpass=f=3500,equalizer=f=2500:t=q:w=0.8:g=3'),('new','highpass=f=250,lowpass=f=4500,equalizer=f=2500:t=q:w=0.8:g=1')]:
  path=OUT/f'{number:03}-{name}.wav';ff(['-v','error','-i',str(ROOT/e['file']),'-af',chain,'-ac','1','-ar','44100',str(path)])
  r=model.transcribe(str(path),fp16=False,language='en',temperature=0,condition_on_previous_text=False,verbose=None);norm=lambda s:re.findall(r"[a-z0-9]+",s.lower());score=difflib.SequenceMatcher(None,norm(e['text']),norm(r['text']),autojunk=False).ratio();row[name]={'text':r['text'].strip(),'wordMatch':round(score,4)}
 rows.append(row);print(row,flush=True)
(OUT/'results.json').write_text(json.dumps(rows,indent=2)+'\n')
text='# Radio clarity A/B\n\nFive identical imported recordings compared through old 350–3500 Hz/+3 dB presence and new 250–4500 Hz/+1 dB presence offline proxies. Both inputs already have +2.5 dB offline presence. Local Whisper small.en, no prompt. This compares recognition, not subjective listening or Unity reverb/occlusion. Headset intelligibility remains a listening check.\n\n| Script | Old match | New match | Old recognition | New recognition |\n|---|---:|---:|---|---|\n'
for r in rows:text+=f"| {r['number']} | {r['old']['wordMatch']:.1%} | {r['new']['wordMatch']:.1%} | {r['old']['text']} | {r['new']['text']} |\n"
text+='\nRuntime: gentler radio colour, faint hiss reduced from .008 to .003 under speech, shorter .22 s reverb with lower wet gain. Squelch opening .3 s and closing .4 s remain. Hiss follows the same room gain as speech. Old recording-specific dropout points are removed to avoid obscuring new words. Existing music/rain ducking remains. No voice-chat recorder, mono input, echo cancellation or transport changes.\n'
(ROOT/'Docs/Audio/radio-ab.md').write_text(text)
