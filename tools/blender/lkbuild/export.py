import bpy, pathlib

def export(path):
 pathlib.Path(path).parent.mkdir(parents=True,exist_ok=True)
 bpy.ops.object.select_all(action='SELECT')
 for ob in bpy.context.selected_objects:
  if ob.type=='MESH':
   bpy.context.view_layer.objects.active=ob;bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'MESH','EMPTY'},global_scale=1.,apply_unit_scale=True,apply_scale_options='FBX_SCALE_NONE',axis_forward='-Z',axis_up='Y',bake_space_transform=True,use_mesh_modifiers=True,mesh_smooth_type='FACE',add_leaf_bones=False,path_mode='AUTO',bake_anim=False)
