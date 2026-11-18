using System;using UnityEngine;using NUnit.Framework;using LighthouseKeepers.Core;
namespace LighthouseKeepers.Net.Tests {
 public sealed class CrewCompanionTests {
  static Type T(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
  static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
  static V Get<V>(object o,string n)=>(V)o.GetType().GetProperty(n).GetValue(o);
  static object New(){var c=Resources.Load("M3Campaign");return Activator.CreateInstance(T("WatchState"),c.GetType().GetField("nights").GetValue(c),3,Resources.Load<S4Tuning>("S4Tuning"),Resources.Load<S4M2Tuning>("S4M2Tuning"),Resources.Load<ImmersionTuning>("ImmersionTuning"));}
  const int Bot=2147483646;
  [Test]public void PartnerEnablesPairedSupportWithoutTakingAwaySoloTime(){var s=New();double budget=Get<double>(s,"Duration");Call(s,"SetScriptedPartner",true);Assert.True(Get<bool>(s,"RadioRelay"));Assert.True(Get<bool>(Get<object>(s,"Pressure"),"CrewRequired"));Assert.AreEqual(budget,Get<double>(s,"Duration"));Assert.AreEqual(1,Get<int>(s,"HumanKeeperCount"));}
  [Test]public void HumanJoinReleasesBotBeforeAcceptingHumanValve(){var s=New();Call(s,"SetScriptedPartner",true);var p=Get<PressureValveState>(s,"Pressure");Assert.True(p.Hold(Bot,true));Assert.True(p.Allows(1));Assert.False(p.Allows(Bot));Call(s,"SetKeeperCount",2);Assert.False(Get<bool>(s,"ScriptedPartner"));Assert.Zero(p.Holder);Assert.True(p.Hold(2,true));Call(s,"SetScriptedPartner",true);Assert.False(Get<bool>(s,"ScriptedPartner"));Assert.AreEqual(2,p.Holder);}
  [Test]public void CompanionDoesNotReassignHumanObjectiveOnEveryHeadMovement(){
   var s=New();Call(s,"SetScriptedPartner",true);var d=Get<object>(s,"Director");
   d.GetType().GetField("StationPosition").SetValue(d,new Func<string,Vector3>(id=>id=="STN_PIPE_LEAK"?Vector3.zero:Vector3.right*100));
   Call(d,"Tick",0f,1,Vector3.zero,0,Vector3.zero,true);var first=Get<object>(d,"First");
   Call(d,"Tick",.1f,1,Vector3.right*100,0,Vector3.zero,true);Assert.AreEqual(first,Get<object>(d,"First"),"A moving human retains the current instruction for the authored assignment hold.");
   Call(d,"Tick",10f,1,Vector3.right*100,0,Vector3.zero,true);Assert.AreEqual(first,Get<object>(d,"First"),"Campaign dependencies stay stable until the engine is repaired, regardless of head position.");
  }
  [Test]public void ReplicaCannotSpawnPartnerOrChangeAuthority(){var h=New();Call(h,"SetScriptedPartner",true);Call(Get<object>(h,"Director"),"Tick",0f,1,Vector3.zero,0,Vector3.zero,true);var packet=Call(h,"Capture",.1f);Assert.True((bool)Call(packet,"Valid"));var g=New();Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",packet));Assert.True(Get<bool>(g,"ScriptedPartner"));Call(Get<object>(g,"Director"),"Tick",1f,42,Vector3.zero,0,Vector3.zero,false);Assert.AreEqual(Bot,Get<int>(Get<object>(g,"Director"),"SecondActor"));Call(g,"SetScriptedPartner",false);Assert.True(Get<bool>(g,"ScriptedPartner"));packet.GetType().GetField("keepers").SetValue(packet,2);Assert.False((bool)Call(packet,"Valid"));}
  [Test]public void HelperShedsLoadBeforeResetAndKeepsTheRequestedCircuit(){
   var s=New();var grid=Get<PowerGrid>(s,"Grid");var method=T("CrewCompanion").GetMethod("WorkCircuitMask");
   Call(s,"CompleteRepair");
   foreach(float capacity in new[]{2.5f,3.25f})foreach(PowerCircuit priority in new[]{PowerCircuit.Pump,PowerCircuit.Radio,PowerCircuit.Beacon}){
    grid.Reset(capacity,15,true);int wanted=(int)method.Invoke(null,new object[]{s,priority});
    Assert.AreNotEqual(0,wanted&(1<<(int)priority));
    for(int n=0;n<4;n++)if((wanted&(1<<n))==0)grid.SetCircuit((PowerCircuit)n,false);
    for(int n=0;n<4;n++)if((wanted&(1<<n))!=0)grid.SetCircuit((PowerCircuit)n,true);
    Assert.LessOrEqual(grid.Demand,capacity+.0001f);if(grid.Tripped)Assert.True(grid.ResetBreaker());Assert.True(grid.Powered(priority));
    Assert.AreEqual(wanted,(int)method.Invoke(null,new object[]{s,priority}),"Plan must remain stable rather than oscillating switches");
   }
  }
 }
}
