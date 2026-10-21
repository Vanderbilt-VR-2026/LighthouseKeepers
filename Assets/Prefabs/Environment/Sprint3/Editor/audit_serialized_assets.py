#!/usr/bin/env python3
"""Offline serialization checks only. Does not replace Unity import or physics validation."""
from pathlib import Path
import hashlib,json,math,re,struct
ROOT=Path('Assets/Prefabs/Environment/Sprint3');ART=Path('Assets/Art/Sprint3');SCENE=Path('Assets/Scenes/Lab/LK_Lab_Environment.unity')
fail=[];checks=0
unrestored_package_guids=set()
baseline_package_guids=set(re.findall(r'guid: ([a-f0-9]{32})',Path('Assets/Art/Materials/Environment/M_Environment_Timber.mat').read_text()))

def require(ok,message):
 global checks
 checks+=1
 if not ok:fail.append(message)
def blocks(path):
 text=path.read_text(); items=[b for b in re.split(r'(?=--- !u!)',text) if b.startswith('--- !u!')]
 ids=[int(re.match(r'--- !u!\d+ &(\d+)',b)[1]) for b in items]
 require(len(ids)==len(set(ids)),str(path)+': duplicate object IDs')
 d=dict(zip(ids,items))
 for i,b in d.items():
  for target in re.findall(r'\{fileID: (\d+)\}',b):require(int(target)==0 or int(target) in d,f'{path}: {i} references missing local {target}')
 for i,b in d.items():
  if not b.startswith('--- !u!4 ') or 'm_Father:' not in b:continue
  father=int(re.search(r'm_Father: \{fileID: (\d+)',b)[1]);m=re.search(r'm_Children:(.*?)(?=  m_Father:)',b,re.S)
  children=list(map(int,re.findall(r'fileID: (\d+)',m[1])))
  require(len(children)==len(set(children)),f'{path}: duplicate children on {i}')
  if father and father in d and 'm_Children:' in d[father]:require(f'{{fileID: {i}}}' in d[father],f'{path}: parent {father} omits child {i}')
  for c in children:
   if c in d and 'm_Father:' in d[c]:require(f'm_Father: {{fileID: {i}}}' in d[c],f'{path}: child {c} has inconsistent parent')
 return d
scene=blocks(SCENE);prefab=blocks(ROOT/'PF_Sprint3Fixtures.prefab')
# Check the native little-endian vertex/index payloads, not just source dimensions.
mesh_count=0
for p in sorted((ART/'Meshes').glob('*.asset')):
 s=p.read_text();count=int(re.search(r'm_VertexCount: (\d+)',s)[1]);size=int(re.search(r'm_DataSize: (\d+)',s)[1]);data=bytes.fromhex(re.search(r'_typelessdata: ([0-9a-f]+)',s)[1]);index=bytes.fromhex(re.search(r'm_IndexBuffer: ([0-9a-f]+)',s)[1]);indices=struct.unpack('<'+'H'*(len(index)//2),index)
 require(size==len(data)==count*40,f'{p}: vertex stride/count mismatch')
 require(len(indices)%3==0 and max(indices)<count,f'{p}: triangle indices invalid')
 vertices=list(struct.iter_unpack('<10f',data));require(all(math.isfinite(x) for v in vertices for x in v),f'{p}: non-finite vertex')
 for v in vertices:require(abs(sum(x*x for x in v[3:6])-1)<.001,f'{p}: non-unit normal')
 lo=[min(v[i] for v in vertices) for i in range(3)];hi=[max(v[i] for v in vertices) for i in range(3)]
 bounds=re.search(r'm_LocalAABB:\n    m_Center: \{([^}]+)\}\n    m_Extent: \{([^}]+)\}',s)
 c=[float(x) for x in re.findall(r'[xyz]: ([^,}]+)',bounds[1])];e=[float(x) for x in re.findall(r'[xyz]: ([^,}]+)',bounds[2])]
 require(all(abs(c[i]-e[i]-lo[i])<1e-5 and abs(c[i]+e[i]-hi[i])<1e-5 for i in range(3)),f'{p}: bounds omit vertices')
 mesh_count+=1
names={}
for i,b in scene.items():
 if b.startswith('--- !u!1 ') and 'm_Name:' in b:
  name=re.search(r'm_Name: (.*)',b)[1];names.setdefault(name,[]).append(i)
for name in ['STN_PIPE_LEAK','STN_MANUAL_PUMP','STN_ELECTRIC_PUMP','STN_BRIEFING_BOARD','STN_TOOL_TABLE','STN_GENERATOR','STN_GENERATOR_PANEL','STN_BED','PIVOT_PumpHandle','GRIP_PumpHandle','GRIP_PrimerLever','GRIP_ChokeLever','GRIP_StarterHandle','READOUT_FuelGauge']:
 require(len(names.get(name,[]))==1,'Scene requires exactly one '+name)
contract=json.loads(Path('Docs/StationContract.json').read_text())
for station in contract['stations']:
 go=names[station['id']][0];tid=int(re.search(r'component: \{fileID: (\d+)',scene[go])[1]);pose=re.search(r'm_LocalPosition: \{([^}]+)',scene[tid])[1];v=[float(x) for x in re.findall(r'[xyz]: ([^,}]+)',pose)]
 require(all(abs(a-b)<.00001 for a,b in zip(v,station['anchor']['position'])),'Frozen station moved: '+station['id'])
 pose=re.search(r'm_LocalRotation: \{([^}]+)',scene[tid])[1];q=[float(x) for x in re.findall(r'[xyzw]: ([^,}]+)',pose)]
 angle=math.radians(station['anchor']['rotationEuler'][1])/2;expected=[0,math.sin(angle),0,math.cos(angle)]
 require(abs(sum(a*b for a,b in zip(q,expected)))>1-1e-6,'Frozen station rotated: '+station['id'])
 scale=re.search(r'm_LocalScale: \{([^}]+)',scene[tid])[1]
 require(all(abs(float(x)-1)<1e-6 for x in re.findall(r'[xyz]: ([^,}]+)',scale)),'Frozen anchor scaled: '+station['id'])
require(not re.search(r'm_Mesh: \{fileID: \d+, guid: 0000000000000000e000000000000000',SCENE.read_text()),'Scene has direct built-in render meshes')
require('m_Name: PF_Shell_ScaleReference' not in SCENE.read_text(),'Measurement reference prop still in ship scene')
# Own new asset GUIDs must be unique, and every newly referenced GUID must resolve.
all_guids={}
for p in Path('Assets').rglob('*.meta'):
 m=re.search(r'^guid: (\w+)',p.read_text(errors='replace'),re.M)
 if m:all_guids.setdefault(m[1],[]).append(p)
for directory in [ROOT,ART,Path('Assets/Scenes/Sprint3')]:
 for p in directory.rglob('*'):
  if p.is_dir() or p.suffix=='.meta':continue
  require(Path(str(p)+'.meta').exists(),f'{p}: missing meta')
  if p.suffix in {'.cs','.py','.shader','.asmdef','.json'}:continue
  for g in re.findall(r'guid: ([a-f0-9]{32})',p.read_text(errors='replace')):
   if g not in all_guids and g in baseline_package_guids:unrestored_package_guids.add(g)
   else:require(g in all_guids,f'{p}: missing asset GUID {g}')
 for p in directory.rglob('*.meta'):
  g=re.search(r'^guid: (\w+)',p.read_text(),re.M)[1];require(len(all_guids[g])==1,f'{p}: duplicate GUID')
summary={'scope':'OFFLINE ONLY — no Unity import, compilation, physics, light bake or device execution','checks':checks,'nativeMeshAssets':mesh_count,'failures':fail,'unrestoredBaselinePackageGuids':sorted(unrestored_package_guids),'packageNote':'Baseline URP shader/editor-version references require Unity Package Manager restoration; not verified here.'}
out=Path('artifacts/sprint3-environment/serialized-audit.json');out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(summary,indent=2)+'\n')
print(json.dumps(summary,indent=2));raise SystemExit(bool(fail))
