using UnityEngine;
namespace LighthouseKeepers.Core
{
 [CreateAssetMenu(menuName="Lighthouse/Audio pass settings")]
 public sealed class AudioPassSettings:ScriptableObject
 {
  public bool adaptiveMusic=true,layeredRain=true,audioMix=true;
  [Min(.1f)]public float crossfadeSeconds=2;
  public float[] barSeconds={3.3436735f,1.6718367f,3.3436735f};
  [Range(0,1)]public float musicVolume=.23f,lobbyVolume=.025f,stingVolume=.45f;
  public Vector3 rainGains=new Vector3(.65f,.45f,.55f);
  public float zoneFadeSeconds=1.5f;
  public static AudioPassSettings Current=>Resources.Load<AudioPassSettings>("AudioPassSettings");
 }
}
