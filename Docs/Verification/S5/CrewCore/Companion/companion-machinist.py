import sys,time,json
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call
p=Path.cwd();o=p/'artifacts/s5/crew-core/companion-machinist';o.mkdir(exist_ok=True)
def ev(c):
 for i in range(12):
  try:return call(p,'eval',{'code':c})
  except Exception as e:
   if 'Connection refused' not in str(e):raise
   time.sleep(2)
 raise RuntimeError('Bridge remained unavailable')
print(ev('UnityEditor.EditorUtility.audioMasterMute=true;UnityEngine.Application.targetFrameRate=72;return true;'),flush=True)

if not ev('return UnityEditor.EditorApplication.isPlaying;')['result']: call(p,'editor_play')
time.sleep(8)
for attempt in range(30):
 try:
  if ev('return UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.Net.CrewLobbyBoard>()!=null;')['result']: break
 except Exception: pass
 time.sleep(2)

ev('LighthouseKeepers.Net.CrewLobbyTransport.ForceOfflineForTests=true;if(UnityEngine.GameObject.Find("Play"))UnityEngine.GameObject.Find("Play").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;');time.sleep(1)
ev('UnityEngine.GameObject.Find("HostPlay").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;');time.sleep(2)
ev('UnityEngine.GameObject.Find("HostDutyMachinist").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;')
ev('LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=3;UnityEngine.GameObject.Find("StartCrew").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;');time.sleep(6)
print(call(p,'run_script',{'file':'artifacts/s5/crew-core/CompanionProbe.cs','entry':'CompanionProbe.Run','args':[str(o)]}),flush=True)
for i in range(50):
 time.sleep(5)
 if (o/'current.txt').exists(): print((o/'current.txt').read_text(),flush=True)
 if (o/'result.json').exists():print((o/'result.json').read_text(),flush=True);break
