import sys,time
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call
p=Path.cwd()
def ev(c):return call(p,'eval',{'code':c})['result']
ev('UnityEditor.EditorUtility.audioMasterMute=true;UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=72;UnityEngine.PlayerPrefs.SetInt(LighthouseKeepers.Net.CrewBriefing.Preference,1);UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity");return true;')
call(p,'editor_play');time.sleep(8)
ev('LighthouseKeepers.Net.CrewLobbyTransport.ForceOfflineForTests=true;LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=3;UnityEngine.GameObject.Find("Play").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();UnityEngine.GameObject.Find("HostPlay").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;');time.sleep(2)
ev('UnityEngine.GameObject.Find("StartCrew").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;');time.sleep(6)
print(ev('var w=UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchDemoController>();return w.DescribeState();'),flush=True)
