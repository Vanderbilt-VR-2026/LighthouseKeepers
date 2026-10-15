import sys,pathlib,argparse,json,hashlib,bpy
ROOT=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'tools/blender'))
from assets import shell
from lkbuild.export import export
from verify import validate
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
p=argparse.ArgumentParser();p.add_argument('--out',default='Assets/Art/Generated');p.add_argument('--manifest',default='artifacts/art-manifest.json');p.add_argument('--greybox',action='store_true');p.add_argument('--only',choices=['House','F1','F2','F3','F4','Exterior','ScaleReference']);a=p.parse_args(args)
layout=json.loads((ROOT/'Assets/Data/Layout/lighthouse-layout.json').read_text());manifest={'version':1,'layoutVersion':layout['version'],'sourceHash':hashlib.sha256((ROOT/'Assets/Data/Layout/lighthouse-layout.json').read_bytes()).hexdigest(),'units':'metres','greybox':a.greybox,'reusedThirdParty':'None copied; existing vendor and legacy scenes remain untouched.','assets':[]}
if a.only:manifest['assets']=json.loads((ROOT/a.out/'ArtManifest.json').read_text())['assets']
for name in ([a.only] if a.only else ['House','F1','F2','F3','F4','Exterior','ScaleReference']):
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);bpy.context.scene.unit_settings.system='METRIC';bpy.context.scene.unit_settings.scale_length=1.
 if name=='House':shell.house(layout)
 elif name=='Exterior':shell.exterior(layout)
 elif name=='ScaleReference':shell.reference()
 else:shell.room(layout,next(f for f in layout['floors'] if f['id']==name))
 path=ROOT/a.out/('SM_Shell_'+name+'.fbx');export(path);records,tris=validate(list(bpy.context.scene.objects),shell.SPEC)
 manifest['assets']=[entry for entry in manifest['assets'] if entry['scene']!=name]
 manifest['assets'].append({'path':path.relative_to(ROOT).as_posix(),'family':'Shell','scene':name,'triangles':tris,'spec':shell.SPEC,'objects':records,'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
 print('VERIFIED_BLENDER',name,tris)
manifest['assets'].sort(key=lambda entry:entry['scene'])
if sum(entry['triangles'] for entry in manifest['assets'])>90000:raise RuntimeError('Combined shell budget exceeds90000 triangles')
mp=ROOT/a.manifest;mp.parent.mkdir(parents=True,exist_ok=True);mp.write_text(json.dumps(manifest,indent=2)+'\n')
# Tracked import manifest is required for fresh clones; artifacts is deliberately ignored by Git.
(ROOT/a.out/'ArtManifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('BLENDER_BATCH_A_VERIFIED')
