using System;
using UnityEngine;
using NUnit.Framework;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.Net.Tests
{
 public sealed class ConnectedTasksTests
 {
  static Type T(string n)=>Type.GetType("LighthouseKeepers.WatchDemo."+n+", Assembly-CSharp",true);
  static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n).Invoke(o,a);
  static V Get<V>(object o,string n)=>(V)o.GetType().GetProperty(n).GetValue(o);
  static object New(int day){var c=Resources.Load("WatchDaysS4M2");return Activator.CreateInstance(T("WatchState"),c.GetType().GetField("days").GetValue(c),day,Resources.Load<S4Tuning>("S4Tuning"),Resources.Load<S4M2Tuning>("S4M2Tuning"),Resources.Load<ImmersionTuning>("ImmersionTuning"));}
  [Test] public void BroadcastNeedsActualLightWithoutDiscardingTuningOrCode()
  {
   var s=New(1);Call(s,"SetCircuit",2,true);var r=Get<object>(s,"Radio");Call(r,"ToggleMain");
   var p=r.GetType().GetField("Program").GetValue(r);float f=(float)p.GetType().GetField("targetMHz").GetValue(p);
   Call(r,"Tune",f);Call(s,"Tick",2d);foreach(int c in (int[])p.GetType().GetField("callCode").GetValue(p))Call(r,"PressCode",c);
   Call(s,"IgniteBeacon",false);Call(r,"SetMicrophone",true,true);Call(s,"Tick",4d);
   Assert.False(Get<bool>(r,"Completed"));Assert.True(Get<bool>(r,"Locked"));Assert.True(Get<bool>(r,"CodeAccepted"));
   Call(s,"IgniteBeacon",true);Call(r,"SetMicrophone",true,true);Call(s,"Tick",4d);Assert.True(Get<bool>(r,"Completed"));
  }
  [Test] public void RadioGuidanceRestoresBeaconBeforeTransmitAndPumpRequiresItsCircuit()
  {
   var s=New(3);var d=Get<object>(s,"Director");Call(s,"CompleteRepair");
   var engine=Get<object>(s,"Engine");Call(engine,"Reset",.8f,0f,true);
   Assert.False(Get<bool>(s,"BeaconShining"));
   string step=Call(d,"StepForStation","STN_RADIO_CONSOLE",false).ToString();Assert.That(step,Does.StartWith("Beacon"));
   Call(s,"SetCircuit",0,false);Assert.False((bool)Call(s,"HasPower",PowerCircuit.Pump));
   Call(s,"SetCircuit",0,true);Assert.True((bool)Call(s,"HasPower",PowerCircuit.Pump));
  }
 }
}
