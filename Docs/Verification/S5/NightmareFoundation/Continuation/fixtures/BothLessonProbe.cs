using System;using System.Linq;using System.IO;using System.Collections;using UnityEngine;using UnityEditor;using Unity.XR.CoreUtils;using UnityEngine.XR;using UnityEngine.XR.Interaction.Toolkit.Interactables;using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Net;using LighthouseKeepers.Gameplay.Interaction;
public sealed class BothLessonProbe:MonoBehaviour {
 string folder;WatchDemoController watch;CrewCompanion bot;XROrigin rig;HandRig hand;XRSimpleInteractable target;bool gripping;
 public static object Run(string output){EditorUtility.audioMasterMute=true;var p=new GameObject("Lesson probe").AddComponent<BothLessonProbe>();p.folder=output;return "started";}
 IEnumerator Start(){
  Directory.CreateDirectory(folder);watch=FindAnyObjectByType<WatchDemoController>();bot=watch.GetComponent<CrewCompanion>();rig=FindAnyObjectByType<XROrigin>();yield return new WaitForSecondsRealtime(2);
  rig.MoveCameraToWorldLocation(new Vector3(-1.8f,4.95f,-1.4f));watch.State.EngageWatch();watch.State.Tick(61);
  bool unknown=false;float start=Time.realtimeSinceStartup;while(watch.State.Recovery.Lesson<4&&Time.realtimeSinceStartup-start<65){if(watch.State.Recovery.Lesson==3){var lesson=watch.GetComponent<PossessionLesson>();var radio=FindAnyObjectByType<HarborRadio>();unknown=lesson.Speaker=="Unknown"&&radio.LastSpeaker=="Unknown";}File.WriteAllText(folder+"/current.txt","stage="+watch.State.Recovery.Lesson+" bot="+bot.Position+" "+bot.Status);yield return new WaitForSecondsRealtime(.25f);}
  var r=watch.State.Recovery;if(r.Lesson!=4){End(false,"Never possessed: "+r.Lesson+" bot "+bot.Position);yield break;}
  if(!unknown){End(false,"Unknown label not retained in recall");yield break;}
  if(watch.GetComponent<NightmareScene>().Begin(true)){End(false,"Nightmare started during active possession lesson");yield break;}
  if(!r.Downed((int)bot.Duty)||r.Downed((int)watch.CurrentDuty)){End(false,"Wrong victim");yield break;}
  double elapsed=watch.State.Elapsed;float water=watch.WaterHeight;yield return new WaitForSecondsRealtime(5);
  if(watch.State.Elapsed!=elapsed||Mathf.Abs(watch.WaterHeight-water)>.002f){End(false,"Danger clock continued");yield break;}
  Capture("possessed-room",new Vector3(.1f,5.1f,2.5f),bot.Position+Vector3.up*1.4f,65);
  var full=GameObject.Find("Owen — scripted keeper").GetComponentInChildren<CrewHumanoidAvatar>();
  var face=full.Joint("Head").position;Capture("possessed-face",face+GameObject.Find("Owen — scripted keeper").transform.forward*.7f+Vector3.up*.1f,face+Vector3.up*.12f,48);
  int gates=FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Count(c=>c.name=="F2 sealed stair gate"&&c.gameObject.activeInHierarchy);if(gates!=2){End(false,"Expected two gates: "+gates);yield break;}
  hand=rig.GetComponentsInChildren<HandRig>(true).Single(x=>x.Node==XRNode.RightHand);foreach(var m in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager>(true))m.enabled=false;hand.transform.parent.gameObject.SetActive(true);foreach(var t in hand.transform.parent.GetComponents<UnityEngine.InputSystem.XR.TrackedPoseDriver>())t.enabled=false;
  target=(XRSimpleInteractable)watch.GetComponent<CrewRecovery>().Target((int)bot.Duty);hand.transform.parent.position=target.transform.position;yield return null;Grip();yield return new WaitForSecondsRealtime(1.2f);Release();yield return new WaitForSecondsRealtime(.5f);float p=r.Slot((int)bot.Duty).progress;yield return new WaitForSecondsRealtime(1);if(p<.1f||p>.8f||Mathf.Abs(r.Slot((int)bot.Duty).progress-p)>.01f){End(false,"Partial grip/release invalid "+p);yield break;}
  Capture("partial-rescue",new Vector3(.1f,5.1f,2.5f),bot.Position+Vector3.up*1.4f,65);Grip();yield return new WaitForSecondsRealtime(4);Release();yield return new WaitForSecondsRealtime(7);
  Capture("restored",new Vector3(.1f,5.1f,2.5f),bot.Position+Vector3.up*1.4f,65);
  bool open=!FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Any(c=>c.name=="F2 sealed stair gate"&&c.gameObject.activeInHierarchy);
  End(r.Lesson==6&&!r.AnyDown&&open&&!watch.State.Failed,"Nightmare enabled without suppressing old lesson; overlapping nightmare rejected; bot possessed; human direct grip/release/regrip; clock held; gates="+gates+" open="+open+" stage="+r.Lesson+" bot "+bot.Position);
 }
 void Grip(){gripping=true;var d=hand.Direct;d.selectInput=new XRInputButtonReader("rescue",inputSourceMode:XRInputButtonReader.InputSourceMode.ManualValue);d.selectInput.manualPerformed=true;d.selectInput.manualValue=1;d.StartManualInteraction((IXRSelectInteractable)target);}
 void Release(){gripping=false;if(hand&&hand.Direct.hasSelection)hand.Direct.EndManualInteraction();}
 void LateUpdate(){if(gripping&&target&&hand)hand.transform.parent.position=target.transform.position;}
 void End(bool passed,string detail){Release();File.WriteAllText(folder+"/result.json",JsonUtility.ToJson(new Result{passed=passed,detail=detail},true));Destroy(gameObject);}
 [Serializable]class Result{public bool passed;public string detail;}
 void Capture(string name,Vector3 from,Vector3 at,float fov){var c=new GameObject("Lesson evidence camera").AddComponent<Camera>();c.CopyFrom(Camera.main);c.enabled=false;c.nearClipPlane=.02f;c.fieldOfView=fov;c.transform.position=from;c.transform.LookAt(at);var old=RenderTexture.active;var rt=RenderTexture.GetTemporary(1400,1100,24);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(1400,1100,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1400,1100),0,0);t.Apply();File.WriteAllBytes(folder+"/"+name+".png",t.EncodeToPNG());RenderTexture.active=old;c.targetTexture=null;RenderTexture.ReleaseTemporary(rt);Destroy(t);Destroy(c.gameObject);}
}
