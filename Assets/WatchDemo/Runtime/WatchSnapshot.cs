using System;
namespace LighthouseKeepers.WatchDemo
{
    // Complete host-owned rules and presentation state; guest commands travel separately.
    [Serializable] public sealed class WatchSnapshot
    {
        public const string Protocol="LK-Lab-Coop-v2";
        public string protocol=Protocol,world;
        public int sequence,day,power,doneMask,starterPulls,bolts,activeBolt,turns,codeProgress;
        public double elapsed,beaconDarkElapsed;
        public double highWaterElapsed;
        public bool highWaterFailed;
        public int sleepDay;
        public float water,stroke,seal,frequency,lockElapsed,transmitElapsed;
        public bool beaconDarkFailed,needsReturn,radioMain,radioLocked,radioDone;
        public int revision,beds,keepers,order0,order1=1,order2=2;
        public float fade,seat,direction,returned;
        public bool sleeping,touching,radioHeld,radioTrigger;
        public string notice;
        public bool Valid() => protocol==Protocol && revision>=0 && beds>=0 && beds<=2 && keepers>=0 && keepers<=2 &&
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
