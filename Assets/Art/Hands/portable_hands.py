"""CPU-only authoring fallback for sandboxed macOS without a Metal device.
Continuous glove surface from an implicit palm/capsule union, 26 joints and baked poses.
Run: python3 Assets/Art/Hands/portable_hands.py
Blender source is author_hands.py; both produce HandRig.Model JSON in Unity metres.
"""
from pathlib import Path
import json,math,struct,wave,random
OUT=Path(__file__).parent/'Resources/Hands'

def v(p):return dict(zip(('x','y','z'),p))
def sub(a,b):return tuple(a[i]-b[i] for i in range(3))
def dot(a,b):return sum(x*y for x,y in zip(a,b))
def distance(p,a,b):
 d=sub(b,a);t=max(0,min(1,dot(sub(p,a),d)/dot(d,d)));q=tuple(a[i]+d[i]*t for i in range(3));return math.sqrt(dot(sub(p,q),sub(p,q))),t

def quat(e):
 x,y,z=[math.radians(f)/2 for f in e];cx,sx,cy,sy,cz,sz=math.cos(x),math.sin(x),math.cos(y),math.sin(y),math.cos(z),math.sin(z)
 return dict(x=sx*cy*cz-cx*sy*sz,y=cx*sy*cz+sx*cy*sz,z=cx*cy*sz-sx*sy*cz,w=cx*cy*cz+sx*sy*sz)

def make(side,mirror):
 bones=[dict(name='wrist',parent=-1,world=(0,-.018,-.035)),dict(name='palm',parent=0,world=(0,-.018,.015))]
 chains=[('thumb',[(.034,-.018,.005),(.052,-.021,.026),(.067,-.021,.044),(.080,-.021,.059)])]
 for name,x,z,length in [('index',.028,.049,.070),('middle',.008,.053,.079),('ring',-.013,.049,.073),('little',-.032,.039,.057)]:
  chains.append((name,[(x,-.018,.012),(x,-.018,z),(x,-.018,z+length*.42),(x,-.018,z+length*.76),(x,-.018,z+length)]))
 segments=[]
 for name,pts in chains:
  parent=1
  for j,p in enumerate(pts):
   p=(p[0]*mirror,p[1],p[2]);idx=len(bones);bones.append(dict(name=f'{name}_{j}',parent=parent,world=p));parent=idx
   if j<len(pts)-1:
    q=pts[j+1];q=(q[0]*mirror,q[1],q[2]);radius=(.010 if name=='thumb' else .009 if name!='little' else .0075)*(1-j*.09)
    segments.append((idx,p,q,radius))
 def field(p):
  palm=(math.sqrt((p[0]/.044)**2+((p[1]+.018)/.020)**2+((p[2]-.013)/.051)**2)-1)*.020
  cuff=(math.sqrt((p[0]/.036)**2+((p[1]+.018)/.022)**2+((p[2]+.032)/.018)**2)-1)*.018
  return min(palm,cuff,*[distance(p,a,b)[0]-r for _,a,b,r in segments])
 step=.004;origin=(-.1,-.048,-.056);size=(51,17,51)
 points=[];values=[]
 for z in range(size[2]):
  for y in range(size[1]):
   for x in range(size[0]):
    p=tuple(origin[i]+(x,y,z)[i]*step for i in range(3));points.append(p);values.append(field(p))
 def index(x,y,z):return x+size[0]*(y+size[1]*z)
 vertices=[];normals=[];triangles=[];edge_cache={}
 def edge(a,b):
  key=tuple(sorted((a,b)))
  if key in edge_cache:return edge_cache[key]
  t=values[a]/(values[a]-values[b]);p=tuple(points[a][i]+(points[b][i]-points[a][i])*t for i in range(3))
  eps=.0003;n=[]
  for i in range(3):
   hi=list(p);lo=list(p);hi[i]+=eps;lo[i]-=eps;n.append(field(hi)-field(lo))
  length=math.sqrt(dot(n,n));n=tuple(f/length for f in n)
  idx=len(vertices);vertices.append(p);normals.append(n);edge_cache[key]=idx;return idx
 def triangle(a,b,c):
  ab=sub(vertices[b],vertices[a]);ac=sub(vertices[c],vertices[a]);cross=(ab[1]*ac[2]-ab[2]*ac[1],ab[2]*ac[0]-ab[0]*ac[2],ab[0]*ac[1]-ab[1]*ac[0])
  if dot(cross,normals[a])<0:b,c=c,b
  triangles.extend((a,b,c))
 tets=[(0,5,1,6),(0,1,2,6),(0,2,3,6),(0,3,7,6),(0,7,4,6),(0,4,5,6)]
 offsets=[(0,0,0),(1,0,0),(1,1,0),(0,1,0),(0,0,1),(1,0,1),(1,1,1),(0,1,1)]
 for z in range(size[2]-1):
  for y in range(size[1]-1):
   for x in range(size[0]-1):
    cube=[index(x+a,y+b,z+c) for a,b,c in offsets]
    if all(values[i]>=0 for i in cube) or all(values[i]<0 for i in cube):continue
    for tet in tets:
     inside=[cube[j] for j in tet if values[cube[j]]<0];outside=[cube[j] for j in tet if values[cube[j]]>=0]
     if len(inside)==1:triangle(*[edge(inside[0],b) for b in outside])
     elif len(inside)==3:triangle(*[edge(outside[0],b) for b in inside])
     elif len(inside)==2:
      a,b,c,d=[edge(i,o) for i in inside for o in outside];triangle(a,b,c);triangle(b,d,c)
 bone_indices=[];weights=[]
 for p in vertices:
  dist,idx,t=min((distance(p,a,b)[0],idx,distance(p,a,b)[1]) for idx,a,b,r in segments)
  if dist>.018 or (p[2]<.036 and abs(p[0])<.032):i,j,w=1,0,.85
  else:i,j,w=idx,min(idx+1,25),1-max(0,(t-.35)/.65)*.8
  bone_indices.extend((i,j));weights.append(round(w,6))
 poses=[]
 for pose in ['Open','Fist','WrapGrip','Pinch']:
  rotations=[]
  for b in bones:
   e=(0,0,0)
   if '_' in b['name']:
    finger,j=b['name'].split('_');j=int(j)
    if finger=='thumb':
     if pose=='Fist':e=(20,-35*mirror,-25*mirror) if j==0 else (35,0,0)
     if pose=='WrapGrip':e=(10,-24*mirror,-15*mirror) if j==0 else (25,0,0)
     if pose=='Pinch':e=(5,-32*mirror,-12*mirror) if j==0 else (25,0,0)
    elif j in (1,2,3):
     if pose=='Fist':e=([0,75,95,70][j],0,0)
     if pose=='WrapGrip':e=([0,50,65,45][j],0,0)
     if pose=='Pinch':e=(([0,48,68,48][j]) if finger=='index' else 12,0,0)
   rotations.append(quat(e))
  poses.append(dict(name=pose,rotations=rotations))
 for b in reversed(bones):
  p=bones[b['parent']]['world'] if b['parent']>=0 else (0,0,0);b['position']=v(sub(b['world'],p));del b['world']
 data=dict(bones=bones,vertices=[v(p) for p in vertices],normals=[v(n) for n in normals],triangles=triangles,boneIndices=bone_indices,boneWeights=weights,poses=poses)
 (OUT/f'Glove_{side}.json').write_text(json.dumps(data,separators=(',',':'))+'\n')
 print(f'CPU_HAND {side}: joints={len(bones)}, vertices={len(vertices)}, triangles={len(triangles)//3}, poses={len(poses)}',flush=True)

if __name__=='__main__':
 make('L',1);make('R',-1)
 rng=random.Random(27);rate=22050
 for name,duration in [('grab',.13),('release',.10),('impact',.24),('clunk',.20),('gulp',.35),('detent',.10),('failed_crank',1.1),('engine_catch',.7)]:
  samples=[]
  for i in range(int(rate*duration)):
   t=i/rate;decay=math.exp(-t/(duration*.24));noise=rng.uniform(-1,1)
   if name=='gulp':s=math.sin(2*math.pi*(180*t-160*t*t))*.7+noise*.22
   elif name=='failed_crank':s=(math.sin(2*math.pi*65*t)+noise*.7)*(.3+.7*max(0,math.sin(2*math.pi*7*t)))
   elif name=='engine_catch':s=(math.sin(2*math.pi*(65*t+80*t*t))+noise*.5)*(.5+.5*math.sin(2*math.pi*18*t))
   else:s=noise*.35+math.sin(2*math.pi*({'grab':280,'release':420,'impact':160,'clunk':95,'detent':620}[name])*t)*.65
   envelope=decay if name not in ('failed_crank','engine_catch') else min(1,t/.02,(duration-t)/.08)
   samples.append(int(max(-1,min(1,s*envelope*.5))*32767))
  with wave.open(str(OUT/(name+'.wav')),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes(struct.pack('<'+'h'*len(samples),*samples))
