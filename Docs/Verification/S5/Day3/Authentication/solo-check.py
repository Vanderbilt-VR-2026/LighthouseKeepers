import sys,time,json
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call
p=Path.cwd();out=p/'artifacts/s5/day3/solo';out.mkdir(exist_ok=True)
def probe(name,args=[]):return call(p,'run_script',{'file':'artifacts/s5/day3/AuthProbe.cs','entry':'AuthProbe.'+name,'args':args})['result']
def facts():return probe('Facts')
def grab(n):probe('Grab',[str(n),'near']);time.sleep(1.2);probe('Release');time.sleep(.3)
probe('Ready');time.sleep(2);a=facts();print('ready',a,flush=True);assert a['frame']['stage']==1
before=call(p,'eval',{'code':'var w=UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchDemoController>();return new[]{w.State.Beacon.Ignited,w.State.HasPower(LighthouseKeepers.Core.PowerCircuit.Beacon)};'})['result']
grab(1);assert facts()['frame']['stage']==1
grab(0);a=facts();assert a['frame']['stage']==2;print('real reply',a,flush=True)
probe('Capture',[str(out/'radio-challenge.png')]);grab(2);assert facts()['frame']['stage']==3
grab(0);grab(1);assert facts()['frame']['realDone']
probe('ForceFalse');time.sleep(.5);assert facts()['frame']['kind']==2
grab(0);a=facts();assert a['frame']['stage']==2;assert a['reply']!=a['code'].replace('   >   ',' · ').replace('\n',' · ')
probe('Capture',[str(out/'false-reply.png')]);grab(1);assert facts()['frame']['stage']==3
grab(0);grab(2);assert facts()['frame']['fakeDone']
a=facts();after=call(p,'eval',{'code':'var w=UnityEngine.Object.FindAnyObjectByType<LighthouseKeepers.WatchDemo.WatchDemoController>();return new[]{w.State.Beacon.Ignited,w.State.HasPower(LighthouseKeepers.Core.PowerCircuit.Beacon)};'})['result'];assert before==after
(out/'result.json').write_text(json.dumps({'passed':True,'final':a,'checks':['direct XRI grip: challenge / accept / reject','accept before challenge rejected','real reply uses current House code','incorrect decisions recover','impostor differs, rejection resolves','beacon unchanged by all answers'],'scope':'Editor fixture powers radio and schedules calls; not a full Night 3 or headset test'},indent=2));print('PASS',flush=True)
call(p,'editor_stop')
