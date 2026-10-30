"""Send local input commands only to the client process while the host records."""
import datetime
import json
import os
import pathlib
import sys
import time

root = pathlib.Path(sys.argv[1])
marker = root / 'host' / 'capture-started.txt'
deadline = time.monotonic() + 75
while not marker.exists():
    if time.monotonic() > deadline:
        raise SystemExit('Host never received the client and began capturing')
    time.sleep(.02)
# These are live test inputs for the second process, not an avatar animation in the host.
commands = [
    (0.0, {}), (.25, {'strafe': .9}), (.85, {'strafe': -.9}),
    (1.8, {'wave': True}), (2.35, {'jump': True}), (2.55, {}),
    (3.15, {'forward': .65}), (3.5, {}), (3.9, {'leave': True}),
]
started = time.monotonic()
journal = []
for revision, (at, values) in enumerate(commands, 1):
    remaining = started + at - time.monotonic()
    if remaining > 0:
        time.sleep(remaining)
    command = dict(revision=revision, strafe=0, forward=0, jump=False, wave=False, leave=False)
    command.update(values)
    path = root / 'client' / 'control.json'
    temporary = path.with_suffix('.tmp')
    temporary.write_text(json.dumps(command))
    os.replace(temporary, path)
    journal.append(dict(utc=datetime.datetime.now(datetime.timezone.utc).isoformat(), command=command))
(root / 'commands.json').write_text(json.dumps(journal, indent=2) + '\n')
