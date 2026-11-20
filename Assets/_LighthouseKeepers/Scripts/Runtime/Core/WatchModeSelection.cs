using UnityEngine;
namespace LighthouseKeepers.Core
{
    public enum KeeperGameMode { Campaign=0, Survival=1 }
    // Local launch request; online hosts publish this in Photon session properties.
    public static class WatchModeSelection
    {
        public const string SessionKey="keeperMode";
        public static KeeperGameMode Selected {get;set;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()=>Selected=KeeperGameMode.Campaign;
    }
}
