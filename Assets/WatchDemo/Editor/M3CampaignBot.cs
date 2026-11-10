#if UNITY_EDITOR
using System;using System.Collections.Generic;using System.IO;using UnityEditor;using UnityEngine;
using LighthouseKeepers.Core;using LighthouseKeepers.Core.Progression;using LighthouseKeepers.Lobby;using LighthouseKeepers.WatchDemo;
namespace LighthouseKeepers.Editor
{
 // Instruction-only rule simulation. Read/gesture/travel durations are explicit assumptions,
 // not measured human times. Two workers advance concurrently on the same authoritative state.
 public static class M3CampaignBot
 {
  sealed class Crew:ICoopWatchLink {public bool Ready=>true;public bool Send(CoopAction a,float v=0,bool p=false)=>true;public bool RemoteHeld(Component p)=>false;public bool RemoteMicrophone(Component p,out bool t){t=false;return false;}}
  sealed class Worker {public int actor,floor,targetFloor;public KeeperStep step;public double ready,start;public bool busy;}
  [Serializable] public sealed class Result {public string night,mode;public bool passed;public double seconds,spare,bedTravelSeconds;public float peakWater;public string cause;public List<string> steps=new();}
  [Serializable] public sealed class Report {public List<Result> runs=new();}
  public static object Run(string path)
  {
   EditorUtility.audioMasterMute=true;var c=Resources.Load<FiveNightCatalog>("M3Campaign");var report=new Report();
   foreach(var d in c.nights)foreach(bool crew in new[]{false,true})report.runs.Add(One(c.nights,d,crew));
   foreach(bool crew in new[]{false,true})report.runs.Add(One(new[]{c.demo},c.demo,crew));
   Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,JsonUtility.ToJson(report,true));return report;
  }
  static Result One(DayDefinition[] all,DayDefinition day,bool crew)
  {
   var s=new WatchState(all,day.day,S4Tuning.Current,S4M2Tuning.Current,ImmersionTuning.Current);if(crew)s.Coop=new Crew();s.Pressure?.SetCrew(crew&&s.RequiresCrewValve);
   var r=new Result{night=day.scenario.id,mode=crew?"duo":"solo"};var workers=crew?new[]{new Worker{actor=1},new Worker{actor=2}}:new[]{new Worker{actor=1}};
   int wrench=0;float water=.02f;const float dt=.1f;double clock=0;
   bool Work(KeeperStep k)=>k!=KeeperStep.None&&k!=KeeperStep.HoldValve&&k!=KeeperStep.Sleep&&k!=KeeperStep.Complete&&k!=KeeperStep.Lost;
   while(clock<1200&&!s.Failed&&!s.Complete)
   {
    s.PipeWorkerActor=wrench;
    s.Director.Tick((float)clock,1,new Vector3(0,workers[0].floor*3.3f,0),crew?2:0,crew?new Vector3(0,workers[1].floor*3.3f,0):Vector3.zero,wrench!=0);
    foreach(var w in workers)
    {
     var next=s.Director.ForActor(w.actor);
     // Do not abandon a held valve because the other keeper is still reading the instruction.
     if(next==KeeperStep.HoldValve&&w.floor==1){s.Pressure.Hold(w.actor,true);w.busy=false;continue;}
     if(s.Pressure?.Holder==w.actor)s.Pressure.Hold(w.actor,false);
     if(!Work(next)&&next!=KeeperStep.HoldValve){w.busy=false;continue;}
     if(!w.busy||w.step!=next)
     {
      w.step=next;w.start=clock;w.busy=true;
      int floor=next==KeeperStep.GrabWrench?0:ObjectiveDirector.Floor(next);
      w.ready=clock+Math.Abs(floor-w.floor)*12+4+Gesture(next);w.targetFloor=floor;
     }
     if(clock+.0001<w.ready)continue;
     w.floor=w.targetFloor;
     if(next==KeeperStep.HoldValve){s.Pressure.Hold(w.actor,true);w.busy=false;continue;}
     if(next==KeeperStep.GrabWrench){if(wrench==0)wrench=w.actor;}
     else if(next==KeeperStep.ClampValve)s.Pressure.Clamp();
     else if(next==KeeperStep.TightenBolt||next>=KeeperStep.BoltA&&next<=KeeperStep.BoltC)
     {if(wrench==w.actor){s.PipeWorkerActor=w.actor;s.Pipe.Observe(.1f,0,true,true,true);s.Pipe.Observe(.2f,s.Pipe.Profile.quarterTurnDegrees,true,true,true);s.Pipe.Observe(.2f,-s.Pipe.Profile.returnDegrees,true,true,true);}}
     else if(next==KeeperStep.Fuel){for(int i=0;i<300&&s.Engine.Fuel<s.Master.guidedFuelTarget;i++)s.PourFuel(.1f);}
     else if(next==KeeperStep.Prime)s.PrimeEngine();
     else if(next==KeeperStep.CloseChoke)s.SetChoke(true);
     else if(next==KeeperStep.Starter)s.StarterPulled();
     else if(next==KeeperStep.OpenChoke)s.SetChoke(false);
     else if(next==KeeperStep.ResetBreaker)s.ResetBreaker();
     else if(next>=KeeperStep.PumpOff&&next<=KeeperStep.LightsOff)s.SetCircuit((int)next-(int)KeeperStep.PumpOff,false);
     else if(next>=KeeperStep.PumpOn&&next<=KeeperStep.LightsOn)s.SetCircuit((int)next-(int)KeeperStep.PumpOn,true);
     else if(next==KeeperStep.RadioPower)s.Radio.ToggleMain();
     else if(next==KeeperStep.RadioTune)s.Radio.Tune(s.Radio.Program.targetMHz);
     else if(next==KeeperStep.RadioCode)s.Radio.PressCode(s.Radio.Program.callCode[s.Radio.CodeProgress]);
     else if(next==KeeperStep.RadioTransmit)s.Radio.SetMicrophone(true,true);
     else if(next==KeeperStep.BeaconLight)s.IgniteBeacon(true);
     else if(next==KeeperStep.BeaconAlign)s.AlignBeacon(s.Beacon.Target);
     else if(next==KeeperStep.BeaconWipe)s.WipeBeacon(1);
     else if(next!=KeeperStep.CheckFuel)throw new InvalidOperationException("Unhandled instruction "+next);
     r.steps.Add($"{clock:F1}s keeper{w.actor} {next} ({clock-w.start:F1}s travel/read/action)");w.busy=false;
    }
    s.AdvanceWatchGate(dt);if(!s.WatchStartHeld)s.Tick(dt);
    if(!s.WatchStartHeld){water=Mathf.Max(.02f,water+(s.Repaired?(s.HasPower(PowerCircuit.Pump)?-s.Tuning.drainMPerSecond:0):(s.HasPower(PowerCircuit.Pump)?s.Tuning.poweredRiseMPerSecond:s.Tuning.unpoweredRiseMPerSecond)*s.FloodRiseMultiplier*s.LeakStrength)*dt);if(s.Tutorial)water=Mathf.Min(water,s.Master.tutorialFloodCap);s.ObserveWater(dt,water);}
    r.peakWater=Mathf.Max(r.peakWater,water);clock+=dt;
   }
   if(s.Complete&&!s.Failed){foreach(var w in workers)r.bedTravelSeconds=Math.Max(r.bedTravelSeconds,Math.Abs(3-w.floor)*12+4);s.Tick(r.bedTravelSeconds);clock+=r.bedTravelSeconds;}
   r.seconds=clock;r.spare=s.Duration-s.Elapsed;r.passed=s.Complete&&!s.Failed;r.cause=r.passed?"tasks complete; return-to-bed travel modeled (ships/shared-bed transport not measured)":s.Failed?s.FailureMessage:"instruction stalled";return r;
  }
  static float Gesture(KeeperStep k)=>k==KeeperStep.GrabWrench?8:k==KeeperStep.Fuel?12:k==KeeperStep.RadioTune?18:k==KeeperStep.BeaconAlign?14:k==KeeperStep.TightenBolt||k>=KeeperStep.BoltA&&k<=KeeperStep.BoltC?6:k==KeeperStep.Starter?3:k==KeeperStep.Prime?2:1;
 }
}
#endif
