#if UNITY_EDITOR
using System;using System.Collections.Generic;using System.IO;using UnityEngine;using LighthouseKeepers.Core;using LighthouseKeepers.Core.Progression;using LighthouseKeepers.WatchDemo;
namespace LighthouseKeepers.Editor {
 // Deterministic decision/pressure simulation, NOT a human timing measurement or locomotion test.
 // It can act only on the current directive; unknown/blocked directives fail instead of skipping jobs.
 public static class FirstTimerDirectorBot {
  public static object Run(string path){var lines=new List<string>();bool pass=true;
   for(int day=1;day<=3;day++){
    var s=new WatchState(Resources.Load<DayCatalog>("WatchDaysS4M2").days,day,S4Tuning.Current,S4M2Tuning.Current,ImmersionTuning.Current);
    bool held=false;int floor=0;float water=.02f;double simulated=0;int safety=0;var d=s.Director;
    void Advance(float seconds){for(float t=0;t<seconds;t+=.1f){float dt=Mathf.Min(.1f,seconds-t);s.Tick(dt);simulated+=dt;water=Mathf.Max(.02f,water+(s.Repaired?(s.HasPower(PowerCircuit.Pump)?-.012f:0):(s.HasPower(PowerCircuit.Pump)?.0019f:.0057f)*s.FloodRiseMultiplier*s.Pipe.InflowFraction)*dt);if(s.Tutorial)water=Mathf.Min(water,s.Master.tutorialFloodCap);s.ObserveWater(dt,water);}}
    while(!s.Complete&&!s.Failed&&safety++<400){d.Tick((float)simulated,0,new Vector3(0,floor*3.3f,0),0,Vector3.zero,held);var step=d.First;double start=simulated;int nextFloor=ObjectiveDirector.Floor(step);if(nextFloor!=floor){Advance(12*Mathf.Abs(nextFloor-floor));floor=nextFloor;}Advance(4);
     switch(step){
      case KeeperStep.GrabWrench:Advance(8);held=true;break;
      case KeeperStep.BoltA:case KeeperStep.BoltB:case KeeperStep.BoltC:
       int active=s.Pipe.ActiveBolt;for(int k=0;k<3&&s.Pipe.ActiveBolt==active;k++){s.Pipe.Observe(.1f,0,true,true,true);Advance(5);s.Pipe.Observe(.2f,30,true,true,true);s.Pipe.Observe(.2f,-30,true,true,true);Advance(1);}break;
      case KeeperStep.CheckFuel:Advance(1);break;
      case KeeperStep.Fuel:for(int k=0;k<300&&s.Engine.Fuel<s.Master.guidedFuelTarget;k++){s.PourFuel(.1f);Advance(.1f);}Advance(8);break;
      case KeeperStep.Prime:s.PrimeEngine();Advance(2);break;
      case KeeperStep.CloseChoke:s.SetChoke(true);Advance(1);break;
      case KeeperStep.Starter:s.StarterPulled();Advance(3);break;
      case KeeperStep.OpenChoke:s.SetChoke(false);Advance(1);break;
      case KeeperStep.ResetBreaker:s.ResetBreaker();break;
      case KeeperStep.RadioPower:s.Radio.ToggleMain();break;
      case KeeperStep.RadioTune:s.Radio.Tune(s.Radio.Program.targetMHz);Advance(18);break;
      case KeeperStep.RadioCode:s.Radio.PressCode(s.Radio.Program.callCode[s.Radio.CodeProgress]);Advance(1);break;
      case KeeperStep.RadioTransmit:s.Radio.SetMicrophone(true,true);Advance(4);s.Radio.SetMicrophone(false,false);break;
      case KeeperStep.BeaconLight:s.IgniteBeacon(true);break;
      case KeeperStep.BeaconAlign:Advance(14);s.AlignBeacon(s.Beacon.Target);break;
      case KeeperStep.BeaconWipe:s.WipeBeacon(1);break;
      default:if(step>=KeeperStep.PumpOff&&step<=KeeperStep.LightsOff)s.SetCircuit((int)step-(int)KeeperStep.PumpOff,false);else if(step>=KeeperStep.PumpOn&&step<=KeeperStep.LightsOn)s.SetCircuit((int)step-(int)KeeperStep.PumpOn,true);else throw new InvalidOperationException("No actionable directive: "+step);break;
     }
     lines.Add($"Night {day} | {step} | {simulated-start:F1} s including travel/read | elapsed {simulated:F1} | water {water:F3}");
    }
    bool won=s.Complete&&!s.Failed;if(won){Advance(12*Mathf.Abs(3-floor)+4);d.Tick((float)simulated,0,new Vector3(0,9.9f,0),0,Vector3.zero,held);won=d.First==KeeperStep.Sleep&&!s.Failed;}double spare=s.Duration-s.Elapsed;pass&=won;lines.Add($"NIGHT {day}: {(won?"PASS":"FAIL")} total={simulated:F1}s spare={spare:F1}s water={water:F3} cause={(won?"none":s.FailureMessage)}");if(won){bool slept=s.Sleep();pass&=slept;lines.Add("BED "+day+": "+(slept?"PASS":"FAIL")+" next="+s.Day);}
   }
   Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllLines(path,lines);return new{passed=pass,lines};
  }
 }
}
#endif
