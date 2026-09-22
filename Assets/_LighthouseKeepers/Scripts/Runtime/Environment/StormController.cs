using System.Collections;
using UnityEngine;
using LighthouseKeepers.Progression;
using LighthouseKeepers.Audio;
namespace LighthouseKeepers.Environment {
public sealed class StormController:MonoBehaviour {
 [SerializeField,Range(0.1f,1)] float intensity=0.3f;
 [SerializeField,Min(1)] float lightningInterval=50;
 [SerializeField,Range(0.1f,2)] float windIntensity=.7f;
 [SerializeField] int randomSeed=417;
 [SerializeField] Light stormLight;
 [SerializeField] AudioSource thunder;
 [SerializeField] AudioClip[] thunderClips;
 System.Random random;float nextStrike, baseline;float proximity=0.2f;bool flashing;
 public float Intensity=>intensity;public float WindIntensity=>windIntensity;
 void Awake(){random=new System.Random(randomSeed);if(stormLight)baseline=stormLight.intensity;Schedule();}
 void Schedule(){nextStrike=Time.time+lightningInterval*Mathf.Lerp(0.65f,1.4f,(float)random.NextDouble());}
 void Update(){Shader.SetGlobalFloat("_LKStormWetness",intensity);if(Time.time>=nextStrike&&!flashing){StartCoroutine(Strike());Schedule();}}
 public void Apply(DayProfile profile){intensity=Mathf.Max(0.1f,profile.StormIntensity);windIntensity=.4f+profile.StormIntensity;lightningInterval=profile.LightningInterval;proximity=profile.ThunderProximity;if(random!=null)Schedule();}
 [ContextMenu("Preview lightning and thunder")] public void PreviewLightning(){if(Application.isPlaying&&!flashing)StartCoroutine(Strike());}
 IEnumerator Strike(){flashing=true;float distance=Mathf.Lerp(1600,100,proximity)*(0.5f+(float)random.NextDouble());
 if(stormLight)stormLight.intensity=baseline+1.8f;yield return new WaitForSeconds(0.07f);if(stormLight)stormLight.intensity=baseline;
 yield return new WaitForSeconds(0.12f);if(stormLight)stormLight.intensity=baseline+0.8f;yield return new WaitForSeconds(0.1f);if(stormLight)stormLight.intensity=baseline;
 yield return new WaitForSeconds(distance/343f);
 if(thunder&&thunderClips.Length>0){thunder.pitch=Mathf.Lerp(0.8f,1.1f,(float)random.NextDouble());thunder.PlayOneShot(thunderClips[random.Next(thunderClips.Length)],Mathf.Lerp(0.8f,0.25f,distance/1800));}flashing=false;}
}}
