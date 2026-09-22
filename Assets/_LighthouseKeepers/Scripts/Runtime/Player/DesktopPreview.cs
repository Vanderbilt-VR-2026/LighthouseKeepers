using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
namespace LighthouseKeepers.Player {
/// <summary>Development-only keyboard preview. Never supplies artificial headset poses on device.</summary>
public sealed class DesktopPreview:MonoBehaviour {
 [SerializeField] CharacterController character;
 [SerializeField] Transform head;
 float fall;
 void Start(){
 #if UNITY_EDITOR
 if(!XRSettings.isDeviceActive){var origin=GetComponent<Unity.XR.CoreUtils.XROrigin>();origin.CameraYOffset=0;origin.CameraFloorOffsetObject.transform.localPosition=Vector3.zero;head.localPosition=Vector3.up*1.65f;}
 #endif
 }
 void Update(){
 #if UNITY_EDITOR
 if(XRSettings.isDeviceActive||Keyboard.current==null)return;
 var k=Keyboard.current;var move=new Vector3((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),0,(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
 var direction=Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized*move.z+head.right*move.x;fall=character.isGrounded?-1:fall-9.81f*Time.deltaTime;
 character.Move((direction*1.6f+Vector3.up*fall)*Time.deltaTime);
 if(k.qKey.wasPressedThisFrame)transform.RotateAround(head.position,Vector3.up,-45);if(k.eKey.wasPressedThisFrame)transform.RotateAround(head.position,Vector3.up,45);
 #endif
 }
}}
