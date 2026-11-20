import sys,time,json,shutil
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call,status
ROOT=Path.cwd();GUEST=Path('/Users/aneeshvasamreddy/LighthouseKeepers-S5-Peer');OUT=ROOT/'artifacts/s5/day3/crew-third';OUT.mkdir(exist_ok=True)
def ev(r,c):return call(r,'eval',{'code':c})['result']
def wait(f,label,seconds=90):
 end=time.monotonic()+seconds
 while time.monotonic()<end:
  try:
   if f():return
  except (OSError,ValueError,KeyError,RuntimeError):pass
  time.sleep(.5)
 raise RuntimeError(label)
def log(label,data=None):
 print(label,data,flush=True)
 with (OUT/'events.jsonl').open('a') as f:f.write(json.dumps({'time':time.time(),'event':label,'data':data})+'\n')
def click(r,n):return ev(r,'UnityEngine.GameObject.Find("'+n+'").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;')
def p(r,m,args=[]):return call(r,'run_script',{'file':'artifacts/s5/day3/AuthProbe.cs','entry':'AuthProbe.'+m,'args':args})['result']
def facts(r=ROOT):return p(r,'Facts')
def grab(n,far=False):p(GUEST,'Grab',[str(n),'far' if far else 'near']);time.sleep(1.5);p(GUEST,'Release');time.sleep(.5)
def join(code):
 click(GUEST,'Play');click(GUEST,'JoinChoice');ev(GUEST,'UnityEngine.GameObject.Find("CrewCodeInput").GetComponent<TMPro.TMP_InputField>().text="'+code+'";return true;');click(GUEST,'JoinCrewCode')
shutil.copy2(ROOT/'artifacts/s5/day3/AuthProbe.cs',GUEST/'artifacts/s5/day3/AuthProbe.cs')
for r in [ROOT,GUEST]:
 wait(lambda:ev(r,'UnityEditor.EditorUtility.audioMasterMute=true;UnityEngine.Application.targetFrameRate=72;UnityEngine.Application.runInBackground=true;return !UnityEditor.EditorApplication.isCompiling&&!UnityEditor.EditorUtility.scriptCompilationFailed;'),'compile',120)
 ev(r,'UnityEngine.PlayerPrefs.SetInt(LighthouseKeepers.Net.CrewBriefing.Preference,1);LighthouseKeepers.Net.CrewLobbyTransport.ForceOfflineForTests=false;return true;')
 try:ev(r,'UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity");return true;')
 except RuntimeError:pass
 wait(lambda:ev(r,'return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="LK_NetworkLobby";'),'scene opened',40)
 call(r,'editor_play');wait(lambda:ev(r,'return UnityEngine.GameObject.Find("Play")!=null;'),'lobby',120)
click(ROOT,'Play');click(ROOT,'HostPlay');wait(lambda:status(ROOT)['online'],'host');code=status(ROOT)['code'];log('crew code',code);join(code);wait(lambda:len(status(ROOT)['players'])==2,'joined')
ev(ROOT,'LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=3;return true;');click(ROOT,'StartCrew');wait(lambda:bool(status(ROOT)['snapshot']) and bool(status(GUEST)['snapshot']),'started',120)
p(ROOT,'Ready');wait(lambda:facts()['frame']['stage']==1 and facts(GUEST)['frame']['stage']==1,'real call');log('shared call',[facts(),facts(GUEST)])
grab(0,True);assert facts()['frame']['stage']==1
# Real XRI guest grip reaches the host, then the shared answer is replicated.
grab(0);wait(lambda:facts()['frame']['stage']==2 and facts(GUEST)['frame']['stage']==2,'challenge');a,b=facts(),facts(GUEST);assert a['reply']==b['reply'];log('shared answer',[a,b])
# Leave during a challenged call. The full frame, decision stage, code and duty must survive.
ev(GUEST,'UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchMenu>().ReturnToLobby();return true;');wait(lambda:ev(GUEST,'return UnityEngine.GameObject.Find("Play")!=null;'),'left');wait(lambda:ev(ROOT,'return UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.CrewCompanion>().Active;'),'bot takeover')
join(code);wait(lambda:bool(status(GUEST)['snapshot']),'rejoined',120);wait(lambda:facts(GUEST)['frame']['stage']==2,'stage restored');assert facts(GUEST)['reply']==a['reply'];assert facts(GUEST)['duty']==b['duty'];log('rejoin restored',facts(GUEST))
grab(2);wait(lambda:facts()['frame']['stage']==3,'wrong decision correction');grab(0);grab(1);wait(lambda:facts()['frame']['realDone'] and facts(GUEST)['frame']['realDone'],'accepted')
p(ROOT,'ForceFalse');wait(lambda:facts(GUEST)['frame']['kind']==2,'false call');grab(0);assert facts()['reply']==facts(GUEST)['reply'];grab(2);wait(lambda:facts()['frame']['fakeDone'] and facts(GUEST)['frame']['fakeDone'],'false rejected')
log('finished',[facts(),facts(GUEST)]);(OUT/'result.json').write_text(json.dumps({'passed':True,'checks':['real host/join','guest distant hand rejected','guest direct challenge replicates identical code','leave mid-challenge / bot takes over / rejoin restores stage and duty','wrong answer recovery','real accepted','false rejected by guest, same result on host'],'scope':'Night 3 radio fixture; not full watch, human voice or headset acceptance'},indent=2))
for r in [ROOT,GUEST]:call(r,'editor_stop')
