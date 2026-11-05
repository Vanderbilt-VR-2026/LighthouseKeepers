# Run from the repository root. Original deterministic synthesis; no external samples.
from pathlib import Path
import math,random,wave,struct
out=Path('Assets/WatchDemo/Resources/ImmersionAudio');out.mkdir(parents=True,exist_ok=True)
rate=24000
for name,duration in [('wind',8),('thunder',5),('creak',2.5),('foghorn',4),('drip',1.5),('motor',3),('heartbeat',2)]:
 rng=random.Random(121);filtered=0;data=[]
 for i in range(int(duration*rate)):
  t=i/rate;x=t/duration;filtered=filtered*.96+(rng.random()*2-1)*.04
  env=math.sin(math.pi*x)**2
  if name=='wind':v=filtered*(.7+.3*math.sin(2*math.pi*t/duration))
  elif name=='thunder':v=(filtered*.8+math.sin(t*2*math.pi*43)*.05)*min(1,t*4)*math.exp(-t*.8)
  elif name=='creak':v=(math.sin(2*math.pi*(145*t+8*math.sin(t*2)))+.3*math.sin(2*math.pi*302*t))*env*.12
  elif name=='foghorn':v=(math.sin(2*math.pi*98*t)+.35*math.sin(2*math.pi*147*t))*env*.16
  elif name=='drip':v=math.sin(2*math.pi*(900*t-280*t*t))*math.exp(-t*18)*.3
  elif name=='heartbeat':
   beat=t%1;envbeat=math.exp(-beat*25)+.6*math.exp(-max(0,beat-.22)*32)*(beat>=.22);v=math.sin(t*2*math.pi*58)*envbeat*.25
  else:v=(math.sin(t*2*math.pi*142)+.2*math.sin(t*2*math.pi*284))*.10
  data.append(int(max(-1,min(1,v))*32767))
 with wave.open(str(out/(name+'.wav')),'wb') as w:w.setparams((1,2,rate,0,'NONE','not compressed'));w.writeframes(struct.pack('<'+'h'*len(data),*data))
