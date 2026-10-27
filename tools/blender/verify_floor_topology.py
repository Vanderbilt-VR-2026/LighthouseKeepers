import bpy,bmesh,json
from pathlib import Path
result=[]
for floor in ['F1','F2','F3','F4']:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 bpy.ops.import_scene.fbx(filepath=str(Path('Assets/Art/Generated/SM_Shell_'+floor+'.fbx').resolve()))
 obj=bpy.data.objects['SM_Floor_'+floor]
 bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=1e-6)
 assert all(len(e.link_faces)==2 for e in bm.edges),floor+' has non-manifold/internal faces'
 unseen=set(bm.verts);components=0
 while unseen:
  components+=1;pending=[unseen.pop()]
  while pending:
   v=pending.pop()
   for edge in v.link_edges:
    other=edge.other_vert(v)
    if other in unseen:unseen.remove(other);pending.append(other)
 assert components==1,floor+' is disconnected wedges'
 bm.transform(obj.matrix_world)
 volume=abs(bm.calc_volume());assert volume>1,floor+' has no solid volume'
 result.append({'floor':floor,'manifold':True,'connectedComponents':components,'volumeM3':volume})
 bm.free()
Path('artifacts/batch-a/floor-topology.json').write_text(json.dumps(result,indent=2))
print('FLOOR_TOPOLOGY_PASS',result)
