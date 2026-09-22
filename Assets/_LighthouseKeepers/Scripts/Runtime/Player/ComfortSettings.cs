using UnityEngine;
namespace LighthouseKeepers.Player {
[CreateAssetMenu(menuName="Lighthouse Keepers/Comfort Settings")]
public sealed class ComfortSettings:ScriptableObject {
 [SerializeField,Range(0.5f,3)] float movementSpeed=1.6f;
 [SerializeField] bool smoothTurning, vignetteEnabled=true;
 [SerializeField,Range(15,90)] float snapAngle=45, smoothTurnSpeed=45;
 [SerializeField,Range(0,1)] float vignetteStrength=0.65f;
 [SerializeField,Min(0.1f)] float fadeSpeed=5;
 public float MovementSpeed=>movementSpeed;public bool SmoothTurning=>smoothTurning;public bool VignetteEnabled=>vignetteEnabled;
 public float SnapAngle=>snapAngle;public float SmoothTurnSpeed=>smoothTurnSpeed;public float VignetteStrength=>vignetteStrength;public float FadeSpeed=>fadeSpeed;
}}
