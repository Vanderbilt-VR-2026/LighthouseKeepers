import json, os, sys, time
from pathlib import Path

sys.path.insert(0, '/Users/aneeshvasamreddy/LighthouseKeepers-Coop-Basics/artifacts/coop')
from mac_pair import call

root = Path.cwd()
out = Path(os.environ['LK_TEST_OUT'])
out.mkdir(parents=True, exist_ok=False)
call(root, 'eval', {'code': 'UnityEditor.EditorUtility.audioMasterMute=true;UnityEditor.AssetDatabase.Refresh();return true;'})
for attempt in range(120):
    try:
        result = call(root, 'eval', {'code': 'UnityEditor.EditorUtility.audioMasterMute=true;return !UnityEditor.EditorApplication.isCompiling&&!UnityEditor.EditorUtility.scriptCompilationFailed;'})
        if result['result'] and attempt > 2:
            break
    except (OSError, RuntimeError, KeyError):
        pass
    time.sleep(1)
else:
    raise RuntimeError('Editor did not compile')

call(root, 'run_tests', {'mode':'playmode', 'filter':'ShellTraversalTests', 'filter_type':'testName', 'async_tests':True, 'timeout':600})
for attempt in range(600):
    time.sleep(1)
    try:
        result = call(root, 'test_status')
    except (OSError, RuntimeError, KeyError):
        continue
    if result.get('status') == 'completed':
        (out/'result.json').write_text(json.dumps(result, indent=2) + '\n')
        print(json.dumps(result.get('summary')), flush=True)
        if result['summary']['total'] != 1 or result['summary']['failed']:
            raise RuntimeError('Traversal test failed; see result.json')
        break
else:
    raise RuntimeError('Traversal test did not finish')
