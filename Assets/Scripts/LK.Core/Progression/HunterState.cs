using System;
namespace LighthouseKeepers.Core.Progression
{
    public enum HunterPhase{Dormant,Sabotage,Stalk,Telegraph,Quiet,Chase,Search,Retreat}
    // Live scene adapter supplies actual LOS, light and reachable escape evidence.
    public readonly struct HunterObservation
    {
        public readonly int Actor;
        public readonly bool Valid, CanSeeTarget, PathComplete, EscapeReachable, Ended, ProtectedStep;
        public readonly KeeperExposure Exposure;
        public readonly float Distance;
        public HunterObservation(int actor,bool valid,bool visible,bool path,bool escape,KeeperExposure exposure,float distance,bool ended=false,bool protectedStep=false)
        {Actor=actor;Valid=valid;CanSeeTarget=visible;PathComplete=path;EscapeReachable=escape;Exposure=exposure;Distance=distance;Ended=ended;ProtectedStep=protectedStep;}
        public bool Eligible=>Valid&&Actor>=0&&PathComplete&&EscapeReachable&&Exposure.CanBeTaken&&!Ended&&!ProtectedStep&&float.IsFinite(Distance)&&Distance>=0;
    }
    [Serializable] public sealed class HunterFrame
    {
        public bool enabled, warned;
        public int phase,actor=-1,hunts;
        public double since,until;
        public float x,y,z,yaw;
        public HunterFrame Copy()=>(HunterFrame)MemberwiseClone();
        public bool Valid(double now,int maximum)=>enabled&&phase>=0&&phase<8&&actor>=-1&&hunts>=0&&hunts<=maximum&&double.IsFinite(since)&&since>=0&&since<=now&&double.IsFinite(until)&&until>=0&&until<=now+120&&float.IsFinite(x)&&Math.Abs(x)<=40&&float.IsFinite(z)&&Math.Abs(z)<=40&&float.IsFinite(y)&&y>=-2&&y<=18&&float.IsFinite(yaw)&&Math.Abs(yaw)<=360;
    }
    // One authoritative FSM. It consumes a sabotage receipt; it never schedules a second fault.
    // The take is a request, not a teleport or damage mutation. Recovery owns that transition.
    public sealed class HunterState
    {
        readonly HorrorTuning tuning;readonly int night,maximum;
        HunterFrame frame=new(){enabled=true};double lastTick=-1;
        public bool ReadOnly{get;set;}
        public HunterPhase Phase=>(HunterPhase)frame.phase;
        public int Target=>frame.actor;
        public int Hunts=>frame.hunts;
        public double Since=>frame.since;
        public bool CatchRequested{get;private set;}
        public float Speed=>Phase==HunterPhase.Chase?tuning.chaseSpeed:Phase==HunterPhase.Stalk||Phase==HunterPhase.Search?tuning.walkSpeed:0;
        public HunterState(HorrorTuning tuning,int night){this.tuning=tuning??throw new ArgumentNullException(nameof(tuning));this.night=night;maximum=tuning.Night(night)?.hunts??0;}
        void Enter(HunterPhase phase,double now,double duration=0){frame.phase=(int)phase;frame.since=now;frame.until=now+duration;}
        public bool BeginSabotage(double now,HunterObservation keeper,bool fullScares)
        {
            if(ReadOnly||!tuning.hunterEnabled||!fullScares||night<3||maximum<=frame.hunts||!double.IsFinite(now)||now<tuning.earliestScare||Phase!=HunterPhase.Dormant||!keeper.Eligible)return false;
            frame.actor=keeper.Actor;frame.warned=false;Enter(HunterPhase.Sabotage,now,tuning.tellMin);return true;
        }
        public void Tick(double now,HunterObservation keeper)
        {
            if(ReadOnly||!double.IsFinite(now)||now<lastTick||now<frame.since)return;lastTick=now;CatchRequested=false;
            if(Phase==HunterPhase.Dormant)return;
            if(Phase==HunterPhase.Retreat){if(now>=frame.until){frame.actor=-1;Enter(HunterPhase.Dormant,now);}return;}
            // Unknown/stale observations fail safe. A restored light, witness or protected step cancels pursuit.
            if(keeper.Valid&&keeper.Exposure.HiddenUnseen){if(Phase!=HunterPhase.Search)Enter(HunterPhase.Search,now,tuning.searchSeconds);else if(now>=frame.until)Repel(now,tuning.retreatSeconds);return;}
            if(!keeper.Valid||keeper.Actor!=frame.actor||keeper.Ended||keeper.ProtectedStep||!keeper.Exposure.CanBeTaken||!keeper.PathComplete||!keeper.EscapeReachable){Repel(now,tuning.retreatSeconds);return;}
            switch(Phase)
            {
                case HunterPhase.Sabotage:if(now>=frame.until)Enter(HunterPhase.Stalk,now,tuning.stalkSeconds);break;
                case HunterPhase.Stalk:
                    if(keeper.CanSeeTarget){Warn(now);}
                    else if(now>=frame.until)Enter(HunterPhase.Search,now,tuning.searchSeconds);
                    break;
                case HunterPhase.Telegraph:if(now>=frame.until)Enter(HunterPhase.Quiet,now,tuning.quietSeconds);break;
                case HunterPhase.Quiet:if(now>=frame.until)Enter(keeper.CanSeeTarget?HunterPhase.Chase:HunterPhase.Search,now,keeper.CanSeeTarget?tuning.maxChaseSeconds:tuning.searchSeconds);break;
                case HunterPhase.Chase:
                    if(!keeper.CanSeeTarget||now>=frame.until){Enter(HunterPhase.Search,now,tuning.searchSeconds);break;}
                    if(tuning.catchesEnabled&&keeper.Distance<=tuning.catchRange){CatchRequested=true;Repel(now,tuning.retreatSeconds);}
                    break;
                case HunterPhase.Search:
                    // Reacquisition also earns the full warning/silence; never instantly lunge from search.
                    if(keeper.CanSeeTarget)Warn(now);
                    else if(now>=frame.until)Repel(now,tuning.retreatSeconds);
                    break;
            }
        }
        void Warn(double now){if(!frame.warned){frame.hunts++;frame.warned=true;}Enter(HunterPhase.Telegraph,now,tuning.telegraphSeconds);}
        public bool Repel(double now,double duration)
        {
            if(ReadOnly||!double.IsFinite(now)||now<frame.since||!double.IsFinite(duration)||duration<=0||Phase==HunterPhase.Dormant)return false;
            Enter(HunterPhase.Retreat,now,Math.Min(120,duration));return true;
        }
        public bool SetPose(float x,float y,float z,float yaw,double now)
        {
            if(ReadOnly)return false;var next=frame.Copy();next.x=x;next.y=y;next.z=z;next.yaw=yaw;
            if(!next.Valid(now,maximum))return false;frame=next;return true;
        }
        public HunterFrame Capture()=>frame.Copy();
        public bool Apply(HunterFrame source,double now){if(!ReadOnly||source==null||!source.Valid(now,maximum))return false;frame=source.Copy();return true;}
    }
}
