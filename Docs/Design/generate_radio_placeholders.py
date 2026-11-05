# Run from the repository root on macOS. Plain voice; effects are applied by Unity.
from pathlib import Path
import re,subprocess,tempfile,sys
root=Path('Assets/Audio/Radio');root.mkdir(parents=True,exist_ok=True)
for name,text in re.findall(r'\| `(keeper_\w+\.wav)` \| [^|]+ \| ([^|]+) \|',Path('Docs/Design/RadioLines.md').read_text()):
 p=root/name
 if not p.exists() or "--force" in sys.argv:
  with tempfile.TemporaryDirectory() as d:
   a=Path(d)/'voice.aiff'
   subprocess.run(['say','-v','Daniel','-r','145','-o',str(a),text.strip()],check=True)
   subprocess.run(['afconvert','-f','WAVE','-d','LEI16@44100','-c','1',str(a),str(p)],check=True)
 print(name,flush=True)
