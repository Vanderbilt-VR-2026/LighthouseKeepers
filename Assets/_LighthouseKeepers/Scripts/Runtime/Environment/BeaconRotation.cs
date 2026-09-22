using UnityEngine;
namespace LighthouseKeepers.Environment {
public sealed class BeaconRotation:MonoBehaviour {
 [SerializeField] float degreesPerSecond=12;
 void Update()=>transform.Rotate(0,degreesPerSecond*Time.deltaTime,0,Space.Self);
}}
