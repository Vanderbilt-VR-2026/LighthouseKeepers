#!/usr/bin/env python3
"""Apply authored fixture YAML to the owned Lab while preserving station/interaction IDs.
Run once on sprint3/base after generate_assets.py. Repeated runs fail closed.
Unity import/validation is still required; this is not an Editor validation substitute.
"""
from pathlib import Path
import math,re,json
SCENE=Path('Assets/Scenes/Lab/LK_Lab_Environment.unity')
ROOT=Path('Assets/Prefabs/Environment/Sprint3')
HEADER='%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'
def split(text):
    return {int(re.match(r'--- !u!\d+ &(\d+)',b)[1]):b for b in re.split(r'(?=--- !u!)',text) if b.startswith('--- !u!')}
def kind(block):return int(re.match(r'--- !u!(\d+)',block)[1])
def names(blocks):return {re.search(r'  m_Name: (.*)',b)[1]:fid for fid,b in blocks.items() if kind(b)==1 and re.search(r'  m_Name: (.*)',b)}
def components(blocks,go):return list(map(int,re.findall(r'component: \{fileID: (\d+)\}',blocks[go])))
def trans(blocks,go):return next(i for i in components(blocks,go) if kind(blocks[i]) in (4,224))
def children(block):
    m=re.search(r'  m_Children:(.*?)(?=  m_Father:)',block,re.S)
    return list(map(int,re.findall(r'fileID: (\d+)',m[1]))) if m else []
def setchildren(block,ids):
    value='\n'+''.join(f'  - {{fileID: {i}}}\n' for i in ids) if ids else ' []\n'
    return re.sub(r'  m_Children:.*?(?=  m_Father:)',lambda m:'  m_Children:'+value,block,flags=re.S)
def setpos(block,pos):return re.sub(r'  m_LocalPosition: \{[^}]+\}',lambda m:'  m_LocalPosition: {'+', '.join(f'{k}: {v:.9g}' for k,v in zip('xyz',pos))+'}',block)
def setparent(block,parent):return re.sub(r'  m_Father: \{fileID: \d+\}',f'  m_Father: {{fileID: {parent}}}',block)
def guid(path):return re.search(r'^guid: (\w+)',Path(str(path)+'.meta').read_text(),re.M)[1]
s=SCENE.read_text()
if 'm_Name: Sprint 3 environment fixtures\n' in s:raise SystemExit('Already applied. Use git review; do not duplicate station hierarchies.')
old=split(s);new=split((ROOT/'PF_Sprint3Fixtures.prefab').read_text());on=names(old);nn=names(new)
# Move station children under the existing frozen anchor IDs. Preserve all legacy sockets.
remap={};drop=set()
for name in ['STN_BRIEFING_BOARD','STN_PIPE_MANIFOLD','STN_GENERATOR','STN_GENERATOR_PANEL']:
    a,b=nn[name],on[name];at,bt=trans(new,a),trans(old,b);remap[a]=b;remap[at]=bt;drop.update((a,at))
    old[bt]=setchildren(old[bt],children(old[bt])+children(new[at]))
    added=[i for i in components(new,a) if i!=at]
    for i in added:
        old[b]=old[b].replace('  m_Layer: 0\n',f'  - component: {{fileID: {i}}}\n  m_Layer: 0\n',1)
# The plinth datum already exists. Keep its exact original transform and name.
plinth=nn['PLINTH_TOP'];pt=trans(new,plinth)
existing=next((tid for tid in children(old[trans(old,on['STN_GENERATOR'])]) if tid in old and 'PLINTH_TOP' in old[int(re.search(r'm_GameObject: \{fileID: (\d+)',old[tid])[1])]),None)
if existing:
    remap[pt]=existing;drop.update((plinth,pt))
# Existing wrench retains its XRGrabInteractable, haptics, rigidbody and controller refs.
# Replace the art; supply generous trigger bounds independently of physical resting bounds.
visual=on['Wrench visual'];vt=trans(old,visual)
wv=nn['Wrench mesh and grab bounds'];wvt=trans(new,wv)
remap[wv]=visual;remap[wvt]=vt;drop.update((wv,wvt))
for fid in components(new,wv):
    if kind(new[fid])==65:drop.add(fid);continue
    if fid!=wvt:old[visual]=old[visual].replace('  m_Layer: 0\n',f'  - component: {{fileID: {fid}}}\n  m_Layer: 0\n',1)
old[vt]=re.sub(r'm_LocalScale: \{[^}]+\}','m_LocalScale: {x: 1, y: 1, z: 1}',old[vt])
# Retire only replaced scenery. Never touch the A/E prefab assets.
retire=['Wall-mounted checklist brown frame','Wall-mounted checklist white face','Wrench table top','Wrench table leg','Steel handle','Open jaw left','Open jaw right','Jaw bridge','Third-floor bed frame','Mattress and blanket','Pillow','Single service pipe','Pipe support','Support foot','Leaking centre coupling']
old_drop=set()
for go,b in old.items():
    if kind(b)!=1 or not re.search(r'  m_Name: (.*)',b):continue
    name=re.search(r'  m_Name: (.*)',b)[1]
    if name in retire:old_drop.add(go);old_drop.update(components(old,go))
# Legacy board controller keeps a valid TextMesh, but only the new TMP fixture is rendered.
board=on['Watch checklist']
for i in components(old,board):
    if kind(old[i])==23:old[i]=old[i].replace('m_Enabled: 1','m_Enabled: 0')
# Maintain legacy controller's bed reference, now a unit-scale station rather than a mesh pivot.
old_bed=trans(old,on['Third-floor bed frame']);new_bed=trans(new,nn['STN_BED'])
for i,b in list(old.items()):
    if '  bed: {fileID:' in b:old[i]=b.replace(f'  bed: {{fileID: {old_bed}}}',f'  bed: {{fileID: {new_bed}}}')
# Place the original wrench on the near edge of the new bench.
for name,go in on.items():
    if name.startswith('Grab wrench'):
        t=trans(old,go);old[t]=setpos(old[t],(1.62,.925,-5.99))
        for c in components(old,go):
            if kind(old[c])==65:old[c]=re.sub(r'm_Size: \{[^}]+\}','m_Size: {x: 0.12, y: 0.035, z: 0.35}',old[c])
        # Copy authored generous trigger collider onto the scene tool root.
        tc=[i for i in components(new,wv) if kind(new[i])==65][-1]
        old[tc]=re.sub(r'm_GameObject: \{fileID: \d+\}',f'm_GameObject: {{fileID: {go}}}',new[tc])
        old[go]=old[go].replace('  m_Layer: 0\n',f'  - component: {{fileID: {tc}}}\n  m_Layer: 0\n',1)
# Put feedback/particle origins on the frozen repair contact. New art supplies the pipe.
a=math.radians(225);origin=(2.439518,0,2.439518)
def pipeworld(x,y,z):return (origin[0]+math.cos(a)*x+math.sin(a)*z,y,origin[2]-math.sin(a)*x+math.cos(a)*z)
for name,pos in [('Repair point',pipeworld(.8,.95,.45)),('Visible centre leak',pipeworld(.8,.95,.45)),('Repair feedback',pipeworld(.8,1.55,.49)),('Sleep prompt',(0,8.15,.1))]:
    if name in on: t=trans(old,on[name]);old[t]=setpos(old[t],pos)
# Remove displaced scale-reference props; these are lab measuring tools, not shipping art.
# Prefab instance structure remains intact; disable renderers/colliders through the Editor
# prepare pass, rather than severing GUID/fileID references in the serialized instance.
for name,go in on.items():
    if name.startswith('Stairwell liner '):
        t=trans(old,go);b=old[t];m=re.search(r'm_LocalPosition: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)',b)
        x,y,z=map(float,m.groups());floor=y-1.63
        b=setpos(b,(x,floor+1.47,z))
        old[t]=re.sub(r'(m_LocalScale: \{x: [^,]+, y:) [^,]+',r'\g<1> 2.94',b)
# Remove direct dangling hierarchy entries for retired geometry.
for i,b in list(old.items()):
    if kind(b) in (4,224):old[i]=setchildren(b,[c for c in children(b) if c not in old_drop])
for i in old_drop:old.pop(i,None)
scale_guid=guid('Assets/Prefabs/Environment/PF_Shell_ScaleReference.prefab')
scale_instances={i for i,b in old.items() if kind(b)==1001 and 'm_SourcePrefab:' in b and scale_guid in b}
scale_drop=set(scale_instances)
for i,b in old.items():
    m=re.search(r'm_PrefabInstance: \{fileID: (\d+)\}',b)
    if m and int(m[1]) in scale_instances:scale_drop.add(i)
for i in scale_drop:old.pop(i,None)
for i,b in list(old.items()):
    for removed in scale_drop:b=re.sub(r'^  - \{fileID: '+str(removed)+r'\}\n','',b,flags=re.M)
    old[i]=b
# Remap all prefab-local references that now point to preserved scene anchors.
for i,b in list(new.items()):
    if i in drop:continue
    b=re.sub(r'\{fileID: (\d+)\}',lambda m:'{fileID: '+str(remap.get(int(m[1]),int(m[1])))+'}',b)
    if kind(b) in (4,224):b=setchildren(b,[c for c in children(b) if c not in remap.values()])
    new[i]=b
for i in drop:new.pop(i,None)
old.update(new)
# Every remaining legacy construction renderer receives an authored mesh, including inactive
# overlay geometry. No mesh is copied from Unity's default resources.
legacy=json.loads((ROOT/'LegacyMeshMap.json').read_text())
for i,b in list(old.items()):
    if kind(b)==33:
        def replace(m):
            path=legacy.get(m[1])
            if not path:raise ValueError('Unrecognised built-in mesh '+m[1])
            return f'm_Mesh: {{fileID: 4300000, guid: {guid(path)}, type: 2}}'
        old[i]=re.sub(r'm_Mesh: \{fileID: (\d+), guid: 0000000000000000e000000000000000, type: 0\}',replace,b)
# Add the new dressing root to SceneRoots (Unity 6).
root_go=nn['Sprint 3 environment fixtures'];root_t=trans(new,root_go)
for i,b in list(old.items()):
    if kind(b)==1660057539:old[i]=b.rstrip()+f'\n  - {{fileID: {root_t}}}\n'
# References appended to existing station transforms also need the datum remap.
for i,b in list(old.items()):
    b=re.sub(r'\{fileID: (\d+)\}',lambda m:'{fileID: '+str(remap.get(int(m[1]),int(m[1])))+'}',b)
    if kind(b) in (4,224):b=setchildren(b,list(dict.fromkeys(children(b))))
    old[i]=b
SCENE.write_text((HEADER+''.join(old.values())).replace('type: 3}\n  m_Name: \n  m_EditorClassIdentifier: Unity.RenderPipelines.Universal.Runtime', 'type: 3}\n  m_Name:\n  m_EditorClassIdentifier: Unity.RenderPipelines.Universal.Runtime'))
print('Applied fixtures to owned scene; Unity import, new lighting bakes and physical acceptance pending.')
