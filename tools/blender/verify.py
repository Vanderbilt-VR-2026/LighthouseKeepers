import math
from lkbuild.naming import valid

def validate(objects,spec):
 errors=[];tris=0;names={o.name for o in objects};records=[]
 for o in objects:
  if not valid(o.name):errors.append('Invalid name '+o.name)
  if o.type!='MESH':continue
  if any(abs(x)>1e-6 for x in o.location) or any(abs(x)>1e-6 for x in o.rotation_euler) or any(abs(x-1)>1e-6 for x in o.scale):errors.append('Unapplied transform '+o.name)
  count=sum(len(p.vertices)-2 for p in o.data.polygons)
  if any(len(p.vertices)>4 for p in o.data.polygons):errors.append('N-gon '+o.name)
  if len(o.data.materials)>spec['materials_max']:errors.append('Material budget '+o.name)
  if not o.name.startswith('UCX_'):
   tris+=count
   if 'UCX_'+o.name+'_01' not in names:errors.append('No collision proxy '+o.name)
  uv=o.data.uv_layers.active
  if uv and any(min(v.uv)<-1e-5 or max(v.uv)>1.00001 for v in uv.data):errors.append('UV outside 0–1 '+o.name)
  records.append({'name':o.name,'triangles':count,'materials':[m.name for m in o.data.materials],'collisionEnabled':bool(o.get('collision_enabled',True)),'staticCollision':bool(o.get('static_collision',False)),'collisionOnly':bool(o.get('collision_only',False)) or 'Safety' in o.name or '_Ramp' in o.name})
 if tris>spec['tri_budget_lod0']:errors.append('Family triangle budget exceeded '+str(tris))
 if errors:raise RuntimeError('\n'.join(errors))
 return records,tris
