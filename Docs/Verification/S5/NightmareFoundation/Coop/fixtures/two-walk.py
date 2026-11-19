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
 ev(R,'var n=UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.NightmareScene>();var started=n.Begin(true);n.enabled=false;return started;')
 wait(lambda:scene(P,'Facts')['beat']==1,'shared wake')
 def driver(root,which):
  x=call(root,'run_script',{'file':str(R/'Docs/Verification/S5/NightmareFoundation/Coop/fixtures/CoopWalkPeer.cs'),'entry':'CoopWalkPeer.Begin','args':[str(O/which),which]})
  if not x.get('success'):raise RuntimeError(str(x.get('diagnostics')))
 driver(R,'lead');driver(P,'follow')
 wait(lambda:(O/'lead'/'ready.txt').exists() and (O/'follow'/'ready.txt').exists(),'movement fixtures ready')
 ev(R,'var w=UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchDemoController>();var c=w.State.Scare;for(int i=0;i<200&&c.Beat!=LighthouseKeepers.Core.Progression.ScareBeat.Escape;i++)c.Tick(.1f,true,true,0,0);c.Place(new UnityEngine.Vector3(.79f,3.33f,-2.97f),0);w.GetComponent<LighthouseKeepers.WatchDemo.NightmareScene>().enabled=true;return true;')
 wait(lambda:(O/'lead'/'result.json').exists() and (O/'follow'/'result.json').exists(),'both movement results',55)
 lead=json.loads((O/'lead'/'result.json').read_text());follow=json.loads((O/'follow'/'result.json').read_text());print('RESULT',lead,follow,flush=True)
 if not lead['passed'] or not follow['passed']:raise AssertionError('One or both movement fixtures failed')
 log('PASS Both keepers traversed chase and grip doors',[scene(R,'Facts'),scene(P,'Facts')])
 (O/'result.json').write_text(json.dumps({'passed':True,'scope':'Live Photon two Editors; actual XRI movement, injected run, real host grip doors and guest synchronized crossing. Initial poses and chase beat were staged.'},indent=2))
except Exception as e:
 print('FAILED',type(e).__name__,str(e),flush=True)
 (O/'result.json').write_text(json.dumps({'passed':False,'error':str(e)},indent=2));raise
