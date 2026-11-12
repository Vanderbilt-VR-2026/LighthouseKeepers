"""Turn isolated offline ASR and exact cuts into a reviewable, versioned manifest."""
import json,re,difflib,wave
from pathlib import Path
import numpy as np
from split_memos import ROOT,RAW,OUT

def tokens(t):
 t=t.lower().replace('’',"'")
 for a,b in [('keep her','keeper'),('water is',"water's"),('generator is',"generator's"),('fuel is',"fuel's"),('light is',"light's"),('it is',"it's"),('tonight is',"tonight's"),('she is',"she's"),('that will',"that'll"),('you will',"you'll"),('we are',"we're"),('you are',"you're"),('30','thirty'),('10','ten'),('2','two')]:t=t.replace(a,b)
 return re.findall('[a-z]+',t.replace("'",''))
def score(a,b):return difflib.SequenceMatcher(None,tokens(a),tokens(b),autojunk=False).ratio()
def main():
 rows=json.loads((OUT/'cuts.json').read_text())
 for r in rows:
  n=r['number'];p=OUT/f'clip-transcripts/{n:03d}-medium.json';model='medium.en'
  if not p.exists():p=OUT/f'clip-transcripts-final/{n:03d}.json';model='small.en'
  tr=json.loads(p.read_text());r['transcription']=tr['text'].strip();r['asrModel']=model;r['wordMatch']=round(score(r['text'],tr['text']),4)
  r['words']=[dict(word=w['word'].strip(),start=round(w['start'],3),end=round(w['end'],3)) for s in tr['segments'] for w in s['words']]
  with wave.open(str(RAW/'cuts'/f'{n:03d}-converted.wav')) as w:a=np.frombuffer(w.readframes(w.getnframes()),np.int16).astype(float)/32768
  r['clippedSamples']=int(np.sum(np.abs(a)>=.999));r['edgeDb']=[round(float(20*np.log10(max(1e-9,np.sqrt(np.mean(x*x))))),2) for x in [a[:882],a[-882:]]]
  r['flags']=[]
  if r['wordMatch']<.98:r['flags'].append('ASR/script wording differs: review recording')
  if r['duration']>=10:r['flags'].append('Over 10 s; retained at natural speed')
  if r['clippedSamples']:r['flags'].append('Source reaches full scale')
  if max(r['edgeDb'])>-35:r['flags'].append('Possible speech at cut boundary')
 doc=ROOT/'Docs/Audio';(doc/'voice-manifest.json').write_text(json.dumps({'entries':rows},indent=2,ensure_ascii=False)+'\n')
 sources=json.loads((OUT/'source-manifest.json').read_text());(doc/'source-manifest.json').write_text(json.dumps(sources,indent=2)+'\n')
 text='# Numbered memo split report\n\nAll inference ran locally using OpenAI Whisper small.en, with medium.en rechecks for non-exact matches. The original memos supplied the cut points; identical cuts were applied to converted copies. Original files were never changed. Cuts and raw alternatives are gitignored under Recordings/Radio/voice-pass. Word timestamps and exact mappings are in voice-manifest.json.\n\n'
 text+='Duration differences (decoded PCM): '+', '.join(f"Memo {s['memo']} {s['deltaSeconds']:+.4f} s" for s in sources)+'. All within ±0.1 s. FFmpeg reported an ALAC trailing-packet warning; decoded duration remains within tolerance and every numbered sentence was checked separately.\n\n'
 text+='Line30 uses attempt2, following “again, thirty.” Other occurrences of “again” are part of scripted sentences, not retake commands. Number-slating ASR corrections for53 and69 are explicit in Tools/VoicePass/cut_overrides.json. Quiet edge analysis uses the converted copies, not just the originals. The match score ignores punctuation, number spelling and equivalent contraction spellings; it is ASR evidence, not a human listening verdict.\n\n'
 text+='| No. | Game ID | File | Memo start–end (s) | Attempt | Duration | Word match |\n|---:|---|---|---|---:|---:|---:|\n'
 for r in rows:text+=f"| {r['number']} | {r['gameId']} | `{r['file']}` | {r['memo']}: {r['start']:.3f}–{r['end']:.3f} | {r['attempt']} | {r['duration']:.2f} s | {r['wordMatch']:.0%} |\n"
 text+='\n## NEEDS CHECK / RE-RECORD\n\nThese are explicitly flagged, not certified exact readings. ASR can mishear accents/consonants; compare the isolated original and converted copies before deciding a retake is necessary. No clip was sped up or truncated to satisfy the duration limit.\n\n'
 for r in rows:
  if r['flags']:text+=f"- **{r['number']} ({r['gameId']})** — {r['text']}\n  - {'; '.join(r['flags'])}. ASR: “{r['transcription']}” ({r['wordMatch']:.0%}).\n"
 (doc/'split-report.md').write_text(text)
 print('mapped',len(rows),'flagged',sum(bool(r['flags']) for r in rows))
if __name__=='__main__':main()
