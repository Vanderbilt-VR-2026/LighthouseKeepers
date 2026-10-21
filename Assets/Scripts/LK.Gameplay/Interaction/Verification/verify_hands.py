"""Run narrower offline checks when Unity's UPM socket is unavailable.
python3 Assets/Scripts/LK.Gameplay/Interaction/Verification/verify_hands.py \
  --unity /Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents \
  --assemblies Library/ScriptAssemblies
This is NOT Unity import, Play Mode, APK compilation, or headset evidence.
"""
from pathlib import Path
import argparse,subprocess,json,re,collections,math
parser=argparse.ArgumentParser();parser.add_argument('--unity',type=Path,default=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents'));parser.add_argument('--assemblies',type=Path,default=Path('Library/ScriptAssemblies'));a=parser.parse_args()
root=Path('Assets/Scripts/LK.Gameplay/Interaction');out=Path('artifacts');out.mkdir(exist_ok=True)
dotnet=a.unity/'Resources/Scripting/NetCoreRuntime/dotnet';csc=a.unity/'Resources/Scripting/DotNetSdkRoslyn/csc.dll'
refs=list((a.unity/'Resources/Scripting/Managed/UnityEngine').glob('*.dll'))+list((a.unity/'Resources/Scripting/NetStandard/ref/2.1.0').glob('*.dll'))
refs += [a.assemblies/n for n in ['Unity.XR.Interaction.Toolkit.dll','Unity.InputSystem.dll','Unity.XR.CoreUtils.dll']]
assert all(p.exists() for p in refs) and len(refs)>20,'Unity/XRI references not found; run Unity import or supply --assemblies.'
logs=[]
def compile(name,sources,references,target='library'):
 rsp=out/(name+'.rsp');dll=out/(name+'.dll')
 args=['-nologo','-target:'+target,'-langversion:latest','-out:'+str(dll)]+['-r:'+str(p) for p in references]+[str(p) for p in sources]
 rsp.write_text('\n'.join('"'+arg+'"' for arg in args))
 r=subprocess.run([str(dotnet),str(csc),'@'+str(rsp)],capture_output=True,text=True);logs.append(r.stdout+r.stderr)
 print(r.stdout+r.stderr,end='');assert r.returncode==0,name+' compile failed';print('PASS C# reference compile:',name,flush=True);return dll
runtime=compile('LK.Gameplay.Interaction',sorted(root.glob('*.cs')),refs)
editorrefs=list((a.unity/'Resources/Scripting/Managed').glob('UnityEditor.*.dll'))
compile('LK.Gameplay.Interaction.Editor',sorted((root/'Editor').glob('*.cs')),refs+[runtime]+editorrefs)
netcore=next((a.unity/'Resources/Scripting/NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
rules=compile('InteractionRules',[root/'Verification/InteractionRules.check',root/'StrokeCycle.cs',root/'GeneratorStartState.cs',root/'AnalogButtonState.cs'],list(netcore.glob('*.dll')),'exe')
(out/'InteractionRules.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':netcore.name}}}))
r=subprocess.run([str(dotnet),str(rules)],capture_output=True,text=True);print(r.stdout+r.stderr,end='');logs.append(r.stdout+r.stderr);assert r.returncode==0,'behavioral rules failed'
allguids={}
for p in Path('Assets').rglob('*.meta'):
 m=re.search(r'^guid: (\w+)',p.read_text(errors='replace'),re.M)
 if m:allguids[m[1]]=p
for side in ('L','R'):
 model=Path(f'Assets/Art/Hands/Resources/Hands/Glove_{side}.json');d=json.loads(model.read_text());n=len(d['vertices'])
 assert len(d['bones'])==26 and len(d['poses'])==4
 assert [p['name'] for p in d['poses']]==['Open','Fist','WrapGrip','Pinch']
 assert all(b['parent']<i for i,b in enumerate(d['bones']))
 assert len(d['normals'])==n and len(d['boneWeights'])==n and len(d['boneIndices'])==2*n
 assert all(0<=i<n for i in d['triangles']) and all(0<=i<26 for i in d['boneIndices'])
 assert all(0<=w<=1 for w in d['boneWeights'])
 assert all(abs(sum(v*v for v in q.values())-1)<.001 for p in d['poses'] for q in p['rotations'])
 assert all(len(p['rotations'])==26 for p in d['poses'])
 edges=collections.Counter()
 for i in range(0,len(d['triangles']),3):
  tri=d['triangles'][i:i+3]
  for x,y in zip(tri,tri[1:]+tri[:1]):edges[tuple(sorted((x,y)))]+=1
 assert all(c==2 for c in edges.values()),'glove surface must be closed/manifold'
 prefab=Path(f'Assets/Prefabs/Hands/Resources/Hands/PF_Hand_{side}.prefab').read_text()
 assert prefab.count('\nTransform:')==28
 for guid in re.findall(r'guid: (\w+)',prefab):assert guid in allguids,'unresolved prefab reference '+guid
 print(f'PASS authored {side}: 26 joints, four normalized poses, {n} weighted vertices, closed surface, prefab references resolve',flush=True)
logs.append('PASS runtime/editor C# reference compilation, behavioral assertions, and hand payload/prefab structural checks. Unity import, shader compile, Play Mode, APK and headset NOT executed.\n')
(out/'hands-offline-verification.txt').write_text('\n'.join(logs))
