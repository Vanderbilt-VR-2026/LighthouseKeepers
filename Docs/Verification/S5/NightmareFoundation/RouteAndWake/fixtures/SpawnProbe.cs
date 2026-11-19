using System;using System.IO;using UnityEngine;using UnityEditor;using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Core.Progression;
public static class SpawnProbe{
 public static object Run(string output){EditorUtility.audioMasterMute=true;var t=HorrorTuning.Current;var w=UnityEngine.Object.FindAnyObjectByType<WatchDemoController>();var scene=w.State.Scare;var log=new System.Text.StringBuilder();int failures=0;
 void Check(bool ok,string msg){log.AppendLine((ok?"PASS ":"FAIL ")+msg);if(!ok)failures++;}
 var one=t.nightmareWakePosition+Vector3.forward*.3f;var two=t.nightmareWakePosition-Vector3.forward*.3f;Check(Vector3.Distance(one,two)>=.5f,"Wake positions leave room for both player capsules");
 using(var nav=new HunterNavigation(t,Resources.Load<UnityEngine.AI.NavMeshData>("HunterNavigation"))){foreach(var p in new[]{one,two}){Check(nav.SampleFloor(p,out var floor)&&nav.Route(p,t.nightmareCorner,out _),"Wake point has an authored route: "+p);var cols=Physics.OverlapCapsule(p+Vector3.up*.3f,p+Vector3.up*1.5f,.22f,~0,QueryTriggerInteraction.Ignore);bool clear=true;foreach(var c in cols)if(!c.attachedRigidbody&&!c.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>())clear=false;Check(clear,"Wake capsule is clear: "+p);
 foreach(float height in new[]{1.25f,1.65f,1.85f}){var eye=p+Vector3.up*height;var d=scene.Body+Vector3.up*1.1f-eye;var hits=Physics.RaycastAll(eye,d.normalized,d.magnitude,~0,QueryTriggerInteraction.Ignore);bool visible=true;foreach(var h in hits)if(!h.collider.attachedRigidbody&&!h.collider.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>())visible=false;Check(visible,"Child sightline from "+p+" eye height "+height);}}}
 File.WriteAllText(output,log.ToString());return new{passed=failures==0,failures};}
}
