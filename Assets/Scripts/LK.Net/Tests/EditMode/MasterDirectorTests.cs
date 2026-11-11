using System;using LighthouseKeepers.Core;using NUnit.Framework;using UnityEngine;
namespace LighthouseKeepers.Net.Tests {
 public sealed class MasterDirectorTests {
  static Type T(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
  static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
  static V Get<V>(object o,string n)=>(V)o.GetType().GetProperty(n).GetValue(o);
  object New(int day){var c=Resources.Load("WatchDaysS4M2");return Activator.CreateInstance(T("WatchState"),c.GetType().GetField("days").GetValue(c),day,Resources.Load<S4Tuning>("S4Tuning"),Resources.Load<S4M2Tuning>("S4M2Tuning"),Resources.Load<ImmersionTuning>("ImmersionTuning"));}
  [Test] public void GripOnlyDirectorSelectsActualActiveBoltThenSleep(){var s=New(1);var d=Get<object>(s,"Director");Call(d,"Tick",0f,0,Vector3.zero,0,Vector3.zero,false);Assert.AreEqual("GrabWrench",Get<object>(d,"First").ToString());Call(d,"Tick",1f,0,Vector3.zero,0,Vector3.zero,true);Assert.True(Get<bool>(s,"PipeReady"));var pipe=Get<object>(s,"Pipe");Assert.AreEqual("Bolt"+(char)('A'+Get<int>(pipe,"ActiveBolt")),Get<object>(d,"First").ToString());Call(s,"CompleteRepair");Call(d,"Tick",2f,0,Vector3.zero,0,Vector3.zero,false);Assert.AreEqual("Sleep",Get<object>(d,"First").ToString());}
  [Test] public void HostSplitsAvailableJobsAndGuestOnlyAppliesAssignments(){var s=New(2);Call(s,"Tick",75d);var d=Get<object>(s,"Director");Call(d,"Tick",0f,1,new Vector3(0,4.6f,0),2,new Vector3(0,1.6f,0),true);Assert.AreNotEqual(Get<object>(d,"First"),Get<object>(d,"Second"));var guest=New(2);Call(guest,"SetRemoteReplica",true);Assert.True((bool)Call(guest,"ApplyRemote",Call(s,"Capture",.02f)));var gd=Get<object>(guest,"Director");Assert.AreEqual(Get<object>(d,"First"),Get<object>(gd,"First"));Call(gd,"Tick",100f,2,Vector3.zero,1,Vector3.zero,false);Assert.AreEqual(Get<object>(d,"First"),Get<object>(gd,"First"));var snap=Call(s,"Capture",.02f);snap.GetType().GetField("stepA").SetValue(snap,999);Assert.False((bool)Call(guest,"ApplyRemote",snap));}
  [Test] public void GuidedGeneratorIgnoresExtraSqueezesAndDoesNotHideAChokeDeadline(){var t=ScriptableObject.CreateInstance<S4M2Tuning>();try{var e=new WatchEngineState(t){Guided=true,IgnoreExtraPulls=true};e.Reset(.8f,0,false);for(int i=0;i<3;i++){Assert.True(e.Prime());e.Tick(.5f);}Assert.False(e.Prime());e.SetChoke(true);for(int i=0;i<3;i++){Assert.True(e.Pull());e.Tick(.5f);}Assert.False(e.Pull());e.Tick(30);Assert.True(e.Running);Assert.False(e.Settled);e.SetChoke(false);Assert.True(e.Settled);}finally{UnityEngine.Object.DestroyImmediate(t);}}
  [Test] public void InstructionsStayShortAndTableHasEveryStep(){var strings=T("KeeperStrings");foreach(var step in Enum.GetValues(T("KeeperStep"))){string key="step."+step;Assert.True((bool)strings.GetMethod("Has").Invoke(null,new object[]{key}),key);var text=(string)strings.GetMethod("Get").Invoke(null,new object[]{key});Assert.LessOrEqual(text.Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries).Length,6,key);}}
  [Test]public void ControllerHintBuildsActualVisibleGeometry(){
   var go=new GameObject("Hint canvas",typeof(RectTransform),typeof(Canvas));
   try {var hint=(UnityEngine.UI.Graphic)go.AddComponent(T("ObjectiveInputIcon"));Assert.NotNull(go.GetComponent<CanvasRenderer>());hint.Rebuild(UnityEngine.UI.CanvasUpdate.PreRender);var mesh=hint.canvasRenderer.GetMesh();Assert.NotNull(mesh);Assert.AreEqual(16,mesh.vertexCount);Assert.Greater(mesh.colors[0].a,.9f);}
   finally{UnityEngine.Object.DestroyImmediate(go);}
  }
  [Test]public void EveryActionHasAnObjectLocationAndOneButton(){
   var director=T("ObjectiveDirector");var stepType=T("KeeperStep");
   foreach(var step in Enum.GetValues(stepType)){
    string name=step.ToString();if(name=="None"||name=="Complete"||name=="Lost")continue;
    Assert.IsNotEmpty((string)director.GetMethod("Location").Invoke(null,new[]{step}),name);
    string input=(string)director.GetMethod("Button").Invoke(null,new[]{step});
    Assert.AreEqual(name=="CheckFuel"?"":name=="RadioTransmit"?"TRIGGER":"GRIP",input,name);
   }
  }
  [Test] public void PressureUsesHostWaterAndRejectsInvalidPeaks(){var host=New(2);Call(host,"ObserveWater",.1d,.44f);Assert.GreaterOrEqual(Get<float>(host,"PeakDanger"),.79f);var guest=New(2);Call(guest,"SetRemoteReplica",true);Assert.True((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.44f)));Assert.AreEqual(Get<float>(host,"Danger"),Get<float>(guest,"Danger"),.001f);Call(guest,"ObserveWater",10d,.9f);Assert.AreEqual(.44f,Get<float>(guest,"LatestWater"));var bad=Call(host,"Capture",.44f);bad.GetType().GetField("peakDanger").SetValue(bad,float.NaN);Assert.False((bool)Call(guest,"ApplyRemote",bad));}
  [Test] public void WarningRecordingsAreSingleTakesWithCaptions(){var cat=Resources.Load("HarborRadioLines");for(int id=19;id<=43;id++){var clip=(AudioClip)Call(cat,"Line",id);Assert.NotNull(clip,"cue "+id);Assert.Less(clip.length,10,"cue "+id);Assert.IsNotEmpty((string)Call(cat,"Caption",id));}}

  [Test] public void StormEscalatesCalmsAndReplicatesWithoutGuestAuthorship(){var t=T("StormDirector");var choose=t.GetMethod("Choose");Assert.AreEqual(1,choose.Invoke(null,new object[]{590d,600d,.1f}));Assert.AreEqual(4,choose.Invoke(null,new object[]{590d,600d,.95f}));Assert.AreEqual(4,choose.Invoke(null,new object[]{119d,600d,0f}));var host=New(3);Call(host,"Tick",40d);var storm=Get<object>(host,"Storm");var guest=New(3);Call(guest,"SetRemoteReplica",true);Assert.True((bool)Call(guest,"ApplyRemote",Call(host,"Capture",.02f)));Assert.AreEqual(Get<int>(storm,"Stage"),Get<int>(Get<object>(guest,"Storm"),"Stage"));var snap=Call(host,"Capture",.02f);snap.GetType().GetField("stormStage").SetValue(snap,5);Assert.False((bool)Call(guest,"ApplyRemote",snap));}

  [Test] public void TutorialCannotLoseAndNightTwoOutageGetsLearningGrace(){var first=New(1);Call(first,"Tick",10000d);Call(first,"ObserveWater",60d,2f);Assert.False(Get<bool>(first,"Failed"));Assert.False(Get<bool>(first,"HighWaterFailed"));var second=New(2);Assert.AreEqual("On",Get<object>(second,"Power").ToString());Assert.True((bool)Call(second,"IsPending","STN_GENERATOR"));Call(second,"Tick",74d);Assert.AreEqual("On",Get<object>(second,"Power").ToString());Call(second,"Tick",2d);Assert.AreEqual("Off",Get<object>(second,"Power").ToString());Assert.AreEqual(119,Get<double>(second,"BeaconGraceRemaining"),.01);var guest=New(2);Call(guest,"SetRemoteReplica",true);Assert.True((bool)Call(guest,"ApplyRemote",Call(second,"Capture",.1f)));Assert.AreEqual(119,Get<double>(guest,"BeaconGraceRemaining"),.01);}

 }
}
