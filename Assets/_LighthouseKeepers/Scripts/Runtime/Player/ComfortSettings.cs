using UnityEngine;
namespace LighthouseKeepers.Player {
[CreateAssetMenu(menuName="Lighthouse Keepers/Comfort Settings")]
public sealed class ComfortSettings:ScriptableObject {
 [SerializeField,Range(0.5f,3)] float movementSpeed=1.6f;
 [SerializeField] bool smoothTurning;
 [SerializeField,Range(15,90)] float snapAngle=45, smoothTurnSpeed=45;
 public float MovementSpeed=>movementSpeed;public bool SmoothTurning=>smoothTurning;// Vignette is intentionally not a configurable option.
 public bool VignetteEnabled=>false;
 public float SnapAngle=>snapAngle;public float SmoothTurnSpeed=>smoothTurnSpeed;
}}
