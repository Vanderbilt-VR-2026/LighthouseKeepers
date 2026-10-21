"""Blender 4.5 headless: author skinned gloves and bake portable pose/mesh payloads.
Run from project root: blender -b -t 1 --python Assets/Art/Hands/author_hands.py
No third party assets or packages. Unity uses the JSON to avoid importer bone-ID coupling.
"""
import bpy, json, math
from pathlib import Path
from mathutils import Vector, Euler
ROOT = Path.cwd() / 'Assets/Art/Hands'
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)

def sphere(name, center, scale):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=12, ring_count=8, location=center)
    o=bpy.context.object; o.name=name; o.scale=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return o

def segment(name,a,b,r):
    a,b=Vector(a),Vector(b)
    o=sphere(name,(a+b)/2,(r,r,(b-a).length/2+r*.55))
    o.rotation_mode='QUATERNION';o.rotation_quaternion=(b-a).to_track_quat('Z','Y')
    return o

for side,mirror in [('L',1),('R',-1)]:
    bones=[dict(name='wrist',parent=-1,world=[0,-.018,-.035]),dict(name='palm',parent=0,world=[0,-.018,.015])]
    chains=[]
    thumb=[(.034,-.018,.005),(.052,-.021,.026),(.067,-.021,.044),(.080,-.021,.059)]
    chains.append(('thumb',thumb))
    for name,x,z,length in [('index',.028,.049,.070),('middle',.008,.053,.079),('ring',-.013,.049,.073),('little',-.032,.039,.057)]:
        chains.append((name,[(x,-.018,.012),(x,-.018,z),(x,-.018,z+length*.42),(x,-.018,z+length*.76),(x,-.018,z+length)]))
    pieces=[sphere('Glove palm',(0,-.018,.017),(.045,.020,.054)),sphere('Cuff',(0,-.018,-.032),(.037,.022,.018))]
    finger_segments=[]
    for name,pts in chains:
        parent=1
        for j,p in enumerate(pts):
            p=(p[0]*mirror,p[1],p[2]);idx=len(bones)
            bones.append(dict(name=f'{name}_{j}',parent=parent,world=list(p)));parent=idx
            if j<len(pts)-1:
                q=pts[j+1];q=(q[0]*mirror,q[1],q[2]);r=(.010 if name=='thumb' else .009 if name!='little' else .0075)*(1-j*.09)
                pieces.append(segment(f'{name} skin {j}',p,q,r));finger_segments.append((idx,Vector(p),Vector(q)))
    bpy.ops.object.select_all(action='DESELECT')
    for p in pieces:p.select_set(True)
    bpy.context.view_layer.objects.active=pieces[0];bpy.ops.object.join();skin=bpy.context.object;skin.name='Glove_'+side
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    remesh=skin.modifiers.new('Continuous sewn glove','REMESH');remesh.mode='VOXEL';remesh.voxel_size=.0035
    bpy.ops.object.modifier_apply(modifier=remesh.name)
    smooth=skin.modifiers.new('Soft fabric','SMOOTH');smooth.factor=.9;smooth.iterations=4;bpy.ops.object.modifier_apply(modifier=smooth.name)
    dec=skin.modifiers.new('Quest topology','DECIMATE');dec.ratio=.45;bpy.ops.object.modifier_apply(modifier=dec.name)
    tri=skin.modifiers.new('Triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=tri.name)
    for poly in skin.data.polygons:poly.use_smooth=True
    arm=bpy.data.armatures.new('OpenXR_26_'+side);rig=bpy.data.objects.new('Skeleton_'+side,arm);bpy.context.collection.objects.link(rig)
    bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT')
    for i,b in enumerate(bones):
        eb=arm.edit_bones.new(b['name']);eb.head=b['world'];eb.tail=Vector(b['world'])+Vector((0,0,.009))
        if b['parent']>=0:eb.parent=arm.edit_bones[bones[b['parent']]['name']]
    bpy.ops.object.mode_set(mode='OBJECT')
    groups=[skin.vertex_groups.new(name=b['name']) for b in bones];indices=[];weights=[]
    for v in skin.data.vertices:
        candidates=[]
        for idx,a,b in finger_segments:
            d=b-a;t=max(0,min(1,(v.co-a).dot(d)/d.length_squared));distance=(v.co-(a+d*t)).length
            candidates.append((distance,idx,t))
        distance,idx,t=min(candidates)
        if distance>.018 or (v.co.z<.036 and abs(v.co.x)<.032):i,j,w=1,0,.85
        else:i,j,w=idx,min(idx+1,len(bones)-1),1-max(0,(t-.35)/.65)*.8
        groups[i].add([v.index],w,'REPLACE');groups[j].add([v.index],1-w,'REPLACE')
        indices.extend([i,j]);weights.append(round(w,6))
    mod=skin.modifiers.new('26 joint skin','ARMATURE');mod.object=rig;skin.parent=rig
    vertices=[dict(x=v.co.x,y=v.co.y,z=v.co.z) for v in skin.data.vertices]
    normals=[dict(x=v.normal.x,y=v.normal.y,z=v.normal.z) for v in skin.data.vertices]
    for b in bones:
        p=Vector(bones[b['parent']]['world']) if b['parent']>=0 else Vector((0,0,0));d=Vector(b['world'])-p
        b['position']=dict(zip(('x','y','z'),d));del b['world']
    poses=[]
    for pose in ['Open','Fist','WrapGrip','Pinch']:
        rotations=[]
        for b in bones:
            name=b['name'];e=(0,0,0)
            if '_' in name:
                finger,j=name.split('_');j=int(j)
                if finger=='thumb':
                    if pose=='Fist':e=(20, -35*mirror, -25*mirror) if j==0 else (35,0,0)
                    if pose=='WrapGrip':e=(10,-24*mirror,-15*mirror) if j==0 else (25,0,0)
                    if pose=='Pinch':e=(5,-32*mirror,-12*mirror) if j==0 else (25,0,0)
                elif j in (1,2,3):
                    if pose=='Fist':e=([0,75,95,70][j],0,0)
                    if pose=='WrapGrip':e=([0,50,65,45][j],0,0)
                    if pose=='Pinch':e=(([0,48,68,48][j]) if finger=='index' else 12,0,0)
            q=Euler(tuple(math.radians(v) for v in e),'XYZ').to_quaternion()
            rotations.append(dict(x=q.x,y=q.y,z=q.z,w=q.w))
        poses.append(dict(name=pose,rotations=rotations))
    payload=dict(bones=bones,vertices=vertices,normals=normals,triangles=[i for p in skin.data.polygons for i in p.vertices],boneIndices=indices,boneWeights=weights,poses=poses)
    (ROOT/'Resources/Hands'/f'Glove_{side}.json').write_text(json.dumps(payload,separators=(',',':'))+'\n')
    bpy.ops.object.select_all(action='DESELECT');skin.select_set(True);rig.select_set(True);bpy.context.view_layer.objects.active=rig
    bpy.ops.export_scene.fbx(filepath=str(ROOT/f'Glove_{side}.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')
    print(f'HAND_ASSET {side}: {len(bones)} bones, {len(vertices)} vertices, {len(skin.data.polygons)} triangles, four baked poses')
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
