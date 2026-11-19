using System;using System.IO;using System.Linq;using System.Text;using System.Reflection;using UnityEngine;using UnityEditor;using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Core.Progression;
public static class DropAuditReal {
 public static object Run(string output){
  EditorUtility.audioMasterMute=true;var w=UnityEngine.Object.FindAnyObjectByType<WatchDemoController>();var n=w.GetComponent<NightmareScene>();var t=HorrorTuning.Current;var nav=(HunterNavigation)typeof(NightmareScene).GetField("navigation",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(n);
  var floor=typeof(NightmareScene).GetMethod("Floor",BindingFlags.NonPublic|BindingFlags.Instance);bool Floor(Vector3 eye,out Vector3 point){object[] args={eye,Vector3.zero};var ok=(bool)floor.Invoke(n,args);point=(Vector3)args[1];return ok;}
  var a=new Vector3(-.406f,4.966f,1.837f);var b=new Vector3(-.417f,4.965f,1.841f);var child=t.nightmareCorner+Vector3.up*1.1f;var f=Vector3.ProjectOnPlane(new Vector3(.888f,.412f,.201f),Vector3.up).normalized;var back=-f;
  Floor(a,out var af);Floor(b,out var bf);var validA=nav.Route(af,t.nightmareOutside,out var ar);var validB=nav.Route(bf,t.nightmareOutside,out var br);var log=new StringBuilder();log.AppendLine($"a={a} aFloor={af} bFloor={bf} back={back} routeA={validA} routeB={validB}");log.AppendLine("routeA="+string.Join(" ",ar.Select(x=>x.ToString("F2"))));
  foreach(float distance in new[]{1.8f,2.2f,2.6f,3.2f,3.8f})foreach(float angle in new[]{-120f,-100f,-80f,-60f,-40f,-20f,0f,20f,40f,60f,80f,100f,120f}){
   var p=a+Quaternion.Euler(0,angle,0)*back*distance;if(!Floor(p,out var pt)||Mathf.Abs(pt.y-af.y)>t.navClimb)continue;
   var cs=Physics.OverlapCapsule(pt+Vector3.up*.25f,pt+Vector3.up*1.2f,t.radius*.9f,~0,QueryTriggerInteraction.Ignore);bool clear=cs.All(c=>c.attachedRigidbody||c.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>());bool routed=nav.Route(pt,af,out _);bool cl1=ScarePathSafety.ClearOfRoute(pt,ar,1f,t.navClimb)&&ScarePathSafety.ClearOfRoute(pt,br,1f,t.navClimb);bool cl15=ScarePathSafety.ClearOfRoute(pt,ar,1.5f,t.navClimb)&&ScarePathSafety.ClearOfRoute(pt,br,1.5f,t.navClimb);bool cl2=ScarePathSafety.ClearOfRoute(pt,ar,2f,t.navClimb)&&ScarePathSafety.ClearOfRoute(pt,br,2f,t.navClimb);
   log.AppendLine($"distance={distance:F1} angle={angle:F0} floor={pt.ToString("F2")} clear={clear} route={routed} c1={cl1} c1.5={cl15} c2={cl2} blockers={string.Join(",",cs.Where(c=>!c.attachedRigidbody&&!c.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>()).Select(c=>c.name).Take(3))}");
  }
  File.WriteAllText(output,log.ToString());return new {done=true};
 }
}
