using System;
using NUnit.Framework;
using UnityEngine;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Tests.M3
{
 public sealed class SabotageTests
 {
  bool before;
  [SetUp] public void EnablePrototype(){before=Sprint5Tuning.Current.sabotageTraces;Sprint5Tuning.Current.sabotageTraces=true;}
  [TearDown] public void RestoreSwitch(){Sprint5Tuning.Current.sabotageTraces=before;}
  [Test] public void DisabledPrototypePreservesOriginalWatchAndReplica(){Sprint5Tuning.Current.sabotageTraces=false;var h=New();Assert.Null(Get<SabotageState>(h,"Sabotage"));Call(h,"Tick",78d);var g=New();Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",.1f)));Assert.Null(Get<SabotageState>(g,"Sabotage"));}
  static NightScenario Data()=>new(){openingSeconds=0,reliefSeconds=15,faultSpacingSeconds=15,faults=new[]{new NightFault{id="pipe",stationId="pipe",gate=NightEventGate.Clock,atSeconds=0},new NightFault{id="engine",stationId="engine",gate=NightEventGate.Clock,atSeconds=20},new NightFault{id="lens",stationId="lens",gate=NightEventGate.Clock,atSeconds=35}}};
  [Test] public void SchedulerGivesFullWarningBeforeFaultAndDoesNotDuplicate(){var d=Data();var s=new SabotageState(d);var n=new NightSchedule(d,null,s);int fired=0;n.Advance(0,true,_=>false,_=>fired++);Assert.AreEqual(1,fired);Assert.AreEqual(-1,s.Warning(0));n.Advance(20,true,_=>false,_=>fired++);Assert.AreEqual(20,s.Warning(1));Assert.AreEqual(1,fired);n.Advance(22.9,true,_=>false,_=>fired++);Assert.AreEqual(1,fired);n.Advance(23,true,_=>false,_=>fired++);n.Advance(23,true,_=>false,_=>fired++);Assert.AreEqual(2,fired);Assert.AreEqual(23,s.Broken(1));}
  [Test] public void ClockFaultWarningDoesNotMoveTheEstablishedDeadline(){var d=Data();var s=new SabotageState(d);var n=new NightSchedule(d,null,s);int fired=0;n.Advance(0,true,_=>false,_=>fired++);n.Advance(16.9,true,_=>false,_=>fired++);Assert.AreEqual(-1,s.Warning(1));n.Advance(17,true,_=>false,_=>fired++);Assert.AreEqual(17,s.Warning(1));n.Advance(19.9,true,_=>false,_=>fired++);Assert.AreEqual(1,fired);n.Advance(20,true,_=>false,_=>fired++);Assert.AreEqual(2,fired);Assert.AreEqual(20,s.Broken(1));}
  [Test] public void EvidenceSurvivesRepairThenFadesAndRetryClearsIt(){var d=Data();var s=new SabotageState(d);s.Broke(0,0);Assert.AreEqual(1,s.Evidence(0,500));s.Completed("pipe",20);Assert.AreEqual(1,s.Evidence(0,40));Assert.AreEqual(.5f,s.Evidence(0,41));Assert.Zero(s.Evidence(0,42));Assert.Zero(new SabotageState(d).Evidence(0,0));}
  [Test] public void FixCancelsPendingWarningAndBreatherRearmsIt(){var d=Data();var s=new SabotageState(d);var n=new NightSchedule(d,null,s);int fired=0;n.Advance(0,true,_=>false,_=>fired++);n.Advance(20,true,_=>false,_=>fired++);s.Completed("pipe",21);n.Completed(21);Assert.AreEqual(-1,s.Warning(1));n.Advance(35,true,_=>false,_=>fired++);Assert.AreEqual(1,fired);n.Advance(36,true,_=>false,_=>fired++);Assert.AreEqual(36,s.Warning(1));n.Advance(39,true,_=>false,_=>fired++);Assert.AreEqual(2,fired);}
  [Test] public void PhysicalFuelExhaustionIsNotDelayed(){var s=new SabotageState(Data());s.Warn(1,72);Assert.True(s.BeforeFault(1,75,true));s.Broke(1,75);Assert.AreEqual(3,s.Broken(1)-s.Warning(1));}
  [Test] public void GuestCannotInventWarningOrChangeEvidenceAndLateJoinHasSameFade(){var d=Data();var h=new SabotageState(d);h.Broke(0,0);h.Completed("pipe",5);h.Warn(1,20);var g=new SabotageState(d){ReadOnly=true};Assert.True(g.Apply(h.Capture(),1,20));g.Broke(1,21);g.Completed("engine",21);g.Warn(2,21);Assert.AreEqual(-1,g.Broken(1));Assert.AreEqual(-1,g.Warning(2));Assert.AreEqual(h.Evidence(0,26),g.Evidence(0,26));}
  [Test] public void CorruptOrContradictoryPayloadFails(){var d=Data();var s=new SabotageState(d);var f=s.Capture();f.warning[0]=double.NaN;Assert.False(s.ValidReplica(f,0,20));f=s.Capture();f.broken[0]=2;Assert.False(s.ValidReplica(f,0,20));f=s.Capture();f.fixedAt[0]=2;Assert.False(s.ValidReplica(f,0,20));f=s.Capture();f.warning[0]=21;Assert.False(s.ValidReplica(f,0,20));}
  static Type State=>Type.GetType("LighthouseKeepers.WatchDemo.WatchState, Assembly-CSharp",true);
  static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
  static T Get<T>(object o,string n)=>(T)o.GetType().GetProperty(n).GetValue(o);
  object New()=>Activator.CreateInstance(State,Resources.Load<FiveNightCatalog>("M3Campaign").nights,2,Resources.Load("S4Tuning"),Resources.Load("S4M2Tuning"),Resources.Load("ImmersionTuning"));
  [Test] public void FuelWarningAndRepairEvidenceReplicateInRealWatchAndRetry(){var h=New();Call(h,"Tick",74d);var traces=Get<SabotageState>(h,"Sabotage");Assert.NotNull(traces);bool warned=false;for(int i=0;i<traces.Count;i++)warned|=traces.Warning(i)>=0;Assert.True(warned,"Fuel gurgle before empty");Call(h,"Tick",4d);var g=New();Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",.1f)));Assert.AreEqual(Get<int>(h,"CampaignFaultMask"),Get<int>(g,"CampaignFaultMask"));Call(h,"CompleteRepair");Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",.1f)));Call(h,"Retry");Assert.True((bool)Call(g,"ApplyRemote",Call(h,"Capture",0f)));Assert.AreEqual(-1,Get<SabotageState>(g,"Sabotage").Warning(1));}
 }
}
