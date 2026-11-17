namespace LighthouseKeepers.WatchDemo {
 public sealed partial class WatchState {
  // Bot does not count as a human for time budgets, networking roster or shared-bed votes.
  public const int CompanionActor=2147483646;
  public bool ScriptedPartner {get;private set;}
  public bool PairedCrew=>keeperCount>1||ScriptedPartner;
  public void SetScriptedPartner(bool present){
   if(ReadOnly)return;present=present&&keeperCount==1;
   if(ScriptedPartner==present)return;
   Pressure?.Hold(CompanionActor,false);ScriptedPartner=present;
   Pressure?.SetCrew(PairedCrew&&RequiresCrewValve);Changed?.Invoke();
  }
 }
}
