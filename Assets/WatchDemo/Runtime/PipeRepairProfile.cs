using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    [CreateAssetMenu(menuName="Lighthouse/Pipe repair tuning")]
    public sealed class PipeRepairProfile:ScriptableObject
    {
        public int[] quarterTurns={2,3,1};
        [Min(30)] public float quarterTurnDegrees=90;
        [Min(15)] public float returnDegrees=65;
        [Min(10)] public float maximumDegreesPerSecond=720;
        [Min(0)] public float seatSeconds=.6f;
        [Min(.1f)] public float sealSeconds=3;
        [Range(0,1)] public float retainedPartialProgress=.65f;
        [Min(.03f)] public float jawContactRadius=.18f;
        [Min(0)] public float steadyDegreesPerSecond=12;
        [Min(0)] public float steadyMetresPerSecond=.06f;
        [Min(1)] public float sprayParticlesPerSecond=75;
        [Range(0,1)] public float tickAmplitude=.35f;
        [Min(.01f)] public float tickSeconds=.045f;
    }
}
