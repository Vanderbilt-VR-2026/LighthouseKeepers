import sys,time,json
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call
p=Path.cwd();o=p/'artifacts/s5/possession/solo-final-clear';o.mkdir(exist_ok=True)
def ev(c):return call(p,'eval',{'code':c})['result']
try:ev('UnityEditor.EditorUtility.audioMasterMute=true;UnityEngine.Application.targetFrameRate=72;UnityEditor.AssetDatabase.Refresh();return true;')
except RuntimeError:pass
time.sleep(15)
ev('UnityEngine.PlayerPrefs.SetInt(LighthouseKeepers.Net.CrewBriefing.Preference,1);UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity");return true;');call(p,'editor_play');time.sleep(10)
ev('LighthouseKeepers.Net.CrewLobbyTransport.ForceOfflineForTests=true;LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=1;UnityEngine.GameObject.Find("Play").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();UnityEngine.GameObject.Find("HostPlay").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;');time.sleep(2)
ev('UnityEngine.GameObject.Find("StartCrew").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;');time.sleep(8)
o=p/'artifacts/s5/playtest-feedback/clearance-play';o.mkdir(exist_ok=True)
print(call(p,'run_script',{'file':'artifacts/s5/playtest-feedback/ClearancePlay.cs','entry':'ClearancePlay.Run','args':[str(o)]}),flush=True)
for i in range(120):
 if (o/'result.json').exists():break
 time.sleep(1)
print((o/'result.json').read_text(),flush=True)
call(p,'editor_stop')
