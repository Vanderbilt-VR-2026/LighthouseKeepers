using UnityEngine;
namespace LighthouseKeepers.Core.Progression
{
    [CreateAssetMenu(menuName="Lighthouse/Sprint 5 pacing and fear")]
    public sealed class Sprint5Tuning : ScriptableObject
    {
        public bool tensionDirector = true;
        [Range(15,60)] public float minimumProblemGapSeconds = 15;
        [Range(2,3)] public float preScareQuietSeconds = 2.5f;
        [Min(60)] public float earliestScareSeconds = 60;
        [Range(.01f,.15f)] public float blackoutFogDensity = .06f;
        public static Sprint5Tuning Current => Resources.Load<Sprint5Tuning>("Sprint5Tuning");
    }
}
