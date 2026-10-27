"""Serialize deterministic Unity hand prefabs/metas without an Editor dependency."""
from pathlib import Path
import hashlib,json,os
ROOT=Path.cwd()
def guid(path):return hashlib.md5(('LK.Sprint3.Hands:'+str(path)).encode()).hexdigest()
def meta(p):
 path=p.relative_to(ROOT)
 if p.suffix=='.meta' or p.name=='__pycache__':return
 if p.is_dir():body='folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
 elif p.suffix=='.cs':body='MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
 elif p.suffix=='.json':body='TextScriptImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
 elif p.suffix=='.prefab':body='PrefabImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
 elif p.suffix in ('.mat','.asset'):body='NativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: '+('2100000' if p.suffix=='.mat' else '11400000')+'\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
 elif p.suffix=='.shader':body='ShaderImporter:\n  externalObjects: {}\n  defaultTextures: []\n  nonModifiableTextures: []\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
 elif p.suffix=='.asmdef':body='AssemblyDefinitionImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
 elif p.suffix=='.wav':body='AudioImporter:\n  externalObjects: {}\n  serializedVersion: 7\n  defaultSettings:\n    serializedVersion: 2\n    loadType: 0\n    sampleRateSetting: 0\n    sampleRateOverride: 22050\n    compressionFormat: 1\n    quality: 0.7\n    conversionMode: 0\n    preloadAudioData: 1\n  platformSettingOverrides: {}\n  forceToMono: 1\n  normalize: 1\n  ambisonic: 0\n  loadInBackground: 0\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
 else:body='DefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
 target=p.with_name(p.name+'.meta')
 if not target.exists():target.write_text('fileFormatVersion: 2\nguid: '+guid(path)+'\n'+body)

header='%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'
mat=Path('Assets/Art/Hands/Resources/Hands/M_Glove.mat');mat.parent.mkdir(parents=True,exist_ok=True)
# Reference the installed URP Lit shader by its existing GUID.
cache=Path(os.environ.get('LK_PACKAGE_CACHE', 'Library/PackageCache'))
shader=next(cache.glob('com.unity.render-pipelines.universal@*/Shaders/Lit.shader.meta'))
shader_guid=next(line.split(': ')[1] for line in shader.read_text().splitlines() if line.startswith('guid:'))
mat.write_text(header+'''--- !u!21 &2100000
Material:
  serializedVersion: 8
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: M_Glove
  m_Shader: {fileID: 4800000, guid: '''+shader_guid+''', type: 3}
  m_Parent: {fileID: 0}
  m_ModifiedSerializedProperties: 0
  m_ValidKeywords: []
  m_InvalidKeywords: []
  m_LightmapFlags: 4
  m_EnableInstancingVariants: 1
  m_DoubleSidedGI: 0
  m_CustomRenderQueue: -1
  stringTagMap: {}
  disabledShaderPasses: []
  m_LockedProperties: 
  m_SavedProperties:
    serializedVersion: 3
    m_TexEnvs: []
    m_Ints: []
    m_Floats:
    - _Metallic: 0
    - _Smoothness: 0.22
    - _Surface: 0
    - _Cull: 2
    m_Colors:
    - _BaseColor: {r: 0.28, g: 0.36, b: 0.32, a: 1}
  m_BuildTextureStacks: []
''')

def gameobject(fileid,name,transform,script=None):
 return f'''--- !u!1 &{fileid}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {transform}}}
'''+(f'  - component: {{fileID: {script}}}\n' if script else '')+f'''  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
'''
def transform(fileid,go,parent,children,pos):
 return f'''--- !u!4 &{fileid}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: {pos['x']}, y: {pos['y']}, z: {pos['z']}}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:'''+(('\n'+''.join(f'  - {{fileID: {c}}}\n' for c in children)) if children else ' []\n')+f'''  m_Father: {{fileID: {parent}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
'''
for side,node in [('L',4),('R',5)]:
 data=json.loads(Path(f'Assets/Art/Hands/Resources/Hands/Glove_{side}.json').read_text());bones=data['bones']
 s=header+gameobject(1000,'PF_Hand_'+side,4000,114000)+transform(4000,1000,0,[4001],dict(x=0,y=0,z=0))
 s+=gameobject(1001,'Skinned glove',4001)+transform(4001,1001,4000,[4100],dict(x=0,y=0,z=0))
 for i,b in enumerate(bones):
  s+=gameobject(1100+i,b['name'],4100+i)+transform(4100+i,1100+i,4001 if b['parent']<0 else 4100+b['parent'],[4100+j for j,c in enumerate(bones) if c['parent']==i],b['position'])
 s+='''--- !u!114 &114000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: '''+guid(Path('Assets/Scripts/LK.Gameplay/Interaction/HandRig.cs'))+''', type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  model: {fileID: 4900000, guid: '''+guid(Path(f'Assets/Art/Hands/Resources/Hands/Glove_{side}.json'))+''', type: 3}
  gloveMaterial: {fileID: 2100000, guid: '''+guid(mat)+''', type: 2}
  hand: '''+str(node)+'''
  blendSeconds: 0.075
  actorId: 0
'''
 Path(f'Assets/Prefabs/Hands/Resources/Hands/PF_Hand_{side}.prefab').write_text(s)
settings=Path('Assets/Prefabs/Hands/Resources/Hands/InteractionSettings.asset')
if not settings.exists():
 settings.write_text(header+'''--- !u!114 &11400000
 MonoBehaviour:
   m_ObjectHideFlags: 0
   m_CorrespondingSourceObject: {fileID: 0}
   m_PrefabInstance: {fileID: 0}
   m_PrefabAsset: {fileID: 0}
   m_GameObject: {fileID: 0}
   m_Enabled: 1
   m_EditorHideFlags: 0
   m_Script: {fileID: 11500000, guid: '''+guid(Path('Assets/Scripts/LK.Gameplay/Interaction/InteractionSettings.cs'))+''', type: 3}
   m_Name: InteractionSettings
   m_EditorClassIdentifier: 
   floodTuning: {fileID: 0}
 ''')
particle_shader=next(cache.glob('com.unity.render-pipelines.universal@*/Shaders/Particles/ParticlesUnlit.shader.meta'))
particle_guid=next(line.split(': ')[1] for line in particle_shader.read_text().splitlines() if line.startswith('guid:'))
water=Path('Assets/Art/Hands/Resources/Hands/M_WaterSurge.mat')
water.write_text(mat.read_text().replace('M_Glove','M_WaterSurge').replace(shader_guid,particle_guid).replace('m_CustomRenderQueue: -1','m_CustomRenderQueue: 3000').replace('m_ValidKeywords: []','m_ValidKeywords:\n  - _SURFACE_TYPE_TRANSPARENT').replace('- _Surface: 0','- _Surface: 1\n    - _Blend: 0\n    - _SrcBlend: 5\n    - _DstBlend: 10\n    - _ZWrite: 0').replace('{r: 0.28, g: 0.36, b: 0.32, a: 1}','{r: 0.30, g: 0.65, b: 0.8, a: 0.75}'))
for root in ['Assets/Art/Hands','Assets/Prefabs/Hands','Assets/Scripts/LK.Gameplay/Interaction']:
 r=ROOT/root;meta(r)
 for p in sorted(r.rglob('*')):meta(p)
print('Serialized two 26-joint hand prefabs, shared material, settings and Unity metadata.')
