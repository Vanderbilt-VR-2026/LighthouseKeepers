import bpy,math
from .palette import COLORS
# Author all positions in Unity XYZ metres; Blender uses Z up and -Y forward.
def bv(p):return (-p[0],-p[2],p[1])
def polar(a,r,y):
 a=math.radians(a);return (r*math.sin(a),y,r*math.cos(a))
def material(surface):
 name='M_Environment_'+surface;m=bpy.data.materials.get(name)
 if not m:
  m=bpy.data.materials.new(name);h=COLORS[surface];m.diffuse_color=tuple(int(h[i:i+2],16)/255 for i in (0,2,4))+(1,)
 return m

def mesh(name,verts,faces,surface='Stone',collision=True,static_collision=False):
 name=name.replace('-', 'N')
 data=bpy.data.meshes.new(name);data.from_pydata([bv(v) for v in verts],[],faces);data.update()
 import bmesh
 bm=bmesh.new();bm.from_mesh(data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(data);bm.free()
 obj=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(obj);obj.data.materials.append(material(surface))
 # UVs are local planar projections, bounded 0-1. Trim detail is a later art pass.
 uv=data.uv_layers.new(name='UVMap');mins=[min(v[k] for v in verts) for k in range(3)];maxs=[max(v[k] for v in verts) for k in range(3)]
 for poly in data.polygons:
  axes=sorted(range(3),key=lambda k:maxs[k]-mins[k],reverse=True)[:2]
  for li in poly.loop_indices:
   v=verts[data.loops[li].vertex_index];uv.data[li].uv=tuple((v[k]-mins[k])/max(.00001,maxs[k]-mins[k]) for k in axes)
 obj['collision_enabled']=collision
 if True:
  c=obj.copy();c.data=data.copy();c.name='UCX_'+name+'_01';c.data.name=c.name;c['static_collision']=static_collision;c['collision_enabled']=collision;bpy.context.collection.objects.link(c)
 return obj

def box(name,center,size,surface='Stone',yaw=0,collision=True):
 c,s=math.cos(math.radians(yaw)),math.sin(math.radians(yaw));vs=[]
 for x,y,z in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]:
  x,y,z=x*size[0]/2,y*size[1]/2,z*size[2]/2;vs.append((center[0]+c*x+s*z,center[1]+y,center[2]-s*x+c*z))
 return mesh(name,vs,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(3,7,6,2),(0,4,7,3),(1,2,6,5)],surface,collision)

def arc(name,inner,outer,y0,y1,a0,a1,surface='Stone',segments=None,collision=True):
 n=segments or max(1,int(abs(a1-a0)/4));verts=[]
 for i in range(n+1):
  a=a0+(a1-a0)*i/n
  for r,y in [(inner,y0),(outer,y0),(inner,y1),(outer,y1)]:verts.append(polar(a,r,y))
 faces=[]
 for i in range(n):
  q=i*4;k=q+4;faces.extend([(q,k,k+1,q+1),(q+2,q+3,k+3,k+2),(q,q+2,k+2,k),(q+1,k+1,k+3,q+3)])
 faces += [(0,1,3,2),(n*4,n*4+2,n*4+3,n*4+1)]
 return mesh(name,verts,faces,surface,collision,True)

def rod(name,a,b,r=.0225,surface='Metal',sides=8,collision=True):
 from mathutils import Vector
 start,end=Vector(a),Vector(b);axis=(end-start).normalized();u=axis.cross(Vector((0,1,0)))
 if u.length<.01:u=axis.cross(Vector((1,0,0)))
 u.normalize();v=axis.cross(u).normalized();verts=[]
 for p in [start,end]:
  for i in range(sides):verts.append(tuple(p+r*(u*math.cos(i*2*math.pi/sides)+v*math.sin(i*2*math.pi/sides))))
 faces=[(i,(i+1)%sides,(i+1)%sides+sides,i+sides) for i in range(sides)]
 for i in range(1,sides-1):faces.extend([(0,i+1,i),(sides,sides+i,sides+i+1)])
 return mesh(name,verts,faces,surface,collision)

def empty(name,pos):
 ob=bpy.data.objects.new(name,None);ob.location=bv(pos);bpy.context.collection.objects.link(ob);return ob
