import sys,time,json
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call
p=Path.cwd();o=p/'artifacts/s5/arrival/tests-final';o.mkdir(exist_ok=True)
try:call(p,'eval',{'code':'UnityEditor.EditorUtility.audioMasterMute=true;UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);return true;'})
except RuntimeError:pass
time.sleep(2)
for n in range(60):
 try:
  a=call(p,'eval',{'code':'UnityEditor.EditorUtility.audioMasterMute=true;UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=72;return new[]{UnityEditor.EditorApplication.isCompiling.ToString(),UnityEditor.EditorUtility.scriptCompilationFailed.ToString()};'})['result']
  if a==['False','False']:break
 except Exception:pass
 time.sleep(1)
else:raise RuntimeError('Editor is not compiling clean source')
for name in sys.argv[1:]:
 x=call(p,'run_tests',{'mode':'editor','filter':name,'filter_type':'testName'});(o/(name+'.json')).write_text(json.dumps(x,indent=2));print(name,x.get('Summary'),flush=True)
 assert (name!='ArrivalBriefingTests' or x.get('Summary',{}).get('Total')==8) and x.get('Summary',{}).get('Total',0)>0 and x['Summary']['Failed']==0,x
