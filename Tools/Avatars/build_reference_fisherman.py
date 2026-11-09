"""Create an original rigged fisherman inspired by the approved yellow-coat reference.

blender --background --factory-startup --threads 4 --python Tools/Avatars/build_reference_fisherman.py
Optional -- --preview renders a full-body turnaround. Exports native modular Unity
art plus portable FBX/GLB/Blend files under artifacts/Avatars/ReferenceFisherman.
"""
from pathlib import Path
import importlib.util
import json
import math
import sys

import bpy
from mathutils import Vector

HERE = Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location("keeper_mesh", HERE/"build_basic_keepers.py")
M = importlib.util.module_from_spec(spec);spec.loader.exec_module(M)
ROOT = M.ROOT
ART = ROOT/"Assets/Art/Avatars/ReferenceFisherman"
OUT = ROOT/"artifacts/Avatars/ReferenceFisherman"
M.ART = ART
OBJECTS = []
EYES = 1.665
BONES = {}


def linear(rgb):
    return tuple(v/12.92 if v <= .04045 else ((v+.055)/1.055)**2.4 for v in rgb)


def surface(name, rgb, rough=.5, variation=.04, grain=.001):
    material = bpy.data.materials.new(name);material.use_nodes=True
    nodes=material.node_tree.nodes;links=material.node_tree.links
    bsdf=nodes.get("Principled BSDF")
    bsdf.inputs["Roughness"].default_value=rough
    noise=nodes.new("ShaderNodeTexNoise");noise.inputs["Scale"].default_value=65
    noise.inputs["Detail"].default_value=3;noise.inputs["Roughness"].default_value=.72
    coords=nodes.new("ShaderNodeTexCoord");links.new(coords.outputs["Object"],noise.inputs["Vector"])
    ramp=nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].position=.22;ramp.color_ramp.elements[1].position=.78
    for element,factor in zip(ramp.color_ramp.elements,(1-variation,1+variation)):
        element.color=(*linear(tuple(min(1,v*factor) for v in rgb)),1)
    links.new(noise.outputs["Fac"],ramp.inputs[0]);links.new(ramp.outputs["Color"],bsdf.inputs["Base Color"])
    bump=nodes.new("ShaderNodeBump");bump.inputs["Strength"].default_value=.25
    bump.inputs["Distance"].default_value=grain
    links.new(noise.outputs["Fac"],bump.inputs["Height"]);links.new(bump.outputs["Normal"],bsdf.inputs["Normal"])
    return material


def body_weights(position):
    y=position[1]
    if y<=1.08:return {"Hips":1}
    if y<=1.24:
        t=(y-1.08)/.16;return {"Hips":1-t,"Spine":t}
    t=min(1,(y-1.24)/.13);return {"Spine":1-t,"Chest":t}


def keep(obj, region, weights):
    obj.vertex_groups.new(name="VIS_"+region).add(list(range(len(obj.data.vertices))),1,"REPLACE")
    bpy.context.view_layer.update()
    groups={}
    for vertex in obj.data.vertices:
        point=M.unity(obj.matrix_world @ vertex.co)
        values=weights(point) if callable(weights) else weights
        for name,weight in values.items():
            if weight<=0:continue
            if name not in groups:groups[name]=obj.vertex_groups.get(name) or obj.vertex_groups.new(name=name)
            groups[name].add([vertex.index],weight,"REPLACE")
    OBJECTS.append(obj)
    return obj


def ellipsoid(name, centre, radii, colour, region, weights, segments=20, rows=12):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rows,location=M.u(centre))
    obj=bpy.context.object;obj.scale=(radii[0],radii[2],radii[1])
    return keep(M.finish(obj,name,colour,True),region,weights)


def custom(name, vertices, faces, colour, region, weights, smooth=True):
    return keep(M.mesh(name,vertices,faces,colour,smooth),region,weights)


def line(name, points, radius, colour, region, weights, sides=6):
    vertices=[];faces=[]
    for i,point in enumerate(points):
        tangent=Vector(points[min(len(points)-1,i+1)])-Vector(points[max(0,i-1)])
        tangent.normalize();helper=Vector((0,0,1)) if abs(tangent.z)<.9 else Vector((0,1,0))
        x=tangent.cross(helper).normalized();z=tangent.cross(x).normalized()
        for k in range(sides):
            a=2*math.pi*k/sides
            vertices.append(tuple(Vector(point)+radius*(x*math.cos(a)+z*math.sin(a))))
    for row in range(len(points)-1):
        for k in range(sides):
            a=row*sides+k;b=row*sides+(k+1)%sides
            faces.append((a,b,b+sides,a+sides))
    faces += [tuple(reversed(range(sides))),tuple((len(points)-1)*sides+k for k in range(sides))]
    return custom(name,vertices,faces,colour,region,weights)


def face():
    # Anatomical head subset from MakeHuman's explicitly CC0 base mesh; provenance
    # and the full asset license are bundled in Tools/Avatars/Sources.
    path=HERE/"Sources/MakeHumanHead.obj"
    vertices=[];faces=[]
    for record in path.read_text().splitlines():
        if record.startswith("v "):
            x,y,z=map(float,record.split()[1:4])
            vertices.append((x*.10,EYES+(y-7.28415)*.10,.0645+(z-1.24535)*.10))
        elif record.startswith("f "):
            faces.append(tuple(int(i)-1 for i in record.split()[1:]))
    head=custom("Anatomical fisherman head",vertices,faces,"Skin","Head",{"Head":1})
    bpy.context.view_layer.objects.active=head
    decimate=head.modifiers.new("Game mesh head reduction","DECIMATE");decimate.ratio=.45
    bpy.ops.object.modifier_apply(modifier=decimate.name)
    # Reassign the neck portion after reduction; no semantic groups depend on
    # MakeHuman's helper geometry, which is not included in this subset.
    head.vertex_groups.clear();keep_weights={}
    for vertex in head.data.vertices:
        keep_weights[vertex.index]={"Neck":1} if M.unity(vertex.co)[1]<EYES-.11 else {"Head":1}
    for name in ("VIS_Head","Neck","Head"):head.vertex_groups.new(name=name)
    head.vertex_groups["VIS_Head"].add(list(range(len(head.data.vertices))),1,"REPLACE")
    for index,weights in keep_weights.items():
        for name,weight in weights.items():head.vertex_groups[name].add([index],weight,"REPLACE")
    points=[M.unity(v.co) for v in head.data.vertices]
    def skin_z(x,y):
        nearby=sorted(( ((px-x)**2+(py-y)**2,pz) for px,py,pz in points if pz>.015),key=lambda q:q[0])[:4]
        total=sum(1/(d+1e-7) for d,z in nearby)
        return sum(z/(d+1e-7) for d,z in nearby)/total
    for side in (-1,1):
        x=side*.030775
        ellipsoid("Eye white",(x,EYES,.064535),(.0138,.0138,.0145),"EyeWhite","Head",{"Head":1},20,12)
        ellipsoid("Iris",(x,EYES,.0781),(.0058,.0062,.0015),"Iris","Head",{"Head":1},16,10)
        ellipsoid("Pupil",(x,EYES,.0795),(.0025,.0033,.0008),"Ink","Head",{"Head":1},12,8)
        brow=[]
        for i in range(9):
            xx=x+side*(-.017+.034*i/8);yy=EYES+.026+.004*math.sin(math.pi*i/8)
            brow.append((xx,yy,skin_z(xx,yy)+.0015))
        line("Fitted eyebrow",brow,.003,"Hair","Head",{"Head":1})
        moustache=[]
        for i in range(9):
            xx=side*(.003+.024*i/8);yy=EYES-.030-.004*i/8
            moustache.append((xx,yy,skin_z(xx,yy)+.002))
        line("Moustache",moustache,.003,"Hair","Head",{"Head":1})
    beard_faces=[]
    for polygon in head.data.polygons:
        p=M.unity(polygon.center)
        top=EYES-.045+.015*min(1,abs(p[0])/.07)
        if EYES-.14<p[1]<top and p[2]>.005 and abs(p[0])<.085:
            beard_faces.append(tuple(polygon.vertices))
    used=sorted({i for f in beard_faces for i in f});mapping={old:i for i,old in enumerate(used)}
    hair_vertices=[M.unity(head.data.vertices[i].co+head.data.vertices[i].normal*.0018) for i in used]
    custom("Fitted beard",hair_vertices,[tuple(mapping[i] for i in f) for f in beard_faces],"Hair","Head",{"Head":1})


def hood():
    n,rows,opening=36,18,.84
    outer=[];faces=[]
    for row in range(rows):
        t=opening+(math.pi-opening-.10)*row/(rows-1)
        for k in range(n):
            a=2*math.pi*k/n
            fold=.0018*math.sin(9*a+3*t)*math.sin(t)**2
            outer.append(((.132+fold)*math.sin(t)*math.cos(a),EYES-.008+(.181+fold)*math.sin(t)*math.sin(a),-.039+(.15+fold)*math.cos(t)))
    for row in range(rows-1):
        for k in range(n):
            a=row*n+k;b=row*n+(k+1)%n;faces.append((a,a+n,b+n,b))
    pole=len(outer);outer.append((0,EYES-.008,-.189))
    for k in range(n):faces.append(((rows-1)*n+k,pole,(rows-1)*n+(k+1)%n))
    custom("Raised yellow hood",outer,faces,"Oilskin","Head",{"Head":1})
    inner=[(x*.95,EYES-.008+(y-EYES+.008)*.96,-.039+(z+.039)*.95) for x,y,z in outer]
    custom("Hood lining",inner,[tuple(reversed(f)) for f in faces],"Lining","Head",{"Head":1})
    lip=outer[:n]+inner[:n]
    custom("Bound hood edge",lip,[(k,(k+1)%n,(k+1)%n+n,k+n) for k in range(n)],"Seams","Head",{"Head":1})
    # Ribbed roll-neck below the face, as in the reference.
    obj=M.rings("Ribbed wool collar",[(EYES-.22,.055,.05),(EYES-.13,.055,.05)],"Knit",24)
    obj.location=M.u((0,0,-.03))
    keep(obj,"Head",{"Neck":1})


LEVELS=[(.69,.219,.12),(.84,.222,.126),(1.02,.187,.12),(1.18,.18,.114),(1.35,.216,.111),(1.43,.18,.095),(1.49,.065,.058)]


def profile(y):
    for a,b in zip(LEVELS,LEVELS[1:]):
        if y<=b[0]:
            t=max(0,(y-a[0])/(b[0]-a[0]));return (a[1]*(1-t)+b[1]*t,a[2]*(1-t)+b[2]*t)
    return LEVELS[-1][1:]


def front(x,y,offset=0):
    rx,rz=profile(y)
    return rz*math.sqrt(max(.08,1-(x/rx)**2))+offset


def patch(name,x,y,width,height,colour,offset=.004,back=False):
    vertices=[];faces=[];n=5
    for row in range(n+1):
        yy=y-height/2+height*row/n
        for k in range(n+1):
            xx=x-width/2+width*k/n
            bulge=.004*math.sin(math.pi*k/n)*math.sin(math.pi*row/n)
            vertices.append((xx,yy,front(xx,yy,offset+bulge)*(-1 if back else 1)))
    for row in range(n):
        for k in range(n):
            a=row*(n+1)+k;face=(a,a+1,a+n+2,a+n+1)
            faces.append(tuple(reversed(face)) if back else face)
    return custom(name,vertices,faces,colour,"Body",body_weights)


def coat():
    n,rows=40,44;vertices=[];faces=[]
    for row in range(rows):
        y=.69+.8*row/(rows-1);rx,rz=profile(y)
        for k in range(n):
            a=2*math.pi*k/n
            ripple=.0025*math.sin(24*y+5*a)+.0015*math.sin(47*y-3*a)
            ripple*=math.sin(math.pi*row/(rows-1))**2
            vertices.append(((rx+ripple)*math.cos(a),y,(rz+ripple*.7)*math.sin(a)))
    for row in range(rows-1):
        for k in range(n):
            a=row*n+k;b=row*n+(k+1)%n;faces.append((a,a+n,b+n,b))
    custom("Oilskin coat body",vertices,faces,"Oilskin","Body",body_weights)
    patch("Front rain flap",0,1.085,.036,.77,"Trim",.009)
    patch("Shoulder storm yoke",0,1.345,.34,.09,"Oilskin",.008)
    patch("Rear storm cape",0,1.35,.36,.105,"Oilskin",.008,True)
    for side in (-1,1):
        x=side*.115
        patch("Large patch pocket",x,.87,.143,.165,"Oilskin",.009)
        patch("Pocket flap",x,.967,.153,.04,"Trim",.015)
        outline=[(x+dx,.79+dy,front(x+dx,.79+dy,.013)) for dx,dy in [(-.065,.15),(-.065,0),(.065,0),(.065,.15)]]
        line("Pocket seam",outline,.001,"Seams","Body",body_weights)
    for y in (.77,.90,1.03,1.16,1.29,1.42):
        ellipsoid("Metal snap",(0,y,front(0,y,.017)),(.0075,.0075,.003),"Metal","Body",body_weights,12,8)
    for side in (-1,1):
        line("Rain flap stitching",[(side*.013,y,front(side*.013,y,.012)) for y in [.72+i*.025 for i in range(30)]],.0008,"Seams","Body",body_weights)
    # Raised open-front collar with thickness, rather than floating front patches.
    vertices=[];faces=[];n=32
    for y,rx,rz in [(1.445,.079,.069),(1.485,.071,.063),(1.522,.062,.056)]:
        for k in range(n):
            a=math.pi/2+.18+(2*math.pi-.36)*k/(n-1)
            vertices.append((rx*math.cos(a),y,-.025+rz*math.sin(a)))
    outer_count=len(vertices)
    vertices += [(x*.94,y,z*.94-.0015) for x,y,z in vertices]
    for row in range(2):
        for k in range(n-1):
            a=row*n+k;f=(a,a+n,a+n+1,a+1)
            faces.append(f);faces.append(tuple(outer_count+i for i in reversed(f)))
    for k in range(n-1):
        a=2*n+k;faces.append((a,a+1,a+1+outer_count,a+outer_count))
    for k in (0,n-1):
        for row in range(2):
            a=row*n+k;faces.append((a,a+outer_count,a+n+outer_count,a+n))
    custom("Raised oilskin collar",vertices,faces,"Oilskin","Body",{"Chest":1})


def tube(name, centres, radii, colour, region, weights, sides=16, folds=0):
    vertices=[];faces=[]
    for row,((x,y,z),(rx,rz)) in enumerate(zip(centres,radii)):
        p=Vector((x,y,z));axis=Vector(centres[min(len(centres)-1,row+1)])-Vector(centres[max(0,row-1)])
        axis.normalize();width=Vector((0,0,1)).cross(axis).normalized();depth=axis.cross(width).normalized()
        for k in range(sides):
            a=2*math.pi*k/sides;r=1+folds*(math.sin(16*row/len(centres)+3*a)+.4*math.sin(37*row/len(centres)-2*a))
            vertices.append(tuple(p+width*(rx*math.cos(a)*r)+depth*(rz*math.sin(a)*r)))
    for row in range(len(centres)-1):
        for k in range(sides):
            a=row*sides+k;b=row*sides+(k+1)%sides;faces.append((a,b,b+sides,a+sides))
    faces += [tuple(reversed(range(sides))),tuple((len(centres)-1)*sides+k for k in range(sides))]
    return custom(name,vertices,faces,colour,region,weights)


def arms_and_hands():
    for side,label in [(-1,"Left"),(1,"Right")]:
        shoulder=Vector((side*.195,1.405,0));elbow=Vector((side*.285,1.18,.005));wrist=Vector((side*.385,.967,.018))
        centres=[];radii=[]
        for row in range(23):
            t=row/22
            p=shoulder.lerp(elbow,t*2) if t<=.5 else elbow.lerp(wrist,(t-.5)*2)
            centres.append(tuple(p));r=.066*(1-t)+.046*t
            radii.append((r,r*.92))
        def sleeve_weight(p,s=shoulder,e=elbow,w=wrist,l=label):
            t=max(0,min(1,(s.y-p[1])/(s.y-w.y)))
            a=max(0,min(1,(t-.37)/.26))
            return {l+"UpperArm":1-a,l+"LowerArm":a}
        tube(label+" rain sleeve",centres,radii,"Oilskin","Arms",sleeve_weight,18,.08)
        ellipsoid(label+" rounded shoulder",(side*.193,1.40,0),(.072,.075,.078),"Oilskin","Arms",{"Chest":.6,label+"UpperArm":.4},20,12)
        cuff=[tuple(wrist+Vector((side*.01,-.02,0))),tuple(wrist+Vector((-side*.005,.015,0)))]
        tube(label+" cuff",cuff,[(.049,.046),(.050,.047)],"Trim","Arms",{label+"LowerArm":1},20)
        palm=wrist+Vector((side*.025,-.055,.012))
        ellipsoid(label+" leather glove",tuple(palm),(.039,.060,.022),"Leather","Hands",{label+"Hand":1},20,12)
        for finger,dx,length in [("Index",-.024,.077),("Middle",-.007,.085),("Ring",.010,.078),("Little",.026,.060)]:
            start=palm+Vector((side*dx,-.040,.003))
            middle=start+Vector((side*.004,-length*.50,.008))
            end=start+Vector((side*.005,-length,.016))
            b1,b2,b3=label+finger+"Proximal",label+finger+"Intermediate",label+finger+"Distal"
            BONES[b1]=(tuple(start),tuple(middle),label+"Hand")
            BONES[b2]=(tuple(middle),tuple(start.lerp(end,.80)),b1)
            BONES[b3]=(tuple(start.lerp(end,.80)),tuple(end),b2)
            centres=[tuple(start.lerp(end,t)+Vector((0,0,-.008*math.sin(math.pi*t)))) for t in [i/6 for i in range(7)]]
            def finger_weight(p,s=start,L=length,a=b1,b=b2,c=b3):
                t=max(0,min(1,(s.y-p[1])/L))
                if t<.5:return {a:1}
                blend=min(1,(t-.5)/.3);return {b:1-blend,c:blend}
            tube(label+finger,centres,[(.011*(1-.35*i/6),.012*(1-.35*i/6)) for i in range(7)],"Leather","Hands",finger_weight,10)
        thumb=palm+Vector((-side*.028,-.003,.011));tip=thumb+Vector((-side*.036,-.045,.018))
        BONES[label+"ThumbProximal"]=(tuple(thumb),tuple(thumb.lerp(tip,.5)),label+"Hand")
        BONES[label+"ThumbIntermediate"]=(tuple(thumb.lerp(tip,.5)),tuple(thumb.lerp(tip,.8)),label+"ThumbProximal")
        BONES[label+"ThumbDistal"]=(tuple(thumb.lerp(tip,.8)),tuple(tip),label+"ThumbIntermediate")
        tube(label+"Thumb",[tuple(thumb.lerp(tip,t)) for t in (0,.25,.5,.75,1)],[(.012,.012),(.012,.012),(.011,.011),(.009,.009),(.008,.009)],"Leather","Hands",{label+"ThumbProximal":1},10)
        for j in (-1,0,1):
            line("Glove seam",[tuple(palm+Vector((side*j*.016,y,.023))) for y in (-.033,.025)],.0008,"LeatherSeam","Hands",{label+"Hand":1})


def legs_and_boots():
    for side,label in [(-1,"Left"),(1,"Right")]:
        centres=[];radii=[]
        for row in range(25):
            t=row/24;y=.94-.74*t
            centres.append((side*(.09+.01*t),y,.006*math.sin(math.pi*t)))
            r=.087*(1-t)+.057*t;r+=.009*math.sin(math.pi*t)
            radii.append((r,r*.9))
        def trouser_weights(p,l=label):
            t=max(0,min(1,(.60-p[1])/.13));return {l+"UpperLeg":1-t,l+"LowerLeg":t}
        tube(label+" navy trousers",centres,radii,"Trousers","Legs",trouser_weights,18,.05)
        x=side*.10
        tube(label+" tall rubber boot",[(x,y,0) for y in (.115,.17,.23,.30,.36)],[(.067,.064),(.067,.059),(.069,.062),(.073,.065),(.075,.068)],"Rubber","Boots",{label+"LowerLeg":1},20,.025)
        ellipsoid(label+" boot foot",(x,.092,.073),(.074,.070,.145),"Rubber","Boots",{label+"Foot":1},24,14)
        ellipsoid(label+" boot sole",(x,.038,.075),(.077,.025,.15),"Sole","Boots",{label+"Foot":1},24,10)
        for z in (-.025,.015,.055,.095,.135,.175):
            obj=M.box(label+" sole tread",(x,.019,z),(.13,.013,.018),"Sole")
            keep(obj,"Boots",{label+"Foot":1})
        # Rolled upper rim and a rear pull tab.
        rim=[(x+.075*math.cos(a),.36,.068*math.sin(a)) for a in [2*math.pi*i/24 for i in range(25)]]
        line(label+" boot rim",rim,.004,"Sole","Boots",{label+"LowerLeg":1})


def skeleton():
    BONES.update({
        "Hips":((0,.94,0),(0,1.08,0),None),
        "Spine":((0,1.08,0),(0,1.26,0),"Hips"),
        "Chest":((0,1.26,0),(0,1.43,0),"Spine"),
        "Neck":((0,1.43,0),(0,1.54,0),"Chest"),
        "Head":((0,1.54,0),(0,1.79,0),"Neck"),
    })
    for side,label in [(-1,"Left"),(1,"Right")]:
        BONES.update({
            label+"Shoulder":((0,1.405,0),(side*.195,1.405,0),"Chest"),
            label+"UpperArm":((side*.195,1.405,0),(side*.285,1.18,.005),label+"Shoulder"),
            label+"LowerArm":((side*.285,1.18,.005),(side*.385,.967,.018),label+"UpperArm"),
            label+"Hand":((side*.385,.967,.018),(side*.41,.87,.03),label+"LowerArm"),
            label+"UpperLeg":((side*.09,.94,0),(side*.10,.555,.006),"Hips"),
            label+"LowerLeg":((side*.10,.555,.006),(side*.10,.15,0),label+"UpperLeg"),
            label+"Foot":((side*.10,.15,0),(side*.10,.07,.15),label+"LowerLeg"),
            label+"Toes":((side*.10,.07,.15),(side*.10,.06,.22),label+"Foot"),
        })
    arm=bpy.data.armatures.new("FishermanSkeleton")
    rig=bpy.data.objects.new("Fisherman_Rig",arm);bpy.context.collection.objects.link(rig)
    bpy.context.view_layer.objects.active=rig;rig.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    for name,(head,tail,parent) in BONES.items():
        bone=arm.edit_bones.new(name);bone.head=M.u(head);bone.tail=M.u(tail)
    for name,(_,_,parent) in BONES.items():
        if parent:arm.edit_bones[name].parent=arm.edit_bones[parent]
    bpy.ops.object.mode_set(mode="OBJECT");rig.show_in_front=True
    return rig


def blend_coat_shoulders():
    """Join the main coat volumes into a continuous shoulder surface before baking."""
    import bmesh
    from mathutils.kdtree import KDTree
    names={"Oilskin coat body","Left rain sleeve","Right rain sleeve","Left rounded shoulder","Right rounded shoulder"}
    parts=[o for o in OBJECTS if o.name in names]
    samples=[]
    for obj in parts:
        for vertex in obj.data.vertices:
            values={obj.vertex_groups[g.group].name:g.weight for g in vertex.groups}
            samples.append((obj.matrix_world@vertex.co,values))
        bm=bmesh.new();bm.from_mesh(obj.data)
        bmesh.ops.holes_fill(bm,edges=[e for e in bm.edges if e.is_boundary],sides=0)
        bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
    tree=KDTree(len(samples))
    for i,(point,weights) in enumerate(samples):tree.insert(point,i)
    tree.balance()
    OBJECTS[:]=[o for o in OBJECTS if o not in parts]
    bpy.ops.object.select_all(action="DESELECT")
    for obj in parts:obj.select_set(True)
    bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join()
    obj=bpy.context.object;obj.name="Continuous coat and sleeves"
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    remesh=obj.modifiers.new("Continuous shoulders","REMESH");remesh.mode="VOXEL";remesh.voxel_size=.012;remesh.use_smooth_shade=True
    bpy.ops.object.modifier_apply(modifier=remesh.name)
    smooth=obj.modifiers.new("Cloth seam smoothing","SMOOTH");smooth.factor=.5;smooth.iterations=4
    bpy.ops.object.modifier_apply(modifier=smooth.name)
    decimate=obj.modifiers.new("Coat game topology","DECIMATE");decimate.ratio=.30
    bpy.ops.object.modifier_apply(modifier=decimate.name)
    obj.vertex_groups.clear();groups={}
    for vertex in obj.data.vertices:
        _,index,_=tree.find(vertex.co)
        for name,weight in samples[index][1].items():
            if name not in groups:groups[name]=obj.vertex_groups.new(name=name)
            groups[name].add([vertex.index],weight,"REPLACE")
    obj.data.materials.clear();obj.data.materials.append(M.MATERIALS["Oilskin"])
    for p in obj.data.polygons:p.material_index=0;p.use_smooth=True
    OBJECTS.append(obj)


def join_and_unwrap():
    bpy.ops.object.select_all(action="DESELECT")
    for obj in OBJECTS:obj.select_set(True)
    bpy.context.view_layer.objects.active=OBJECTS[0]
    bpy.ops.object.join();obj=bpy.context.object;obj.name="Fisherman_SkinnedMesh"
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    bpy.ops.object.mode_set(mode="EDIT");bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.quads_convert_to_tris(quad_method="FIXED",ngon_method="BEAUTY")
    bpy.ops.uv.smart_project(angle_limit=math.radians(66),island_margin=.012)
    bpy.ops.object.mode_set(mode="OBJECT")
    obj.data.update();return obj


def bake(obj):
    scene=bpy.context.scene;scene.render.engine="CYCLES";scene.cycles.device="CPU"
    scene.cycles.samples=4;scene.cycles.use_denoising=False
    scene.render.bake.use_pass_direct=False;scene.render.bake.use_pass_indirect=False;scene.render.bake.use_pass_color=True
    scene.render.bake.margin=6
    result={}
    for kind,space in [("Albedo","sRGB"),("Normal","Non-Color"),("Roughness","Non-Color")]:
        image=bpy.data.images.new("Fisherman_"+kind,width=2048,height=2048,alpha=False)
        image.colorspace_settings.name=space
        for material in obj.data.materials:
            nodes=material.node_tree.nodes
            for node in nodes:node.select=False
            target=nodes.new("ShaderNodeTexImage");target.image=image;target.select=True;nodes.active=target
        bpy.context.view_layer.objects.active=obj;obj.select_set(True)
        print("BAKING",kind,flush=True)
        bpy.ops.object.bake(type={"Albedo":"DIFFUSE","Normal":"NORMAL","Roughness":"ROUGHNESS"}[kind])
        image.filepath_raw=str(ART/("Fisherman_"+kind+".png"));image.file_format="PNG";image.save()
        result[kind]=image
    material=bpy.data.materials.new("FishermanSurface");material.use_nodes=True
    nodes=material.node_tree.nodes;links=material.node_tree.links;bsdf=nodes.get("Principled BSDF")
    colour=nodes.new("ShaderNodeTexImage");colour.image=result["Albedo"];links.new(colour.outputs["Color"],bsdf.inputs["Base Color"])
    normal=nodes.new("ShaderNodeTexImage");normal.image=result["Normal"]
    decode=nodes.new("ShaderNodeNormalMap");links.new(normal.outputs["Color"],decode.inputs["Color"]);links.new(decode.outputs["Normal"],bsdf.inputs["Normal"])
    roughness=nodes.new("ShaderNodeTexImage");roughness.image=result["Roughness"];links.new(roughness.outputs["Color"],bsdf.inputs["Roughness"])
    # Unity's metallic workflow stores smoothness in alpha. This is a baked data map.
    import numpy as np
    pixels=np.empty(2048*2048*4,dtype=np.float32);result["Roughness"].pixels.foreach_get(pixels)
    pixels=pixels.reshape((-1,4));pixels[:,3]=1-pixels[:,0];pixels[:,:3]=0
    smooth=bpy.data.images.new("Fisherman_Smoothness",width=2048,height=2048,alpha=True)
    smooth.colorspace_settings.name="Non-Color";smooth.pixels.foreach_set(pixels.reshape(-1))
    smooth.filepath_raw=str(ART/"Fisherman_Smoothness.png");smooth.file_format="PNG";smooth.save();result["Smoothness"]=smooth
    obj.data.materials.clear();obj.data.materials.append(material)
    for face in obj.data.polygons:face.material_index=0
    return result


def extract(obj,region,offset):
    group=obj.vertex_groups["VIS_"+region].index
    chosen={v.index for v in obj.data.vertices if any(g.group==group and g.weight>.99 for g in v.groups)}
    source_faces=[p for p in obj.data.polygons if all(i in chosen for i in p.vertices)]
    indices=sorted({i for p in source_faces for i in p.vertices});mapping={old:i for i,old in enumerate(indices)}
    positions=[obj.data.vertices[i].co-M.u(offset) for i in indices]
    mesh=bpy.data.meshes.new("Fisherman_"+region)
    mesh.from_pydata(positions,[],[[mapping[i] for i in p.vertices] for p in source_faces]);mesh.update()
    uv=mesh.uv_layers.new(name="UVMap");source=obj.data.uv_layers.active.data
    for face,original in zip(mesh.polygons,source_faces):
        face.use_smooth=original.use_smooth
        for dest,old in zip(face.loop_indices,original.loop_indices):uv.data[dest].uv=source[old].uv
    item=bpy.data.objects.new("Fisherman_"+region,mesh);bpy.context.collection.objects.link(item)
    mesh.materials.append(obj.data.materials[0]);item.hide_render=True;item.hide_viewport=True
    return item


def texture_meta(path,normal):
    # Start from a committed texture importer so Unity's versioned fields stay compatible.
    source=ROOT/("Assets/Abandoned_Asylum/Textures/Pipe_Normal.png.meta" if normal else "Assets/_LighthouseKeepers/Art/Textures/LK_Water_Wear.png.meta")
    import re
    text=source.read_text();text=re.sub(r"^guid: \w+", "guid: "+M.guid(path),text,flags=re.M)
    text=re.sub(r"maxTextureSize: \d+","maxTextureSize: 2048",text)
    text=re.sub(r"wrap[UVW]: \d+",lambda m:m[0].split(':')[0]+': 1',text)
    Path(str(path)+".meta").write_text("\n".join(l.rstrip() for l in text.splitlines())+"\n")


def unity_material():
    for kind in ("Albedo","Normal","Roughness","Smoothness"):
        path=ART/("Fisherman_"+kind+".png");texture_meta(path,kind=="Normal")
        if kind in ("Roughness","Smoothness"):
            meta=Path(str(path)+".meta");meta.write_text(meta.read_text().replace("sRGBTexture: 1","sRGBTexture: 0"))
    text=(ROOT/"Assets/WatchDemo/Metal.mat").read_text()
    import re
    text=text.replace("m_Name: Metal","m_Name: FishermanSurface")
    text=text.replace("m_ValidKeywords: []","m_ValidKeywords:\n  - _NORMALMAP\n  - _METALLICSPECGLOSSMAP")
    for prop,kind in [("_BaseMap","Albedo"),("_BumpMap","Normal"),("_MetallicGlossMap","Smoothness")]:
        pattern=r"(- "+prop+r":\n        m_Texture:) \{fileID: 0\}"
        text=re.sub(pattern,r"\1 {fileID: 2800000, guid: "+M.guid(ART/("Fisherman_"+kind+".png"))+", type: 3}",text)
    text=re.sub(r"- _BaseColor: .*","- _BaseColor: {r: 1, g: 1, b: 1, a: 1}",text)
    text=re.sub(r"- _Color: .*","- _Color: {r: 1, g: 1, b: 1, a: 1}",text)
    text=re.sub(r"- _Metallic: .*","- _Metallic: 0",text)
    text=re.sub(r"- _Smoothness: .*","- _Smoothness: 1",text)
    text=re.sub(r"- _BumpScale: .*","- _BumpScale: 0.7",text)
    path=ART/"FishermanSurface.mat";path.write_text("\n".join(l.rstrip() for l in text.splitlines())+"\n");M.meta(path,main=2100000)


def rig_checks(obj,rig):
    names={b.name for b in rig.data.bones}
    for vertex in obj.data.vertices:
        weights=[g.weight for g in vertex.groups if obj.vertex_groups[g.group].name in names]
        assert weights and abs(sum(weights)-1)<.001,vertex.index
    obj.parent=rig;modifier=obj.modifiers.new("Fisherman skeleton","ARMATURE");modifier.object=rig
    baseline=[v.co.copy() for v in obj.evaluated_get(bpy.context.evaluated_depsgraph_get()).data.vertices]
    rig.pose.bones["LeftLowerArm"].rotation_mode="XYZ";rig.pose.bones["LeftLowerArm"].rotation_euler.x=.6
    bpy.context.view_layer.update()
    posed=obj.evaluated_get(bpy.context.evaluated_depsgraph_get()).data.vertices
    moved=sum((a-b.co).length>.001 for a,b in zip(baseline,posed))
    assert moved>100,"Arm rig did not deform the mesh"
    rig.pose.bones["LeftLowerArm"].rotation_euler=(0,0,0);bpy.context.view_layer.update()
    return {"bones":len(names),"vertices":len(obj.data.vertices),"triangles":sum(len(p.vertices)-2 for p in obj.data.polygons),"arm_pose_vertices_moved":moved}


def exports(obj,rig,images):
    bpy.ops.object.select_all(action="DESELECT");obj.select_set(True);rig.select_set(True)
    bpy.context.view_layer.objects.active=rig
    bpy.ops.export_scene.fbx(filepath=str(OUT/"Fisherman_Rigged.fbx"),use_selection=True,object_types={"MESH","ARMATURE"},
        add_leaf_bones=False,bake_anim=False,axis_forward="-Z",axis_up="Y",path_mode="COPY",embed_textures=True)
    bpy.ops.export_scene.gltf(filepath=str(OUT/"Fisherman_Rigged.glb"),use_selection=True,export_format="GLB",export_animations=False)
    for image in images.values():image.pack()


def studio(obj,rig):
    scene=bpy.context.scene;scene.render.engine="CYCLES";scene.cycles.samples=32;scene.cycles.use_denoising=False
    scene.render.resolution_x=1500;scene.render.resolution_y=1100;scene.render.resolution_percentage=100
    scene.world.color=(.18,.18,.18)
    bpy.ops.object.camera_add(location=M.u((2.5,1.45,5)))
    camera=bpy.context.object;camera.rotation_euler=(M.u((0,.95,0))-camera.location).to_track_quat('-Z','Y').to_euler()
    camera.data.type="ORTHO";camera.data.ortho_scale=2.85;scene.camera=camera
    for position,power,size in [((-2,3,3),350,3),((2,2,2),180,3),((0,3,-2),250,2)]:
        bpy.ops.object.light_add(type="AREA",location=M.u(position));light=bpy.context.object
        light.data.energy=power;light.data.shape="DISK";light.data.size=size
        light.rotation_euler=(M.u((0,1,0))-light.location).to_track_quat('-Z','Y').to_euler()
    bpy.context.view_layer.objects.active=rig;bpy.ops.object.select_all(action="DESELECT");rig.select_set(True)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"Fisherman_Rigged.blend"))
    if "--preview" in sys.argv:
        scene.render.filepath=str(OUT/"Fisherman-preview.png");bpy.ops.render.render(write_still=True)
        # Orthographic front/side/back views of the actual model, with the same camera scale.
        scene.render.resolution_x=700;scene.render.resolution_y=1100
        for name,position in [("front",(0,.97,5)),("side",(5,.97,0)),("back",(0,.97,-5))]:
            camera.location=M.u(position);camera.rotation_euler=(M.u((0,.97,0))-camera.location).to_track_quat('-Z','Y').to_euler()
            camera.data.ortho_scale=2.05;scene.render.filepath=str(OUT/("Fisherman-"+name+".png"))
            bpy.ops.render.render(write_still=True)
        # Render the three actual model views together without editing reference images.
        for x,angle in [(0,math.pi/2),(.78,math.pi)]:
            copy_rig=rig.copy();copy_rig.data=rig.data.copy();bpy.context.collection.objects.link(copy_rig)
            copy_mesh=obj.copy();bpy.context.collection.objects.link(copy_mesh);copy_mesh.parent=copy_rig
            copy_mesh.modifiers["Fisherman skeleton"].object=copy_rig
            copy_rig.location.x=x;copy_rig.rotation_euler.z=angle
        rig.location.x=-.78
        camera.location=M.u((0,.97,5));camera.rotation_euler=(M.u((0,.97,0))-camera.location).to_track_quat('-Z','Y').to_euler()
        camera.data.ortho_scale=3.05;scene.render.resolution_x=1800;scene.render.resolution_y=1250
        scene.render.filepath=str(OUT/"Fisherman-turnaround.png");bpy.ops.render.render(write_still=True)


if __name__ == "__main__":
    bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
    M.directory(ART);OUT.mkdir(parents=True,exist_ok=True)
    M.MATERIALS={
        "Skin":surface("Skin",(.67,.44,.31),.62,.07,.0004),
        "Oilskin":surface("Oilskin",(.86,.54,.065),.38,.10,.0015),
        "Trim":surface("Trim",(.74,.43,.045),.42,.08,.001),
        "Seams":surface("Seams",(.92,.68,.25),.5,.03,.0001),
        "Lining":surface("Lining",(.40,.25,.055),.75,.08,.001),
        "Hair":surface("Hair",(.22,.18,.15),.85,.32,.001),
        "EyeWhite":surface("EyeWhite",(.78,.75,.67),.28,0,0),
        "Iris":surface("Iris",(.20,.27,.24),.2,0,0),
        "Ink":surface("Ink",(.02,.025,.025),.4,0,0),
        "Lips":surface("Lips",(.30,.16,.12),.7,0,0),
        "Knit":surface("Knit",(.32,.28,.21),.9,.15,.002),
        "Leather":surface("Leather",(.23,.12,.065),.58,.15,.0015),
        "LeatherSeam":surface("LeatherSeam",(.37,.23,.13),.7,.04,.0002),
        "Trousers":surface("Trousers",(.075,.095,.13),.85,.15,.002),
        "Rubber":surface("Rubber",(.08,.09,.085),.48,.12,.001),
        "Sole":surface("Sole",(.035,.04,.035),.75,.08,.001),
        "Metal":surface("Metal",(.26,.23,.16),.32,.08,.0002),
    }
    print("BUILDING detailed fisherman geometry",flush=True)
    face();hood();coat();arms_and_hands();legs_and_boots();blend_coat_shoulders()
    obj=join_and_unwrap();images=bake(obj);unity_material()
    head=extract(obj,"Head",(0,EYES,0));body=extract(obj,"Body",(0,EYES-.48,0))
    M.prefab("PF_ReferenceFisherman",M.native_mesh("Fisherman_Head",[head],True),M.native_mesh("Fisherman_Coat",[body],True),EYES)
    rig=skeleton();facts=rig_checks(obj,rig)
    (OUT/"model-checks.json").write_text(json.dumps(facts,indent=2));print("RIG CHECKS",facts,flush=True)
    exports(obj,rig,images);studio(obj,rig)
