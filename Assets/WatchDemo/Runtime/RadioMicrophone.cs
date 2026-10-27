using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
    // Tapan's activated/deactivated lifecycle, with explicit held/release cancellation.
    [RequireComponent(typeof(XRGrabInteractable))]
    public sealed class RadioMicrophone : MonoBehaviour,IHandPoseTarget
    {
        public HandPose RequiredPose=>HandPose.WrapGrip;
        public bool TriggerHeld {get;private set;}
        public bool Held=>grab&&grab.isSelected;
        public Transform cordSocket;
        public LineRenderer cord;
        XRGrabInteractable grab;Vector3 home;Quaternion rotation;
        readonly Vector3[] cordPoints=new Vector3[81];
        void Awake()
        {
            grab=GetComponent<XRGrabInteractable>();home=transform.position;rotation=transform.rotation;
            grab.activated.AddListener(Activate);grab.deactivated.AddListener(Deactivate);grab.selectExited.AddListener(Release);
            cord.positionCount=cordPoints.Length;
        }
        void Activate(ActivateEventArgs e){if(Held)TriggerHeld=true;}
        void Deactivate(DeactivateEventArgs e)=>TriggerHeld=false;
        void Release(SelectExitEventArgs e){TriggerHeld=false;transform.SetPositionAndRotation(home,rotation);}
        void LateUpdate()
        {
            if(!Held)transform.SetPositionAndRotation(home,rotation);
            var start=cordSocket.position;var end=transform.TransformPoint(new Vector3(0,-.12f,0));var axis=(end-start).normalized;
            var side=Vector3.Cross(axis,Vector3.up);if(side.sqrMagnitude<.01f)side=Vector3.right;side.Normalize();var other=Vector3.Cross(axis,side);
            for(int i=0;i<cordPoints.Length;i++)
            {float t=(float)i/(cordPoints.Length-1),a=t*Mathf.PI*24;cordPoints[i]=Vector3.Lerp(start,end,t)+Vector3.down*(Mathf.Sin(t*Mathf.PI)*.16f)+(side*Mathf.Cos(a)+other*Mathf.Sin(a))*.014f*Mathf.Sin(t*Mathf.PI);}
            cord.SetPositions(cordPoints);
        }
        void OnDisable()=>TriggerHeld=false;
        void OnDestroy(){if(grab){grab.activated.RemoveListener(Activate);grab.deactivated.RemoveListener(Deactivate);grab.selectExited.RemoveListener(Release);}}
    }
}
