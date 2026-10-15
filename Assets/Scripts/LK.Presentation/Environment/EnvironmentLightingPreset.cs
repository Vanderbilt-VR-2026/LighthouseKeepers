using UnityEngine;
namespace LighthouseKeepers.Presentation.Environment
{
    [CreateAssetMenu(menuName="Lighthouse/Environment lighting preset")]
    public sealed class EnvironmentLightingPreset : ScriptableObject
    {
        public Texture2D[] colorMaps, directionMaps, shadowMasks;
        public int[] rendererIndices;
        public Vector4[] rendererScaleOffsets;
        public LightmapsMode mode;
        public Color ambient;
        public float[] probeCoefficients;
    }
}
