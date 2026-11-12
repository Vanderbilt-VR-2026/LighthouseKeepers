"""Process timestamped single takes; originals and raw cuts remain read-only."""
from pathlib import Path
import json,subprocess,uuid,wave,re,difflib,concurrent.futures
ROOT=Path(__file__).resolve().parents[2]
FF=str(Path.home()/'.local/bin/ffmpeg')
BASE='highpass=f=80,afftdn=nr=6:nf=-40,deesser=i=0.15:m=0.35:f=0.5,equalizer=f=2800:t=q:w=0.8:g=2.5,acompressor=threshold=0.125:ratio=2:attack=10:release=80'
def ff(args):return subprocess.run([FF,'-hide_banner','-nostdin','-y',*args],capture_output=True,check=True)
def measure(p):
 s=ff(['-i',str(p),'-af','loudnorm=I=-18:TP=-1.5:LRA=7:print_format=json','-f','null','-']).stderr.decode();return json.loads(s[s.rfind('{'):s.rfind('}')+1])
def process(e):
 raw=ROOT/f"Recordings/Radio/voice-pass/cuts/{e['number']:03}-converted.wav";tmp=raw.with_name(f"{e['number']:03}-processed.wav");dest=ROOT/e['file'];dest.parent.mkdir(parents=True,exist_ok=True)
 ff(['-v','error','-i',str(raw),'-af',BASE,'-ac','1','-ar','44100','-c:a','pcm_s16le',str(tmp)])
 m=measure(tmp);norm='loudnorm=I=-18:TP=-1.5:LRA=7:measured_I={input_i}:measured_TP={input_tp}:measured_LRA={input_lra}:measured_thresh={input_thresh}:offset={target_offset}:linear=true'.format(**m)
 ff(['-v','error','-i',str(tmp),'-af',norm,'-ac','1','-ar','44100','-c:a','pcm_s16le',str(dest)])
 m=measure(dest)
 # Peak-constrained short lines may miss the integrated target; gently limit only their peaks.
 for _ in range(3):
  error=-18-float(m['input_i'])
  if abs(error)<.35:break
  correction=tmp.with_name(tmp.stem+'-level.wav')
  ff(['-v','error','-i',str(dest),'-af',f'volume={error}dB,alimiter=limit=0.81:attack=5:release=80:level=false:latency=true','-ac','1','-ar','44100','-c:a','pcm_s16le',str(correction)])
  dest.write_bytes(correction.read_bytes());m=measure(dest)
 with wave.open(str(dest)) as w:duration=w.getnframes()/w.getframerate();assert(w.getnchannels(),w.getframerate(),w.getsampwidth())==(1,44100,2)
 assert float(m['input_tp'])<=-1.2,(e['number'],m)
 meta=Path(str(dest)+'.meta')
 if not meta.exists():meta.write_text((ROOT/'Assets/Audio/Radio/keeper_arrival.wav.meta').read_text().replace('60606595eeada4ad5aa784d17aa7fd5b',uuid.uuid4().hex))
 return {'number':e['number'],'id':e['id'],'file':e['file'],'duration':duration,'lufs':float(m['input_i']),'truePeakDb':float(m['input_tp'])}
def norm(s):return re.sub('[^a-z0-9]','',s.lower())
def timing(e):
 # Preserve script punctuation/text. Map matching words to recorded starts; interpolate unmatched words.
 tokens=list(re.finditer(r'\S+',e['text']));heard=e['words'];times=[None]*len(tokens)
 matcher=difflib.SequenceMatcher(None,[norm(t.group()) for t in tokens],[norm(w['word']) for w in heard],autojunk=False)
 for a,b,n in matcher.get_matching_blocks():
  for k in range(n):times[a+k]=max(.08,heard[b+k]['start'])
 for i,t in enumerate(times):
  if t is None:
   lo=next((j for j in range(i-1,-1,-1) if times[j] is not None),-1);hi=next((j for j in range(i+1,len(times)) if times[j] is not None),len(times));start=times[lo] if lo>=0 else .08;end=times[hi] if hi<len(times) else e['duration']-.08;times[i]=start+(end-start)*(i-lo)/(hi-lo)
 for i in range(1,len(times)):times[i]=max(times[i-1],times[i])
 return {'id':e['id'],'number':e['number'],'file':e['file'],'caption':e['text'],'duration':e['duration'],'wordStarts':times,'characterEnds':[t.end() for t in tokens],'needsCheck':bool(e['flags']),'longSingleTake':e['duration']>=10}
def main():
 entries=json.loads((ROOT/'Docs/Audio/voice-manifest.json').read_text())['entries']
 with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:rows=list(pool.map(process,entries))
 (ROOT/'Docs/Audio/processing-report.json').write_text(json.dumps({'chain':BASE,'targetLufs':-18,'truePeakCeiling':-1.2,'entries':rows},indent=2)+'\n')
 data={'entries':[timing(e) for e in entries]};target=ROOT/'Assets/WatchDemo/Resources/HarborVoiceTiming.json';target.write_text(json.dumps(data,indent=2,ensure_ascii=False)+'\n')
 strings=ROOT/'Assets/Data/Strings/Resources/KeeperStrings.json';s=json.loads(strings.read_text());by={r['key']:r for r in s['entries']};changes=[]
 asset=ROOT/'Assets/WatchDemo/Resources/HarborRadioLines.asset';text=asset.read_text();before,rest=text.split('  subtitles:\n');old,after=rest.split('  pauseCenters:\n');caps=[json.loads(x[4:]) for x in old.splitlines() if x.startswith('  - ')]
 for e in entries:
  i=e['id'];previous=caps[i-1] if i<=18 else by.get('radio.'+str(i),{}).get('text','(new)')
  if previous!=e['text']:changes.append(f"| {e['number']} | {e['gameId']} | {previous} | {e['text']} |")
  if i<=18:caps[i-1]=e['text']
  else:
   for key,value in [('radio.'+str(i),e['text']),('radio.file.'+str(i),Path(e['file']).stem)]:
    if key in by:by[key]['text']=value
    else:s['entries'].append({'key':key,'text':value})
 asset.write_text(before+'  subtitles:\n'+''.join('  - '+json.dumps(c,ensure_ascii=False)+'\n' for c in caps)+'  pauseCenters:\n'+''.join('  - {x: -10, y: -10}\n' for c in caps))
 strings.write_text(json.dumps(s,indent=2,ensure_ascii=False)+'\n')
 (ROOT/'Docs/Audio/caption-changes.md').write_text('# Exact script captions\n\nScript numbers differ from game cue IDs. Unrecorded IDs are untouched. Timing uses original word alignment; unmatched ASR tokens are interpolated, not substituted into captions. Old dropout locations were cleared because they belong to different recordings.\n\n| Script number | Cue | Previous caption | Exact script caption |\n|---|---|---|---|\n'+'\n'.join(changes)+'\n')
 print(json.dumps({'imported':len(rows),'lufsRange':[min(r['lufs'] for r in rows),max(r['lufs'] for r in rows)],'maxTruePeak':max(r['truePeakDb'] for r in rows),'long':[r for r in rows if r['duration']>=10]},indent=2))
if __name__=='__main__':main()
