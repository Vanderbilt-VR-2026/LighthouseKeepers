using UnityEngine;
namespace LighthouseKeepers.Environment
{
 [RequireComponent(typeof(ParticleSystem))]
 public sealed class CoastalFog : MonoBehaviour
 {
  [SerializeField, Range(.01f, 1)] float maximumEmission = .35f;
  ParticleSystem particles; StormController storm;
  void Start() { particles = GetComponent<ParticleSystem>(); storm = FindAnyObjectByType<StormController>(); }
  void Update()
  {
   var emission = particles.emission;
   emission.rateOverTime = maximumEmission * Mathf.Lerp(.35f, 1, storm ? storm.Intensity : .3f);
  }
 }
}
