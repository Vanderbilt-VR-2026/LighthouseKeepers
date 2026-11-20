import sys,time,json,shutil
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call,run,status,snapshot
raw_call=call

def call(*args,**kwargs):
 for attempt in range(8):
  try:return raw_call(*args,**kwargs)
  except OSError as e:
   if 'Connection refused' not in str(e) or attempt==7:raise
   time.sleep(1)

ROOT=Path.cwd()
GUEST=Path("/Users/aneeshvasamreddy/LighthouseKeepers-S5-Peer")
OUT=ROOT/'artifacts/s5/crew-core/two-mac-companion';OUT.mkdir(parents=True,exist_ok=True)
for f in ['artifacts/s5/lantern/Crew.cs','artifacts/s5/story/PaperCrew.cs','artifacts/s4-connected/ValveHold.cs','artifacts/s4-connected/RadioHold.cs','artifacts/coop/EditorHost.cs','artifacts/s4-master/part-b/CoopDriver.cs','artifacts/day3-balance/voice/VoiceFacts.cs']:
 (GUEST/f).parent.mkdir(parents=True,exist_ok=True);shutil.copy2(ROOT/f,GUEST/f)
def wait(f,label,seconds=90):
 end=time.monotonic()+seconds
 while time.monotonic()<end:
  try:
   if f():return
  except (OSError,ValueError,KeyError,RuntimeError):pass
  time.sleep(1)
 raise RuntimeError(label)
def log(label,data=None):
 print(label,flush=True)
 with (OUT/'events.jsonl').open('a') as f:f.write(json.dumps({'time':time.time(),'event':label,'data':data})+'\n')
def ev(r,code):return call(r,'eval',{'code':code})['result']
def click(r,name):return ev(r,'UnityEngine.GameObject.Find("'+name+'").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;')
def shot(label,method='Capture'):
 for r,role in [(ROOT,'host'),(GUEST,'guest')]:run(r,method,[str(OUT/(label+'-'+role+'.png'))])
def action(r,verb):
 out=OUT/(('host-' if r==ROOT else 'guest-')+verb);out.mkdir(exist_ok=True)
 call(r,'run_script',{'file':'artifacts/s4-master/part-b/CoopDriver.cs','entry':'LighthouseKeepers.Editor.ImmersionCoopDriver.Run','args':[verb,str(out)]})
 wait(lambda:(out/'result.json').exists(),verb,150)
 assert json.loads((out/'result.json').read_text())['passed'],str(out)
def valve(method):return call(ROOT,'run_script',{'file':'artifacts/s4-connected/ValveHold.cs','entry':'ValveHold.'+method})
def join(code):
 click(GUEST,'Play');click(GUEST,'JoinChoice');ev(GUEST,'UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.Lobby.LobbyManager>().PlayerName="Mac Guest";UnityEngine.GameObject.Find("CrewCodeInput").GetComponent<TMPro.TMP_InputField>().text="'+code+'";return true;');click(GUEST,'JoinCrewCode')
def special(r,method,args=[]):return call(r,"run_script",{"file":"artifacts/s4-connected/RadioHold.cs","entry":"RadioHold."+method,"args":args})["result"]
def radiofacts(r):return special(r,"Facts")
def probe(r,m,args=[]):return call(r,'run_script',{'file':'artifacts/s5/crew-core/RelayProbe.cs','entry':'RelayProbe.'+m,'args':args})['result']
def main():
 for r in [ROOT,GUEST]:
  wait(lambda:call(r,'editor_status')['status'] in ['ready','playing'],'Editor ready',120)
  wait(lambda:ev(r,'UnityEditor.EditorUtility.audioMasterMute=true;UnityEngine.Application.targetFrameRate=72;LighthouseKeepers.Net.CrewLobbyTransport.ForceOfflineForTests=false;LighthouseKeepers.Lobby.CrewDuties.Preference=LighthouseKeepers.Lobby.CrewDuty.Lightkeeper;return true;'),'Mute/setup',40)
  if call(r,'editor_status')['status']!='playing':call(r,'editor_play')
  wait(lambda:ev(r,'return UnityEngine.GameObject.Find("Play")!=null;'),'Lobby')
  ev(r,'UnityEditor.EditorUtility.audioMasterMute=true;UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=72;return true;')
 click(ROOT,'Play');click(ROOT,'HostPlay');wait(lambda:status(ROOT)['online'],'Host');click(ROOT,'HostDutyMachinist');code=status(ROOT)['code'];join(code);wait(lambda:len(status(ROOT)['players'])==2,'Crew')
 ev(ROOT,'LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=3;return true;');click(ROOT,'StartCrew');wait(lambda:bool(status(ROOT)['snapshot']) and bool(status(GUEST)['snapshot']),'Both House',120)
 assert snapshot(ROOT)['day']==3
 probe(ROOT,'Setup');time.sleep(2)
 a,b=probe(ROOT,'Facts'),probe(GUEST,'Facts');log('Radio ready fixture; paired instructions',[a,b])
 assert a['step']=='ReadCode' and b['step']=='RadioCode' and a['house'] and b['house'] and a['printed']==b['printed'] and not b['answerGlow'],(a,b)
 probe(ROOT,'Capture',[str(OUT/'house-telegram.png'),'clue']);probe(GUEST,'Capture',[str(OUT/'radio-buttons.png'),'radio'])
 import re
 numbers=re.findall(r'\b([1-4]) (?:RED|AMBER|BLUE|GREEN)\b',a['printed']);assert len(numbers)==4,a['printed']
 def press(n,label):
  path=OUT/(label+'.txt');probe(GUEST,'Press',[n,str(path)]);wait(lambda:path.exists(),label,15);time.sleep(.3)
 press(str((int(numbers[0])%4)+1),'wrong');assert probe(ROOT,'Facts')['codeProgress']==0
 for i,n in enumerate(numbers[:2]):press(n,'code-'+str(i))
 wait(lambda:probe(ROOT,'Facts')['codeProgress']==2,'First symbols shared')
 ev(GUEST,'UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchMenu>().ReturnToLobby();return true;');wait(lambda:ev(GUEST,'return UnityEngine.GameObject.Find("Play")!=null;'),'Guest lobby',45)
 wait(lambda:ev(ROOT,'return UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.CrewCompanion>().Active;'),'Companion spawned after human leave',20);assert probe(ROOT,'Facts')['house'];assert probe(ROOT,'Facts')['codeProgress']==2
 join(code);wait(lambda:bool(status(GUEST)['snapshot']),'Rejoin running watch',90);wait(lambda:probe(GUEST,'Facts')['house'],'House clue after rejoin',20)
 assert probe(GUEST,'Facts')['duty']=='Lightkeeper' and probe(GUEST,'Facts')['codeProgress']==2
 wait(lambda:ev(ROOT,'return !UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.CrewCompanion>().Active;'),'Companion retired before rejoined human acts',20)
 log('Actual leave/rejoin preserves role, clue and partial code',[probe(ROOT,'Facts'),probe(GUEST,'Facts')])
 for i,n in enumerate(numbers[2:]):press(n,'finish-'+str(i))
 wait(lambda:probe(ROOT,'Facts')['radioStep']==3 and probe(GUEST,'Facts')['radioStep']==3,'Code accepted')
 marker=OUT/'mic.txt';probe(GUEST,'Transmit',[str(marker)]);wait(lambda:marker.exists(),'Guest transmission',20);assert marker.read_text()=='complete',marker.read_text()
 log('Physical guest buttons and mic complete shared broadcast',[probe(ROOT,'Facts'),probe(GUEST,'Facts')])
 (OUT/'result.json').write_text(json.dumps({'passed':True,'fixture':'Earlier tasks restored on host to isolate the relay; not a full Night3 run','checks':['host House clue matches guest','Lightkeeper operator / Machinist reader','no HUD/button answer','wrong symbol recovery','XRI button input replicated','actual leave/rejoin preserves duty and partial code','companion replaces departed keeper, then retires on human rejoin; no duplicate owner','guest leased microphone transmits shared report'],'voice':'NOT VERIFIED: Editors muted'},indent=2))
 for r in [ROOT,GUEST]:call(r,'editor_stop')
try:main()
except Exception as e:
 log('FAIL',str(e));(OUT/'result.json').write_text(json.dumps({'passed':False,'error':str(e)},indent=2));raise
