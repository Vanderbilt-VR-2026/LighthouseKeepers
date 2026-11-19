using System;using System.IO;using System.Collections;using System.Linq;using UnityEngine;using UnityEditor;using Unity.XR.CoreUtils;using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Core.Progression;using LighthouseKeepers.Flood;
public class WakeDryProbe:MonoBehaviour{
 string folder;int failures;WatchDemoController w;XROrigin rig;Transform head;NightmareScene scene;
 public static object Begin(string folder){EditorUtility.audioMasterMute=true;Directory.CreateDirectory(folder);var p=new GameObject("Nightmare wake and restoration probe").AddComponent<WakeDryProbe>();p.folder=folder;p.StartCoroutine(p.Go());return true;}
 void Check(bool ok,string text){File.AppendAllText(folder+"/checks.txt",(ok?"PASS ":"FAIL ")+text+"\n");if(!ok)failures++;}
 void MoveHead(Vector3 p){var cc=rig.GetComponent<CharacterController>();if(cc)cc.enabled=false;rig.MoveCameraToWorldLocation(p);Physics.SyncTransforms();}
 IEnumerator Go(){
  w=FindAnyObjectByType<WatchDemoController>();scene=w.GetComponent<NightmareScene>();var s=w.State;HorrorTuning.Current.nightmareEnabled=true;
  w.enabled=false;s.SetScriptedPartner(false);s.EngageWatch();for(int n=0;n<70;n++)s.PourFuel(.1f);
  for(int n=0;n<3;n++){for(int i=0;i<100&&!s.StarterInWindow;i++)s.Tick(.05);s.PullStarterAs(1);s.Tick(.4);}
  s.CompleteRepair();s.SetCircuit(2,true);s.Radio.ToggleMain();s.Radio.Tune(s.Radio.TargetMHz);s.Tick(3);for(int n=0;n<s.Radio.CodeLength;n++)s.Radio.PressCode(s.Radio.CodeSymbol(n));s.Radio.SetMicrophone(true,true);s.Tick(5);s.IgniteBeacon(true);
  for(int i=0;i<5000&&!s.Complete&&!s.Failed;i++){s.AlignBeacon(s.ShipWheelTarget);s.Tick(.1);}Check(s.Complete&&!s.Failed,"Completed Day 2 using station and ship rules");if(failures>0)yield break;w.enabled=true;
  rig=FindAnyObjectByType<XROrigin>();head=rig.Camera.transform;foreach(var b in rig.GetComponentsInChildren<Behaviour>(true))if(b.GetType().Name.Contains("TrackedPose")||b.GetType().Name.Contains("InputModality"))b.enabled=false;
  MoveHead(w.BedPosition);head.rotation=Quaternion.LookRotation(Vector3.right);yield return null;Check(w.TrySleep(),"Actual bedtime action starts nightmare");
  yield return new WaitForSeconds(HorrorTuning.Current.nightmareWakeSeconds+1);var radio=FindAnyObjectByType<HarborRadio>();Check(s.Scare.Beat==ScareBeat.Wake,"Wake waits while player looks away");Check(radio.LastCaption!="Can you come with me? I'm scared.","Child line waits until player looks");
  head.LookAt(s.Scare.Body+Vector3.up*1.1f);yield return new WaitForSeconds(.5f);Check(s.Scare.Beat==ScareBeat.Follow,"Child starts after gaze from the actual bunk position");Check(radio.LastCaption=="Can you come with me? I'm scared."&&radio.LastSpeaker=="Unknown","Gaze begins the child caption as Unknown");
  var flood=FindAnyObjectByType<FloodController>();var water=GameObject.Find("Visible first-floor flood").GetComponent<Renderer>();var house=GameObject.Find("House flood through tower doorway").GetComponentsInChildren<Renderer>(true);
  scene.enabled=false;for(int i=0;i<200&&s.Scare.Beat!=ScareBeat.Escape;i++)s.Scare.Tick(.1f,true,true,0,0);w.enabled=false;flood.SetHeight(.2f);s.HouseFlood.Apply(1f);float volume=s.HouseFlood.Volume;MoveHead(new Vector3(0,1.65f,-10.6f));s.Scare.Tick(.1f,false,false,(int)w.CurrentDuty,0);scene.enabled=true;yield return new WaitForSeconds(.3f);
  Check(s.Scare.Beat==ScareBeat.Return&&s.Scare.DryReturn,"Outside starts the dry return");Check(!water.enabled&&house.All(x=>!x.enabled),"Tower and House flood renderers are hidden during return");Check(float.IsNegativeInfinity(w.WaterHeightAt(new Vector3(0,0,0))),"Dry return does not wet equipment");Check(Mathf.Abs(flood.Height-.2f)<.001f&&Mathf.Abs(s.HouseFlood.Volume-volume)<.001f,"Underlying tower and House water are preserved");
  MoveHead(new Vector3(0,1.65f,-8));yield return new WaitForSeconds(.3f);Check(s.Scare.Beat==ScareBeat.Relief&&!water.enabled,"Returning inside holds the dry presentation through relief");
  float until=Time.time+HorrorTuning.Current.nightmareReliefSeconds+2;while(Time.time<until&&!s.Scare.Finished)yield return null;yield return null;Check(s.Scare.Finished&&water.enabled,"Tower renderer restores when nightmare ends");Check(Mathf.Abs(flood.Height-.2f)<.001f&&Mathf.Abs(s.HouseFlood.Volume-volume)<.001f,"Finishing does not discard the real flood state");
  File.WriteAllText(folder+"/result.json","{\"passed\":"+(failures==0?"true":"false")+",\"failures\":"+failures+"}");Destroy(gameObject);
 }
}
