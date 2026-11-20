using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
 public sealed partial class WatchState {
  public LanternState Lanterns {get;private set;}
  LanternState NewLanterns(DayDefinition day){
   if(!Master||!Sprint5||!Sprint5.oilLanterns||day.scenario?.configured!=true)return null;
   // Reuse the existing replicated frame and host-owned stun timers. Helmet lamps
   // are always on and have no fuel or hand-held switch in either game mode.
   var lights=new LanternState(Sprint5.lanternHighSeconds,Sprint5.lanternLowSeconds,Sprint5.lanternPourSeconds,Sprint5.lanternStunSeconds,Sprint5.lanternStunCost,true,true);
   lights.Cycle(0);lights.Cycle(1);lights.ReadOnly=ReadOnly;return lights;
  }
  public bool CycleLantern(int slot)=>false;
  public bool RepelWithLantern(int slot,float dt,bool held,bool aim){if(ReadOnly||Failed||Lanterns==null||!Lanterns.Expose(slot,dt,held,aim,Drowned?.Phase==DrownedPhase.Manifest))return false;bool result=Drowned.Repel(Elapsed,Sprint5.lanternRetreatSeconds);if(result){TickTension();Storm?.Tick();Changed?.Invoke();}return result;}
 }
}
