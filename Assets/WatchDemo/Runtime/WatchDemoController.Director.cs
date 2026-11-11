using UnityEngine;using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo {
 public sealed partial class WatchDemoController {
  int LocalKeeper=>(coop as ICoopKeeperLocations)?.LocalActor??0;
  public KeeperStep CurrentInstruction=>state?.Director?.ForActor(LocalKeeper)??KeeperStep.None;
  public KeeperStep PartnerInstruction=>state?.Director?.Partner(LocalKeeper)??KeeperStep.None;
  void InitializeDirector(){if(state.Director==null)return;state.Director.StationPosition=StationPosition;UpdateDirector();DirectorPresentation.Create(this,head);}
  Vector3 StationPosition(string id)=>id=="STN_PRESSURE_VALVE"&&pressureValve?pressureValve.transform.position:id==WatchState.PipeStation?pipeRepair.ActivePosition:id==WatchState.GeneratorStation?generator.transform.position+Vector3.up:id==WatchState.RadioStation?radio.transform.position+Vector3.up:beaconStation?beaconStation.transform.position+Vector3.up:head.position;
  void UpdateDirector(){if(state?.Director==null||state.ReadOnly)return;int a=0,b=0;var p=head.position;var q=p;if(coop is ICoopKeeperLocations locations)locations.Locations(out a,out p,out b,out q);if(a==0)p=head.position;state.Director.Tick(Time.time,a,p,b,q,WrenchHeld);}
  void DirectorBoard(){boardText.Clear();boardText.Append(KeeperStrings.Get("board.goal")).Append('\n').AppendFormat(KeeperStrings.Get("board.night"),state.Day).Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.Director.1"));foreach(var task in state.Definition.tasks)boardText.Append(state.IsDone(task.stationId)?"✓ ":"○ ").Append(KeeperStrings.Get("job."+task.stationId)).Append('\n');boardText.Append(state.Complete?"○ ":"  ").Append(KeeperStrings.Get("job.BED"));if(crewBeds>0&&!sleeping)boardText.Append(" (").Append(crewBeds).Append('/').Append(crewKeepers).Append(')');boardText.Append(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchDemoController.Director.1")).Append(KeeperStrings.Get(state.Campaign?"board.loop.five":"board.loop")).Append('\n').Append(KeeperStrings.Get("board.loss"));if(!string.IsNullOrEmpty(crewNotice))boardText.Append('\n').Append(crewNotice);board.text=boardText.ToString();}
 }
}