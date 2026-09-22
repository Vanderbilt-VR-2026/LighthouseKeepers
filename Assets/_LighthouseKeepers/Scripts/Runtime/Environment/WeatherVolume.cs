using UnityEngine;
namespace LighthouseKeepers.Environment {
[RequireComponent(typeof(ParticleSystem))]
public sealed class WeatherVolume:MonoBehaviour {
 [SerializeField] bool followOutsidePlayer;
 [SerializeField,Min(1)] float baselineEmission=65;
 ParticleSystem rain;StormController storm;Transform head;
 void Start(){rain=GetComponent<ParticleSystem>();storm=FindAnyObjectByType<StormController>();if(Camera.main)head=Camera.main.transform;}
 void LateUpdate(){var emission=rain.emission;emission.rateOverTime=baselineEmission*Mathf.Lerp(0.6f,1.5f,storm?storm.Intensity:0.3f);
 var wind=rain.velocityOverLifetime;wind.x=-1.8f*(storm?storm.WindIntensity:1);
 if(followOutsidePlayer&&head){var p=head.position;float radius=new Vector2(p.x,p.z).magnitude;bool outside=radius>5.1f&&p.y>9;emission.enabled=outside;transform.position=p+Vector3.up*3;}}
}}
