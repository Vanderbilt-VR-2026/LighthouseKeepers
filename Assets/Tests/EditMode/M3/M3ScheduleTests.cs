using System.Collections.Generic;
using NUnit.Framework;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Tests.M3
{
 public sealed class M3ScheduleTests
 {
  NightScenario Data()=>new NightScenario{openingSeconds=45,reliefSeconds=15,faultSpacingSeconds=12,faults=new[]{
   new NightFault{id="pipe",stationId="pipe"},
   new NightFault{id="generator",stationId="generator",gate=NightEventGate.ClockOrJobComplete,atSeconds=75,afterStationId="pipe",graceSeconds=60},
   new NightFault{id="radio",stationId="radio",gate=NightEventGate.PowerRestored},
   new NightFault{id="beacon",stationId="beacon",gate=NightEventGate.PowerRestored}}};
  [Test] public void OpeningReliefAndPowerRestorationGateTheJobs()
  {
   var s=new NightSchedule(Data());var fired=new List<string>();bool pipe=false;
   void Step(double t,bool power=true)=>s.Advance(t,power,id=>id=="pipe"&&pipe,f=>fired.Add(f.id));
   Step(0);CollectionAssert.AreEqual(new[]{"pipe"},fired);Assert.True(s.Pending("generator"));
   pipe=true;s.Completed(40);Step(45);Assert.AreEqual(1,fired.Count);Step(55);Assert.AreEqual("generator",fired[1]);
   Step(70,false);Assert.True(s.Pending("radio"));Step(71);Assert.AreEqual("radio",fired[2]);Step(82);Assert.AreEqual(3,fired.Count);Step(83);Assert.True(s.Finished);
   Step(500);Assert.AreEqual(4,fired.Count);Assert.AreEqual(115,s.GraceUntil);
  }
  [Test] public void PowerInitiallyOnDoesNotUnlockUpperJobsAndTimedFailureWorks()
  {
   var s=new NightSchedule(Data());var fired=new List<string>();s.Advance(0,true,_=>false,f=>fired.Add(f.id));s.Advance(74,true,_=>false,f=>fired.Add(f.id));Assert.AreEqual(1,fired.Count);
   s.Advance(75,true,_=>false,f=>fired.Add(f.id));Assert.AreEqual(2,fired.Count);Assert.False(s.Finished);
  }
  [Test] public void SnapshotPreservesOneShotEventsAndRejectsInvalidBounds()
  {
   var d=Data();var host=new NightSchedule(d);host.Advance(0,true,_=>false,_=>{});host.Advance(80,false,_=>false,_=>{});host.Completed(81);
   var guest=new NightSchedule(d);Assert.True(guest.ValidReplica(host.FiredMask,host.SawPowerOff,host.ReliefUntil,host.NextFaultAt,host.GraceUntil,81));
   guest.Apply(host.FiredMask,host.SawPowerOff,host.ReliefUntil,host.NextFaultAt,host.GraceUntil);Assert.AreEqual(host.Pending("generator"),guest.Pending("generator"));
   Assert.False(guest.ValidReplica(1<<10,false,0,0,0,81));Assert.False(guest.ValidReplica(1,false,double.NaN,0,0,81));Assert.False(guest.ValidReplica(1,false,999,0,0,81));
   Assert.True(new NightSchedule(d).Pending("pipe"),"Retry creates a fresh schedule");
  }
  [Test] public void OnlyExplicitFinaleAllowsCoincidentRepeatFaults()
  {
   var d=Data();d.faults=new[]{new NightFault{id="a",stationId="pipe",atSeconds=225,repeat=true},new NightFault{id="b",stationId="generator",atSeconds=225,repeat=true}};
   var normal=new NightSchedule(d);normal.Advance(225,true,_=>false,_=>{});Assert.False(normal.Finished);
   d.finaleFaultPileup=true;var finale=new NightSchedule(d);finale.Advance(225,true,_=>false,_=>{});Assert.True(finale.Finished);
  }
 }
}
