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
        ProfilerRecorder draws, allocations, mainThread, audioDsp;
        string scene;
        float start, next;
        float maxFrameMs;
        bool extended;
        double gpu, cpu, appCpu;int appCpuCount;
        int count;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install() { var go=new GameObject("Development GPU measurement"); DontDestroyOnLoad(go); go.AddComponent<WatchDeviceMetrics>(); }
        void Start()
        {
            extended=LighthouseKeepers.Core.S4Tuning.Current;
            SubsystemManager.GetSubsystems(displays);
            mainThread=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread");
            try{audioDsp=ProfilerRecorder.StartNew(ProfilerCategory.Audio,"MasterDSP");}catch(System.Exception){Debug.LogWarning("AUDIO_PERF MasterDSP counter unavailable");}
            draws=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count");
            allocations=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame");
        }
        void Update()
        {
            var current=SceneManager.GetActiveScene().name;
            if(current!=scene) { scene=current; start=Time.realtimeSinceStartup;next=15;gpu=cpu=0;count=0; }
            float elapsed=Time.realtimeSinceStartup-start;
            if(scene=="LK_Lab_Environment" && elapsed>(extended?1800:50)){Destroy(gameObject);return;}
            if(scene!="LK_Lab_Environment" || elapsed<15) return;
            maxFrameMs=Mathf.Max(maxFrameMs,Time.unscaledDeltaTime*1000);
            foreach(var display in displays)
            {
                if(!display.running || !display.TryGetAppGPUTimeLastFrame(out float seconds) || seconds<=0) continue;
                if(UnityEngine.XR.Provider.XRStats.TryGetStat(display,"perfmetrics.appcputime",out float ms)&&ms>0){appCpu+=ms;appCpuCount++;}
                gpu+=seconds*1000;cpu+=mainThread.Valid?mainThread.LastValue/1000000.0:0;count++;
                if(elapsed<next+1)break;
                display.TryGetDisplayRefreshRate(out float hz);
                var head=Camera.main ? Camera.main.transform : null;
                Debug.Log($"WATCH_PERF build={Application.buildGUID} scene={scene} elapsed={elapsed:F2} samples={count} gpu_ms={gpu/count:F3} main_thread_ms={cpu/count:F3} hz={hz:F1} draws={(draws.Valid?draws.LastValue:-1)} gc_bytes={(allocations.Valid?allocations.LastValue:-1)} head={(head?head.position:Vector3.zero)} forward={(head?head.forward:Vector3.zero)}");
                if(extended)
                {
                    var watch=FindAnyObjectByType<WatchDemoController>();var voice=LighthouseKeepers.Lobby.CrewVoiceBridge.Current;
                    Debug.Log($"S4_PERF day={(watch?watch.State.Day:0)} fps={count/(elapsed-next):F1} max_frame_ms={maxFrameMs:F2} voice={(voice!=null?voice.StatusText:"OFF")} focus={Application.isFocused}");
                }
                Debug.Log($"CPU_PERF app_cpu_ms={(appCpuCount>0?appCpu/appCpuCount:-1):F3} samples={appCpuCount}");appCpu=0;appCpuCount=0;
                var ceiling=FindAnyObjectByType<LighthouseKeepers.Core.AudioCeiling>();
                Debug.Log($"AUDIO_PERF dsp_valid={audioDsp.Valid} dsp_ms={(audioDsp.Valid?audioDsp.LastValue/1000000.0:-1):F4} ceiling_ms={(ceiling?ceiling.MeanCpuMs:-1):F4} real_voice_cap={AudioSettings.GetConfiguration().numRealVoices}");
                next=elapsed;gpu=cpu=0;count=0;maxFrameMs=0;break;
            }
        }
        void OnDestroy(){draws.Dispose();allocations.Dispose();mainThread.Dispose();audioDsp.Dispose();}
    }
}
#endif
