using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

namespace LighthouseKeepers.Net
{
    /// <summary>Cosmetic first-person skin. Reads the rig and glove wrists; never writes tracked transforms or input.</summary>
    [DefaultExecutionOrder(300)]
    public sealed class LocalKeeperBody : MonoBehaviour
    {
        XROrigin origin;
        CrewHumanoidAvatar body;
        Transform leftController,rightController,leftWrist,rightWrist;
        float nextScan,nextHands;
        readonly KeeperStature stature=new();
        public float StandingHeight=>stature.Height;
        static LocalKeeperBody instance;
        public static LocalKeeperBody Current=>instance;
        public CrewHumanoidAvatar Body=>body;
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
            if(!XRSettings.isDeviceActive){Hide();return;}
            if((!origin||!origin.Camera||!origin.Camera.isActiveAndEnabled)&&Time.unscaledTime>=nextScan)
            {
                nextScan=Time.unscaledTime+1;
                var camera=Camera.main;
                Bind(camera?camera.GetComponentInParent<XROrigin>():null);
            }
            var headset=InputDevices.GetDeviceAtXRNode(XRNode.Head);
            bool tracked=headset.isValid&&headset.TryGetFeatureValue(CommonUsages.isTracked,out var valid)&&valid;
            Present(tracked,Time.unscaledDeltaTime);
        }
        /// <summary>Bind the actual active-camera rig. Also used by graphics tests with tracked-pose fixtures.</summary>
        public void Bind(XROrigin source)
        {
            if(origin==source&&body)return;
            if(body){body.gameObject.SetActive(false);Destroy(body.gameObject);}
            origin=source;body=null;leftController=rightController=leftWrist=rightWrist=null;
            if(!origin||!origin.Camera||!origin.Camera.isActiveAndEnabled)return;
            foreach(var t in origin.GetComponentsInChildren<Transform>(true))
            {if(t.name=="Left Controller")leftController=t;else if(t.name=="Right Controller")rightController=t;}
            var prefab=Resources.Load<GameObject>("PF_HarborKeeperBody");if(!prefab)return;
            // A world-space cosmetic sibling must not inherit locomotion/XR scaling or camera-offset changes.
            body=Instantiate(prefab,transform).GetComponent<CrewHumanoidAvatar>();body.gameObject.SetActive(false);
            if(!body.TryInitialize(out var error)){Debug.LogWarning(error);Destroy(body.gameObject);body=null;return;}
            leftWrist=GloveWrist(leftController);rightWrist=GloveWrist(rightController);
        }
        static Transform GloveWrist(Transform controller)
        {
            if(!controller)return null;
            foreach(var skin in controller.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if(skin.name=="Skinned glove"&&skin.bones.Length==26)return skin.bones[0];
            return null;
        }
        void Hide(){if(body)body.gameObject.SetActive(false);}
        /// <summary>Render only a valid tracked pose, with feet grounded to locomotion rather than guessed stature.</summary>
        public void Present(bool headTracked,float seconds)
        {
            if(!body||!origin||!origin.Camera||!origin.Camera.isActiveAndEnabled||!headTracked){Hide();stature.Interrupt();return;}
            if((!leftWrist||!rightWrist)&&Time.unscaledTime>=nextHands)
            {nextHands=Time.unscaledTime+.5f;if(!leftWrist)leftWrist=GloveWrist(leftController);if(!rightWrist)rightWrist=GloveWrist(rightController);}
            var head=origin.Camera.transform;
            // XROrigin's locomotion root is floor level in both Floor and Device tracking modes.
            float floor=origin.Origin.transform.position.y,height=head.position.y-floor;
            if(!float.IsFinite(height)||height<.25f||height>2.5f){Hide();stature.Interrupt();return;}
            if(!stature.Observe(height,seconds)){Hide();return;}
            var pose=new KeeperPoseFrame{Actor=1,Sequence=1,Head=head.position,HeadRotation=head.rotation,
                Left=leftWrist&&leftWrist.gameObject.activeInHierarchy?leftWrist.position:head.TransformPoint(new Vector3(-.25f,-.45f,.15f)),
                Right=rightWrist&&rightWrist.gameObject.activeInHierarchy?rightWrist.position:head.TransformPoint(new Vector3(.25f,-.45f,.15f)),
                LeftRotation=leftWrist?leftWrist.rotation:head.rotation,
                RightRotation=rightWrist?rightWrist.rotation:head.rotation};
            if(!pose.Valid){Hide();return;}
            body.gameObject.SetActive(true);
            body.ApplyPose(pose,seconds,floor,stature.Height/body.ReferenceEyeHeight);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if(Time.unscaledTime>=diagnosticAt&&diagnosticCount<3)
            {
                diagnosticAt=Time.unscaledTime+8;diagnosticCount++;
                Debug.Log($"KEEPER_BODY_FIXED floor={floor:F3} stature={stature.Height:F3} scale={body.transform.localScale.x:F3} head={pose.Head:F3} hip={body.Joint("Hips").position:F3} leftFoot={body.Joint("LeftFoot").position:F3} leftWristError={Vector3.Distance(body.Joint("LeftHand").position,pose.Left):F3} rightWristError={Vector3.Distance(body.Joint("RightHand").position,pose.Right):F3} gloves={leftWrist!=null}/{rightWrist!=null}");
            }
#endif
        }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        float diagnosticAt;int diagnosticCount;
#endif
        void OnDisable()=>Hide();
    }
    /// <summary>Fit to a stable initial floor-relative eye height; later crouching never shrinks the keeper.</summary>
    public sealed class KeeperStature
    {
        public float Height {get;private set;}
        float candidate,stable;
        public void Interrupt(){candidate=stable=0;}
        public bool Observe(float height,float seconds)
        {
            if(!float.IsFinite(height)||!float.IsFinite(seconds)||height<.8f||height>2.25f||seconds<0){Interrupt();return Height>0;}
            if(Mathf.Abs(height-candidate)>.04f){candidate=height;stable=0;}
            else{stable+=Mathf.Min(seconds,.1f);candidate=Mathf.Lerp(candidate,height,.1f);}
            if(stable>=.75f&&(Height==0||candidate>Height+.06f))Height=candidate;
            return Height>0;
        }
    }
}
