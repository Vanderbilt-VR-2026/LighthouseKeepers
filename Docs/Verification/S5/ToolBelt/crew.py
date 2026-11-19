import sys,time,json
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call,status
R=Path.cwd();P=Path('/Users/aneeshvasamreddy/LighthouseKeepers-S5-Peer');O=R/'artifacts/s5/tool-belt/crew';O.mkdir(exist_ok=True)
def ev(r,s):return call(r,'eval',{'code':s})['result']
def wait(f,label,seconds=90):
 end=time.monotonic()+seconds
 while time.monotonic()<end:
  try:
   if f():return
  except (OSError,RuntimeError,ValueError,KeyError):pass
  time.sleep(.25)
 raise RuntimeError(label)
def click(r,n):return ev(r,'UnityEngine.GameObject.Find("'+n+'").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;')
def probe(r,m,args=[]):return call(r,'run_script',{'file':'artifacts/s5/tool-belt/BeltPeer.cs','entry':'BeltPeer.'+m,'args':args})['result']
def log(s,x):
 print(s,x,flush=True)
 with (O/'events.jsonl').open('a') as f:f.write(json.dumps({'event':s,'data':x})+'\n')
for r in [R,P]:
 ev(r,'UnityEditor.EditorUtility.audioMasterMute=true;UnityEditor.AssetDatabase.Refresh();return true;');time.sleep(3)
 wait(lambda:ev(r,'UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=72;return !UnityEditor.EditorApplication.isCompiling&&!UnityEditor.EditorUtility.scriptCompilationFailed;'),'compile')
 if ev(r,'return UnityEditor.EditorApplication.isPlaying;'):call(r,'editor_stop');time.sleep(3)
 ev(r,'LighthouseKeepers.Net.CrewLobbyTransport.ForceOfflineForTests=false;UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity");return true;');call(r,'editor_play');wait(lambda:ev(r,'return UnityEngine.GameObject.Find("Play")!=null;'),'lobby')
click(R,'Play');click(R,'Survival');click(R,'HostPlay');wait(lambda:status(R)['online'],'host');code=status(R)['code']
click(P,'Play');click(P,'Survival');click(P,'JoinChoice');ev(P,'UnityEngine.GameObject.Find("CrewCodeInput").GetComponent<TMPro.TMP_InputField>().text="'+code+'";return true;');click(P,'JoinCrewCode');wait(lambda:len(status(R)['players'])==2,'join');click(R,'StartCrew');wait(lambda:bool(status(R)['snapshot']) and bool(status(P)['snapshot']),'start')
for r in [R,P]:ev(r,'UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.InWorldBriefing>().Folder.Skip();return true;')
wait(lambda:not ev(R,'return UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchDemoController>().State.BriefingHeld;'),'briefing')
log('two Editors joined and started',[status(R),status(P)])
for kind in ['flashlight','wrench','radio']:
 probe(P,'Prepare',[kind]);time.sleep(.5);probe(P,'Press');wait(lambda:probe(P,'Facts',[kind])['isSelected'],'grip '+kind)
 wait(lambda:probe(R,'Facts',[kind])['Owner']!=0,'owner '+kind)
 for n in range(8):probe(P,'Slot',[kind]);time.sleep(.08)
 probe(P,'Release');wait(lambda:probe(R,'Facts',[kind])['IsCarried'] and probe(P,'Facts',[kind])['local'],'clip '+kind)
 a=probe(R,'Facts',[kind]);b=probe(P,'Facts',[kind]);assert a['Owner']==b['Owner'] and not a['wrenchActive'];log('guest belt replicated '+kind,[a,b])
 probe(P,'Away');time.sleep(.3);probe(P,'Regrip',[kind]);time.sleep(.5);probe(P,'Press');wait(lambda:probe(P,'Facts',[kind])['isSelected'] and not probe(R,'Facts',[kind])['IsCarried'],'regrip '+kind)
 probe(P,'Away');time.sleep(.4);probe(P,'Release');wait(lambda:probe(R,'Facts',[kind])['Owner']==0,'release lease '+kind);log('released '+kind,probe(R,'Facts',[kind]))
(O/'result.json').write_text(json.dumps({'passed':True,'checks':['two muted Editors joined Survival Day 1','guest direct grip and carry/regrip/release for flashlight, wrench and existing radio','carry state and owner agree on both peers','carried tools do not count as remote hand-held work'],'scope':'Focused actual Photon session; not a full night, voice, rejoin, or worn acceptance.'},indent=2))
