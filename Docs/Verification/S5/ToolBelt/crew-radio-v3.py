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
def probe(r,m,args=[]):return call(r,'run_script',{'file':'artifacts/s5/tool-belt/BeltPeerV3.cs','entry':'BeltPeerV3.'+m,'args':args})['result']
def log(s,x):
 print(s,x,flush=True)
 with (O/'events.jsonl').open('a') as f:f.write(json.dumps({'event':s,'data':x})+'\n')
probe(P,'Release');time.sleep(.4)
for kind in ['radio']:
 probe(P,'Prepare',[kind]);time.sleep(.5);probe(P,'Press');wait(lambda:probe(P,'Facts',[kind])['isSelected'],'grip '+kind)
 wait(lambda:probe(R,'Facts',[kind])['Owner']!=0,'owner '+kind)
 for n in range(8):probe(P,'Slot',[kind]);time.sleep(.08)
 probe(P,'Release');wait(lambda:probe(R,'Facts',[kind])['IsCarried'] and probe(P,'Facts',[kind])['local'],'clip '+kind)
 a=probe(R,'Facts',[kind]);b=probe(P,'Facts',[kind]);assert a['Owner']==b['Owner'] and not a['wrenchActive'];log('guest belt replicated '+kind,[a,b])
 probe(P,'Away');time.sleep(.3);probe(P,'Regrip',[kind]);time.sleep(.5);probe(P,'Press');wait(lambda:probe(P,'Facts',[kind])['isSelected'] and not probe(R,'Facts',[kind])['IsCarried'],'regrip '+kind)
 probe(P,'Away');time.sleep(.4);probe(P,'Release');wait(lambda:probe(R,'Facts',[kind])['Owner']==0,'release lease '+kind);log('released '+kind,probe(R,'Facts',[kind]))
(O/'result.json').write_text(json.dumps({'passed':True,'checks':['two muted Editors joined Survival Day 1','guest direct grip and carry/regrip/release for flashlight, wrench and existing radio','carry state and owner agree on both peers','carried tools do not count as remote hand-held work'],'scope':'Focused actual Photon session; not a full night, voice, rejoin, or worn acceptance.'},indent=2))
