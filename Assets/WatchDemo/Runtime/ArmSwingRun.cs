using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
    // Tracking-space movement removes locomotion/turning from the signal. Never moves the camera itself.
    public sealed class ArmSwingRun : MonoBehaviour
    {
        public sealed class Gait
        {
            float charge, hold; int lastSign; public float Boost {get;private set;}
            public static float ForwardSpeed(Vector3 velocity,Vector3 headForward)
            {return Vector3.Dot(velocity,Vector3.ProjectOnPlane(headForward,Vector3.up).normalized);}
            public float Tick(float left,float right,bool moving,bool free,float dt)
            {
                dt=Mathf.Clamp(dt,0,.1f);
                bool swing=moving&&free&&Mathf.Abs(left)>.25f&&Mathf.Abs(right)>.25f&&left*right<0&&Mathf.Abs(left)<5&&Mathf.Abs(right)<5;
                int sign=left>0?1:-1;
                if(swing){charge=Mathf.Min(1,charge+dt*3);if(lastSign!=0&&sign!=lastSign&&charge>.25f)hold=.65f;lastSign=sign;}
                else charge=Mathf.MoveTowards(charge,0,dt*.35f);
                hold=Mathf.Max(0,hold-dt);if(!moving||!free){hold=0;charge=0;lastSign=0;}
                Boost=Mathf.MoveTowards(Boost,hold>0?1:0,dt/(hold>0?.5f:.3f));return 1+.6f*Boost;
            }
        }
        readonly Gait gait=new(); WatchDemoController watch;XROrigin rig;ContinuousMoveProvider move;HandRig left,right;Vector3 oldL,oldR;bool ready;float nextBind;
        public float SpeedMultiplier {get;private set;}=1;
        public static void Create(WatchDemoController w){if(!w.State.Sprint5||!w.State.Sprint5.armSwingRun)return;var p=w.gameObject.AddComponent<ArmSwingRun>();p.watch=w;p.rig=Object.FindAnyObjectByType<XROrigin>();p.BindHands();}
        void BindHands()
        {
            if(!rig)rig=Object.FindAnyObjectByType<XROrigin>();if(!rig)return;move=rig.GetComponentInChildren<ContinuousMoveProvider>();
            foreach(var h in rig.GetComponentsInChildren<HandRig>(true)){if(h.Node==XRNode.LeftHand)left=h;else if(h.Node==XRNode.RightHand)right=h;}
        }
        void Update()
        {
            if(!left||!right||!move){if(Time.unscaledTime>=nextBind){nextBind=Time.unscaledTime+.5f;BindHands();}return;}
            var l=rig.transform.InverseTransformPoint(left.transform.position);var r=rig.transform.InverseTransformPoint(right.transform.position);
            float dt=Mathf.Max(.001f,Time.deltaTime);var dl=(l-oldL)/dt;var dr=(r-oldR)/dt;oldL=l;oldR=r;
            if(!ready){ready=true;return;}
            var a=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);var b=InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            bool tracked=a.TryGetFeatureValue(CommonUsages.isTracked,out bool lt)&&lt&&b.TryGetFeatureValue(CommonUsages.isTracked,out bool rt)&&rt;
            bool moving=move.leftHandMoveInput.ReadValue().sqrMagnitude>.09f;
            bool free=!watch.Sleeping&&!watch.State.Failed&&tracked&&!(left.Direct&&left.Direct.hasSelection)&&!(right.Direct&&right.Direct.hasSelection);
            var facing=rig.transform.InverseTransformDirection(rig.Camera.transform.forward);
            SpeedMultiplier=gait.Tick(Gait.ForwardSpeed(dl,facing),Gait.ForwardSpeed(dr,facing),moving,free,dt);KeeperMovement.Current?.SetRun(SpeedMultiplier);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if(SpeedMultiplier>1.4f&&!runningLogged&&runSamples<3){runningLogged=true;runSamples++;Debug.Log($"ARM_SWING_RUN multiplier={SpeedMultiplier:F2} emptyHands={free}");}
            if(SpeedMultiplier<1.05f)runningLogged=false;
#endif
        }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        bool runningLogged;int runSamples;
#endif
        void OnDisable(){KeeperMovement.Current?.SetRun(1);ready=false;}
    }
}
