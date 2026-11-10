"""Validate the committed runtime skin. Run with Python, or Blender for posed-skin comparison.

python3 Tools/Avatars/validate_fisherman_skin.py
blender --background artifacts/Avatars/ReferenceFisherman/Fisherman_Rigged.blend --threads 4 --python-exit-code 1 --python Tools/Avatars/validate_fisherman_skin.py
"""
import json
from pathlib import Path
import numpy as np

ROOT = Path(__file__).resolve().parents[2]
DATA = ROOT / 'Assets/Art/Avatars/ReferenceFisherman/FishermanRig.bytes'


def vector(v):
    return np.array([v[k] for k in ('x', 'y', 'z')])


def rotation(q):
    x, y, z, w = [q[k] for k in ('x', 'y', 'z', 'w')]
    return np.array(((1-2*(y*y+z*z), 2*(x*y-z*w), 2*(x*z+y*w)),
                     (2*(x*y+z*w), 1-2*(x*x+z*z), 2*(y*z-x*w)),
                     (2*(x*z-y*w), 2*(y*z+x*w), 1-2*(x*x+y*y))))


def validate():
    data = json.loads(DATA.read_text())
    assert data['version'] == 1
    joints = data['joints']
    assert len(joints) == 51 and len({j['name'] for j in joints}) == 51
    vertices = np.array(data['vertices']).reshape((-1, 3))
    normals = np.array(data['normals']).reshape((-1, 3))
    tangents = np.array(data['tangents']).reshape((-1, 4))
    uv = np.array(data['uv']).reshape((-1, 2))
    ids = np.array(data['boneIndices']).reshape((-1, 4))
    weights = np.array(data['boneWeights']).reshape((-1, 4))
    tris = np.array(data['triangles']).reshape((-1, 3))
    count = len(vertices)
    assert all(len(a) == count for a in (normals, tangents, uv, ids, weights))
    assert all(np.isfinite(a).all() for a in (vertices, normals, tangents, uv, weights))
    assert np.max(np.abs(np.linalg.norm(normals, axis=1)-1)) < 1e-5
    assert np.max(np.abs(np.linalg.norm(tangents[:, :3], axis=1)-1)) < 1e-5
    assert np.max(np.abs(np.sum(normals*tangents[:, :3], axis=1))) < 1e-5
    assert set(tangents[:, 3]).issubset({-1, 1})
    assert tris.min() >= 0 and tris.max() < count
    assert ids.min() >= 0 and ids.max() < len(joints)
    assert weights.min() >= 0 and np.max(np.abs(weights.sum(axis=1)-1)) < 1e-6
    triangle_points = vertices[tris]
    area = np.linalg.norm(np.cross(triangle_points[:, 1]-triangle_points[:, 0], triangle_points[:, 2]-triangle_points[:, 0]), axis=1)
    assert area.min() > 1e-12
    binds = []
    for i, joint in enumerate(joints):
        assert -1 <= joint['parent'] < i
        r = rotation(joint['rotation'])
        assert abs(np.linalg.det(r)-1) < 1e-5
        matrix = np.eye(4);matrix[:3, :3] = r;matrix[:3, 3] = vector(joint['position'])
        binds.append(matrix)
    binds = np.array(binds)
    assert abs(vertices[:, 1].min()-data['floorHeight']) < 1e-6
    print(f'Runtime skin: {count} vertices, {len(tris)} triangles, {len(joints)} bones; finite attributes, valid weights/binds/indices/UVs/tangent basis.')
    try:
        import bpy
    except ImportError:
        return
    # Compare the exact uploaded vertex data against Blender's CPU-deformed mesh.
    # This verifies basis conversion, parent joints, weights and all UV-seam duplicates.
    rig = bpy.data.objects['Fisherman_Rig'];obj = bpy.data.objects['Fisherman_SkinnedMesh']
    c = np.array(((1, 0, 0, 0), (0, 0, 1, 0), (0, -1, 0, 0), (0, 0, 0, 1)))
    originals = np.array([c[:3, :3] @ np.array(v.co) for v in obj.data.vertices])
    # Source duplicates at the same position are harmless only if their exported weights agree.
    source = {}
    for i, p in enumerate(originals):
        source.setdefault(tuple(np.round(p, 6)), []).append(i)
    original_indices = []
    for p in vertices:
        candidates = source.get(tuple(np.round(p, 6)))
        if not candidates:
            d = np.linalg.norm(originals-p, axis=1);candidates = [int(d.argmin())]
            assert d[candidates[0]] < 1e-6
        original_indices.append(candidates[0])
    def compare(label):
        bpy.context.view_layer.update()
        evaluated = obj.evaluated_get(bpy.context.evaluated_depsgraph_get()).to_mesh()
        expected = np.array([c[:3, :3] @ np.array(v.co) for v in evaluated.vertices])[original_indices]
        matrices = np.array([c @ np.array(rig.pose.bones[j['name']].matrix) @ np.linalg.inv(binds[i]) for i, j in enumerate(joints)])
        points = np.column_stack((vertices, np.ones(count)))
        skinned = np.zeros((count, 4))
        for k in range(4):
            skinned += np.einsum('nij,nj->ni', matrices[ids[:, k]], points)*weights[:, k, None]
        error = np.linalg.norm(skinned[:, :3]-expected, axis=1)
        assert error.max() < 1e-5, (label, error.max())
        obj.evaluated_get(bpy.context.evaluated_depsgraph_get()).to_mesh_clear()
        print(f'{label}: runtime bind/weight deformation matches Blender; maximum error {error.max():.8f} m.')
    compare('Rest')
    for name, angle in [('LeftLowerArm', .65), ('RightUpperLeg', -.4), ('Head', .3), ('LeftIndexProximal', .8)]:
        bone = rig.pose.bones[name];bone.rotation_mode = 'XYZ';bone.rotation_euler.x = angle
    compare('Arm, leg, head and finger pose')
    for bone in rig.pose.bones:
        bone.rotation_mode = 'XYZ';bone.rotation_euler = (0, 0, 0)


if __name__ == '__main__':
    validate()
