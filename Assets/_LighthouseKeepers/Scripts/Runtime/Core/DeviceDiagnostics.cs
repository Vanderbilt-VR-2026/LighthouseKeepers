using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
namespace LighthouseKeepers.Core {
/// <summary>Small development-build telemetry; records evidence without treating compositor rate as measured app rate.</summary>
public sealed class DeviceDiagnostics:MonoBehaviour {
 float windowStart;int frames;
 void Update(){if(!Debug.isDebugBuild)return;frames++;float now=Time.realtimeSinceStartup;if(now-windowStart<5)return;
 bool head=false,left=false,right=false;InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(CommonUsages.isTracked,out head);InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.isTracked,out left);InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.isTracked,out right);
 var move=FindAnyObjectByType<ContinuousMoveProvider>();var turn=FindAnyObjectByType<SnapTurnProvider>();
 Vector2 rawLeft=Vector2.zero,rawRight=Vector2.zero;InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primary2DAxis,out rawLeft);InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxis,out rawRight);
 Debug.Log($"LK_INPUT raw={rawLeft}/{rawRight} moveEnabled={move&&move.isActiveAndEnabled} actionEnabled={move?.leftHandMoveInput.inputActionReference?.action.enabled} read={move?.leftHandMoveInput.ReadValue()} turnEnabled={turn&&turn.isActiveAndEnabled} turnAction={turn?.rightHandTurnInput.inputActionReference?.action.enabled}");
 Debug.Log($"LK_METRICS appFPS={frames/(now-windowStart):F1} focus={Application.isFocused} tracked H/L/R={head}/{left}/{right} headPosition={(Camera.main?Camera.main.transform.position.ToString():"none")}");windowStart=now;frames=0;
 }
}}
