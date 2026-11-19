using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
 public sealed partial class WatchState {
  public LanternState Lanterns {get;private set;}
  LanternState NewLanterns(DayDefinition day)=>Master&&Sprint5&&Sprint5.oilLanterns&&day.scenario?.configured==true?new LanternState(Sprint5.lanternHighSeconds,Sprint5.lanternLowSeconds,Sprint5.lanternPourSeconds,Sprint5.lanternStunSeconds,Sprint5.lanternStunCost,Sprint5.lanternUnlimitedOil,true){ReadOnly=ReadOnly}:null;
  public bool CycleLantern(int slot){if(ReadOnly||Failed||Lanterns==null||!Lanterns.Cycle(slot))return false;EngageWatch();Changed?.Invoke();return true;}
  public bool RepelWithLantern(int slot,float dt,bool held,bool aim){if(ReadOnly||Failed||Lanterns==null||!Lanterns.Expose(slot,dt,held,aim,Drowned?.Phase==DrownedPhase.Manifest))return false;bool result=Drowned.Repel(Elapsed,Sprint5.lanternRetreatSeconds);if(result){TickTension();Storm?.Tick();Changed?.Invoke();}return result;}
 }
}
