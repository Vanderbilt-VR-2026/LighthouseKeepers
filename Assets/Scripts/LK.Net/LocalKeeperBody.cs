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
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        float diagnosticAt;int diagnosticCount;
#endif
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
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if(diagnosticCount<3&&Time.unscaledTime>=diagnosticAt)
            {
                diagnosticAt=Time.unscaledTime+8;diagnosticCount++;
                var baked=new Mesh();body.Skin.BakeMesh(baked);baked.RecalculateBounds();
                Debug.Log($"KEEPER_BODY poseValid={pose.Valid} origin={origin.name} rootScale={origin.transform.lossyScale} head={head.position:F3} headRot={head.eulerAngles:F1} left={pose.Left:F3} right={pose.Right:F3} body={body.transform.position:F3} bodyScale={body.transform.lossyScale} mesh={baked.bounds} verts={baked.vertexCount} hip={body.Joint("Hips").position:F3} foot={body.Joint("LeftFoot").position:F3} wrist={body.Joint("LeftHand").position:F3} graphics={SystemInfo.graphicsDeviceType}");
                Destroy(baked);
            }
#endif
        }
    }
}
