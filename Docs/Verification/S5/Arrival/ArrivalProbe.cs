using System;using System.Linq;using System.IO;using UnityEngine;using UnityEditor;using Unity.XR.CoreUtils;using TMPro;using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Net;
public static class ArrivalProbe {
 static WatchDemoController W=>UnityEngine.Object.FindAnyObjectByType<WatchDemoController>();
 static InWorldBriefing B=>W.GetComponent<InWorldBriefing>();
 public static object Facts(){var b=B;return new{day=W.State.Day,elapsed=W.State.Elapsed,gate=W.State.WatchGateElapsed,water=W.WaterHeight,held=W.State.BriefingHeld,open=b.IsOpen,page=b.Folder.Page,seen=CrewBriefing.Seen,scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,overflow=b.Folder.GetComponentsInChildren<TMP_Text>(true).Where(t=>{t.ForceMeshUpdate();return t.isTextOverflowing;}).Select(t=>t.name).ToArray()};}
 public static object SimulateReading(){W.State.AdvanceWatchGate(90);W.State.Tick(90);return Facts();}
 public static object Next(){B.Folder.Advance();return Facts();}
 public static object Skip(){B.Folder.Skip();return Facts();}
 public static object Open(){B.Open();return Facts();}
 public static object Capture(string file){var rig=UnityEngine.Object.FindAnyObjectByType<XROrigin>();var c=new GameObject("Arrival receipt camera").AddComponent<Camera>();c.CopyFrom(rig.Camera);c.enabled=false;c.fieldOfView=58;c.transform.SetPositionAndRotation(rig.Camera.transform.position,rig.Camera.transform.rotation);var old=RenderTexture.active;var rt=RenderTexture.GetTemporary(1440,1100,24);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(1440,1100,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1440,1100),0,0);t.Apply();File.WriteAllBytes(file,t.EncodeToPNG());RenderTexture.active=old;c.targetTexture=null;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.Destroy(t);UnityEngine.Object.Destroy(c.gameObject);return true;}
}
