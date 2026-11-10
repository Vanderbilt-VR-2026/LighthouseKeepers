"""Blender round-trip checks for the generated FBX and GLB; no Unity claims.

blender --background --factory-startup --python-exit-code 1 --python Tools/Avatars/validate_fisherman_rig.py
"""
from pathlib import Path
import json
import math

import bpy

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'artifacts/Avatars/ReferenceFisherman'


def positions(obj):
    mesh=obj.evaluated_get(bpy.context.evaluated_depsgraph_get())
    return [obj.matrix_world@v.co for v in mesh.data.vertices]


def check(path):
    for obj in list(bpy.data.objects):bpy.data.objects.remove(obj,do_unlink=True)
    if path.suffix=='.fbx':bpy.ops.import_scene.fbx(filepath=str(path),use_anim=False)
    else:bpy.ops.import_scene.gltf(filepath=str(path))
    rigs=[o for o in bpy.data.objects if o.type=='ARMATURE']
    assert len(rigs)==1,(path,'armature count',len(rigs))
    rig=rigs[0];names={b.name for b in rig.data.bones}
    assert {'Hips','Head','LeftHand','RightHand','LeftFoot','RightFoot'}<=names
    meshes=[o for o in bpy.data.objects if o.type=='MESH' and any(m.type=='ARMATURE' and m.object==rig for m in o.modifiers)]
    assert len(meshes)==1,(path,'skinned mesh count',len(meshes))
    obj=meshes[0]
    for vertex in obj.data.vertices:
        weights=[g.weight for g in vertex.groups if obj.vertex_groups[g.group].name in names]
        assert weights and abs(sum(weights)-1)<.005,(path,'invalid skin weights',vertex.index)
    points=positions(obj)
    assert all(all(math.isfinite(x) for x in p) for p in points)
    height=max(p.z for p in points)-min(p.z for p in points)
    assert 1.7<height<2.0,(path,'unexpected model scale/axis',height)
    moves={}
    for name in ('LeftLowerArm','LeftUpperLeg','Head'):
        bone=rig.pose.bones[name];bone.rotation_mode='XYZ';original=bone.rotation_euler.copy()
        baseline=positions(obj)
        bone.rotation_euler.x+=.35;bpy.context.view_layer.update()
        changed=positions(obj)
        moved=sum((a-b).length>.001 for a,b in zip(baseline,changed))
        assert moved>100,(path,name,'did not deform the imported mesh',moved)
        moves[name]=moved
        bone.rotation_euler=original;bpy.context.view_layer.update()
    result={'file':path.name,'bones':len(names),'vertices':len(obj.data.vertices),'height_metres':height,'pose_vertices_moved':moves}
    path.with_suffix(path.suffix+'.roundtrip.json').write_text(json.dumps(result,indent=2)+'\n')
    print('PASS ROUNDTRIP',json.dumps(result),flush=True)


for suffix in ('.fbx','.glb'):check(OUT/('Fisherman_Rigged'+suffix))
