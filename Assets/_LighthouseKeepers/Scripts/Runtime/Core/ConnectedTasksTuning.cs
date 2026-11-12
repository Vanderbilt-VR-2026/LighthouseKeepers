using UnityEngine;
namespace LighthouseKeepers.Core
{
    [CreateAssetMenu(menuName="Lighthouse Keepers/Connected tasks tuning")]
    public sealed class ConnectedTasksTuning : ScriptableObject
    {
        public bool enabledForWatch=true, dependencies=true, pressureValve=true, proximityVoice=true, handheldRadios=true;
        [Range(.1f,1)] public float valveLeakFraction=.65f;
        [Range(.2f,1)] public float holdTimeoutSeconds=.55f;
        public float conduitTravelSeconds=2.4f;
        public float voiceNearMetres=3, voiceFarMetres=18, floorHeight=3.3f;
        [Range(0,1)] public float wallVoiceGain=.55f, distantFloorGain=.07f;
        public float wallCutoffHz=1800, radioLowHz=350, radioHighHz=3500, radioHeadDistance=.38f;
        public static ConnectedTasksTuning Current=>System.Environment.GetEnvironmentVariable("LK_CONNECTED_DISABLED")=="1"?null:Resources.Load<ConnectedTasksTuning>("ConnectedTasksTuning");
    }
}
