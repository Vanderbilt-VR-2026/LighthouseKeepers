#!/usr/bin/env python3
"""Original paper/ink atlas. Text comes from the existing player-string table.
No photography/third-party texture; deterministic grain only. Four 1K pages, one2K atlas.
"""
from pathlib import Path
import json,random,textwrap
from PIL import Image,ImageDraw,ImageFont
R=Path(__file__).resolve().parents[2]
D={e['key']:e['text'] for e in json.loads((R/'Assets/Data/Strings/Resources/KeeperStrings.json').read_text())['entries']}
font=Path('/System/Library/Fonts/Supplemental/Georgia.ttf')
if not font.exists():font=Path('/Library/Fonts/Arial.ttf')
body=ImageFont.truetype(str(font),40);title=ImageFont.truetype(str(font),46)
atlas=Image.new('RGB',(2048,2048));rng=random.Random(1874)
for i,key in enumerate(['orders','letter','engineLog','tomLetter']):
 page=Image.new('RGB',(1024,1024));px=page.load()
 for y in range(1024):
  for x in range(1024):
   grain=rng.randint(-4,4);edge=min(x,y,1023-x,1023-y);patina=int(max(0,1-edge/90)*15)
   px[x,y]=(213+grain-patina,200+grain-patina,170+grain-patina)
 draw=ImageDraw.Draw(page);ink=(43,36,29)
 # The 2D page is stretched onto an A5-ish physical sheet; generous margins and crisp letters.
 titleText=D['story.'+key+'.title'];draw.text((80,78),titleText,font=title,fill=ink)
 draw.line((80,145,944,145),fill=(104,84,59),width=2)
 y=185
 for para in D['story.'+key+'.body'].split('\n'):
  words=para.split();line=''
  for word in words:
   candidate=(line+' '+word).strip()
   if draw.textlength(candidate,font=body)>864:
    draw.text((80,y),line,font=body,fill=ink);y+=53;line=word
   else:line=candidate
  if line:draw.text((80,y),line,font=body,fill=ink);y+=53
  else:y+=28
 if y>960:raise ValueError(f'Page overflow {key}: {y}')
 atlas.paste(page,((i%2)*1024,(i//2)*1024))
out=R/'Assets/Art/Story/Resources/KettleStory';out.mkdir(parents=True,exist_ok=True);atlas.save(out/'papers.png')
print('Baked four readable pages into one2048 atlas.')
