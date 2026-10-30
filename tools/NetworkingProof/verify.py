"""Verify each rendered remote pose against a packet sent by the independent client."""
import datetime
import json
import pathlib
import sys

root = pathlib.Path(sys.argv[1])
results = {role: json.loads((root / role / 'result.json').read_text()) for role in ('host', 'client')}
for role, result in results.items():
    assert result['passed'] and result['errors'] == 0, f'{role}: {result}'
    assert result['wasConnected'] and result['received'] >= 10
assert results['host']['processId'] != results['client']['processId']
assert results['host']['disconnectObserved'] and not results['host']['partnerVisibleAtEnd']
assert results['client']['intentionalLeave']
traces = {role: [json.loads(line) for line in (root / role / 'traffic.jsonl').read_text().splitlines()] for role in results}
sent = {row['sequence']: row for row in traces['client'] if row['kind'] == 'tx'}
received = [row for row in traces['host'] if row['kind'] == 'rx']
assert len(received) >= 10
for row in received:
    assert row['sequence'] in sent, 'Host rendered a pose with no matching independent client packet'
    for part in ('head', 'right'):
        for axis in ('x', 'y', 'z'):
            assert abs(row[part][axis] - sent[row['sequence']][part][axis]) < .00001, 'Sent/received poses differ'
assert max(row['head']['z'] for row in received) - min(row['head']['z'] for row in received) > .3, 'No remote lateral movement'
assert max(row['head']['y'] for row in received) - min(row['head']['y'] for row in received) > .3, 'No remote jump'
visibility = [row for row in traces['host'] if row['kind'] == 'visibility']
assert visibility[0]['visible'] and not visibility[-1]['visible']
assert 3.8 < visibility[-1]['elapsed'] < 5.3, 'Departure must be visible inside the five-second clip'
frames = sorted((root / 'host').glob('frame-*.png'))
assert int(frames[-1].stem.split('-')[1]) - int(frames[0].stem.split('-')[1]) >= 74
commands = json.loads((root / 'commands.json').read_text())
leave = next(row for row in commands if row['command']['leave'])
def utc(value): return datetime.datetime.fromisoformat(value.replace('Z', '+00:00'))
latency = (utc(visibility[-1]['utc']) - utc(leave['utc'])).total_seconds()
assert -.05 <= latency < 1.2, 'Avatar did not disappear promptly after client leave'
report = {'passed': True, 'matchedClientPackets': len(received), 'leaveToRemoteHideSeconds': latency,
          'remoteHideAtSeconds': visibility[-1]['elapsed'], 'processes': results,
          'scope': 'Independent input commands applied only to the client; matching Photon pose traffic on the host; actual client leave removes the received avatar. First-person desktop capture, not headset acceptance.'}
(root / 'verification.json').write_text(json.dumps(report, indent=2) + '\n')
print(json.dumps(report, indent=2))
