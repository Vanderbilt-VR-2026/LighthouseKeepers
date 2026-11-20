#!/usr/bin/env python3
"""Rebuild the user-supplied demo score. Requires numpy and ffmpeg; never edits source."""
from pathlib import Path
import argparse, subprocess, wave, json, hashlib, os
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser();p.add_argument('--source',type=Path,default=ROOT/'Docs/Audio/Sources/DemoHorrorSample.mp3');p.add_argument('--output',type=Path,default=ROOT/'Assets/Audio/Resources/DemoHorror');a=p.parse_args()
FF=os.environ.get('FFMPEG',str(Path.home()/'.local/bin/ffmpeg'));R=44100
work=ROOT/'artifacts/s5/finish/music';work.mkdir(parents=True,exist_ok=True);a.output.mkdir(parents=True,exist_ok=True)
def ff(args):return subprocess.run([FF,'-hide_banner','-nostdin','-y',*map(str,args)],capture_output=True,text=True,check=True)
ff(['-i',a.source,'-ar',R,'-ac',2,work/'decoded.wav'])
with wave.open(str(work/'decoded.wav')) as w:x=np.frombuffer(w.readframes(w.getnframes()),'<i2').reshape(-1,2).astype(float)/32768
# Approved reference intervals; no speed/pitch change. Crossfaded ends form a circular buffer.
specs=[('calm',.5,14.5,1.5,-25),('tension',40,70,1.5,-24),('crisis',82,114,1.5,-23),('scare',70.9,74.4,0,-18)]
report={'source_sha256':hashlib.sha256(a.source.read_bytes()).hexdigest(),'credit':'User-supplied temporary demo sample; not claimed as team-composed or CC0.','clips':[]}
for name,start,end,overlap,lufs in specs:
 y=x[round(start*R):round(end*R)].copy();n=round(overlap*R)
 if n:
  fade=(.5-.5*np.cos(np.linspace(0,np.pi,n)))[:,None]
  y=np.concatenate([y[n:-n],y[-n:]*(1-fade)+y[:n]*fade])
 else:
  ni=round(.01*R);no=round(.7*R);y[:ni]*=np.linspace(0,1,ni)[:,None];y[-no:]*=np.linspace(1,0,no)[:,None]
 raw=work/(name+'.wav')
 with wave.open(str(raw),'wb') as w:w.setnchannels(2);w.setsampwidth(2);w.setframerate(R);w.writeframes((np.clip(y,-.999,.999)*32767).astype('<i2').tobytes())
 filt=f'loudnorm=I={lufs}:TP=-2:LRA=12'
 log=ff(['-i',raw,'-af',filt+':print_format=json','-f','null','-']).stderr
 stats=json.JSONDecoder().raw_decode(log[log.rfind('{'):])[0];chain=filt+':linear=true:'+':'.join(k+'='+stats[v] for k,v in [('measured_I','input_i'),('measured_TP','input_tp'),('measured_LRA','input_lra'),('measured_thresh','input_thresh'),('offset','target_offset')])
 dest=a.output/(name+'.ogg');ff(['-i',raw,'-af',chain,'-ar',R,'-c:a','libvorbis','-q:a',5,dest])
 log=ff(['-i',dest,'-af','loudnorm=print_format=json','-f','null','-']).stderr;measured=json.JSONDecoder().raw_decode(log[log.rfind('{'):])[0]
 report['clips'].append(dict(name=name,start=start,end=end,crossfade=overlap,duration=len(y)/R,lufs=float(measured['input_i']),true_peak=float(measured['input_tp']),seam_delta=float(np.max(abs(y[-1]-y[0]))) if n else None))
(a.output/'mix-analysis.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
