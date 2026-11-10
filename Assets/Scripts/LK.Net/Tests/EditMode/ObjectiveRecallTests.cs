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
 }
}
