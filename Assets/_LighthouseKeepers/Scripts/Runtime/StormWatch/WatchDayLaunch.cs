using UnityEngine;
namespace LighthouseKeepers.StormWatch
{
    // One-shot development launch request; compiled out of release, defaults to Day 1.
    public static class WatchDayLaunch
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static int RequestedDay { get; set; } = 1;
#endif
        public static int Consume()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            int day = RequestedDay; RequestedDay = 1; return day;
#else
            return 1;
#endif
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            RequestedDay = 1;
#endif
        }
    }
}
