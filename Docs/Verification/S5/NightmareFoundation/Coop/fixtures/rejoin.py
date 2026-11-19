import sys,time,json,os
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call,status
R=Path.cwd();P=Path('/Users/aneeshvasamreddy/LighthouseKeepers-S5-Peer');O=Path(os.environ['LK_TEST_OUT']);O.mkdir(parents=True,exist_ok=False)
def ev(r,s):
 x=call(r,'eval',{'code':s})
 if not x.get('success'):raise RuntimeError(str(x.get('diagnostics')))
 return x['result']
def scene(r,m,args=[]):
 x=call(r,'run_script',{'file':str(R/'Docs/Verification/S5/NightmareFoundation/Coop/fixtures/TwoPeer.cs'),'entry':'TwoPeer.'+m,'args':args})
 if not x.get('success'):raise RuntimeError(str(x.get('diagnostics')))
 return x['result']
def wait(f,label,seconds=60):
 end=time.monotonic()+seconds
 while time.monotonic()<end:
  try:
   if f():return
  except (OSError,RuntimeError,KeyError,ValueError):pass
  time.sleep(.25)
 raise RuntimeError(label)
def log(label,data):
 print(label,flush=True)
 with (O/'events.jsonl').open('a') as f:f.write(json.dumps({'event':label,'data':data})+'\n')
def click(r,n):return ev(r,'UnityEngine.GameObject.Find("'+n+'").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;')
try:
 code=status(R)['code'];before=scene(R,'Facts');log('Before guest leaves',before)
 assert before['beat']==8 and before['participants']==3
 ev(P,'UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchMenu>().ReturnToLobby();return true;')
 wait(lambda:ev(P,'return UnityEngine.GameObject.Find("Play")!=null;'),'guest lobby')
 wait(lambda:scene(R,'Facts')['attempt']>before['attempt'] and scene(R,'Facts')['participants']!=3,'host adjusts crew')
 solo=scene(R,'Facts');assert solo['beat']==1 and not solo['reposition'] and solo['head']==before['head'];log('PASS Host resumes at safe wake without camera teleport',solo)
 click(P,'Play');click(P,'Campaign');click(P,'JoinChoice');ev(P,'UnityEngine.GameObject.Find("CrewCodeInput").GetComponent<TMPro.TMP_InputField>().text="'+code+'";return true;');click(P,'JoinCrewCode')
 wait(lambda:bool(status(P)['snapshot']),'guest rejoin');wait(lambda:scene(R,'Facts')['participants']==3 and scene(R,'Facts')['attempt']>solo['attempt'],'crew restored')
 wait(lambda:scene(P,'Facts')['attempt']==scene(R,'Facts')['attempt'],'guest restored')
 host,guest=scene(R,'Facts'),scene(P,'Facts');assert host['head']==before['head'] and host['beat']==1 and guest['beat']==1 and not host['reposition'] and not guest['reposition'] and host['valid'] and guest['valid'];log('PASS Rejoin shares wake without forced reposition',[host,guest])
 (O/'result.json').write_text(json.dumps({'passed':True,'scope':'Live Photon leave/rejoin during staged Return; scene restarts Wake without moving the host or commanding a retry teleport. Not a headset or full-night test.'},indent=2))
except Exception as e:
 print('FAILED',type(e).__name__,str(e),flush=True);(O/'result.json').write_text(json.dumps({'passed':False,'error':str(e)},indent=2));raise
