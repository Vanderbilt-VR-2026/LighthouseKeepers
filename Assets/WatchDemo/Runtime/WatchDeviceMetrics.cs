#if DEVELOPMENT_BUILD && UNITY_ANDROID
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Unity.Profiling;

namespace LighthouseKeepers.WatchDemo
{
    // Bounded development-only measurement; no camera or gameplay changes.
    public sealed class WatchDeviceMetrics : MonoBehaviour
    {
        readonly List<XRDisplaySubsystem> displays = new();
        ProfilerRecorder draws, allocations, mainThread;
        string scene;
        float start, next;
        double gpu, cpu;
        int count;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install() { var go=new GameObject("Development GPU measurement"); DontDestroyOnLoad(go); go.AddComponent<WatchDeviceMetrics>(); }
        void Start()
        {
            SubsystemManager.GetSubsystems(displays);
            mainThread=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread");
            draws=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count");
            allocations=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame");
        }
        void Update()
        {
            var current=SceneManager.GetActiveScene().name;
            if(current!=scene) { scene=current; start=Time.realtimeSinceStartup;next=15;gpu=cpu=0;count=0; }
            float elapsed=Time.realtimeSinceStartup-start;
            if(scene!="LK_Lab_Environment" || elapsed<15 || elapsed>50) return;
            foreach(var display in displays)
            {
                if(!display.running || !display.TryGetAppGPUTimeLastFrame(out float seconds) || seconds<=0) continue;
                gpu+=seconds*1000;cpu+=mainThread.Valid?mainThread.LastValue/1000000.0:0;count++;
                if(elapsed<next+1)break;
                display.TryGetDisplayRefreshRate(out float hz);
                var head=Camera.main ? Camera.main.transform : null;
                Debug.Log($"WATCH_PERF build={Application.buildGUID} scene={scene} elapsed={elapsed:F2} samples={count} gpu_ms={gpu/count:F3} main_thread_ms={cpu/count:F3} hz={hz:F1} draws={(draws.Valid?draws.LastValue:-1)} gc_bytes={(allocations.Valid?allocations.LastValue:-1)} head={(head?head.position:Vector3.zero)} forward={(head?head.forward:Vector3.zero)}");
                next=elapsed;gpu=cpu=0;count=0;break;
            }
        }
        void OnDestroy(){draws.Dispose();allocations.Dispose();mainThread.Dispose();}
    }
}
#endif
