using UnityEngine;
using LighthouseKeepers.Progression;
namespace LighthouseKeepers.Environment {
public sealed class DayEnvironmentResponse:MonoBehaviour {
 [SerializeField] Light practical;
 [SerializeField] AudioSource machinery, radio;
 [SerializeField] ParticleSystem[] leaks;
 DayDirector days;float intensity=1.8f;
 void Start(){days=FindAnyObjectByType<DayDirector>();if(practical)intensity=practical.intensity;}
 void Update(){if(!days||!days.Current)return;var day=days.Current;
 if(practical){float pulse=Mathf.PerlinNoise(Time.time*.2f,transform.position.y);practical.intensity=intensity*(pulse<day.LightFailureFrequency*.5f?.35f:1);}
 if(machinery){machinery.pitch=1+Mathf.Sin(Time.time*.7f)*day.MachineryInstability*.04f;machinery.volume=.3f*day.AudioIntensity;}
 if(radio)radio.volume=.1f+day.RadioInterference*.25f;
 for(int i=0;i<leaks.Length;i++)if(leaks[i]){var emission=leaks[i].emission;emission.enabled=i<day.LeakCount;}
 }
}}
