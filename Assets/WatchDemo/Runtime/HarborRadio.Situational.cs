using System;
using UnityEngine;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo
{
 public sealed partial class HarborRadio
 {
  readonly HarborCueRules cueRules=new();Func<int,bool> queueSituational;
  int priorValve;bool engineWasRunning,wrongCodePending,broadcastReplyPending,stallPending;
  PoleSlide[] poles;Transform pressureValve;
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
   wrongCodePending=broadcastReplyPending=stallPending=false;poles=FindObjectsByType<PoleSlide>(FindObjectsSortMode.None);var valve=watch.GetComponentInChildren<WatchPressureValve>();pressureValve=valve?valve.transform:null;
  }
  bool QueueSituational(int id)
  {
   if(!(watch.State.Campaign&&id>=94?cueRules.AllowsCampaign(id):cueRules.Allows(id,Time.time))||pending.Contains(id)||pending.Count>=16)return false;
   AddPending(id);return true;
  }
  public static int TonightCue(WatchState s)
  {
   bool pipe=false,generator=false,beacon=false,radio=false;
   foreach(var task in s.Definition.tasks){pipe|=task.stationId==WatchState.PipeStation;generator|=task.stationId==WatchState.GeneratorStation;beacon|=task.stationId==WatchState.BeaconStation;radio|=task.stationId==WatchState.RadioStation;}
   return HarborCueRules.Tonight(pipe,generator,beacon,radio,s.Definition.tasks.Length);
  }
  bool KeepersSeparated()
  {if(watch.State.Coop is not ICoopKeeperLocations locations)return false;locations.Locations(out _,out var a,out int second,out var b);return second>0&&Mathf.Abs(a.y-b.y)>=6.6f;}
  bool AtUsefulPole(Vector3 p,KeeperStep step)
  {if(poles==null||ObjectiveDirector.Floor(step)>=ObjectiveHud.FloorAt(p.y)||step==KeeperStep.None)return false;foreach(var pole in poles)if(pole&&pole.CanEnterAt(p.y-1.6f)&&Vector2.Distance(new Vector2(p.x,p.z),new Vector2(pole.transform.position.x,pole.transform.position.z))<1.8f)return true;return false;}
  bool UsefulPoleNearby()
  {var s=watch.State;var a=head.position;var b=a;int actor=0,other=0;if(s.Coop is ICoopKeeperLocations locations)locations.Locations(out actor,out a,out other,out b);return AtUsefulPole(a,s.Director?.ForActor(actor)??KeeperStep.None)||other>0&&AtUsefulPole(b,s.Director?.ForActor(other)??KeeperStep.None);}
  void ObserveSituationalLines()
  {
   var s=watch.State;bool crew=s.Pressure?.CrewRequired==true;
   int holder=s.Pressure?.Holder??0;bool running=s.Engine?.Running??false;
   if(engineWasRunning&&!running)stallPending=true;
   var c=new HarborCueRules.Context{
    wrenchDown=s.Repaired&&watch.WrenchHeld,
    valvePuzzle=crew&&!s.Repaired&&AtStation(7),
    valveReleased=crew&&!s.Repaired&&(s.Pipe.BoltsDone>0||s.Pipe.TurnsDone>0||s.Pipe.StrokeDegrees>0)&&priorValve>0&&holder==0,
    soloClamp=s.Pressure!=null&&!crew&&!s.Pressure.Clamped&&!s.Repaired&&watch.WrenchHeld&&AtStation(7),
    crewRadios=KeepersSeparated(),
    pole=UsefulPoleNearby(),
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
