using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
namespace LighthouseKeepers.Player {
public sealed class PlayerComfort:MonoBehaviour {
 [SerializeField] ComfortSettings settings;
 [SerializeField] ContinuousMoveProvider move;
 [SerializeField] SnapTurnProvider snap;
 [SerializeField] ContinuousTurnProvider smooth;
 [SerializeField] ControllerInputActionManager[] inputManagers;
 void Awake(){move.rightHandMoveInput=new XRInputValueReader<Vector2>("Unused right move",XRInputValueReader.InputSourceMode.Unused);snap.leftHandTurnInput=new XRInputValueReader<Vector2>("Unused left snap",XRInputValueReader.InputSourceMode.Unused);smooth.leftHandTurnInput=new XRInputValueReader<Vector2>("Unused left turn",XRInputValueReader.InputSourceMode.Unused);}
 void Update(){if(!settings)return;
 move.moveSpeed=settings.MovementSpeed;snap.turnAmount=settings.SnapAngle;smooth.turnSpeed=settings.SmoothTurnSpeed;
 foreach(var manager in inputManagers){bool left=manager.gameObject.name.Contains("Left");if(manager.smoothMotionEnabled!=left)manager.smoothMotionEnabled=left;if(manager.smoothTurnEnabled!=settings.SmoothTurning)manager.smoothTurnEnabled=settings.SmoothTurning;}
 snap.enabled=!settings.SmoothTurning;smooth.enabled=settings.SmoothTurning;

 }
}}
