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
  [Test]public void ReplicaCannotSpawnPartnerOrChangeAuthority(){var h=New();Call(h,"SetScriptedPartner",true);var packet=Call(h,"Capture",.1f);Assert.True((bool)Call(packet,"Valid"));var g=New();Call(g,"SetRemoteReplica",true);Assert.True((bool)Call(g,"ApplyRemote",packet));Assert.True(Get<bool>(g,"ScriptedPartner"));Call(g,"SetScriptedPartner",false);Assert.True(Get<bool>(g,"ScriptedPartner"));packet.GetType().GetField("keepers").SetValue(packet,2);Assert.False((bool)Call(packet,"Valid"));}
 }
}
