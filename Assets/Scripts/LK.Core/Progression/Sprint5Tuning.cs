using UnityEngine;
namespace LighthouseKeepers.Core.Progression
{
    [CreateAssetMenu(menuName="Lighthouse/Sprint 5 pacing and fear")]
    public sealed class Sprint5Tuning : ScriptableObject
    {
        public bool tensionDirector = true;
        public bool fearFeedback = true;
        [Tooltip("Host-owned warnings and evidence for scheduled sabotage.")]
        public bool sabotageTraces = true;
        public bool drownedPresence = true;
        public bool flareDefense = true;
        public bool oilLanterns = true;
        [Min(1)] public float lanternHighSeconds = 90, lanternLowSeconds = 180, lanternPourSeconds = 4;
        [Min(.1f)] public float lanternStunSeconds = 2, lanternRetreatSeconds = 20;
        [Range(.01f,1)] public float lanternStunCost = .15f;
        [Min(.5f)] public float lanternLowRange = .8f, lanternHighRange = 1.5f;
        public bool heavyRain = true;
        public bool armSwingRun = true;
        public bool groundedHorror = true;
        [Range(30,120)] public float drownedIntervalSeconds = 60;
        [Range(4,15)] public float drownedVisibleSeconds = 12;
        [Range(2,5)] public float sabotageWarningSeconds = 3;
        [Min(0)] public float sabotageEvidenceSeconds = 20;
        [Tooltip("Ship presentation is gated until host-owned passage outcomes are verified.")]
        public bool shipVisuals;
        public bool shipPassages;
        public bool shipWheelTracking = true;
        [Range(0,180)] public float shipCrewHoldSeconds = 120;
        [Range(30,75)] public float shipGuidanceLeadSeconds = 45;
        [Range(3,15)] public float shipAimToleranceDegrees = 8;
        [Range(0,8)] public float lanternLowIntensity=1.1f, lanternHighIntensity=3.6f;
        public float[] nightlyBeamTolerance={14,11,9,7.5f,6};
        public float BeamTolerance(int night)=>nightlyBeamTolerance!=null&&nightlyBeamTolerance.Length>0?Mathf.Clamp(nightlyBeamTolerance[Mathf.Clamp(night-1,0,nightlyBeamTolerance.Length-1)],3,18):shipAimToleranceDegrees;
        [Range(.01f,.08f)] public float exteriorFogCalm=.027f,exteriorFogStorm=.038f;
        [Range(.1f,2)] public float shipAimSteadySeconds = .6f;
        [Range(.3f,1)] public float shipMaximumSoot = .7f;
        [Range(0,30)] public float shipApproachDangerSeconds = 15;
        [Range(15,60)] public float minimumProblemGapSeconds = 15;
        [Range(2,3)] public float preScareQuietSeconds = 2.5f;
        [Min(60)] public float earliestScareSeconds = 60;
        [Range(.01f,.2f)] public float blackoutFogDensity = .18f;
        public static Sprint5Tuning Current => Resources.Load<Sprint5Tuning>("Sprint5Tuning");
    }
}
