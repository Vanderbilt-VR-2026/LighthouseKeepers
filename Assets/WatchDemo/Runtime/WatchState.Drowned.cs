using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchState
    {
        public DrownedState Drowned{get;private set;}
        public bool DrownedViewReady {get;set;} = true;
        DrownedState NewDrowned(SabotageState traces,DayDefinition day)=>traces!=null&&Sprint5&&Sprint5.drownedPresence?new DrownedState(traces,day.scenario.noFail,day.day==2?2:3,Sprint5.earliestScareSeconds,Sprint5.drownedIntervalSeconds,Sprint5.drownedVisibleSeconds){ReadOnly=ReadOnly}:null;
        // The reveal does no damage. First-control danger grace remains intact, but no
        // longer cancels the visual encounter before a player can notice the culprit.
        void TickDrowned(){Drowned?.Tick(Elapsed,!HasPower(LighthouseKeepers.Core.PowerCircuit.Lights)||!BeaconShining,Tension!=null&&Elapsed<Tension.ReliefUntil,Failed||Complete,WatchStartHeld,DrownedViewReady);}
        bool ValidDrowned(DayDefinition day,WatchSnapshot snapshot){var traces=NewSabotage(day);if(traces!=null){traces.ReadOnly=true;traces.Apply(snapshot.sabotage,snapshot.nightFaultMask,snapshot.elapsed);}var d=NewDrowned(traces,day);return d==null?!snapshot.drowned.enabled:d.ValidReplica(snapshot.drowned,snapshot.elapsed);}
    }
}
