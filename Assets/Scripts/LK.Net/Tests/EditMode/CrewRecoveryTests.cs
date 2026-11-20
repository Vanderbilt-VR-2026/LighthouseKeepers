using System;using NUnit.Framework;using UnityEngine;using LighthouseKeepers.Core;using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Net.Tests {
 public sealed class CrewRecoveryTests {
  HorrorTuning tuning;
  [SetUp]public void SetUp()=>tuning=ScriptableObject.CreateInstance<HorrorTuning>();
  [TearDown]public void TearDown()=>UnityEngine.Object.DestroyImmediate(tuning);
  CrewRecoveryState New(int humans=3){var s=new CrewRecoveryState(tuning);s.SetHumans(humans);return s;}
  static void Hold(CrewRecoveryState s,int who,int target,float seconds){for(float t=0;t<seconds&&s.Downed(target);t+=.05f){Assert.True(s.Hold(who,target,true));s.Tick(.05f);}}
  [Test]public void ReleaseRetainsWorkButDeadlineAndLeaseKeepRunning(){var s=New();Assert.True(s.Down(1));Hold(s,2,1,1);float p=s.Slot(1).progress;s.Hold(2,1,false);s.Tick(2);Assert.AreEqual(p,s.Slot(1).progress);Assert.That(s.Slot(1).remaining,Is.LessThan(38));Hold(s,2,1,1);s.Tick(2);Assert.That(s.Slot(1).progress,Is.LessThan(.85f));Hold(s,2,1,1);Assert.False(s.AnyDown);Assert.Greater(s.Slot(1).grace,0);Assert.False(s.Down(1));s.Tick(tuning.reviveGraceSeconds);Assert.True(s.Down(1));}
  [Test]public void AStaleGripOrSelfRescueCannotFinish(){var s=New();s.Down(2);Assert.False(s.Hold(2,2,true));Assert.True(s.Hold(1,2,true));s.Tick(5);Assert.That(s.Slot(2).progress,Is.EqualTo(tuning.reviveGripLease/tuning.reviveHoldSeconds).Within(.001));s.Tick(40);Assert.True(s.Failed);Assert.False(s.Hold(1,2,true));}
  [Test]public void BothDownLosesAndBotIsNotACatchableEscort(){var s=New();s.Down(1);s.Down(2);Assert.True(s.Failed);Assert.True(s.Capture().Valid());s=New(2);Assert.False(s.Down(1));s.Down(2);Hold(s,1,2,3.05f);Assert.False(s.AnyDown);}
  [Test]public void DisconnectReleasesHoldAndAbsentTargetWithoutFailingRemainingKeeper(){var s=New();s.Down(1);Hold(s,2,1,.5f);float p=s.Slot(1).progress;s.SetHumans(1);s.Tick(1);Assert.AreEqual(p,s.Slot(1).progress);Hold(s,2,1,2.6f);Assert.False(s.AnyDown);s=New();s.Down(2);s.SetHumans(1);Assert.False(s.AnyDown);s.SetHumans(3);Assert.False(s.Downed(2));}
  [Test]public void ReplicaCannotDownTickHoldOrChangeRosterAndSnapshotsAreCopies(){var h=New();h.Down(2);Hold(h,1,2,1);var frame=h.Capture();var g=New();g.ReadOnly=true;Assert.True(g.Apply(frame));Assert.False(g.Down(1));Assert.False(g.Hold(1,2,true));g.Tick(5);g.SetHumans(1);Assert.AreEqual(3,g.Humans);Assert.AreEqual(frame.machinist.progress,g.Slot(2).progress);frame.machinist.progress=0;Assert.Greater(g.Slot(2).progress,0);Assert.False(h.Apply(frame));}
  [Test]public void MalformedFrameRejectedWithoutChangingReplica(){var g=New();g.ReadOnly=true;var f=new RecoveryFrame();f.humans=3;f.lightkeeper.remaining=float.NaN;Assert.False(g.Apply(f));f=new(){humans=3};f.lightkeeper.down=f.machinist.down=true;Assert.False(g.Apply(f));f=new();f.machinist.down=true;Assert.False(g.Apply(f));Assert.False(g.AnyDown);}
  [Test]public void LessonWaitsForBothAndSoloHumanRevivesBotWithoutDeadline(){var s=New(1);s.AdvanceLesson(1,1,100,true,2,true);Assert.Zero(s.Lesson);s.AdvanceLesson(1,2,60,false,2,true);Assert.AreEqual(1,s.Lesson);s.AdvanceLesson(30,2,60,false,2,true);Assert.AreEqual(1,s.Lesson);s.AdvanceLesson(1,2,60,true,2,true);Assert.AreEqual(2,s.Lesson);s.AdvanceLesson(4,2,60,true,2,true);s.AdvanceLesson(6,2,60,true,2,true);Assert.AreEqual(4,s.Lesson);Assert.True(s.Downed(2));Assert.False(s.Downed(1));s.Tick(500);Assert.False(s.Failed);Assert.True(s.Capture().Valid());Hold(s,1,2,3.05f);s.AdvanceLesson(.1f,2,60,true,2,true);Assert.AreEqual(5,s.Lesson);s.AdvanceLesson(6,2,60,true,2,true);Assert.AreEqual(6,s.Lesson);Assert.False(s.RoomSealed);}
  [Test]public void LessonRosterChangeReleasesVictimAndRegathersInsteadOfTrappingLateJoin(){var s=New(1);s.AdvanceLesson(1,2,60,true,2,true);s.AdvanceLesson(8,2,60,true,2,true);s.AdvanceLesson(4,2,60,true,2,true);s.AdvanceLesson(6,2,60,true,2,true);Assert.True(s.AnyDown);s.SetHumans(3);Assert.AreEqual(1,s.Lesson);Assert.False(s.AnyDown);Assert.False(s.RoomSealed);Assert.True(s.Capture().Valid());}
  static Type T(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
  [Test]public void RoomAdmissionIgnoresKeeperCapsuleButRejectsTheStairs(){
   UnityEditor.EditorUtility.audioMasterMute=true;
   var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);var rig=new GameObject("test keeper");var stair=GameObject.CreatePrimitive(PrimitiveType.Cube);
   try{
    floor.transform.position=new Vector3(100,3.25f,100);floor.transform.localScale=new Vector3(5,.1f,5);
    rig.AddComponent(Type.GetType("Unity.XR.CoreUtils.XROrigin, Unity.XR.CoreUtils",true));var capsule=rig.AddComponent<CharacterController>();rig.transform.position=new Vector3(100,3.3f,100);capsule.height=1.8f;capsule.center=Vector3.up*.9f;
    stair.transform.position=new Vector3(101,3.5f,100);stair.transform.localScale=new Vector3(.6f,.4f,.6f);Physics.SyncTransforms();
    var inside=T("PossessionLesson").GetMethod("Inside");Assert.True((bool)inside.Invoke(null,new object[]{new Vector3(100,4.95f,100)}));Assert.False((bool)inside.Invoke(null,new object[]{new Vector3(101,5.15f,100)}));
   }finally{UnityEngine.Object.DestroyImmediate(floor);UnityEngine.Object.DestroyImmediate(rig);UnityEngine.Object.DestroyImmediate(stair);}
  }
  [Test]public void VisibleLiftStopsBelowTheCeiling(){
   var ceiling=GameObject.CreatePrimitive(PrimitiveType.Cube);try{
    ceiling.transform.position=new Vector3(100,5.5f,100);ceiling.transform.localScale=new Vector3(5,.1f,5);Physics.SyncTransforms();
    float lift=(float)T("PossessionLesson").GetMethod("SafeLift").Invoke(null,new object[]{new Vector3(100,5,100),.32f,.18f,new RaycastHit[24]});Assert.That(lift,Is.InRange(0,.025f));
   }finally{UnityEngine.Object.DestroyImmediate(ceiling);}
  }
  static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
  static V Get<V>(object o,string n)=>(V)o.GetType().GetProperty(n).GetValue(o);
  static object Watch(){var c=Resources.Load("M3Campaign");return Activator.CreateInstance(T("WatchState"),c.GetType().GetField("nights").GetValue(c),3,Resources.Load<S4Tuning>("S4Tuning"),Resources.Load<S4M2Tuning>("S4M2Tuning"),Resources.Load<ImmersionTuning>("ImmersionTuning"));}
  [Test]public void RescueLossReportsItsCauseAndRetryRestoresTheNight(){var w=Watch();var r=Get<CrewRecoveryState>(w,"Recovery");r.Down(1);r.Tick(41);Assert.True(Get<bool>(w,"Failed"));Assert.That(Get<string>(w,"FailureMessage"),Does.Contain("keeper"));Assert.False(Get<bool>(w,"Complete"));Call(w,"Retry");Assert.False(Get<bool>(w,"Failed"));Assert.False(Get<CrewRecoveryState>(w,"Recovery").AnyDown);}
  [Test]public void BreathLeaseReplicatesByDutyExpiresAndResetsOnRetry(){
   var h=Watch();Assert.True((bool)Call(h,"SetBreath",2,true));Assert.AreEqual(2,Get<int>(h,"BreathMask"));
   var g=Watch();Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",.1f)));Assert.AreEqual(2,Get<int>(g,"BreathMask"));Assert.False((bool)Call(g,"SetBreath",1,true));
   Call(h,"TickBreath",.5f);Assert.Zero(Get<int>(h,"BreathMask"));Call(h,"SetBreath",1,true);Call(h,"Retry");Assert.Zero(Get<int>(h,"BreathMask"));
  }
  [Test]public void WatchLateJoinRestoresDownProgressAndRetryClearsIt(){var h=Watch();var r=Get<CrewRecoveryState>(h,"Recovery");r.SetHumans(3);r.Down(2);Hold(r,1,2,1);var packet=Call(h,"Capture",.1f);Assert.True((bool)Call(packet,"Valid"));var g=Watch();Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",packet));Assert.True(Get<CrewRecoveryState>(g,"Recovery").Downed(2));Assert.Greater(Get<CrewRecoveryState>(g,"Recovery").Slot(2).progress,.3f);Call(h,"Retry");Assert.False(Get<CrewRecoveryState>(h,"Recovery").AnyDown);}
 }
}
