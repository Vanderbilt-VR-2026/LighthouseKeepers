using System;using System.IO;using System.Collections;using System.Linq;using UnityEngine;using UnityEditor;using Unity.XR.CoreUtils;using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Core.Progression;
public class NightmareProbe:MonoBehaviour{
 string folder;WatchDemoController w;NightmareScene scene;XROrigin rig;Transform head;Camera camera;int failures;Vector3 previous;double elapsed;
 public static object Begin(string folder){EditorUtility.audioMasterMute=true;var t=new GameObject("Nightmare editor acceptance").AddComponent<NightmareProbe>();t.folder=folder;Directory.CreateDirectory(folder);t.StartCoroutine(t.Run());return true;}
 void Check(bool pass,string text){File.AppendAllText(folder+"/checks.txt",(pass?"PASS ":"FAIL ")+text+"\n");if(!pass)failures++;}
 void Pose(Vector3 feet,Vector3 target){var cc=rig.GetComponent<CharacterController>();if(cc)cc.enabled=false;rig.transform.position+=new Vector3(feet.x-head.position.x,feet.y-rig.transform.position.y,feet.z-head.position.z);head.rotation=Quaternion.LookRotation(target-head.position);Physics.SyncTransforms();}
 void Capture(string name){camera.transform.SetPositionAndRotation(head.position,head.rotation);var rt=new RenderTexture(1100,850,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var t=new Texture2D(1100,850,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1100,850),0,0);t.Apply();File.WriteAllBytes(folder+"/"+name+".png",t.EncodeToPNG());RenderTexture.active=null;camera.targetTexture=null;Destroy(t);Destroy(rt);}
 IEnumerator Run(){
  w=FindAnyObjectByType<WatchDemoController>();scene=w.GetComponent<NightmareScene>();rig=FindAnyObjectByType<XROrigin>();head=rig.Camera.transform;
  foreach(var b in rig.GetComponentsInChildren<Behaviour>(true))if(b.GetType().Name.Contains("TrackedPose")||b.GetType().Name.Contains("InputModality"))b.enabled=false;
  HorrorTuning.Current.nightmareEnabled=true;elapsed=w.State.Elapsed;var start=HorrorTuning.Current.nightmareStart;
  using(var n=new HunterNavigation(HorrorTuning.Current,Resources.Load<UnityEngine.AI.NavMeshData>("HunterNavigation"))){bool ok=n.Route(HorrorTuning.Current.nightmareCorner,HorrorTuning.Current.nightmareOutside,out var route);Check(ok,"Escape has complete NavMesh route");if(ok){for(int i=1;i<route.Length;i++)Check(n.SegmentClear(route[i-1],route[i]),"Clear escape segment "+i);}}
  camera=new GameObject("Nightmare review camera").AddComponent<Camera>();camera.enabled=false;camera.fieldOfView=65;camera.nearClipPlane=.04f;
  Pose(start+Vector3.right*1.5f,start+Vector3.up*1.1f);Check(scene.Begin(true),"Host starts Day 2 nightmare");yield return null;
  Check(w.State.Capture(w.WaterHeight).Valid(),"Wake snapshot validates");yield return new WaitForSeconds(.8f);Capture("wake");
  float limit=Time.time+90,lastCheck=Time.time;previous=w.State.Scare.Body;
  while(Time.time<limit&&w.State.Scare.Beat<ScareBeat.Turn){
   var c=w.State.Scare;var at=c.Body;Pose(at+Vector3.right*.8f,at+Vector3.up*1.1f);
   if(Time.time-lastCheck>1){File.AppendAllText(folder+"/route.txt",c.Beat+" "+c.Body+"\n");lastCheck=Time.time;}
   yield return null;
  }
  Check(w.State.Scare.Beat==ScareBeat.Turn,"Child reaches F2 through the authored stairs");
  Check(Math.Abs(w.State.Elapsed-elapsed)<.05,"Watch and flood time paused through follow");
  Check(w.State.Scare.Clock>20,"Separate scene clock advances");yield return new WaitForSeconds(.3f);Capture("turn");
  Pose(new Vector3(-.4f,3.3f,-.6f),w.State.Scare.Body+Vector3.up);while(w.State.Scare.Beat==ScareBeat.Turn&&w.State.Scare.Age<HorrorTuning.Current.nightmareTurnSeconds-.2f)yield return null;var child=GameObject.Find("Nightmare child").transform;head.LookAt(child.TransformPoint(Vector3.up*.65f));Capture("ceiling");
  Pose(new Vector3(-1,3.3f,0),w.State.Scare.Body+Vector3.up);
  limit=Time.time+12;while(Time.time<limit&&w.State.Scare.Beat!=ScareBeat.Escape)yield return null;
  Check(w.State.Scare.Beat==ScareBeat.Escape,"Warning and near-silence precede chase");Capture("chase");
  Check(w.State.Scare.PullDoor(1)&&w.State.Scare.Door(0)<=.18f,"Door command is host bounded, cannot snap fully open");
  for(int i=0;i<8;i++)w.State.Scare.PullDoor(1);
  yield return null;Check(w.State.Scare.Door(0)==1,"Pulling opens first escape door");
  Pose(new Vector3(0,0,-10.5f),new Vector3(0,1.5f,-9));yield return new WaitForSeconds(.2f);File.AppendAllText(folder+"/checks.txt","Escape state "+w.State.Scare.Beat+" caught="+w.State.Scare.Caught+" head="+head.position+"\n");Check(w.State.Scare.Beat==ScareBeat.Return,"Solo outside escape ends pursuit; return inside requested");Capture("outside-relief");Pose(new Vector3(0,0,-8),new Vector3(0,1.5f,-7));yield return new WaitForSeconds(.3f);Check(w.State.Scare.Beat==ScareBeat.Relief,"Return inside starts relief");
  Check(w.State.Capture(w.WaterHeight).Valid(),"Relief snapshot validates");
  File.WriteAllText(folder+"/result.json","{\"passed\":"+(failures==0?"true":"false")+",\"failures\":"+failures+"}");Destroy(camera.gameObject);Destroy(gameObject);
 }
}
