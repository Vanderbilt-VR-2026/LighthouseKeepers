using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchState
    {
        public DrownedState Drowned{get;private set;}
        DrownedState NewDrowned(SabotageState traces,DayDefinition day)=>traces!=null&&Sprint5&&Sprint5.drownedPresence?new DrownedState(traces,day.scenario.noFail,day.day==2?2:3,Sprint5.earliestScareSeconds,Sprint5.drownedIntervalSeconds,Sprint5.drownedVisibleSeconds){ReadOnly=ReadOnly}:null;
        void TickDrowned(){Drowned?.Tick(Elapsed,!HasPower(LighthouseKeepers.Core.PowerCircuit.Lights)||!BeaconShining,Tension!=null&&Elapsed<Tension.ReliefUntil,Failed||Complete,WatchStartHeld||StationGraceRemaining>0||DrownedWaitingForGuidance);}
        bool DrownedWaitingForGuidance {
            get {
                if(!Master||!Master.firstStationGrace||Drowned==null||Drowned.Fault<0)return false;
                string job=Scenario.faults[Drowned.Fault].stationId;
                int bit=job==PipeStation?1:job==GeneratorStation?2:job==BeaconStation?4:job==RadioStation?8:0;
                return bit!=0&&!IsPending(job)&&!IsDone(job)&&(seenStationMask&bit)==0;
            }
        }
        bool ValidDrowned(DayDefinition day,WatchSnapshot snapshot){var traces=NewSabotage(day);if(traces!=null){traces.ReadOnly=true;traces.Apply(snapshot.sabotage,snapshot.nightFaultMask,snapshot.elapsed);}var d=NewDrowned(traces,day);return d==null?!snapshot.drowned.enabled:d.ValidReplica(snapshot.drowned,snapshot.elapsed);}
    }
}
