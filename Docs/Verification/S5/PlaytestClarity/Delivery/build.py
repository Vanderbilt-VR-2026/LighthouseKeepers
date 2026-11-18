from pathlib import Path
import subprocess,os,json,hashlib,shutil
r=Path.cwd();o=r/'artifacts/s5/playtest-feedback/build-delivery';sha=subprocess.check_output(['git','rev-parse','--short=8','HEAD'],text=True).strip()
if subprocess.check_output(['git','status','--porcelain'],text=True).strip():raise RuntimeError('Build requires clean source')
b=o/'build-backup';b.mkdir(exist_ok=False)
files=['Assets/Settings/UniversalRenderPipelineGlobalSettings.asset','Assets/_LighthouseKeepers/Settings/RenderPipeline/QuestPipeline.asset','ProjectSettings/ProjectSettings.asset']
for f in files:
 p=b/f;p.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(r/f,p)
apk=r/'Builds'/('LighthouseKeepers-Sprint5-Clarity-'+sha+'.apk');env=os.environ.copy();env['LK_APK_OUTPUT']=str(apk)
args=['/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity','-batchmode','--burst-disable-compilation','-quit','-projectPath',str(r),'-buildTarget','Android','-executeMethod','LighthouseKeepers.Net.Editor.MultiplayerBuild.BuildQuest','-logFile',str(o/'build.log')]
code=subprocess.call(args,env=env)
result={'commit':sha,'exit':code,'apk':str(apk),'size':apk.stat().st_size if apk.exists() else 0,'sha256':hashlib.sha256(apk.read_bytes()).hexdigest() if apk.exists() else None}
(o/'build-result.json').write_text(json.dumps(result,indent=2));print(json.dumps(result),flush=True)
if code:raise SystemExit(code)
