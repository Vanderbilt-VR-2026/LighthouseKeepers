using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
namespace LighthouseKeepers.Player {
/// <summary>Editor keyboard preview shares XRI's movement/collision pipeline; never supplies device poses.</summary>
public sealed class DesktopPreview:MonoBehaviour {
 [SerializeField] CharacterController character;
 [SerializeField] Transform head;
#if UNITY_EDITOR
 ContinuousMoveProvider move;SnapTurnProvider turn;
 XRInputValueReader<Vector2> originalMove,originalTurn;bool moving,turning;
 void Start(){
  if(!XRSettings.isDeviceActive){var origin=GetComponent<Unity.XR.CoreUtils.XROrigin>();origin.CameraYOffset=0;origin.CameraFloorOffsetObject.transform.localPosition=Vector3.zero;head.localPosition=Vector3.up*1.65f;}
  move=GetComponentInChildren<ContinuousMoveProvider>();turn=GetComponentInChildren<SnapTurnProvider>();
 }
 void Update(){
  if(XRSettings.isDeviceActive||Keyboard.current==null){Release();return;}
  var k=Keyboard.current;var value=new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
  // Direct CharacterController.Move competes with XRBodyTransformer's queued gravity pose.
  // Feed the existing provider instead, so keyboard and controller traversal use identical collision.
  if(move&&value!=Vector2.zero){if(!moving){originalMove=move.leftHandMoveInput;move.leftHandMoveInput=new XRInputValueReader<Vector2>("Desktop movement",XRInputValueReader.InputSourceMode.ManualValue);moving=true;}move.leftHandMoveInput.manualValue=Vector2.ClampMagnitude(value,1);}
  else if(moving){move.leftHandMoveInput=originalMove;moving=false;}
  float rotation=(k.eKey.isPressed?1:0)-(k.qKey.isPressed?1:0);
  if(turn&&rotation!=0){if(!turning){originalTurn=turn.rightHandTurnInput;turn.rightHandTurnInput=new XRInputValueReader<Vector2>("Desktop snap turn",XRInputValueReader.InputSourceMode.ManualValue);turning=true;}turn.rightHandTurnInput.manualValue=new Vector2(rotation,0);}
  else if(turning){turn.rightHandTurnInput=originalTurn;turning=false;}
 }
 void Release(){if(moving&&move)move.leftHandMoveInput=originalMove;if(turning&&turn)turn.rightHandTurnInput=originalTurn;moving=turning=false;}
 void OnDisable()=>Release();
#endif
}}
