using System;
namespace LighthouseKeepers.WatchDemo
{
    // Complete host-owned rules and presentation state; guest commands travel separately.
    [Serializable] public sealed class WatchSnapshot
    {
        public const string Protocol=LighthouseKeepers.Core.CrewNetworkVersion.Snapshot;
        public string protocol=Protocol,world;
        public bool valveEnabled,valveCrew,valveClamped;public int valveActor;
        public int sequence,day,power,doneMask,starterPulls,bolts,activeBolt,turns,codeProgress;
        public double elapsed,beaconDarkElapsed;
        public double highWaterElapsed;
        public bool highWaterFailed;
        public int sleepDay,radioCue,radioSerial,bedMask;
        public float radioAge;
        public float houseWaterVolume;
        public int day3Stage; public bool day3Crew; public double day3GraceUntil;
        public int objectiveIndex;
        public bool nightTwoOutage;public double firstGeneratorGraceUntil;
        public int stormStage=1;
        public float peakDanger;public bool previousCloseCall;
        public bool directorEnabled;public int stepA,stepB,actorA,actorB;
        public int musicMode;public double musicSince;
        public bool beaconStationEnabled,beaconIgnited;
        public float beaconAlignment,beaconTarget,beaconSoot;
        public bool engineEnabled,chokeClosed,engineSettled;
        public float fuel,chokeRemaining,engineFloodRemaining;
        public int primer,enginePulls,engineClosedPulls,coughSerial;
        public bool gridEnabled,gridTripped;
        public float gridCapacity;
        public int gridMask;
        public bool CircuitPowered(int circuit)=>power==1&&(!gridEnabled||!gridTripped&&(gridMask&(1<<circuit))!=0);
        public float water,stroke,seal,frequency,lockElapsed,transmitElapsed;
        public bool beaconDarkFailed,needsReturn,radioMain,radioLocked,radioDone;
        public int revision,beds,keepers,order0,order1=1,order2=2;
        public float fade,seat,direction,returned;
        public bool sleeping,touching,radioHeld,radioTrigger;
        public string notice;
        static bool Unit(float value)=>float.IsFinite(value)&&value>=0&&value<=1;
        public bool Valid() => LighthouseKeepers.Core.PressureValveState.Valid(valveCrew,valveClamped,valveActor)&& (valveEnabled||!valveCrew&&!valveClamped&&valveActor==0)&& double.IsFinite(firstGeneratorGraceUntil)&&firstGeneratorGraceUntil>=0&& stormStage>=1&&stormStage<=4&& Unit(peakDanger)&& ObjectiveDirector.Valid(stepA)&&ObjectiveDirector.Valid(stepB)&&actorA>=0&&actorB>=0&&(actorB==0||actorB!=actorA)&& float.IsFinite(houseWaterVolume)&&houseWaterVolume>=0&&houseWaterVolume<=500&& day3Stage>=0&&day3Stage<=3&&double.IsFinite(day3GraceUntil)&&day3GraceUntil>=0&& objectiveIndex>=-2&&objectiveIndex<=30&& musicMode>=0&&musicMode<=2&&double.IsFinite(musicSince)&&musicSince>=0&&musicSince<=elapsed+.1&& radioCue>=0&&radioCue<=RadioLineCatalog.LastLine&&radioSerial>=0&&float.IsFinite(radioAge)&&radioAge>=0&&bedMask>=0&&bedMask<=3&& (!beaconStationEnabled||Unit(beaconAlignment)&&Unit(beaconTarget)&&Unit(beaconSoot))&& (!engineEnabled||float.IsFinite(fuel)&&fuel>=0&&fuel<=1&&primer>=0&&primer<=20&&enginePulls>=0&&enginePulls<=20&&engineClosedPulls>=0&&engineClosedPulls<=20&&float.IsFinite(chokeRemaining)&&chokeRemaining>=0&&chokeRemaining<=60&&float.IsFinite(engineFloodRemaining)&&engineFloodRemaining>=0&&engineFloodRemaining<=60&&coughSerial>=0)&& (!gridEnabled||LighthouseKeepers.Core.PowerGrid.ValidState(gridCapacity,gridMask)) && protocol==Protocol && revision>=0 && beds>=0 && beds<=2 && keepers>=0 && keepers<=2 &&
            (notice==null||notice.Length<=160) && float.IsFinite(fade)&&fade>=0&&fade<=1 &&
            order0>=0&&order0<=2&&order1>=0&&order1<=2&&order2>=0&&order2<=2&&order0!=order1&&order0!=order2&&order1!=order2 &&
            float.IsFinite(seat)&&seat>=0&&float.IsFinite(direction)&&Math.Abs(direction)<=1&&float.IsFinite(returned)&&returned>=0 && day>0 && power>=0 && power<=1 && doneMask>=0 &&
            double.IsFinite(elapsed)&&elapsed>=0 && double.IsFinite(beaconDarkElapsed)&&beaconDarkElapsed>=0 &&
            double.IsFinite(highWaterElapsed)&&highWaterElapsed>=0&&highWaterElapsed<=60&&sleepDay>=0 &&
            float.IsFinite(water)&&water>=-10&&water<=30 && bolts>=0&&bolts<=3 &&
            activeBolt>=-1&&activeBolt<=2 && (bolts==3?activeBolt==-1:activeBolt>=0) &&
            turns>=0&&turns<=20 && starterPulls>=0&&starterPulls<=3 &&
            float.IsFinite(stroke)&&stroke>=0&&stroke<=360 && float.IsFinite(seal)&&seal>=0&&seal<=60 &&
            float.IsFinite(frequency)&&frequency>=0&&frequency<1000 && float.IsFinite(lockElapsed)&&lockElapsed>=0 &&
            float.IsFinite(transmitElapsed)&&transmitElapsed>=0 && codeProgress>=0&&codeProgress<=4;
    }
}
