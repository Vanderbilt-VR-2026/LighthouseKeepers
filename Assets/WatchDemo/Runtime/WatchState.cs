using System;
namespace LighthouseKeepers.WatchDemo
{
    // Small local demonstration state; networking and the team's future director are not implied.
    public sealed class WatchState
    {
        public const double WatchSeconds=420, RepairSeconds=3.5;
        public int Day {get;private set;}=1;
        public double Elapsed {get;private set;}
        public bool Repaired {get;private set;}
        public bool Failed=>Elapsed>=WatchSeconds&&!Repaired;
        public void Tick(double seconds){if(seconds<0||double.IsNaN(seconds)||double.IsInfinity(seconds))throw new ArgumentOutOfRangeException(nameof(seconds));Elapsed=Math.Min(WatchSeconds,Elapsed+seconds);}
        public bool CompleteRepair(){if(Failed)return false;Repaired=true;return true;}
        public bool Sleep(){if(!Repaired)return false;Day++;Elapsed=0;Repaired=false;return true;}
        public void Retry(){Elapsed=0;Repaired=false;}
    }
}
