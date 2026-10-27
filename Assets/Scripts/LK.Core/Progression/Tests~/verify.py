#!/usr/bin/env python3
"""Offline rules checks and C# API compilation using Unity's bundled compiler/reference DLLs.

Does not substitute for Unity import, serialization, Play Mode, Android build or headset checks.
No packages or SDK downloads; source assets are read, not rewritten.
"""
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[5]
UNITY = Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
DOTNET = UNITY / 'NetCoreRuntime/dotnet'
CSC = UNITY / 'DotNetSdkRoslyn/csc.dll'
MONO = UNITY / 'MonoBleedingEdge/bin/mono'
REFERENCES = UNITY / 'MonoBleedingEdge/lib/mono/4.8-api'
MANAGED = UNITY / 'Managed/UnityEngine'
OUTPUT = ROOT / 'artifacts/progression-checks'
CORE = ROOT / 'Assets/Scripts/LK.Core/Progression'
BOARD = ROOT / 'Assets/Scripts/LK.Presentation/Board'


def run(arguments):
    result = subprocess.run([str(arg) for arg in arguments], cwd=ROOT, text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    print(result.stdout, end='')
    if result.returncode:
        raise SystemExit(result.returncode)


def compile_cs(name, sources, refs=(), defines=(), executable=False):
    target = OUTPUT / (name + ('.exe' if executable else '.dll'))
    args = ['-nologo', '-nostdlib+', '-langversion:9.0', '-warnaserror+', '-nowarn:0649',
            '-target:' + ('exe' if executable else 'library'), '-out:' + str(target)]
    if defines:
        args += ['-define:' + ';'.join(defines)]
    framework = sorted((UNITY / 'NetStandard/ref/2.1.0').glob('*.dll')) if refs else [REFERENCES / name for name in ('mscorlib.dll', 'System.dll', 'System.Core.dll')]
    args += ['-r:' + str(ref) for ref in framework]
    args += ['-r:' + str(ref) for ref in refs]
    args += [str(source) for source in sources]
    rsp = OUTPUT / (name + '.rsp')
    rsp.write_text('\n'.join('"' + arg + '"' for arg in args) + '\n')
    run([DOTNET, CSC, '-noconfig', '@' + str(rsp)])
    print('PASS C# compile: ' + name)
    return target


def scalar(text, name):
    match = re.search(r'^  ' + re.escape(name) + r':\s*(.*)$', text, re.M)
    if match is None:
        raise AssertionError('Missing asset field ' + name)
    return match.group(1)


def fixture():
    """Read the real authored YAML scalars/enum arrays into a rules fixture.

    This deliberately does not claim to exercise Unity's YAML importer.
    """
    source = ['using System;', 'using LighthouseKeepers.Core.Progression;', 'static class AssetPlans {',
              'public static DayPlan Load(int day) { switch(day) {']
    for day in (1, 2, 3):
        asset = ROOT / f'Assets/Data/Days/Day{day:02}.asset'
        text = asset.read_text()
        objectives = []
        for label, codes, incident in re.findall(r'^  - text: (.*)\n    predicates: ([0-9a-f]+)\n    incidentId: (.*)$', text, re.M):
            raw = bytes.fromhex(codes)
            assert len(raw) % 4 == 0
            predicates = ','.join('(DayPredicate)' + str(int.from_bytes(raw[i:i + 4], 'little')) for i in range(0, len(raw), 4))
            objectives.append('new ObjectiveDefinition { text = ' + label + ', predicates = new[]{' + predicates + '}, incidentId = ' + incident + ' }')
        incidents = ['new IncidentDefinition { id = ' + label + ', atSeconds = ' + seconds + 'f, kind = (DayIncidentKind)' + kind + ' }'
                     for label, seconds, kind in re.findall(r'^  - id: (.*)\n    atSeconds: ([0-9.]+)\n    kind: ([0-9]+)$', text, re.M)]
        assert objectives and incidents
        source += [f'case {day}: return new DayPlan {{ Day={scalar(text,"day")}, RequiredKeepers={scalar(text,"requiredKeepers")},',
                   f'Heading={scalar(text,"briefingHeading")}, Briefing={scalar(text,"briefingText")}, FaultObjectivePrefix={scalar(text,"faultObjectivePrefix")},',
                   'Objectives=new[]{' + ','.join(objectives) + '}, Incidents=new[]{' + ','.join(incidents) + '},',
                   f'RequiresPartnerIsolation={"true" if scalar(text,"requiresPartnerIsolation")=="1" else "false"},',
                   f'PrimerStrokes={scalar(text,"primerStrokes")}, CrankCooldownSeconds={scalar(text,"crankCooldownSeconds")}f,',
                   f'ChokeReleaseSeconds={scalar(text,"chokeReleaseSeconds")}f, IsolationFreshnessSeconds={scalar(text,"isolationFreshnessSeconds")}f,',
                   f'Success=(DaySuccessCondition){scalar(text,"successCondition")}, Failure=(DayFailureCondition){scalar(text,"failCondition")} }};']
        script_guid = re.search(r'^guid: (\w+)', (CORE / 'DayDefinition.cs.meta').read_text(), re.M).group(1)
        assert script_guid in text
    source += ['default: throw new ArgumentOutOfRangeException(nameof(day)); } } }']
    target = OUTPUT / 'AssetPlans.g.cs'
    target.write_text('\n'.join(source) + '\n')
    catalog = (ROOT / 'Assets/Data/Days/DayCatalog.asset').read_text()
    for day in (1, 2, 3):
        meta = (ROOT / f'Assets/Data/Days/Day{day:02}.asset.meta').read_text()
        assert re.search(r'^guid: (\w+)', meta, re.M).group(1) in catalog
    print('PASS authored day scalar, enum, script and catalog reference checks (not Unity deserialization)')
    return target


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    generated = fixture()
    pure = compile_cs('DayFlowScenarios', [CORE / 'DayContracts.cs', CORE / 'DayFlow.cs',
                                          BOARD / 'DayBoardContent.cs', Path(__file__).with_name('DayFlowScenarios.cs'), generated], executable=True)
    run([MONO, pure])
    unity_refs = sorted(MANAGED.glob('UnityEngine*.dll'))
    core_sources = sorted(CORE.glob('*.cs'))
    board_sources = sorted(BOARD.glob('*.cs'))
    for config, defines in [('Release', []), ('Development', ['DEVELOPMENT_BUILD'])]:
        core_dll = compile_cs('LK.Core.Progression.' + config, core_sources, unity_refs, defines)
        compile_cs('LK.Presentation.Board.' + config, board_sources, unity_refs + [core_dll], defines)
    editor_refs = unity_refs + sorted(MANAGED.glob('UnityEditor*.dll')) + [OUTPUT / 'LK.Core.Progression.Development.dll']
    compile_cs('LK.Core.Progression.Editor', sorted((CORE / 'Editor').glob('*.cs')), editor_refs, ['UNITY_EDITOR'])
    assert b'JumpToDay' not in (OUTPUT / 'LK.Core.Progression.Release.dll').read_bytes()
    assert b'JumpToDay' in (OUTPUT / 'LK.Core.Progression.Development.dll').read_bytes()
    print('PASS day selector method absent from release assembly, present in development assembly')
    print('PASS offline verification; Unity import, APK, headset and integrated APIs remain unverified.')


if __name__ == '__main__':
    main()
