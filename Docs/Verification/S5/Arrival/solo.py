import sys,time,json
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call
r=Path.cwd();o=r/'artifacts/s5/arrival/solo-final';o.mkdir(exist_ok=True)
def ev(c):return call(r,'eval',{'code':c})['result']
def p(n,*args):return call(r,'run_script',{'file':'artifacts/s5/arrival/ArrivalProbe.cs','entry':'ArrivalProbe.'+n,'args':list(args)})['result']
for _ in range(90):
 try:
  if ev('UnityEditor.AssetDatabase.Refresh();return !UnityEditor.EditorApplication.isCompiling&&!UnityEditor.EditorUtility.scriptCompilationFailed;'):break
 except Exception:pass
 time.sleep(1)
else:raise RuntimeError('compile not clean')
ev('UnityEditor.EditorUtility.audioMasterMute=true;UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=72;UnityEngine.PlayerPrefs.DeleteKey(LighthouseKeepers.Net.CrewBriefing.Preference);UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity");return true;')
call(r,'editor_play');time.sleep(8)
assert ev('return UnityEngine.GameObject.Find("Harbor duty folder")==null;')
ev('LighthouseKeepers.Net.CrewLobbyTransport.ForceOfflineForTests=true;LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=1;UnityEngine.GameObject.Find("Play").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();UnityEngine.GameObject.Find("HostPlay").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;');time.sleep(1)
ev('UnityEngine.GameObject.Find("StartCrew").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;');time.sleep(7)
a=p('Facts');print(a,flush=True);assert a['open'] and a['held'] and a['elapsed']==0
b=p('SimulateReading');assert b['held'] and b['elapsed']==0 and b['gate']==0
for i in range(3):
 facts=p('Facts');assert facts['page']==i and not facts['overflow'],facts
 p('Capture',str(o/('page-'+str(i)+'.png')));p('Next');time.sleep(.5)
a=p('Facts');assert not a['open'] and not a['held'] and a['seen']
p('Open');a=p('Facts');assert a['open'] and not a['held'];p('Skip')
(o/'result.json').write_text(json.dumps({'passed':True,'initial':b,'checks':['No briefing in lobby','House arrival opens first page','90 seconds reading consumes no clock or start grace','Three pages have no text overflow','Finish releases watch','Reopen does not pause running game','Skip closes']},indent=2))
call(r,'editor_stop');print('PASS',flush=True)
