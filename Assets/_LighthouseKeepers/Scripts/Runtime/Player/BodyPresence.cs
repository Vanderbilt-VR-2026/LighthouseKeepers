using UnityEngine;
namespace LighthouseKeepers.Player {
public sealed class BodyPresence:MonoBehaviour {
 [SerializeField] Transform head;
 [SerializeField] float yawSmoothing=3;
 void LateUpdate(){if(!head)return;transform.position=head.position-Vector3.up*0.55f-head.forward*0.08f;var forward=Vector3.ProjectOnPlane(head.forward,Vector3.up);if(forward.sqrMagnitude>0.2f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(forward),1-Mathf.Exp(-yawSmoothing*Time.deltaTime));}
}}
