from pathlib import Path
import wave,math,random,struct,uuid
root=Path('Assets/WatchDemo/Resources/S5Audio');root.mkdir(exist_ok=True)
if not Path(str(root)+'.meta').exists():Path(str(root)+'.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n')
base=Path('Assets/WatchDemo/Resources/Sprint5Ships/foghorn.wav.meta').read_text()
for j,name in enumerate(['tear','fuel','wire','smear']):
 rate=44100;duration=3;random.seed(700+j);out=[];low=0
 for i in range(rate*duration):
  t=i/rate;noise=random.uniform(-1,1);low=.965*low+.035*noise
  envelope=min(1,t/.06)*min(1,(duration-t)/.15)
  if name=='tear':
   steps=sum(math.exp(-max(0,t-a)*18) if t>=a else 0 for a in [.1,.8,1.5]);sound=.22*steps*(math.sin(2*math.pi*57*t)+low*3)+.15*math.sin(2*math.pi*(105*t-5*t*t))*max(0,t-1.3)/1.7
  elif name=='fuel':sound=.28*math.sin(2*math.pi*(120*t+7*math.sin(t*9)))*(.4+.6*math.sin(t*21)**2)+low*.4
  elif name=='wire':sound=.18*math.sin(2*math.pi*(60*t+2*t*t))*math.exp(-t*.5)+noise*.12*math.exp(-abs(t-1.7)*22)+low*.35
  else:sound=low*(1+.4*math.sin(t*13))+.06*math.sin(2*math.pi*450*t)*math.sin(t*4)**2
  out.append(sound*envelope)
 peak=max(abs(x) for x in out);gain=.50/max(peak,.01)
 p=root/('sabotage_'+name+'.wav')
 with wave.open(str(p),'wb') as w:w.setparams((1,2,rate,0,'NONE','not compressed'));w.writeframes(b''.join(struct.pack('<h',int(x*gain*32767)) for x in out))
 if not Path(str(p)+'.meta').exists():
  meta=base.splitlines();meta[1]='guid: '+uuid.uuid4().hex;Path(str(p)+'.meta').write_text('\n'.join(meta).replace('preloadAudioData: 0','preloadAudioData: 1')+'\n')
 print(p,'3s mono44.1k, peak -6.02dBFS; original deterministic synthesis')
