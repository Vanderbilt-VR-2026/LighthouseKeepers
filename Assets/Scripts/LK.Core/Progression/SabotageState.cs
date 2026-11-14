using System;
namespace LighthouseKeepers.Core.Progression
{
    [Serializable] public sealed class SabotageFrame
    {
        public bool enabled;
        public double[] warning=Array.Empty<double>(),broken=Array.Empty<double>(),fixedAt=Array.Empty<double>();
        public SabotageFrame Copy()=>new(){enabled=enabled,warning=(double[])warning.Clone(),broken=(double[])broken.Clone(),fixedAt=(double[])fixedAt.Clone()};
        public bool Valid(double now)
        {
            if(!double.IsFinite(now)||now<0||warning==null||broken==null||fixedAt==null||warning.Length>30||warning.Length!=broken.Length||warning.Length!=fixedAt.Length)return false;
            if(!enabled&&warning.Length!=0)return false;
            for(int i=0;i<warning.Length;i++){
                if(!Time(warning[i],now)||!Time(broken[i],now)||!Time(fixedAt[i],now))return false;
                if(broken[i]<0&&fixedAt[i]>=0||fixedAt[i]>=0&&fixedAt[i]<broken[i]||warning[i]>=0&&broken[i]>=0&&broken[i]<warning[i])return false;
            }
            return true;
        }
        static bool Time(double value,double now)=>double.IsFinite(value)&&(value==-1||value>=0&&value<=now+.001);
    }
    // Receipts from the existing night scheduler, not a second source of faults.
    // Both clients use watch time for warnings and repair-evidence fade.
    public sealed class SabotageState
    {
        readonly NightScenario data;readonly double lead,linger;
        SabotageFrame frame;
        public bool ReadOnly{get;set;}
        public int Count=>frame.warning.Length;
        public double WarningSeconds=>lead;
        public double Warning(int i)=>frame.warning[i];
        public double Broken(int i)=>frame.broken[i];
        public double FixedAt(int i)=>frame.fixedAt[i];
        public SabotageState(NightScenario data,double lead=3,double linger=20)
        {
            this.data=data;this.lead=Math.Clamp(lead,2,5);this.linger=Math.Max(0,linger);
            frame=new(){enabled=true,warning=new double[data.faults.Length],broken=new double[data.faults.Length],fixedAt=new double[data.faults.Length]};
            Array.Fill(frame.warning,-1);Array.Fill(frame.broken,-1);Array.Fill(frame.fixedAt,-1);
        }
        public void Warn(int i,double now){if(!ReadOnly&&frame.broken[i]<0&&frame.warning[i]<0)frame.warning[i]=now;}
        public bool BeforeFault(int i,double now,bool alreadyBreaking)
        {
            if(ReadOnly)return false;
            // Arrival leak predates the keeper. Physical tank exhaustion must never be delayed.
            if(alreadyBreaking)return true;
            Warn(i,now);return now-frame.warning[i]>=lead-.000001;
        }
        public void Broke(int i,double now){if(!ReadOnly&&frame.broken[i]<0)frame.broken[i]=now;}
        public void Completed(string station,double now)
        {
            if(ReadOnly)return;
            for(int i=0;i<Count;i++){
                if(frame.broken[i]>=0&&frame.fixedAt[i]<0&&data.faults[i].stationId==station)frame.fixedAt[i]=now;
                // A new relief period cancels a warning that has not become a fault.
                // It is re-armed after the scheduler's breather, with a fresh full warning.
                if(frame.broken[i]<0)frame.warning[i]=-1;
            }
        }
        public float Evidence(int i,double now)=>frame.broken[i]<0?0:frame.fixedAt[i]<0?1:(float)Math.Clamp(1-(now-frame.fixedAt[i]-linger)/2,0,1);
        public SabotageFrame Capture()=>frame.Copy();
        public bool ValidReplica(SabotageFrame source,int fired,double now)
        {
            if(source==null||!source.enabled||!source.Valid(now)||source.warning.Length!=data.faults.Length)return false;
            for(int i=0;i<Count;i++)if((source.broken[i]>=0)!=((fired&(1<<i))!=0))return false;
            return true;
        }
        public bool Apply(SabotageFrame source,int fired,double now){if(!ReadOnly||!ValidReplica(source,fired,now))return false;frame=source.Copy();return true;}
    }
}
