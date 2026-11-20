import sys,time,json,os
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call
R=Path.cwd();P=Path('/Users/aneeshvasamreddy/LighthouseKeepers-S5-Peer');O=Path(os.environ['LK_TEST_OUT']);O.mkdir(parents=True,exist_ok=False)
def ev(r,s):
 x=call(r,'eval',{'code':s})
 if not x.get('success'):raise RuntimeError(str(x.get('diagnostics')))
 return x['result']
def scene(r,m,args=[]):
 x=call(r,'run_script',{'file':str(R/'Docs/Verification/S5/NightmareFoundation/Coop/fixtures/TwoPeer.cs'),'entry':'TwoPeer.'+m,'args':args})
 if not x.get('success'):raise RuntimeError(str(x.get('diagnostics')))
 return x['result']
def wait(f,label,seconds=30):
 end=time.monotonic()+seconds
 while time.monotonic()<end:
  try:
   if f():return
  except (OSError,RuntimeError,KeyError,ValueError):pass
  time.sleep(.2)
 raise RuntimeError(label)
def log(label,data):
 print(label,flush=True)
 with (O/'events.jsonl').open('a') as f:f.write(json.dumps({'event':label,'data':data})+'\n')
try:
 initial=[scene(R,'Facts'),scene(P,'Facts')];log('Initial wake',initial);scene(R,'StageEscape')
 ok=ev(R,'var w=UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchDemoController>();var c=w.State.Scare;var a=w.State.Recovery.Down(1);var b=c.Catch(1,new UnityEngine.Vector3(1,3.3f,0));var d=w.State.Recovery.Down(2);var e=c.Catch(2,new UnityEngine.Vector3(1,3.3f,1));w.GetComponent<LighthouseKeepers.WatchDemo.NightmareScene>().enabled=true;return a&&b&&d&&e;')
 assert ok;wait(lambda:scene(R,'Facts')['beat']==11 and scene(P,'Facts')['beat']==11,'both caught synchronized');log('PASS Both caught fails on both clients',[scene(R,'Facts'),scene(P,'Facts')])
 assert ev(R,'return UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchDemoController>().RetryDay();')
 wait(lambda:scene(R,'Facts')['beat']==1 and scene(P,'Facts')['beat']==1 and scene(P,'Facts')['attempt']>initial[1]['attempt'],'shared retry')
 time.sleep(.8);host,guest=scene(R,'Facts'),scene(P,'Facts')
 def expected(f):return '(2.000, 8.' in f['head'] and ('-0.500)' in f['head'] if f['duty']==1 else '-1.100)' in f['head'])
 assert expected(host) and expected(guest) and not host['localDown'] and not guest['localDown'] and host['valid'] and guest['valid'];log('PASS Explicit retry places both keepers at separate wake points',[host,guest])
 (O/'result.json').write_text(json.dumps({'passed':True,'scope':'Live Photon shared both-caught failure and explicit host retry. Catch was staged; recorded retry places both clients at distinct safe wake points.'},indent=2))
except Exception as e:
 print('FAILED',type(e).__name__,str(e),flush=True);(O/'result.json').write_text(json.dumps({'passed':False,'error':str(e)},indent=2));raise
