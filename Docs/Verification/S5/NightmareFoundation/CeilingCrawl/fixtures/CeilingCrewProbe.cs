using System.Reflection;using UnityEngine;using UnityEditor;using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Core.Progression;
public static class CeilingCrewProbe {
 static WatchDemoController W=>Object.FindAnyObjectByType<WatchDemoController>();
 static ScareKit Kit=> (ScareKit)typeof(NightmareScene).GetField("kit",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(W.GetComponent<NightmareScene>());
 public static object Prepare(){EditorUtility.audioMasterMute=true;var n=W.GetComponent<NightmareScene>();n.enabled=false;var c=W.State.Scare;for(int i=0;i<200&&c.Beat!=ScareBeat.Turn;i++)c.Tick(.1f,true,true,0,0);c.Place(HorrorTuning.Current.nightmareCorner,0);while(c.Age<3.5f)c.Tick(.1f,true,true,0,0);Kit.Beat(c.Beat,c.Age);Kit.Present(c,Camera.main.transform.position);return Facts();}
 public static object Facts(){EditorUtility.audioMasterMute=true;var k=Kit;var body=(Transform)typeof(ScareKit).GetField("body",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(k);var c=W.State.Scare;var p=body.position;var q=body.rotation;var end=(Vector3)typeof(ScareKit).GetField("ceilingEnd",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(k);return new{beat=(int)c.Beat,age=c.Age,position=new[]{p.x,p.y,p.z},rotation=new[]{q.x,q.y,q.z,q.w},end=new[]{end.x,end.y,end.z}};}
}
