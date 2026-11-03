using UnityEngine;

namespace LighthouseKeepers.Core
{
    [CreateAssetMenu(menuName="Lighthouse/S4 M2 power tuning")]
    public sealed class S4M2Tuning : ScriptableObject
    {
        public bool enabledForWatch=true;
        public bool powerGrid=true;
        [Min(0)] public float pumpLoad=1,beaconLoad=1,radioLoad=1,lightsLoad=.25f;
        public const string DayCatalogResource="WatchDaysS4M2";
        static S4M2Tuning current;
        public static S4M2Tuning Current
        {
            get
            {
#if UNITY_EDITOR
                if(System.Environment.GetEnvironmentVariable("LK_S4M2_DISABLED")=="1")return null;
#endif
                if(!current)current=Resources.Load<S4M2Tuning>("S4M2Tuning");
                return current&&current.enabledForWatch&&current.powerGrid?current:null;
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetCache()=>current=null;
    }
}
