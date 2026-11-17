using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

namespace LighthouseKeepers.Net
{
    /// <summary>Cosmetic first-person coat, arms and legs. No head, duplicate hands, collision or player motion.</summary>
    [DefaultExecutionOrder(300)]
    public sealed class LocalKeeperBody : MonoBehaviour
    {
        XROrigin origin;
        CrewHumanoidAvatar body;
        Transform left,right;
        float nextScan;
        static LocalKeeperBody instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()=>instance=null;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if(instance)return;
            instance=new GameObject("Local keeper body presentation").AddComponent<LocalKeeperBody>();
            DontDestroyOnLoad(instance.gameObject);
        }
        void LateUpdate()
        {
            if(!XRSettings.isDeviceActive){if(body)body.gameObject.SetActive(false);return;}
            if(!body&&Time.unscaledTime>=nextScan)
            {
                nextScan=Time.unscaledTime+1;
                origin=FindFirstObjectByType<XROrigin>();if(!origin||!origin.Camera)return;
                var prefab=Resources.Load<GameObject>("PF_HarborKeeperBody");if(!prefab)return;
                foreach(var t in origin.GetComponentsInChildren<Transform>(true))
                {if(t.name=="Left Controller")left=t;else if(t.name=="Right Controller")right=t;}
                body=Instantiate(prefab,origin.transform).GetComponent<CrewHumanoidAvatar>();
                if(!body.TryInitialize(out var error)){Debug.LogWarning(error);Destroy(body.gameObject);body=null;return;}
            }
            if(!body||!origin||!origin.Camera)return;
            body.gameObject.SetActive(true);var head=origin.Camera.transform;
            // Hide the skin's head/hands in the exported mesh, never the working tracked hands.
            var pose=new KeeperPoseFrame{Actor=1,Sequence=1,Head=head.position,HeadRotation=head.rotation,
                Left=left?left.position:head.TransformPoint(new Vector3(-.25f,-.4f,.2f)),
                Right=right?right.position:head.TransformPoint(new Vector3(.25f,-.4f,.2f)),
                LeftRotation=left?left.rotation:head.rotation,RightRotation=right?right.rotation:head.rotation};
            body.ApplyPose(pose,Time.unscaledDeltaTime);
        }
    }
}
