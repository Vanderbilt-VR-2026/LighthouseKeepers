using System.Linq;
using UnityEngine;
namespace LighthouseKeepers.Net
{
    /// <summary>Remote-only pose driver. No input readers, camera, physics hand or local XR Origin.</summary>
    public sealed class RemoteKeeperBody : MonoBehaviour
    {
        Transform leftShoulder,leftElbow,leftWrist,rightShoulder,rightElbow,rightWrist;
        public void Bind(GameObject body)
        {
            foreach(var behaviour in body.GetComponentsInChildren<MonoBehaviour>(true))behaviour.enabled=false;
            leftShoulder=Find(body,"Shoulder_L");leftElbow=Find(body,"Elbow_L");leftWrist=Find(body,"Wrist_L");
            rightShoulder=Find(body,"Shoulder_R");rightElbow=Find(body,"Elbow_R");rightWrist=Find(body,"Wrist_R");
            if(!leftShoulder || !leftElbow || !leftWrist || !rightShoulder || !rightElbow || !rightWrist)
                Debug.LogWarning("PF_PlayerBody requires Shoulder/Elbow/Wrist_L/R bones for remote arm IK.",body);
        }
        static Transform Find(GameObject body,string name)=>body.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==name);
        public void Apply(KeeperPoseFrame p)
        {
            transform.SetPositionAndRotation(p.Head-Vector3.up*.6f,Quaternion.Euler(0,p.TorsoYaw,0));
            Arm(leftShoulder,leftElbow,leftWrist,p.Left,p.LeftRotation,-1);
            Arm(rightShoulder,rightElbow,rightWrist,p.Right,p.RightRotation,1);
        }
        void Arm(Transform shoulder,Transform elbow,Transform wrist,Vector3 target,Quaternion rotation,float side)
        {
            if(!shoulder || !elbow || !wrist)return;
            float upper=Vector3.Distance(shoulder.position,elbow.position),lower=Vector3.Distance(elbow.position,wrist.position);
            Vector3 delta=target-shoulder.position;float reach=Mathf.Clamp(delta.magnitude,.01f,upper+lower-.001f);
            Vector3 direction=delta.normalized;
            Vector3 pole=transform.right*side-transform.up*.6f;
            Vector3 bend=Vector3.ProjectOnPlane(pole,direction).normalized;
            float along=(upper*upper-lower*lower+reach*reach)/(2*reach);
            Vector3 elbowGoal=shoulder.position+direction*along+bend*Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
            shoulder.rotation=Quaternion.FromToRotation(elbow.position-shoulder.position,elbowGoal-shoulder.position)*shoulder.rotation;
            elbow.rotation=Quaternion.FromToRotation(wrist.position-elbow.position,target-elbow.position)*elbow.rotation;
            wrist.rotation=rotation;
        }
    }
}
