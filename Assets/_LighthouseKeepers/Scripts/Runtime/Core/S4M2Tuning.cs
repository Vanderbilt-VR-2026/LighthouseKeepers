using UnityEngine;

namespace LighthouseKeepers.Core
{
    [CreateAssetMenu(menuName="Lighthouse/S4 M2 power tuning")]
    public sealed class S4M2Tuning : ScriptableObject
    {
        public bool enabledForWatch=true;
        public bool powerGrid=true;
        public bool generatorSequence=true;
        public bool beaconStation=true;
        [Range(.001f,.2f)] public float alignmentTolerance=.025f;
        [Min(.01f)] public float sootRemovedPerMetre=.4f;
        [Range(.05f,1)] public float dirtyBeamBrightness=.25f;
        [Min(1)] public int primerSqueezes=3, starterPulls=3, floodAtPulls=5;
        [Min(.1f)] public float chokeOpenSeconds=5, engineFloodSeconds=8;
        [Range(0,1)] public float minimumFuel=.12f, lowFuelWarning=.2f;
        [Min(0)] public float fuelPourPerSecond=.06f;
        [Min(.1f)] public float primerCooldown=.35f, crankCooldown=.55f;
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
