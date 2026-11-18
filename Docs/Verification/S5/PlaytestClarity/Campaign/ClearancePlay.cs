using System;using System.Collections;using System.IO;using UnityEngine;using UnityEditor;using LighthouseKeepers.WatchDemo;
public sealed class ClearancePlay:MonoBehaviour {
 string folder;WatchDemoController w;
 public static object Run(string path){EditorUtility.audioMasterMute=true;var p=new GameObject("Clearance Play check").AddComponent<ClearancePlay>();p.folder=path;return "started";}
 IEnumerator Start(){Directory.CreateDirectory(folder);w=FindAnyObjectByType<WatchDemoController>();var s=w.State;s.EngageWatch();s.CompleteRepair();s.Tick(180);yield return null;
 if(s.Ships.ClearanceGranted||s.Failed){End(false,"Approach did not wait");yield break;}
 var r=s.Radio;r.ToggleMain();r.Tune(r.Program.targetMHz);r.Tick(3,true);for(int i=0;i<r.CodeLength;i++)r.PressCode(r.CodeSymbol(i));r.SetMicrophone(true,true);r.Tick(r.Program.transmitSeconds,true);s.Tick(.1);yield return null;
 if(!s.Ships.ClearanceGranted){End(false,"Transmission did not clear ship");yield break;}
 for(int i=0;i<700;i++){s.AlignBeacon(0);s.Tick(.1);if(i%20==0)yield return null;}
 if(s.Ships.Finished||s.Failed){End(false,"Tutorial passed without aim or failed");yield break;}
 Capture("tutorial-waiting-for-beam");
 for(int i=0;i<1100&&!s.Ships.Finished;i++){s.AlignBeacon(s.ShipWheelTarget);s.Tick(.1);if(i%10==0)yield return null;}
 yield return null;Capture("tutorial-ship-guided");End(s.Ships.Finished&&s.Complete&&!s.Failed,"Actual Night 1 scene: pipe prerequisite, real RadioState sequence/hold, waiting offshore, miss without loss, continuous wheel target then completion. Inputs automated; not human grip or comfort.");}
 void Capture(string name){var c=new GameObject("Evidence view").AddComponent<Camera>();c.transform.SetPositionAndRotation(new Vector3(0,11.65f,3.9f),Quaternion.LookRotation(new Vector3(-55,-4,120)));c.fieldOfView=65;var rt=new RenderTexture(1280,720,24);c.targetTexture=rt;c.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1280,720,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(folder+"/"+name+".png",t.EncodeToPNG());RenderTexture.active=old;Destroy(t);rt.Release();Destroy(rt);Destroy(c.gameObject);}
 void End(bool ok,string detail){File.WriteAllText(folder+"/result.json",JsonUtility.ToJson(new Result{passed=ok,detail=detail,elapsed=w.State.Elapsed,remaining=w.State.Duration-w.State.Elapsed},true));Destroy(gameObject);}
 [Serializable]class Result{public bool passed;public string detail;public double elapsed,remaining;}
}
