using System;using System.Reflection;using NUnit.Framework;using UnityEngine;
namespace LighthouseKeepers.Net.Tests {
 public sealed class ObjectiveRecallTests {
  static Type T(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
  [Test] public void HeightStaysBelowEyeLineAndPersistsWithoutChangingHead() {
   var t=T("ObjectiveHudSettings");var height=t.GetProperty("Height");float old=(float)height.GetValue(null);
   try {foreach(float input in new[]{-30f,-13.5f,0f}) {height.SetValue(null,input);float actual=(float)height.GetValue(null);Assert.AreEqual(Mathf.Clamp(input,-20,-5),actual);Assert.AreEqual(actual,PlayerPrefs.GetFloat("LK.Objective.Height"));
    var q=(Quaternion)t.GetMethod("Facing").Invoke(null,new object[]{Quaternion.identity});var f=q*Vector3.forward;Assert.That(Mathf.Asin(f.y)*Mathf.Rad2Deg,Is.EqualTo(actual).Within(.01));Assert.That(f.x,Is.EqualTo(0).Within(.001));}}
   finally{height.SetValue(null,old);}
  }
  [Test] public void TapShowsImmediatelyHoldExtendsAndOnlyDoubleTapReplays() {
   var t=T("ObjectiveRecallState");var o=Activator.CreateInstance(t);var input=t.GetMethod("Input");var visible=t.GetMethod("Visible");
   Assert.AreEqual(1,input.Invoke(o,new object[]{true,10f}));Assert.True((bool)visible.Invoke(o,new object[]{10f}));
   Assert.AreEqual(0,input.Invoke(o,new object[]{false,10.1f}));Assert.AreEqual(2,input.Invoke(o,new object[]{true,10.2f}));
   Assert.AreEqual(0,input.Invoke(o,new object[]{true,18f}));Assert.True((bool)visible.Invoke(o,new object[]{18f}));
   input.Invoke(o,new object[]{false,19f});Assert.True((bool)visible.Invoke(o,new object[]{23.9f}));Assert.False((bool)visible.Invoke(o,new object[]{24.1f}));
   Assert.AreEqual(1,input.Invoke(o,new object[]{true,25f}));
  }
  [Test] public void HistoryKeepsThreeMostRecentHostLines() {
   var t=T("HarborRecallHistory");var o=Activator.CreateInstance(t);foreach(int cue in new[]{1,7,3,5})t.GetMethod("Remember").Invoke(o,new object[]{cue});
   CollectionAssert.AreEqual(new[]{5,3,7},(System.Collections.IEnumerable)t.GetProperty("Cues").GetValue(o));Assert.AreEqual(5,t.GetProperty("Latest").GetValue(o));
  }
  [Test] public void ReminderPersistsUntilActualBUseEvenOnPreviouslyTaughtInstalls() {
   const string key="LK.Objective.BUsed.v2";int old=PlayerPrefs.GetInt(key,0),legacy=PlayerPrefs.GetInt("LK.Objective.BTaught",0);var hud=T("ObjectiveHud");var due=hud.GetMethod("RecallTeachingDue");
   try {PlayerPrefs.SetInt(key,0);PlayerPrefs.SetInt("LK.Objective.BTaught",1);
    Assert.True((bool)due.Invoke(null,new object[]{1,int.MinValue,2}),"Arrival teaches B even if the old flash was missed");
    Assert.True((bool)due.Invoke(null,new object[]{2,2,3}));Assert.True((bool)due.Invoke(null,new object[]{5,3,4}));
    Assert.False((bool)due.Invoke(null,new object[]{1,2,2}),"An unchanged objective must not repeat the hint");
    hud.GetMethod("AcknowledgeRecallButton").Invoke(null,null);
    Assert.False((bool)due.Invoke(null,new object[]{1,2,3}));Assert.False((bool)due.Invoke(null,new object[]{5,3,4}));
   } finally {PlayerPrefs.SetInt(key,old);PlayerPrefs.SetInt("LK.Objective.BTaught",legacy);}
  }
  [Test] public void IdleRecallRemindersRespectSpacingAndBlockedPresentation() {
   var method=T("ObjectiveHud").GetMethod("IdleRecallReminderDue");
   bool Due(float now,float next,float changed,bool blocked)=>(bool)method.Invoke(null,new object[]{now,next,changed,blocked});
   Assert.False(Due(59,60,0,false));Assert.True(Due(60,60,0,false));
   Assert.False(Due(150,150,125,false),"A recently changed objective should not immediately nag.");
   Assert.False(Due(150,150,0,true),"A briefing, loss, or sleep must not reveal a reminder.");
   Assert.True(Due(150,150,0,false));Assert.False(Due(float.NaN,150,0,false));
  }
  [Test] public void OwenGuidanceFollowsHisFloorAndPortableWalkieHasNoInventedFloor() {
   var root=new GameObject("Companion objective fixture");var actor=new GameObject("Owen fixture");
   var watch=root.AddComponent(T("WatchDemoController"));var companion=root.AddComponent(T("CrewCompanion"));
   var actorField=companion.GetType().GetField("actor",BindingFlags.Instance|BindingFlags.NonPublic);actorField.SetValue(companion,actor);
   try {
    var step=T("KeeperStep");var follow=Enum.Parse(step,"HelpPartner");var portable=Enum.Parse(step,"ReadyShip");
    var floor=watch.GetType().GetMethod("InstructionFloor");
    foreach(var sample in new[]{(new Vector3(0,0,-6),0),(Vector3.zero,1),(new Vector3(0,3.3f,0),2),(new Vector3(0,6.6f,0),3),(new Vector3(0,9.9f,0),4)}){
     actor.transform.position=sample.Item1;Assert.AreEqual(sample.Item2,floor.Invoke(watch,new[]{follow}));
    }
    Assert.AreEqual(-1,floor.Invoke(watch,new[]{portable}));
    var director=T("ObjectiveDirector");Assert.IsNotEmpty((string)director.GetMethod("Location").Invoke(null,new[]{follow}));
    Assert.IsNotEmpty((string)director.GetMethod("Location").Invoke(null,new[]{portable}));
    Assert.AreEqual("Your walkie",T("ObjectiveHud").GetMethod("LocationLine").Invoke(null,new object[]{"Your walkie",-1,2}));
    var guide=root.AddComponent(T("DirectorPresentation"));guide.GetType().GetField("watch",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(guide,watch);
    Assert.AreSame(actor.transform,guide.GetType().GetMethod("Target").Invoke(guide,new[]{follow}));
    actor.SetActive(false);Assert.AreEqual(-1,floor.Invoke(watch,new[]{follow}));
   }finally{actorField.SetValue(companion,null);UnityEngine.Object.DestroyImmediate(actor);UnityEngine.Object.DestroyImmediate(root);}
  }
 }
}
