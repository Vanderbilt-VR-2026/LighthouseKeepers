using System;using System.IO;using System.Collections;using UnityEngine;using UnityEditor;using Unity.XR.CoreUtils;using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Core.Progression;
public class TurnProbe:MonoBehaviour{
 string folder;WatchDemoController w;XROrigin rig;Transform head;Camera renderCamera;
 public static object Begin(string folder){EditorUtility.audioMasterMute=true;var t=new GameObject("Ceiling review fixture").AddComponent<TurnProbe>();t.folder=folder;Directory.CreateDirectory(folder);t.StartCoroutine(t.Run());return true;}
 void Shot(string name){renderCamera.transform.SetPositionAndRotation(head.position,head.rotation);var rt=new RenderTexture(1100,850,24);renderCamera.targetTexture=rt;renderCamera.Render();RenderTexture.active=rt;var image=new Texture2D(1100,850,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1100,850),0,0);image.Apply();File.WriteAllBytes(folder+"/"+name+".png",image.EncodeToPNG());RenderTexture.active=null;renderCamera.targetTexture=null;Destroy(image);Destroy(rt);}
 IEnumerator Run(){
 w=FindAnyObjectByType<WatchDemoController>();var scene=w.GetComponent<NightmareScene>();rig=FindAnyObjectByType<XROrigin>();head=rig.Camera.transform;
 foreach(var b in rig.GetComponentsInChildren<Behaviour>(true))if(b.GetType().Name.Contains("TrackedPose")||b.GetType().Name.Contains("InputModality"))b.enabled=false;
 var cc=rig.GetComponent<CharacterController>();if(cc)cc.enabled=false;
 HorrorTuning.Current.nightmareEnabled=true;scene.Begin(true);scene.enabled=false;
 var c=w.State.Scare;for(int i=0;i<100&&c.Beat!=ScareBeat.Turn;i++)c.Tick(.1f,true,true,0,0);c.Place(HorrorTuning.Current.nightmareCorner,0);
 rig.MoveCameraToWorldLocation(new Vector3(-.4f,4.95f,-.6f));head.LookAt(c.Body+Vector3.up);Physics.SyncTransforms();
 renderCamera=new GameObject("Review camera").AddComponent<Camera>();renderCamera.enabled=false;renderCamera.fieldOfView=65;renderCamera.nearClipPlane=.04f;
 scene.enabled=true;yield return null;
 while(c.Beat==ScareBeat.Turn&&c.Age<3.8f){head.LookAt(GameObject.Find("Nightmare child").transform.TransformPoint(Vector3.up*.65f));yield return null;}
 var child=GameObject.Find("Nightmare child").transform;Shot("child-on-ceiling");
 var data="beat="+c.Beat+" age="+c.Age+" position="+child.position+" rotation="+child.eulerAngles+" head="+head.position+" distance="+Vector3.Distance(head.position,child.TransformPoint(Vector3.up*1.1f));
 int count=0;foreach(var mf in FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(mf.name.StartsWith("Wet child handprint"))count++;File.WriteAllText(folder+"/pose.txt",data+"\nProjected rail prints="+count);
 scene.enabled=false;Destroy(renderCamera.gameObject);Destroy(gameObject);
 }
}
