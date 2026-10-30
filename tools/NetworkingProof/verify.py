"""Check independent-process transport evidence before encoding the first-person clip."""
import json
import pathlib
import sys

root = pathlib.Path(sys.argv[1])
results = {}
for role in ('host', 'client'):
    result = json.loads((root / role / 'result.json').read_text())
    assert result['passed'], f'{role} runtime acceptance failed: {result}'
    assert result['received'] >= 20 and result['maxRemoteHandMotion'] > .05
    assert result['partnerConnected'] and result['handContactObserved'] and result['errors'] == 0
    frames = sorted((root / role).glob('frame-*.png'))
    assert len(frames) >= 60, f'{role}: too few graphical frames'
    assert int(frames[-1].stem.split('-')[1]) - int(frames[0].stem.split('-')[1]) >= 74
    results[role] = result
assert results['host']['processId'] != results['client']['processId']
report = {'passed': True, 'processes': results, 'scope': 'Two macOS Unity processes; real Photon traffic; scripted local hands; received partner; desktop first-person camera. No headset acceptance.'}
(root / 'verification.json').write_text(json.dumps(report, indent=2) + '\n')
print(json.dumps(report, indent=2))
