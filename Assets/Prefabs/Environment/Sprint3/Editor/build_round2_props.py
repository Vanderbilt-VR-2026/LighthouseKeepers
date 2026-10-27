"""Headless metre-scale, <=2 materials per prop; Unity authoring binds these to named anchors."""
import bpy, math, pathlib
from mathutils import Vector
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
root=pathlib.Path.cwd()/ 'Assets/Art/Sprint3/Round2'
root.mkdir(parents=True,exist_ok=True)
def mat(name,c):
 m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);return m
iron=mat('Ship Iron',(.055,.065,.07));wood=mat('Ship Oak',(.26,.12,.045));red=mat('Ship Valve Red',(.45,.025,.015));paper=mat('Ship Chart Ivory',(.7,.66,.49));amber=mat('Ship Amber',(1,.70,.28))
parts=[]
def box(name,p,s,m):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=name;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 b=o.modifiers.new('Worked edges','BEVEL');b.width=.012;b.segments=1;bpy.ops.object.modifier_apply(modifier=b.name);o.data.materials.append(m);parts.append(o);return o
def cyl(name,p,r,depth,m,rot=(0,0,0)):
 bpy.ops.mesh.primitive_cylinder_add(vertices=16,radius=r,depth=depth,location=p,rotation=rot);o=bpy.context.object;o.name=name;o.data.materials.append(m);parts.append(o);return o
def export(name):
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
 bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
 bpy.ops.export_scene.fbx(filepath=str(root/(name+'.fbx')),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True,add_leaf_bones=False,bake_anim=False)
 bpy.data.objects.remove(o,do_unlink=True);parts.clear()
# Blender Z is Unity Y; front/operator is Blender -Y.
box('Desk top',(0,-.3,.79),(1.55,.65,.07),wood)
for x in [-.66,.66]:
 for y in [-.08,-.53]:box('Desk leg',(x,y,.38),(.075,.075,.76),wood)
box('Radio chassis',(-.20,-.29,1.0),(.65,.36,.35),iron)
for x in [-.43,-.1]:cyl('Tuning dial',(x,-.48,1.0),.065,.035,wood,(math.pi/2,0,0))
for i in range(7):box('Speaker grille',(.045,-.478,.9+i*.029),(.15,.012,.009),wood)
cyl('Aerial',(-.48,-.18,1.43),.009,.56,iron)
export('SM_Round2_RadioDesk')
box('Chart backing',(0,0,1.50),(1.12,.04,.76),iron)
box('Forecast paper',(0,-.026,1.50),(1.04,.008,.68),paper)
for i in range(5):box('Latitude',(0,-.032,1.25+i*.12),(.95,.003,.004),iron)
for i in range(6):box('Longitude',(-.44+i*.175,-.032,1.5),(.004,.003,.60),iron)
export('SM_Round2_StormChart')
# Wheel axle faces operator, on a toroidal rim.
bpy.ops.mesh.primitive_torus_add(major_segments=24,minor_segments=6,location=(0,0,0),rotation=(math.pi/2,0,0),major_radius=.16,minor_radius=.018)
o=bpy.context.object;o.name='Valve rim';o.data.materials.append(red);parts.append(o)
cyl('Wheel hub',(0,0,0),.045,.10,iron,(math.pi/2,0,0))
for a in [0,math.pi/3,2*math.pi/3]:
 o=box('Wheel spoke',(0,0,0),(.30,.026,.023),red);o.rotation_euler.y=a
export('SM_Round2_RedValve')
cyl('Pedestal',(0,0,.48),.22,.96,iron)
cyl('Lantern base',(0,0,1.0),.43,.10,iron)
cyl('Fresnel amber lens',(0,0,1.39),.30,.64,amber)
for z in [1.10,1.20,1.30,1.40,1.50,1.60,1.70]:cyl('Lens prism band',(0,0,z),.315,.024,amber)
cyl('Lantern crown',(0,0,1.78),.43,.13,iron)
for x in [-.35,.35]:
 for y in [-.2,.2]:cyl('Cage upright',(x,y,1.40),.025,.75,iron)
export('SM_Round2_Lantern')
# Wall tool rack with recognizable hanging spanners and mallet.
box('Rack',(0,0,1.48),(1.7,.07,.13),wood)
for x in [-.62,-.25,.12]:
 box('Spanner shaft',(x,-.08,1.15),(.035,.025,.40),iron)
 for dx in [-.035,.035]:box('Open jaw',(x+dx,-.08,1.37),(.026,.035,.10),iron)
box('Mallet shaft',(.53,-.08,1.20),(.04,.035,.45),wood);box('Mallet head',(.53,-.08,1.43),(.24,.09,.08),iron)
export('SM_Round2_ToolRack')
print('ROUND2_PROPS_EXPORTED')
