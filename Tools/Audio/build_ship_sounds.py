"""Original project synthesis. Run from the repo root; no external recordings."""
from pathlib import Path
import math
import random
import struct
import wave

folder = Path('Assets/WatchDemo/Resources/Sprint5Ships')
folder.mkdir(parents=True, exist_ok=True)
rate = 22050
for name, seconds in [('foghorn', 3.8), ('thanks', 3.2), ('wreck', 4.8)]:
    rng = random.Random(1923)
    samples, low = [], 0
    for i in range(int(rate * seconds)):
        t = i / rate
        noise = rng.uniform(-1, 1)
        low = .94 * low + .06 * noise
        if name == 'wreck':
            env = min(1, t / .025) * math.exp(-t * .8)
            value = env * (.9 * low + .09 * noise + .1 * math.sin(t * 2 * math.pi * (53 - 4 * t)))
        else:
            env = min(1, t / .18) * min(1, (seconds - t) / .65)
            if name == 'thanks':
                env *= min(1, max(0, abs(t - 1.55) - .12) * 7)
            hz = 82 if name == 'foghorn' else 103
            value = env * (.37 * math.sin(t * 2 * math.pi * hz) + .15 * math.sin(t * 2 * math.pi * hz * 1.5) + .09 * math.sin(t * 2 * math.pi * hz * 2) + .09 * low)
        samples.append(value)
    scale = .68 / max(abs(x) for x in samples)
    with wave.open(str(folder / (name + '.wav')), 'wb') as stream:
        stream.setparams((1, 2, rate, 0, 'NONE', 'not compressed'))
        stream.writeframes(b''.join(struct.pack('<h', round(x * scale * 32767)) for x in samples))
