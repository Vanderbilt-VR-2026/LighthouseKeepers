using System;
namespace LighthouseKeepers.Core.Progression
{
    // Host-only scheduler. It has no Unity clock: replay, retry and network snapshots use watch time.
    public sealed class NightSchedule
    {
        readonly NightScenario data;
        public int FiredMask {get;private set;}
        public bool SawPowerOff {get;private set;}
        public double ReliefUntil {get;private set;}
        public double NextFaultAt {get;private set;}
        public double GraceUntil {get;private set;}
        public bool Finished=>FiredMask==((1<<data.faults.Length)-1);
        public NightSchedule(NightScenario data){this.data=data;}
        public bool Fired(int i)=>(FiredMask&(1<<i))!=0;
        public bool Pending(string station)
        {
            bool scheduled=false;
            for(int i=0;i<data.faults.Length;i++)if(data.faults[i].stationId==station){scheduled=true;if(Fired(i))return false;}
            return scheduled;
        }
        public void Completed(double now)=>ReliefUntil=Math.Max(ReliefUntil,now+data.reliefSeconds);
        public void Advance(double now,bool powerRestored,Func<string,bool> done,Action<NightFault> fire)
        {
            if(!powerRestored)SawPowerOff=true;
            for(int i=0;i<data.faults.Length;i++)
            {
                if(Fired(i))continue;
                var f=data.faults[i];
                bool exhaustion=f.kind==NightFaultKind.FuelExhaustion;
                bool ready=exhaustion?!powerRestored:f.gate==NightEventGate.Clock?now>=f.atSeconds:
                    f.gate==NightEventGate.ClockOrJobComplete?now>=f.atSeconds||done(f.afterStationId):
                    f.gate==NightEventGate.JobComplete?done(f.afterStationId):SawPowerOff&&powerRestored;
                // Initial leak is the tutorial's immediate job; later faults respect the calm opening.
                bool initial=f.gate==NightEventGate.Clock&&f.atSeconds==0;
                bool finale=data.finaleFaultPileup&&f.repeat&&f.kind==NightFaultKind.StationFailure;
                if(f.kind==NightFaultKind.LowFuel&&!powerRestored)continue;
                if(!ready||!initial&&!exhaustion&&(now<data.openingSeconds||now<ReliefUntil||!finale&&now<NextFaultAt))continue;
                FiredMask|=1<<i;GraceUntil=Math.Max(GraceUntil,now+f.graceSeconds);
                if(!initial)NextFaultAt=now+data.faultSpacingSeconds;
                fire(f);
            }
        }
        public bool ValidReplica(int mask,bool sawOff,double relief,double next,double grace,double now)
        {
            if(mask<0||(mask>>data.faults.Length)!=0)return false;
            double maxGrace=data.firstStationGraceSeconds;
            foreach(var f in data.faults)maxGrace=Math.Max(maxGrace,f.graceSeconds);
            return Finite(relief,now+data.reliefSeconds)&&Finite(next,now+data.faultSpacingSeconds)&&Finite(grace,now+maxGrace);
        }
        static bool Finite(double value,double max)=>!double.IsNaN(value)&&!double.IsInfinity(value)&&value>=0&&value<=max+.01;
        public void Apply(int mask,bool sawOff,double relief,double next,double grace)
        {FiredMask=mask;SawPowerOff=sawOff;ReliefUntil=relief;NextFaultAt=next;GraceUntil=grace;}
    }
}
