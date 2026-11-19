using System;using System.IO;using System.Collections;using System.Collections.Generic;using System.Linq;using System.Reflection;using UnityEngine;using UnityEditor;using LighthouseKeepers.WatchDemo;
public class GuidanceCheckV2:MonoBehaviour {
 string folder;List<string> checks=new();bool failed;
 public static object Run(string p){EditorUtility.audioMasterMute=true;var c=new GameObject("Guidance acceptance").AddComponent<GuidanceCheckV2>();c.folder=p;return "started";}
 void Check(bool ok,string s){checks.Add((ok?"PASS ":"FAIL ")+s);failed|=!ok;}
 IEnumerator Start(){Directory.CreateDirectory(folder);var w=FindAnyObjectByType<WatchDemoController>();w.enabled=false;w.GetComponent<CrewCompanion>().SetCommand(CompanionCommand.Wait);w.GetComponent<QuestRenderQuality>().enabled=false;
 var kit=w.GetComponent<ArrivalEquipment>();foreach(string s in new[]{"radioTaken","lanternTaken","lit"})typeof(ArrivalEquipment).GetField(s,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(kit,true);
 var d=w.State.Director;var pipe=FindAnyObjectByType<WatchPipeRepair>();var engine=FindAnyObjectByType<WatchEngineControls>();var radio=FindAnyObjectByType<WatchRadioConsole>();var beacon=FindAnyObjectByType<WatchBeaconStation>();var valve=FindAnyObjectByType<WatchPressureValve>();
 foreach(var step in new[]{KeeperStep.GrabWrench,KeeperStep.HoldValve,KeeperStep.TightenBolt,KeeperStep.Fuel,KeeperStep.Starter,KeeperStep.HoldStarterAssist,KeeperStep.RadioPower,KeeperStep.RadioTune,KeeperStep.ReadCode,KeeperStep.RadioCode,KeeperStep.RadioTransmit,KeeperStep.BeaconLight,KeeperStep.BeaconAlign,KeeperStep.KeepLight,KeeperStep.Sleep}){
 typeof(ObjectiveDirector).GetProperty("First").SetValue(d,step);typeof(ObjectiveDirector).GetProperty("Second").SetValue(d,step==KeeperStep.Fuel?KeeperStep.HoldValve:KeeperStep.Fuel);yield return new WaitForSeconds(.35f);
 Check(w.CurrentInstruction==step,step+" is local objective");int visible=0,wrong=0,renderedWrong=0;
 foreach(var group in new[]{(pipe.transform,StationObjectiveGuidance.Station.Pipe),(engine.transform,StationObjectiveGuidance.Station.Engine),(radio.transform,StationObjectiveGuidance.Station.Radio),(beacon.transform,StationObjectiveGuidance.Station.Beacon),(valve.transform,StationObjectiveGuidance.Station.Valve)})foreach(var t in group.Item1.GetComponentsInChildren<TextMesh>(true)){
 bool expected=StationObjectiveGuidance.Allows(group.Item2,t.name,step,w.State.Pipe.ActiveBolt);if(t==pipe.prompt)expected=step==KeeperStep.TightenBolt||step>=KeeperStep.BoltA&&step<=KeeperStep.BoltC;
 if(WorldLabelRaster.ObjectiveVisible(t)!=expected)wrong++;var surface=t.transform.Find("Mipmapped writing surface");bool shown=surface&&surface.gameObject.activeInHierarchy&&surface.GetComponent<Renderer>().enabled;if(shown)visible++;if(shown&&!expected)renderedWrong++;
 }
 Check(wrong==0&&renderedWrong==0,step+" no unrelated text/mipmapped ink (visible "+visible+", wrong "+wrong+"/"+renderedWrong+")");
 if(step==KeeperStep.ReadCode||step==KeeperStep.GrabWrench||step==KeeperStep.Sleep)Check(visible==0,step+" station text blank while away from a station objective");
 if(step==KeeperStep.Fuel||step==KeeperStep.RadioPower||step==KeeperStep.RadioCode||step==KeeperStep.BeaconAlign){Transform t=step==KeeperStep.Fuel?engine.transform:step==KeeperStep.BeaconAlign?beacon.transform:radio.transform;Capture(t.TransformPoint(new Vector3(0,1.35f,2.2f)),t.TransformPoint(new Vector3(0,1.3f,.2f)),step+".png");}
 }
 // The filter must release every label on later nights.
 int oldDay=w.State.Definition.day;w.State.Definition.day=2;yield return new WaitForSeconds(.3f);
 Check(engine.GetComponentsInChildren<TextMesh>(true).All(t=>WorldLabelRaster.ObjectiveVisible(t)),"Night 2 restores ordinary station text policy");
 w.State.Definition.day=oldDay;File.WriteAllText(folder+"/guidance.json",JsonUtility.ToJson(new Result{passed=!failed,checks=checks.ToArray()},true));Destroy(gameObject);
 }
 void Capture(Vector3 p,Vector3 target,string file){var w=FindAnyObjectByType<WatchDemoController>();var flashlight=w.Lantern(0);flashlight.transform.position=p;flashlight.transform.LookAt(target);var spot=flashlight.GetComponentInChildren<Light>();spot.enabled=true;var c=new GameObject("Evidence camera").AddComponent<Camera>();c.transform.position=p;c.transform.LookAt(target);c.fieldOfView=55;c.nearClipPlane=.02f;var rt=new RenderTexture(1200,900,24);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(1200,900,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1200,900),0,0);t.Apply();File.WriteAllBytes(folder+"/"+file,t.EncodeToPNG());RenderTexture.active=null;c.targetTexture=null;Destroy(c.gameObject);Destroy(rt);Destroy(t);}
 [Serializable]class Result{public bool passed;public string[] checks;}
}
