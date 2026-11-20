import sys,time,json,os
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop');from mac_pair import call
R=Path.cwd();P=Path('/Users/aneeshvasamreddy/LighthouseKeepers-S5-Peer');O=Path(os.environ['LK_TEST_OUT']);O.mkdir(exist_ok=False)
def run(r,role):
 x=call(r,'run_script',{'file':str(R/'Docs/Verification/S5/NightmareFoundation/Drop/fixtures/NaturalCrewDebug.cs'),'entry':'NaturalCrewDebug.Begin','args':[str(O/role),role]})
 if not x.get('success'):raise RuntimeError(str(x.get('diagnostics')))
 print(role,'started',flush=True)
try:
 run(R,'lead');run(P,'follow');end=time.monotonic()+145
 while time.monotonic()<end:
  if (O/'lead'/'result.json').exists() and (O/'follow'/'result.json').exists():break
  time.sleep(.5)
 else:raise RuntimeError('Natural sequence timed out')
 a=json.loads((O/'lead'/'result.json').read_text());b=json.loads((O/'follow'/'result.json').read_text());print('RESULT',a,b,flush=True)
 if not a['passed'] or not b['passed']:raise AssertionError('Natural sequence failed')
 (O/'result.json').write_text(json.dumps({'passed':True,'scope':'Two live Photon Editors, natural shared Wake through Follow/Turn/Escape and Return; virtual head/stick/buttons/hands; initial state from explicit retry.'},indent=2))
except Exception as e:
 print('FAILED',type(e).__name__,str(e),flush=True);(O/'result.json').write_text(json.dumps({'passed':False,'error':str(e)},indent=2));raise
