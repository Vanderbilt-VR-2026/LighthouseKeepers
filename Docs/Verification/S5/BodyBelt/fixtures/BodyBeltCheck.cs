using System;using System.Collections;using System.Collections.Generic;using System.IO;using System.Linq;
using UnityEngine;using UnityEditor;using UnityEngine.XR;using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Gameplay.Interaction;using LighthouseKeepers.Core;using LighthouseKeepers.Interaction;using LighthouseKeepers.Net;
public class BodyBeltCheck:MonoBehaviour {
 string folder;HandRig hand;List<string> checks=new();bool failed;KeeperToolBelt belt;
 public static object Run(string p){EditorUtility.audioMasterMute=true;var c=new GameObject("Body belt acceptance").AddComponent<BodyBeltCheck>();c.folder=p;return "started";}
 void Check(bool x,string s){checks.Add((x?"PASS ":"FAIL ")+s);failed|=!x;}
 IEnumerator Start(){
  Directory.CreateDirectory(folder);var w=FindAnyObjectByType<WatchDemoController>();w.enabled=false;
  var buddy=w.GetComponent<CrewCompanion>();if(buddy)buddy.SetCommand(CompanionCommand.Wait);
  var quality=w.GetComponent<QuestRenderQuality>();if(quality)quality.enabled=false;
  belt=w.GetComponent<KeeperToolBelt>();var rig=FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
  foreach(var b in rig.GetComponentsInChildren<Behaviour>(true))if(b.GetType().Name=="DesktopPreview")b.enabled=false;
  foreach(var m in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager>(true))m.enabled=false;
  foreach(var h in rig.GetComponentsInChildren<HandRig>(true))h.transform.parent.gameObject.SetActive(false);
  hand=rig.GetComponentsInChildren<HandRig>(true).First(h=>h.Node==XRNode.RightHand);hand.transform.parent.gameObject.SetActive(true);
  foreach(var t in hand.transform.parent.GetComponents<UnityEngine.InputSystem.XR.TrackedPoseDriver>())t.enabled=false;hand.enabled=false;
  var d=hand.Direct;d.enabled=true;d.selectInput=new XRInputButtonReader("test grip",inputSourceMode:XRInputButtonReader.InputSourceMode.ManualValue);d.selectInput.QueueManualState(false,0);
  var presenter=LocalKeeperBody.Current;Check(presenter!=null,"local body presenter exists");presenter.enabled=false;presenter.Bind(rig);
  for(int i=0;i<100;i++){presenter.Present(true,1f/72);yield return null;}
  Check(presenter.Body&&presenter.Body.gameObject.activeInHierarchy,"tracked local body active");
  var hip=presenter.Body.Joint("Hips");Check(ToolBeltPose.TrackedHips!=null&&ToolBeltPose.TrackedHips()==hip,"body presentation exposes the active hip");
  foreach(var slot in new[]{ToolBeltSlot.Radio,ToolBeltSlot.Wrench})Check(Vector3.Distance(belt.Position(slot),ToolBeltPose.Position(slot,hip))<.003f,slot+" clip is anchored to animated hip");
  Check(!FindObjectsByType<Transform>(FindObjectsSortMode.None).Any(t=>t.name=="Flashlight waist clip"),"no flashlight holder");
  var wrench=FindAnyObjectByType<WatchPipeRepair>().wrench;
  var radio=FindObjectsByType<HandheldCrewRadio>(FindObjectsSortMode.None).OrderBy(r=>r.name).First().Grab;
  foreach(var g in new[]{wrench,radio}){
   var tool=g.GetComponent<INetworkPropCarry>();var slot=g.GetComponent<INetworkBeltSlot>().BeltSlot;
   g.transform.SetPositionAndRotation(belt.Head.position+Vector3.forward*.7f-Vector3.up*.3f,Quaternion.identity);g.GetComponent<Rigidbody>().isKinematic=true;
   hand.transform.parent.rotation=Quaternion.identity;hand.transform.parent.position=g.transform.position;Physics.SyncTransforms();yield return new WaitForSeconds(.4f);
   d.selectInput.QueueManualState(true,1);yield return new WaitForSeconds(.5f);Check(g.isSelected,g.name+" direct grip selects");
   for(int n=0;n<8;n++){hand.transform.parent.position+=belt.Position(slot)-g.transform.position;yield return null;}
   d.selectInput.QueueManualState(false,0);yield return new WaitForSeconds(.4f);Check(tool.Carried,g.name+" clipped on release");
   Check(g.GetComponent<Rigidbody>().isKinematic,g.name+" clipped body stays kinematic");
   Check(Vector3.Distance(g.transform.position,belt.Position(slot))<.005f,g.name+" follows body slot");
   hand.transform.parent.position=belt.Head.position+Vector3.up;yield return new WaitForSeconds(.2f);
  }
  Check(Vector3.Distance(radio.transform.position,wrench.transform.position)>.30f,"wrench and radio remain separated");
  var previous=rig.Camera.transform.localRotation;rig.Camera.transform.localRotation=Quaternion.Euler(20,105,0);
  for(int i=0;i<50;i++){presenter.Present(true,1f/72);yield return null;}
  Check(Vector3.Distance(wrench.transform.position,belt.Position(ToolBeltSlot.Wrench))<.01f,"wrench follows turning torso");
  Check(Vector3.Distance(radio.transform.position,belt.Position(ToolBeltSlot.Radio))<.01f,"radio follows turning torso");
  rig.Camera.transform.localRotation=previous;for(int i=0;i<50;i++){presenter.Present(true,1f/72);yield return null;}
  var centre=presenter.Body.Joint("Hips").position;var light=new GameObject("Belt inspection light - test only").AddComponent<Light>();light.type=LightType.Point;light.range=3;light.intensity=20f;light.transform.position=centre+Vector3.forward*.7f+Vector3.up*.3f;
  Capture(centre+Vector3.forward*.8f+Vector3.up*.4f,centre,"belt-front.png");Capture(centre+Vector3.left*.85f+Vector3.up*.4f,centre,"belt-left.png");Destroy(light.gameObject);
  foreach(var g in new[]{wrench,radio}){
   hand.transform.parent.position=g.transform.position;Physics.SyncTransforms();yield return new WaitForSeconds(.4f);d.selectInput.QueueManualState(true,1);yield return new WaitForSeconds(.5f);
   Check(g.isSelected&&!g.GetComponent<INetworkPropCarry>().Carried,g.name+" regrip detaches belt");
   hand.transform.parent.position=belt.Head.position+Vector3.forward*.8f;yield return new WaitForSeconds(.3f);d.selectInput.QueueManualState(false,0);yield return new WaitForSeconds(.4f);
   Check(!g.GetComponent<INetworkPropCarry>().Carried,g.name+" ordinary release does not snap");hand.transform.parent.position=belt.Head.position+Vector3.up;yield return new WaitForSeconds(.2f);
  }
  File.WriteAllText(folder+"/result.json",JsonUtility.ToJson(new Result{passed=!failed,checks=checks.ToArray()},true));Destroy(gameObject);
 }
 void Capture(Vector3 p,Vector3 target,string file){var c=new GameObject("Evidence camera").AddComponent<Camera>();c.transform.position=p;c.transform.LookAt(target);c.fieldOfView=55;c.nearClipPlane=.02f;var rt=new RenderTexture(1200,900,24);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(1200,900,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1200,900),0,0);t.Apply();File.WriteAllBytes(folder+"/"+file,t.EncodeToPNG());RenderTexture.active=null;c.targetTexture=null;Destroy(c.gameObject);Destroy(rt);Destroy(t);}
 [Serializable]class Result{public bool passed;public string[] checks;}
}
