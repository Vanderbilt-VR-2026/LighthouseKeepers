"""Read-only checks for the serialized native mesh data and model bindings."""
from pathlib import Path
import math
import re
import struct

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / "Assets/Art/Avatars/BasicKeepers"


def guid(path):
    return re.search(r"^guid: ([a-f0-9]{32})$", Path(str(path)+".meta").read_text(), re.M).group(1)


def validate_mesh(path):
    text = path.read_text()
    count = int(re.search(r"m_VertexCount: (\d+)", text).group(1))
    raw = bytes.fromhex(re.search(r"_typelessdata: ([a-f0-9]+)", text).group(1))
    assert len(raw) == count*24 == int(re.search(r"m_DataSize: (\d+)", text).group(1)), path
    vertices = list(struct.iter_unpack("<6f", raw))
    indices_raw = bytes.fromhex(re.search(r"m_IndexBuffer: ([a-f0-9]+)", text).group(1))
    indices = [i[0] for i in struct.iter_unpack("<H", indices_raw)]
    assert len(indices)%3 == 0 and 0 < len(indices)//3 < 10000, path
    for vertex in vertices:
        assert all(math.isfinite(v) for v in vertex), path
        assert .99 < sum(n*n for n in vertex[3:]) < 1.01, path
    covered = 0
    submeshes = text.split("  m_SubMeshes:",1)[1].split("  m_Shapes:",1)[0]
    ranges = re.findall(r"firstByte: (\d+)\n    indexCount: (\d+)\n    topology: 0\n    baseVertex: 0\n    firstVertex: (\d+)\n    vertexCount: (\d+)", submeshes)
    assert ranges, path
    for start, length, first, amount in ranges:
        start, length, first, amount = map(int, (start,length,first,amount))
        assert start == covered*2 and length%3 == 0, path
        group = indices[start//2:start//2+length]
        assert len(group) == length and all(first <= i < first+amount <= count for i in group), path
        covered += length
    assert covered == len(indices), path
    for i in range(0,len(indices),3):
        a,b,c = [vertices[n] for n in indices[i:i+3]]
        ab = [b[j]-a[j] for j in range(3)];ac = [c[j]-a[j] for j in range(3)]
        cross = (ab[1]*ac[2]-ab[2]*ac[1],ab[2]*ac[0]-ab[0]*ac[2],ab[0]*ac[1]-ab[1]*ac[0])
        assert sum(v*v for v in cross)>1e-18, (path,'degenerate triangle',i//3)
        normal = [(a[j+3]+b[j+3]+c[j+3])/3 for j in range(3)]
        assert sum(cross[j]*normal[j] for j in range(3))>0, (path,'reversed normals',i//3)
    bounds = text.split("  m_LocalAABB:",1)[1].split("  m_MeshUsageFlags:",1)[0]
    centre,extent = [list(map(float,re.findall(r"[xyz]: ([-+.0-9e]+)",line))) for line in bounds.strip().splitlines()]
    for vertex in vertices:
        assert all(abs(vertex[j]-centre[j]) <= extent[j]+1e-6 for j in range(3)), path
    return len(indices)//3, len(ranges)


if __name__ == "__main__":
    meshes = {path:validate_mesh(path) for path in sorted(ART.glob("*.asset"))}
    assert len(meshes) == 4
    for path,stats in meshes.items():
        print(f"PASS {path.stem}: {stats[0]} triangles; {stats[1]} submeshes; finite normals, winding, bounds and index ranges")
    network = (ROOT/"Assets/Prefabs/Network/Resources/PF_PlayerRig.prefab").read_text()
    scripts = guid(ROOT/"Assets/Scripts/LK.Net/CrewAvatarModel.cs")
    material_guids = {guid(p) for p in ART.glob('*.mat')}
    for name,field in [('PF_BasicCaptain','captainModel'),('PF_BasicFisherman','fishermanModel')]:
        path = ROOT/'Assets/Prefabs/Avatars'/(name+'.prefab')
        text = path.read_text()
        assert f'{field}: {{fileID: 1000, guid: {guid(path)}, type: 3}}' in network, field
        assert f'guid: {scripts}' in text and 'head: {fileID: 4001}' in text and 'body: {fileID: 4002}' in text, path
        assert text.count('\nMeshRenderer:') == 2 and text.count('\nMeshFilter:') == 2, path
        assert text.count('m_Father: {fileID: 4000}') == 2, path
        assert text.count('m_Materials:') == 2 and 'Collider:' not in text and 'Camera:' not in text, path
        slots = text.split('  m_Materials:')[1:]
        for slot,part in zip(slots,['Head','Body']):
            actual = re.findall(r'guid: ([a-f0-9]{32})',slot.split('  m_StaticBatchInfo:',1)[0])
            mesh = ART/(name+'_'+part+'.asset')
            assert len(actual) == meshes[mesh][1] and set(actual) <= material_guids, path
            assert f'guid: {guid(mesh)}' in text, path
    print('PASS: both models are assigned; anchor, mesh, material and script references resolve. Unity import/rendering remains a separate check.')
