import sys,time,json
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call
p=Path.cwd();log=p/'artifacts/s5/crew-core/editor-restart.log'
call(p,'eval',{'code':'UnityEditor.EditorUtility.audioMasterMute=true;UnityEngine.Application.targetFrameRate=72;return true;'})
call(p,'editor_play');time.sleep(12)
for i in range(30):
 try:
  if call(p,'eval',{'code':'return UnityEngine.GameObject.Find("Play")!=null;'})['result']:break
 except OSError:pass
 time.sleep(1)
call(p,'eval',{'code':'LighthouseKeepers.Net.CrewLobbyTransport.ForceOfflineForTests=true;LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=3;UnityEngine.GameObject.Find("Play").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();UnityEngine.GameObject.Find("HostPlay").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;'})
time.sleep(2)
call(p,'eval',{'code':'UnityEngine.GameObject.Find("StartCrew").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;'})
time.sleep(6)
assert call(p,'eval',{'code':'return UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.CrewCompanion>().Active;'})['result']
start=log.stat().st_size
call(p,'editor_stop');time.sleep(4)
with log.open() as f:f.seek(start);tail=f.read()
(p/'artifacts/s5/crew-core/scene-exit.log').write_text(tail)
assert 'Exception' not in tail and 'leaked navmesh' not in tail,tail
(p/'Docs/Verification/S5/CrewCore/Companion/scene-exit.json').write_text(json.dumps({'passed':True,'exception':False,'leakedNavMesh':False,'change':'Actual active-companion scene exit frees navigation and visuals without broadcasting changes to destroyed station listeners.'},indent=2))
print('Active companion scene exit passed')
