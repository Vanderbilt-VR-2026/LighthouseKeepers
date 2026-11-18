#!/usr/bin/env python3
"""User-provided loop + exact 01:11-01:16 accent. Work on copies; numpy + ffmpeg."""
from pathlib import Path
import argparse,subprocess,wave,json,hashlib,os
import numpy as np
ROOT=Path(__file__).resolve().parents[2];R=44100
p=argparse.ArgumentParser();p.add_argument('--source',type=Path,default=ROOT/'Docs/Audio/Sources/LighthouseKeepersSoundtrack.mp3');p.add_argument('--accent',type=Path,default=ROOT/'Docs/Audio/Sources/DemoHorrorSample.mp3');a=p.parse_args()
work=ROOT/'artifacts/s5/soundtrack';work.mkdir(parents=True,exist_ok=True);out=ROOT/'Assets/Audio/Resources/PlayerScore';out.mkdir(parents=True,exist_ok=True)
def ff(args):return subprocess.run([os.environ.get('FFMPEG',str(Path.home()/'.local/bin/ffmpeg')),'-hide_banner','-nostdin','-y',*map(str,args)],capture_output=True,text=True,check=True)
def decode(src,name):
 dest=work/(name+'-decoded.wav');ff(['-i',src,'-ar',R,'-ac',2,dest])
 with wave.open(str(dest)) as w:return np.frombuffer(w.readframes(w.getnframes()),'<i2').reshape(-1,2).astype(np.float32)/32768
base=decode(a.source,'main');accent=decode(a.accent,'accent')[71*R:76*R].copy();assert len(accent)==5*R
# Preserve the composition and its natural rests. Remove only room/silent padding.
energy=np.max(np.abs(base),axis=1);nonzero=np.flatnonzero(energy>.0005);start=max(0,int(nonzero[0])-.08*R);end=min(len(base),int(nonzero[-1])+.08*R);base=base[int(start):int(end)]
n=4*R;theta=np.linspace(0,np.pi/2,n,dtype=np.float32)[:,None]
loop=np.concatenate([base[n:-n],base[-n:]*np.cos(theta)+base[:n]*np.sin(theta)])
accent[:int(.01*R)]*=np.linspace(0,1,int(.01*R))[:,None];accent[-int(.35*R):]*=np.linspace(1,0,int(.35*R))[:,None]
report={'main_source_sha256':hashlib.sha256(a.source.read_bytes()).hexdigest(),'accent_source_sha256':hashlib.sha256(a.accent.read_bytes()).hexdigest(),'clips':[]}
for name,data,target in [('main-loop',loop,-20),('intense-hit',accent,-18)]:
 raw=work/(name+'.wav')
 # Temporary float WAV avoids clipping the overlap before normalization.
 floatfile=work/(name+'.f32');data.astype('<f4').tofile(floatfile);ff(['-f','f32le','-ar',R,'-ac',2,'-i',floatfile,'-c:a','pcm_f32le',raw])
 norm=f'loudnorm=I={target}:TP=-2:LRA=14';log=ff(['-i',raw,'-af',norm+':print_format=json','-f','null','-']).stderr;v=json.JSONDecoder().raw_decode(log[log.rfind('{'):])[0]
 chain=norm+':linear=true:'+':'.join(k+'='+v[z] for k,z in [('measured_I','input_i'),('measured_TP','input_tp'),('measured_LRA','input_lra'),('measured_thresh','input_thresh'),('offset','target_offset')])
 dest=out/(name+'.ogg');ff(['-i',raw,'-af',chain,'-ar',R,'-c:a','libvorbis','-q:a',5,dest]);log=ff(['-i',dest,'-af','loudnorm=print_format=json','-f','null','-']).stderr;v=json.JSONDecoder().raw_decode(log[log.rfind('{'):])[0]
 measured=dict(name=name,duration=len(data)/R,lufs=float(v['input_i']),true_peak=float(v['input_tp']),sha256=hashlib.sha256(dest.read_bytes()).hexdigest(),source_start=start/R if name=='main-loop' else 71,source_end=end/R if name=='main-loop' else 76,crossfade=4 if name=='main-loop' else 0)
 if name=='main-loop':measured['unencoded_seam_delta']=float(np.max(abs(data[-1]-data[0])))
 assert measured['true_peak']<=-1.2
 report['clips'].append(measured)
(work/'analysis.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
