using UnityEngine;

namespace LighthouseKeepers.StormWatch
{
    // A one-shot launch request. Returning to the lobby never changes the next crew's rules.
    public static class SoloWatchSession
    {
        static bool requested;
        public static void Request() => requested = true;
        public static void Cancel() => requested = false;
        public static bool ConsumeRequest()
        {
            bool value = requested;
            requested = false;
            return value;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => requested = false;
    }
}
