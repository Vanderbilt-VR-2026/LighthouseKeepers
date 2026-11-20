import sys,time,json,math,os
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop')
from mac_pair import call,status
R=Path.cwd();P=Path('/Users/aneeshvasamreddy/LighthouseKeepers-S5-Peer');O=Path(os.environ['LK_TEST_OUT']);O.mkdir(parents=True,exist_ok=False)
def ev(r,s):
 v=call(r,'eval',{'code':s})
 if not v.get('success'):raise RuntimeError(str(v.get('diagnostics')))
 return v['result']
def wait(f,label,seconds=35):
 end=time.monotonic()+seconds
 while time.monotonic()<end:
  try:
   if f():return
  except (OSError,RuntimeError,ValueError,KeyError):pass
  time.sleep(.2)
 raise RuntimeError(label)
def click(r,n):return ev(r,'UnityEngine.GameObject.Find("'+n+'").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;')
def scene(r,m,args=[]):
 x=call(r,'run_script',{'file':str(R/'Docs/Verification/S5/NightmareFoundation/Coop/fixtures/TwoPeer.cs'),'entry':'TwoPeer.'+m,'args':args})
 if not x.get('success'):raise RuntimeError(str(x.get('diagnostics')))
 return x['result']
def log(label,items):
 print(label,flush=True)
 with (O/'events.jsonl').open('a') as f:f.write(json.dumps({'event':label,'data':items})+'\n')
def check(ok,label):
 if not ok:raise AssertionError(label)
 log('PASS '+label,[scene(R,'Facts'),scene(P,'Facts')])
try:
 for r in [R,P]:
  ev(r,'UnityEditor.EditorUtility.audioMasterMute=true;UnityEditor.AssetDatabase.Refresh();return true;');time.sleep(2)
  wait(lambda:ev(r,'UnityEditor.EditorUtility.audioMasterMute=true;UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=72;return !UnityEditor.EditorApplication.isCompiling&&!UnityEditor.EditorUtility.scriptCompilationFailed;'),'compile')
  if ev(r,'return UnityEditor.EditorApplication.isPlaying;'):ev(r,'UnityEditor.EditorApplication.isPlaying=false;return true;');time.sleep(2)
  ev(r,'LighthouseKeepers.Net.CrewLobbyTransport.ForceOfflineForTests=false;UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity");return true;');call(r,'editor_play');wait(lambda:ev(r,'UnityEditor.EditorUtility.audioMasterMute=true;return UnityEngine.GameObject.Find("Play")!=null;'),'lobby')
 click(R,'Play');click(R,'Campaign');click(R,'HostPlay');wait(lambda:status(R)['online'],'host');code=status(R)['code']
 click(P,'Play');click(P,'Campaign');click(P,'JoinChoice');ev(P,'UnityEngine.GameObject.Find("CrewCodeInput").GetComponent<TMPro.TMP_InputField>().text="'+code+'";return true;');click(P,'JoinCrewCode');wait(lambda:len(status(R)['players'])==2,'join');ev(R,'LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=2;return true;');click(R,'StartCrew');wait(lambda:bool(status(R)['snapshot']) and bool(status(P)['snapshot']),'start')
 for r in [R,P]:ev(r,'UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.InWorldBriefing>().Folder.Skip();LighthouseKeepers.Core.Progression.HorrorTuning.Current.nightmareEnabled=true;return true;')
 wait(lambda:not ev(R,'return UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchDemoController>().State.BriefingHeld;'),'briefing')
 ev(R,'return UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.NightmareScene>().Begin(true);')
 wait(lambda:scene(P,'Facts')['beat']==1,'guest wake')
 scene(R,'Pose',['near1','child']);scene(P,'Pose',['far','away']);time.sleep(4.8)
 check(scene(R,'Facts')['beat']==1,'Wake waits for both players to look')
 scene(P,'Look',['child']);wait(lambda:scene(R,'Facts')['beat']==2,'both gaze starts follow',15)
 before=scene(R,'Facts')['body'];time.sleep(1.5);after=scene(R,'Facts')['body'];check(before==after,'Child waits for the far keeper')
 scene(P,'Pose',['near2','child']);wait(lambda:scene(R,'Facts')['body']!=after,'child moves after both approach',15);log('PASS Both keepers see child resume',[scene(R,'Facts'),scene(P,'Facts')])
 scene(R,'StageEscape');wait(lambda:scene(P,'Facts')['beat']==7,'guest escape snapshot');scene(R,'Pose',['outside','away']);scene(P,'Pose',['victim','away']);time.sleep(1.2);scene(R,'EnableScene')
 wait(lambda:scene(P,'Facts')['localDown'],'actual host pursuit catches guest',15);wait(lambda:scene(R,'Facts')['beat']==8,'escaped host enters return',8);log('PASS Actual pursuit catch and return',[scene(R,'Facts'),scene(P,'Facts')])
 scene(R,'PrepareRescue');time.sleep(.5);scene(R,'Press');wait(lambda:scene(R,'Held'),'host grip',5);time.sleep(.7);scene(R,'Release');time.sleep(.5);first=scene(R,'Facts')['otherProgress'];time.sleep(.6);later=scene(R,'Facts')['otherProgress'];check(first>.04 and abs(first-later)<.04,'Release pauses rescue while preserving progress')
 scene(R,'Press');wait(lambda:scene(R,'Held'),'regrip',5);wait(lambda:not scene(P,'Facts')['localDown'],'guest revived',12);log('PASS Regrip completes rescue on both clients',[scene(R,'Facts'),scene(P,'Facts')]);scene(R,'Release')
 (O/'result.json').write_text(json.dumps({'passed':True,'scope':'Live Photon two Editors; real gaze/wait, host AI catch, host XR grip release and regrip. Poses and scene progression were staged; no full two-player locomotion run.'},indent=2))
except Exception as e:
 print('FAILED',type(e).__name__,str(e),flush=True)
 (O/'result.json').write_text(json.dumps({'passed':False,'error':str(e)},indent=2));raise
