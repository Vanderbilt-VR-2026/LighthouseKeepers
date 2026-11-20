"""Blender-authored historical keeper. Reuses the CC0 head and proven 51-bone exporter.
Run Blender --background --factory-startup --threads 4 --python this_file.py.
The old fisherman art remains untouched. Native skin imports without Blender installed.
"""
from pathlib import Path
import sys, math, json, importlib.util, re
import bpy
from mathutils import Vector
HERE=Path(__file__).resolve().parent
sys.path.insert(0,str(HERE))
import build_reference_fisherman as F
import export_fisherman_skin as E
M=F.M; ROOT=F.ROOT
ART=ROOT/'Assets/Art/Avatars/HarborKeeper'; OUT=ROOT/'artifacts/Avatars/HarborKeeper'
F.ART=M.ART=ART; F.OUT=OUT


def box(name,p,size,mat,region='Body',weights=F.body_weights):
    return F.keep(M.box(name,p,size,mat),region,weights)


def band(name,levels,mat,region,weights,sides=32):
    return F.keep(M.rings(name,levels,mat,sides),region,weights)


def cap():
    # Rounded working helmet and fitted brow lamp, all weighted to the existing head.
    # The wearer sees the beam from the tracked HMD, not this first-person mesh.
    band('Steel helmet brim',[(1.735,.111,.116),(1.744,.116,.121),(1.751,.115,.120),(1.756,.105,.111)],'Helmet','Head',{'Head':1},40)
    band('Steel helmet shell',[(1.748,.103,.109),(1.779,.105,.110),(1.812,.096,.101),(1.844,.073,.077),(1.862,.042,.045),(1.869,.004,.004)],'Helmet','Head',{'Head':1},40)
    band('Lamp retaining strap',[(1.765,.106,.111),(1.777,.106,.111)],'Rubber','Head',{'Head':1},40)
    box('Headlamp mounting plate',(0,1.786,.113),(.065,.058,.018),'Metal','Head',{'Head':1})
    F.ellipsoid('Headlamp housing',(0,1.789,.128),(.037,.037,.018),'Rubber','Head',{'Head':1},20,10)
    F.ellipsoid('Headlamp rim',(0,1.789,.142),(.031,.031,.010),'Metal','Head',{'Head':1},20,10)
    F.ellipsoid('Headlamp glass',(0,1.789,.149),(.024,.024,.004),'EyeWhite','Head',{'Head':1},20,10)
    for side in (-1,1):
        F.ellipsoid('Grey temple',(side*.076,1.691,-.012),(.012,.037,.053),'Hair','Head',{'Head':1},16,8)


def expressive_face():
    F.face()
    # No separate whisker tubes protruding through the nostrils; retain the fitted beard.
    for ob in list(F.OBJECTS):
        if ob.name.startswith("Moustache"):
            F.OBJECTS.remove(ob);bpy.data.objects.remove(ob,do_unlink=True)
    # Anatomical eyelids remain on the head; iris and sclera rotate inside their sockets.
    for ob in F.OBJECTS:
        if ob.name.startswith(('Eye white','Iris','Pupil')):
            x=sum(M.unity(ob.matrix_world@v.co)[0] for v in ob.data.vertices)/len(ob.data.vertices)
            joint='EyeL' if x<0 else 'EyeR'
            for g in list(ob.vertex_groups):
                if not g.name.startswith('VIS_'):ob.vertex_groups.remove(g)
            ob.vertex_groups.new(name=joint).add(list(range(len(ob.data.vertices))),1,'REPLACE')
        if ob.name == 'Anatomical fisherman head':
            jaw=ob.vertex_groups.get('Jaw') or ob.vertex_groups.new(name='Jaw')
            for v in ob.data.vertices:
                x,y,z=M.unity(ob.matrix_world@v.co)
                if y<1.600 and y>1.55 and z>-.02:
                    w=max(0,min(1,(1.600-y)/.003))*max(0,min(1,(z+.02)/.055))
                    if w>0:
                        for g in list(v.groups):
                            group=ob.vertex_groups[g.group]
                            if not group.name.startswith('VIS_'):group.remove([v.index])
                        jaw.add([v.index],w,'REPLACE');ob.vertex_groups['Head'].add([v.index],1-w,'REPLACE')
    # Bake the beard into the skin atlas. A duplicate beard shell was intersecting
    # the nostrils and jaw during animation; the head alone deforms reliably.
    head=next(o for o in F.OBJECTS if o.name=='Anatomical fisherman head')
    beard=next(o for o in F.OBJECTS if o.name=='Fitted beard')
    head.data.materials.append(M.MATERIALS['Hair']);hair_index=len(head.data.materials)-1
    for face in head.data.polygons:
        x,y,z=M.unity(face.center);top=F.EYES-.045+.015*min(1,abs(x)/.07)
        if F.EYES-.14<y<top and z>.005 and abs(x)<.085 and not (abs(x)<.045 and y>1.61):
            face.material_index=hair_index
    F.OBJECTS.remove(beard);bpy.data.objects.remove(beard,do_unlink=True)
    # Mouth interior is concealed by the closed lips, exposed by the lowered jaw.
    F.ellipsoid('Mouth interior',(0,1.585,.050),(.020,.018,.010),'Mouth','Head',{'Head':1},20,12)
    from mathutils.bvhtree import BVHTree
    skin_tree=BVHTree.FromPolygons([Vector(M.unity(head.matrix_world@v.co)) for v in head.data.vertices],[list(f.vertices) for f in head.data.polygons])
    for side,joint in ((-1,'EyeL'),(1,'EyeR')):
        x=side*.030775
        F.BONES[joint]=((x,1.665,.064535),(x,1.685,.064535),'Head')
        tear='TearL' if side<0 else 'TearR'
        F.BONES[tear]=((x,1.651,.076),(x,1.621,.071),'Head')
        points=[]
        for i in range(13):
            y=1.654-i*.0038
            hit=skin_tree.ray_cast(Vector((x,y,.2)),Vector((0,0,-1)))
            if hit[0] is not None:points.append((x,y,hit[0].z+.002))
        F.line('Dark tear trail',points,.0032,'Tear','Head',{tear:1},6)
        # Tiny iris spokes and corneal highlight stay with the moving eyeball.
        for i in range(16):
            a=i*math.pi/8
            F.line('Iris fibre',[(x+math.cos(a)*r,1.665+math.sin(a)*r,.080) for r in (.0036,.0052)],.00018,'IrisLight','Head',{joint:1},4)
        F.ellipsoid('Eye catchlight',(x-.0018,1.668,.0804),(.0008,.0008,.0004),'EyeWhite','Head',{joint:1},8,6)
    F.BONES['Jaw']=((0,1.618,.015),(0,1.58,.04),'Head')


def clothes():
    F.LEVELS=[(.84,.180,.111),(1.02,.181,.112),(1.18,.180,.112),(1.35,.211,.105),(1.43,.177,.091),(1.49,.062,.053)]
    band('Wool shirt',F.LEVELS,'Knit','Body',F.body_weights)
    F.patch('Canvas overalls bib',0,1.222,.224,.277,'Canvas',.007)
    F.patch('Bib breast pocket',0,1.228,.126,.094,'Canvas',.013)
    for s in (-1,1):
        F.line('Bib stitched border',[(s*.106,y,F.front(s*.106,y,.010)) for y in (1.09,1.2,1.35)],.0015,'Seams','Body',F.body_weights)
        # Flat woven straps curve over the shoulder, underneath the open coat.
        pts=[(s*.10,1.351,F.front(s*.1,1.351,.012)),(s*.12,1.405,.091),(s*.115,1.453,.043),(s*.10,1.447,-.03)]
        vs=[(x+d,y,z) for x,y,z in pts for d in (-.015,.015)]
        F.custom('Overall shoulder strap',vs,[(i*2,i*2+1,i*2+3,i*2+2) for i in range(len(pts)-1)],'Canvas','Body',F.body_weights)
        box('Brass strap buckle',(s*.10,1.346,F.front(s*.1,1.346,.017)),(.035,.025,.006),'Metal')
        box('Buckle opening',(s*.10,1.346,F.front(s*.1,1.346,.021)),(.023,.014,.003),'Ink')
    # Coat sides/back surround the shirt, leaving the pale working bib exposed.
    vs=[];fs=[];rows=23;n=36
    for j in range(rows):
        y=.82+.66*j/(rows-1);rx,rz=F.profile(max(.84,y));rx+=.012;rz+=.012
        gap=.39 if y<1.35 else .27
        for i in range(n):
            a=math.pi/2+gap+(2*math.pi-2*gap)*i/(n-1)
            ripple=.002*math.sin(j*1.4+i*.7)
            vs.append(((rx+ripple)*math.cos(a),y,(rz+ripple)*math.sin(a)))
    for j in range(rows-1):
        for i in range(n-1):
            a=j*n+i;fs.append((a,a+n,a+n+1,a+1))
    coat=F.custom('Open navy sea coat',vs,fs,'Oilskin','Body',F.body_weights)
    # Thin folded edge closes the cut surface; no see-through back faces at the lapel.
    for side,index in ((-1,0),(1,n-1)):
        edge=[vs[j*n+index] for j in range(rows)]
        F.line('Coat bound edge',edge,.003,'Trim','Body',F.body_weights,6)
        lapel=[(side*.056,1.469,.065),(side*.131,1.419,.113),(side*.087,1.286,.122),(side*.063,1.386,.124)]
        F.custom('Folded storm lapel',lapel,[(0,1,2,3)],'Trim','Body',F.body_weights)
        F.patch('Coat pocket',side*.135,.955,.079,.117,'Oilskin',.023)
        F.patch('Coat pocket flap',side*.135,1.016,.085,.026,'Trim',.030)
        for y in (1.05,1.20,1.34):
            x=side*.085;F.ellipsoid('Coat brass button',(x,y,F.front(x,y,.029)),(.004,.005,.003),'Metal','Body',F.body_weights,10,6)
    # Short scarf/collar stays on the chest, never crosses the face/tracked eyes.
    scarf=band('Wool scarf collar',[(1.470,.071,.062),(1.510,.065,.060),(1.554,.060,.057)],'Scarf','Body',{'Neck':1},24)
    F.patch('Scarf tail',.025,1.385,.045,.16,'Scarf',.027)
    F.line('Scarf edge',[(-.001,y,F.front(0,y,.032)) for y in (1.31,1.38,1.46)],.0015,'Seams','Body',F.body_weights)
    # Closed belt kit is dress, not a pretend held gameplay tool.
    band('Worn leather belt',[(1.013,.197,.130),(1.047,.195,.129)],'LeatherSeam','Body',{'Hips':1})
    box('Belt buckle',(0,1.030,.134),(.045,.035,.008),'Metal','Body',{'Hips':1})
    box('Belt buckle hole',(0,1.030,.140),(.031,.021,.003),'Ink','Body',{'Hips':1})
    box('Weatherproof belt pouch',(.174,.947,.093),(.075,.112,.053),'LeatherSeam','Body',{'Hips':1})
    box('Pouch flap',(.174,1.005,.111),(.080,.025,.034),'Leather','Body',{'Hips':1})
    F.line('Whistle lanyard',[(-.04,1.486,.050),(-.07,1.32,.131),(-.018,1.27,.137),(.08,1.48,.052)],.0018,'Ink','Body',F.body_weights)
    box('Brass signal whistle',(-.018,1.266,.141),(.012,.032,.013),'Metal')


def legs():
    for s,label in ((-1,'Left'),(1,'Right')):
        centres=[];radii=[]
        for i in range(21):
            t=i/20;centres.append((s*(.09+.01*t),.99-.75*t,.004*math.sin(math.pi*t)))
            r=.089-.034*t+.005*math.sin(t*math.pi);radii.append((r,r*.96))
        def weights(p,l=label):
            t=max(0,min(1,(.62-p[1])/.14));return {l+'UpperLeg':1-t,l+'LowerLeg':t}
        F.tube(label+' canvas overall leg',centres,radii,'Canvas','Legs',weights,16,.035)
        F.tube(label+' rolled trouser cuff',[(s*.1,.24,0),(s*.1,.272,0)],[(.061,.06),(.062,.06)],'Seams','Legs',{label+'LowerLeg':1},16)
        F.tube(label+' ankle boot shaft',[(s*.1,.12,0),(s*.1,.20,0),(s*.1,.255,0)],[(.061,.064),(.062,.058),(.063,.06)],'Rubber','Boots',{label+'LowerLeg':1},16,.02)
        F.ellipsoid(label+' leather work boot',(s*.1,.079,.067),(.069,.069,.136),'Rubber','Boots',{label+'Foot':1},20,10)
        F.ellipsoid(label+' sole',(s*.1,.031,.073),(.071,.023,.139),'Sole','Boots',{label+'Foot':1},20,8)
        for z in (.01,.055,.10,.15):box(label+' tread',(s*.1,.015,z),(.12,.012,.019),'Sole','Boots',{label+'Foot':1})
        for y,z in ((.143,.091),(.160,.071),(.180,.052)):
            F.line(label+' boot lacing',[(s*.1-.024,y,z),(s*.1+.024,y,z+.002)],.0017,'Seams','Boots',{label+'Foot':1})


def studio(obj,rig,images):
    scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=20
    scene.render.resolution_x=800;scene.render.resolution_y=1100;scene.render.resolution_percentage=100
    scene.world.color=(.19,.19,.19)
    bpy.ops.object.camera_add(location=M.u((2.4,1.3,5)))
    camera=bpy.context.object;camera.data.type='ORTHO';camera.data.ortho_scale=2.04;scene.camera=camera
    camera.rotation_euler=(M.u((0,.96,0))-camera.location).to_track_quat('-Z','Y').to_euler()
    for pos,power,size in [((-2,3,3),450,3),((2,2,2),230,3),((0,3,-2),300,2)]:
        bpy.ops.object.light_add(type='AREA',location=M.u(pos));l=bpy.context.object;l.data.energy=power;l.data.size=size
        l.rotation_euler=(M.u((0,1,0))-l.location).to_track_quat('-Z','Y').to_euler()
    for im in images.values():im.pack()
    source=ROOT/'SourceArt/Avatars';source.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(source/'HarborKeeper.blend'))
    for name,p in [('front',(0,.96,5)),('side',(5,.96,0)),('back',(0,.96,-5)),('three-quarter',(2.7,1.1,5))]:
        camera.location=M.u(p);camera.rotation_euler=(M.u((0,.96,0))-camera.location).to_track_quat('-Z','Y').to_euler()
        scene.render.filepath=str(OUT/(name+'.png'));bpy.ops.render.render(write_still=True)


def export_unity_skin(obj,rig,path):
    result=E.export_skin(obj,rig,path)
    path.write_text(json.dumps(result,separators=(',',':'))+'\n')
    return result


def main():
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    M.directory(ART);OUT.mkdir(parents=True,exist_ok=True)
    colours={'Skin':((.67,.47,.35),.64),'Oilskin':((.10,.17,.20),.64),'Trim':((.15,.23,.25),.68),
       'Canvas':((.49,.47,.38),.90),'Seams':((.60,.57,.46),.85),'Hair':((.60,.59,.53),.9),
       'EyeWhite':((.75,.73,.66),.35),'Iris':((.20,.29,.28),.3),'Ink':((.035,.043,.045),.65),
       'Knit':((.15,.21,.21),.9),'Scarf':((.20,.29,.26),.92),'Leather':((.50,.33,.18),.63),
       'LeatherSeam':((.23,.14,.08),.7),'Rubber':((.095,.09,.07),.62),'Sole':((.033,.034,.03),.85),
       'Mouth':((.055,.012,.015),.5),'Tear':((.17,.012,.014),.22),'IrisLight':((.33,.38,.24),.3),
       'Metal':((.49,.36,.17),.38),'Helmet':((.25,.29,.23),.64)}
    M.MATERIALS={n:F.surface(n,c,r,.13,.001 if n not in ('EyeWhite','Iris','Skin') else .0002) for n,(c,r) in colours.items()}
    expressive_face();cap();clothes();F.arms_and_hands();legs()
    # The sleeve itself overlaps the jacket shoulder. Detached sphere caps read as armour.
    for ob in list(F.OBJECTS):
        if 'rounded shoulder' in ob.name:
            F.OBJECTS.remove(ob);bpy.data.objects.remove(ob,do_unlink=True)
    # Weld the sleeve roots into the inner torso, keeping the open jacket/bib layered above.
    for sign,label in ((-1,'Left'),(1,'Right')):
        F.ellipsoid(label+' jacket shoulder',(sign*.190,1.398,0),(.064,.056,.067),'Oilskin','Arms',{'Chest':.55,label+'UpperArm':.45},16,10)
    F.blend_coat_shoulders({'Wool shirt','Left rain sleeve','Right rain sleeve','Left jacket shoulder','Right jacket shoulder'})
    shirt=next(o for o in F.OBJECTS if o.name=='Continuous coat and sleeves')
    for v in shirt.data.vertices:
        x,y,z=M.unity(v.co);blend=max(0,min(1,(abs(x)-.125)/.12));bend=max(0,min(1,(1.27-y)/.16))
        weights={n:w*(1-blend) for n,w in F.body_weights((x,y,z)).items()};side='Left' if x<0 else 'Right'
        weights[side+'UpperArm']=blend*(1-bend);weights[side+'LowerArm']=blend*bend
        for group in shirt.vertex_groups:
            if not group.name.startswith('VIS_'):group.remove([v.index])
        for name,w in weights.items():
            if w>.00001:(shirt.vertex_groups.get(name) or shirt.vertex_groups.new(name=name)).add([v.index],w,'REPLACE')
    # Trim the CC0 head's shoulder remnant below the covered scarf seam.
    import bmesh
    head=next(o for o in F.OBJECTS if o.name=='Anatomical fisherman head')
    bm=bmesh.new();bm.from_mesh(head.data)
    bmesh.ops.delete(bm,geom=[v for v in bm.verts if M.unity(v.co)[1]<1.537],context='VERTS');bm.to_mesh(head.data);bm.free()
    # Fit the hidden neck stump inside the scarf instead of leaving the source head's broad nape.
    for v in head.data.vertices:
        x,y,z=M.unity(v.co)
        blend=max(0,min(1,(1.590-y)/.043))
        if blend>0:
            radius=((x/.048)**2+(z/.046)**2)**.5
            scale=1/max(1,radius)
            v.co=M.u((x*(1-blend+blend*scale),y,z*(1-blend+blend*scale)))
    obj=F.join_and_unwrap();obj.name='HarborKeeper_Skin'
    images=F.bake(obj,resolution=1024);F.unity_material()
    # The old material template now has world textures. Replace all texture slots explicitly.
    mat=ART/'FishermanSurface.mat';s=mat.read_text()
    for prop,kind in [('_BaseMap','Albedo'),('_MainTex','Albedo'),('_BumpMap','Normal'),('_MetallicGlossMap','Smoothness')]:
        s=re.sub(r'(- '+prop+r':\n        m_Texture:) \{[^}]*\}',r'\1 {fileID: 2800000, guid: '+M.guid(ART/('Fisherman_'+kind+'.png'))+', type: 3}',s)
    s=re.sub(r'(- _OcclusionMap:\n        m_Texture:) \{[^}]*\}',r'\1 {fileID: 0}',s)
    s=s.replace('  - _OCCLUSIONMAP\n','').replace('- _Cull: 0','- _Cull: 2');mat.write_text('\n'.join(l.rstrip() for l in s.splitlines())+'\n')
    rig=F.skeleton();rig.name='HarborKeeper_Rig';facts=F.rig_checks(obj,rig)
    for name in ('TearL','TearR'):rig.pose.bones[name].scale=(.001,.001,.001)
    path=ART/'HarborKeeperRig.bytes';result=export_unity_skin(obj,rig,path);M.meta(path,'TextScriptImporter')
    # First-person mesh has no head or duplicated gloves. Same bone/pose solver, visual only.
    local=obj.copy();local.data=obj.data.copy();bpy.context.collection.objects.link(local)
    hidden={local.vertex_groups[n].index for n in ('VIS_Head','VIS_Hands')}
    excluded={v.index for v in local.data.vertices if any(g.group in hidden and g.weight>.99 for g in v.groups) or M.unity(v.co)[1]>1.45}
    bm=bmesh.new();bm.from_mesh(local.data);bm.verts.ensure_lookup_table()
    bmesh.ops.delete(bm,geom=[bm.verts[i] for i in excluded],context='VERTS');bm.to_mesh(local.data);bm.free()
    bm=bmesh.new();bm.from_mesh(local.data)
    # Capped belts/torso volumes must not become a dinner-plate in a downward local view.
    bmesh.ops.delete(bm,geom=[f for f in bm.faces if f.calc_center_median().z>.995 and abs(f.normal.z)>.8],context='FACES');bm.to_mesh(local.data);bm.free()
    body=ART/'HarborKeeperBody.bytes';export_unity_skin(local,rig,body);M.meta(body,'TextScriptImporter');bpy.data.objects.remove(local,do_unlink=True)
    facts.update(unityVertices=len(result['vertices'])//3,materials=1,textureSize=1024)
    (OUT/'model-checks.json').write_text(json.dumps(facts,indent=2));print(facts,flush=True)
    mat=ART/'FishermanSurface.mat'
    prefab=ROOT/'Assets/Prefabs/Avatars/PF_HarborKeeper.prefab'
    text=(ROOT/'Assets/Prefabs/Avatars/PF_RiggedFisherman.prefab').read_text().replace('PF_RiggedFisherman','PF_HarborKeeper')
    text=text.replace('8f87e5cf65da5a0989d9b2f904112177',M.guid(path)).replace('2ab83fbafb6454e18ccde64c4cbbbf6d',M.guid(mat))
    text+='  plantedFootRotation: 1\n  maximumLegStretch: 1.06\n'
    prefab.write_text(text);M.meta(prefab,'PrefabImporter')
    resource=ROOT/'Assets/Prefabs/Avatars/Resources';M.directory(resource)
    body_prefab=resource/'PF_HarborKeeperBody.prefab'
    body_prefab.write_text(text.replace('PF_HarborKeeper','PF_HarborKeeperBody').replace(M.guid(path),M.guid(body))+'  bodyBehindEyes: 0.18\n')
    M.meta(body_prefab,'PrefabImporter')
    for meta in ART.glob('*.png.meta'):
        s=meta.read_text();s=s.replace('maxTextureSize: 2048','maxTextureSize: 1024')
        # Android ASTC, mipmapped; normals use 4x4, colour/data use 6x6.
        fmt=48 if '_Normal.' in meta.name else 50
        block=f'  - serializedVersion: 3\n    buildTarget: Android\n    maxTextureSize: 1024\n    resizeAlgorithm: 0\n    textureFormat: {fmt}\n    textureCompression: 1\n    compressionQuality: 50\n    crunchedCompression: 0\n    allowsAlphaSplitting: 0\n    overridden: 1\n    androidETC2FallbackOverride: 0\n    forceMaximumCompressionQuality_BC6H_BC7: 0\n'
        s=re.sub(r'  - serializedVersion: \d+\n    buildTarget: Android\n.*?(?=  - serializedVersion:|  spriteSheet:)', '',s,flags=re.S);s=s.replace('  spriteSheet:',block+'  spriteSheet:');meta.write_text(s)
    studio(obj,rig,images)

if __name__=='__main__':main()
