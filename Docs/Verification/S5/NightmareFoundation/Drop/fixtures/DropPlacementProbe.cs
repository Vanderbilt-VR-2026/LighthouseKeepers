using System;using System.IO;using System.Reflection;using UnityEngine;using UnityEditor;using LighthouseKeepers.WatchDemo;using LighthouseKeepers.Core.Progression;
public static class DropPlacementProbe {
 public static object Run(string output){
  EditorUtility.audioMasterMute=true;Directory.CreateDirectory(output);var w=UnityEngine.Object.FindAnyObjectByType<WatchDemoController>();var n=w.GetComponent<NightmareScene>();var t=HorrorTuning.Current;
  var method=typeof(NightmareScene).GetMethod("TryDrop",BindingFlags.NonPublic|BindingFlags.Instance);var floor=typeof(NightmareScene).GetMethod("Floor",BindingFlags.NonPublic|BindingFlags.Instance);var nav=(HunterNavigation)typeof(NightmareScene).GetField("navigation",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(n);
  bool Floor(Vector3 eye,out Vector3 pt){object[] p={eye,Vector3.zero};bool ok=(bool)floor.Invoke(n,p);pt=(Vector3)p[1];return ok;}
  var a=new Vector3(-.406f,4.966f,1.837f);var b=new Vector3(-.417f,4.965f,1.841f);var forward=new Vector3(.888f,.412f,.201f);
  bool Try(out Vector3 p){object[] args={a,forward,1,b,Vector3.zero};bool ok=(bool)method.Invoke(n,args);p=(Vector3)args[4];return ok;}
  var log="";int errors=0;void Check(bool value,string text){log+=(value?"PASS ":"FAIL ")+text+"\n";if(!value)errors++;}
  float min=t.nightmareDropMinDistance,clear=t.nightmareExitClearance;
  try {
   t.nightmareDropMinDistance=t.nightmareDropDistance;Check(!Try(out _),"The former outer ring has no safe drop for the measured crew positions");
   t.nightmareDropMinDistance=min;bool found=Try(out var drop);Check(found,"The inner ring finds a safe drop for both keepers");
   Vector3 af,bf;bool floorA=Floor(a,out af),floorB=Floor(b,out bf);bool floors=floorA&&floorB;var routeA=Array.Empty<Vector3>();var routeB=Array.Empty<Vector3>();bool routes=floors&&nav.Route(af,t.nightmareOutside,out routeA)&&nav.Route(bf,t.nightmareOutside,out routeB);Check(routes,"Both escape routes remain navigable");
   Check(found&&Vector3.Dot(Vector3.ProjectOnPlane(drop-a,Vector3.up).normalized,-forward)>.7f,"Drop is behind the first keeper");
   Check(found&&routes&&ScarePathSafety.ClearOfRoute(drop,routeA,clear,t.navClimb)&&ScarePathSafety.ClearOfRoute(drop,routeB,clear,t.navClimb),"Drop stays outside both escape routes at the configured clearance");
   Check(found&&Vector3.Distance(drop+Vector3.up,a)>=1.8f&&Vector3.Distance(drop+Vector3.up,b)>=1.8f,"Drop clears both viewers");
   t.nightmareExitClearance=50;Check(!Try(out _),"Impossible route clearance suppresses the drop instead of forcing it");
   log+="Chosen drop: "+drop.ToString("F2")+"\n";
  }finally{t.nightmareDropMinDistance=min;t.nightmareExitClearance=clear;}
  File.WriteAllText(output+"/checks.txt",log);File.WriteAllText(output+"/result.json","{\"passed\":"+(errors==0?"true":"false")+",\"failures\":"+errors+"}");return new {passed=errors==0,failures=errors};
 }
}
