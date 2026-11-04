"""Offline processing only. Originals are read-only; alternate takes stay gitignored.
Usage: python process_radio_takes.py '/path/to/Lighthouse Keepers voice lines'
Requires numpy and ffmpeg. Change one `take` in radio_takes.json and rerun to switch.
"""
from pathlib import Path
import sys,subprocess,json,wave,hashlib,shutil
import numpy as np
ROOT=Path(__file__).resolve().parents[2];FF=shutil.which('ffmpeg') or str(Path.home()/'.local/bin/ffmpeg');RATE=44100
SETTINGS=Path(__file__).with_name('radio_takes.json')
def ff(args):return subprocess.run([FF,'-hide_banner','-nostdin','-y',*args],capture_output=True,check=True)
def decode(p):return np.frombuffer(ff(['-v','error','-i',str(p),'-ac','1','-ar',str(RATE),'-f','f32le','-']).stdout,dtype='<f4')
def save(p,a):
 p.parent.mkdir(parents=True,exist_ok=True)
 with wave.open(str(p),'wb') as w:w.setparams((1,2,RATE,0,'NONE',''));w.writeframes((np.clip(a,-1,1)*32767).astype('<i2').tobytes())
def measure(p):
 r=ff(['-i',str(p),'-af','loudnorm=I=-18:TP=-1.2:LRA=7:print_format=json','-f','null','-']).stderr.decode();return json.loads(r[r.rfind('{'):r.rfind('}')+1])
def main(source):
 settings=json.loads(SETTINGS.read_text()) if SETTINGS.exists() else {};report=[]
 for p in sorted(Path(source).glob('*.m4a')):
  name=p.stem.lower().replace(' ','_');a=decode(p);n=882;r=np.sqrt(np.mean(a[:len(a)//n*n].reshape(-1,n)**2,axis=1));threshold=max(float(np.quantile(r,.1)*4),float(r.max()*.055));active=np.flatnonzero(r>threshold)
  gaps=[(int(x),int(y)) for x,y in zip(active[:-1],active[1:]) if y-x>25];cuts=sorted(sorted(gaps,key=lambda x:x[1]-x[0],reverse=True)[:2]);assert len(cuts)==2,(name,'Expected three takes')
  starts=[int(active[0])]+[y for x,y in cuts];ends=[x for x,y in cuts]+[int(active[-1])+1];takes=[]
  for i,(s,e) in enumerate(zip(starts,ends),1):
   lo=max(0,round(s*.02*RATE)-3528);hi=min(len(a),round(e*.02*RATE)+3528);x=a[lo:hi];path=ROOT/f'Recordings/Radio/takes/{name}_take{i}.wav';save(path,x)
   takes.append({'take':i,'start':lo/RATE,'end':hi/RATE,'seconds':len(x)/RATE,'clippedSamples':int(np.sum(np.abs(x)>.999)),'noiseRms':float(np.sqrt(np.mean(np.r_[x[:1764],x[-1764:]]**2))),'path':str(path)})
  median=float(np.median([t['seconds'] for t in takes]));chosen=min(takes,key=lambda t:t['clippedSamples']*100+max(0,t['seconds']-10)*10+abs(t['seconds']-median)/median+t['noiseRms']*10)
  if name in settings:chosen=takes[settings[name]['take']-1]
  reason='No clipped samples; clean silence boundaries; middle-length preference with measured edge noise penalty.'
  settings[name]={'take':chosen['take'],'reason':reason}
  dest=ROOT/f'Assets/Audio/Radio/{name}.wav';tmp=ROOT/f'Recordings/Radio/takes/{name}_processed.wav'
  base='highpass=f=80,afftdn=nr=6:nf=-40,acompressor=threshold=0.125:ratio=2:attack=10:release=80'
  ff(['-v','error','-i',chosen['path'],'-af',base,'-ac','1','-ar',str(RATE),str(tmp)])
  m=measure(tmp);norm='loudnorm=I=-18:TP=-1.2:LRA=7:measured_I={input_i}:measured_TP={input_tp}:measured_LRA={input_lra}:measured_thresh={input_thresh}:offset={target_offset}:linear=true'.format(**m)
  ff(['-v','error','-i',str(tmp),'-af',norm,'-ac','1','-ar',str(RATE),'-c:a','pcm_s16le',str(dest)])
  measured=measure(dest);row={'line':name,'selected':chosen['take'],'reason':reason,'seconds':len(decode(dest))/RATE,'lufs':float(measured['input_i']),'truePeakDb':float(measured['input_tp']),'sourceSha256':hashlib.sha256(p.read_bytes()).hexdigest(),'takes':takes};report.append(row);print(name,chosen['take'],round(row['seconds'],2),row['lufs'],row['truePeakDb'],flush=True)
 SETTINGS.write_text(json.dumps(settings,indent=2)+'\n');out=ROOT/'artifacts/audio-pass/phase2';out.mkdir(parents=True,exist_ok=True);(out/'voice-report.json').write_text(json.dumps(report,indent=2))
 assert len(report)==17 and all(r['seconds']<10 and r['truePeakDb']<=-1 for r in report)
 doc=ROOT/'Docs/Design/RadioLines.md';s=doc.read_text().split('\n## Recorded takes')[0];s+='\n## Recorded takes\n\nTeammate recordings replace TTS. Automatic selection uses clipping, edge noise and middle-length preference; subjective delivery awaits headset listening. Every output is one take, mono 44.1 kHz, high-pass 80 Hz, gentle denoise/compression, two-pass −18 LUFS normalization, true peak ≤ −1 dBTP. Unity adds the radio filter. Originals are untouched.\n\n| Line | Take | Seconds | LUFS | True peak dBTP |\n|---|---:|---:|---:|---:|\n'
 for r in report:s+=f"| `{r['line']}` | {r['selected']} | {r['seconds']:.2f} | {r['lufs']:.1f} | {r['truePeakDb']:.1f} |\n"
 s+='\nSelection reason for each row: unclipped, cleanly bounded take with a middle-length preference and measured edge-noise penalty. Change that line’s `take` (1–3) in `Tools/S4/radio_takes.json`, then rerun `Tools/S4/process_radio_takes.py` with the source folder. All alternatives are in gitignored `Recordings/Radio/takes/`. These take files can also be copied individually into `Recordings/Radio/` for the Unity import menu; never import a three-take source as a finished line.\n';doc.write_text(s)
if __name__=='__main__':main(sys.argv[1])
