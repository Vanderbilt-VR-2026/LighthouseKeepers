using System.Collections.Generic;using UnityEngine;using UnityEngine.XR;using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo {
 [DefaultExecutionOrder(1500)]public sealed class QuestRenderQuality:MonoBehaviour {
  WatchBeacon beacon;readonly List<Light> lights=new();readonly List<XRDisplaySubsystem> displays=new();float nextScan;
  public static void Create(WatchDemoController watch){var s=VisualPassSettings.Current;if(!s||!s.renderQuality)return;watch.gameObject.AddComponent<QuestRenderQuality>();}
  void Start(){beacon=FindAnyObjectByType<WatchBeacon>();foreach(var camera in FindObjectsByType<Camera>(FindObjectsSortMode.None)){camera.nearClipPlane=.04f;camera.useOcclusionCulling=false;camera.allowMSAA=true;}SubsystemManager.GetSubsystems(displays);foreach(var display in displays){display.foveatedRenderingFlags=XRDisplaySubsystem.FoveatedRenderingFlags.None;display.foveatedRenderingLevel=VisualPassSettings.Current.fixedFoveation;}}
  void LateUpdate(){if(!Resources.Load<TextAsset>("VisualLightingReady"))return;if(Time.time>=nextScan){nextScan=Time.time+2;lights.Clear();lights.AddRange(FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None));}foreach(var light in lights)if(light&&(!beacon||light!=beacon.lantern))light.enabled=false;}
 }
}