using UnityEngine;
namespace LighthouseKeepers.Environment {
[RequireComponent(typeof(ParticleSystem))]
public sealed class WeatherVolume:MonoBehaviour {
 [SerializeField] bool followOutsidePlayer;
 [SerializeField,Min(1)] float baselineEmission=65;
 [SerializeField,Tooltip("Exterior precipitation only; never enable on authored plumbing leaks.")] bool excludeShelteredParticles;
 [SerializeField,Tooltip("A fixed circular emitter covers the entire balcony rather than following a doorway.")] bool balconyRing;
 ParticleSystem rain;StormController storm;Transform head;
 ParticleSystem.Particle[] particles;
 void Start(){rain=GetComponent<ParticleSystem>();storm=FindAnyObjectByType<StormController>();if(Camera.main)head=Camera.main.transform;}
 void LateUpdate(){var emission=rain.emission;emission.rateOverTime=baselineEmission*Mathf.Lerp(0.6f,1.5f,storm?storm.Intensity:0.3f);
 var wind=rain.velocityOverLifetime;wind.x=(balconyRing?-.25f:-1.8f)*(storm?storm.WindIntensity:1);
 if(followOutsidePlayer&&head){var p=head.position;bool outside=IsOnExposedBalcony(p);emission.enabled=outside;if(!balconyRing)transform.position=p+Vector3.up*3;
 if(!outside&&rain.particleCount>0)rain.Clear();}
 if(excludeShelteredParticles)CullShelteredParticles();}
 public static bool IsOnExposedBalcony(Vector3 p)=>p.y>9.6f&&p.y<14&&new Vector2(p.x,p.z).magnitude>5.35f;
 public static bool IsSheltered(Vector3 p){
 // Bounds match the authored tower roof and keeper-house eaves, in world metres.
 return (p.y<13.2f&&new Vector2(p.x,p.z).sqrMagnitude<5.35f*5.35f)
 ||(p.x> -11.5f&&p.x< -4.8f&&Mathf.Abs(p.z)<2.9f&&p.y<4.6f);
 }
 void CullShelteredParticles(){
 if(particles==null||particles.Length<rain.main.maxParticles)particles=new ParticleSystem.Particle[rain.main.maxParticles];
 int count=rain.GetParticles(particles);for(int i=0;i<count;i++){
 var p=rain.main.simulationSpace==ParticleSystemSimulationSpace.World?particles[i].position:transform.TransformPoint(particles[i].position);
 if(IsSheltered(p)||(balconyRing&&p.y<9.62f))particles[i].remainingLifetime=0;
 }rain.SetParticles(particles,count);
 }
}}
