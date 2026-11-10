"""Check the reference-inspired avatar's serialized meshes and portable rig export."""
from pathlib import Path
import json
import math
import re
import struct

ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/"Assets/Art/Avatars/ReferenceFisherman"
OUT=ROOT/"artifacts/Avatars/ReferenceFisherman"


def asset_guid(path):
    return re.search(r"^guid: ([a-f0-9]{32})$",Path(str(path)+".meta").read_text(),re.M).group(1)


def mesh_check(path):
    text=path.read_text()
    count=int(re.search(r"m_VertexCount: (\d+)",text).group(1))
    raw=bytes.fromhex(re.search(r"_typelessdata: ([a-f0-9]+)",text).group(1))
    assert len(raw)==count*48==int(re.search(r"m_DataSize: (\d+)",text).group(1)),path
    vertices=list(struct.iter_unpack("<12f",raw))
    for v in vertices:
        assert all(math.isfinite(x) for x in v),path
        assert .99<sum(x*x for x in v[3:6])<1.01,path
        assert .99<sum(x*x for x in v[6:9])<1.01,path
        assert abs(sum(a*b for a,b in zip(v[3:6],v[6:9])))<.01,path
        assert abs(abs(v[9])-1)<.001,path
        assert all(-.0001<=x<=1.0001 for x in v[10:]),path
    indices=[n[0] for n in struct.iter_unpack('<H',bytes.fromhex(re.search(r'm_IndexBuffer: ([a-f0-9]+)',text).group(1)))]
    assert len(indices)%3==0 and max(indices)<count,path
    bad=0
    for i in range(0,len(indices),3):
        a,b,c=[vertices[n] for n in indices[i:i+3]]
        p=[b[j]-a[j] for j in range(3)];q=[c[j]-a[j] for j in range(3)]
        cross=(p[1]*q[2]-p[2]*q[1],p[2]*q[0]-p[0]*q[2],p[0]*q[1]-p[1]*q[0])
        if sum(x*x for x in cross)<1e-18:bad+=1
    assert bad==0,(path,'degenerate triangles',bad)
    sub=text.split('  m_SubMeshes:',1)[1].split('  m_Shapes:',1)[0]
    assert len(re.findall(r'firstByte:',sub))==1,path
    assert int(re.search(r'indexCount: (\d+)',sub).group(1))==len(indices),path
    print(f'PASS {path.name}: {len(indices)//3} triangles, UVs, tangents, normals and indices')
    return len(indices)//3


def glb_check(path):
    data=path.read_bytes();magic,version,total=struct.unpack_from('<4sII',data)
    assert magic==b'glTF' and version==2 and total==len(data)
    length,kind=struct.unpack_from('<I4s',data,12);assert kind==b'JSON'
    doc=json.loads(data[20:20+length]);offset=20+length
    binlength,kind=struct.unpack_from('<I4s',data,offset);assert kind==b'BIN\0'
    binary=data[offset+8:offset+8+binlength]
    assert len(doc['skins'])==1 and len(doc['skins'][0]['joints'])>=51
    assert all('bufferView' in image for image in doc['images']),'Portable textures are not embedded'
    joint_names={doc['nodes'][i]['name'] for i in doc['skins'][0]['joints']}
    assert {'Hips','Head','LeftHand','RightHand','LeftFoot','RightFoot'}<=joint_names
    weighted=0
    for mesh in doc['meshes']:
        for primitive in mesh['primitives']:
            attr=primitive['attributes'];assert 'JOINTS_0' in attr and 'WEIGHTS_0' in attr
            accessor=doc['accessors'][attr['WEIGHTS_0']]
            view=doc['bufferViews'][accessor['bufferView']]
            code,size,maximum={5126:('f',4,1),5123:('H',2,65535),5121:('B',1,255)}[accessor['componentType']]
            stride=view.get('byteStride',size*4)
            base=view.get('byteOffset',0)+accessor.get('byteOffset',0)
            for i in range(accessor['count']):
                weights=struct.unpack_from('<4'+code,binary,base+i*stride)
                assert abs(sum(weights)/maximum-1)<.002,'Unnormalized exported skin weights'
            weighted+=accessor['count']
    print('PASS GLB: embedded textures;',len(joint_names),'joints;',weighted,'normalized weighted vertices')


if __name__=='__main__':
    counts=[mesh_check(ART/name) for name in ['Fisherman_Head.asset','Fisherman_Coat.asset']]
    prefab=(ROOT/'Assets/Prefabs/Avatars/PF_ReferenceFisherman.prefab').read_text()
    material=(ART/'FishermanSurface.mat').read_text()
    for name in ('Fisherman_Head.asset','Fisherman_Coat.asset'):
        assert asset_guid(ART/name) in prefab
    assert asset_guid(ART/'FishermanSurface.mat') in prefab
    for kind in ('Albedo','Normal','Smoothness'):
        assert asset_guid(ART/('Fisherman_'+kind+'.png')) in material
    assert '_NORMALMAP' in material and '_METALLICSPECGLOSSMAP' in material
    assert 'textureType: 1' in Path(str(ART/'Fisherman_Normal.png')+'.meta').read_text()
    assert 'sRGBTexture: 0' in Path(str(ART/'Fisherman_Smoothness.png')+'.meta').read_text()
    print('PASS modular prefab/material/texture references;',sum(counts),'triangles total')
    if (OUT/'Fisherman_Rigged.glb').exists():
        glb_check(OUT/'Fisherman_Rigged.glb')
        assert (OUT/'Fisherman_Rigged.fbx').read_bytes().startswith(b'Kaydara FBX Binary')
        assert (OUT/'Fisherman_Rigged.blend').read_bytes().startswith(b'BLENDER')
        facts=json.loads((OUT/'model-checks.json').read_text())
        assert facts['arm_pose_vertices_moved']>100 and facts['bones']>=51
        print('PASS full-body FBX/Blend exports and recorded deformation check')
    else:print('Portable exports not present; regenerate to check the full-body rig.')
