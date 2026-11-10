using System;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
 public sealed partial class WatchState
 {
  NightSchedule schedule;
  bool advancingSchedule;
  public NightScenario Scenario=>Definition.scenario?.configured==true?Definition.scenario:null;
  public bool Campaign=>Scenario!=null;
  public int CampaignFaultMask=>schedule?.FiredMask??0;
  public double CampaignReliefRemaining=>schedule==null?0:Math.Max(0,schedule.ReliefUntil-Elapsed);
  double CampaignGraceRemaining=>schedule==null?0:Math.Max(0,schedule.GraceUntil-Elapsed);
  public bool FuelJobPending
  {
   get
   {
    if(schedule==null||Engine==null)return false;
    for(int i=Scenario.faults.Length-1;i>=0;i--)if(schedule.Fired(i)&&Scenario.faults[i].stationId==GeneratorStation)
     return Scenario.faults[i].kind==NightFaultKind.LowFuel&&Engine.Fuel<Scenario.refillTargetFraction;
    return false;
   }
  }
  void FinishJob(string station)
  {if(faults.Remove(station)&&!resetting){schedule?.Completed(Elapsed);Changed?.Invoke();}}
  void AdvanceSchedule()
  {
   if(schedule==null||ReadOnly||advancingSchedule)return;
   advancingSchedule=true;
   try{schedule.Advance(Elapsed,Engine?.Settled??Power==PowerState.On,id=>!IsPending(id)&&IsDone(id),ApplyNightFault);}
   finally{advancingSchedule=false;}
  }
  void ApplyNightFault(NightFault f)
  {
   faults.Add(f.stationId);
   if(f.kind==NightFaultKind.LowFuel){Engine?.SetFuel(Math.Min(Engine.Fuel,f.fuelFraction));}
   else if(f.stationId==PipeStation)Pipe.Reset(unchecked(++repairSeed),false);
   else if(f.stationId==GeneratorStation)Engine?.Reset(Definition.initialFuelFraction,Definition.fuelDrainPerSecond,false);
   else if(f.stationId==BeaconStation)Beacon?.Reset(Definition.beaconInitiallyLit,Definition.beaconInitialAlignment,Definition.beaconTargetAlignment,Definition.beaconInitialSoot,Definition.beaconSootPerSecond);
   else if(f.stationId==RadioStation)Radio.Reset(RadioAvailable);
   Changed?.Invoke();
  }
  bool ValidCampaignSnapshot(DayDefinition d,WatchSnapshot s)
  {
   bool enabled=d.scenario?.configured==true;
   if(s.campaignEnabled!=enabled)return false;
   if(!enabled)return s.nightFaultMask==0&&!s.nightSawPowerOff&&s.nightReliefUntil==0&&s.nightNextFaultAt==0&&s.nightGraceUntil==0;
   return new NightSchedule(d.scenario).ValidReplica(s.nightFaultMask,s.nightSawPowerOff,s.nightReliefUntil,s.nightNextFaultAt,s.nightGraceUntil,s.elapsed);
  }
 }
}
