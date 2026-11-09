using System;
using UnityEngine;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo
{
 public sealed partial class HarborRadio
 {
  readonly HarborCueRules cueRules=new();Func<int,bool> queueSituational;
  int priorValve;bool engineWasRunning,wrongCodePending,broadcastReplyPending,stallPending;
  float nightStarted;PoleSlide[] poles;
  void InitializeSituationalLines()
  {
   queueSituational=QueueSituational;
   watch.State.Radio.CodeRejected+=WrongCode;watch.State.Radio.BroadcastCompleted+=BroadcastReply;
  }
  void WrongCode(){if(!watch.State.ReadOnly)wrongCodePending=true;}
  void BroadcastReply(){if(!watch.State.ReadOnly)broadcastReplyPending=true;}
  void ResetSituationalLines()
  {
   cueRules.Reset();priorValve=watch.State.Pressure?.Holder??0;engineWasRunning=watch.State.Engine?.Running??false;
   wrongCodePending=broadcastReplyPending=stallPending=false;nightStarted=Time.time;poles=FindObjectsByType<PoleSlide>(FindObjectsSortMode.None);
  }
  bool QueueSituational(int id)
  {
   if(!cueRules.Allows(id,Time.time)||pending.Contains(id)||pending.Count>=16)return false;
   pending.Add(id);return true;
  }
  public static int TonightCue(WatchState s)
  {
   bool pipe=false,generator=false,beacon=false,radio=false;
   foreach(var task in s.Definition.tasks){pipe|=task.stationId==WatchState.PipeStation;generator|=task.stationId==WatchState.GeneratorStation;beacon|=task.stationId==WatchState.BeaconStation;radio|=task.stationId==WatchState.RadioStation;}
   return HarborCueRules.Tonight(pipe,generator,beacon,radio,s.Definition.tasks.Length);
  }
  static bool InHouse(Vector3 p)=>p.z< -4.3f&&p.z> -10.5f&&Mathf.Abs(p.x)<4.5f&&p.y<3.4f;
  bool AtPoleFloor(Vector3 p){if(poles==null)return false;foreach(var pole in poles)if(pole&&ObjectiveHud.FloorAt(p.y)==ObjectiveHud.FloorAt(pole.transform.position.y+1.6f))return true;return false;}
  void ObserveSituationalLines()
  {
   var s=watch.State;bool crew=s.Pressure?.CrewRequired==true;var a=head.position;var b=a;int second=0;
   if(s.Coop is ICoopKeeperLocations locations)locations.Locations(out _,out a,out second,out b);
   int holder=s.Pressure?.Holder??0;bool running=s.Engine?.Running??false;
   if(engineWasRunning&&!running)stallPending=true;
   var c=new HarborCueRules.Context{
    wrenchDown=s.Repaired&&watch.WrenchHeld,
    valvePuzzle=crew&&!s.Repaired,
    valveReleased=crew&&!s.Repaired&&priorValve>0&&holder==0,
    soloClamp=s.Pressure!=null&&!crew&&!s.Pressure.Clamped&&!s.Repaired,
    crewRadios=second>0&&((Time.time-nightStarted<45&&InHouse(a)&&InHouse(b))||Mathf.Abs(a.y-b.y)>=6.6f),
    pole=AtPoleFloor(a)||second>0&&AtPoleFloor(b),
    breakerTripped=s.Grid?.Tripped==true,
    lowFuel=s.Engine?.LowFuel==true&&!s.IsPending(WatchState.GeneratorStation),
    engineStalled=stallPending&&!running,
    lensSoot=s.Director!=null&&(s.Director.First==KeeperStep.BeaconWipe||s.Director.Second==KeeperStep.BeaconWipe),
    wrongCode=wrongCodePending,broadcast=broadcastReplyPending
   };
   cueRules.Observe(c,Time.time,queueSituational);
   priorValve=holder;engineWasRunning=running;wrongCodePending=false;
   // Keep the radio completion latch for the night to suppress its generic acknowledgement.
  }
 }
}
