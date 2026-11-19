using System;using System.Collections;using System.Collections.Generic;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEngine.XR;using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;using UnityEngine.XR.Interaction.Toolkit.Interactables;using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Gameplay.Interaction;using LighthouseKeepers.Core;using LighthouseKeepers.Core.Progression;using LighthouseKeepers.Interaction;
public class BeltCheckV2:MonoBehaviour {
 string folder;HandRig hand;List<string> checks=new();bool failed;KeeperToolBelt belt;
 public static object Run(string p){EditorUtility.audioMasterMute=true;var c=new GameObject("Tool belt acceptance").AddComponent<BeltCheckV2>();c.folder=p;return "started";}
 void Check(bool x,string s){checks.Add((x?"PASS ":"FAIL ")+s);failed|=!x;}
 IEnumerator Start(){Directory.CreateDirectory(folder);var w=FindAnyObjectByType<WatchDemoController>();w.enabled=false;w.GetComponent<CrewCompanion>().SetCommand(CompanionCommand.Wait);w.GetComponent<QuestRenderQuality>().enabled=false;belt=w.GetComponent<KeeperToolBelt>();var rig=FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
 foreach(var m in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager>(true))m.enabled=false;
 foreach(var h in rig.GetComponentsInChildren<HandRig>(true))h.transform.parent.gameObject.SetActive(false);
 hand=rig.GetComponentsInChildren<HandRig>(true).First(h=>h.Node==XRNode.RightHand);hand.transform.parent.gameObject.SetActive(true);foreach(var t in hand.transform.parent.GetComponents<UnityEngine.InputSystem.XR.TrackedPoseDriver>())t.enabled=false;hand.enabled=false;
 var d=hand.Direct;d.enabled=true;d.selectInput=new XRInputButtonReader("test grip",inputSourceMode:XRInputButtonReader.InputSourceMode.ManualValue);d.selectInput.QueueManualState(false,0);d.activateInput=new XRInputButtonReader("test trigger",inputSourceMode:XRInputButtonReader.InputSourceMode.ManualValue);d.activateInput.QueueManualState(false,0,false,true);
 var wrench=FindAnyObjectByType<WatchPipeRepair>().wrench;var items=new[]{w.Lantern(0).Grab,wrench,w.Lantern(1).Grab,FindObjectsByType<HandheldCrewRadio>(FindObjectsSortMode.None).OrderBy(r=>r.name).First().GetComponent<XRGrabInteractable>()};
 for(int i=0;i<items.Length;i++){
  var g=items[i];var tool=g.GetComponent<INetworkPropCarry>();var slot=g.GetComponent<INetworkBeltSlot>().BeltSlot;g.transform.SetPositionAndRotation(belt.Head.position+Vector3.forward*.7f-Vector3.up*.3f,Quaternion.identity);g.GetComponent<Rigidbody>().isKinematic=true;hand.transform.parent.rotation=Quaternion.identity;hand.transform.parent.position=g.transform.position;Physics.SyncTransforms();yield return new WaitForSeconds(.4f);d.selectInput.QueueManualState(true,1);yield return new WaitForSeconds(.5f);Check(g.isSelected,g.name+" direct grip selects");
  for(int n=0;n<8;n++){hand.transform.parent.position+=ToolBeltPose.Position(slot,belt.Head.position,belt.Head.rotation)-g.transform.position;yield return null;}
  d.selectInput.QueueManualState(false,0);yield return new WaitForSeconds(.4f);Check(tool.Carried==(i!=2),g.name+(i==2?" duplicate flashlight rejected":" clipped on release"));
  if(i!=2){Check(g.GetComponent<Rigidbody>().isKinematic,g.name+" clipped body stays kinematic");Check(Vector3.Distance(g.transform.position,ToolBeltPose.Position(slot,belt.Head.position,belt.Head.rotation))<.005f,g.name+" correct slot");}
  hand.transform.parent.position=belt.Head.position+Vector3.up;yield return new WaitForSeconds(.2f);
 }
 Check(Vector3.Distance(w.Lantern(0).transform.position,wrench.transform.position)>.30f,"flashlight and wrench separated");
 var centre=belt.Head.position-Vector3.up*.75f;var light=new GameObject("Belt inspection light - test only").AddComponent<Light>();light.type=LightType.Point;light.range=3;light.intensity=1.3f;light.transform.position=centre+Vector3.forward*.7f+Vector3.up*.3f;Capture(centre+Vector3.forward*.8f+Vector3.up*.4f,centre,"belt-front.png");Capture(centre+Vector3.left*.85f+Vector3.up*.4f,centre,"belt-left.png");Destroy(light.gameObject);
 foreach(var g in new[]{w.Lantern(0).Grab,wrench}){
  hand.transform.parent.position=g.transform.position;Physics.SyncTransforms();yield return new WaitForSeconds(.4f);d.selectInput.QueueManualState(true,1);yield return new WaitForSeconds(.4f);Check(g.isSelected&&!g.GetComponent<BeltTool>().Carried,g.name+" regrip detaches belt");
  hand.transform.parent.position=belt.Head.position+Vector3.forward*.8f;yield return new WaitForSeconds(.3f);d.selectInput.QueueManualState(false,0);yield return new WaitForSeconds(.4f);Check(!g.GetComponent<BeltTool>().Carried,g.name+" ordinary release outside belt does not snap");hand.transform.parent.position=belt.Head.position+Vector3.up;yield return new WaitForSeconds(.2f);
 }
 File.WriteAllText(folder+"/belt.json",JsonUtility.ToJson(new Result{passed=!failed,checks=checks.ToArray()},true));Destroy(gameObject);
 }
 void Capture(Vector3 p,Vector3 target,string file){var c=new GameObject("Evidence camera").AddComponent<Camera>();c.transform.position=p;c.transform.LookAt(target);c.fieldOfView=55;c.nearClipPlane=.02f;var rt=new RenderTexture(1200,900,24);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(1200,900,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1200,900),0,0);t.Apply();File.WriteAllBytes(folder+"/"+file,t.EncodeToPNG());RenderTexture.active=null;c.targetTexture=null;Destroy(c.gameObject);Destroy(rt);Destroy(t);}
 [Serializable]class Result{public bool passed;public string[] checks;}
}
