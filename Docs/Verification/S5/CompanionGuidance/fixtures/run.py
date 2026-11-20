import json,sys,time
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop')
from mac_pair import call
root=Path.cwd();out=Path(sys.argv[1]).resolve();out.mkdir(parents=True,exist_ok=False);started=False
def ev(code):return call(root,'eval',{'code':code})['result']
def wait(test,what,seconds=90):
 end=time.monotonic()+seconds
 while time.monotonic()<end:
  try:
   if test():return
  except Exception:pass
  time.sleep(.3)
 raise RuntimeError('Timed out: '+what)
try:
 wait(lambda:ev('UnityEditor.EditorUtility.audioMasterMute=true;return !UnityEditor.EditorApplication.isCompiling&&!UnityEditor.EditorUtility.scriptCompilationFailed;'),'compile')
 if ev('return UnityEditor.EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty;'):
  raise RuntimeError('Stop Play Mode and save or preserve the dirty scene before running this fixture')
 wait(lambda:ev('UnityEditor.EditorUtility.audioMasterMute=true;if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Lab/LK_Lab_Environment.unity")UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Lab/LK_Lab_Environment.unity");return true;'),'scene')
 call(root,'editor_play')
 started=True
 wait(lambda:ev('UnityEditor.EditorUtility.audioMasterMute=true;return UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.CrewCompanion>()!=null;'),'watch')
 ev('var b=UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.InWorldBriefing>();if(b)b.Folder.Skip();return true;')
 fixture=root/'Docs/Verification/S5/CompanionGuidance/fixtures/GuideCheck.cs'
 print(call(root,'run_script',{'file':str(fixture),'entry':'GuideCheck.Begin','args':[str(out)]}),flush=True)
 wait(lambda:(out/'result.json').exists(),'guidance',120)
 result=json.loads((out/'result.json').read_text());print((out/'checks.txt').read_text(),flush=True)
 if not result['passed']:raise RuntimeError('Guidance check failed')
finally:
 if started:
  try:call(root,'editor_stop')
  except Exception:pass
