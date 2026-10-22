#!/usr/bin/env python3
"""Compile B's assemblies and run its NUnit methods using an installed Unity toolchain.

No Editor, scene, APK or headset execution is implied. The package assembly cache is
read-only and supplies only installed UI/XRI binary dependencies, never game code.
"""
import argparse
import os
from pathlib import Path
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--unity', type=Path, required=True, help='Unity.app/Contents directory')
    parser.add_argument('--assembly-cache', type=Path, required=True, help='Library/ScriptAssemblies with UI/XRI DLLs')
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[4]
    os.chdir(root)
    output = root / 'artifacts' / 'standalone'
    output.mkdir(parents=True, exist_ok=True)
    scripting = args.unity / 'Resources/Scripting'
    mono = scripting / 'MonoBleedingEdge/bin/mono'
    csc = scripting / 'MonoBleedingEdge/lib/mono/4.5/csc.exe'
    managed = scripting / 'Managed'
    engine = managed / 'UnityEngine'
    nunit = args.unity / 'Resources/PackageManager/BuiltInPackages/com.unity.ext.nunit/net40/unity-custom/nunit.framework.dll'
    engine_refs = list(engine.glob('UnityEngine*.dll')) + list(engine.glob('UnityEditor*.dll'))
    engine_refs += [scripting / 'MonoBleedingEdge/lib/mono/4.5/Facades/netstandard.dll']
    package_refs = [args.assembly_cache / 'UnityEngine.UI.dll', args.assembly_cache / 'Unity.XR.Interaction.Toolkit.dll']
    core = root / 'Assets/Scripts/LK.Core/Simulation'

    def compile_assembly(name, sources, references=(), defines='UNITY_EDITOR', executable=False):
        target = output / (name + ('.exe' if executable else '.dll'))
        command = [str(mono), str(csc), '-nologo', '-target:' + ('exe' if executable else 'library'),
                   '-define:' + defines, '-out:' + str(target)]
        command += ['-r:' + str(ref) for ref in dict.fromkeys(references)]
        command += [str(source) for source in sources]
        result = subprocess.run(command, text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
        if result.stdout:
            print(result.stdout, end='', flush=True)
        if result.returncode:
            raise SystemExit(result.returncode)
        print('COMPILED ' + target.name, flush=True)
        return target

    core_dll = compile_assembly('LK.Core.Simulation', core.glob('*.cs'))
    compile_assembly('LK.Core.Simulation.Release', core.glob('*.cs'), defines='LK_STANDALONE_RELEASE')
    unity_dll = compile_assembly('LK.Simulation.Unity', (core / 'Unity').glob('*.cs'),
                                 [core_dll] + engine_refs + package_refs)
    compile_assembly('LK.Simulation.Unity.Release', (core / 'Unity').glob('*.cs'),
                     [output / 'LK.Core.Simulation.Release.dll'] + engine_refs + package_refs,
                     defines='LK_STANDALONE_RELEASE')
    editor_dll = compile_assembly('LK.Simulation.Editor', (core / 'Unity/Editor').glob('*.cs'),
                     [core_dll, unity_dll] + engine_refs)
    tests = root / 'Assets/Tests/EditMode/Simulation'
    test_dll = compile_assembly('LK.Simulation.Tests', tests.glob('*.cs'),
                               [core_dll, unity_dll, editor_dll, nunit] + engine_refs + package_refs +
                               [args.assembly_cache / name for name in ('Unity.TextMeshPro.dll', 'UnityEngine.TestRunner.dll', 'UnityEditor.TestRunner.dll')])
    runner = compile_assembly('SimulationStandalone', [tests / 'StandaloneRunner.cs'],
                              [test_dll, core_dll, nunit], defines='LK_SIMULATION_STANDALONE', executable=True)
    env = os.environ.copy()
    env['MONO_PATH'] = os.pathsep.join(map(str, [output, engine, managed, nunit.parent, args.assembly_cache]))
    result = subprocess.run([str(mono), str(runner)], env=env, text=True,
                            stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    print(result.stdout, end='')
    (output / 'results.txt').write_text(result.stdout)
    return result.returncode


if __name__ == '__main__':
    raise SystemExit(main())
