from pathlib import Path
import subprocess,json,zipfile,os,hashlib,shutil
r=Path.cwd();out=r/'artifacts/s5/playtest-feedback/build-delivery';receipt=json.loads((out/'build-result.json').read_text());apk=Path(receipt['apk']);assert receipt['exit']==0
sdk=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/PlaybackEngines/AndroidPlayer/SDK');jdk=sdk.parent/'OpenJDK';adb=sdk/'platform-tools/adb';bt=sdk/'build-tools/36.0.0'
def run(args,check=True,env=None):return subprocess.run(list(map(str,args)),check=check,capture_output=True,text=True,env=env)
badging=run([bt/'aapt','dump','badging',apk]).stdout;(out/'apk-badging.txt').write_text(badging)
assert "package: name='com.lighthousekeepers.game'" in badging
assert "native-code: 'arm64-v8a'" in badging
manifest=run([bt/'aapt','dump','xmltree',apk,'AndroidManifest.xml']).stdout;assert 'com.oculus.intent.category.VR' in manifest
(out/'apk-manifest.txt').write_text(manifest)
env=dict(os.environ,JAVA_HOME=str(jdk));signed=run([bt/'apksigner','verify','--verbose',apk],env=env);(out/'apk-signature.txt').write_text(signed.stdout)
with zipfile.ZipFile(apk) as z:assert z.testzip() is None
verification={'package':'com.lighthousekeepers.game','arm64':True,'vr_category':True,'signature':True,'zip_crc':True}
receipt.update(installed=False,launched=False,validation=verification,network='LK-S5-CLEARANCE1',transport='LK-Lab-Coop-v43')
attached=run([adb,'devices','-l']).stdout;(out/'adb-devices.txt').write_text(attached)
serial='2G0YC1ZG32048T'
assert serial+'\t' in attached or serial+' ' in attached
print('APK verified; installing without launch',flush=True)
installed=run([adb,'-s',serial,'install','-r',apk]);(out/'install.txt').write_text(installed.stdout+installed.stderr);assert 'Success' in installed.stdout
package=run([adb,'-s',serial,'shell','pm','path','com.lighthousekeepers.game']).stdout.strip();assert package.startswith('package:');base=package.splitlines()[0][8:]
device_hash=run([adb,'-s',serial,'shell','sha256sum',base]).stdout.split()[0];assert device_hash==receipt['sha256']
receipt.update(installed=True,installed_sha256=device_hash,device='Quest 3',launched=False)
(out/'delivery.json').write_text(json.dumps(receipt,indent=2));print(json.dumps(receipt),flush=True)
# Save only the documented build-setting churn before restoring the exact source tip.
for f in ['Assets/Settings/UniversalRenderPipelineGlobalSettings.asset','Assets/_LighthouseKeepers/Settings/RenderPipeline/QuestPipeline.asset','ProjectSettings/ProjectSettings.asset']:
 p=out/'generated-settings'/f;p.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(r/f,p)
 shutil.copy2(out/'build-backup'/f,r/f)
(out/'post-build-diff.patch').write_bytes(subprocess.check_output(['git','diff']))
