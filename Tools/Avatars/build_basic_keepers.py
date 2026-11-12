"""Build original modular keeper meshes/prefabs with Blender 4.3+.

Run from the repository root:
  blender --background --factory-startup --python Tools/Avatars/build_basic_keepers.py
Add -- --preview to render an offline geometry preview under artifacts/Avatars.
The generated native Unity meshes need no Blender/FBX importer on teammates' machines.
This script never changes the network prefab's artist-selected model slots.
"""
from pathlib import Path
import math
import re
import struct
import sys
import uuid

import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / "Assets/Art/Avatars/BasicKeepers"
PREFABS = ROOT / "Assets/Prefabs/Avatars"
NAMESPACE = uuid.UUID("651de85e-3aef-453d-a83d-fc09e3651d1c")
LIT = "933532a4fcc9baf4fa0491de14d08ed7"
MODEL_SCRIPT = "31d0bc9a5083472bb200951a9d6a1470"
PALETTE = {
    "Skin": ((.62, .40, .27), 0, .3),
    "Beard": ((.12, .085, .065), 0, .15),
    "Ink": ((.025, .035, .05), 0, .25),
    "Navy": ((.035, .075, .15), 0, .25),
    "Cream": ((.88, .86, .78), 0, .3),
    "Brass": ((.83, .55, .16), .65, .5),
    "Oilskin": ((.95, .65, .025), 0, .48),
    "OilskinTrim": ((.7, .4, .018), 0, .4),
}


def guid(path):
    return uuid.uuid5(NAMESPACE, Path(path).relative_to(ROOT).as_posix()).hex


def meta(path, importer="NativeFormatImporter", main=0, folder=False):
    p = Path(str(path) + ".meta")
    if p.exists():
        assert re.search(r"^guid: (\w+)$", p.read_text(), re.M).group(1) == guid(path), p
        return
    value = "fileFormatVersion: 2\nguid: " + guid(path) + "\n"
    if folder:
        value += "folderAsset: yes\n"
    value += importer + ":\n  externalObjects: {}\n"
    if main:
        value += f"  mainObjectFileID: {main}\n"
    value += "  userData:\n  assetBundleName:\n  assetBundleVariant:\n"
    p.write_text(value)


def directory(path):
    if not path.exists():
        if not path.parent.exists():
            directory(path.parent)
        path.mkdir()
        meta(path, "DefaultImporter", folder=True)


def u(value):
    """Unity Y-up/+Z-forward to Blender Z-up/-Y-forward (a proper rotation)."""
    x, y, z = value
    return Vector((x, -z, y))


def unity(value):
    return (value.x, value.z, -value.y)


def vec(value):
    return "{x: %.8g, y: %.8g, z: %.8g}" % tuple(value)


def material(name):
    rgb, metallic, smoothness = PALETTE[name]
    linear = tuple(v/12.92 if v <= .04045 else ((v+.055)/1.055)**2.4 for v in rgb)
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*linear, 1)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*linear, 1)
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = 1 - smoothness
    path = ART / (name + ".mat")
    path.write_text(f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!21 &2100000
Material:
  serializedVersion: 8
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: BasicKeeper_{name}
  m_Shader: {{fileID: 4800000, guid: {LIT}, type: 3}}
  m_Parent: {{fileID: 0}}
  m_ModifiedSerializedProperties: 0
  m_ValidKeywords: []
  m_InvalidKeywords: []
  m_LightmapFlags: 4
  m_EnableInstancingVariants: 1
  m_DoubleSidedGI: 0
  m_CustomRenderQueue: -1
  stringTagMap: {{}}
  disabledShaderPasses: []
  m_LockedProperties:
  m_SavedProperties:
    serializedVersion: 3
    m_TexEnvs: []
    m_Ints: []
    m_Floats:
    - _Metallic: {metallic}
    - _Smoothness: {smoothness}
    m_Colors:
    - _BaseColor: {{r: {rgb[0]}, g: {rgb[1]}, b: {rgb[2]}, a: 1}}
  m_BuildTextureStacks: []
  m_AllowLocking: 1
""")
    meta(path, main=2100000)
    return m


def finish(obj, name, colour, smooth=False):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    obj.name = name
    obj.data.materials.clear()
    obj.data.materials.append(MATERIALS[colour])
    for face in obj.data.polygons:
        face.use_smooth = smooth
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return obj


def sphere(name, position, radii, colour, smooth=True):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=12, ring_count=8, location=u(position))
    obj = bpy.context.object
    obj.scale = (radii[0], radii[2], radii[1])
    return finish(obj, name, colour, smooth)


def box(name, position, size, colour):
    bpy.ops.mesh.primitive_cube_add(size=1, location=u(position))
    obj = bpy.context.object
    obj.scale = (size[0], size[2], size[1])
    return finish(obj, name, colour)


def mesh(name, vertices, faces, colour, smooth=False):
    data = bpy.data.meshes.new(name)
    data.from_pydata([u(v) for v in vertices], [], faces)
    data.update()
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    return finish(obj, name, colour, smooth)


def rings(name, levels, colour, segments=16, smooth=True):
    """Closed, elliptical coat/cap section; each level is (y, radiusX, radiusZ)."""
    vertices = [(rx*math.cos(a), y, rz*math.sin(a)) for y, rx, rz in levels
                for a in [2*math.pi*i/segments for i in range(segments)]]
    faces = []
    for row in range(len(levels)-1):
        for i in range(segments):
            a, b = row*segments+i, row*segments+(i+1)%segments
            faces.append((a, a+segments, b+segments, b))
    faces += [tuple(range(segments)), tuple((len(levels)-1)*segments+i for i in reversed(range(segments)))]
    return mesh(name, vertices, faces, colour, smooth)


def face():
    parts = [sphere("Face", (0,-.02,-.015), (.105,.145,.095), "Skin"),
             rings("Neck", [(-.29,.052,.05),(-.10,.052,.05)], "Skin",12),
             sphere("Nose", (0,-.015,.085), (.022,.032,.026), "Skin"),
             sphere("Short beard", (0,-.102,.052), (.082,.062,.052), "Beard")]
    for x in (-.043,.043):
        parts += [sphere("Eye", (x,.025,.073), (.024,.015,.012), "Cream"),
                  sphere("Pupil", (x,.025,.084), (.009,.011,.005), "Ink"),
                  box("Brow", (x,.052,.073), (.048,.012,.012), "Beard")]
    parts += [box("Mouth", (0,-.072,.103), (.047,.008,.006), "Skin")]
    return parts


def captain_head():
    parts = face()
    parts += [rings("Cap band", [(.095,.117,.105),(.13,.117,.105)], "Ink"),
              rings("White peaked crown", [(.13,.117,.105),(.16,.14,.12),(.215,.13,.11),(.23,.095,.085)], "Cream"),
              sphere("Cap visor", (0,.098,.105), (.147,.012,.125), "Ink"),
              sphere("Cap badge", (0,.175,.119), (.018,.024,.006), "Brass")]
    return parts


def fisherman_head():
    parts = face()
    segments, rows, opening = 20, 8, .8
    vertices, faces = [], []
    # The last ring stops short of the pole; a single pole avoids degenerate triangles.
    for row in range(rows):
        theta = opening + (math.pi-opening-.12)*row/(rows-1)
        for i in range(segments):
            phi = 2*math.pi*i/segments
            vertices.append((.15*math.sin(theta)*math.cos(phi),
                             -.01+.20*math.sin(theta)*math.sin(phi), -.025+.14*math.cos(theta)))
    for row in range(rows-1):
        for i in range(segments):
            a, b = row*segments+i, row*segments+(i+1)%segments
            faces.append((a,a+segments,b+segments,b))
    pole = len(vertices);vertices.append((0,-.01,-.165))
    for i in range(segments):
        faces.append(((rows-1)*segments+i,pole,(rows-1)*segments+(i+1)%segments))
    parts.append(mesh("Yellow rain hood",vertices,faces,"Oilskin",True))
    inner=[(x*.145/.15,-.01+(y+.01)*.195/.20,-.025+(z+.025)*.13/.14) for x,y,z in vertices]
    parts.append(mesh("Hood lining",inner,[tuple(reversed(f)) for f in faces],"OilskinTrim",True))
    # A narrow solid lip gives the face opening thickness from oblique views.
    lip, quads = [], []
    for layer in (0,1):
        for i in range(segments):
            phi=2*math.pi*i/segments
            lip.append(((.15-layer*.005)*math.sin(opening)*math.cos(phi),
                        -.01+(.20-layer*.005)*math.sin(opening)*math.sin(phi),
                        -.025+(.14-layer*.01)*math.cos(opening)))
    for i in range(segments):
        j=(i+1)%segments;quads.append((i,j,j+segments,i+segments))
    parts.append(mesh("Hood face opening",lip,quads,"OilskinTrim",True))
    return parts


def coat(captain):
    main = "Navy" if captain else "Oilskin"
    trim = "Brass" if captain else "OilskinTrim"
    parts = [rings("Captain coat" if captain else "Yellow oilskin coat",
                   [(-.62,.25,.15),(-.3,.225,.145),(.08,.255,.14),(.16,.19,.11),(.21,.075,.07)],main)]
    for side in (-1,1):
        parts.append(sphere("Shoulder",(side*.235,.075,0),(.085,.13,.105),main))
        parts.append(box("Pocket",(side*.135,-.36,.142),(.125,.13,.019),main))
        parts.append(box("Pocket flap",(side*.135,-.29,.155),(.14,.028,.017),trim))
        if captain:
            parts.append(box("Epaulette",(side*.215,.17,0),(.08,.018,.095),"Brass"))
    if captain:
        parts += [mesh("Cream shirt",[(-.055,.15,.16),(.055,.15,.16),(0,-.02,.16)],[(0,2,1)],"Cream"),
                  box("Coat belt",(0,-.26,.15),(.42,.035,.014),"Ink")]
        for x in (-.065,.065):
            for y in (.035,-.10,-.22):
                parts.append(sphere("Brass button",(x,y,.151),(.012,.012,.006),"Brass"))
    else:
        parts += [box("Rain flap",(0,-.20,.156),(.035,.73,.018),"OilskinTrim"),
                  box("Storm yoke",(0,.025,.145),(.40,.12,.022),"Oilskin")]
        for y in (.12,-.03,-.18,-.34,-.50):
            parts.append(sphere("Raincoat fastener",(0,y,.17),(.009,.009,.005),"Ink"))
    return parts


def native_mesh(name, objects, textured=False):
    """Export evaluated Blender triangles using the project's Unity 6 mesh format."""
    groups = {}
    for obj in objects:
        data = obj.data
        data.calc_loop_triangles()
        if textured:
            data.calc_tangents()
        world = obj.matrix_world
        normal_matrix = world.to_3x3().inverted().transposed()
        for tri in data.loop_triangles:
            colour = obj.data.materials[tri.material_index].name
            dest = groups.setdefault(colour, [])
            for loop in tri.loops:
                position = world @ data.vertices[data.loops[loop].vertex_index].co
                normal = (normal_matrix @ data.corner_normals[loop].vector).normalized()
                # Quantize harmless float noise from Blender's threaded normal evaluation.
                values = (*unity(position), *unity(normal))
                if textured:
                    tangent = (world.to_3x3() @ data.loops[loop].tangent).normalized()
                    if tangent.length_squared < .5:
                        # Tiny UV islands can yield a zero Mikk tangent. Keep the
                        # vertex basis valid without moving or dropping geometry.
                        axis = Vector((0,0,1)) if abs(normal.z)<.9 else Vector((0,1,0))
                        tangent = axis.cross(normal).normalized()
                    uv = data.uv_layers.active.data[loop].uv
                    values += (*unity(tangent),data.loops[loop].bitangent_sign,uv.x,uv.y)
                dest.append(tuple(0.0 if abs(v)<.000005 else round(v,5) for v in values))
    vertices, submeshes, materials = [], [], []
    for colour, points in sorted(groups.items()):
        # Blender/BMesh can enumerate faces in a different order between processes.
        # Rotate each triangle without changing its winding, then sort its key.
        triangles = [points[i:i+3] for i in range(0,len(points),3)]
        triangles = [min(triangle[i:]+triangle[:i] for i in range(3)) for triangle in triangles]
        points = [point for triangle in sorted(triangles) for point in triangle]
        first=len(vertices);vertices.extend(points)
        positions=[p[:3] for p in points]
        lo=[min(p[i] for p in positions) for i in range(3)];hi=[max(p[i] for p in positions) for i in range(3)]
        submeshes.append((first,len(points),[(a+b)/2 for a,b in zip(lo,hi)],[(b-a)/2 for a,b in zip(lo,hi)]))
        materials.append(colour)
    assert len(vertices)<65536
    packed=b"".join(struct.pack("<12f" if textured else "<6f",*v) for v in vertices)
    indices=struct.pack("<"+"H"*len(vertices),*range(len(vertices)))
    template=(ROOT/"Assets/WatchDemo/Radio/ConsoleBody0.asset").read_text()
    text=template.replace("m_Name: ConsoleBody0","m_Name: "+name)
    sub="  m_SubMeshes:\n"
    for start,count,center,extent in submeshes:
        sub+=f"""  - serializedVersion: 2
    firstByte: {start*2}
    indexCount: {count}
    topology: 0
    baseVertex: 0
    firstVertex: {start}
    vertexCount: {count}
    localAABB:
      m_Center: {vec(center)}
      m_Extent: {vec(extent)}
"""
    text=re.sub(r"  m_SubMeshes:.*?(?=  m_Shapes:)",sub,text,count=1,flags=re.S)
    text=re.sub(r"  m_IndexBuffer: [a-f0-9]+","  m_IndexBuffer: "+indices.hex(),text,count=1)
    vertex=f"  m_VertexData:\n    serializedVersion: 3\n    m_VertexCount: {len(vertices)}\n    m_Channels:\n"
    for i in range(14):
        dimension = 3 if i<2 else 4 if textured and i==2 else 2 if textured and i==4 else 0
        offset = 12 if i==1 else 24 if textured and i==2 else 40 if textured and i==4 else 0
        vertex+=f"    - stream: 0\n      offset: {offset}\n      format: 0\n      dimension: {dimension}\n"
    vertex+=f"    m_DataSize: {len(packed)}\n    _typelessdata: {packed.hex()}\n"
    text=re.sub(r"  m_VertexData:.*?(?=  m_CompressedMesh:)",vertex,text,count=1,flags=re.S)
    lo=[min(v[i] for v in vertices) for i in range(3)];hi=[max(v[i] for v in vertices) for i in range(3)]
    bounds=f"  m_LocalAABB:\n    m_Center: {vec([(a+b)/2 for a,b in zip(lo,hi)])}\n    m_Extent: {vec([(b-a)/2 for a,b in zip(lo,hi)])}\n"
    text=re.sub(r"  m_LocalAABB:.*?(?=  m_MeshUsageFlags:)",bounds,text,count=1,flags=re.S)
    lod="  m_MeshLodInfo:\n    serializedVersion: 2\n    m_LodSelectionCurve:\n      serializedVersion: 1\n      m_LodSlope: 0\n      m_LodBias: 0\n    m_NumLevels: 1\n    m_SubMeshes:\n"
    for start,count,*_ in submeshes:
        lod+=f"    - serializedVersion: 2\n      m_Levels:\n      - serializedVersion: 1\n        m_IndexStart: {start}\n        m_IndexCount: {count}\n"
    text=text.split("  m_MeshLodInfo:")[0]+lod
    path=ART/(name+".asset");path.write_text("\n".join(line.rstrip() for line in text.splitlines())+"\n");meta(path,main=4300000)
    print(name, "triangles", len(vertices)//3,"material slots",len(materials),flush=True)
    return path,materials


def gameobject(file, name, components):
    value=f"--- !u!1 &{file}\nGameObject:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {{fileID: 0}}\n  m_PrefabInstance: {{fileID: 0}}\n  m_PrefabAsset: {{fileID: 0}}\n  serializedVersion: 6\n  m_Component:\n"
    for component in components:value+=f"  - component: {{fileID: {component}}}\n"
    return value+f"  m_Layer: 0\n  m_Name: {name}\n  m_TagString: Untagged\n  m_Icon: {{fileID: 0}}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1\n"


def transform(file, owner, position, parent, children=()):
    value=f"--- !u!4 &{file}\nTransform:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {{fileID: 0}}\n  m_PrefabInstance: {{fileID: 0}}\n  m_PrefabAsset: {{fileID: 0}}\n  m_GameObject: {{fileID: {owner}}}\n  serializedVersion: 2\n  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}\n  m_LocalPosition: {vec(position)}\n  m_LocalScale: {{x: 1, y: 1, z: 1}}\n  m_ConstrainProportionsScale: 0\n"
    value+="  m_Children:\n"+"".join(f"  - {{fileID: {i}}}\n" for i in children) if children else "  m_Children: []\n"
    return value+f"  m_Father: {{fileID: {parent}}}\n  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}\n"


def prefab(name, head, body, eye_height=1.62):
    value="%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n"
    value+=gameobject(1000,name,(4000,114000))+transform(4000,1000,(0,0,0),0,(4001,4002))
    value+=f"""--- !u!114 &114000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1000}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {MODEL_SCRIPT}, type: 3}}
  m_Name:
  m_EditorClassIdentifier: LK.Net::LighthouseKeepers.Net.CrewAvatarModel
  head: {{fileID: 4001}}
  body: {{fileID: 4002}}
  bodyOffsetFromHead: {{x: 0, y: -0.48, z: 0}}
  leftHand: {{fileID: 0}}
  rightHand: {{fileID: 0}}
"""
    for i,label,data,y in [(1,"HeadAnchor",head,eye_height),(2,"BodyAnchor",body,eye_height-.48)]:
        path,materials=data
        value+=gameobject(1000+i,label,(4000+i,3300+i,2300+i))+transform(4000+i,1000+i,(0,y,0),4000)
        value+=f"--- !u!33 &{3300+i}\nMeshFilter:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {{fileID: 0}}\n  m_PrefabInstance: {{fileID: 0}}\n  m_PrefabAsset: {{fileID: 0}}\n  m_GameObject: {{fileID: {1000+i}}}\n  m_Mesh: {{fileID: 4300000, guid: {guid(path)}, type: 2}}\n"
        value+=f"--- !u!23 &{2300+i}\nMeshRenderer:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {{fileID: 0}}\n  m_PrefabInstance: {{fileID: 0}}\n  m_PrefabAsset: {{fileID: 0}}\n  m_GameObject: {{fileID: {1000+i}}}\n  m_Enabled: 1\n  m_CastShadows: 0\n  m_ReceiveShadows: 1\n  m_DynamicOccludee: 1\n  m_StaticShadowCaster: 0\n  m_MotionVectors: 1\n  m_LightProbeUsage: 1\n  m_ReflectionProbeUsage: 1\n  m_RayTracingMode: 0\n  m_RenderingLayerMask: 1\n  m_RendererPriority: 0\n  m_Materials:\n"
        for m in materials:value+=f"  - {{fileID: 2100000, guid: {guid(ART/(m+'.mat'))}, type: 2}}\n"
        value+="  m_StaticBatchInfo:\n    firstSubMesh: 0\n    subMeshCount: 0\n  m_StaticBatchRoot: {fileID: 0}\n  m_ProbeAnchor: {fileID: 0}\n  m_LightProbeVolumeOverride: {fileID: 0}\n  m_ScaleInLightmap: 1\n  m_ReceiveGI: 1\n  m_PreserveUVs: 0\n  m_IgnoreNormalsForChartDetection: 0\n  m_ImportantGI: 0\n  m_StitchLightmapSeams: 1\n  m_SelectedEditorRenderState: 3\n  m_MinimumChartSize: 4\n  m_AutoUVMaxDistance: 0.5\n  m_AutoUVMaxAngle: 89\n  m_LightmapParameters: {fileID: 0}\n  m_SortingLayerID: 0\n  m_SortingLayer: 0\n  m_SortingOrder: 0\n  m_AdditionalVertexStreams: {fileID: 0}\n"
    path=PREFABS/(name+".prefab");path.write_text(value);meta(path,"PrefabImporter")


def preview(groups):
    out=ROOT/"artifacts/Avatars";out.mkdir(parents=True,exist_ok=True)
    for x,(head,body) in zip((-.43,.43),groups):
        for obj in head:obj.location+=u((x,1.62,0))
        for obj in body:obj.location+=u((x,1.14,0))
    scene=bpy.context.scene
    scene.render.engine="CYCLES";scene.cycles.device="CPU";scene.cycles.samples=24;scene.cycles.use_denoising=False
    scene.render.resolution_x=1280;scene.render.resolution_y=1080;scene.render.resolution_percentage=100
    scene.world.color=(.22,.22,.22)
    bpy.ops.object.camera_add(location=u((2.2,1.9,4.6)))
    camera=bpy.context.object;camera.rotation_euler=(u((0,1.2,0))-camera.location).to_track_quat('-Z','Y').to_euler()
    camera.data.type="ORTHO";camera.data.ortho_scale=2.15;scene.camera=camera
    for position,power,size in [((-2,3,3),450,4),((2,2,1),220,3),((0,3,-2),300,2)]:
        bpy.ops.object.light_add(type="AREA",location=u(position));light=bpy.context.object
        light.data.energy=power;light.data.shape="DISK";light.data.size=size
        light.rotation_euler=(u((0,1.3,0))-light.location).to_track_quat('-Z','Y').to_euler()
    scene.render.image_settings.file_format="PNG";scene.render.filepath=str(out/"BasicKeepers-preview.png")
    bpy.ops.wm.save_as_mainfile(filepath=str(out/"BasicKeepers.blend"))
    bpy.ops.render.render(write_still=True)


if __name__ == "__main__":
    bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
    directory(ART);directory(PREFABS)
    MATERIALS={name:material(name) for name in PALETTE}
    captain=(captain_head(),coat(True));fisherman=(fisherman_head(),coat(False))
    for name,(head,body) in [("PF_BasicCaptain",captain),("PF_BasicFisherman",fisherman)]:
        prefab(name,native_mesh(name+"_Head",head),native_mesh(name+"_Body",body))
    if "--preview" in sys.argv:preview([captain,fisherman])
