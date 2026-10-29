using System;
namespace LighthouseKeepers.WatchDemo
{
    // Small, versioned read-only view; guests never send mission commands in co-op step 1.
    [Serializable] public sealed class WatchSnapshot
    {
        public const string Protocol="LK-Lab-Coop-Step1-v1";
        public string protocol=Protocol,world;
        public int sequence,day,power,doneMask,starterPulls,bolts,activeBolt,turns,codeProgress;
        public double elapsed,beaconDarkElapsed;
        public float water,stroke,seal,frequency,lockElapsed,transmitElapsed;
        public bool beaconDarkFailed,needsReturn,radioMain,radioLocked,radioDone;
        public bool Valid() => protocol==Protocol && day>0 && power>=0 && power<=1 && doneMask>=0 &&
            double.IsFinite(elapsed)&&elapsed>=0 && double.IsFinite(beaconDarkElapsed)&&beaconDarkElapsed>=0 &&
            float.IsFinite(water)&&water>=-10&&water<=30 && bolts>=0&&bolts<=3 &&
            activeBolt>=-1&&activeBolt<=2 && (bolts==3?activeBolt==-1:activeBolt>=0) &&
            turns>=0&&turns<=20 && starterPulls>=0&&starterPulls<=3 &&
            float.IsFinite(stroke)&&stroke>=0&&stroke<=360 && float.IsFinite(seal)&&seal>=0&&seal<=60 &&
            float.IsFinite(frequency)&&frequency>=0&&frequency<1000 && float.IsFinite(lockElapsed)&&lockElapsed>=0 &&
            float.IsFinite(transmitElapsed)&&transmitElapsed>=0 && codeProgress>=0&&codeProgress<=4;
    }
}
