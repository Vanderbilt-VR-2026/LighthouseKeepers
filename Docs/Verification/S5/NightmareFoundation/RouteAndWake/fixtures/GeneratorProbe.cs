using System;using System.IO;using System.Collections;using System.Linq;
using UnityEngine;using UnityEditor;using Unity.XR.CoreUtils;using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Core.Progression;using LighthouseKeepers.Gameplay.Interaction;
public class GeneratorStartProbe:MonoBehaviour{

 string folder;WatchDemoController w;NightmareScene scene;XROrigin rig;Transform head;ContinuousMoveProvider move;KeeperRunInput run;XRInputValueReader<Vector2> stick;HandRig hand;int failures;float start;bool wantRun,recovering;float minimumStamina=1,maximumSpeed;
 public static object Begin(string folder){EditorUtility.audioMasterMute=true;Directory.CreateDirectory(folder);var p=new GameObject("Nightmare physical escape probe").AddComponent<GeneratorStartProbe>();p.folder=folder;p.StartCoroutine(p.Go());return true;}
 void Update(){if(run){run.SampleButtons(wantRun,false,Time.unscaledDeltaTime);minimumStamina=Mathf.Min(minimumStamina,run.Remaining);if(move)maximumSpeed=Mathf.Max(maximumSpeed,move.moveSpeed);}}
 void Log(string text){File.AppendAllText(folder+"/checks.txt",text+"\n");}void Check(bool ok,string text){Log((ok?"PASS ":"FAIL ")+text);if(!ok)failures++;}
 Vector3[] Points(string id){
  if(id=="RT-F1_F2")return new[]{new Vector3(-1.589758f,0.000000f,2.544144f),new Vector3(-1.928363f,0.000000f,2.298133f),new Vector3(-2.037470f,0.091667f,2.201980f),new Vector3(-2.141789f,0.183333f,2.100652f),new Vector3(-2.241075f,0.275000f,1.994388f),new Vector3(-2.335095f,0.366667f,1.883436f),new Vector3(-2.423627f,0.458333f,1.768059f),new Vector3(-2.506463f,0.550000f,1.648527f),new Vector3(-2.583410f,0.641667f,1.525121f),new Vector3(-2.654285f,0.733333f,1.398131f),new Vector3(-2.718923f,0.825000f,1.267855f),new Vector3(-2.777172f,0.916667f,1.134600f),new Vector3(-2.828894f,1.008333f,0.998678f),new Vector3(-2.873969f,1.100000f,0.860410f),new Vector3(-2.912289f,1.191667f,0.720119f),new Vector3(-2.943766f,1.283333f,0.578137f),new Vector3(-2.968325f,1.375000f,0.434796f),new Vector3(-2.985908f,1.466667f,0.290433f),new Vector3(-2.996475f,1.558333f,0.145387f),new Vector3(-3.000000f,1.650000f,-0.000000f),new Vector3(-2.996475f,1.741667f,-0.145387f),new Vector3(-2.985908f,1.833333f,-0.290433f),new Vector3(-2.968325f,1.925000f,-0.434796f),new Vector3(-2.943766f,2.016667f,-0.578137f),new Vector3(-2.912289f,2.108333f,-0.720119f),new Vector3(-2.873969f,2.200000f,-0.860410f),new Vector3(-2.828894f,2.291667f,-0.998678f),new Vector3(-2.777172f,2.383333f,-1.134600f),new Vector3(-2.718923f,2.475000f,-1.267855f),new Vector3(-2.654285f,2.566667f,-1.398131f),new Vector3(-2.583410f,2.658333f,-1.525121f),new Vector3(-2.506463f,2.750000f,-1.648527f),new Vector3(-2.423627f,2.841667f,-1.768059f),new Vector3(-2.335095f,2.933333f,-1.883436f),new Vector3(-2.241075f,3.025000f,-1.994388f),new Vector3(-2.141789f,3.116667f,-2.100652f),new Vector3(-2.037470f,3.208333f,-2.201980f),new Vector3(-1.928363f,3.300000f,-2.298133f),new Vector3(-1.589758f,3.300000f,-2.544144f)};
  if(id=="RT-F1-ENTRY")return new[]{new Vector3(0.000000f,0.000000f,-3.850000f),new Vector3(0.000000f,0.000000f,-1.700000f),new Vector3(-1.200000f,0.000000f,-0.700000f),new Vector3(-1.200000f,0.000000f,1.000000f),new Vector3(-0.300000f,0.000000f,1.700000f),new Vector3(-1.026060f,0.000000f,2.819078f),new Vector3(-1.589758f,0.000000f,2.544144f)};
  throw new ArgumentException(id);
 }
 IEnumerator Walk(Vector3 goal,string label,float tolerance=.16f){
  float until=Time.time+7;while(Time.time<until){var d=Vector3.ProjectOnPlane(goal-rig.transform.position,Vector3.up);if(d.magnitude<tolerance)break;
   var f=Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized;var r=Vector3.Cross(Vector3.up,f);d.Normalize();stick.manualValue=new Vector2(Vector3.Dot(d,r),Vector3.Dot(d,f));
   if(run.Remaining<=.01f)recovering=true;else if(run.Remaining>=.95f)recovering=false;wantRun=!recovering;yield return null;
  }
  float distance=Vector3.Distance(Vector3.ProjectOnPlane(goal-rig.transform.position,Vector3.up),Vector3.zero);
  if(distance>=tolerance){Check(false,label+" stuck at "+rig.transform.position+" target "+goal+" remaining "+run.Remaining.ToString("F2"));}
 }
 IEnumerator Pull(int index){
  stick.manualValue=Vector2.zero;wantRun=false;
  var target=scene.DoorTarget(index*2);var d=hand.Direct;hand.transform.parent.gameObject.SetActive(true);hand.enabled=false;d.enabled=true;
  d.selectInput=new XRInputButtonReader("test physical grip",inputSourceMode:XRInputButtonReader.InputSourceMode.ManualValue);d.selectInput.QueueManualState(false,0);
  hand.transform.parent.position=target.transform.position;Physics.SyncTransforms();yield return new WaitForSeconds(.15f);
  float reach=Vector3.Distance(head.position,target.transform.position);d.selectInput.QueueManualState(true,1);yield return new WaitForSeconds(.2f);
  Check(d.hasSelection,"Door "+index+" selected by actual XR direct grip; hand reach from head "+reach.ToString("F2")+" m");
  for(int i=0;i<12&&w.State.Scare.Door(index)<.75f;i++){hand.transform.parent.position+=Vector3.back*.08f;Physics.SyncTransforms();yield return new WaitForSeconds(.1f);}
  Check(w.State.Scare.Door(index)>=.72f,"Door "+index+" opens through held grip/pull ("+w.State.Scare.Door(index).ToString("F2")+")");
  d.selectInput.QueueManualState(false,0);yield return new WaitForSeconds(.15f);
 }
 IEnumerator Go(){
  w=FindAnyObjectByType<WatchDemoController>();scene=w.GetComponent<NightmareScene>();rig=FindAnyObjectByType<XROrigin>();head=rig.Camera.transform;
  foreach(var b in rig.GetComponentsInChildren<Behaviour>(true))if(b.GetType().Name.Contains("TrackedPose")||b.GetType().Name.Contains("InputModality")||b.GetType().Name=="DesktopPreview")b.enabled=false;
  var cc=rig.GetComponent<CharacterController>();cc.enabled=false;rig.transform.position+=new Vector3(-.4f-head.position.x,3.3f-rig.transform.position.y,0-head.position.z);cc.enabled=true;head.LookAt(new Vector3(1.7f,4.4f,1.3f));Physics.SyncTransforms();
  move=rig.GetComponentInChildren<ContinuousMoveProvider>();stick=new XRInputValueReader<Vector2>("Physical escape stick",XRInputValueReader.InputSourceMode.ManualValue);move.leftHandMoveInput=stick;
  hand=rig.GetComponentsInChildren<HandRig>(true).First(h=>h.Node==XRNode.RightHand);run=w.GetComponent<KeeperRunInput>();run.enabled=false;
  HorrorTuning.Current.nightmareEnabled=true;Check(scene.Begin(true),"Nightmare began for "+w.CurrentDuty);scene.enabled=false;
  var c=w.State.Scare;for(int i=0;i<200&&c.Beat!=ScareBeat.Turn;i++)c.Tick(.1f,true,true,0,0);c.Place(HorrorTuning.Current.nightmareCorner,0);scene.enabled=true;
  while(c.Beat!=ScareBeat.Escape&&!c.Failed)yield return null;start=Time.time;Log("Starting rig "+rig.transform.position+" child "+c.Body+" speed "+move.moveSpeed);
  yield return Walk(new Vector3(-.78f,3.3f,-1.75f),"Generator to stair passage",.2f);if(failures==0)yield return Walk(new Vector3(-1.22f,3.3f,-2.74f),"F2 stair landing",.2f);
  var stairs=Points("RT-F1_F2");Array.Reverse(stairs);foreach(var p in stairs){yield return Walk(p,"Downstairs",.20f);if(failures>0||c.Failed)break;}
  Check(!c.Failed&&rig.transform.position.y<.35f,"Reach F1 using real XRI locomotion and capsule; scene="+c.Beat+" pos="+rig.transform.position);
  if(failures==0){var entry=Points("RT-F1-ENTRY");Array.Reverse(entry);foreach(var p in entry){if(p.z< -3.4f)break;yield return Walk(p,"F1 route",.20f);if(failures>0||c.Failed)break;}}
  if(failures==0){yield return Walk(new Vector3(0,0,-3.25f),"First door approach",.18f);yield return Pull(0);}
  if(failures==0){yield return Walk(new Vector3(0,0,-4.5f),"Through first door",.18f);yield return Walk(new Vector3(0,0,-6.5f),"House aisle",.18f);yield return Walk(new Vector3(0,0,-9.25f),"Exit door approach",.18f);yield return Pull(1);}
  if(failures==0){yield return Walk(new Vector3(0,0,-10.6f),"Through exit",.16f);yield return new WaitForSeconds(.3f);Check(c.Beat==ScareBeat.Return,"Stepping outside stops chase and starts return");}
  stick.manualValue=Vector2.zero;wantRun=false;Check(maximumSpeed>2.8f,"A-run input raises actual locomotion speed to "+maximumSpeed.ToString("F2"));Check(minimumStamina<.2f,"Sprint spends stamina to "+minimumStamina.ToString("F2"));Check(run.Remaining>minimumStamina+.2f,"Stamina refills after releasing run");Log("Elapsed="+(Time.time-start).ToString("F2")+" remaining="+run.Remaining.ToString("F2")+" beat="+c.Beat+" rig="+rig.transform.position+" child="+c.Body+" door0="+c.Door(0)+" door1="+c.Door(1));File.WriteAllText(folder+"/result.json", "{\"passed\":"+(failures==0?"true":"false")+",\"failures\":"+failures+"}");Destroy(gameObject);
 }
}
