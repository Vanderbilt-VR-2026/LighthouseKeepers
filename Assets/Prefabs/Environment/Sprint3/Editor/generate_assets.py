#!/usr/bin/env python3
"""Deterministic native Unity mesh/prefab authoring. No copied built-in meshes.
Run from repository root. Unity import, baking and device acceptance remain separate.
"""
from pathlib import Path
import hashlib, json, math, re, struct
ROOT = Path('Assets/Prefabs/Environment/Sprint3')
ART = Path('Assets/Art/Sprint3')
HEADER = '%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'
def guid(path):
    p=Path(str(path)+'.meta')
    if p.exists(): return re.search(r'^guid: (\w+)',p.read_text(),re.M)[1]
    return hashlib.sha256(('LK-Sprint3-C/'+str(path)).encode()).hexdigest()[:32]
def meta(path, fileid=None):
    p=Path(path); m=Path(str(p)+'.meta')
    if m.exists(): return
    g=guid(p)
    if p.is_dir(): body='folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n'
    elif p.suffix == '.cs': body='MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {fileID: 0}\n'
    elif fileid: body=f'NativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: {fileid}\n'
    elif p.suffix == '.asmdef': body='AssemblyDefinitionImporter:\n  externalObjects: {}\n'
    elif p.suffix == '.json': body='TextScriptImporter:\n  externalObjects: {}\n'
    elif p.suffix == '.prefab': body='PrefabImporter:\n  externalObjects: {}\n'
    else: body='DefaultImporter:\n  externalObjects: {}\n'
    m.write_text(f'fileFormatVersion: 2\nguid: {g}\n'+body+'  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
def vec(v, letters='xyz'): return '{'+', '.join(f'{a}: {x:.9g}' for a,x in zip(letters,v))+'}'
def quat(axis,angle):
    s=math.sin(math.radians(angle)/2); return (*[a*s for a in axis], math.cos(math.radians(angle)/2))
def ref(path,fid): return f'{{fileID: {fid}, guid: {guid(path)}, type: 2}}'
# Flat-shaded triangles, two UV channels, deterministic and lightmapper-readable.
def mesh(name, vertices, triangles):
    expanded=[]; indices=[]
    for i in range(0,len(triangles),3):
        pts=[vertices[t] for t in triangles[i:i+3]]
        a=[pts[1][j]-pts[0][j] for j in range(3)]; b=[pts[2][j]-pts[0][j] for j in range(3)]
        n=[a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0]]
        length=math.sqrt(sum(x*x for x in n)); n=[x/max(length,1e-9) for x in n]
        # Each triangle has a separate, padded UV2 chart; Unity may repack during import/bake.
        cell=i//3; cols=math.ceil(math.sqrt(len(triangles)//3)); x=cell%cols; y=cell//cols
        uv2=[((x+.12)/cols,(y+.12)/cols),((x+.88)/cols,(y+.12)/cols),((x+.12)/cols,(y+.88)/cols)]
        for k,p in enumerate(pts):
            expanded.append((*p,*n,*[(0,0),(1,0),(0,1)][k],*uv2[k])); indices.append(len(indices))
    lo=[min(p[i] for p in vertices) for i in range(3)]; hi=[max(p[i] for p in vertices) for i in range(3)]
    center=[(a+b)/2 for a,b in zip(lo,hi)]; extent=[(b-a)/2 for a,b in zip(lo,hi)]
    template=Path('Assets/_LighthouseKeepers/Art/Models/Ceiling_2.asset').read_text()
    template=re.sub(r'm_Name: .*', 'm_Name: '+name,template,count=1)
    template=re.sub(r'indexCount: \d+',f'indexCount: {len(indices)}',template,count=1)
    template=re.sub(r'vertexCount: \d+',f'vertexCount: {len(expanded)}',template,count=1)
    template=re.sub(r'm_VertexCount: \d+',f'm_VertexCount: {len(expanded)}',template,count=1)
    template=re.sub(r'm_IndexBuffer: .*', 'm_IndexBuffer: '+struct.pack('<'+'H'*len(indices),*indices).hex(),template,count=1)
    data=b''.join(struct.pack('<10f',*v) for v in expanded)
    template=re.sub(r'm_DataSize: \d+',f'm_DataSize: {len(data)}',template,count=1)
    template=re.sub(r'_typelessdata: .*','_typelessdata: '+data.hex(),template,count=1)
    template=re.sub(r'm_Center: \{[^}]+\}', 'm_Center: '+vec(center),template)
    template=re.sub(r'm_Extent: \{[^}]+\}', 'm_Extent: '+vec(extent),template)
    path=ART/'Meshes'/(name+'.asset');path.write_text(template);meta(path,4300000)
    return path
mesh_cache={}
def box(size, bevel=0):
    key=('box',*size,bevel)
    if key in mesh_cache:return mesh_cache[key]
    # A clipped-corner timber/metal profile; four narrow edge faces show actual worn edges.
    x,y,z=[s/2 for s in size]; b=min(bevel,x*.3,z*.3)
    ring=[(-x+b,-z),(x-b,-z),(x,-z+b),(x,z-b),(x-b,z),(-x+b,z),(-x,z-b),(-x,-z+b)] if b else [(-x,-z),(x,-z),(x,z),(-x,z)]
    v=[(a,h,c) for h in (-y,y) for a,c in ring]; n=len(ring); t=[]
    for i in range(1,n-1):t += [0,i,i+1,n,n+i+1,n+i]
    for i in range(n):j=(i+1)%n;t += [i,n+i,n+j,i,n+j,j]
    p=mesh('SM_C_'+hashlib.sha256(str(key).encode()).hexdigest()[:10],v,t);mesh_cache[key]=p;return p
def cylinder(radius,height,segments=20):
    key=('cylinder',radius,height,segments)
    if key in mesh_cache:return mesh_cache[key]
    v=[(radius*math.cos(i*math.tau/segments),h,radius*math.sin(i*math.tau/segments)) for h in (-height/2,height/2) for i in range(segments)]
    t=[]
    for i in range(1,segments-1):t += [0,i,i+1,segments,segments+i+1,segments+i]
    for i in range(segments):j=(i+1)%segments;t +=[i,segments+i,segments+j,i,segments+j,j]
    p=mesh('SM_C_'+hashlib.sha256(str(key).encode()).hexdigest()[:10],v,t);mesh_cache[key]=p;return p
def plane():
    return mesh('SM_C_Decal',[(-.5,-.5,0),(.5,-.5,0),(.5,.5,0),(-.5,.5,0)],[0,1,2,0,2,3])
def mat(name,color,metallic=0,emission=0):
    path=ART/'Materials'/('M_C_'+name+'.mat')
    s=Path('Assets/Art/Materials/Environment/M_Environment_Timber.mat').read_text()
    s=re.sub(r'm_Name: M_Environment_Timber','m_Name: M_C_'+name,s,count=1)
    c=[int(color[i:i+2],16)/255 for i in (0,2,4)] if isinstance(color,str) else color
    s=re.sub(r'- _BaseColor: \{[^}]+\}', '- _BaseColor: '+vec([*c,1],'rgba'),s)
    s=re.sub(r'- _Color: \{[^}]+\}', '- _Color: '+vec([*c,1],'rgba'),s)
    s=re.sub(r'- _Metallic: .*',f'- _Metallic: {metallic}',s)
    s=re.sub(r'- _EmissionColor: \{[^}]+\}', '- _EmissionColor: '+vec([*[x*emission for x in c],1],'rgba'),s)
    if emission:
        s=s.replace('m_ValidKeywords: []','m_ValidKeywords:\n  - _EMISSION').replace('m_LightmapFlags: 4','m_LightmapFlags: 2')
    path.write_text(s);meta(path,2100000);return path
M={k:mat(k,*v) for k,v in {
 'Board':([.85,.85,.85],0,.15),'Aluminium':('A6ADB3',.8,0),'Timber':('6A4A2D',0,0),
 'TimberWear':('AA8554',0,0),'BlueEnamel':('2C5F7A',.45,0),'Steel':('676F72',.75,0),'Iron':('272E30',.6,0),
 'YellowEnamel':('C8961E',.45,0),'Concrete':('A7A295',0,0),'RedEnamel':('A83F2A',.3,0),
 'Ink':('050505',0,0),'Dial':('D8D5BC',0,.08),'Warm':('FFB85C',0,1.5),'Emergency':('8B2B22',0,0),
 'BoardLamp':('FFB85C',0,.8),'Green':('458C40',0,1),'Blanket':('3B5547',0,0),'Linen':('CEC4A9',0,0)}.items()}
class Node:
    seq=600000000000
    all=[]
    def __init__(self,name,parent=None,pos=(0,0,0),rot=(0,0,0,1),scale=(1,1,1),active=True):
        Node.seq+=20;self.id=Node.seq; self.name=name;self.parent=parent;self.pos=pos;self.rot=rot;self.scale=scale;self.active=active;self.children=[];self.components=[]
        if parent:parent.children.append(self)
        Node.all.append(self)
    def add(self,cls,body):
        fid=self.id+2+len(self.components);self.components.append((cls,fid,body));return fid
    def script(self,path,body=''):
        return self.add(114,f'  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {ref(path,11500000)}\n  m_Name: \n  m_EditorClassIdentifier: \n'.replace('type: 2','type: 3')+body)
    def render(self,mesh,material):
        self.add(33,f'  m_Mesh: {ref(mesh,4300000)}\n')
        self.add(23,'  m_Enabled: 1\n  m_CastShadows: 1\n  m_ReceiveShadows: 1\n  m_DynamicOccludee: 1\n  m_StaticShadowCaster: 0\n  m_MotionVectors: 0\n  m_LightProbeUsage: 1\n  m_ReflectionProbeUsage: 1\n  m_RayTracingMode: 0\n  m_RenderingLayerMask: 1\n  m_RendererPriority: 0\n  m_Materials:\n  - '+ref(material,2100000)+'\n  m_StaticBatchInfo:\n    firstSubMesh: 0\n    subMeshCount: 0\n  m_StaticBatchRoot: {fileID: 0}\n  m_ProbeAnchor: {fileID: 0}\n  m_LightProbeVolumeOverride: {fileID: 0}\n  m_ScaleInLightmap: 1\n  m_ReceiveGI: 1\n  m_PreserveUVs: 0\n  m_IgnoreNormalsForChartDetection: 0\n  m_ImportantGI: 0\n  m_StitchLightmapSeams: 1\n  m_SelectedEditorRenderState: 3\n  m_MinimumChartSize: 4\n  m_AutoUVMaxDistance: 0.5\n  m_AutoUVMaxAngle: 89\n  m_LightmapParameters: {fileID: 0}\n  m_SortingLayerID: 0\n  m_SortingLayer: 0\n  m_SortingOrder: 0\n  m_AdditionalVertexStreams: {fileID: 0}\n')
    def collider(self,size,center=(0,0,0),trigger=False):
        self.add(65,'  m_Material: {fileID: 0}\n  m_IncludeLayers:\n    serializedVersion: 2\n    m_Bits: 0\n  m_ExcludeLayers:\n    serializedVersion: 2\n    m_Bits: 0\n  m_LayerOverridePriority: 0\n  m_IsTrigger: '+str(int(trigger))+'\n  m_ProvidesContacts: 0\n  m_Enabled: 1\n  serializedVersion: 3\n  m_Size: '+vec(size)+'\n  m_Center: '+vec(center)+'\n')
    def dump(self):
        common='  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n'
        s=f'--- !u!1 &{self.id}\nGameObject:\n'+common+'  serializedVersion: 6\n  m_Component:\n'+f'  - component: {{fileID: {self.id+1}}}\n'+''.join(f'  - component: {{fileID: {i}}}\n' for _,i,_ in self.components)+f'  m_Layer: 0\n  m_Name: {self.name}\n  m_TagString: Untagged\n  m_Icon: {{fileID: 0}}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: {int(self.active)}\n'
        children='\n'+''.join(f'  - {{fileID: {n.id+1}}}\n' for n in self.children) if self.children else ' []\n'
        s+=f'--- !u!4 &{self.id+1}\nTransform:\n'+common+f'  m_GameObject: {{fileID: {self.id}}}\n  serializedVersion: 2\n  m_LocalRotation: '+vec(self.rot,'xyzw')+'\n  m_LocalPosition: '+vec(self.pos)+'\n  m_LocalScale: '+vec(self.scale)+'\n  m_ConstrainProportionsScale: 0\n  m_Children:'+children+f'  m_Father: {{fileID: {self.parent.id+1 if self.parent else 0}}}\n  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}\n'
        types={33:'MeshFilter',23:'MeshRenderer',65:'BoxCollider',114:'MonoBehaviour'}
        for cls,i,b in self.components:s+=f'--- !u!{cls} &{i}\n{types[cls]}:\n'+common+f'  m_GameObject: {{fileID: {self.id}}}\n'+b
        return s

def part(parent,name,pos,size,material,bevel=.008,collision=False,rot=(0,0,0,1)):
    n=Node(name,parent,pos,rot);n.render(box(size,bevel),M[material]);
    if collision:n.collider(size)
    return n
def cyl(parent,name,pos,r,h,material,rot=(0,0,0,1),collision=False):
    n=Node(name,parent,pos,rot);n.render(cylinder(r,h),M[material]);
    if collision:n.collider((r*2,h,r*2))
    return n
def label(parent,name,text,pos,cap=.025,width=.5,color=(0,0,0,1)):
    n=Node(name,parent,pos);n.script(ROOT/'Runtime/FixtureLabel.cs',f'  text: {json.dumps(text)}\n  capHeightM: {cap}\n  widthM: {width}\n  color: '+vec(color,'rgba')+'\n');return n
root=Node('Sprint 3 environment fixtures')
root.script(ROOT/'Runtime/ShipScene.cs')
contract=json.load(open('Docs/StationContract.json'))
stations={s['id']:s for s in contract['stations']}
def station(name,pos=None,yaw=0):
    if pos is None:
        s=stations[name];pos=s['anchor']['position'];yaw=s['anchor']['rotationEuler'][1]
    return Node(name,root,pos,quat((0,1,0),yaw))
board=station('STN_BRIEFING_BOARD')
part(board,'White enamel writing surface',(0,1.4,.022),(1.2,.8,.04),'Board',.002,True)
for x in (-.61,.61):part(board,'Aluminium vertical frame',(x,1.4,.023),(.02,.84,.046),'Aluminium',.002)
for y in (.99,1.81):part(board,'Aluminium horizontal frame',(0,y,.023),(1.24,.02,.046),'Aluminium',.002)
for i in range(5):
    y=1.51-i*.105;boxroot=Node('Checkbox '+str(i+1),board,(.50,y,.05))
    for x in (-.023,.023):part(boxroot,'Checkbox side',(x,0,0),(.004,.046,.002),'Ink',0)
    for h in (-.023,.023):part(boxroot,'Checkbox edge',(0,h,0),(.046,.004,.002),'Ink',0)
    tick=Node('Tick '+str(i+1),board,(.50,y,.052),scale=(-1,1,1),active=False)
    part(tick,'Tick short',(-.01,-.005,0),(.026,.006,.002),'Ink',0,rot=quat((0,0,1),-45))
    part(tick,'Tick long',(.009,.003,0),(.043,.006,.002),'Ink',0,rot=quat((0,0,1),45))
board.script(ROOT/'Runtime/BriefingBoardView.cs','  font: {fileID: 11400000, guid: 8f586378b4e144a9851e7b34d9b748ee, type: 2}\n  initialHeading: KEEPER WATCH\n  initialRows:\n  - Take the wrench\n  - Find the leak\n  - Seal the pipe\n  - Restore power\n  - Regroup upstairs\n')
# Wall is at local z=0. Top fixture brackets hold a dedicated down-aimed spotlight.
part(board,'Board task lamp bracket',(0,1.99,.09),(.16,.035,.18),'Iron')
cyl(board,'Board task lamp shade',(0,1.97,.16),.075,.055,'Aluminium')
cyl(board,'Board task lamp lens',(0,1.935,.16),.06,.008,'BoardLamp')
Node('LIGHT_BoardEmergencySpot',board,(0,1.92,.20),quat((1,0,0),145))
# Keep the frozen board anchor; offset only the new fixture to the actual wall face z=-4.85.
for child in board.children: child.pos=(child.pos[0],child.pos[1],child.pos[2]-.20)
bench=station('STN_TOOL_TABLE',(1.8,0,-5.75),180)
part(bench,'Solid timber top',(0,.87,0),(1.4,.06,.7),'Timber',.012,True)
for x in (-.59,.59):
 for z in (-.24,.24):
    part(bench,'Square tenoned leg',(x,.42,z),(.10,.84,.10),'Timber',.006,True)
    part(bench,'Exposed tenon',(x,.83,z),(.075,.03,.12),'TimberWear',.003)
part(bench,'Lower stretcher rail',(0,.22,0),(1.18,.085,.08),'Timber',.004,True)
for x in (-.59,.59):part(bench,'Cross stretcher',(x,.22,0),(.08,.085,.48),'Timber',.004,True)
part(bench,'Worn front arris',(0,.895,.342),(1.32,.008,.008),'TimberWear',.003)
Node('DOCK_Wrench',bench,(.18,.923,.24))
# One forged tool silhouette. Custom mesh is assembled from extruded handle and open jaws.
wrench=station('Wrench mesh and grab bounds',(1.62,.925,-5.99))
wrench_v=[];wrench_t=[]
for center,size in [((0,0,-.025),(.034,.024,.27)),((-.037,0,.124),(.027,.03,.095)),((.037,0,.124),(.027,.03,.095)),((0,0,.091),(.10,.03,.028))]:
 x,y,z=[s/2 for s in size];o=len(wrench_v)
 wrench_v += [(center[0]+a*x,center[1]+b*y,center[2]+c*z) for a,b,c in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]]
 wrench_t += [o+i for i in [0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,2,3,7,2,7,6,1,2,6,1,6,5,3,0,4,3,4,7]]
wrenchmesh=mesh('SM_C_ForgedWrench',wrench_v,wrench_t);wrench.render(wrenchmesh,M['Steel']);wrench.collider((.12,.035,.35));wrench.collider((.24,.16,.46),trigger=True)
# F1 lever retains a +X axis and a 0.25 m grip radius. Only A moves the pivot.
pump=station('STN_MANUAL_PUMP',(.45,0,1.25),90)
part(pump,'Cast mounting base',(0,.045,.18),(.46,.09,.48),'BlueEnamel',.025,True)
for x in (-.17,.17):
 for z in (.02,.34):cyl(pump,'Floor anchor bolt',(x,.10,z),.025,.025,'Steel')
cyl(pump,'Cast pump barrel',(0,.49,.18),.125,.80,'BlueEnamel',collision=True)
cyl(pump,'Barrel upper collar',(0,.89,.18),.16,.07,'BlueEnamel')
cyl(pump,'Polished piston',(0,.98,.18),.027,.20,'Steel')
part(pump,'Pivot yoke',(0,1.05,.28),(.20,.12,.20),'BlueEnamel',.018)
cyl(pump,'Axle pin',(0,1.05,.33),.035,.25,'Steel',quat((0,0,1),90))
pivot=Node('PIVOT_PumpHandle',pump,(0,1.05,.38))
part(pivot,'Forged lever',(0,0,.125),(.034,.035,.25),'BlueEnamel',.008)
cyl(pivot,'Horizontal rubber grip',(0,0,.25),.028,.20,'Iron',quat((0,0,1),90))
Node('GRIP_PumpHandle',pivot,(0,0,.25))
# Welded outlet bends forward and discharges into a visible floor sleeve, not through the floor mesh.
cyl(pump,'Discharge outlet',(0,.29,.38),.04,.30,'BlueEnamel',quat((1,0,0),90))
cyl(pump,'Discharge drop',(0,.145,.53),.04,.23,'BlueEnamel')
cyl(pump,'Floor discharge sleeve',(0,.015,.53),.065,.03,'Steel')
Node('VFX_PumpDischarge',pump,(0,.26,.53),quat((1,0,0),90))
part(pump,'Barrel chipped enamel',(.122,.51,.18),(.005,.12,.035),'Steel',.001)
part(pump,'Lever worn edge',(0,.020,.12),(.035,.004,.12),'Steel',.001)
pump.script(ROOT/'Runtime/PumpDischargeEmitter.cs','  waterMaterial: '+ref('Assets/_LighthouseKeepers/Art/Materials/LK_Rain.mat',2100000)+'\n')
label(pump,'Manual pump plate','HAND PUMP',(0,.68,.308),.024,.28,(1,1,1,1))
part(pump,'Recovery lamp support',(0,1.33,.18),(.035,.61,.035),'Iron',.004)
part(pump,'Pump recovery cage',(0,1.67,.18),(.16,.10,.10),'Iron',.008)
part(pump,'Pump recovery lens',(0,1.62,.23),(.12,.018,.06),'Emergency',.002)
Node('LIGHT_ManualPumpEmergency',pump,(0,1.61,.26))
# Preserve the large manifold anchor and repair contact from the contract.
pipe=station('STN_PIPE_MANIFOLD')
cyl(pipe,'Main service header',(0,.95,.23),.09,2.8,'BlueEnamel',quat((0,0,1),90))
for x in (-1.1,1.1):
 part(pipe,'Header foot',(x,.035,.24),(.30,.07,.35),'Iron',.015,True)
 part(pipe,'Header pedestal',(x,.48,.23),(.075,.89,.10),'Iron',.005,True)
for x in (-.8,.8):cyl(pipe,'Service flange',(x,.95,.23),.135,.10,'Steel',quat((0,0,1),90))
Node('STN_PIPE_LEAK',pipe,(.8,.95,.45))
# F2; exact PLINTH_TOP stays the geometric threshold, decorative lip does not raise it.
gen=station('STN_GENERATOR')
part(gen,'Concrete plinth',(0,.225,.375),(2.10,.45,.75),'Concrete',.01,True)
part(gen,'Crisp plinth top edge',(0,.442,.39),(2.07,.016,.71),'Concrete',.002)
Node('PLINTH_TOP',gen,(0,.45,.4))
part(gen,'Diesel crankcase',(-.32,.86,.39),(1.20,.69,.57),'YellowEnamel',.055,True)
part(gen,'Alternator cover',(.64,.82,.39),(.55,.60,.56),'YellowEnamel',.045,True)
part(gen,'Fuel tank',(-.32,1.29,.35),(1.12,.18,.51),'YellowEnamel',.035)
cyl(gen,'Filler cap',(-.65,1.40,.35),.065,.05,'Steel')
for x in (-.80,.78):part(gen,'Resilient mounting foot',(x,.485,.38),(.20,.07,.55),'Iron',.01)
for x in [-.77+i*.065 for i in range(10)]:part(gen,'Radiator grille',(x,.88,.687),(.021,.42,.012),'Iron',.003)
part(gen,'DIESEL enamel plaque',(.35,1.10,.684),(.92,.19,.016),'YellowEnamel',.003)
label(gen,'DIESEL inscription','DIESEL',(.35,1.10,.696),.075,.82)
for x in (-.83,.85):part(gen,'Paint wear at service edge',(x,1.13,.684),(.035,.07,.005),'Steel',.002)
# Lagged vertical stack and wallward elbow; wall penetration is a declared architectural service.
cyl(gen,'Lagged exhaust riser',(-.72,1.72,.12),.08,.62,'Linen')
cyl(gen,'Lagged exhaust wall elbow',(-.72,2.03,.07),.08,.13,'Linen',quat((1,0,0),90))
for y in (1.46,1.60,1.74,1.88):cyl(gen,'Exhaust lagging band',(-.72,y,.12),.084,.018,'Steel')
# Additional A-facing grip names do not replace the frozen PRIMER / panel STARTER aliases.
for name,pos in [('GRIP_PrimerLever',(-.24,1.15,.75)),('GRIP_ChokeLever',(0,1.15,.75)),('GRIP_StarterHandle',(.24,1.15,.75)),('READOUT_FuelGauge',(.55,1.35,.70))]:
 n=Node(name,gen,pos)
 if 'Gauge' not in name:
  cyl(n,'Control stem',(0,0,-.035),.012,.07,'Steel',quat((1,0,0),90));cyl(n,'Control grip',(0,0,0),.025,.10,'Iron',quat((0,0,1),90))
 else:
  cyl(n,'Fuel gauge bezel',(0,0,0),.072,.018,'Steel',quat((1,0,0),90));cyl(n,'Fuel gauge face',(0,0,.011),.060,.004,'Dial',quat((1,0,0),90));part(n,'Fuel needle',(0,.014,.016),(.004,.07,.003),'Ink',0,rot=quat((0,0,1),-25))
label(gen,'Starter legend','PRIME   CHOKE   START',(0,1.00,.718),.025,.92)
part(gen,'Starter lamp support',(0,1.55,.54),(.04,.44,.04),'Iron',.004)
part(gen,'Starter recovery cage',(0,1.80,.60),(.18,.10,.10),'Iron',.008)
part(gen,'Starter recovery lens',(0,1.75,.64),(.14,.018,.06),'Emergency',.002)
Node('LIGHT_GeneratorStarterEmergency',gen,(0,1.73,.66))
panel=station('STN_GENERATOR_PANEL')
part(panel,'Panel steel enclosure',(0,1.12,.11),(.80,1.12,.22),'Iron',.025,True)
part(panel,'Panel instrument face',(0,1.12,.226),(.70,1.02,.012),'Concrete',.005)
for x in (-.19,.19):
 cyl(panel,'Analogue gauge bezel',(x,1.29,.25),.13,.04,'Steel',quat((1,0,0),90))
 cyl(panel,'Analogue gauge face',(x,1.29,.274),.112,.006,'Dial',quat((1,0,0),90))
 part(panel,'Analogue needle',(x,1.31,.280),(.008,.12,.004),'Ink',0,rot=quat((0,0,1),-35))
 for a in range(-60,61,30):
  part(panel,'Scale graduation',(x+math.sin(math.radians(a))*.088,1.29+math.cos(math.radians(a))*.088,.282),(.006,.018,.003),'Ink',0,rot=quat((0,0,1),-a))
for x,col,title in [(-.19,'Green','RUNNING'),(.19,'RedEnamel','FAULT')]:
 cyl(panel,title+' indicator',(x,.98,.262),.045,.025,col,quat((1,0,0),90))
 label(panel,title+' label',title,(x,.87,.265),.023,.32)
# Glazed door uses four slim rails and an authored transparent glass panel assigned by the editor.
for x in (-.37,.37):part(panel,'Glazed door upright',(x,1.12,.295),(.025,1.08,.025),'Aluminium',.003)
for y in (.58,1.66):part(panel,'Glazed door rail',(0,y,.295),(.76,.025,.025),'Aluminium',.003)
glass=part(panel,'Glazed inspection pane',(0,1.12,.29),(.71,1.02,.004),'Aluminium',0)
glass_path=ART/'Materials/M_C_PanelGlass.mat'
glass_text=Path('Assets/Art/Materials/Environment/M_Environment_Glass.mat').read_text().replace('M_Environment_Glass','M_C_PanelGlass')
glass_path.write_text(glass_text);meta(glass_path,2100000)
glass.components[-1]=(glass.components[-1][0],glass.components[-1][1],glass.components[-1][2].replace(ref(M['Aluminium'],2100000),ref(glass_path,2100000)))
# Standalone dressing sits east, away from the western stair transfer.
dress=station('F2 service stores',(1.55,3.3,.25))
for x in (-.21,.18):
 part(dress,'Red diesel jerry can',(x,.22,0),(.30,.44,.18),'RedEnamel',.025,True)
 part(dress,'Jerry can handle',(x,.47,0),(.18,.045,.06),'Iron',.008)
 for a in (-35,35):part(dress,'Pressed can rib',(x,.22,.098),(.018,.30,.01),'Steel',.003,rot=quat((0,0,1),a))
cyl(dress,'Oil drum',(.05,.40,-.58),.23,.80,'Iron',collision=True)
for y in (.07,.36,.73):cyl(dress,'Drum rolled seam',(.05,y,-.58),.237,.025,'Steel')
cyl(dress,'Funnel spout',(.05,.85,-.58),.022,.10,'Steel')
cyl(dress,'Funnel bowl',(.05,.91,-.58),.09,.04,'Steel')
bed=station('STN_BED',(0,6.6,.10))
for x in (-.46,.46):
 for z in (-.88,.88):part(bed,'Bed leg',(x,.20,z),(.08,.40,.08),'Timber',.006,True)
part(bed,'Bed timber rail',(0,.40,0),(1.06,.16,2.06),'Timber',.012,True)
part(bed,'Mattress',(0,.56,0),(1.00,.16,2.00),'Linen',.018)
part(bed,'Wool blanket',(0,.665,-.20),(1.015,.05,1.48),'Blanket',.012)
part(bed,'Pillow',(0,.71,.72),(.70,.12,.36),'Linen',.025)
part(bed,'Headboard',(0,.74,1.0),(1.06,.60,.06),'Timber',.008,True)
# Fixtures for every landing, plus four recovery stations; the editor creates attached Lights.
house_emergency=station('Caged emergency House',(3.31,2.35,-7.35),270)
part(house_emergency,'Emergency wall bracket',(0,0,-.08),(.10,.10,.18),'Iron',.004)
part(house_emergency,'Red caged lens',(0,0,.04),(.09,.18,.07),'Emergency',.008)
for x in (-.065,.065):part(house_emergency,'Emergency cage bar',(x,0,.08),(.008,.22,.008),'Steel',.001)
Node('LIGHT_EmergencyHouse',house_emergency,(0,0,.13))
for floor,y,r in [('F1',0,3.9),('F2',3.3,3.7),('F3',6.6,3.65),('F4',9.9,2.7)]:
 for az in (135,315):
  a=math.radians(az);s=station('Caged emergency '+floor+' '+str(az),(math.sin(a)*(r-.18),y+2.35,math.cos(a)*(r-.18)),az+180)
  part(s,'Wall fixture bracket',(0,0,-.085),(.10,.10,.17),'Iron',.004)
  part(s,'Cage wall plate',(0,0,0),(.16,.23,.035),'Iron',.006)
  cyl(s,'Red emergency lens',(0,0,.07),.055,.17,'Emergency')
  for x in (-.07,.07):part(s,'Protective cage bar',(x,0,.10),(.008,.21,.008),'Steel',.001)
  Node('LIGHT_EmergencyLanding',s,(0,0,.14))
for name,anchor in [('Breaker','STN_BREAKER_PANEL'),('Beacon','STN_BEACON_CONTROL')]:
 s=stations[anchor];node=station('Recovery light '+name,s['anchor']['position'],s['anchor']['rotationEuler'][1]);part(node,'Recovery fixture',(0,1.65,.18),(.18,.10,.10),'Iron',.008);part(node,'Recovery lens',(0,1.62,.23),(.14,.02,.06),'Emergency',.002);Node('LIGHT_'+name+'Emergency',node,(0,1.60,.25))
# Author a separate custom decal mesh; the editor positions it on the exterior wall.
plane()
# Soot patch stays inside the wall surface (no texture/package dependency). The editor places
# the exterior patch against the curved wall using the actual wall raycast.
Node('EXHAUST_ExteriorSoot',root,(1.50,5.33,3.92),quat((0,1,0),20))
# Bespoke replacements for older scene construction pieces; the canonical station props above
# use their own metre-sized meshes. These retain the existing transform/collider layout.
legacy_box=box((1,1,1),.012)
legacy_cylinder=cylinder(.5,2,24)
legacy_quad=plane()
legacy_plane=mesh('SM_C_LegacyFloorSheet',[(-5,0,-5),(5,0,-5),(5,0,5),(-5,0,5)],[0,2,1,0,3,2])
# Save source native assets and a reusable station-prefab hierarchy.
Path(ROOT/'LegacyMeshMap.json').write_text(json.dumps({'10202':str(legacy_box),'10206':str(legacy_cylinder),'10210':str(legacy_quad),'10209':str(legacy_plane)},indent=2)+'\n')
prefab=ROOT/'PF_Sprint3Fixtures.prefab';prefab.write_text(HEADER+''.join(n.dump() for n in Node.all));meta(prefab)
# Explicit manifest is the authoring/build gate, never derived from currently open scenes.
Path('Assets/Scenes/Sprint3/ShipScenes.json').write_text(json.dumps({'scenes':['Assets/Scenes/Lab/LK_Lab_Environment.unity']},indent=2)+'\n')
Path(ROOT/'FixtureDimensions.json').write_text(json.dumps({'source':'Docs/StationContract.json v1.0','generatorPlinthTopWorldY':3.75,'manualPumpRoot':[.45,0,1.25],'manualPumpYaw':90,'pumpPivotY':1.05,'pumpGripRadiusM':.25,'pumpArcDegrees':[-22.5,22.5],'pumpVerticalTravelM':2*.25*math.sin(math.radians(22.5)),'benchTopM':[1.4,.06,.7],'benchSurfaceY':.9,'boardSurfaceM':[1.2,.8,.04],'boardBottomY':1.0,'bodyCapHeightM':.045,'headingCapHeightM':.065},indent=2)+'\n')
for top in [ROOT,ART,Path('Assets/Scenes/Sprint3'),Path('Assets/Settings/Lighting/Sprint3')]:
 for p in [top,*top.rglob('*')]:
  if p.suffix != '.meta':meta(p)
for top in [ROOT,ART,Path('Assets/Scenes/Sprint3')]:
 for p in top.rglob('*'):
  if p.is_file() and p.suffix in {'.prefab','.mat','.asset','.meta'}:
   p.write_text('\n'.join(line.rstrip() for line in p.read_text().splitlines())+'\n')
print(f'Authored {len(Node.all)} nodes and {len(mesh_cache)+2} custom meshes; import/bake/device checks pending.')
