#!/usr/bin/env python3
"""Read-only repository hygiene check. Run from the Lighthouse project root."""
from pathlib import Path
import subprocess
import sys
root = Path.cwd()
errors = []
if not (root / 'ProjectSettings/ProjectVersion.txt').exists():
    raise SystemExit('Run from the Unity project root')
tracked = subprocess.check_output(['git', 'ls-files', '-z']).decode().split('\0')
for name in filter(None, tracked):
    if name.split('/')[0].lower() in {'library', 'temp', 'logs', 'obj', 'build', 'builds', 'usersettings'}:
        errors.append('Forbidden generated path tracked: ' + name)
for meta in (root / 'Assets').rglob('*.meta'):
    if not Path(str(meta)[:-5]).exists():
        errors.append('Orphan metadata: ' + str(meta.relative_to(root)))
for asset in (root / 'Assets/_LighthouseKeepers').rglob('*'):
    if asset.name.startswith('.') or asset.suffix == '.meta':
        continue
    if not Path(str(asset) + '.meta').exists():
        errors.append('Missing metadata: ' + str(asset.relative_to(root)))
manifest = (root / 'Packages/manifest.json').read_text()
for forbidden in ['com.unity.ai.assistant', 'com.unity.netcode.gameobjects', 'com.unity.services.vivox']:
    if forbidden in manifest:
        errors.append('Forbidden dependency: ' + forbidden)
print('\n'.join(errors) if errors else 'PASS: no generated cache paths tracked; asset/meta pairs present; forbidden dependencies absent.')
sys.exit(bool(errors))
