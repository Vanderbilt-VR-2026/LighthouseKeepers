import sys,time,json
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call
r=Path.cwd();o=r/'artifacts/s5/soundtrack/play';o.mkdir(exist_ok=True)
def ev(c):return call(r,'eval',{'code':c})['result']
def p(m):return call(r,'run_script',{'file':'artifacts/s5/soundtrack/ScoreProbe.cs','entry':'ScoreProbe.'+m,'args':[]})['result']
ev('UnityEditor.EditorUtility.audioMasterMute=true;UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=72;UnityEngine.PlayerPrefs.SetInt(LighthouseKeepers.Net.CrewBriefing.Preference,1);UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity");return true;')
call(r,'editor_play');time.sleep(7)
lobby=ev('var a=UnityEngine.GameObject.Find("Quiet lobby calm drone").GetComponentsInChildren<UnityEngine.AudioSource>();return a.Where(x=>x.isPlaying).Select(x=>new{x.name,clip=x.clip.name,x.volume}).ToArray();');print('lobby',lobby,flush=True);assert len(lobby)==1 and lobby[0]['clip']=='main-loop'
ev('LighthouseKeepers.Net.CrewLobbyTransport.ForceOfflineForTests=true;LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=2;UnityEngine.GameObject.Find("Play").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();UnityEngine.GameObject.Find("HostPlay").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;');time.sleep(1)
ev('UnityEngine.GameObject.Find("StartCrew").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;');time.sleep(7)
a=p('Facts');assert a['ContinuousScore'] and a['MainClip']=='main-loop';print('initial',a,flush=True)
b=p('Accent');assert b['first'] and not b['duplicate'];time.sleep(1);c=p('Mode');assert c['MainTime']>a['MainTime'] and c['OverlayCount']==1 and c['OverlayPlaying'];print('mode transition during accent',c,flush=True)
time.sleep(8);d=p('Facts');assert not d['OverlayPlaying'] and d['MainTime']>c['MainTime'] and d['MainClip']=='main-loop'
p('Threat');time.sleep(1);e=p('Facts');assert e['OverlayPlaying'] and e['OverlayCount']==2;print('possession threat event',e,flush=True)
(o/'result.json').write_text(json.dumps({'passed':True,'lobby':lobby,'initial':a,'accent':b,'crisis':c,'after':d,'possession':e,'scope':'Muted actual lobby and offline Play Mode; injected crisis/possession events, no headset listening acceptance.'},indent=2))
call(r,'editor_stop');print('PASS',flush=True)
