using UnityEngine;
namespace LighthouseKeepers.Core.Progression
{
    [CreateAssetMenu(menuName="Lighthouse/Sprint 5 pacing and fear")]
    public sealed class Sprint5Tuning : ScriptableObject
    {
        public bool tensionDirector = true;
        public bool fearFeedback = true;
        [Tooltip("Ship presentation is gated until host-owned passage outcomes are verified.")]
        public bool shipVisuals;
        public bool shipPassages;
        [Range(0,30)] public float shipApproachDangerSeconds = 15;
        [Range(15,60)] public float minimumProblemGapSeconds = 15;
        [Range(2,3)] public float preScareQuietSeconds = 2.5f;
        [Min(60)] public float earliestScareSeconds = 60;
        [Range(.01f,.2f)] public float blackoutFogDensity = .18f;
        public static Sprint5Tuning Current => Resources.Load<Sprint5Tuning>("Sprint5Tuning");
    }
}
