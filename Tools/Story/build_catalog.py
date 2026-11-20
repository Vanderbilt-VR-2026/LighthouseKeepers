#!/usr/bin/env python3
"""Compile the approved Markdown dialogue without editing the user's recordings.
Run from repo root. Cue numbers are append-only; original 1..104 are untouched.
"""
from pathlib import Path
import json, re, argparse
ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT/'Docs/Story/02-Voice-Script.md'
DEST = ROOT/'Assets/Data/Strings/Resources/StoryVoiceCatalog.json'

def compile_rows():
    previous = {e['id']: e for e in json.loads(DEST.read_text())['entries']} if DEST.exists() else {}
    next_cue = max([199] + [e['cue'] for e in previous.values()]) + 1
    entries = []
    for line in SOURCE.read_text().splitlines():
        if not re.match(r'\| (HM|ANT)_', line): continue
        row = [v.strip() for v in line.strip('|').split('|')]
        if len(row) != 10: raise ValueError(f'Malformed dialogue row: {line}')
        keys = ('id','speaker','text','delivery','trigger','prerequisite','priority','repeat','suppress','mode')
        e = dict(zip(keys,row)); e['priority'] = int(e['priority'][1:])
        if e['id'] in previous: e['cue'] = previous[e['id']]['cue']
        else: e['cue'] = next_cue; next_cue += 1
        e['file'] = 'Assets/Audio/Radio/Resources/StoryRadio/' + e['id'].lower() + '.wav'
        e['resource'] = 'StoryRadio/' + e['id'].lower()
        entries.append(e)
    assert len({e['id'] for e in entries}) == len(entries), 'Duplicate IDs'
    assert len({e['cue'] for e in entries}) == len(entries), 'Duplicate cue integers'
    assert len(entries) == 112 and sum(e['speaker']=='HM' for e in entries) == 89
    return entries

def main():
    parser=argparse.ArgumentParser();parser.add_argument('--check',action='store_true');args=parser.parse_args()
    entries=compile_rows();text=json.dumps({'schema':1,'entries':entries},ensure_ascii=False,indent=2)+'\n'
    if args.check:
        assert DEST.read_text() == text, 'Run python3 Tools/Story/build_catalog.py'
        print(f'Approved script matches {len(entries)} unique runtime captions (HM89 / ANT23).');return
    DEST.parent.mkdir(parents=True,exist_ok=True);DEST.write_text(text)
    # Recording sheets are user-authored; never regenerate or overwrite SHEET 0.
    print(f'Wrote {len(entries)} lines; cue range {entries[0]["cue"]}–{entries[-1]["cue"]}. No audio was generated or relabelled.')
if __name__=='__main__':main()
