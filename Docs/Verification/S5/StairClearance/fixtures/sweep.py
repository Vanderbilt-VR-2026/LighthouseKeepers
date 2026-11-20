import sys,time
from pathlib import Path
sys.path.insert(0,'/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop')
from mac_pair import call
root=Path.cwd()
output=Path(sys.argv[1]).resolve()
if output.exists():raise RuntimeError('Use a fresh output path')
output.parent.mkdir(parents=True,exist_ok=True)
fixture=root/'Docs/Verification/S5/StairClearance/fixtures/LinerSweep.cs'
call(root,'eval',{'code':'UnityEditor.EditorUtility.audioMasterMute=true;return true;'})
print(call(root,'run_script',{'file':str(fixture),'entry':'LinerSweep.Begin','args':[str(output)]}),flush=True)
for _ in range(120):
 if output.exists():break
 time.sleep(1)
else:raise RuntimeError('Sweep timed out')
if output.with_suffix(output.suffix+'.error').exists():raise RuntimeError('Sweep failed; see .error file')
print(output.read_text(),flush=True)
