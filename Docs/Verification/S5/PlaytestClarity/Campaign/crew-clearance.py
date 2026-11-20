import sys,time,json,shutil
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call,status
ROOT=Path.cwd();GUEST=Path('/Users/aneeshvasamreddy/LighthouseKeepers-S5-Peer');OUT=ROOT/'artifacts/s5/playtest-feedback/crew-clearance';OUT.mkdir(exist_ok=True)
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
for r in [ROOT,GUEST]:
 (r/'artifacts/s5/playtest-feedback').mkdir(exist_ok=True)
 if r!=ROOT:shutil.copy2(ROOT/'artifacts/s5/playtest-feedback/ClearanceProbe.cs',r/'artifacts/s5/playtest-feedback/ClearanceProbe.cs')
 wait(lambda:ev(r,'UnityEditor.EditorUtility.audioMasterMute=true;UnityEngine.Application.targetFrameRate=72;UnityEngine.Application.runInBackground=true;return !UnityEditor.EditorApplication.isCompiling&&!UnityEditor.EditorUtility.scriptCompilationFailed;'),'compile',120)
 ev(r,'UnityEngine.PlayerPrefs.SetInt(LighthouseKeepers.Net.CrewBriefing.Preference,1);LighthouseKeepers.Net.CrewLobbyTransport.ForceOfflineForTests=false;UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity");return true;')
 call(r,'editor_play');wait(lambda:ev(r,'return UnityEngine.GameObject.Find("Play")!=null;'),'lobby',120)
def probe(r,m='Facts'):return call(r,'run_script',{'file':'artifacts/s5/playtest-feedback/ClearanceProbe.cs','entry':'ClearanceProbe.'+m,'args':[]})['result']
click(ROOT,'Play');click(ROOT,'HostPlay');wait(lambda:status(ROOT)['online'],'host');code=status(ROOT)['code'];join(code);wait(lambda:len(status(ROOT)['players'])==2,'joined')
ev(ROOT,'LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=1;return true;');click(ROOT,'StartCrew');wait(lambda:bool(status(ROOT)['snapshot']) and bool(status(GUEST)['snapshot']),'started',120)
wait(lambda:not ev(ROOT,'return UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchDemoController>().State.BriefingHeld;'),'arrival ack');probe(ROOT,'Prepare');wait(lambda:probe(GUEST)['repaired'] and probe(GUEST)['wait']>175,'waiting frame')
assert not probe(GUEST)['cleared'];log('shared preparation hold',[probe(ROOT),probe(GUEST)])
probe(ROOT,'Transmit');wait(lambda:probe(GUEST)['cleared'] and probe(GUEST)['radio'],'clearance');log('radio clearance replicated',[probe(ROOT),probe(GUEST)])
ev(GUEST,'UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchMenu>().ReturnToLobby();return true;');wait(lambda:ev(GUEST,'return UnityEngine.GameObject.Find("Play")!=null;'),'left');join(code);wait(lambda:bool(status(GUEST)['snapshot']),'rejoined',120);wait(lambda:probe(GUEST)['cleared'],'clearance restored');log('rejoin preserves clearance',probe(GUEST))
probe(ROOT,'Pass');wait(lambda:probe(GUEST)['complete'],'guided passage replicated');log('ship passed',[probe(ROOT),probe(GUEST)])
probe(ROOT,'Retry');wait(lambda:not probe(GUEST)['cleared'] and not probe(GUEST)['repaired'],'retry resets');log('retry reset',[probe(ROOT),probe(GUEST)])
(OUT/'result.json').write_text(json.dumps({'passed':True,'checks':['host join','preparation wait replicated','radio clearance replicated','leave/rejoin retains clearance','guided pass replicated','retry restores uncleared ship'],'scope':'Two live Editors; actions scripted on host, not physical controller or audible voice acceptance'},indent=2))
for r in [ROOT,GUEST]:call(r,'editor_stop')
