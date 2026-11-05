using System;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;

namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchState
    {
        // Pending jobs remain incomplete: they cannot award a tick or allow early sleep.
        int day3Stage; // 0 legacy, 1 pipe first, 2 outage, 3 upper-floor jobs available
        bool balanceCrew;
        double day3GraceUntil;
        public bool Day3Balance=>Tuning&&Tuning.day3Balance&&Day==3&&Engine!=null&&Grid!=null&&Beacon!=null;
        public int Day3Stage=>day3Stage;
        public bool Day3Crew=>balanceCrew;
        public float FloodRiseMultiplier=>Day3Balance?(balanceCrew?Tuning.day3CrewFloodMultiplier:Tuning.day3SoloFloodMultiplier):1;
        double Day3GraceSeconds=>balanceCrew?Tuning.day3CrewBeaconGraceSeconds:Tuning.day3SoloBeaconGraceSeconds;
        public double BeaconGraceRemaining=>Day3Balance?Math.Max(0,day3GraceUntil-Elapsed):0;
        bool RadioAvailable=>HasPower(PowerCircuit.Radio)&&!IsPending(RadioStation);
        public bool IsPending(string station)=>Day3Balance&&
            (station==GeneratorStation&&day3Stage==1 || (station==RadioStation||station==BeaconStation)&&day3Stage<3);
        void StartDay3Outage()
        {
            if(!Day3Balance||day3Stage!=1||ReadOnly)return;
            day3Stage=2;day3GraceUntil=Elapsed+Day3GraceSeconds;
            Engine.Reset(Math.Min(Engine.Fuel,Tuning.day3FailureFuel),Tuning.day3FuelDrainPerSecond,false);
            Changed?.Invoke();
        }
        void ActivateDay3UpperJobs()
        {
            day3Stage=3;
            Beacon.Reset(Definition.beaconInitiallyLit,Definition.beaconInitialAlignment,Definition.beaconTargetAlignment,Definition.beaconInitialSoot,Definition.beaconSootPerSecond);
            Radio.SetPower(RadioAvailable);
        }
        double SnapshotDuration(DayDefinition day,bool crew)=>Tuning&&Tuning.day3Balance&&day.day==3&&Engine!=null&&Grid!=null&&Beacon!=null
            ?(crew?Tuning.day3CrewSeconds:Tuning.day3SoloSeconds):day.dayLengthSeconds;
        bool ValidDay3Snapshot(DayDefinition day,WatchSnapshot s)
        {
            bool balanced=Tuning&&Tuning.day3Balance&&day.day==3&&Engine!=null&&Grid!=null&&Beacon!=null;
            if(!balanced)return s.day3Stage==0&&s.day3GraceUntil==0;
            if(s.day3Stage<1||s.day3Stage>3)return false;
            double grace=s.day3Crew?Tuning.day3CrewBeaconGraceSeconds:Tuning.day3SoloBeaconGraceSeconds;
            return s.day3GraceUntil<=s.elapsed+grace+.01&&(s.day3Stage!=1||s.day3GraceUntil==0);
        }
    }
}
