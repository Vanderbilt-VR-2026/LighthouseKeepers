import json, os, sys, time
from pathlib import Path

sys.path.insert(0, '/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop')
from mac_pair import call, status

ROOT = Path.cwd()
PEER = Path('/Users/aneeshvasamreddy/LighthouseKeepers-S5-Peer')
OUT = Path(os.environ['LK_TEST_OUT'])
OUT.mkdir(parents=True, exist_ok=False)

def ev(root, code):
    result = call(root, 'eval', {'code': code})
    if not result.get('success'):
        raise RuntimeError(str(result.get('diagnostics')))
    return result['result']

def wait(test, description, seconds=90):
    until = time.monotonic() + seconds
    while time.monotonic() < until:
        try:
            if test():
                return
        except (OSError, RuntimeError, KeyError, ValueError):
            pass
        time.sleep(.3)
    raise RuntimeError('Timed out: ' + description)

def click(root, name):
    ev(root, 'UnityEngine.GameObject.Find("' + name + '").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();return true;')

def facts(root):
    return ev(root, '''var w=UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchDemoController>();
var s=w.State;var d=s.Director;var id=UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.Net.WatchCoopSession>().LocalActor;
return new{day=s.Day,me=id,firstActor=d.FirstActor,secondActor=d.SecondActor,first=d.First.ToString(),second=d.Second.ToString(),
mine=d.ForActor(id).ToString(),partner=d.Partner(id).ToString(),fuel=s.Engine.Fuel,pulls=s.Engine.Pulls,running=s.Engine.Running,
helper=s.StartAssist.Holder,failed=s.Failed,valid=s.Capture(w.WaterHeight).Valid()};''')

def record(event):
    data = {'event': event, 'host': facts(ROOT), 'guest': facts(PEER)}
    print(json.dumps(data), flush=True)
    with (OUT/'events.jsonl').open('a') as f:
        f.write(json.dumps(data) + '\n')

try:
    for root in (ROOT, PEER):
        ev(root, 'UnityEditor.EditorUtility.audioMasterMute=true;UnityEditor.AssetDatabase.Refresh();return true;')
        wait(lambda: ev(root, 'UnityEditor.EditorUtility.audioMasterMute=true;UnityEngine.Application.runInBackground=true;return !UnityEditor.EditorApplication.isCompiling&&!UnityEditor.EditorUtility.scriptCompilationFailed;'), 'compile')
        if ev(root, 'return UnityEditor.EditorApplication.isPlaying;'):
            call(root, 'editor_stop')
            wait(lambda: not ev(root, 'return UnityEditor.EditorApplication.isPlaying;'), 'stop')
        ev(root, 'LighthouseKeepers.Net.CrewLobbyTransport.ForceOfflineForTests=false;UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity");return true;')
        call(root, 'editor_play')
        wait(lambda: ev(root, 'UnityEditor.EditorUtility.audioMasterMute=true;return UnityEngine.GameObject.Find("Play")!=null;'), 'lobby')
    click(ROOT, 'Play'); click(ROOT, 'Campaign'); click(ROOT, 'HostPlay')
    wait(lambda: status(ROOT)['online'], 'host online')
    code = status(ROOT)['code']
    click(PEER, 'Play'); click(PEER, 'Campaign'); click(PEER, 'JoinChoice')
    ev(PEER, 'UnityEngine.GameObject.Find("CrewCodeInput").GetComponent<TMPro.TMP_InputField>().text="' + code + '";return true;')
    click(PEER, 'JoinCrewCode')
    wait(lambda: len(status(ROOT)['players']) == 2, 'guest joined')
    ev(ROOT, 'LighthouseKeepers.StormWatch.WatchDayLaunch.RequestedDay=4;return true;')
    click(ROOT, 'StartCrew')
    wait(lambda: bool(status(ROOT)['snapshot']) and bool(status(PEER)['snapshot']), 'both entered watch', 120)
    for root in (ROOT, PEER):
        ev(root, 'UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.InWorldBriefing>().Folder.Skip();return true;')
    wait(lambda: not ev(ROOT, 'return UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchDemoController>().State.BriefingHeld;'), 'briefing')
    ev(ROOT, 'var s=UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchDemoController>().State;s.EngageWatch();for(int i=0;i<160;i++)s.PourFuel(.1f);return true;')
    def assigned():
        h, g = facts(ROOT), facts(PEER)
        return h['day'] == g['day'] == 4 and sorted([h['first'],h['second']]) == ['HoldStarterAssist','Starter'] and h['first'] == g['first'] and h['second'] == g['second'] and h['firstActor'] == g['firstActor'] and h['secondActor'] == g['secondActor']
    wait(assigned, 'complementary assignments replicated')
    record('paired instructions')
    # Stage the physical actions on the host; this fixture tests the real transport
    # and replicated objectives, not human hand reach or XRI input.
    r = ev(ROOT, '''var w=UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchDemoController>();var s=w.State;var d=s.Director;
int worker=d.First==LighthouseKeepers.WatchDemo.KeeperStep.Starter?d.FirstActor:d.SecondActor;
int helper=worker==d.FirstActor?d.SecondActor:d.FirstActor;
for(int i=0;i<3;i++){for(int j=0;j<100&&!s.StarterInWindow;j++)s.Tick(.05);s.HoldStartAssist(helper,true);if(!s.PullStarterAs(worker))return false;s.Tick(.4);}
return s.Engine.Running;''')
    if not r: raise RuntimeError('staged paired start failed')
    wait(lambda: facts(PEER)['running'] and facts(ROOT)['running'], 'engine replicated')
    record('running engine')
    (OUT/'result.json').write_text(json.dumps({'passed': True, 'scope': 'Real Photon host/join and Day 4 replicated assignments and engine state; fuel and paired pulls staged on host. Not a physical-controller, full-night, or voice test.'}, indent=2))
except Exception as error:
    (OUT/'result.json').write_text(json.dumps({'passed': False, 'error': str(error)}, indent=2))
    raise
finally:
    for root in (ROOT, PEER):
        try:
            if ev(root, 'return UnityEditor.EditorApplication.isPlaying;'):
                call(root, 'editor_stop')
        except Exception:
            pass
