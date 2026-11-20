"""Original, deterministic Drowned palette. No recordings or third-party samples.

Run with numpy 2.0.2 / scipy 1.13.1 and ffmpeg available. Raw work/analysis stays
in ignored artifacts/s5/drowned-horror/audio; only mastered clips enter Resources.
"""
from pathlib import Path
import json
import subprocess
import uuid
import numpy as np
from scipy import signal
from scipy.io import wavfile

RATE = 44100
WORK = Path('artifacts/s5/drowned-horror/audio')
DEST = Path('Assets/WatchDemo/Resources/S5Audio/DrownedHorror')
WORK.mkdir(parents=True, exist_ok=True)
DEST.mkdir(parents=True, exist_ok=True)
rng = np.random.default_rng(1907)


def tone(t, hz, phase=0):
    return np.sin(2 * np.pi * hz * t + phase)


def noise(t, low, high):
    # Periodic spectrum: the looping bed has no random-noise seam.
    f = np.fft.rfftfreq(len(t), 1 / RATE)
    phase = rng.uniform(0, 2 * np.pi, len(f))
    weight = np.exp(-np.maximum(0, low-f)/max(1, low*.12))
    weight *= np.exp(-np.maximum(0, f-high)/max(1, high*.12))
    weight[0] = 0
    x = np.fft.irfft(weight * np.exp(1j*phase), n=len(t))
    return x / max(1e-8, np.std(x))


def envelope(t, start, length, attack=.06):
    a = np.clip((t-start)/attack, 0, 1)
    b = np.clip((start+length-t)/max(.1, length*.5), 0, 1)
    return a*a*(3-2*a) * b*b*(3-2*b)


def wet_breath(t, start, length, pitch=79):
    e = envelope(t, start, length, .35)
    chest = tone(t, pitch + 1.7*tone(t, .41))  # intentionally rough, wordless
    flutter = .68 + .22*tone(t, 5.1) + .1*tone(t, 9.3)
    return e * (noise(t, 340, 1700)*.08 + chest*.11*flutter + noise(t, 65, 240)*.045)


def boot(t, at):
    x = np.maximum(0, t-at)
    heel = tone(t, 68-8*np.minimum(x,.3))*np.exp(-x*19)
    squelch = noise(t, 120, 1800)*np.exp(-x*8)*(.45+.3*tone(t,29))
    return (t>=at)*np.minimum(x/.018,1)*(heel*.22+squelch*.12)


def edge(x, seconds=.04):
    n = min(int(RATE*seconds), len(x)//2)
    ramp = np.linspace(0,1,n)**2
    x[:n] *= ramp[:,None] if x.ndim==2 else ramp
    x[-n:] *= ramp[::-1,None] if x.ndim==2 else ramp[::-1]
    return x


def probe(path):
    p = subprocess.run(['ffmpeg','-hide_banner','-i',str(path),'-af',
                        'loudnorm=I=-23:TP=-4.5:LRA=14:print_format=json','-f','null','-'],
                       capture_output=True,text=True,check=True)
    return json.loads(p.stderr[p.stderr.rfind('{'):p.stderr.rfind('}')+1])


report = []
clips = {}
for name, duration, target, loop in [
    ('approach',3.1,-22,False), ('reveal_a',3.6,-20,False),
    ('reveal_b',3.9,-20,False), ('breathing',12,-24,True),
    ('retreat',2.8,-23,False), ('score',24,-22,True),
]:
    t = np.arange(round(duration*RATE))/RATE
    if name=='approach':
        x=boot(t,.35)+boot(t,1.17)*.72+wet_breath(t,1.35,1.7)
        x+=noise(t,180,1100)*envelope(t,.1,2.3,.4)*.028
    elif name.startswith('reveal'):
        # Bow scrape + descending weight, not a high scream or bell transient.
        detune=1 if name.endswith('a') else .93
        phase=2*np.pi*(96*detune*t-9*t*t)
        bowed=sum(np.sin(phase*k+.18*tone(t,.61))*np.exp(-k*.55) for k in range(1,8))
        x=envelope(t,.03,duration-.1,.11)*(bowed*.24+noise(t,95,850)*.065)
        x+=wet_breath(t,.65,duration-.75,67)*.65
        x+=boot(t,.04)*.5
    elif name=='breathing':
        x=wet_breath(t,.2,4.1,73)+wet_breath(t,5.2,5.7,67)*.8
        for at in [1.4,3.9,6.3,8.8]:
            d=t-at
            x+=np.exp(-(d/.11)**2)*tone(t,144+.7*tone(t,2.8))*.025
        x+=noise(t,90,450)*.006*(1-np.cos(2*np.pi*t/duration))
        x=edge(x,.09)
    elif name=='retreat':
        x=wet_breath(t,0,2.4,61)[::-1]*.75
        x+=noise(t,160,1350)*envelope(t,.2,2.5,.25)*np.exp(-t*.8)*.12
        x+=tone(t,48)*envelope(t,0,2.8,.1)*.10
    else:
        # Periodic 24-second bowed chord: bass, minor seconds and slow beating.
        # All carriers/modulators land on the loop grid, no rhythmic tinkling.
        l=np.zeros_like(t);r=np.zeros_like(t)
        for hz,gain,offset in [(41.6667,.10,0),(55,.12,.4),(82.5,.075,.8),(110,.045,1.2),(116.6667,.034,1.6),(165,.021,2)]:
            hz=round(hz*duration)/duration
            mod=.16*np.sin(2*np.pi*t/duration*3+offset)
            amp=.72+.28*np.cos(2*np.pi*t/duration+offset)
            l+=gain*amp*tone(t,hz,mod)
            r+=gain*amp*tone(t,hz,mod+offset*.4)
        scrape=noise(t,130,960)*.018*(.5-.5*np.cos(2*np.pi*t/duration*2))
        l+=scrape;r+=np.roll(scrape,2200)
        x=np.column_stack([l,r])
    # No infrasonic energy. Wrap padding prevents filter transients in loops.
    sos=signal.butter(2,38,fs=RATE,btype='highpass',output='sos')
    if loop:
        x=signal.sosfilt(sos,np.concatenate([x,x,x]),axis=0)[len(t):2*len(t)]
    else:x=edge(signal.sosfilt(sos,x,axis=0))
    x=np.tanh(x*1.2)/1.2
    raw=WORK/(name+'-raw.wav');wavfile.write(raw,RATE,x.astype(np.float32))
    m=probe(raw)
    chain=(f"loudnorm=I={target}:TP=-4.5:LRA=14:measured_I={m['input_i']}:"
           f"measured_TP={m['input_tp']}:measured_LRA={m['input_lra']}:"
           f"measured_thresh={m['input_thresh']}:linear=true")
    path=DEST/(name+'.wav')
    subprocess.run(['ffmpeg','-v','error','-y','-i',str(raw),'-af',chain,'-ar',str(RATE),
                    '-c:a','pcm_s16le',str(path)],check=True)
    rate,data=wavfile.read(path);f=data.astype(np.float64)/32768
    measured=probe(path)
    assert float(measured['input_tp'])<=-4.35, (name, measured)
    assert np.isfinite(f).all()
    seam=float(np.max(np.abs(f[0]-f[-1])))
    if loop:assert seam<.012,(name,seam)
    report.append({'file':str(path),'seconds':len(f)/rate,'channels':1 if f.ndim==1 else f.shape[1],
                   'lufs':float(measured['input_i']),'truePeakDb':float(measured['input_tp']),
                   'loop':loop,'loopBoundaryDelta':seam,'source':'Original deterministic synthesis; seed1907'})
    clips[name]=f
    meta=Path(str(path)+'.meta')
    if not meta.exists():
        template=Path('Assets/WatchDemo/Resources/S5Audio/drowned_breath.wav.meta').read_text()
        template=template.replace('guid: 5ede65e9dca24d828a1af4c3e6701bd8','guid: '+uuid.uuid4().hex)
        if name=='score':template=template.replace('loadType: 1','loadType: 2').replace('forceToMono: 1','forceToMono: 0')
        meta.write_text(template)

# Offline preview, not speaker playback. Approximate source/bus gains, no gameplay audio changed.
mix=np.zeros((round(20*RATE),2))
def add(name,at,gain):
    x=clips[name];x=x[:,None] if x.ndim==1 else x
    start=round(at*RATE);length=min(len(x),len(mix)-start)
    mix[start:start+length]+=x[:length]*gain
add('approach',0,.5*10**(-3/20));add('reveal_a',3.1,.5*10**(-3/20))
add('breathing',3.1,.36*10**(-3/20));add('score',3.1,.20*10**(-2/20));add('retreat',15.1,.45*10**(-3/20))
mix=edge(mix,.8);wavfile.write(WORK/'encounter-preview.wav',RATE,(np.clip(mix,-1,1)*32767).astype(np.int16))
(WORK/'analysis.json').write_text(json.dumps({'clips':report,'preview':probe(WORK/'encounter-preview.wav'),
    'note':'Offline source analysis only; full bus mix and human listening remain device checks.'},indent=2)+'\n')
print(json.dumps(report,indent=2))
