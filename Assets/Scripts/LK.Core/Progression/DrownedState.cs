using System;
namespace LighthouseKeepers.Core.Progression
{
    public enum DrownedPhase { Dormant, Warning, Manifest, Retreat }
    [Serializable] public sealed class DrownedFrame
    {
        public bool enabled; public int phase, fault=-1, appearances, consumed;
        public double since, nextAt;
        public DrownedFrame Copy()=>(DrownedFrame)MemberwiseClone();
        public bool Valid(double now,int faults)=>phase>=0&&phase<=3&&fault>=-1&&fault<faults&&appearances>=0&&appearances<=3&&consumed>=0&&(consumed&~((1<<faults)-1))==0&&double.IsFinite(since)&&since>=0&&since<=now+.001&&double.IsFinite(nextAt)&&nextAt>=0&&nextAt<=now+120.01&&(phase==0||fault>=0)&&(enabled||phase==0&&fault==-1&&appearances==0&&consumed==0&&since==0&&nextAt==0);
    }
    // The culprit is tied to the SAME fault receipts, not another random event/failure scheduler.
    // This slice is a non-contact saboteur. Hunts remain gated until lantern/escape verification.
    public sealed class DrownedState
    {
        readonly SabotageState traces; readonly bool tutorial; readonly int maximum;
        readonly double first, interval, duration;
        DrownedFrame frame=new(){enabled=true};
        public bool ReadOnly{get;set;}
        public DrownedPhase Phase=>(DrownedPhase)frame.phase;
        public int Fault=>frame.fault;
        public double Since=>frame.since;
        public DrownedState(SabotageState traces,bool tutorial,int maximum=3,double first=60,double interval=60,double duration=12)
        {this.traces=traces;this.tutorial=tutorial;this.maximum=Math.Clamp(maximum,0,3);this.first=Math.Max(60,first);this.interval=Math.Clamp(interval,30,120);this.duration=Math.Clamp(duration,4,15);}
        void Move(DrownedPhase phase,double now){frame.phase=(int)phase;frame.since=now;}
        public void Tick(double now,bool dark,bool relief,bool ended,bool protectedStep=false)
        {
            if(ReadOnly)return;
            if(Phase==DrownedPhase.Retreat){if(now-frame.since>=2)Move(DrownedPhase.Dormant,now);return;}
            if(Phase!=DrownedPhase.Dormant){
                bool fixedJob=traces.FixedAt(frame.fault)>=0;
                if(protectedStep){Move(DrownedPhase.Dormant,now);return;}
                if(ended||relief||fixedJob||Phase==DrownedPhase.Manifest&&(!dark||now-frame.since>=duration)){
                    Move(DrownedPhase.Retreat,now);frame.nextAt=now+interval;return;
                }
                if(Phase==DrownedPhase.Warning){
                    if(traces.Warning(frame.fault)<0){Move(DrownedPhase.Dormant,now);return;}
                    if(traces.Broken(frame.fault)>=0&&now-traces.Broken(frame.fault)>=.5&&now-frame.since>=2.5){Move(DrownedPhase.Manifest,now);frame.consumed|=1<<frame.fault;frame.appearances++;}
                }
                return;
            }
            // Never replay an old event at late join/recovery. Initial pipe fault is only evidence.
            for(int i=0;i<traces.Count;i++){
                if((frame.consumed&(1<<i))!=0||traces.Warning(i)<0)continue;
                
                if(tutorial||ended||frame.appearances>=maximum||traces.FixedAt(i)>=0){frame.consumed|=1<<i;continue;}
                if(now<first||protectedStep||relief||now<frame.nextAt)continue;
                frame.fault=i;Move(DrownedPhase.Warning,now);break;
            }
        }
        public DrownedFrame Capture()=>frame.Copy();
        public bool ValidReplica(DrownedFrame source,double now)=>source!=null&&source.enabled&&source.Valid(now,traces.Count)&&source.appearances<=maximum&&(!tutorial||source.phase==0)&& (source.fault<0||traces.Warning(source.fault)>=0||source.phase==0||source.phase==3)&& (source.phase!=2||traces.Broken(source.fault)>=0&&(source.consumed&(1<<source.fault))!=0);
        public bool Apply(DrownedFrame source,double now){if(!ReadOnly||!ValidReplica(source,now))return false;frame=source.Copy();return true;}
    }
}
