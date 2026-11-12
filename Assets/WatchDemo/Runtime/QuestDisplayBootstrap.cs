using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
using LighthouseKeepers.Core;

namespace LighthouseKeepers.WatchDemo
{
    // Display-only policy; no replicated fields or gameplay state.
    public sealed class QuestDisplayBootstrap : MonoBehaviour
    {
        readonly List<XRDisplaySubsystem> displays = new();
        bool configured;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var root=new GameObject("Quest display baseline");DontDestroyOnLoad(root);root.AddComponent<QuestDisplayBootstrap>();
#endif
        }
        IEnumerator Start()
        {
            Application.targetFrameRate=72;SceneManager.sceneLoaded+=SceneLoaded;
            // XR initialization can complete after the first scene. Retry without per-frame allocation.
            for(int i=0;i<40&&!configured;i++){Configure();yield return new WaitForSecondsRealtime(.5f);}
        }
        void SceneLoaded(Scene scene,LoadSceneMode mode)=>Configure();
        void Configure()
        {
            SubsystemManager.GetSubsystems(displays);
            foreach(var display in displays)if(display.running){display.foveatedRenderingFlags=XRDisplaySubsystem.FoveatedRenderingFlags.None;display.foveatedRenderingLevel=VisualPassSettings.Current?VisualPassSettings.Current.fixedFoveation:.5f;configured=true;}
            foreach(var camera in FindObjectsByType<Camera>(FindObjectsSortMode.None)) {
                camera.allowHDR=false;camera.allowMSAA=true;camera.allowDynamicResolution=false;
                var data=camera.GetUniversalAdditionalCameraData();data.requiresDepthOption=CameraOverrideOption.Off;data.requiresColorOption=CameraOverrideOption.Off;
                // Camera flag is needed by OnTilePostProcessFeature. QuestRenderer.postProcessData is null,
                // so the classic URP post-processing pass remains absent.
                data.renderPostProcessing=true;
            }
            if(configured&&Debug.isDebugBuild)Debug.Log("QUEST_QUALITY runtime: fixed foveation active; MSAA4/scale1 authored; target72, verify actual refresh/GPU in OVR Metrics.");
        }
        void OnDestroy(){SceneManager.sceneLoaded-=SceneLoaded;}
    }
}
