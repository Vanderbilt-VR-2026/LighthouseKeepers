"""Export the generated Blender skin for CrewHumanoidAvatar, without an FBX importer dependency.

Run against the source .blend, or call export_skin(obj, rig, path) from the generator.
Coordinates and winding match build_basic_keepers.native_mesh. Bone local +Y stays
its length axis; only the armature's world basis changes from Blender to Unity.
"""
import json
import math
from pathlib import Path
import bpy
from mathutils import Matrix, Quaternion, Vector

ROOT = Path(__file__).resolve().parents[2]
DEST = ROOT / 'Assets/Art/Avatars/ReferenceFisherman/FishermanRig.bytes'
C = Matrix(((1, 0, 0), (0, 0, 1), (0, -1, 0)))


def vec(v):
    return dict(zip(('x', 'y', 'z'), (round(float(x), 7) for x in v)))


def quat(q):
    return dict(zip(('x', 'y', 'z', 'w'), (round(float(x), 8) for x in (q.x, q.y, q.z, q.w))))


def export_skin(obj, rig, path=DEST):
    mesh = obj.data
    mesh.calc_loop_triangles()
    mesh.calc_tangents()
    # Parent-first order lets the runtime evaluate finger/toe FK in one pass.
    bones = []
    def visit(bone):
        bones.append(bone)
        for child in bone.children:
            visit(child)
    for bone in rig.data.bones:
        if bone.parent is None:
            visit(bone)
    indices = {b.name: i for i, b in enumerate(bones)}
    joints = []
    for bone in bones:
        rotation = (C @ bone.matrix_local.to_3x3()).to_quaternion()
        # Fingers bend toward the palm (model -Z in this hanging rest pose).
        axis = bone.vector.normalized().cross(Vector((0, 1, 0)))
        if axis.length < .1:
            axis = Vector((1, 0, 0))
        curl = bone.matrix_local.to_3x3().inverted() @ axis.normalized()
        joints.append(dict(name=bone.name, parent=indices[bone.parent.name] if bone.parent else -1,
                           position=vec(C @ bone.head_local), rotation=quat(rotation), curlAxis=vec(curl)))
    result = dict(version=1, eyeReference=vec(Vector((0, 1.665, 0))), joints=joints,
                  vertices=[], normals=[], tangents=[], uv=[], boneIndices=[], boneWeights=[], triangles=[])
    unique = {}
    for tri in mesh.loop_triangles:
        for loop_id in tri.loops:
            loop = mesh.loops[loop_id]
            vertex = mesh.vertices[loop.vertex_index]
            normal = mesh.corner_normals[loop_id].vector.normalized()
            tangent = loop.tangent.normalized()
            if tangent.length_squared < .5:
                axis = Vector((0, 0, 1)) if abs(normal.z) < .9 else Vector((0, 1, 0))
                tangent = axis.cross(normal).normalized()
            uv = mesh.uv_layers.active.data[loop_id].uv
            groups = sorted(((indices[obj.vertex_groups[g.group].name], g.weight) for g in vertex.groups
                             if obj.vertex_groups[g.group].name in indices and g.weight > 0), key=lambda p: -p[1])
            assert groups and len(groups) <= 4, (vertex.index, groups)
            total = sum(w for _, w in groups)
            weights = [w / total for _, w in groups] + [0] * (4 - len(groups))
            ids = [i for i, _ in groups] + [0] * (4 - len(groups))
            values = (*C @ vertex.co, *C @ normal, *C @ tangent, loop.bitangent_sign, *uv, *ids, *weights)
            key = tuple(round(float(x), 7) for x in values)
            if key not in unique:
                unique[key] = len(unique)
                result['vertices'].extend(key[:3]); result['normals'].extend(key[3:6])
                result['tangents'].extend(key[6:10]); result['uv'].extend(key[10:12])
                result['boneIndices'].extend(ids); result['boneWeights'].extend(key[16:20])
            result['triangles'].append(unique[key])
    result['floorHeight'] = min(result['vertices'][1::3])
    assert len(unique) < 65536 and len(bones) == 51
    path = Path(path)
    path.write_text(json.dumps(result, separators=(',', ':'), allow_nan=False) + '\n')
    print('UNITY SKIN:', len(unique), 'vertices,', len(result['triangles']) // 3, 'triangles,', len(bones), 'bones:', path)
    return result


if __name__ == '__main__':
    export_skin(bpy.data.objects['Fisherman_SkinnedMesh'], bpy.data.objects['Fisherman_Rig'])
