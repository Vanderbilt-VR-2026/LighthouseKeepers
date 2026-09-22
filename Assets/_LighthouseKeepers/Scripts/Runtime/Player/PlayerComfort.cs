using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
namespace LighthouseKeepers.Player {
public sealed class PlayerComfort:MonoBehaviour,ITunnelingVignetteProvider {
 [SerializeField] ComfortSettings settings;
 [SerializeField] TunnelingVignetteController vignette;
 [SerializeField] ContinuousMoveProvider move;
 [SerializeField] SnapTurnProvider snap;
 [SerializeField] ContinuousTurnProvider smooth;
 [SerializeField] ControllerInputActionManager[] inputManagers;
 readonly VignetteParameters parameters=new();Vector3 previous;float previousYaw,amount;
 public VignetteParameters vignetteParameters=>parameters;
 void Awake(){move.rightHandMoveInput=new XRInputValueReader<Vector2>("Unused right move",XRInputValueReader.InputSourceMode.Unused);snap.leftHandTurnInput=new XRInputValueReader<Vector2>("Unused left snap",XRInputValueReader.InputSourceMode.Unused);smooth.leftHandTurnInput=new XRInputValueReader<Vector2>("Unused left turn",XRInputValueReader.InputSourceMode.Unused);}
 void OnEnable(){previous=transform.position;previousYaw=transform.eulerAngles.y;}
 void Update(){if(!settings)return;
 move.moveSpeed=settings.MovementSpeed;snap.turnAmount=settings.SnapAngle;smooth.turnSpeed=settings.SmoothTurnSpeed;
 foreach(var manager in inputManagers){bool left=manager.gameObject.name.Contains("Left");if(manager.smoothMotionEnabled!=left)manager.smoothMotionEnabled=left;if(manager.smoothTurnEnabled!=settings.SmoothTurning)manager.smoothTurnEnabled=settings.SmoothTurning;}
 snap.enabled=!settings.SmoothTurning;smooth.enabled=settings.SmoothTurning;
 float speed=Vector3.ProjectOnPlane(transform.position-previous,Vector3.up).magnitude/Mathf.Max(Time.deltaTime,0.001f);
 float rotation=Mathf.Abs(Mathf.DeltaAngle(previousYaw,transform.eulerAngles.y))/Mathf.Max(Time.deltaTime,0.001f);
 previous=transform.position;previousYaw=transform.eulerAngles.y;
 float target=settings.VignetteEnabled?Mathf.Clamp01(Mathf.Max(speed/1.6f,settings.SmoothTurning?rotation/60:0))*settings.VignetteStrength:0;
 amount=Mathf.Lerp(amount,target,1-Mathf.Exp(-settings.FadeSpeed*Time.deltaTime));
 parameters.apertureSize=1-amount*0.55f;parameters.featheringEffect=0.25f;parameters.easeInTime=0.15f;parameters.easeOutTime=0.3f;
 if(vignette){if(amount>0.01f)vignette.BeginTunnelingVignette(this);else vignette.EndTunnelingVignette(this);}
 }
 void OnDisable(){if(vignette)vignette.EndTunnelingVignette(this);}
}}
