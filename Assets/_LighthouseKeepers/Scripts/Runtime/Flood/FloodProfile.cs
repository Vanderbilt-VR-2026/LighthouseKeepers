using UnityEngine;
namespace LighthouseKeepers.Flood {
[CreateAssetMenu(menuName="Lighthouse Keepers/Flood Profile")]
public sealed class FloodProfile:ScriptableObject {
 [SerializeField] float minimumHeight=-0.25f, maximumHeight=10.5f;
 [SerializeField,Min(0)] float riseMetresPerSecond=0.003f;
 [SerializeField,Min(0),Tooltip("Fractional rise acceleration per metre of water risen. 0.1 doubles the base rate over the full range.")] float surgePerMetre=0.1f;
 public float MinimumHeight=>minimumHeight; public float MaximumHeight=>maximumHeight; public float RiseSpeed=>riseMetresPerSecond; public float SurgePerMetre=>surgePerMetre;
}}