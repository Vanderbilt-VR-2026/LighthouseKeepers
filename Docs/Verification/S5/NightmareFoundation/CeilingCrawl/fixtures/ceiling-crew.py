import sys,time,json,math
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop')
from mac_pair import call
R=Path.cwd();P=Path('/Users/aneeshvasamreddy/LighthouseKeepers-S5-Peer');out=Path(__import__('os').environ['LK_TEST_OUT']);out.mkdir(exist_ok=False)
def run(root,method):
 r=call(root,'run_script',{'file':str(R/'Docs/Verification/S5/NightmareFoundation/CeilingCrawl/fixtures/CeilingCrewProbe.cs'),'entry':'CeilingCrewProbe.'+method,'args':[]})
 if not r.get('success'):raise RuntimeError(str(r.get('diagnostics')))
 return r['result']
def dist(a,b):return math.sqrt(sum((x-y)**2 for x,y in zip(a,b)))
try:
 host=run(R,'Prepare');deadline=time.monotonic()+20
 while time.monotonic()<deadline:
  guest=run(P,'Facts')
  if guest['beat']==host['beat'] and abs(guest['age']-host['age'])<.02 and dist(guest['position'],host['position'])<.03:break
  time.sleep(.2)
 else:raise RuntimeError('Guest did not match staged host ceiling pose')
 angle=2*math.degrees(math.acos(min(1,abs(sum(x*y for x,y in zip(host['rotation'],guest['rotation']))))))
 assert angle<1 and dist(host['end'],guest['end'])<.03,'Ceiling path or orientation mismatch'
 result={'passed':True,'scope':'Live Photon host snapshot, staged Turn/age; both muted Editors present the same body pose and path. No headset test.','host':host,'guest':guest,'rotationDegrees':angle}
 (out/'result.json').write_text(json.dumps(result,indent=2));print('PASS Shared ceiling position and rotation match')
except Exception as e:
 (out/'result.json').write_text(json.dumps({'passed':False,'error':str(e)},indent=2));raise
