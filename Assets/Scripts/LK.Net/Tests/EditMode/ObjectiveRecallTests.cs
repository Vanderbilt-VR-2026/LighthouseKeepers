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
 }
}
