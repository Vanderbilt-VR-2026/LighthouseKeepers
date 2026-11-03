#!/usr/bin/env python3
"""Original S4 procedural effects. No recordings, samples, or third-party audio."""
import math, random, wave, struct
from pathlib import Path
OUT=Path(__file__).resolve().parents[2]/'Assets/WatchDemo/Resources/S4Audio'
RATE=22050

def render(name,seconds,kind,seed=1):
    rng=random.Random(seed); low=0; values=[]
    for i in range(int(seconds*RATE)):
        t=i/RATE; noise=rng.uniform(-1,1);low+=.035*(noise-low)
        if kind=='wood': v=(.65*noise*math.exp(-t*60)+.6*math.sin(2*math.pi*(100+seed*9)*t)*math.exp(-t*22))
        elif kind=='metal':v=noise*.25*math.exp(-t*60)+sum(math.sin(2*math.pi*f*t)*math.exp(-t*(8+k*4))*.14 for k,f in enumerate([470,783,1243,2210]))
        elif kind=='clunk':v=.7*math.sin(2*math.pi*95*t)*math.exp(-t*12)+noise*.3*math.exp(-t*25)
        elif kind=='cough':v=(math.sin(2*math.pi*63*t)+noise*.4)*(.5+.5*math.sin(2*math.pi*12*t))*math.exp(-t*5)*.5
        elif kind=='water':v=low*1.8+math.sin(2*math.pi*83*t)*(.5+.5*math.sin(2*math.pi*3*t))*.025
        elif kind=='hiss':v=noise*.20+low*.8
        elif kind=='night':v=low*2.5*math.sin(math.pi*t/seconds)**2+math.sin(2*math.pi*(47*t-3*t*t))*math.exp(-t)*.25
        elif kind=='bell':v=sum(math.sin(2*math.pi*f*t)*math.exp(-t*(2+k))*.2 for k,f in enumerate([440,880,1193,1760]))
        elif kind=='fail':v=math.sin(2*math.pi*(220*t-45*t*t))*.35*math.sin(math.pi*t/seconds)**2
        elif kind=='success':v=sum(math.sin(2*math.pi*f*t)*.16 for f in [440,550,660])*math.sin(math.pi*t/seconds)**2
        elif kind=='breaker':v=noise*.7*math.exp(-t*45)+math.sin(2*math.pi*160*t)*.3*math.exp(-t*15)
        else:raise ValueError(kind)
        # Bounded peaks and gentle endpoints, including loops.
        fade=min(1,t/.008,(seconds-t)/.02)
        values.append(max(-.85,min(.85,v))*max(0,fade))
    with wave.open(str(OUT/(name+'.wav')),'wb') as f:
        f.setparams((1,2,RATE,0,'NONE','not compressed'))
        f.writeframes(struct.pack('<'+'h'*len(values),*[int(x*32767) for x in values]))

if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True)
    for i in range(1,5):render('wood'+str(i),.3,'wood',i)
    for name,seconds,kind in [('metal',.5,'metal'),('bolt-clunk',.45,'clunk'),('cough',.6,'cough'),('water',4,'water'),('hiss',3,'hiss'),('night',3,'night'),('bell',1.2,'bell'),('fail',1.2,'fail'),('success',1.2,'success'),('breaker',.3,'breaker')]:render(name,seconds,kind)
